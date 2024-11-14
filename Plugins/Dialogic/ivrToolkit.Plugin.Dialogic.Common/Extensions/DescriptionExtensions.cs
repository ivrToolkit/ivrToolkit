using System;
using System.Net;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
// ReSharper disable StringLiteralTypo

namespace ivrToolkit.Plugin.Dialogic.Common.Extensions;

public static class DescriptionExtensions
{
    public static string EventTypeDescription(this int type)
    {
        switch (type)
        {
            case DXXXLIB_H.TDX_PLAY:
                return "Play Completed";
            case DXXXLIB_H.TDX_RECORD:
                return "Record Complete";
            case DXXXLIB_H.TDX_GETDIG:
                return "Get Digits Completed";
            case DXXXLIB_H.TDX_DIAL:
                return "Dial Completed";
            case DXXXLIB_H.TDX_CALLP:
                return "Call Progress Completed";
            case DXXXLIB_H.TDX_CST:
                return "CST Event Received";
            case DXXXLIB_H.TDX_SETHOOK:
                return "SetHook Completed";
            case DXXXLIB_H.TDX_WINK:
                return "Wink Completed";
            case DXXXLIB_H.TDX_ERROR:
                return "Error Event";
            case DXXXLIB_H.TDX_PLAYTONE:
                return "Play Tone Completed";
            case DXXXLIB_H.TDX_GETR2MF:
                return "Get R2MF completed";
            case DXXXLIB_H.TDX_BARGEIN:
                return "Barge in completed";
            case DXXXLIB_H.TDX_NOSTOP:
                return "No Stop needed to be Issued";
            case DXXXLIB_H.TDX_UNKNOWN:
                return "TDX_UNKNOWN";
        }

        return gcmsg_h.GCEV_MSG(type);
    }

    public static string ChannelStateDescription(this int channelState)
    {

        switch (channelState)
        {
            case DXXXLIB_H.CS_IDLE:
                return "Channel is idle";
            case DXXXLIB_H.CS_PLAY:
                return "Channel is playing back";
            case DXXXLIB_H.CS_RECD:
                return "Channel is recording";
            case DXXXLIB_H.CS_DIAL:
                return "Channel is dialing";
            case DXXXLIB_H.CS_GTDIG:
                return "Channel is getting digits";
            case DXXXLIB_H.CS_TONE:
                return "Channel is generating a tone";
            case DXXXLIB_H.CS_STOPD:
                return "Operation has terminated";
            case DXXXLIB_H.CS_SENDFAX:
                return "Channel is sending a fax";
            case DXXXLIB_H.CS_RECVFAX:
                return "Channel is receiving a fax";
            case DXXXLIB_H.CS_FAXIO:
                return "Channel is between fax pages";
            case DXXXLIB_H.CS_HOOK:
                return "A change in hookstate is in progress";
            case DXXXLIB_H.CS_WINK:
                return "A wink operation is in progress";
            case DXXXLIB_H.CS_CALL:
                return "Channel is Call Progress Mode";
            case DXXXLIB_H.CS_GETR2MF:
                return "Channel is Getting R2MF";
            case DXXXLIB_H.CS_RINGS:
                return "Call status Rings state";
            case DXXXLIB_H.CS_BLOCKED:
                return "Channel is blocked";
            case DXXXLIB_H.CS_RECDPREPARE:
                return "Channel is preparing record and driver has not yet sent record";
        }

        return $"Unknown channel: {channelState}";
    }

    public static string CallStateDescription(this int callState)
    {
        switch (callState)
        {
            case gclib_h.GCST_NULL:
                return "GCST_NULL";
            case gclib_h.GCST_ACCEPTED:
                return "GCST_ACCEPTED";
            case gclib_h.GCST_ALERTING:
                return "GCST_ALERTING";
            case gclib_h.GCST_CONNECTED:
                return "GCST_CONNECTED";
            case gclib_h.GCST_OFFERED:
                return "GCST_OFFERED";
            case gclib_h.GCST_DIALING:
                return "GCST_DIALING";
            case gclib_h.GCST_IDLE:
                return "GCST_IDLE";
            case gclib_h.GCST_DISCONNECTED:
                return "GCST_DISCONNECTED";
            case gclib_h.GCST_DIALTONE:
                return "GCST_DIALTONE";
            case gclib_h.GCST_ONHOLDPENDINGTRANSFER:
                return "GCST_ONHOLDPENDINGTRANSFER";
            case gclib_h.GCST_ONHOLD:
                return "GCST_ONHOLD";
            case gclib_h.GCST_DETECTED:
                return "GCST_DETECTED";
            case gclib_h.GCST_PROCEEDING:
                return "GCST_PROCEEDING";
            case gclib_h.GCST_SENDMOREINFO:
                return "GCST_SENDMOREINFO";
            case gclib_h.GCST_GETMOREINFO:
                return "GCST_GETMOREINFO";
            case gclib_h.GCST_CALLROUTING:
                return "GCST_CALLROUTING";
        }

        return callState.ToString();
    }

