using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SIPSorcery.Media;
using SIPSorcery.SIP.App;
using SIPSorcery.SIP;
using SIPSorceryMedia.Abstractions;
using System.Text;
using ivrToolkit.Core.Exceptions;
using System.Diagnostics;
namespace ivrToolkit.Plugin.SipSorcery;

internal class SipSorceryLine : IIvrBaseLine, IIvrLineManagement
{
    private readonly SipVoiceProperties _voiceProperties;
    private readonly int _lineNumber;
    private readonly ILogger<SipSorceryLine> _logger;
    private readonly SIPUserAgent _userAgent;
    private VoIPMediaSession? _voipMediaSession;

    private string _digitBuffer = "";
    private bool _digitPressed;
    private readonly object _lockObject = new object();
    private readonly KeypressSemaphore _keypressSemaphore;

    public SipSorceryLine(ILoggerFactory loggerFactory, SipVoiceProperties voiceProperties, int lineNumber, SIPTransport sipTransport)
    {
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _voiceProperties = voiceProperties.ThrowIfNull(nameof(voiceProperties));
        _lineNumber = lineNumber;

        _logger = loggerFactory.CreateLogger<SipSorceryLine>();
        _logger.LogDebug("ctr(ILoggerFactory, VoiceProperties, {0})", lineNumber);

        _keypressSemaphore = new KeypressSemaphore(loggerFactory);

        _userAgent = new SIPUserAgent(sipTransport, null);

        _userAgent.ClientCallFailed += (_, error, _) => _logger.LogDebug("Call failed {error}.", error);
        _userAgent.ClientCallAnswered += (_, _) => _logger.LogDebug("Answered");
        _userAgent.ClientCallRinging += (_, _) => _logger.LogDebug("Ringing");
        _userAgent.ClientCallTrying += (_, _) => _logger.LogDebug("Trying");

        _userAgent.OnCallHungup += (_) =>
        {
            _logger.LogDebug("OnCallHungup");
            CloseDialResources();
        };
        _userAgent.OnDtmfTone += (aByte, aInt) =>
        {
            _logger.LogDebug("OnDtmfTone - {byte},{int}", aByte, aInt);

            _digitPressed = true;
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
                _keypressSemaphore.ReleaseMaybe(_digitBuffer);
            }
        };

