namespace ivrToolkit.Plugin.Dialogic.Sip
{
    public interface ILibrary
    {

        void StartLibraries(int h323_signaling_port, int sip_signaling_port, int maxCalls);
        void StopLibraries();

    }
}
