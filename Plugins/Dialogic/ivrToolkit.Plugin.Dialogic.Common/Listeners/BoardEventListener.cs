using System;
using System.Runtime.InteropServices;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using ivrToolkit.Plugin.Dialogic.Common.Extensions;
using Microsoft.Extensions.Logging;
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace ivrToolkit.Plugin.Dialogic.Common.Listeners;

public class BoardEventListener : BaseEventListener
{
    private readonly ILogger<BoardEventListener> _logger;

    public BoardEventListener(ILoggerFactory loggerFactory, int[] handles) : base(loggerFactory, handles)
    {
        _logger = loggerFactory.CreateLogger<BoardEventListener>();
        _logger.LogDebug("Ctr(ILoggerFactory, {0})", handles);
    }

    protected override void HandleEvent(METAEVENT metaEvt)
    {
        switch (metaEvt.evttype)
        {
            case gclib_h.GCEV_SERVICERESP:
                _logger.LogDebug("GCEV_SERVICERESP");
                HandleRegisterStuff(metaEvt);
                break;
            case gclib_h.GCEV_EXTENSION:
                _logger.LogDebug("GCEV_EXTENSION");
                ProcessExtension(metaEvt); // todo some or all of this may not be for board events.
                break;
        }
    }

    /**
        * Process a metaevent extension block.
        */
    private void ProcessExtension(METAEVENT metaEvt)
    {
        // todo this mess needs to be written better :)
        _logger.LogDebug("ProcessExtension(METAEVENT metaEvt)");

        var gcParmBlkp = metaEvt.extevtdatap + 1;
        var parmDatap = IntPtr.Zero;

        parmDatap = gcip_h.gc_util_next_parm(gcParmBlkp, parmDatap);

        while (parmDatap != IntPtr.Zero)
        {
            var parmData = Marshal.PtrToStructure<GC_PARM_DATA>(parmDatap);

            switch (parmData.set_ID)
            {
                case gcip_defs_h.IPSET_SWITCH_CODEC:
                    _logger.LogDebug("IPSET_SWITCH_CODEC:");
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_AUDIO_REQUESTED:
                            _logger.LogDebug("  IPPARM_AUDIO_REQUESTED:");
                            break;
                        case gcip_defs_h.IPPARM_READY:
                            _logger.LogDebug("  IPPARM_READY:");
                            break;
                        default:
                            _logger.LogError("  Got unknown extension parmID {0}", parmData.parm_ID);
                            break;
                    }

                    break;
                case gcip_defs_h.IPSET_MEDIA_STATE:
                    _logger.LogDebug("IPSET_MEDIA_STATE:");
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_TX_CONNECTED:
                            _logger.LogDebug("  IPPARM_TX_CONNECTED");
                            break;
                        case gcip_defs_h.IPPARM_TX_DISCONNECTED:
                            _logger.LogDebug("  IPPARM_TX_DISCONNECTED");
                            break;
                        case gcip_defs_h.IPPARM_RX_CONNECTED:
                            _logger.LogDebug("  IPPARM_RX_CONNECTED");
                            break;
                        case gcip_defs_h.IPPARM_RX_DISCONNECTED:
                            _logger.LogDebug("  IPPARM_RX_DISCONNECTED");
                            break;
                        default:
                            _logger.LogError("  Got unknown extension parmID {0}", parmData.parm_ID);
                            break;
                    }

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
                    _logger.LogDebug("IPSET_IPPROTOCOL_STATE:");
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_SIGNALING_CONNECTED:
                            _logger.LogDebug("  IPPARM_SIGNALING_CONNECTED");
                            break;
                        case gcip_defs_h.IPPARM_SIGNALING_DISCONNECTED:
                            _logger.LogDebug("  IPPARM_SIGNALING_DISCONNECTED");
                            break;
                        case gcip_defs_h.IPPARM_CONTROL_CONNECTED:
                            _logger.LogDebug("  IPPARM_CONTROL_CONNECTED");
                            break;
                        case gcip_defs_h.IPPARM_CONTROL_DISCONNECTED:
                            _logger.LogDebug("  IPPARM_CONTROL_DISCONNECTED");
                            break;
                        default:
                            _logger.LogError("  Got unknown extension parmID {0}", parmData.parm_ID);
                            break;
                    }

