using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.SipSorcery;

public class SipVoiceProperties : VoiceProperties, IDisposable
{
    private readonly ILogger _logger;
    public SipVoiceProperties(ILoggerFactory loggerFactory, string fileName) : base(loggerFactory, fileName)
    {
        _logger = loggerFactory.CreateLogger<SipVoiceProperties>();
        _logger.LogDebug("Ctr(ILoggerFactory loggerFactory, {0})", fileName);
    }

    public SipVoiceProperties(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SipVoiceProperties>();
        _logger.LogDebug("Ctr(ILoggerFactory loggerFactory)");
    }

    /// <summary>
    /// Display the Sipsorcery log information in the log. Default is True.
    /// </summary>
    public bool SipTransportEnableTraceLogs
    {
        get => bool.Parse(GetProperty("debug.sipTransport.enableTraceLogs", "true"));
        set => SetProperty("debug.sipTransport.enableTraceLogs", value.ToString());
    }

    /// <summary>
    /// The SIP port used for SIP signaling
    /// </summary>
    public ushort SipSignalingPort
    {
        get => ushort.Parse(GetProperty("sip.sip_signaling_port", "5060"));
        set => SetProperty("sip.sip_signaling_port", value.ToString());
    }

    /// <summary>
    /// The SIP proxy ip address.  This is the address of the PBX that will be used to connect to the SIP Trunk.
    /// </summary>
    public string SipProxyIp
    {
        get => GetProperty("sip.proxy_ip", "");
        set => SetProperty("sip.proxy_ip", value);
    }

    /// <summary>
    /// The SIP account on the PBX server. This is the account that will be used to make and receive calls for this ADS SIP instance.
    /// </summary>
    public string SipAlias
    {
        get => GetProperty("sip.alias", "");
        set => SetProperty("sip.alias", value);
    }

    /// <summary>
    /// The SIP password for the SipAlias on the PBX server. 
    /// </summary>
    public string SipPassword
    {
        get => GetProperty("sip.password", "");
        set => SetProperty("sip.password", value);
    }

    public new void Dispose()
    {
        _logger.LogDebug("Dispose()");
        base.Dispose();
    }
}