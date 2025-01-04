using ivrToolkit.Core;
using ivrToolkit.Core.Interfaces;

namespace ivrToolkit.Plugin.SipSorcery
{
    public class SipPlugin : IIvrPlugin
    {
        public VoiceProperties VoiceProperties => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IIvrLine GetLine(int lineNumber)
        {
            throw new NotImplementedException();
        }
    }
}
