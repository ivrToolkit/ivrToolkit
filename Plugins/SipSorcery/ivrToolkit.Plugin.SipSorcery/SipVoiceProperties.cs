using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.SipSorcery;

public class SipVoiceProperties : VoiceProperties, IDisposable
{
    private readonly ILogger _logger;

    // Constants for property keys
    private const string DEBUG_SIP_TRANSPORT_ENABLE_TRACE_LOGS_KEY = "debug.sipTransport.enableTraceLogs";
    private const string DEBUG_SIP_TRANSPORT_ENABLE_TRACE_LOGS_DEFAULT = "true";

    // legacy
    private const string SIP_SIGNALING_PORT_KEY = "sip.sip_signaling_port";
    private const string SIP_ALIAS_KEY = "sip.alias";
    private const string SIP_PROXY_IP_KEY = "sip.proxy_ip";
    private const string SIP_LOCAL_IP_KEY = "sip.local_ip";


    private const string SIP_PASSWORD_KEY = "sip.password";
    private const string SIP_SERVER_KEY = "sip.server";
    private const string SIP_USERNAME_KEY = "sip.username";
    private const string SIP_LOCAL_ENDPOINT_KEY = "sip.localEndpoint";

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
    /// The SIP password for the SipAlias on the PBX server. 
    /// </summary>
    public string SipPassword
    {
        get => GetProperty(SIP_PASSWORD_KEY, string.Empty);
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
            // legacy support for Dialogic
            var legacyServerIp = GetProperty(SIP_PROXY_IP_KEY, string.Empty);
            
            var result = GetProperty(SIP_SERVER_KEY, $"{legacyServerIp}:{5060}");
            if (result.StartsWith("sip:", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Substring(4); // strip off sip: if there is one.
            }

            if (!result.Contains(":"))
            {
                result += ":5060";
            }

            return result;
        }
        init => SetProperty(SIP_SERVER_KEY, value);
    }

    ///
    /// <summary>
    /// The username for registry. In FreePBX it is the extension.
    /// </summary>
    public string SipUsername
    {
        get
        {
            var legacyUsername = GetProperty(SIP_ALIAS_KEY, string.Empty);
            return GetProperty(SIP_USERNAME_KEY, legacyUsername);
        }
        // legacy for Dialogic
        init => SetProperty(SIP_USERNAME_KEY, value);
    }

    /// <summary>
    /// The endpoint for the sip transport. Default is 0.0.0.0:5060.
    /// Format is IpAddress[:port]
    /// You do no need to specify a port. The default is 5060. This port can be any available port on your
    /// local computer. You can specify :0 to use a dynamic port.
    /// It can be quicker to use your actual IP rather than 0.0.0.0
    /// </summary>
    public string SipLocalEndpoint
    {
        get
        {
            var legacyLocalIp = GetProperty(SIP_LOCAL_IP_KEY, string.Empty);
            if (string.IsNullOrWhiteSpace(legacyLocalIp))
            {
                legacyLocalIp = "0.0.0.0";
            }
            var legacyLocalPort = GetProperty(SIP_SIGNALING_PORT_KEY, string.Empty);
            if (!string.IsNullOrWhiteSpace(legacyLocalIp))
            {
                legacyLocalPort = ":" + legacyLocalPort;
            }
            
            var result = GetProperty(SIP_LOCAL_ENDPOINT_KEY, $"{legacyLocalIp}{legacyLocalPort}");
            if (!result.Contains(":"))
            {
                result += ":5060";
            }
            return result;
        }
        init => SetProperty(SIP_SERVER_KEY, value);
    }
    

    public new void Dispose()
    {
        _logger.LogDebug("{method}()", nameof(Dispose));
        base.Dispose();
    }
}