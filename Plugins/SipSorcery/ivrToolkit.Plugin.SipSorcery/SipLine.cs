using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SIPSorcery.Media;
using SIPSorcery.SIP.App;
using SIPSorcery.SIP;
using SIPSorceryMedia.Abstractions;
using SIPSorcery.Net;

namespace ivrToolkit.Plugin.SipSorcery
{
    internal class SipLine : IIvrBaseLine, IIvrLineManagement
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly SipVoiceProperties _voiceProperties;
        private readonly int _lineNumber;
        private readonly ILogger<SipLine> _logger;
        private readonly SIPUserAgent _userAgent;

        public SipLine(ILoggerFactory loggerFactory, SipVoiceProperties voiceProperties, int lineNumber, SIPTransport sipTransport)
        {
            _loggerFactory = loggerFactory.ThrowIfNull(nameof(loggerFactory));
            _voiceProperties = voiceProperties.ThrowIfNull(nameof(voiceProperties));
            _lineNumber = lineNumber;

            _logger = loggerFactory.CreateLogger<SipLine>();
            _logger.LogDebug("ctr(ILoggerFactory, VoiceProperties, {0})", lineNumber);

            _userAgent = new SIPUserAgent(sipTransport, null);
        }

        public IIvrLineManagement Management => this;

        public string LastTerminator { get; set; }

        public int LineNumber => _lineNumber;

        public int Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
        {
            var to = $"{number}@{_voiceProperties.SipProxyIp}:{_voiceProperties.SipSignalingPort}";
            TestAsync(_voiceProperties.SipAlias, _voiceProperties.SipPassword, to).GetAwaiter().GetResult(); // blocking

            return CallAnalysis.Busy;
        }

        private async Task TestAsync(string user, string pass, string to)
        {

            var voipMediaSession = new VoIPMediaSession();
            voipMediaSession.AcceptRtpFromAny = true;
            voipMediaSession.TakeOffHold();

            // Place the call and wait for the result.
            var callResult = await _userAgent.Call(to, user, pass, voipMediaSession);

            if (!callResult)
            {
                _logger.LogDebug("The call failed!");
                return;
            }

            voipMediaSession.AudioExtrasSource.AudioSamplePeriodMilliseconds = 20;
            await voipMediaSession.AudioExtrasSource.StartAudio();

            WavConverter wavConverter = new WavConverter();
            await voipMediaSession.AudioExtrasSource.SendAudioFromStream(
                wavConverter.NAudioConvert8BitUnsignedTo16BitSignedPCM(
                "Voice Files/Press1234.wav"),
                AudioSamplingRatesEnum.Rate8KHz);

        }

        public void Dispose()
        {
            _logger.LogDebug("Dispose() - Disposing of the line");
            _userAgent.Dispose();
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
            _logger.LogDebug("Hangup()");
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
            _logger.LogDebug("TakeOffHook()");
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
