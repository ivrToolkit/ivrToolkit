using ivrToolkit.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.SipSorcery
{
    public class KeypressSemaphore
    {
        private readonly ILogger<KeypressSemaphore> _logger;
        private SemaphoreSlim? _semaphoreSlim;
        private CancellationTokenSource _cts;
        private int? _maxDigits;
        private string? _terminators;

        public KeypressSemaphore(ILoggerFactory loggerFactory)
        {
            loggerFactory.ThrowIfNull(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<KeypressSemaphore>();
            _logger.LogDebug("{name}()", nameof(KeypressSemaphore));
        }

        public void ReleaseMaybe(string buffer)
        {
            _logger.LogDebug("{name}({buffer}) - semaphoreSlim setup = {setup}", nameof(ReleaseMaybe), buffer, _semaphoreSlim != null);
            if (_semaphoreSlim == null || _terminators == null) return;

            var count = 0;
            ReadOnlySpan<char> span = buffer.AsSpan();
            foreach (char c in span)
            {
                count++;
                if (count == _maxDigits || _terminators.Contains(c))
                {
                    _logger.LogDebug("{name}({buffer}) - wakey wakey!", nameof(ReleaseMaybe), buffer);
                    _semaphoreSlim.Release();
                    return;
                }
            }
            // did not find a terminator and did not hit the number of digits required
            _logger.LogDebug("{name}({buffer}) - did not find a terminator and did not hit the number of digits required", nameof(ReleaseMaybe), buffer);
        }

        public bool Wait(int milliseconds)
        {
            _logger.LogDebug("{name}({buffer})", nameof(Wait), milliseconds);
            try
            {
                _semaphoreSlim?.Wait(milliseconds, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            return true;
        }
        
        public async Task<bool> WaitAsync(int milliseconds, CancellationToken cancellationToken)
        {
            _logger.LogDebug("{name}({buffer})", nameof(Wait), milliseconds);
            try
            {
                if (_semaphoreSlim != null)
                {
                    await _semaphoreSlim.WaitAsync(milliseconds, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            return true;
        }

        public void Setup(int maxDigits, string terminators)
        {
            _logger.LogDebug("{name}({max}, {terminators})", nameof(Setup), maxDigits, terminators);
            _semaphoreSlim?.Dispose();
            _semaphoreSlim = new SemaphoreSlim(0, 1);
            _cts = new CancellationTokenSource();
            _maxDigits = maxDigits;
            _terminators = terminators;
        }

        public void Teardown()
        {
            _logger.LogDebug("{name}()", nameof(Teardown));
            _cts?.Cancel(); // signal the cancellation
            _semaphoreSlim?.Dispose();
            _semaphoreSlim = null;

            _maxDigits = null;
            _terminators = null;
        }
    }
}
