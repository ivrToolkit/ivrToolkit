// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core
{
    /// <summary>
    /// Holds the voice.properties in a static Properties class.
    /// </summary>
    public class VoiceProperties
    {
        private readonly Properties _p;
        private readonly ILogger<VoiceProperties> _logger;

        public VoiceProperties(ILoggerFactory loggerFactory, string fileName)
        {
            _logger = loggerFactory.CreateLogger<VoiceProperties>();
            _logger.LogDebug("ctr(ILoggerFactory, {0})", fileName);
            _p = new Properties(loggerFactory, fileName);
        }

        /// <summary>
        /// converts value to boolean
        /// </summary>
        /// <param name="value">the value to convert to bool</param>
        /// <returns>true if value equals 'true' or 'on'. Otherwise returns false. case insensitive.</returns>
        private bool ToBool(string value)
        {
            _logger.LogDebug("ToBool({0})", value);
            if (value.ToLower() == "true" || value.ToLower() == "on")
            {
                return true;
            }
            return false;
        }



        /// <summary>
        /// The device name pattern. Default is ':P_pdk_na_an_io:V_dxxxB{board}C{channel}' for gx and 'dxxxB{board}C{channel}' for dx.
        /// </summary>
        public string DeviceNamePattern => _p.GetProperty("voice.deviceNamePattern", UseGc ? ":P_pdk_na_an_io:V_dxxxB{board}C{channel}" : "dxxxB{board}C{channel}");

        /// <summary>
        /// True to use GC_OpenEx instead of DX_OPEN
        /// </summary>
        public bool UseGc => ToBool(_p.GetProperty("voice.useGC", "false"));

        /// <summary>
        /// The class name of the plugin to instantiate. Default is 'ivrToolkit.SimulatorPlugin.Simulator'.
        /// </summary>
        public string ClassName => _p.GetProperty("voice.className", "ivrToolkit.SimulatorPlugin.Simulator");

        /// <summary>
        /// The assembly name of the plugin. Default is 'ivrToolkit.SimulatorPlugin.dll'.ies
        /// </summary>
        public string AssemblyName => _p.GetProperty("voice.assemblyName", "ivrToolkit.SimulatorPlugin.dll");

        /// <summary>
        /// Dial tone detection.  Default is 'L' for Leading.
        /// </summary>
        public string DialToneType => _p.GetProperty("dial.cpa.dialTone.type", "L");

        /// <summary>
        /// Total number of attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '99'.
        /// </summary>
        public int PromptAttempts => int.Parse(_p.GetProperty("prompt.attempts", "99"));

        /// <summary>
        /// Number of blank entry attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '5'.
        /// </summary>
        public int PromptBlankAttempts => int.Parse(_p.GetProperty("prompt.blankAttempts", "5"));

        /// <summary>
        /// Number of milliseconds between keypress before it considers it to be a prompt attempt. Default is '5000'.
        /// </summary>
        public int DigitsTimeoutInMilli => int.Parse(_p.GetProperty("getDigits.timeoutInMilliseconds", "5000"));

        /// <summary>
        /// if true then the noFreeLine tone will be enabled
        /// </summary>
        public bool CustomOutboundEnabled => ToBool(_p.GetProperty("dial.customOutbound.enabled", "true"));

        /// <summary>
        /// if true then the dial method will check for dial tone after picking up the receiver/dialing the number.
        /// </summary>
        public bool PreTestDialTone => ToBool(_p.GetProperty("dial.preTestDialTone", "true"));

        /// <summary>
        /// Number to add the line in order to get the channel.
        /// </summary>
        public uint SipChannelOffset => uint.Parse(_p.GetProperty("sip.channel_offset", "0"));

        /// <summary>
        /// The SIP port used for H323 signaling
        /// </summary>
        public UInt16 SipH323SignalingPort => UInt16.Parse(_p.GetProperty("sip.h323_signaling_port", "1720"));

        public uint MaxCalls => uint.Parse(_p.GetProperty("sip.max_calls", "1"));

        /// <summary>
        /// The SIP port used for SIP signaling
        /// </summary>
        public ushort SipSignalingPort => ushort.Parse(_p.GetProperty("sip.sip_signaling_port", "5060"));

        /// <summary>
        /// The SIP proxy ip address.  This is the address of the PBX that will be used to connect to the SIP Trunk.
        /// </summary>
        public string SipProxyIp => _p.GetProperty("sip.proxy_ip", "10.143.102.42");

        /// <summary>
        /// The SIP local ip address.  This is the address of the server that runs this program.
        /// </summary>
        public string SipLocalIp => _p.GetProperty("sip.local_ip", "127.0.0.1");

        /// <summary>
        /// The SIP account on the PBX server. This is the account that will be used to make and receive calls for this ADS SIP instance.
        /// </summary>
        public string SipAlias => _p.GetProperty("sip.alias", "SipAccount");

        /// <summary>
        /// The SIP password for the SipAlias on the PBX server. 
        /// </summary>
        public string SipPassword => _p.GetProperty("sip.password", "password");

        /// <summary>
        /// The SIP realm for the SipAlias on the PBX server. 
        /// </summary>
        public string SipRealm => _p.GetProperty("sip.realm", "");


        /// <summary>
        /// The definition of dial tone. Default is '350,20,440,20,L'. The default Tone Id is '306'.
        /// </summary>
        public CustomTone DialTone
        {
            get
            {
                var tone = new CustomTone(_p.GetProperty("customTone.dialTone", "350,20,440,20,L"))
                {
                    Tid = int.Parse(_p.GetProperty("customTone.dialTone.tid", "306"))
                };
                return tone;
            }
        }
        /// <summary>
        /// The definition of the no free line dial tone. Usually is a fast busy signal. Default is '480,30,620,40,25,5,25,5,2' with a tone id of '305'.
        /// </summary>
        public CustomTone NoFreeLineTone
        {
            get
            {
                var tone = new CustomTone(_p.GetProperty("dial.customOutbound.noFreeLineTone", "480,30,620,40,25,5,25,5,2"))
                {
                    Tid = int.Parse(_p.GetProperty("dial.customOutbound.noFreeLineTone.tid", "305"))
                };
                return tone;
            }
        }

        // Trivial severity levels from c++ Boost
        enum SeverityLevel
        {
            // ReSharper disable UnusedMember.Local
            Trace,
            Debug,
            Info,
            Warning,
            Error,
            Fatal
            // ReSharper restore UnusedMember.Local
        };

        public int CppLogMaxFiles => int.Parse(_p.GetProperty("sip.cppLogMaxFiles", "5"));
        public int CppLogRotationSize => int.Parse(_p.GetProperty("sip.cppLogRotationSize", "2097152")); // 2MB default

        public int CppLogLevel
        {
            get
            {
                var result = _p.GetProperty("sip.cppLogLevel", "info");
                try
                {
                    var level = (SeverityLevel)Enum.Parse(typeof(SeverityLevel), result, true);
                    return (int)level;
                }
                catch (Exception)
                {
                    return 0; // info
                }
            }
        }

        /// <summary>
        /// Gets a property given the name parameter. Only use this method if there is no VoiceProperties helper method.
        /// </summary>
        /// <param name="name">The name of the property to look up.</param>
        /// <returns>The value of the property.</returns>
        public string GetProperty(string name)
        {
            return _p.GetProperty(name);
        }
        /// <summary>
        /// Gets a property given the name parameter. Only use this method if there is no VoiceProperties helper method.
        /// </summary>
        /// <param name="name">The name of the property to look up.</param>
        /// <param name="def">The default value if the property name is not found</param>
        /// <returns>The value of the property.</returns>
        public string GetProperty(string name, string def)
        {
            return _p.GetProperty(name, def);
        }
        /// <summary>
        /// Gets a list of property names that matches the prefix.
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of parameter names where the property name begins with the prefix</returns>
        public string[] GetKeyPrefixMatch(string prefix)
        {
            return _p.GetKeyPrefixMatch(prefix);
        }
        /// <summary>
        /// Gets a list of property values where the property name matches the prefix.
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of values where the property name begins with the prefix</returns>
        public string[] GetPrefixMatch(string prefix)
        {
            return _p.GetPrefixMatch(prefix);
        }


    } // class

    // class

} // namespace
