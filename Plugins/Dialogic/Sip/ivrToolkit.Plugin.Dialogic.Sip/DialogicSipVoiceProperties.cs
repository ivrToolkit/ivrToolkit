using ivrToolkit.Core.Enums;
using ivrToolkit.Plugin.Dialogic.Common;
using Microsoft.Extensions.Logging;
using System;

namespace ivrToolkit.Plugin.Dialogic.Sip;

public class DialogicSipVoiceProperties : DialogicVoiceProperties, IDisposable
{
    private readonly ILogger _logger;

    // Constants for property keys and defaults
    private const string SIP_CHANNEL_OFFSET_KEY = "sip.channel_offset";
    private const string SIP_CHANNEL_OFFSET_DEFAULT = "0";

    private const string MAX_CALLS_KEY = "sip.max_calls";
    private const string MAX_CALLS_DEFAULT = "1";

    private const string SIP_SIGNALING_PORT_KEY = "sip.sip_signaling_port";
    private const string SIP_SIGNALING_PORT_DEFAULT = "5060";

    private const string SIP_PROXY_IP_KEY = "sip.proxy_ip";
    private const string SIP_PROXY_IP_DEFAULT = "";

    private const string SIP_LOCAL_IP_KEY = "sip.local_ip";
    private const string SIP_LOCAL_IP_DEFAULT = "127.0.0.1";

    private const string SIP_ALIAS_KEY = "sip.alias";
    private const string SIP_ALIAS_DEFAULT = "";

    private const string SIP_PASSWORD_KEY = "sip.password";
    private const string SIP_PASSWORD_DEFAULT = "";

    private const string SIP_REALM_KEY = "sip.realm";
    private const string SIP_REALM_DEFAULT = "";

    private const string SIP_CONTACT_KEY = "sip.contact";
    private const string SIP_CONTACT_DEFAULT = "{alias}@{local_ip}:{sip_signaling_port}";

    private const string SIP_CONNECT_ALERT_HANDLING = "sip.connectedAlertHandling";
    private const string SIP_CONNECT_ALERT_HANDLING_DEFAULT = "NoAnswer";

    public const string SIP_IGNORE_CALL_STATE_CHECK_KEY = "sip.ignoreCallStateCheck";
    private const string SIP_CALL_IGNORE_STATE_CHECK_DEFAULT = "True";
    
    //-----------------------------------------------------------------------------

    public const string ATTEMPT_RECOVERY_START_POSITION = "attemptRecovery.StartPosition";
    private const string ATTEMPT_RECOVERY_START_POSITION_DEFAULT = "DropCall";

    public const string ATTEMPT_RECOVERY_RESETLINEDEV_SUCCESS = "attemptRecovery.ReturnOnResetLineDevSuccess";
    private const string ATTEMPT_RECOVERY_RESETLINEDEV_SUCCESS_DEFAULT = "True";

    public const string ATTEMPT_RECOVERY_TRY_REOPEN_ON = "attemptRecovery.TryReopenOn";
    private const string ATTEMPT_RECOVERY_TRY_REOPEN_ON_DEFAULT = "MakeCall";

    public const string ATTEMPT_RECOVERY_THROW_FAILURE_ON = "attemptRecovery.ThrowFailureOn";
    private const string ATTEMPT_RECOVERY_THROW_FAILURE_ON_DEFAULT = "MakeCall";

    public DialogicSipVoiceProperties(ILoggerFactory loggerFactory, string fileName) : base(loggerFactory, fileName)
    {
        _logger = loggerFactory.CreateLogger<DialogicSipVoiceProperties>();
        _logger.LogDebug("Ctr(ILoggerFactory loggerFactory, {0})", fileName);
    }

    private T GetEnum<T>(string key, string @default) where T : struct, Enum
    {
        var result = GetProperty(key, @default); // Use the method's default parameter
        if (string.IsNullOrWhiteSpace(result))
            result = @default;

        if (Enum.TryParse<T>(result, ignoreCase: true, out var parsedEnum))
        {
            return parsedEnum;
        }

        throw new Exception($"Invalid {key}: {result}");
    }

    public AttemptRecoveryStartPositions AttemptRecoveryStartPosition => 
        GetEnum<AttemptRecoveryStartPositions>(ATTEMPT_RECOVERY_START_POSITION, ATTEMPT_RECOVERY_START_POSITION_DEFAULT);

