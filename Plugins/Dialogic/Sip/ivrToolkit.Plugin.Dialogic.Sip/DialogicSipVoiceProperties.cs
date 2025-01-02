using ivrToolkit.Plugin.Dialogic.Common;
using Microsoft.Extensions.Logging;
using System;

namespace ivrToolkit.Plugin.Dialogic.Sip
{
    public class DialogicSipVoiceProperties : DialogicVoiceProperties, IDisposable
    {
        private readonly ILogger _logger;

        public DialogicSipVoiceProperties(ILoggerFactory loggerFactory, string fileName) : base(loggerFactory, fileName)
        {
            _logger = loggerFactory.CreateLogger<DialogicSipVoiceProperties>();
            _logger.LogDebug("Ctr(ILoggerFactory loggerFactory, {0})", fileName);
        }

        /// <summary>
        /// Number to add the line in order to get the channel.
        /// </summary>
        public uint SipChannelOffset => uint.Parse(GetProperty("sip.channel_offset", "0"));

        public ushort MaxCalls => ushort.Parse(GetProperty("sip.max_calls", "1"));

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

        /// <summary>
        /// The SIP realm for the SipAlias on the PBX server. 
        /// </summary>
        public string SipRealm => GetProperty("sip.realm", "");

        /// <summary>
        /// The SIP contact
        /// </summary>
        public string SipContact
        {
            get
            {
                var result = GetProperty("sip.contact", "{alias}@{proxy_ip}:{sip_signaling_port}");
                result = result.Replace("{alias}", SipAlias)
                    .Replace("{proxy_ip}", SipProxyIp)
                    .Replace("{sip_signaling_port}", SipSignalingPort.ToString());
                return result;
            }
        }

        public new void Dispose()
        {
            _logger.LogDebug("Dispose()");
            base.Dispose();
        }

    }
}
