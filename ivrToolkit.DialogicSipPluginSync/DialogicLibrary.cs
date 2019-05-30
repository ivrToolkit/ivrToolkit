//Please note that the dll must exist in order for this using to work correctly.
using DialogicWrapperSync;

namespace ivrToolkit.DialogicSipPluginSync
{
    class DialogicLibrary : ILibrary
    {
        public void StartLibraries(int h323SignalingPort, int sipSignalingPort)
        {
            DialogicSIPSync sip = new DialogicSIPSync();
            sip.WStartLibraries(h323SignalingPort, sipSignalingPort);
        }
        public void StopLibraries()
        {
            DialogicSIPSync sip = new DialogicSIPSync();
            sip.WStopLibraries();
        }
    }
}
