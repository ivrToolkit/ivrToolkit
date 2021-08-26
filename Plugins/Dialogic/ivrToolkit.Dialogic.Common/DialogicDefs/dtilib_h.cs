using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo

namespace ivrToolkit.Dialogic.Common.DialogicDefs
{
    public class dtilib_h
    {
        /* Define device class */
        public const string DEV_CLASS_DTI = "DTI";

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dt_open([MarshalAs(UnmanagedType.LPStr)] string namep, int oflags);

        [DllImport("LIBDXXMT.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dt_close(int dev);

        /*
         * Defines for Sync mode and async mode
         */
        public const int EV_ASYNC = 0x8000;
        public const int EV_SYNC = 0x0000;

    }
    public struct SC_TSINFO
    {
        public uint sc_numts;
        public IntPtr sc_tsarrayp;
    }
}
