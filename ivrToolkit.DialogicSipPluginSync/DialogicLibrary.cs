using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core;
using NLog;
//Please note that the dll must exist in order for this using to work correctly.
using DialogicWrapperSync;

namespace ivrToolkit.DialogicSipPluginSync
{
    class DialogicLibrary : ILibrary
    {
        public void StartLibraries()
        {
            DialogicSIPSync sip = new DialogicSIPSync();
            sip.WStartLibraries();
        }
        public void StopLibraries()
        {
            DialogicSIPSync sip = new DialogicSIPSync();
            sip.WStopLibraries();
        }
    }
}
