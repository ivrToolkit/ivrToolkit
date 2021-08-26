using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo

namespace ivrToolkit.Plugin.Dialogic.Common.DialogicDefs
{
    public class dcblib_h
    {
        /* Define device class */
        public const string DEV_CLASS_DCB = "DCB";

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dcb_open([MarshalAs(UnmanagedType.LPStr)] string namep, int oflags);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dcb_close(int dev);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dcb_dsprescount(int chDev, ref int dspResourceCount);

    }
}
