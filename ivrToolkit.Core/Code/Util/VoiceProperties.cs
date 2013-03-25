/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core.Exceptions;

namespace ivrToolkit.Core.Util
{
    /// <summary>
    /// Holds the voice.properties in a static Properties class.
    /// </summary>
    public class VoiceProperties
    {
        private Properties p;

        [ThreadStatic]
        private static VoiceProperties current;

        /// <summary>
        /// The voice properties singleton object.
        /// </summary>
        public static VoiceProperties Current
        {
            get
            {
                if (current == null)
                {
                    current = new VoiceProperties();
                }
                return current;
            }
        }

        private VoiceProperties()
        {
            p = new Properties("voice.properties");
        }
        private void reset()
        {
            p = new Properties("voice.properties");
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
            else
            {
                return false;
            }
        }
        /// <summary>
        /// The class name of the plugin to instantiate. Default is 'ivrToolkit.SimulatorPlugin.Simulator'.
        /// </summary>
        public string ClassName { get { return p.GetProperty("voice.className", "ivrToolkit.SimulatorPlugin.Simulator"); } }
        /// <summary>
        /// The assembly name of the plugin. Default is 'ivrToolkit.SimulatorPlugin.dll'.ies
        /// </summary>
        public string AssemblyName { get { return p.GetProperty("voice.assemblyName", "ivrToolkit.SimulatorPlugin.dll"); } }
        /// <summary>
        /// Dial tone detection.  Default is 'L' for Leading.
        /// </summary>
        public string DialToneType { get { return p.GetProperty("dial.cpa.dialTone.type", "L"); } }
        /// <summary>
        /// Number of attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '5'.
        /// </summary>
        public int PromptAttempts { get { return int.Parse(p.GetProperty("prompt.attempts", "5")); } }
        /// <summary>
        /// Number of milliseconds between keypress before it considers it to be a prompt attempt. Default is '5000'.
        /// </summary>
        public int DigitsTimeoutInMilli { get { return int.Parse(p.GetProperty("getDigits.timeoutInMilliseconds", "5000")); } }
        /// <summary>
        /// if true then the noFreeLine tone will be enabled
        /// </summary>
        public bool CustomOutboundEnabled { get { return ToBool(p.GetProperty("dial.customOutbound.enabled", "true")); } }
        /// <summary>
        /// if true then the dial method will check for dial tone after picking up the receiver/dialing the number.
        /// </summary>
        public bool PreTestDialTone { get { return ToBool(p.GetProperty("dial.preTestDialTone", "true")); } }
        /// <summary>
        /// The definition of dial tone. Default is '350,20,440,20,L'. The default Tone Id is '306'.
        /// </summary>
        public CustomTone DialTone
        {
            get
            {
                CustomTone tone = new CustomTone(p.GetProperty("customTone.dialTone","350,20,440,20,L"))
                {
                    Tid = int.Parse(p.GetProperty("customTone.dialTone.tid","306"))
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
                CustomTone tone = new CustomTone(p.GetProperty("dial.customOutbound.noFreeLineTone","480,30,620,40,25,5,25,5,2"))
                {
                    Tid = int.Parse(p.GetProperty("dial.customOutbound.noFreeLineTone.tid","305"))
                };
                return tone;
            }
        }
        /// <summary>
        /// Gets a property given the name parameter. Only use this method if there is no VoiceProperties helper method.
        /// </summary>
        /// <param name="name">The name of the property to look up.</param>
        /// <returns>The value of the property.</returns>
        public string GetProperty(string name)
        {
            return p.GetProperty(name);
        }
        /// <summary>
        /// Gets a property given the name parameter. Only use this method if there is no VoiceProperties helper method.
        /// </summary>
        /// <param name="name">The name of the property to look up.</param>
        /// <param name="def">The default value if the property name is not found</param>
        /// <returns>The value of the property.</returns>
        public string GetProperty(string name, string def)
        {
            return p.GetProperty(name, def);
        }
        /// <summary>
        /// Gets a list of property names that matches the prefix.
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of parameter names where the property name begins with the prefix</returns>
        public string[] GetKeyPrefixMatch(string prefix)
        {
            return p.GetKeyPrefixMatch(prefix);
        }
        /// <summary>
        /// Gets a list of property values where the property name matches the prefix.
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of values where the property name begins with the prefix</returns>
        public string[] GetPrefixMatch(string prefix)
        {
            return p.GetPrefixMatch(prefix);
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
        public int Frq1dev;
        /// <summary>
        /// Second frequency value.
        /// </summary>
        public int Freq2;
        /// <summary>
        /// Second frequency deviation value.
        /// </summary>
        public int Frq2dev;
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
            string[] parts = definition.Split(',');
            if (parts.Length == 9)
            {
                ToneType = CustomToneType.DualWithCadence;
                Freq1 = int.Parse(parts[0]);
                Frq1dev = int.Parse(parts[1]);
                Freq2 = int.Parse(parts[2]);
                Frq2dev = int.Parse(parts[3]);
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
                Frq1dev = int.Parse(parts[1]);
                Freq2 = int.Parse(parts[2]);
                Frq2dev = int.Parse(parts[3]);
                if (parts[4] == "L")
                {
                    Mode = ToneDetection.leading;
                }
                else if (parts[4] == "T")
                {
                    Mode = ToneDetection.trailing;
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
                Frq1dev = int.Parse(parts[1]);
                if (parts[2] == "L")
                {
                    Mode = ToneDetection.leading;
                }
                else if (parts[2] == "T")
                {
                    Mode = ToneDetection.trailing;
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
