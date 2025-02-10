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
using System.Net;
using NAudio.Wave;
using SIPSorcery.Net;

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
    private readonly IncomingSemaphore _incomingSemaphore;
    private bool _dialResourcesClosed;
    
    private const string ROOT = "System Recordings\\";
    
    private static WaveFileWriter? _waveFile;
    
    private readonly InviteManager _inviteManager;
    private int _volume;
    private SIPResponseStatusCodesEnum _responseStatus;

    public SipSorceryLine(ILoggerFactory loggerFactory, 
        SipVoiceProperties voiceProperties, 
        int lineNumber, 
        SIPTransport sipTransport,
        InviteManager inviteManager)
    {
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _voiceProperties = voiceProperties.ThrowIfNull(nameof(voiceProperties));
        _inviteManager = inviteManager.ThrowIfNull(nameof(inviteManager));
        
        _lineNumber = lineNumber;

        _logger = loggerFactory.CreateLogger<SipSorceryLine>();
        _logger.LogDebug("ctr(ILoggerFactory, VoiceProperties, {0})", lineNumber);

        _keypressSemaphore = new KeypressSemaphore(loggerFactory);
        _incomingSemaphore = new IncomingSemaphore(loggerFactory);

        _userAgent = new SIPUserAgent(sipTransport, null);

        _userAgent.ClientCallFailed += (_, error, response) =>
        {
            _logger.LogDebug("Call failed: {error}.", error);
            if (response == null)
            {
                _logger.LogWarning("Call failed but the response was null.");
                // This happens if the call timed out before there was an answer
                _responseStatus = SIPResponseStatusCodesEnum.RequestTimeout;
                return;
            }
            _responseStatus = response.Status;
        };
        
        _userAgent.ClientCallAnswered += (_, response) =>
        {
            _responseStatus = response.Status;
            _logger.LogDebug("Answered");
        };
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

        _userAgent.OnReinviteRequest += (_) => _logger.LogDebug("OnReinviteRequest");
        _userAgent.RemotePutOnHold += () => _logger.LogDebug("RemotePutOnHold");
    }

    private void CloseDialResources()
    {
        _logger.LogDebug("{method}", nameof(CloseDialResources));
        
        lock (_lockObject)
        {
            // in case we hung up during a recording
            _waveFile?.Close();
            
            if (_dialResourcesClosed)
            {
                _logger.LogDebug("Dial resources are already closed.");
                return;
            }
            
            _keypressSemaphore.Teardown();
            _incomingSemaphore.Teardown();
            _dialResourcesClosed = true;
            _voipMediaSession?.Dispose();
            _voipMediaSession = null;

            if (_userAgent.IsCallActive) _userAgent.Hangup();
        }
    }

    public IIvrLineManagement Management => this;

    public int LineNumber => _lineNumber;

    public int Volume
    {
        // todo do something with the volume
        get => _volume;
        set
        {
            if (value < -10 || value > 10)
            {
                throw new VoiceException("size must be between -10 to 10");
            }

            _volume = value;
        }
    }
    
    public string LastTerminator { get; set; } = string.Empty;

    private void ResetLine()
    {
        _logger.LogDebug("{method}", nameof(ResetLine));
        // reset the line
        _dialResourcesClosed = false;
        _keypressSemaphore.Teardown();
        _digitBuffer = "";
        _digitPressed = false;
        _waveFile = null;
    }

    public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
    {
        return DialAsync(number, answeringMachineLengthInMilliseconds, CancellationToken.None).GetAwaiter().GetResult(); // blocking
    }
    
    public async Task<CallAnalysis> DialAsync(string number, int answeringMachineLengthInMilliseconds, CancellationToken cancellationToken)
    {
        ResetLine();
        var to = $"{number}@{_voiceProperties.SipServer}";
            
        _voipMediaSession = new VoIPMediaSession();
        _voipMediaSession.AcceptRtpFromAny = true;
        _voipMediaSession.TakeOffHold();

        // Place the call and wait for the result.
        var startTime = Stopwatch.GetTimestamp();
        var callResult = await _userAgent.Call(
            to, _voiceProperties.SipUsername, _voiceProperties.SipPassword, 
            _voipMediaSession, _voiceProperties.SipRingTimeoutInSeconds);
        var duration = Stopwatch.GetElapsedTime(startTime);
        _logger.LogInformation("Dial call duration: {duration}", duration);

        if (!callResult)
        {
            _logger.LogDebug("The call failed: {status}", _responseStatus);
            switch (_responseStatus)
            {
                case SIPResponseStatusCodesEnum.BusyHere:
                    return CallAnalysis.Busy;
                case SIPResponseStatusCodesEnum.RequestTimeout:
                    return CallAnalysis.NoAnswer;
                case SIPResponseStatusCodesEnum.NotFound:
                case SIPResponseStatusCodesEnum.Forbidden:
                case SIPResponseStatusCodesEnum.Gone:
                case SIPResponseStatusCodesEnum.ServiceUnavailable:
                    return CallAnalysis.OperatorIntercept;
                default:
                    return CallAnalysis.Error;
            }
        }

        _voipMediaSession.AudioExtrasSource.AudioSamplePeriodMilliseconds = 20;
        await _voipMediaSession.AudioExtrasSource.StartAudio();
        return CallAnalysis.Connected;
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed)
        {
            _logger.LogDebug("{method}() - Already disposed of the line", nameof(Dispose));
            return;
        }
        _disposed = true;
        
        _logger.LogDebug("{method}() - Disposing of the line", nameof(Dispose));
        CloseDialResources();
        _userAgent.Dispose();
        Task.Delay(1000).GetAwaiter().GetResult(); // give dispose time to complete
    }

        
    public string FlushDigitBuffer()
    {
        string currentDigitBuffer;
        lock(_lockObject)
        {
            _logger.LogDebug("{method}() - Digit Buffer is currently: {buffer}", nameof(FlushDigitBuffer), _digitBuffer);
            currentDigitBuffer = _digitBuffer;
            _digitBuffer = "";
            _digitPressed = false;
        }
        return currentDigitBuffer;
    }
    
    public string GetDigits(int numberOfDigits, string terminators, int timeoutMilliseconds = 0)
    {
        return GetDigitsInternalAsync(numberOfDigits, terminators, timeoutMilliseconds,
            ms => { var result = _keypressSemaphore.Wait(ms); return Task.FromResult(result); }
        ).GetAwaiter().GetResult();
    }

    public async Task<string> GetDigitsAsync(int numberOfDigits, string terminators, CancellationToken cancellationToken, int timeoutMilliseconds = 0)
    {
        return await GetDigitsInternalAsync(numberOfDigits, terminators, timeoutMilliseconds,
            async (ms) => await _keypressSemaphore.WaitAsync(ms, cancellationToken));
    }


    private async Task<string> GetDigitsInternalAsync(int numberOfDigits, string terminators, int timeoutMilliseconds,
        Func<int, Task<bool>> wait)
    {
        if (!_userAgent.IsCallActive)
        {
            // break area for testing
            _logger.LogDebug("{method}({digits}, {terminators}) - call is not active, throwing HangupException", nameof(GetDigits), numberOfDigits, terminators);
            if (_digitBuffer.Length != 0)
                CloseDialResources();
            throw new HangupException();
        }

        _logger.LogDebug("{method}({digits}, {terminators})", nameof(GetDigits), numberOfDigits, terminators);
        if (_digitBuffer.Length != 0)
        {
            // we need to deal with whatever is in the buffer already
            var result = GetUpToAndIncludingTerminator(numberOfDigits, terminators);
            if (result != "")
            {
                // if there is anything left, set _digitPressed to true, otherwise set it tp false
                _digitPressed = _digitBuffer.Length > 0;
                _logger.LogDebug("{method}({digits}, {terminators}) - returning {result}", 
                    nameof(GetDigits), numberOfDigits, terminators, result);
                return result;
            }
        }

            
        _keypressSemaphore.Setup( numberOfDigits, terminators);

        if (timeoutMilliseconds == 0) timeoutMilliseconds = _voiceProperties.DigitsTimeoutInMilli;

        _logger.LogDebug("_semaphore wait for {} milliseconds", timeoutMilliseconds);
        var worked = await wait(timeoutMilliseconds);
        if (!worked)
        {
            // A teardown must have happened during hangup
            _logger.LogDebug("{method}({digits}, {terminators}) - _keypressSemaphore.Wait was cancelled, throwing HangupException", nameof(GetDigits), numberOfDigits, terminators);
            throw new HangupException();
        }

        _keypressSemaphore.Teardown();

        // reset this so that messages can be played again
        _logger.LogDebug("setting _digitPressed=false and returning FlushDigitBuffer()");
        _digitPressed = false;
        var answer = FlushDigitBuffer();
        SetLastTerminator(answer, terminators);
        return answer;
    }
    
    private void SetLastTerminator(string answer, string? terminators)
    {
        _logger.LogDebug("{method}({answer}, {terminators})", nameof(SetLastTerminator), answer, terminators);

        LastTerminator = "";
        if (answer.Length >= 1)
        {
            var lastDigit = answer.Substring(answer.Length - 1, 1);
            if (!string.IsNullOrWhiteSpace(terminators))
            {
                if (terminators.IndexOf(lastDigit, StringComparison.Ordinal) != -1)
                {
                    LastTerminator = lastDigit;
                }
            }
        }
    }

    private string GetUpToAndIncludingTerminator(int numberOfDigits, string terminators)
    {
        _logger.LogDebug("{method}({digits}, {terminators})", nameof(GetUpToAndIncludingTerminator), numberOfDigits, terminators);
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
                _logger.LogDebug("{method}({digits}, {terminators}) - returning {result}, _digitBuffer is now = {buffer}", 
                    nameof(GetUpToAndIncludingTerminator), numberOfDigits, terminators, resultString, _digitBuffer);
                return resultString;
            }
        }
        // did not find a terminator and did not hit the number of digits required
        _logger.LogDebug("{method}({digits}, {terminators}) - did not find a terminator or hit the number of digits required",
            nameof(GetUpToAndIncludingTerminator), numberOfDigits, terminators);
        return "";
    }
    
    public void Hangup()
    {
        _logger.LogDebug("{method}()", nameof(Hangup));
        CloseDialResources();
    }

    public void PlayFile(string filename)
    {
        _logger.LogDebug("{method}({fileName}) - _digitPressed = {pressed}", nameof(PlayFile), filename, _digitPressed);
        if (!_userAgent.IsCallActive)
        {
            // break area for testing
            _logger.LogDebug("{method}({filename}) - _userAgent is inactive, throwing HangupException", nameof(PlayFile), filename);
            CloseDialResources();
            throw new HangupException();
        }
        if (_digitPressed) return; // todo there is(should be) a way to stop digit presses during play.
        PlayFileInternalAsync(filename).GetAwaiter().GetResult();
        Task.Delay(300).Wait();
        if (!_userAgent.IsCallActive)
        {
            _logger.LogDebug("{method}({filename}) - _userAgent is inactive, throwing HangupException", nameof(PlayFile), filename);
            // break area for testing
            CloseDialResources();
            throw new HangupException();
        }
    }


    public async Task PlayFileAsync(string filename, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({filename}) - _digitPressed = {pressed}", nameof(PlayFile), filename, _digitPressed);
        if (!_userAgent.IsCallActive)
        {
            // break area for testing
            _logger.LogDebug("{method}({filename}) - _userAgent is inactive, throwing HangupException", nameof(PlayFile), filename);
            CloseDialResources();
            throw new HangupException();
        }
        if (_digitPressed) return;
        await PlayFileInternalAsync(filename);
        await Task.Delay(300, cancellationToken);
        if (!_userAgent.IsCallActive)
        {
            _logger.LogDebug("{method}({filename}) - _userAgent is inactive, throwing HangupException", nameof(PlayFile), filename);
            // break area for testing
            CloseDialResources();
            throw new HangupException();
        }
    }

    private async Task PlayFileInternalAsync(string filename)
    {
        if (_voipMediaSession == null) return;
        
        var memoryStream = GetFileStream(filename);
        var headerSize = GetWavHeaderSize(memoryStream);
        memoryStream = SkipFirstBytes(memoryStream, headerSize);
        
        await _voipMediaSession.AudioExtrasSource.SendAudioFromStream(memoryStream,
            AudioSamplingRatesEnum.Rate8KHz);
    }

    private MemoryStream GetFileStream(string filename)
    {
        using FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        // Now you can read the raw audio data
        var audioData = new byte[fs.Length];
        fs.ReadExactly(audioData, 0, audioData.Length);
        return new MemoryStream(audioData);
    }
    
    private MemoryStream SkipFirstBytes(MemoryStream originalStream, int bytesToSkip)
    {
        originalStream.Seek(0, SeekOrigin.Begin);
        if (originalStream.Length <= bytesToSkip) return originalStream;

        var buffer = originalStream.ToArray();
        return new MemoryStream(buffer, bytesToSkip, (int)originalStream.Length - bytesToSkip, writable: false);
    }
    
    private int GetWavHeaderSize(Stream wavStream)
    {
        wavStream.Seek(0, SeekOrigin.Begin);
        using (var reader = new BinaryReader(wavStream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            // Read the "RIFF" chunk descriptor
            if (new string(reader.ReadChars(4)) != "RIFF")
                throw new InvalidDataException("Invalid WAV file: Missing RIFF header");

            reader.ReadInt32(); // Skip file size
            if (new string(reader.ReadChars(4)) != "WAVE")
                throw new InvalidDataException("Invalid WAV file: Missing WAVE format");

            // Search for the "data" chunk
            while (wavStream.Position < wavStream.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();

                if (chunkId == "data")
                {
                    return (int)wavStream.Position; // Current position is the header size
                }

                // Skip this chunk and move to the next
                wavStream.Seek(chunkSize, SeekOrigin.Current);
            }

            throw new InvalidDataException("Invalid WAV file: No data chunk found");
        }
    }
 
    public void PlayWavStream(MemoryStream audioStream)
    {
        PlayWavStreamAsync(audioStream, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task PlayWavStreamAsync(MemoryStream audioStream, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}() - _digitPressed = {pressed}", nameof(PlayFile), _digitPressed);
        if (!_userAgent.IsCallActive)
        {
            // break area for testing
            _logger.LogDebug("{method}() - _userAgent is inactive, throwing HangupException", nameof(PlayFile));
            CloseDialResources();
            throw new HangupException();
        }
        if (_digitPressed) return;
        await PlayWavStreamInternalAsync(audioStream);
        await Task.Delay(300, cancellationToken);
        if (!_userAgent.IsCallActive)
        {
            _logger.LogDebug("{method}() - _userAgent is inactive, throwing HangupException", nameof(PlayFile));
            // break area for testing
            CloseDialResources();
            throw new HangupException();
        }
    }

    private async Task PlayWavStreamInternalAsync(Stream audioStream)
    {
        if (_voipMediaSession == null) return;
        await _voipMediaSession.AudioExtrasSource.SendAudioFromStream(audioStream,
            AudioSamplingRatesEnum.Rate8KHz);
    }

    public void RecordToFile(string filename)
    {
        RecordToFileAsync(filename, 0, CancellationToken.None).GetAwaiter().GetResult();
    }
    
    public void RecordToFile(string filename, int timeoutMilliseconds)
    {
        RecordToFileAsync(filename, timeoutMilliseconds, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task RecordToFileAsync(string filename, CancellationToken cancellationToken)
    {
        await RecordToFileAsync(filename, 0, cancellationToken);
    }

    public async Task RecordToFileAsync(string filename, int timeoutMilliseconds, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({fileName}, {timeout})", nameof(RecordToFileAsync), filename, timeoutMilliseconds);
        FlushDigitBuffer();

        if (timeoutMilliseconds == 0) timeoutMilliseconds = 60000 * 5; // set to 5 minutes

        try
        {
            await PlayFileAsync($"{ROOT}beep.wav", cancellationToken);

            var wavFormat = new WaveFormat(8000, 16, 1);
    
            _waveFile = new WaveFileWriter(filename, wavFormat);

            if (_voipMediaSession != null)
            {
                _voipMediaSession.OnRtpPacketReceived += OnVoipMediaSessionOnOnRtpPacketReceived;
                await GetDigitsAsync(1, "0123456789*#", cancellationToken, timeoutMilliseconds);

                if (_voipMediaSession != null)
                    _voipMediaSession.OnRtpPacketReceived -= OnVoipMediaSessionOnOnRtpPacketReceived;
            }

        }
        finally
        {
            _waveFile?.Close();
            _waveFile = null;
        }
    }

    private void OnVoipMediaSessionOnOnRtpPacketReceived(IPEndPoint remoteEndPoint, SDPMediaTypesEnum mediaType, RTPPacket rtpPacket)
    {
        if (mediaType == SDPMediaTypesEnum.audio)
        {
            var sample = rtpPacket.Payload;

            foreach (var aByte in sample)
            {
                if (rtpPacket.Header.PayloadType == (int)SDPWellKnownMediaFormatsEnum.PCMA)
                {
                    short pcm = NAudio.Codecs.ALawDecoder.ALawToLinearSample(aByte);
                    byte[] pcmSample = [(byte)(pcm & 0xFF), (byte)(pcm >> 8)];
                    _waveFile?.Write(pcmSample, 0, 2);
                }
                else
                {
                    short pcm = NAudio.Codecs.MuLawDecoder.MuLawToLinearSample(aByte);
                    byte[] pcmSample = [(byte)(pcm & 0xFF), (byte)(pcm >> 8)];
                    _waveFile?.Write(pcmSample, 0, 2);
                }
            }
        }
    }

    public void Reset()
    {
        // todo - should implement this
    }

    public void TakeOffHook()
    {
        _logger.LogDebug("{method}()", nameof(TakeOffHook));
    }

    public void TriggerDispose()
    {
        _logger.LogDebug("{method} for line: {lineNumber}", nameof(TriggerDispose), _lineNumber);
        Dispose();
    }

    /// <summary>
    /// Exists for legacy purposes. Use <see cref="IIvrLine.StartIncomingListener"/> instead
    /// </summary>
    /// <param name="rings">Not used</param>
    public void WaitRings(int rings)
    {
        ResetLine();
        _logger.LogDebug("{method}({rings})", nameof(WaitRings), rings);
        _incomingSemaphore.Setup();
        
        _userAgent.OnIncomingCall += OnUserAgentOnOnIncomingCall;
        _incomingSemaphore.Wait();
        _userAgent.OnIncomingCall -= OnUserAgentOnOnIncomingCall;
        _incomingSemaphore.Teardown();
    }

    /// <summary>
    /// Exists for legacy purposes. Use <see cref="IIvrLine.StartIncomingListener"/> instead
    /// </summary>
    /// <param name="rings">Not used</param>
    /// <param name="cancellationToken"></param>
    public async Task WaitRingsAsync(int rings, CancellationToken cancellationToken)
    {
        ResetLine();
        _logger.LogDebug("{method}({rings})", nameof(WaitRingsAsync), rings);
        _incomingSemaphore.Setup();
        
        _userAgent.OnIncomingCall += OnUserAgentOnOnIncomingCall;
        await _incomingSemaphore.WaitAsync(cancellationToken);
        _userAgent.OnIncomingCall -= OnUserAgentOnOnIncomingCall;
        _incomingSemaphore.Teardown();
    }

    private void OnUserAgentOnOnIncomingCall(SIPUserAgent ua, SIPRequest request)
    {
        _logger.LogDebug("Incoming call from {remoteSIPEndPoint}.", request.RemoteSIPEndPoint);


        if (!request.StatusLine.Contains($"sip:{_voiceProperties.SipUsername}@"))
        {
            _logger.LogError("We have should not be getting: " + request.StatusLine);
            ua.Cancel();
            return;
        }

        if (!_inviteManager.IsAvailable(request))
        {
            _logger.LogDebug("We have already handled this INVITE");
            ua.Cancel();
            return;
        }

        ResetLine();

        _logger.LogDebug("About to accept the call");
        var uas = _userAgent.AcceptCall(request);

        _logger.LogDebug(request.StatusLine); // INVITE sip:201@192.168.3.193:5060 SIP/2.0

        _voipMediaSession = new VoIPMediaSession();
        _voipMediaSession.AcceptRtpFromAny = true;
        _voipMediaSession.TakeOffHold();

        _voipMediaSession.AudioExtrasSource.AudioSamplePeriodMilliseconds = 20;
        _voipMediaSession.AudioExtrasSource.SetAudioSourceFormat(new AudioFormat(SDPWellKnownMediaFormatsEnum.PCMU));

        _voipMediaSession.AudioExtrasSource.StartAudio().GetAwaiter().GetResult();

        Task.Delay(2000); // I want to hear a ring
        ua.Answer(uas, _voipMediaSession).GetAwaiter().GetResult();
        
        _userAgent.OnIncomingCall -= OnUserAgentOnOnIncomingCall;
        _incomingSemaphore.Release();
    }
    
    void IIvrBaseLine.StartIncomingListener(Func<IIvrLine, CancellationToken, Task> callback, IIvrLine line, CancellationToken cancellationToken)
    {
        _logger.LogDebug("StartIncomingListener({lineNo})", line.LineNumber);

        _userAgent.OnIncomingCall += async (ua, request) =>
        {
            _logger.LogDebug("Line: {lineNo}, Incoming call from {remoteSIPEndPoint}.", line.LineNumber, request.RemoteSIPEndPoint);


            if (!request.StatusLine.Contains($"sip:{_voiceProperties.SipUsername}@"))
            {
                _logger.LogError("We have should not be getting: " + request.StatusLine);
                ua.Cancel();
                return;
            }
            
            
            if (!_inviteManager.IsAvailable(request))
            {
                _logger.LogDebug("We have already handled this INVITE");
                ua.Cancel();
                return;
            }
            
            ResetLine();

            _logger.LogDebug("About to accept the call");
            var uas = _userAgent.AcceptCall(request);
            
            _logger.LogDebug(request.StatusLine); // INVITE sip:201@192.168.3.193:5060 SIP/2.0
            
            _voipMediaSession = new VoIPMediaSession();
            _voipMediaSession.AcceptRtpFromAny = true;
            _voipMediaSession.TakeOffHold();

            _voipMediaSession.AudioExtrasSource.AudioSamplePeriodMilliseconds = 20;
            _voipMediaSession.AudioExtrasSource.SetAudioSourceFormat(new AudioFormat(SDPWellKnownMediaFormatsEnum.PCMU));

            await _voipMediaSession.AudioExtrasSource.StartAudio();

            await Task.Delay(2000, cancellationToken); // I want to hear a ring
            await ua.Answer(uas, _voipMediaSession);
            
            // execute the callback
            await callback(line, cancellationToken);
        };
    }
    
}