    public static string CallProgressDescription(this int callProgress)
    {
        switch (callProgress)
        {
            case DXCALLP_H.CR_BUSY:
                return "Line busy";
            case DXCALLP_H.CR_NOANS:
                return "No answer";
            case DXCALLP_H.CR_NORB:
                return "No ringback";
            case DXCALLP_H.CR_CNCT:
                return "Call connected";
            case DXCALLP_H.CR_CEPT:
                return "Operator intercept";
            case DXCALLP_H.CR_STOPD:
                return "Call analysis stopped";
            case DXCALLP_H.CR_NODIALTONE:
                return "No dialtone detected";
            case DXCALLP_H.CR_FAXTONE:
                return "Fax tone detected";
            case DXCALLP_H.CR_ERROR:
                return "Call analysis error";
        }

        return callProgress.ToString();
    }


    public static string CstDescription(this ushort type)
    {
        switch (type)
        {
            case DXXXLIB_H.DE_DIGITS:
                return "Digit Received";
            case DXXXLIB_H.DE_DIGOFF:
                return "Digit tone off event";
            case DXXXLIB_H.DE_LCOF:
                return "Loop current off";
            case DXXXLIB_H.DE_LCON:
                return "Loop current on";
            case DXXXLIB_H.DE_LCREV:
                return "Loop current reversal";
            case DXXXLIB_H.DE_RINGS:
                return "Rings received";
            case DXXXLIB_H.DE_RNGOFF:
                return "Ring off event";
            case DXXXLIB_H.DE_SILOF:
                return "Silenec off";
            case DXXXLIB_H.DE_SILON:
                return "Silence on";
            case DXXXLIB_H.DE_STOPRINGS:
                return "Stop ring detect state";
            case DXXXLIB_H.DE_TONEOFF:
                return "Tone OFF Event Received";
            case DXXXLIB_H.DE_TONEON:
                return "Tone ON Event Received";
            case DXXXLIB_H.DE_UNDERRUN:
                return "R4 Streaming to Board API FW underrun event. Improves streaming data to board";
            case DXXXLIB_H.DE_VAD:
                return "Voice Energy detected";
            case DXXXLIB_H.DE_WINK:
                return "Wink received";
        }

        return "unknown";
    }
    public static string SipMsgInfo(this ushort parmId)
    {
        switch (parmId)
        {
            case gcip_defs_h.IPPARM_REQUEST_URI:
                return "IPPARM_REQUEST_URI";
            case gcip_defs_h.IPPARM_CONTACT_URI:
                return "IPPARM_CONTACT_URI";
            case gcip_defs_h.IPPARM_FROM_DISPLAY:
                return "IPPARM_FROM_DISPLAY";
            case gcip_defs_h.IPPARM_TO_DISPLAY:
                return "IPPARM_TO_DISPLAY";
            case gcip_defs_h.IPPARM_CONTACT_DISPLAY:
                return "IPPARM_CONTACT_DISPLAY";
            case gcip_defs_h.IPPARM_REFERRED_BY:
                return "IPPARM_REFERRED_BY";
            case gcip_defs_h.IPPARM_REPLACES:
                return "IPPARM_REPLACES";
            case gcip_defs_h.IPPARM_CONTENT_DISPOSITION:
                return "IPPARM_CONTENT_DISPOSITION";
            case gcip_defs_h.IPPARM_CONTENT_ENCODING:
                return "IPPARM_CONTENT_ENCODING";
            case gcip_defs_h.IPPARM_CONTENT_LENGTH:
                return "IPPARM_CONTENT_LENGTH";
            case gcip_defs_h.IPPARM_CONTENT_TYPE:
                return "IPPARM_CONTENT_TYPE";
            case gcip_defs_h.IPPARM_REFER_TO:
                return "IPPARM_REFER_TO";
            case gcip_defs_h.IPPARM_DIVERSION_URI:
                return "IPPARM_DIVERSION_URI";
            case gcip_defs_h.IPPARM_EVENT_HDR:
                return "IPPARM_EVENT_HDR";
            case gcip_defs_h.IPPARM_EXPIRES_HDR:
                return "IPPARM_EXPIRES_HDR";
            case gcip_defs_h.IPPARM_CALLID_HDR:
                return "IPPARM_CALLID_HDR";
            case gcip_defs_h.IPPARM_SIP_HDR:
                return "IPPARM_SIP_HDR";
            case gcip_defs_h.IPPARM_FROM:
                return "IPPARM_FROM";
            case gcip_defs_h.IPPARM_TO:
                return "IPPARM_TO";
            case gcip_defs_h.IPPARM_SIP_HDR_REMOVE:
                return "IPPARM_SIP_HDR_REMOVE";
            case gcip_defs_h.IPPARM_SIP_VIA_HDR_REPLACE:
                return "IPPARM_SIP_VIA_HDR_REPLACE";
        }
        return $"unknown {parmId}";
    }

    public static string ToIp(this uint ip)
    {
        try
        {
            //var netorderIp = IPAddress.HostToNetworkOrder(shit);
            var ipAddress = new IPAddress(ip);
            return ipAddress.ToString();
        }
        catch (Exception)
        {
            return ip.ToString();
        }
    }
}