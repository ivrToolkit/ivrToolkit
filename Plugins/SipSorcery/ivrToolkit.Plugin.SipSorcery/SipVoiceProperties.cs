using ivrToolkit.Core;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.SipSorcery
{
    public class SipVoiceProperties : VoiceProperties, IDisposable
    {
        private readonly ILogger _logger;
        public SipVoiceProperties(ILoggerFactory loggerFactory, string fileName) : base(loggerFactory, fileName)
        {
            _logger = loggerFactory.CreateLogger<SipVoiceProperties>();
            _logger.LogDebug("Ctr(ILoggerFactory loggerFactory, {0})", fileName);
        }

        /// <summary>
        /// Display the Sipsorcery log information in the log. Default is True.
        /// </summary>
        public bool SipTransportEnableTraceLogs => bool.Parse(GetProperty("debug.sipTransport.enableTraceLogs", "true"));

        /// <summary>
        /// Number of milliseconds between keypress before it considers it to be a prompt attempt. Default is '5000'.
        /// </summary>
        public int DigitsTimeoutInMilli => int.Parse(GetProperty("getDigits.timeoutInMilliseconds", "5000"));

        /// <summary>
        /// The SIP port used for SIP signaling
        /// </summary>
        public ushort SipSignalingPort => ushort.Parse(GetProperty("sip.sip_signaling_port", "5060"));

        /// <summary>
        /// The SIP proxy ip address.  This is the address of the PBX that will be used to connect to the SIP Trunk.
        /// </summary>
        public string SipProxyIp => GetProperty("sip.proxy_ip", "");

        /// <summary>
        /// The SIP account on the PBX server. This is the account that will be used to make and receive calls for this ADS SIP instance.
        /// </summary>
        public string SipAlias => GetProperty("sip.alias", "");

        /// <summary>
        /// The SIP password for the SipAlias on the PBX server. 
        /// </summary>
        public string SipPassword => GetProperty("sip.password", "");

        public new void Dispose()
        {
            _logger.LogDebug("Dispose()");
            base.Dispose();
        }
    }
}
