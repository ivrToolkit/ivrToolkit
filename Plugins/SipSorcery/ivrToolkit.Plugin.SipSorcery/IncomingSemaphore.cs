using ivrToolkit.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.SipSorcery;

public class IncomingSemaphore
{
    private readonly ILogger<IncomingSemaphore> _logger;
    private SemaphoreSlim? _semaphoreSlim;
    private CancellationTokenSource _cts = null!;

    public IncomingSemaphore(ILoggerFactory loggerFactory)
    {
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<IncomingSemaphore>();
        _logger.LogDebug("{method}()", nameof(IncomingSemaphore));
    }

    public void Release()
    {
        _logger.LogDebug("{method}() - semaphoreSlim setup = {setup}", nameof(Release),
            _semaphoreSlim != null);
        if (_semaphoreSlim == null) return;

        _logger.LogDebug("{method}() - wakey wakey!", nameof(Release));
        _semaphoreSlim.Release();
    }

public void Wait()
    {
        _logger.LogDebug("{method}()", nameof(Wait));
        try
        {
            _semaphoreSlim?.Wait(_cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }
        
    public async Task<bool> WaitAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}()", nameof(WaitAsync));
        try
        {
            if (_semaphoreSlim != null)
            {
                await _semaphoreSlim.WaitAsync(_cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        return true;
    }

    public void Setup()
    {
        _logger.LogDebug("{method}()", nameof(Setup));
        _semaphoreSlim?.Dispose();
        _semaphoreSlim = new SemaphoreSlim(0, 1);
        _cts = new CancellationTokenSource();
    }

    public void Teardown()
    {
        _logger.LogDebug("{method}()", nameof(Teardown));
        _cts?.Cancel(); // signal the cancellation
        _semaphoreSlim = null;
    }
}