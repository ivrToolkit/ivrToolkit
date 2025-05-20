using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;

namespace ivrToolkit.Plugin.Dialogic.Sip;

public class StateProgress
{
    private bool _dialing;
    private bool _proceeding;
    private bool _alerting;
    private bool _connected;
    private bool _disconnected;

    public int LastEventState { get; private set; } = -1;
    public int LastCallState { get; private set; } = -1;

    public void SetState(int eventState, int lastCallState)
    {
        LastEventState = eventState;
        LastCallState = lastCallState;
        switch (eventState)
        {
            case gclib_h.GCEV_DIALING:
                _dialing = true;
                break;
            case gclib_h.GCEV_PROCEEDING:
                _proceeding = true;
                break;
            case gclib_h.GCEV_ALERTING:
                _alerting = true;
                break;
            case gclib_h.GCEV_CONNECTED:
                _connected = true;
                break;
            case gclib_h.GCEV_DISCONNECTED:
            case gclib_h.GCEV_DROPCALL:
            case gclib_h.GCEV_RELEASECALL:
                _disconnected = true;
                break;
        }
    }

    public bool IsRegularDial() => _dialing && _proceeding && _alerting && _connected;
    public bool IsDisconnected() => _disconnected;
}