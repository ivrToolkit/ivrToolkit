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

    /**
    * Process a metaevent extension block.
    */
    public bool HandleExtension(METAEVENT metaEvt)
    {
        var hangupStarting = false;
        _logger.LogDebug("ProcessExtension(METAEVENT metaEvt) - evtDev = {eventDevice}", metaEvt.evtdev);

        var receivedNotify = false;
        var callIdHeader = "";
        var eventDev = metaEvt.evtdev;

        var extensionBlockPtr = metaEvt.extevtdatap;
        var extensionBlock = Marshal.PtrToStructure<EXTENSIONEVTBLK>(extensionBlockPtr);
        _logger.LogDebug("Extension ID = {extensionId}: {description}", extensionBlock.ext_id, extensionBlock.ext_id.IpExtIdDescription());

        var extentionParametersBlockPtr = metaEvt.extevtdatap + 1;

        var parmData = gcip_h.CreateAndInitGcParmDataExt();

        var result = gclib_h.gc_util_next_parm_ex(extentionParametersBlockPtr, ref parmData);

        while (result == GcErr_h.GC_SUCCESS)
        {
            _logger.LogDebug("{description}:", parmData.set_ID.GetIdDescription());

            switch (parmData.set_ID)
            {
                case gcip_defs_h.IPSET_SWITCH_CODEC:
                    _logger.LogDebug("{  description} - (SIP)", parmData.parm_ID.IpSetSwitchCodeDescription());
                    break;
                case gcip_defs_h.IPSET_MEDIA_STATE:
                    _logger.LogDebug("  {description} - (SIP)", parmData.parm_ID.IpSetMediaStateDescription());

                    // todo there has got to be a better way than this
                    if (parmData.data_size == Marshal.SizeOf<IP_CAPABILITY>())
                    {
                        var ptr = parmData.pData;
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
                        hangupStarting = true;
                    }
                    break;
                case gcip_defs_h.IPSET_RTP_ADDRESS:
                    _logger.LogDebug("  {description}", parmData.parm_ID.IpSetRptAddressDescription());
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_LOCAL:
                            var ptr = parmData.pData;
                            var ipAddr = Marshal.PtrToStructure<RTP_ADDR>(ptr);
                            _logger.LogDebug("  IPPARM_LOCAL: address:{0}, port {1} - (SIP)", ipAddr.u_ipaddr.ipv4.ToIp(),
                                ipAddr.port);
                            break;
                        case gcip_defs_h.IPPARM_REMOTE:
                            var ptr2 = parmData.pData;
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
                            var messType = GetValueFromPtr(parmData.pData, parmData.data_size);
                            _logger.LogDebug("    {description} - (SIP)", messType.IpMsgTypeDescription());
                            if (messType == gcip_defs_h.IP_MSGTYPE_SIP_NOTIFY)
                            {
                                receivedNotify = true;
                            }
                            break;
                    }
                    break;
                case gcip_defs_h.IPSET_SIP_MSGINFO:
                    var str = GetStringFromPtr(parmData.pData, (int)parmData.data_size);
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
                            var pointerValue = GetValueFromPtr(parmData.pData, parmData.data_size);
                            HandleMime(pointerValue);
                            break;
                    }
                    break;
            }
 
            result = gclib_h.gc_util_next_parm_ex(extentionParametersBlockPtr, ref parmData);
        }
        if (result != GcErr_h.EGC_NO_MORE_PARMS)
        {
            result.LogIfGlobalCallError(_logger);
        }

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
        return hangupStarting;
    }

    private void HandleMime(int pointerValue)
    {
        var bodySize = 0;

        var gcParmBlkp = new IntPtr(pointerValue);
        var parmData = gcip_h.CreateAndInitGcParmDataExt();

        var result = gclib_h.gc_util_next_parm_ex(gcParmBlkp, ref parmData);

        while (result == GcErr_h.GC_SUCCESS)
        {
            _logger.LogDebug("    {description}", parmData.parm_ID.IpSetMimeDescription());

            switch (parmData.parm_ID)
            {
                case gcip_defs_h.IPPARM_MIME_PART_TYPE:
                    var contentType = GetStringFromPtr(parmData.pData, (int)parmData.data_size);
                    _logger.LogDebug("      {contentType}", contentType);
                    break;
                case gcip_defs_h.IPPARM_MIME_PART_HEADER:
                    var header = GetStringFromPtr(parmData.pData, (int)parmData.data_size);
                    _logger.LogDebug("      {header}", header);
                    break;
                case gcip_defs_h.IPPARM_MIME_PART_BODY:
                    var bodyBuff = GetValueFromPtr(parmData.pData, parmData.data_size);
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
                    bodySize = GetValueFromPtr(parmData.pData, parmData.data_size);
                    _logger.LogDebug("      Body size = {size}", bodySize);
                    break;
            }
            // do something here
            result = gclib_h.gc_util_next_parm_ex(gcParmBlkp, ref parmData);
        }
        if (result != GcErr_h.EGC_NO_MORE_PARMS)
        {
            result.LogIfGlobalCallError(_logger);
        }
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
            _logger.LogDebug("Deleting gc_Extension parameter block: 0x{paramBlock:X}", gcParmBlkPtr);
            gclib_h.gc_util_delete_parm_blk(gcParmBlkPtr);
            Marshal.FreeHGlobal(pCallIdHeader);
        }


    }

    private string GetStringFromPtr(IntPtr ptr, int size)
    {
        return Marshal.PtrToStringAnsi(ptr, size).TrimEnd('\0');
    }

    private int GetValueFromPtr(IntPtr ptr, uint size)
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
