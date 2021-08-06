// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using ivrToolkit.Core.Exceptions;
using System.IO;

namespace ivrToolkit.Core.Util
{
    /// <summary>
    /// Holds the voice.properties in a static Properties class.
    /// </summary>
    public class VoiceProperties
    {
        private readonly Properties _p;

        [ThreadStatic]
        private static VoiceProperties _current;

        /// <summary>
        /// The voice properties singleton object.
        /// </summary>
        public static VoiceProperties Current
        {
            get { return _current ?? (_current = new VoiceProperties()); }
        }

        private VoiceProperties()
        {

            string tenantDirectory = TenantSingleton.Instance.TenantDirectory;
            var path = Path.Combine(tenantDirectory, "voice.properties");
            _p = new Properties(path);
        }

        /// <summary>
        /// converts value to boolean
        /// </summary>
        /// <param name="value">the value to convert to bool</param>
        /// <returns>true if value equals 'true' or 'on'. Otherwise returns false. case insensitive.</returns>
        private bool ToBool(string value)
        {
            if (value.ToLower() == "true" || value.ToLower() == "on")
            {
                return true;
            }
            return false;
        }



        /// <summary>
        /// The device name pattern. Default is ':P_pdk_na_an_io:V_dxxxB{board}C{channel}' for gx and 'dxxxB{board}C{channel}' for dx.
        /// </summary>
        public string DeviceNamePattern {
            get
            {
                return _p.GetProperty("voice.deviceNamePattern", UseGc ? ":P_pdk_na_an_io:V_dxxxB{board}C{channel}" : "dxxxB{board}C{channel}");
            }
        }

        /// <summary>
        /// True to use GC_OpenEx instead of DX_OPEN
        /// </summary>
        public bool UseGc { get { return ToBool(_p.GetProperty("voice.useGC", "false")); } }
        
        /// <summary>
        /// The class name of the plugin to instantiate. Default is 'ivrToolkit.SimulatorPlugin.Simulator'.
        /// </summary>
        public string ClassName { get { return _p.GetProperty("voice.className", "ivrToolkit.SimulatorPlugin.Simulator"); } }
        /// <summary>
        /// The assembly name of the plugin. Default is 'ivrToolkit.SimulatorPlugin.dll'.ies
        /// </summary>
        public string AssemblyName { get { return _p.GetProperty("voice.assemblyName", "ivrToolkit.SimulatorPlugin.dll"); } }
        /// <summary>
        /// Dial tone detection.  Default is 'L' for Leading.
        /// </summary>
        public string DialToneType { get { return _p.GetProperty("dial.cpa.dialTone.type", "L"); } }
        /// <summary>
        /// Total number of attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '99'.
        /// </summary>
        public int PromptAttempts { get { return int.Parse(_p.GetProperty("prompt.attempts", "99")); } }
        /// <summary>
        /// Number of blank entry attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '5'.
        /// </summary>
        public int PromptBlankAttempts { get { return int.Parse(_p.GetProperty("prompt.blankAttempts", "5")); } }
        /// <summary>
        /// Number of milliseconds between keypress before it considers it to be a prompt attempt. Default is '5000'.
        /// </summary>
        public int DigitsTimeoutInMilli { get { return int.Parse(_p.GetProperty("getDigits.timeoutInMilliseconds", "5000")); } }
        /// <summary>
        /// if true then the noFreeLine tone will be enabled
        /// </summary>
        public bool CustomOutboundEnabled { get { return ToBool(_p.GetProperty("dial.customOutbound.enabled", "true")); } }
        /// <summary>
        /// if true then the dial method will check for dial tone after picking up the receiver/dialing the number.
        /// </summary>
        public bool PreTestDialTone { get { return ToBool(_p.GetProperty("dial.preTestDialTone", "true")); } }
        /// <summary>
        /// Number to add the line in order to get the channel.
        /// </summary>
        public int SipChannelOffset { get { return int.Parse(_p.GetProperty("sip.channel_offset", "0")); } }
        /// <summary>
        /// The SIP port used for H323 signaling
        /// </summary>
        public int SipH323SignalingPort { get { return int.Parse(_p.GetProperty("sip.h323_signaling_port", "1720")); } }
        public int MaxCalls => int.Parse(_p.GetProperty("sip.max_calls", "1"));

        /// <summary>
        /// The SIP port used for SIP signaling
        /// </summary>
        public int SipSignalingPort { get { return int.Parse(_p.GetProperty("sip.sip_signaling_port", "5060")); } }
        /// <summary>
        /// The SIP proxy ip address.  This is the address of the PBX that will be used to connect to the SIP Trunk.
        /// </summary>
        public string SipProxyIp { get { return _p.GetProperty("sip.proxy_ip", "10.143.102.42"); } }
        /// <summary>
        /// The SIP local ip address.  This is the address of the server that runs this program.
        /// </summary>
        public string SipLocalIp { get { return _p.GetProperty("sip.local_ip", "127.0.0.1"); } }
        /// <summary>
        /// The SIP account on the PBX server. This is the account that will be used to make and receive calls for this ADS SIP instance.
        /// </summary>
        public string SipAlias { get { return _p.GetProperty("sip.alias", "SipAccount"); } }
        /// <summary>
        /// The SIP password for the SipAlias on the PBX server. 
        /// </summary>
        public string SipPassword { get { return _p.GetProperty("sip.password", "password"); } }
        /// <summary>
        /// The SIP realm for the SipAlias on the PBX server. 
        /// </summary>
        public string SipRealm { get { return _p.GetProperty("sip.realm", ""); } }