                    break;
                case gcip_defs_h.IPSET_RTP_ADDRESS:
                    _logger.LogDebug("IPSET_RTP_ADDRESS:");
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_LOCAL:
                            _logger.LogDebug("IPPARM_LOCAL: size = {0}", parmData.value_size);
                            var ptr = parmDatap + 5;
                            var ipAddr = Marshal.PtrToStructure<RTP_ADDR>(ptr);
                            _logger.LogDebug("  IPPARM_LOCAL: address:{0}, port {1}", ipAddr.u_ipaddr.ipv4.ToIp(),
                                ipAddr.port);
                            break;
                        case gcip_defs_h.IPPARM_REMOTE:
                            _logger.LogDebug("IPPARM_REMOTE: size = {0}", parmData.value_size);
                            var ptr2 = parmDatap + 5;
                            var ipAddr2 = Marshal.PtrToStructure<RTP_ADDR>(ptr2);
                            _logger.LogDebug("  IPPARM_REMOTE: address:{0}, port {1}", ipAddr2.u_ipaddr.ipv4.ToIp(),
                                ipAddr2.port);
                            break;
                        default:
                            _logger.LogError("  Got unknown extension parmID {0}", parmData.parm_ID);
                            break;
                    }

                    break;
                /* Set ID for SIP message types handed by GCEV_EXTENSION
                 * IPSET_MSG_SIP | This Set ID is used to set or get the SIP message type.
                 */
                case gcip_defs_h.IPSET_MSG_SIP:
                    _logger.LogInformation("IPSET_MSG_SIP: {0}", parmData.parm_ID);
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_MSGTYPE:
                            var messType = parmData.value_buf;
                            _logger.LogDebug("  value_size = {0}, value_buf = {1}", parmData.value_size, parmData.value_buf);

                            // TODO I don;t think this is done properly at all.
                            switch (messType)
                            {
                                case gcip_defs_h.IP_MSGTYPE_SIP_INFO_OK:
                                    _logger.LogDebug("  IP_MSGTYPE_SIP_INFO_OK");
                                    break;
                                case gcip_defs_h.IP_MSGTYPE_SIP_INFO_FAILED:
                                    _logger.LogDebug("  IP_MSGTYPE_SIP_INFO_FAILED");
                                    break;
                            }
                            break;
                        case gcip_defs_h.IPPARM_MSG_SIP_RESPONSE_CODE:
                            _logger.LogDebug(" value_size = {0}, value_buf = {1}", parmData.value_size, parmData.value_buf);
                            break;
                    }
                    _logger.LogInformation("  IPSET_MSG_SIP:: size = {0}", parmData.value_size);
                    break;
                case gcip_defs_h.IPSET_SIP_MSGINFO:
                    _logger.LogInformation("IPSET_SIP_MSGINFO:");
                    var str = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                    _logger.LogDebug("  {0}: {1}", parmData.parm_ID.SipMsgInfo(), str);
                    break;
                default:
                    _logger.LogError("Got unknown set_ID({0}).", parmData.set_ID);
                    break;
            }

            parmDatap = gcip_h.gc_util_next_parm(gcParmBlkp, parmDatap);
        }
    }

    private void HandleRegisterStuff(METAEVENT metaEvt)
    {
        _logger.LogDebug("HandleRegisterStuff(METAEVENT metaEvt)");
        var gcParmBlkp = metaEvt.extevtdatap;
        var parmDatap = IntPtr.Zero;

        parmDatap = gcip_h.gc_util_next_parm(gcParmBlkp, parmDatap);

        while (parmDatap != IntPtr.Zero)
        {
            var parmData = Marshal.PtrToStructure<GC_PARM_DATA>(parmDatap);

            switch (parmData.set_ID)
            {
                case gcip_defs_h.IPSET_REG_INFO:
                    switch (parmData.parm_ID)
                    {
                        case gcip_defs_h.IPPARM_REG_STATUS:
                        {
                            var value = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                            switch (value)
                            {
                                case gcip_defs_h.IP_REG_CONFIRMED:
                                    _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_CONFIRMED");
                                    break;
                                case gcip_defs_h.IP_REG_REJECTED:
                                    _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_STATUS: IP_REG_REJECTED");
                                    break;
                            }

                            break;
                        }
                        case gcip_defs_h.IPPARM_REG_SERVICEID:
                        {
                            var value = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                            _logger.LogDebug("    IPSET_REG_INFO/IPPARM_REG_SERVICEID: 0x{0:X}", value);
                            break;
                        }
                        default:
                            _logger.LogDebug(
                                "    Missed one: set_ID = IPSET_REG_INFO, parm_ID = {1:X}, bytes = {2}",
                                parmData.parm_ID, parmData.value_size);
                            break;
                    }

                    break;
                case gcip_defs_h.IPSET_PROTOCOL:
                    var value2 = GetValueFromPtr(parmDatap + 5, parmData.value_size);
                    _logger.LogDebug("    IPSET_PROTOCOL value: {0}", value2);
                    break;
                case gcip_defs_h.IPSET_LOCAL_ALIAS:
                {
                    var localAlias = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                    _logger.LogDebug("    IPSET_LOCAL_ALIAS value: {0}", localAlias);
                    break;
                }
                case gcip_defs_h.IPSET_SIP_MSGINFO:
                {
                    var msgInfo = GetStringFromPtr(parmDatap + 5, parmData.value_size);
                    _logger.LogDebug("    IPSET_SIP_MSGINFO value: {0}", msgInfo);
                    break;
                }
                default:
                    _logger.LogDebug("    Missed one: set_ID = {0:X}, parm_ID = {1:X}, bytes = {2}",
                        parmData.set_ID, parmData.parm_ID, parmData.value_size);
                    break;
            }

            parmDatap = gcip_h.gc_util_next_parm(gcParmBlkp, parmDatap);
        }

        _logger.LogDebug("HandleRegisterStuff(METAEVENT metaEvt) - done!");
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