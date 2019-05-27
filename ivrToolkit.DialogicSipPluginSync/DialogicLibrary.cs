using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
//Please note that the dll must exist in order for this using to work correctly.
using DialogicWrapperSync;

namespace ivrToolkit.DialogicSipPluginSync
{
    class DialogicLibrary : ILibrary
    {
        public void StartLibraries(int h323_signaling_port, int sip_signaling_port)
        {
            DialogicSIPSync sip = new DialogicSIPSync();
            sip.WStartLibraries(h323_signaling_port, sip_signaling_port);
        }
        public void StopLibraries()
        {
            DialogicSIPSync sip = new DialogicSIPSync();
            sip.WStopLibraries();
        }
    }
}
