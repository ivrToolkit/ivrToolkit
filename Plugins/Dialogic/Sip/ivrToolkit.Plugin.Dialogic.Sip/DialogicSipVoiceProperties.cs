﻿using ivrToolkit.Plugin.Dialogic.Common;
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
        public ushort SipSignalingPort => ushort.Parse(GetProperty("sip.sip_signaling_port", "0"));

        /// <summary>
        /// The SIP proxy ip address.  This is the address of the PBX that will be used to connect to the SIP Trunk.
        /// </summary>
        public string SipProxyIp => GetProperty("sip.proxy_ip", "");

        /// <summary>
        /// The SIP local ip address.  This is the address of the server that runs this program.
        /// </summary>
        public string SipLocalIp => GetProperty("sip.local_ip", "127.0.0.1");

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
        /// Required for call-out
        /// </summary>
        public string SipUserAgent => GetProperty("sip.user_agent", "");

        /// <summary>
        /// Required for call-out
        /// </summary>
        public string SipFrom => GetProperty("sip.from", "");

        /// <summary>
        /// Required for call-out
        /// </summary>
        public string SipConctact => GetProperty("sip.contact", "");

        public new void Dispose()
        {
            _logger.LogDebug("Dispose()");
            base.Dispose();
        }

    }
}
