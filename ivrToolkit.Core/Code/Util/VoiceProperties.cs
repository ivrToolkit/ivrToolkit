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
        private static VoiceProperties _current;

        public static VoiceProperties current
        {
            get
            {
                if (_current == null)
                {
                    _current = new VoiceProperties();
                }
                return _current;
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
        public bool isCheck(string value)
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

        // custom fields defined below
        public string className { get { return p.getProperty("voice.className", "ivrToolkit.Core.Voice.simulator.Simulator"); } }

        public string assemblyName { get { return p.getProperty("voice.assemblyName", "msVoice.dll"); } }

        public string dialToneType { get { return p.getProperty("dial.cpa.dialTone.type", "L"); } }

        public int promptAttempts { get { return int.Parse(p.getProperty("prompt.attempts", "5")); } }

        public int digitsTimeoutInMilli { get { return int.Parse(p.getProperty("getDigits.timeoutInMilliseconds", "5000")); } }

        public bool customOutboundEnabled { get { return isCheck(p.getProperty("dial.customOutbound.enabled", "true")); } }

        public bool preTestDialTone { get { return isCheck(p.getProperty("dial.preTestDialTone", "true")); } }

        public CustomTone dialTone
        {
            get
            {
                CustomTone tone = new CustomTone(p.getProperty("customTone.dialTone","350,20,440,20,L"))
                {
                    tid = int.Parse(p.getProperty("customTone.dialTone.tid","306"))
                };
                return tone;
            }
        }
        public CustomTone noFreeLineTone
        {
            get
            {
                CustomTone tone = new CustomTone(p.getProperty("dial.customOutbound.noFreeLineTone","480,30,620,40,25,5,25,5,2"))
                {
                    tid = int.Parse(p.getProperty("dial.customOutbound.noFreeLineTone.tid","305"))
                };
                return tone;
            }
        }

        public string getProperty(string name)
        {
            return p.getProperty(name);
        }

        public string getProperty(string name, string def)
        {
            return p.getProperty(name, def);
        }

        public string[] getKeyPrefixMatch(string prefix)
        {
            return p.getKeyPrefixMatch(prefix);
        }

        public string[] getPrefixMatch(string prefix)
        {
            return p.getPrefixMatch(prefix);
        }


    } // class

    public class Dial9Information
    {
        public bool enabled;
        public CustomTone fastBusy;
        public CustomTone dialTone;
    }

    public enum CustomToneType
    {
        single,
        dual,
        dualWithCadence
    }

    public class CustomTone
    {
        public CustomToneType toneType;
        public int tid;

        public int freq1;
        public int frq1dev;
        public int freq2;
        public int frq2dev;
        public int ontime;
        public int ontdev;
        public int offtime;
        public int offtdev;
        public int repcnt;
        public ToneDetection mode;

        public CustomTone(string definition)
        {
            string[] parts = definition.Split(',');
            if (parts.Length == 9)
            {
                toneType = CustomToneType.dualWithCadence;
                freq1 = int.Parse(parts[0]);
                frq1dev = int.Parse(parts[1]);
                freq2 = int.Parse(parts[2]);
                frq2dev = int.Parse(parts[3]);
                ontime = int.Parse(parts[4]);
                ontdev = int.Parse(parts[5]);
                offtime = int.Parse(parts[6]);
                offtdev = int.Parse(parts[7]);
                repcnt = int.Parse(parts[8]);
            }
            else if (parts.Length == 5)
            {
                toneType = CustomToneType.dual;
                freq1 = int.Parse(parts[0]);
                frq1dev = int.Parse(parts[1]);
                freq2 = int.Parse(parts[2]);
                frq2dev = int.Parse(parts[3]);
                if (parts[4] == "L")
                {
                    mode = ToneDetection.leading;
                }
                else if (parts[4] == "T")
                {
                    mode = ToneDetection.trailing;
                }
                else
                {
                    throw new VoiceException("Custom tone is invalid");
                }
            }
            else if (parts.Length == 3)
            {
                toneType = CustomToneType.single;
                freq1 = int.Parse(parts[0]);
                frq1dev = int.Parse(parts[1]);
                if (parts[2] == "L")
                {
                    mode = ToneDetection.leading;
                }
                else if (parts[2] == "T")
                {
                    mode = ToneDetection.trailing;
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
    }

} // namespace