        /// <summary>
        /// The definition of dial tone. Default is '350,20,440,20,L'. The default Tone Id is '306'.
        /// </summary>
        public CustomTone DialTone
        {
            get
            {
                var tone = new CustomTone(_p.GetProperty("customTone.dialTone","350,20,440,20,L"))
                {
                    Tid = int.Parse(_p.GetProperty("customTone.dialTone.tid","306"))
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
                var tone = new CustomTone(_p.GetProperty("dial.customOutbound.noFreeLineTone","480,30,620,40,25,5,25,5,2"))
                {
                    Tid = int.Parse(_p.GetProperty("dial.customOutbound.noFreeLineTone.tid","305"))
                };
                return tone;
            }
        }

        // Trivial severity levels from c++ Boost
        enum severity_level
        {
            trace,
            debug,
            info,
            warning,
            error,
            fatal
        };

        public int CppLogLevel
        {
            get
            {
                var result = _p.GetProperty("sip.cppLogLevel", "info");
                try
                {
                    var level = (severity_level)Enum.Parse(typeof(severity_level), result, true);
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

    /// <summary>
    /// Custom Tone Types
    /// </summary>
    public enum CustomToneType
    {
        /// <summary>
        /// Single tone.
        /// </summary>
        Single,
        /// <summary>
        /// Dual tone.
        /// </summary>
        Dual,
        /// <summary>
        /// Dual tone with cadence.
        /// </summary>
        DualWithCadence
    }

    /// <summary>
    /// Custom tone definition.
    /// </summary>
    public class CustomTone
    {
        /// <summary>
        /// The type of tone.
        /// </summary>
        public CustomToneType ToneType;
        /// <summary>
        /// The tone Id.
        /// </summary>
        public int Tid;
        /// <summary>
        /// First frequency value
        /// </summary>
        public int Freq1;
        /// <summary>
        /// First frequency deviation value.
        /// </summary>
        public int Frq1Dev;
        /// <summary>
        /// Second frequency value.
        /// </summary>
        public int Freq2;
        /// <summary>
        /// Second frequency deviation value.
        /// </summary>
        public int Frq2Dev;
        /// <summary>
        /// Cadence on time value.
        /// </summary>
        public int Ontime;
        /// <summary>
        /// Cadence on time deviation value.
        /// </summary>
        public int Ontdev;
        /// <summary>
        /// Cadence off time value.
        /// </summary>
        public int Offtime;
        /// <summary>
        /// Cadence off time deviation value.
        /// </summary>
        public int Offtdev;
        /// <summary>
        /// Repeat count.
        /// </summary>
        public int Repcnt;
        /// <summary>
        /// Leading or Trailing tone detection.
        /// </summary>
        public ToneDetection Mode;

        /// <summary>
        /// Builds a custom tone based on the string definition passed in. example: '480,30,620,40,25,5,25,5,2'
        /// </summary>
        /// <param name="definition">The definition of the tone to split up. Example: '480,30,620,40,25,5,25,5,2'</param>
        public CustomTone(string definition)
        {
            var parts = definition.Split(',');
            if (parts.Length == 9)
            {
                ToneType = CustomToneType.DualWithCadence;
                Freq1 = int.Parse(parts[0]);
                Frq1Dev = int.Parse(parts[1]);
                Freq2 = int.Parse(parts[2]);
                Frq2Dev = int.Parse(parts[3]);
                Ontime = int.Parse(parts[4]);
                Ontdev = int.Parse(parts[5]);
                Offtime = int.Parse(parts[6]);
                Offtdev = int.Parse(parts[7]);
                Repcnt = int.Parse(parts[8]);
            }
            else if (parts.Length == 5)
            {
                ToneType = CustomToneType.Dual;
                Freq1 = int.Parse(parts[0]);
                Frq1Dev = int.Parse(parts[1]);
                Freq2 = int.Parse(parts[2]);
                Frq2Dev = int.Parse(parts[3]);
                if (parts[4] == "L")
                {
                    Mode = ToneDetection.Leading;
                }
                else if (parts[4] == "T")
                {
                    Mode = ToneDetection.Trailing;
                }
                else
                {
                    throw new VoiceException("Custom tone is invalid");
                }
            }
            else if (parts.Length == 3)
            {
                ToneType = CustomToneType.Single;
                Freq1 = int.Parse(parts[0]);
                Frq1Dev = int.Parse(parts[1]);
                if (parts[2] == "L")
                {
                    Mode = ToneDetection.Leading;
                }
                else if (parts[2] == "T")
                {
                    Mode = ToneDetection.Trailing;
                }
                else
                {
                    throw new VoiceException("Custom tone is invalid");
                }
            }
            else
            {
                throw new VoiceException("Custom tone is invalid");
            }
        }
    } // class

} // namespace
