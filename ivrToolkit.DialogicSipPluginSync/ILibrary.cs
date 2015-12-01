using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ivrToolkit.DialogicSipPluginSync
{
    public interface ILibrary
    {

        void StartLibraries(int h323_signaling_port, int sip_signaling_port);
        void StopLibraries();

    }
}
