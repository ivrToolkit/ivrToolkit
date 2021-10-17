using ivrToolkit.Core.Exceptions;

namespace ivrToolkit.Core.Enums
{
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
    }
}