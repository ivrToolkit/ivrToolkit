using ivrToolkit.Core.Extensions;
using Microsoft.Extensions.Logging;
using SIPSorcery.SIP;

namespace ivrToolkit.Plugin.SipSorcery;

class InviteManager
{
    private readonly ILogger<InviteManager> _logger;
    private readonly object _lock = new();
    private readonly Dictionary<string, string> _callIds = new();

    public InviteManager(ILoggerFactory loggerFactory)
    {
        loggerFactory.ThrowIfNull(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<InviteManager>();
    }

    public bool IsAvailable(SIPRequest sipRequest)
    {
        _logger.LogDebug("{method}() - callId = {callId}", nameof(IsAvailable), sipRequest.Header.CallId);
        lock (_lock)
        {
            var callId = sipRequest.Header.CallId;
            return _callIds.TryAdd(callId, callId);
        }
    }
}