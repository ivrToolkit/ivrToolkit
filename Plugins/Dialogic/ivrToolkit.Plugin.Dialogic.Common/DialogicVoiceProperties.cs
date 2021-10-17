using ivrToolkit.Core;
using ivrToolkit.Core.Enums;
using Microsoft.Extensions.Logging;
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace ivrToolkit.Plugin.Dialogic.Common
{
    public class DialogicVoiceProperties : VoiceProperties
    {
        public DialogicVoiceProperties(ILoggerFactory loggerFactory, string fileName) : base(loggerFactory, fileName)
        {
        }

        /// <summary>
        /// The device name pattern. Default is ':P_pdk_na_an_io:V_dxxxB{board}C{channel}' for gx and 'dxxxB{board}C{channel}' for dx.
        /// </summary>
        public string DeviceNamePattern => TheProperties.GetProperty("voice.deviceNamePattern", UseGc ? ":P_pdk_na_an_io:V_dxxxB{board}C{channel}" : "dxxxB{board}C{channel}");

        /// <summary>
        /// True to use GC_OpenEx instead of DX_OPEN
        /// </summary>
        public bool UseGc => ToBool(TheProperties.GetProperty("voice.useGC", "false"));

        /// <summary>
        /// Dial tone detection.  Default is 'L' for Leading.
        /// </summary>
        public string DialToneType => TheProperties.GetProperty("dial.cpa.dialTone.type", "L");


        /// <summary>
        /// Number of milliseconds between keypress before it considers it to be a prompt attempt. Default is '5000'.
        /// </summary>
        public int DigitsTimeoutInMilli => int.Parse(TheProperties.GetProperty("getDigits.timeoutInMilliseconds", "5000"));

        /// <summary>
        /// if true then the noFreeLine tone will be enabled
        /// </summary>
        public bool CustomOutboundEnabled => ToBool(TheProperties.GetProperty("dial.customOutbound.enabled", "true"));

        /// <summary>
        /// if true then the dial method will check for dial tone after picking up the receiver/dialing the number.
        /// </summary>
        public bool PreTestDialTone => ToBool(TheProperties.GetProperty("dial.preTestDialTone", "true"));



        /// <summary>
        /// The definition of dial tone. Default is '350,20,440,20,L'. The default Tone Id is '306'.
        /// </summary>
        public CustomTone DialTone
        {
            get
            {
                var tone = new CustomTone(TheProperties.GetProperty("customTone.dialTone", "350,20,440,20,L"))
                {
                    Tid = int.Parse(TheProperties.GetProperty("customTone.dialTone.tid", "306"))
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
                var tone = new CustomTone(TheProperties.GetProperty("dial.customOutbound.noFreeLineTone", "480,30,620,40,25,5,25,5,2"))
                {
                    Tid = int.Parse(TheProperties.GetProperty("dial.customOutbound.noFreeLineTone.tid", "305"))
                };
                return tone;
            }
        }

    }
}
