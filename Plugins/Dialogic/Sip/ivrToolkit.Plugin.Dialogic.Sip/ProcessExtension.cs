using System.Runtime.InteropServices;
using System;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using Microsoft.Extensions.Logging;
using ivrToolkit.Plugin.Dialogic.Common.Extensions;
using ivrToolkit.Core.Exceptions;
using static System.Net.Mime.MediaTypeNames;

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

        while (parmDatap != IntPtr.Zero)
        {
            var parmData = Marshal.PtrToStructure<GC_PARM_DATA>(parmDatap);
            _logger.LogDebug("    {description}", parmData.parm_ID.IpSetMimeDescription());
            // do something here
            parmDatap = gclib_h.gc_util_next_parm(gcParmBlkp, parmDatap);
        }
        gclib_h.gc_util_delete_parm_blk(gcParmBlkp);
    }

    /**
    * Process a metaevent extension block.
    */
    public void HandleExtension(METAEVENT metaEvt)
    {
        // todo this mess needs to be written better :)
        _logger.LogDebug("ProcessExtension(METAEVENT metaEvt)");

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
                    _logger.LogDebug("{  description}", parmData.parm_ID.IpSetSwitchCodeDescription());
                    switch (parmData.parm_ID)
                    {
                        // todo I don't think this ever happens
                        case gcip_defs_h.IPPARM_AUDIO_REQUESTED:
                            //ResponseCodecRequest(true);
                            break;
                    }

                    break;
                case gcip_defs_h.IPSET_MEDIA_STATE:
                    _logger.LogDebug("  {description}", parmData.parm_ID.IpSetMediaStateDescription());

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
                    _logger.LogDebug("  {description}", parmData.parm_ID.IpSetIpProtoolStateDescription());
                    break;
                case gcip_defs_h.IPSET_RTP_ADDRESS:
                    _logger.LogDebug("  {description}", parmData.parm_ID.IpSetRptAddressDescription());
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_LOCAL:
                            var ptr = parmDatap + 5;
                            var ipAddr = Marshal.PtrToStructure<RTP_ADDR>(ptr);
                            _logger.LogDebug("  IPPARM_LOCAL: address:{0}, port {1}", ipAddr.u_ipaddr.ipv4.ToIp(),
                                ipAddr.port);
                            break;
                        case gcip_defs_h.IPPARM_REMOTE:
                            var ptr2 = parmDatap + 5;
                            var ipAddr2 = Marshal.PtrToStructure<RTP_ADDR>(ptr2);
                            _logger.LogDebug("  IPPARM_REMOTE: address:{0}, port {1}", ipAddr2.u_ipaddr.ipv4.ToIp(),
                                ipAddr2.port);
                            break;
                    }

                    break;
                case gcip_defs_h.IPSET_MSG_SIP:
                    _logger.LogDebug("  {description}", parmData.parm_ID.IpSetMsgSipDescription());
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_MSGTYPE:
                            var messType = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                            _logger.LogDebug("  {description}", messType.IpMsgTypeDescription());
                            break;
                    }
                    break;
                case gcip_defs_h.IPSET_SIP_MSGINFO:
                    var str = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                    _logger.LogDebug("  {0}: {1}", parmData.parm_ID.SipMsgInfo(), str);
                    break;
                case gcip_defs_h.IPSET_MIME:
                    _logger.LogDebug("  {description}", parmData.parm_ID.IpSetMimeDescription());
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
    }

    private string GetStringFromPtr(IntPtr ptr, int size)
    {
        return Marshal.PtrToStringAnsi(ptr, size);
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
