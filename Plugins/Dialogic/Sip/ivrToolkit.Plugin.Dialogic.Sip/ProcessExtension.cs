using System.Runtime.InteropServices;
using System;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using Microsoft.Extensions.Logging;
using ivrToolkit.Plugin.Dialogic.Common.Extensions;
using ivrToolkit.Core.Exceptions;

namespace ivrToolkit.Plugin.Dialogic.Sip;

public class ProcessExtension
{
    private readonly ILogger<ProcessExtension> _logger;

    public ProcessExtension(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<ProcessExtension>();
    }

    private void HandleMime(ref IntPtr gcParmBlkp)
    {
        var parmDatap = IntPtr.Zero;
        parmDatap = gclib_h.gc_util_next_parm(gcParmBlkp, parmDatap);

        var bodySize = 0;

        while (parmDatap != IntPtr.Zero)
        {
            var parmData = Marshal.PtrToStructure<GC_PARM_DATA>(parmDatap);
            _logger.LogDebug("    {description}", parmData.parm_ID.IpSetMimeDescription());

            switch (parmData.parm_ID)
            {
                case gcip_defs_h.IPPARM_MIME_PART_TYPE:
                    var contentType = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                    _logger.LogDebug("      {contentType}", contentType);
                    break;
                case gcip_defs_h.IPPARM_MIME_PART_HEADER:
                    var header = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                    _logger.LogDebug("      {header}", header);
                    break;
                case gcip_defs_h.IPPARM_MIME_PART_BODY:
                    var bodyBuff = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                    var bodyBuffp = new IntPtr(bodyBuff);

                    if (bodySize == 0) break;

                    // Allocate memory for the buffer
                    byte[] appBuff = new byte[bodySize + 1]; // +1 for null termination

                    // Copy data from unmanaged memory to managed byte array
                    Marshal.Copy(bodyBuffp, appBuff, 0, bodySize);

                    // Null-terminate the buffer
                    appBuff[bodySize] = 0;

                    // Convert to a string and print it (assuming content is text)
                    string bodyContent = System.Text.Encoding.Default.GetString(appBuff).TrimEnd('\0');
                    _logger.LogDebug($"      {bodyContent}");

                    break;
                case gcip_defs_h.IPPARM_MIME_PART_BODY_SIZE:
                    bodySize = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                    _logger.LogDebug("      Body size = {size}", bodySize);
                    break;
            }
            // do something here
            parmDatap = gclib_h.gc_util_next_parm(gcParmBlkp, parmDatap);
        }
        gclib_h.gc_util_delete_parm_blk(gcParmBlkp);
    }