    public bool AttemptRecoveryReturnOnResetLineDevSuccess => bool.Parse(GetProperty(ATTEMPT_RECOVERY_RESETLINEDEV_SUCCESS, ATTEMPT_RECOVERY_RESETLINEDEV_SUCCESS_DEFAULT));

    public AttemptRecoveryWhen AttemptRecoveryTryReopenOn =>
        GetEnum<AttemptRecoveryWhen>(ATTEMPT_RECOVERY_TRY_REOPEN_ON, ATTEMPT_RECOVERY_TRY_REOPEN_ON_DEFAULT);

    public AttemptRecoveryWhen AttemptRecoveryThrowFailureOn =>
        GetEnum<AttemptRecoveryWhen>(ATTEMPT_RECOVERY_THROW_FAILURE_ON, ATTEMPT_RECOVERY_THROW_FAILURE_ON_DEFAULT);

    /// <summary>
    /// Number to add the line in order to get the channel.
    /// </summary>
    public uint SipChannelOffset => uint.Parse(GetProperty(SIP_CHANNEL_OFFSET_KEY, SIP_CHANNEL_OFFSET_DEFAULT));

    /// <summary>
    /// Checking the call state was recently added to the SIP line but I am starting to worry
    /// that it may be unreliable. Therefore, this property allows you to ignore the state check and
    /// rely on the events to do determine if the line is hung up or not.
    /// 
    /// True to rely on the SIP events to determine the state of the line.
    /// </summary>
    public bool IgnoreCallStateCheck => bool.Parse(GetProperty(SIP_IGNORE_CALL_STATE_CHECK_KEY, SIP_CALL_IGNORE_STATE_CHECK_DEFAULT));

    public ushort MaxCalls => ushort.Parse(GetProperty(MAX_CALLS_KEY, MAX_CALLS_DEFAULT));

    /// <summary>
    /// The SIP port used for SIP signaling
    /// </summary>
    public ushort SipSignalingPort => ushort.Parse(GetProperty(SIP_SIGNALING_PORT_KEY, SIP_SIGNALING_PORT_DEFAULT));

    /// <summary>
    /// The SIP proxy ip address.  This is the address of the PBX that will be used to connect to the SIP Trunk.
    /// </summary>
    public string SipProxyIp => GetProperty(SIP_PROXY_IP_KEY, SIP_PROXY_IP_DEFAULT);
    
    /// <summary>
    /// The local ip address.
    /// </summary>
    public string SipLocalIp => GetProperty(SIP_LOCAL_IP_KEY, SIP_LOCAL_IP_DEFAULT);

    /// <summary>
    /// The SIP account on the PBX server. This is the account that will be used to make and receive calls for this ADS SIP instance.
    /// </summary>
    public string SipAlias => GetProperty(SIP_ALIAS_KEY, SIP_ALIAS_DEFAULT);

    /// <summary>
    /// The SIP password for the SipAlias on the PBX server. 
    /// </summary>
    public string SipPassword => GetProperty(SIP_PASSWORD_KEY, SIP_PASSWORD_DEFAULT);

    /// <summary>
    /// The SIP realm for the SipAlias on the PBX server. 
    /// </summary>
    public string SipRealm => GetProperty(SIP_REALM_KEY, SIP_REALM_DEFAULT);

    /// <summary>
    /// The SIP contact
    /// </summary>
    public string SipContact
    {
        get
        {
            var result = GetProperty(SIP_CONTACT_KEY, SIP_CONTACT_DEFAULT);
            result = result.Replace("{alias}", SipAlias)
                .Replace("{local_ip}", SipLocalIp)
                .Replace("{sip_signaling_port}", SipSignalingPort.ToString());
            return result;
        }
    }

    public CallAnalysis ConnectedAlertHandling
    {
        get
        {
            var result = GetProperty(SIP_CONNECT_ALERT_HANDLING, SIP_CONNECT_ALERT_HANDLING_DEFAULT);

            if (result is { } strValue && Enum.TryParse<CallAnalysis>(strValue, ignoreCase: true, out var analysis))
            {
                return analysis;
            }

            return CallAnalysis.NoAnswer;
        }
    }
    
    public new void Dispose()
    {
        _logger.LogDebug("Dispose()");
        base.Dispose();
    }

}
public enum AttemptRecoveryStartPositions
{
    DropCall,
    ReleaseCall,
    ResetLineDev,
    Disabled
}

public enum AttemptRecoveryWhen
{
    None,
    MakeCall,
    All
}