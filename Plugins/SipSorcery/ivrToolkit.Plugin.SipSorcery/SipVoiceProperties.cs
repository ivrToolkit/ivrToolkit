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
    
    private const string SIP_SERVER = "sip.server";
    
    private const string SIP_USERNAME = "sip.username";
    
    private const string SIP_LOCAL_ENDPOINT = "sip.localEndpoint";
    private const string SIP_LOCAL_ENDPOINT_DEFAULT = "0.0.0.0:5060";

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
        init => SetProperty(DEBUG_SIP_TRANSPORT_ENABLE_TRACE_LOGS_KEY, value.ToString());
    }

    /// <summary>
    /// The SIP port used for SIP signaling
    /// </summary>
    [Obsolete("Now combined with SipServer")]
    public ushort SipSignalingPort
    {
        get => ushort.Parse(GetProperty(SIP_SIGNALING_PORT_KEY, SIP_SIGNALING_PORT_DEFAULT));
        init => SetProperty(SIP_SIGNALING_PORT_KEY, value.ToString());
    }

    /// <summary>
    /// The SIP proxy IP address. This is the address of the PBX that will be used to connect to the SIP Trunk.
    /// </summary>
    [Obsolete("Now combined with SipServer")]
    public string SipProxyIp
    {
        get => GetProperty(SIP_PROXY_IP_KEY, SIP_PROXY_IP_DEFAULT);
        init => SetProperty(SIP_PROXY_IP_KEY, value);
    }

    /// <summary>
    /// The SIP account on the PBX server. This is the account that will be used to make and receive calls for this ADS SIP instance.
    /// </summary>
    [Obsolete("Use SipUsername instead")]
    public string SipAlias
    {
        get => GetProperty(SIP_ALIAS_KEY, SIP_ALIAS_DEFAULT);
        init => SetProperty(SIP_ALIAS_KEY, value);
    }

    /// <summary>
    /// The SIP password for the SipAlias on the PBX server. 
    /// </summary>
    public string SipPassword
    {
        get => GetProperty(SIP_PASSWORD_KEY, SIP_PASSWORD_DEFAULT);
        init => SetProperty(SIP_PASSWORD_KEY, value);
    }
    
    /// <summary>
    /// This is the address of the PBX that will be used to connect to the SIP Trunk.
    /// Format can be IpAddress[:port]
    /// You do not need to specify a port. The default is 5060
    /// </summary>
    public string SipServer
    {
        get
        {
            var result = GetProperty(SIP_SERVER, "");
            if (string.IsNullOrWhiteSpace(result))
            {
                result = $"{SipProxyIp}:{SipSignalingPort}";
            }
            else
            {
                if (result.StartsWith("sip:", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Substring(4); // strip off sip: if there is one.
                }
                if (!result.Contains(":"))
                {
                    result += ":5060";
                }
            }
            return result;
        }
        init => SetProperty(SIP_SERVER, value);
    }

    ///
    /// <summary>
    /// The username for registry. In FreePBX it is the extension.
    /// </summary>
    public string SipUsername
    {
        get => GetProperty(SIP_USERNAME, SipAlias);
        init => SetProperty(SIP_USERNAME, value);
    }
    
    /// <summary>
    /// The endpoint for the sip transport. Default is 0.0.0.0:5060.
    /// Format is IpAddress[:port]
    /// You do no need to specify a port. The default is 5060. This port can be any available port on your
    /// local computer. You can specify 0 to use a dynamic port.
    /// It can be quicker to use your actual IP rather than 0.0.0.0
    /// </summary>
    public string SipLocalEndpoint
    {
        get
        {
            var result = GetProperty(SIP_LOCAL_ENDPOINT, SIP_LOCAL_ENDPOINT_DEFAULT);
            if (!result.Contains(":"))
            {
                result += ":5060";
            }
            return result;
        }
        init => SetProperty(SIP_SERVER, value);
    }
    

    public new void Dispose()
    {
        _logger.LogDebug("{method}()", nameof(Dispose));
        base.Dispose();
    }
}