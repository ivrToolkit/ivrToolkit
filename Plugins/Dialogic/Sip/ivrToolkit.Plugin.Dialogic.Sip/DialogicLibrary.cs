//Please note that the dll must exist in order for this using to work correctly.

using ivrToolkit.DialogicSipWrapper;

namespace ivrToolkit.Plugin.Dialogic.Sip
{
    class DialogicLibrary : ILibrary
    {
        private readonly DialogicSip _sip;

        public DialogicLibrary()
        {
            _sip = new DialogicSip();
        }

        public DialogicLibrary(DialogicSip sip)
        {
            _sip = sip;
        }

        public void StartLibraries(int h323SignalingPort, int sipSignalingPort, int maxCalls)
        {
            _sip.WStartLibraries(h323SignalingPort, sipSignalingPort, maxCalls);
        }
        public void StopLibraries()
        {
            _sip.WStopLibraries();
        }
    }
}
