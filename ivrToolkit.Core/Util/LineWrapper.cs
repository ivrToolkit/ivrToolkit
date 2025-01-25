using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace ivrToolkit.Core.Util;

/// <summary>
/// This wrapper handles common functionality in all plugin lines so that the implementation doesn't have to handle it.
/// </summary>
internal partial class LineWrapper : IIvrLine, IIvrLineManagement
{

    private readonly int _lineNumber;
    private readonly IIvrBaseLine _lineImplementation;

    private readonly ILogger<LineWrapper> _logger;


    private LineStatusTypes _status = LineStatusTypes.OnHook;

    private bool _disposeTriggerActivated;
    private int _volume;
    private bool _disposed;
    private readonly VoiceProperties _voiceProperties;


    internal LineWrapper(ILoggerFactory loggerFactory, VoiceProperties voiceProperties, int lineNumber, IIvrBaseLine lineImplementation)
    {
        _lineNumber = lineNumber;
        _lineImplementation = lineImplementation.ThrowIfNull(nameof(lineImplementation));
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _voiceProperties = voiceProperties.ThrowIfNull(nameof(voiceProperties));
            
        _logger = loggerFactory.CreateLogger<LineWrapper>();
        _logger.LogDebug("ctr(ILoggerFactory, VoiceProperties, {lineNumber})", lineNumber);
    }

    public IIvrLineManagement Management => this;

    public LineStatusTypes Status => _status;

    public string LastTerminator
    {
        get => _lineImplementation.LastTerminator;
        set => _lineImplementation.LastTerminator = value;
    }

    public int LineNumber => _lineNumber;

    public void CheckDispose()
    {
        _logger.LogTrace("CheckDispose()");
        CheckDisposed();
        CheckDisposing();
    }
    
    public void WaitRings(int rings)
    {
        _logger.LogDebug("{method}({rings})", nameof(WaitRings), rings);
        CheckDispose();

        _status = LineStatusTypes.AcceptingCalls;

        _lineImplementation.WaitRings(rings);

        _status = LineStatusTypes.Connected;
        CheckDisposing();
    }

    public async Task WaitRingsAsync(int rings, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({rings})", nameof(WaitRings), rings);
        CheckDispose();

        _status = LineStatusTypes.AcceptingCalls;

        await _lineImplementation.WaitRingsAsync(rings, cancellationToken);

        _status = LineStatusTypes.Connected;
        CheckDisposing();
    }

    public void StartIncomingListener(Func<IIvrLine, CancellationToken, Task> callback, CancellationToken cancellationToken)
    {
        _lineImplementation.StartIncomingListener(callback, this, cancellationToken);
    }

    void IIvrBaseLine.StartIncomingListener(Func<IIvrLine, CancellationToken, Task> callback, IIvrLine line, CancellationToken cancellationToken)
    {
        _lineImplementation.StartIncomingListener(callback, line, cancellationToken);
    }

    public void Hangup()
    {
        _logger.LogDebug("{method}()",nameof(Hangup));
        _status = LineStatusTypes.OnHook;
        CheckDispose();

        _lineImplementation.Hangup();
    }

    public void TakeOffHook()
    {
        _logger.LogDebug("{method}()", nameof(TakeOffHook));
        _status = LineStatusTypes.OffHook;
        CheckDispose();

        _lineImplementation.TakeOffHook();
    }


    public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
    {
        answeringMachineLengthInMilliseconds.ThrowIfLessThanOrEqualTo(999, nameof(answeringMachineLengthInMilliseconds));
        number.ThrowIfNullOrWhiteSpace(nameof(number));

        _logger.LogDebug("{method}({number}, {answeringMachineLength})", nameof(Dial), number, answeringMachineLengthInMilliseconds);
        CheckDispose();

        TakeOffHook();
        _logger.LogDebug("Line is now off hook");

        _logger.LogDebug("about to dial: {number}", number);
        var result = _lineImplementation.Dial(number, answeringMachineLengthInMilliseconds);

        _logger.LogDebug("CallAnalysis is: {callAnalysis}", result.ToString());

        if (result == CallAnalysis.Stopped) ThrowDisposingException();

        if (result == CallAnalysis.AnsweringMachine || result == CallAnalysis.Connected)
        {
            _status = LineStatusTypes.Connected;
        }
        else
        {
            Hangup();
        }

        return result;
    }

