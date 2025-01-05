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
        private readonly SipVoiceProperties _voiceProperties;
        private readonly int _lineNumber;
        private readonly ILogger<SipLine> _logger;
        private readonly SIPUserAgent _userAgent;
        private VoIPMediaSession? _voipMediaSession;

        private string _digitBuffer = "";
        private object _lockObject = new object();

        public SipLine(ILoggerFactory loggerFactory, SipVoiceProperties voiceProperties, int lineNumber, SIPTransport sipTransport)
        {
            loggerFactory.ThrowIfNull(nameof(loggerFactory));
            _voiceProperties = voiceProperties.ThrowIfNull(nameof(voiceProperties));
            _lineNumber = lineNumber;

            _logger = loggerFactory.CreateLogger<SipLine>();
            _logger.LogDebug("ctr(ILoggerFactory, VoiceProperties, {0})", lineNumber);

            _userAgent = new SIPUserAgent(sipTransport, null);
            _userAgent.ClientCallFailed += (uac, error, sipResponse) => _logger.LogDebug("Call failed {error}.", error);
            _userAgent.ClientCallAnswered += (uac, sipResonse) => _logger.LogDebug("Answered");
            _userAgent.ClientCallRinging += (uac, sipResonse) => _logger.LogDebug("Ringing");
            _userAgent.ClientCallTrying += (uac, sipResonse) => _logger.LogDebug("Trying");

            _userAgent.OnCallHungup += (dialog) => _logger.LogDebug("OnCallHungup");
            _userAgent.OnDtmfTone += (aByte, aInt) =>
            {
                _voipMediaSession?.AudioExtrasSource.CancelSendAudioFromStream();
                lock (_lockObject)
                {
                    switch (aByte)
                    {
                        case 10:
                            _digitBuffer += "*";
                            break;
                        case 11:
                            _digitBuffer += "#";
                            break;
                        default:
                            _digitBuffer += aByte.ToString();
                            break;
                    }
                }
                _logger.LogDebug("OnDtmfTone - {byte},{int}", aByte, aInt);
            };

            _userAgent.OnIncomingCall += (uac, sipAction) => _logger.LogDebug("OnIncomingCall");
            _userAgent.OnReinviteRequest += (inviteTransaction) => _logger.LogDebug("OnReinviteRequest");
            //_userAgent.OnRtpEvent += (rptEvent, header) => _logger.LogDebug("OnRtpEvent");
            _userAgent.RemotePutOnHold += () => _logger.LogDebug("RemotePutOnHold");

        }

        public IIvrLineManagement Management => this;

        public string LastTerminator { get; set; } = string.Empty;

        public int LineNumber => _lineNumber;

        public int Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
        {
            var to = $"{number}@{_voiceProperties.SipProxyIp}:{_voiceProperties.SipSignalingPort}";
            return DialAsync(_voiceProperties.SipAlias, _voiceProperties.SipPassword, to).GetAwaiter().GetResult(); // blocking
        }

        private async Task<CallAnalysis> DialAsync(string user, string pass, string to)
        {

            _voipMediaSession = new VoIPMediaSession();
            _voipMediaSession.AcceptRtpFromAny = true;
            _voipMediaSession.TakeOffHold();

            // Place the call and wait for the result.
            var callResult = await _userAgent.Call(to, user, pass, _voipMediaSession);

            if (!callResult)
            {
                _logger.LogDebug("The call failed!");
                return CallAnalysis.Error; // not really. It could be for some other reason
            }

            _voipMediaSession.AudioExtrasSource.AudioSamplePeriodMilliseconds = 20;
            await _voipMediaSession.AudioExtrasSource.StartAudio();

            return CallAnalysis.Connected;

        }

        public void Dispose()
        {
            _logger.LogDebug("Dispose() - Disposing of the line");
            _voipMediaSession?.Dispose();
            _voipMediaSession = null!; // todo fixme
            _userAgent.Dispose();
        }

        public string FlushDigitBuffer()
        {
            string currentDigitBuffer;
            lock(_lockObject)
            {
                currentDigitBuffer = _digitBuffer;
                _digitBuffer = "";
            }
            return currentDigitBuffer;
        }

        public string GetDigits(int numberOfDigits, string terminators)
        {
            // todo need to wait until timeout or terminator pressed.
            Task.Delay(5000).Wait();
            return FlushDigitBuffer();
        }

        public void Hangup()
        {
            _logger.LogDebug("Hangup()");
            _voipMediaSession?.Dispose();
            _voipMediaSession = null!; // todo fixme
            _userAgent.Hangup();
        }

        public void PlayFile(string filename)
        {
            PlayFileAsync(filename).GetAwaiter().GetResult();
        }

        private async Task PlayFileAsync(string filename)
        {
            var wavConverter = new WavConverter();
            await _voipMediaSession.AudioExtrasSource.SendAudioFromStream(
                wavConverter.NAudioConvert8BitUnsignedTo16BitSignedPCM(
                filename),
                AudioSamplingRatesEnum.Rate8KHz);
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
            _logger.LogDebug("ILineManagement.TriggerDispose() for line: {0}", _lineNumber);

            //var result = DXXXLIB_H.dx_stopch(_dxDev, DXXXLIB_H.EV_SYNC);
            //result.ThrowIfStandardRuntimeLibraryError(_dxDev);
            //_eventWaiter.DisposeTriggerActivated = true;
        }

        public void WaitRings(int rings)
        {
            throw new NotImplementedException();
        }
    }
}
