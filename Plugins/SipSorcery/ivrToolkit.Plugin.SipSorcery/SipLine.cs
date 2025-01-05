using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.SipSorcery
{
    internal class SipLine : IIvrBaseLine, IIvrLineManagement
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly SipVoiceProperties _voiceProperties;
        private readonly int _lineNumber;
        private readonly ILogger<SipLine> _logger;

        public SipLine(ILoggerFactory loggerFactory, SipVoiceProperties voiceProperties, int lineNumber)
        {
            _loggerFactory = loggerFactory.ThrowIfNull(nameof(loggerFactory));
            _voiceProperties = voiceProperties.ThrowIfNull(nameof(voiceProperties));
            _lineNumber = lineNumber;

            _logger = loggerFactory.CreateLogger<SipLine>();
            _logger.LogDebug("ctr(ILoggerFactory, VoiceProperties, {0})", lineNumber);

            Start();
        }
        private void Start()
        {
            _logger.LogDebug("Start() - Starting line: {0}", _lineNumber);
        }

        public IIvrLineManagement Management => this;

        public string LastTerminator { get; set; }

        public int LineNumber => _lineNumber;

        public int Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _logger.LogDebug("Dispose() - Disposing of the line");
        }

        public string FlushDigitBuffer()
        {
            throw new NotImplementedException();
        }

        public string GetDigits(int numberOfDigits, string terminators)
        {
            throw new NotImplementedException();
        }

        public void Hangup()
        {
            throw new NotImplementedException();
        }

        public void PlayFile(string filename)
        {
            throw new NotImplementedException();
        }

        public void RecordToFile(string filename)
        {
            throw new NotImplementedException();
        }

        public void RecordToFile(string filename, int timeoutMillisconds)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void TakeOffHook()
        {
            throw new NotImplementedException();
        }

        public void TriggerDispose()
        {
            throw new NotImplementedException();
        }

        public void WaitRings(int rings)
        {
            throw new NotImplementedException();
        }
    }
}