    public async Task<CallAnalysis> DialAsync(string number, int answeringMachineLengthInMilliseconds, CancellationToken cancellationToken)
    {
        answeringMachineLengthInMilliseconds.ThrowIfLessThanOrEqualTo(999, nameof(answeringMachineLengthInMilliseconds));
        number.ThrowIfNullOrWhiteSpace(nameof(number));

        _logger.LogDebug("{method}({number}, {answeringMachineLength})", nameof(DialAsync), number, answeringMachineLengthInMilliseconds);
        CheckDispose();

        TakeOffHook();
        _logger.LogDebug("Line is now off hook");

        _logger.LogDebug("about to dial: {number}", number);
        var result = await _lineImplementation.DialAsync(number, answeringMachineLengthInMilliseconds, cancellationToken);

        _logger.LogDebug("CallAnalysis is: {callAnalysis}", result.ToString());

        if (result == CallAnalysis.Stopped) ThrowDisposingException();

        if (result == CallAnalysis.AnsweringMachine || result == CallAnalysis.Connected)
        {
            _status = LineStatusTypes.Connected;
        }
        else
        {
            Hangup();
        }

        return result;
    }
        
        
    #region ILineManagement region

    void IIvrLineManagement.TriggerDispose()
    {
        _logger.LogDebug("{method} for line: {lineNumber}", nameof(IIvrLineManagement.TriggerDispose), _lineNumber);
        if (_disposed)
        {
            _logger.LogDebug("Line {lineNumber} has already been disposed", _lineNumber);
            return;
        }

        _lineImplementation.Management.TriggerDispose();
        _disposeTriggerActivated = true;
    }

    #endregion

    public void Dispose()
    {
        if (_disposed)
        {
            _logger.LogDebug("{method}() - Line is already disposed", nameof(Dispose));
            return;
        }
        _logger.LogDebug("{method}() - Disposing of the line", nameof(Dispose));

        try
        {
            _status = LineStatusTypes.OnHook;
            _lineImplementation.Hangup();
            _lineImplementation.Dispose();
        }
        finally
        {
            _disposed = true;
            _disposeTriggerActivated = false;
        }
    }

    public void PlayFile(string filename)
    {
        _logger.LogDebug("{method}({filename})", nameof(PlayFile), filename);
        CheckDispose();
        try
        {
            _lineImplementation.PlayFile(filename);
        }
        catch (DisposingException)
        {
            ThrowDisposingException();
        }
        catch (HangupException)
        {
            _status = LineStatusTypes.OnHook;
            throw;
        }
    }

    public async Task PlayFileAsync(string filename, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({filename})", nameof(PlayFileAsync), filename);
        CheckDispose();
        try
        {
            await _lineImplementation.PlayFileAsync(filename, cancellationToken);
        }
        catch (DisposingException)
        {
            ThrowDisposingException();
        }
        catch (HangupException)
        {
            _status = LineStatusTypes.OnHook;
            throw;
        }
    }
        
    public void RecordToFile(string filename)
    {
        RecordToFile(filename, 60000 * 5); // default timeout of 5 minutes
    }
    
