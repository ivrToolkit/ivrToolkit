using ivrToolkit.Core.Enums;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.Dialogic.Sip;

public class StateProgress
{
    private CallStateProgressEnum _callStateProgress = CallStateProgressEnum.None;
    
    public int LastEventState { get; private set; } = -1;
    public int LastCallState { get; private set; } = -1;

    public void SetState(int eventState, int lastCallState)
    {
        LastEventState = eventState;
        LastCallState = lastCallState;
        switch (eventState)
        {
            case gclib_h.GCEV_PROCEEDING:
                _callStateProgress |= CallStateProgressEnum.Proceeding;
                break;
            case gclib_h.GCEV_ALERTING:
                _callStateProgress |= CallStateProgressEnum.Alerting;
                break;
            case gclib_h.GCEV_CONNECTED:
                _callStateProgress |= CallStateProgressEnum.Connected;
                break;
            case gclib_h.GCEV_DISCONNECTED:
            case gclib_h.GCEV_DROPCALL:
            case gclib_h.GCEV_RELEASECALL:
                _callStateProgress |= CallStateProgressEnum.Disconnected;
                break;
        }
    }

    public bool IsRegularDial() =>
        _callStateProgress.HasAll(CallStateProgressEnum.Proceeding | CallStateProgressEnum.Alerting | CallStateProgressEnum.Connected);

    public bool IsDisconnected() => _callStateProgress.HasAll(CallStateProgressEnum.Disconnected);
    
    public CallStateProgressEnum GetCallStateProgress() => _callStateProgress;
}

public static class EnumExtensions
{
    public static bool HasAll(this CallStateProgressEnum value, CallStateProgressEnum flags) =>
        (value & flags) == flags;
}