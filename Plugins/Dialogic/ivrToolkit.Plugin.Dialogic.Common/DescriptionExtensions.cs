using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
namespace ivrToolkit.Plugin.Dialogic.Common
{
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

            return null;
        }
    }
}
