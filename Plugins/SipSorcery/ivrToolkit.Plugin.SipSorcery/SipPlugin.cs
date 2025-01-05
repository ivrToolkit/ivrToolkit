using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SIPSorcery.Media;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;
using SIPSorceryMedia.Abstractions;

namespace ivrToolkit.Plugin.SipSorcery
{
    public class SipPlugin : IIvrPlugin
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly SipVoiceProperties _voiceProperties;
        private readonly ILogger<SipPlugin> _logger;
        private bool _disposed;

        public SipPlugin(ILoggerFactory loggerFactory, SipVoiceProperties voiceProperties)
        {
            loggerFactory.ThrowIfNull(nameof(loggerFactory));
            voiceProperties.ThrowIfNull(nameof(voiceProperties));

            _loggerFactory = loggerFactory;
            _voiceProperties = voiceProperties;
            _logger = loggerFactory.CreateLogger<SipPlugin>();

            _logger.LogDebug("ctr()");

            Start();
        }

        public void Start()
        {
            var to = $"{_voiceProperties.TemporaryPhoneNumber}@{_voiceProperties.SipProxyIp}:{_voiceProperties.SipSignalingPort}";
            TestAsync(_voiceProperties.SipAlias, _voiceProperties.SipPassword, to).GetAwaiter().GetResult(); // blocking
        }


            // userAgent.ClientCallFailed += (uac, error, sipResponse) => _logger.LogDebug("Call failed {error}.", error);
            //userAgent.ClientCallFailed += (uac, error, sipResponse) => exitCts.Cancel();
            //userAgent.OnCallHungup += (dialog) => exitCts.Cancel();
            //userAgent.ClientCallAnswered += (uac, sipResonse) => _logger.LogDebug("Answered");
            //userAgent.ClientCallFailed += (uac, error, sipResonse) => _logger.LogDebug("Call failed {error}.", error);
            //userAgent.OnCallHungup += (dialog) => _logger.LogDebug("Hungup");
            //userAgent.ClientCallRinging += (uac, sipResonse) => _logger.LogDebug("Ringing");
            //userAgent.ClientCallTrying += (uac, sipResonse) => _logger.LogDebug("Trying");
            //userAgent.OnCallHungup += (dialog) => _logger.LogDebug("OnCallHungup");
            //userAgent.OnDtmfTone += (aByte, aInt) => _logger.LogDebug("OnDtmfTone - {byte},{int}", aByte, aInt);
            //userAgent.OnIncomingCall += (uac, sipAction) => _logger.LogDebug("OnIncomingCall");
            //userAgent.OnReinviteRequest += (inviteTransaction) => _logger.LogDebug("OnReinviteRequest");
            //userAgent.OnRtpEvent += (rptEvent, header) => _logger.LogDebug("OnRtpEvent");
            //userAgent.RemotePutOnHold += () => _logger.LogDebug("RemotePutOnHold");

        
        
        private async Task TestAsync(string user, string pass, string to)
        {
            CancellationTokenSource exitCts = new CancellationTokenSource();
            var sipTransport = new SIPTransport();
            sipTransport.EnableTraceLogs();

            var userAgent = new SIPUserAgent(sipTransport, null);

            var voipMediaSession = new VoIPMediaSession();
            voipMediaSession.AcceptRtpFromAny = true;

            // Place the call and wait for the result.
            var callResult = await userAgent.Call(to, user, pass, voipMediaSession);

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


        public VoiceProperties VoiceProperties => _voiceProperties;

        public void Dispose()
        {
            if (_disposed)
            {
                _logger.LogWarning("Dispose() - Already Disposed");
                return;
            }
            _logger.LogDebug("Dispose()");
            _disposed = true;
        }

        public IIvrLine GetLine(int lineNumber)
        {
            _logger.LogDebug("GetLine({0})", lineNumber);
            lineNumber.ThrowIfLessThanOrEqualTo(0, nameof(lineNumber));

            if (_disposed) throw new DisposedException("You cannot get a line from a disposed plugin");

            var line = new SipLine(_loggerFactory, _voiceProperties, lineNumber);
            return new LineWrapper(_loggerFactory, lineNumber, line);
        }
    }
}
