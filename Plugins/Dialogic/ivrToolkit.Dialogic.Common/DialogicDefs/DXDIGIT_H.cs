using System.Runtime.InteropServices;

namespace ivrToolkit.Dialogic.Common.DialogicDefs
{
    public class DXDIGIT_H
    {
        /* 
         * dx_getdig( ) related defines
         */
        public const int LEN_DIGBUF = 31;          /* Max # of entries for collecting DTMF */
        public const int DG_MAXDIGS = LEN_DIGBUF;  /* Max Digits Returned by dx_getdig() */
        public const int DG_END = -1; /* Terminator for dg_type Array in DV_DIGIT */
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DV_DIGIT
    {

        /// char[]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dg_value;

        /// char[]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dg_type;
    }
}