    public void RecordToFile(string filename, int timeoutMilliseconds)
    {
        RecordToFileInternalAsync(filename, timeoutMilliseconds,
            (fileN, timeout) =>
            {
                _lineImplementation.RecordToFile(fileN, timeout);
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
    }

    public async Task RecordToFileAsync(string filename, CancellationToken cancellationToken)
    {
        await RecordToFileAsync(filename, 60000 * 5, cancellationToken); // default timeout of 5 minutes
    }

    public async Task RecordToFileAsync(string filename, int timeoutMilliseconds, CancellationToken cancellationToken)
    {
        await RecordToFileInternalAsync(filename, timeoutMilliseconds,
            async (fileN, timeout) => await _lineImplementation.RecordToFileAsync(fileN, timeout, cancellationToken));
    }

    private async Task RecordToFileInternalAsync(string filename, int timeoutMilliseconds, 
        Func<string, int, Task> recordToFileAsyncFunc)
    {
        _logger.LogDebug("{method}({filename}, {timeout})", nameof(RecordToFile), filename, timeoutMilliseconds);
        CheckDispose();

        try
        {
            await recordToFileAsyncFunc(filename, timeoutMilliseconds);
        }
        catch (DisposingException)
        {
            ThrowDisposingException();
        }
        catch (HangupException)
        {
            _status = LineStatusTypes.OnHook;
            throw;
        }
    }


    public string GetDigits(int numberOfDigits, string terminators, int timeoutMilliseconds = 0)
    {
        _logger.LogDebug("{method}({numberOfDigits}, {terminators})", nameof(GetDigits), numberOfDigits, terminators);
        CheckDispose();
        try
        {
            var answer = _lineImplementation.GetDigits(numberOfDigits, terminators, timeoutMilliseconds);
            return StripOffTerminator(answer, terminators);
        }
        catch (DisposingException)
        {
            ThrowDisposingException();
        }
        catch (HangupException)
        {
            _status = LineStatusTypes.OnHook;
            throw;
        }

        return null; // will never get here
    }

    public async Task<string> GetDigitsAsync(int numberOfDigits, string terminators, CancellationToken cancellationToken, int timeoutMilliseconds = 0)
    {
        _logger.LogDebug("{method}({numberOfDigits}, {terminators})", nameof(GetDigitsAsync), numberOfDigits, terminators);
        CheckDispose();
        try
        {
            var answer = await _lineImplementation.GetDigitsAsync(numberOfDigits, terminators, cancellationToken, timeoutMilliseconds);
            return StripOffTerminator(answer, terminators);
        }
        catch (DisposingException)
        {
            ThrowDisposingException();
        }
        catch (HangupException)
        {
            _status = LineStatusTypes.OnHook;
            throw;
        }

        return null; // will never get here
    }

    public string FlushDigitBuffer()
    {
        _logger.LogDebug("{method}()", nameof(FlushDigitBuffer));
        CheckDispose();

        var all = "";
        try
        {
            _lineImplementation.FlushDigitBuffer();
        }
        catch (GetDigitsTimeoutException)
        {
        }

        return all;
    }

    public int Volume
    {
        get
        {
            CheckDispose();
            return _volume;
        }
        set
        {
            if (value < -10 || value > 10)
            {
                throw new VoiceException("size must be between -10 to 10");
            }

            CheckDispose();

            _lineImplementation.Volume = value;
            _volume = value;
        }
    }

    private string StripOffTerminator(string answer, string terminators)
    {
        _logger.LogDebug("{method}({answer}, {terminators})", nameof(StripOffTerminator), answer, terminators);

        LastTerminator = "";
        if (answer.Length >= 1)
        {
            var lastDigit = answer.Substring(answer.Length - 1, 1);
            if (terminators != null & terminators != "")
            {
                if (terminators.IndexOf(lastDigit, StringComparison.Ordinal) != -1)
                {
                    LastTerminator = lastDigit;
                    answer = answer.Substring(0, answer.Length - 1);
                }
            }
        }

        return answer;
    }

    private void CheckDisposed()
    {
        if (_disposed) ThrowDisposedException();
    }

    private void CheckDisposing()
    {
        if (_disposeTriggerActivated) ThrowDisposingException();
    }

    private void ThrowDisposingException()
    {
        _logger.LogDebug("{method}()", nameof(ThrowDisposingException));
        _disposeTriggerActivated = false;
        throw new DisposingException();
    }

    private void ThrowDisposedException()
    {
        _logger.LogDebug("{method}()", nameof(ThrowDisposedException));
        throw new DisposedException($"Line {_lineNumber} has already been disposed");
    }

    public void Reset()
    {
        if (_disposed)
        {
            _logger.LogDebug("{method}() - Line is already disposed", nameof(Reset));
            return;
        }
        _logger.LogDebug("{method}() - Disposing and recreate the line from scratch", nameof(Reset));
        _lineImplementation.Reset();
    }
}