        _userAgent.OnIncomingCall += (_, _) => _logger.LogDebug("OnIncomingCall");
        _userAgent.OnReinviteRequest += (_) => _logger.LogDebug("OnReinviteRequest");
        //_userAgent.OnRtpEvent += (rptEvent, header) => _logger.LogDebug("OnRtpEvent");
        _userAgent.RemotePutOnHold += () => _logger.LogDebug("RemotePutOnHold");

    }

    private void CloseDialResources()
    {
        _logger.LogDebug("{name}", nameof(CloseDialResources));

        _voipMediaSession?.Dispose();
        _voipMediaSession = null;
        //_userAgent.Hangup();

        _keypressSemaphore.Teardown();
    }

    public IIvrLineManagement Management => this;

    public string LastTerminator { get; set; } = string.Empty;

    public int LineNumber => _lineNumber;

    public int Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
    {
        // reset the line
        _keypressSemaphore.Teardown();
        _digitBuffer = "";
        _digitPressed = false;
        LastTerminator = "";

        return DialAsync(number, answeringMachineLengthInMilliseconds, CancellationToken.None).GetAwaiter().GetResult(); // blocking
    }

    public async Task<CallAnalysis> DialAsync(string number, int answeringMachineLengthInMilliseconds, CancellationToken cancellationToken)
    {
        // reset the line
        _keypressSemaphore.Teardown();
        _digitBuffer = "";
        _digitPressed = false;
        LastTerminator = "";

        var to = $"{number}@{_voiceProperties.SipProxyIp}:{_voiceProperties.SipSignalingPort}";
            
        _voipMediaSession = new VoIPMediaSession();
        _voipMediaSession.AcceptRtpFromAny = true;
        _voipMediaSession.TakeOffHold();

        // Place the call and wait for the result.
        var startTime = Stopwatch.GetTimestamp();
        var callResult = await _userAgent.Call(to, _voiceProperties.SipAlias, _voiceProperties.SipPassword, _voipMediaSession);
        var duration = Stopwatch.GetElapsedTime(startTime);
        _logger.LogInformation("Dial call duration: {duration}", duration);

        if (!callResult)
        {
            _logger.LogDebug("The call failed!");
            return CallAnalysis.Error; // not really. It could be for some other reason
        }

        _voipMediaSession.AudioExtrasSource.AudioSamplePeriodMilliseconds = 20;
        await _voipMediaSession.AudioExtrasSource.StartAudio();

        if (!_userAgent.IsCallActive)
        {
            // break area for testing
        }

        return CallAnalysis.Connected;
    }
        
    public void Dispose()
    {
        _logger.LogDebug("Dispose() - Disposing of the line");
        CloseDialResources();
        _userAgent.Dispose();
    }

        
    public string FlushDigitBuffer()
    {
        string currentDigitBuffer;
        lock(_lockObject)
        {
            _logger.LogDebug("{name}() - Digit Buffer is currently: {buffer}", nameof(FlushDigitBuffer), _digitBuffer);
            currentDigitBuffer = _digitBuffer;
            _digitBuffer = "";
        }
        return currentDigitBuffer;
    }

    public string GetDigits(int numberOfDigits, string terminators)
    {
        return GetDigitsInternalAsync(numberOfDigits, terminators, 
            ms => { var result = _keypressSemaphore.Wait(ms); return Task.FromResult(result); }
        ).GetAwaiter().GetResult();
    }

    public async Task<string> GetDigitsAsync(int numberOfDigits, string terminators, CancellationToken cancellationToken)
    {
        return await GetDigitsInternalAsync(numberOfDigits, terminators, 
            async (ms) => await _keypressSemaphore.WaitAsync(ms, cancellationToken));
    }


    private async Task<string> GetDigitsInternalAsync(int numberOfDigits, string terminators,
        Func<int, Task<bool>> wait)
    {
        if (!_userAgent.IsCallActive)
        {
            // break area for testing
            _logger.LogDebug("{name}({digits}, {terminators}) - call is not active, throwing HangupException", nameof(GetDigits), numberOfDigits, terminators);
            if (_digitBuffer.Length != 0)
                CloseDialResources();
            throw new HangupException();
        }

        _logger.LogDebug("{name}({digits}, {terminators})", nameof(GetDigits), numberOfDigits, terminators);
        if (_digitBuffer.Length != 0)
        {
            // we need to deal with whatever is in the buffer already
            var result = GetUpToAndIncludingTerminator(numberOfDigits, terminators);
            if (result != "")
            {
                // if there is anything left, set _digitPressed to true, otherwise set it tp false
                _digitPressed = _digitBuffer.Length > 0;
                _logger.LogDebug("{name}({digits}, {terminators}) - returning {result}", 
                    nameof(GetDigits), numberOfDigits, terminators, result);
                return result;
            }
        }

            
        _keypressSemaphore.Setup( numberOfDigits, terminators);

        _logger.LogDebug("_semaphore wait for {} milliseconds", _voiceProperties.DigitsTimeoutInMilli);
        var worked = await wait(_voiceProperties.DigitsTimeoutInMilli);
        if (!worked)
        {
            // A teardown must have happened during hangup
            _logger.LogDebug("{name}({digits}, {terminators}) - _keypressSemaphore.Wait was cancelled, throwing HangupException", nameof(GetDigits), numberOfDigits, terminators);
            throw new HangupException();
        }

        _keypressSemaphore.Teardown();

        // reset this so that messages can be played again
        _logger.LogDebug("setting _digitPressed=false and returning FlushDigitBuffer()");
        _digitPressed = false;
        return FlushDigitBuffer();
    }
        
    private string GetUpToAndIncludingTerminator(int numberOfDigits, string terminators)
    {
        _logger.LogDebug("{name}({digits}, {terminators})", nameof(GetUpToAndIncludingTerminator), numberOfDigits, terminators);
        var result = new StringBuilder();
        ReadOnlySpan<char> span = _digitBuffer.AsSpan();
        foreach (char c in span)
        {
            result.Append(c);
            if (result.Length == numberOfDigits || terminators.Contains(c))
            {
                var resultString = result.ToString();
                // pull what we are returning out of the buffer
                _digitBuffer = _digitBuffer.Substring(result.Length); // todo this may cause an error if we are returning everything
                _logger.LogDebug("{name}({digits}, {terminators}) - returning {result}, _digitBuffer is now = {buffer}", 
                    nameof(GetUpToAndIncludingTerminator), numberOfDigits, terminators, resultString, _digitBuffer);
                return resultString;
            }
        }
        // did not find a terminator and did not hit the number of digits required
        _logger.LogDebug("{name}({digits}, {terminators}) - did not find a terminator or hit the number of digits required",
            nameof(GetUpToAndIncludingTerminator), numberOfDigits, terminators);
        return "";
    }

    public void Hangup()
    {
        _logger.LogDebug("Hangup()");
        CloseDialResources();
    }

    public void PlayFile(string filename)
    {
        _logger.LogDebug("{}({}) - _digitPressed = {pressed}", nameof(PlayFile), filename, _digitPressed);
        if (!_userAgent.IsCallActive)
        {
            // break area for testing
            _logger.LogDebug("{}({}) (1)- _userAgent is inactive, throwing HangupException", nameof(PlayFile), filename);
            CloseDialResources();
            throw new HangupException();
        }
        if (_digitPressed) return; // todo there is(should be) a way to stop digit presses during play.
        PlayFileAsync(filename).GetAwaiter().GetResult();
        Task.Delay(200).Wait();
        if (!_userAgent.IsCallActive)
        {
            _logger.LogDebug("{}({}) (2)- _userAgent is inactive, throwing HangupException", nameof(PlayFile), filename);
            // break area for testing
            CloseDialResources();
            throw new HangupException();
        }
    }

    public async Task PlayFileAsync(string filename, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{}({}) - _digitPressed = {pressed}", nameof(PlayFile), filename, _digitPressed);
        if (!_userAgent.IsCallActive)
        {
            // break area for testing
            _logger.LogDebug("{}({}) (1)- _userAgent is inactive, throwing HangupException", nameof(PlayFile), filename);
            CloseDialResources();
            throw new HangupException();
        }
        if (_digitPressed) return; // todo there is(should be) a way to stop digit presses during play.
        await PlayFileAsync(filename);
        await Task.Delay(200, cancellationToken);
        if (!_userAgent.IsCallActive)
        {
            _logger.LogDebug("{}({}) (2)- _userAgent is inactive, throwing HangupException", nameof(PlayFile), filename);
            // break area for testing
            CloseDialResources();
            throw new HangupException();
        }
    }

    private async Task PlayFileAsync(string filename)
    {
        if (_voipMediaSession == null) return;
            
        await _voipMediaSession.AudioExtrasSource.SendAudioFromStream(new FileStream(filename, FileMode.Open),
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