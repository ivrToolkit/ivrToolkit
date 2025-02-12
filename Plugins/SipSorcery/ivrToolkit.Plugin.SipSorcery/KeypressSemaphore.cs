using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.SipSorcery;

public class KeypressSemaphore
{
    private readonly ILogger<KeypressSemaphore> _logger;
    private SemaphoreSlim? _semaphoreSlim;
    private CancellationTokenSource? _cts;
    private int? _maxDigits;
    private string? _terminators;

    private string _buffer = string.Empty;

    public KeypressSemaphore(ILoggerFactory loggerFactory)
    {
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<KeypressSemaphore>();
        _logger.LogDebug("{method}()", nameof(KeypressSemaphore));
    }

    public void CheckDigits(string buffer)
    {
        _logger.LogDebug("{method}({buffer}) - semaphoreSlim setup = {setup}", nameof(CheckDigits), buffer, _semaphoreSlim != null);
        if (_semaphoreSlim == null || _terminators == null) return;

        _buffer = buffer;
        // this method happens on every keypress. I now want to release on every keypress.
        _semaphoreSlim.Release();
    }

    public string WaitForDigits(int milliseconds)
    {
        if (_semaphoreSlim == null || _terminators == null)
        {
            throw new VoiceException("Unlikely to ever happen. _semaphoreSlim or _terminators are null.");
        }

        return WaitForDigitsInternalAsync(milliseconds,
            (ms, tks) =>
            {
                var result = _semaphoreSlim.Wait(ms, tks);
                return Task.FromResult(result);
            }, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task<string> WaitForDigitsAsync(int milliseconds, CancellationToken cancellationToken)
    {
        if (_semaphoreSlim == null|| _terminators == null)
        {
            throw new VoiceException("Unlikely to ever happen. _semaphoreSlim or _terminators are null.");
        }

        return await WaitForDigitsInternalAsync(milliseconds,
            async (ms, tks) => await _semaphoreSlim.WaitAsync(ms, tks), cancellationToken);
    }

    private async Task<string> WaitForDigitsInternalAsync(int milliseconds,
        Func<int, CancellationToken, Task<bool>> waitFunc,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({milliseconds})", nameof(WaitForDigitsInternalAsync), milliseconds);

        if (_semaphoreSlim == null|| _terminators == null || _cts == null)
        {
            throw new VoiceException("Unlikely to ever happen. _semaphoreSlim, _terminators or _cts are null.");
        }
        
        // every time a key is pressed the semaphore is released so the time can restart
        while (!_cts.IsCancellationRequested)
        {
            var acquired = await waitFunc(milliseconds, _cts.Token);
            if (acquired)
            {
                // detect max digits or a terminator
                var count = 0;
                var span = _buffer.ToArray();
                foreach (var c in span)
                {
                    count++;
                    if (count == _maxDigits || _terminators.Contains(c))
                    {
                        _logger.LogDebug("{method}({buffer}) - wakey wakey!", nameof(WaitForDigitsInternalAsync), _buffer);
                        return _buffer;
                    }
                }
                // did not find a terminator and did not hit the number of digits required
                _logger.LogDebug("{method}({buffer}) - did not find a terminator and did not hit the number of digits required", nameof(WaitForDigitsInternalAsync), _buffer);
            }
            else
            {
                // timed out
                _logger.LogDebug("{method}({buffer}) - inter-digit timeout", nameof(WaitForDigitsInternalAsync), _buffer);
                if (_terminators.Contains('t'))
                {
                    // inter digit timeout acts like a terminator
                    _buffer += 't';
                    return _buffer;
                }
                throw new GetDigitsTimeoutException();
            }
        }

        throw new OperationCanceledException();
    }
    
    public void Setup(int maxDigits, string terminators)
    {
        _logger.LogDebug("{method}({max}, {terminators})", nameof(Setup), maxDigits, terminators);
        _semaphoreSlim?.Dispose();
        _semaphoreSlim = new SemaphoreSlim(0, 1);
        _cts = new CancellationTokenSource();
        _maxDigits = maxDigits;
        _terminators = terminators;
    }

    public void Teardown()
    {
        _logger.LogDebug("{method}()", nameof(Teardown));
        _cts?.Cancel(); // signal the cancellation
        _semaphoreSlim = null;

        _maxDigits = null;
        _terminators = null;
    }
}