    /**
    * Process a metaevent extension block.
    */
    public bool HandleExtension(METAEVENT metaEvt)
    {
        var doStop = false;
        _logger.LogDebug("ProcessExtension(METAEVENT metaEvt) - evtDev = {eventDevice}", metaEvt.evtdev);

        var receivedNotify = false;
        var callIdHeader = "";
        var eventDev = metaEvt.evtdev;

        var extensionBlockPtr = metaEvt.extevtdatap;
        if (extensionBlockPtr != IntPtr.Zero)
        {
            var extensionBlock = Marshal.PtrToStructure<EXTENSIONEVTBLK>(extensionBlockPtr);
            _logger.LogDebug("Extension ID = {extensionId}: {description}", extensionBlock.ext_id, extensionBlock.ext_id.IpExtIdDescription());
        } else
        {
            // I don't think this ever happens
            _logger.LogDebug("There is no extension ID");
        }

        var gcParmBlkp = metaEvt.extevtdatap + 1;
        var parmDatap = IntPtr.Zero;

        parmDatap = gclib_h.gc_util_next_parm(gcParmBlkp, parmDatap);

        while (parmDatap != IntPtr.Zero)
        {
            var parmData = Marshal.PtrToStructure<GC_PARM_DATA>(parmDatap);

            _logger.LogDebug("{description}:", parmData.set_ID.SetIdDescription());

            switch (parmData.set_ID)
            {
                case gcip_defs_h.IPSET_SWITCH_CODEC:
                    _logger.LogDebug("{  description} - (SIP)", parmData.parm_ID.IpSetSwitchCodeDescription());
                    break;
                case gcip_defs_h.IPSET_MEDIA_STATE:
                    _logger.LogDebug("  {description} - (SIP)", parmData.parm_ID.IpSetMediaStateDescription());

                    // todo there has got to be a better way than this
                    if (parmData.value_size == Marshal.SizeOf<IP_CAPABILITY>())
                    {
                        var ptr = parmDatap + 5;
                        var ipCapp = Marshal.PtrToStructure<IP_CAPABILITY>(ptr);

                        _logger.LogDebug(
                            "    stream codec infomation for TX: capability({0}), dir({1}), frames_per_pkt({2}), VAD({3})",
                            ipCapp.capability, ipCapp.direction, ipCapp.extra.audio.frames_per_pkt,
                            ipCapp.extra.audio.VAD);
                    }

                    break;
                case gcip_defs_h.IPSET_IPPROTOCOL_STATE:
                    _logger.LogDebug("  {description} - (SIP)", parmData.parm_ID.IpSetIpProtoolStateDescription());
                    if (parmData.parm_ID == gcip_defs_h.IPPARM_SIGNALING_DISCONNECTED)
                    {
                        doStop = true;
                    }
                    break;
                case gcip_defs_h.IPSET_RTP_ADDRESS:
                    _logger.LogDebug("  {description}", parmData.parm_ID.IpSetRptAddressDescription());
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_LOCAL:
                            var ptr = parmDatap + 5;
                            var ipAddr = Marshal.PtrToStructure<RTP_ADDR>(ptr);
                            _logger.LogDebug("  IPPARM_LOCAL: address:{0}, port {1} - (SIP)", ipAddr.u_ipaddr.ipv4.ToIp(),
                                ipAddr.port);
                            break;
                        case gcip_defs_h.IPPARM_REMOTE:
                            var ptr2 = parmDatap + 5;
                            var ipAddr2 = Marshal.PtrToStructure<RTP_ADDR>(ptr2);
                            _logger.LogDebug("  IPPARM_REMOTE: address:{0}, port {1} - (SIP)", ipAddr2.u_ipaddr.ipv4.ToIp(),
                                ipAddr2.port);
                            break;
                    }

                    break;
                case gcip_defs_h.IPSET_MSG_SIP:
                    _logger.LogDebug("  {description} - (SIP)", parmData.parm_ID.IpSetMsgSipDescription());
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_MSGTYPE:
                            var messType = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                            _logger.LogDebug("  {description} - (SIP)", messType.IpMsgTypeDescription());
                            if (messType == gcip_defs_h.IP_MSGTYPE_SIP_NOTIFY)
                            {
                                receivedNotify = true;
                            }
                            break;
                    }
                    break;
                case gcip_defs_h.IPSET_SIP_MSGINFO:
                    var str = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                    _logger.LogDebug("  {0}: {1} - (SIP)", parmData.parm_ID.SipMsgInfo(), str);
                    if (parmData.parm_ID == gcip_defs_h.IPPARM_CALLID_HDR)
                    {
                        callIdHeader = str;
                    }

                    break;
                case gcip_defs_h.IPSET_MIME:
                    _logger.LogDebug("  {description} - (SIP)", parmData.parm_ID.IpSetMimeDescription());
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_MIME_PART:
                            var parmblkp = parmDatap + 5;
                            var pointerValue = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                            parmblkp = new IntPtr(pointerValue);
                            HandleMime(ref parmblkp);
                            break;
                    }
                    break;
            }

            parmDatap = gclib_h.gc_util_next_parm(gcParmBlkp, parmDatap);
        }
        gclib_h.gc_util_delete_parm_blk(gcParmBlkp);

        if (receivedNotify)
        {
            try
            {
                RespondToNotify(true, callIdHeader, eventDev); // send accept
            }
            catch (Exception e)
            {
                // I don't want to crash the program just because of this and it may never happen
                _logger.LogError(e, "Failed to respond to notify");
            }
        }
        return doStop;
    }

    private void RespondToNotify(bool accept, string callIdHeader, int eventDev)
    {
        _logger.LogDebug("{name}({acceptReject}, {callIdHeader})", nameof(RespondToNotify), accept ? "accept" : "reject", callIdHeader);
        if (string.IsNullOrEmpty(callIdHeader))
        {
            _logger.LogDebug("Missing IPPARM_EVENT_HDR");
            return;
        }

        var gcParmBlkPtr = IntPtr.Zero;
        var result = gclib_h.gc_util_insert_parm_val(ref gcParmBlkPtr, gcip_defs_h.IPSET_MSG_SIP,
            gcip_defs_h.IPPARM_MSGTYPE,
            sizeof(int),
            (uint)(accept ? gcip_defs_h.IPPARM_ACCEPT : gcip_defs_h.IPPARM_REJECT));
        result.ThrowIfGlobalCallError();

        // format the caller id correctly
        callIdHeader = $"Call-ID: {callIdHeader}\0";

        // Insert SIP Call ID field
        var pCallIdHeader = Marshal.StringToHGlobalAnsi(callIdHeader);
        try
        {
            // string has a null on the end so don't use Length + 1
            result = gclib_h.gc_util_insert_parm_ref_ex(ref gcParmBlkPtr, gcip_defs_h.IPSET_SIP_MSGINFO,
                gcip_defs_h.IPPARM_SIP_HDR, (uint)callIdHeader.Length, pCallIdHeader);
            result.ThrowIfGlobalCallError();

            var returnParamPtr = IntPtr.Zero;

            result = gclib_h.gc_Extension(gclib_h.GCTGT_GCLIB_CHAN, eventDev, gcip_defs_h.IPEXTID_SENDMSG,
                gcParmBlkPtr, ref returnParamPtr, DXXXLIB_H.EV_ASYNC);
            result.ThrowIfGlobalCallError();

        }
        finally
        {
            gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);
            Marshal.FreeHGlobal(pCallIdHeader);
        }


    }

    private string GetStringFromPtr(IntPtr ptr, int size)
    {
        return Marshal.PtrToStringAnsi(ptr, size).TrimEnd('\0');
    }

    private int GetValueFromPtr(IntPtr ptr, byte size)
    {
        int value;
        switch (size)
        {
            case 1:
                value = Marshal.ReadByte(ptr);
                break;
            case 2:
                value = Marshal.ReadInt16(ptr);
                break;
            case 4:
                value = Marshal.ReadInt32(ptr);
                break;
            default:
                throw new VoiceException($"Unable to get value from ptr. Size is {size}");
        }

        return value;
    }

}
