using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.SipSorcery;

public class SipVoiceProperties : VoiceProperties, IDisposable
{
    private readonly ILogger _logger;

    // Constants for property keys
    private const string DEBUG_SIP_TRANSPORT_ENABLE_TRACE_LOGS_KEY = "debug.sipTransport.enableTraceLogs";
    private const string DEBUG_SIP_TRANSPORT_ENABLE_TRACE_LOGS_DEFAULT = "true";

    private const string SIP_SIGNALING_PORT_KEY = "sip.sip_signaling_port";
    private const string SIP_SIGNALING_PORT_DEFAULT = "5060";

    private const string SIP_PROXY_IP_KEY = "sip.proxy_ip";
    private const string SIP_PROXY_IP_DEFAULT = "";

    private const string SIP_ALIAS_KEY = "sip.alias";
    private const string SIP_ALIAS_DEFAULT = "";

    private const string SIP_PASSWORD_KEY = "sip.password";
    private const string SIP_PASSWORD_DEFAULT = "";

    public SipVoiceProperties(ILoggerFactory loggerFactory, string fileName) : base(loggerFactory, fileName)
    {
        _logger = loggerFactory.CreateLogger<SipVoiceProperties>();
        _logger.LogDebug("Ctr(ILoggerFactory loggerFactory, {fileName})", fileName);
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
        get => bool.Parse(GetProperty(DEBUG_SIP_TRANSPORT_ENABLE_TRACE_LOGS_KEY, DEBUG_SIP_TRANSPORT_ENABLE_TRACE_LOGS_DEFAULT));
        set => SetProperty(DEBUG_SIP_TRANSPORT_ENABLE_TRACE_LOGS_KEY, value.ToString());
    }

    /// <summary>
    /// The SIP port used for SIP signaling
    /// </summary>
    public ushort SipSignalingPort
    {
        get => ushort.Parse(GetProperty(SIP_SIGNALING_PORT_KEY, SIP_SIGNALING_PORT_DEFAULT));
        set => SetProperty(SIP_SIGNALING_PORT_KEY, value.ToString());
    }

    /// <summary>
    /// The SIP proxy IP address. This is the address of the PBX that will be used to connect to the SIP Trunk.
    /// </summary>
    public string SipProxyIp
    {
        get => GetProperty(SIP_PROXY_IP_KEY, SIP_PROXY_IP_DEFAULT);
        set => SetProperty(SIP_PROXY_IP_KEY, value);
    }

    /// <summary>
    /// The SIP account on the PBX server. This is the account that will be used to make and receive calls for this ADS SIP instance.
    /// </summary>
    public string SipAlias
    {
        get => GetProperty(SIP_ALIAS_KEY, SIP_ALIAS_DEFAULT);
        set => SetProperty(SIP_ALIAS_KEY, value);
    }

    /// <summary>
    /// The SIP password for the SipAlias on the PBX server. 
    /// </summary>
    public string SipPassword
    {
        get => GetProperty(SIP_PASSWORD_KEY, SIP_PASSWORD_DEFAULT);
        set => SetProperty(SIP_PASSWORD_KEY, value);
    }

    public new void Dispose()
    {
        _logger.LogDebug("{method}()", nameof(Dispose));
        base.Dispose();
    }
}