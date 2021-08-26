using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo

namespace ivrToolkit.Dialogic.Common.DialogicDefs
{
    public class msilib_h
    {
        /* Define device class */
        public const string DEV_CLASS_MSI = "MSI";

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ms_open([MarshalAs(UnmanagedType.LPStr)] string namep, int oflags);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ms_close(int dev);
    }
}
