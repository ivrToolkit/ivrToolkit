using System;
using System.Runtime.InteropServices;

namespace ivrToolkit.Plugin.Dialogic.Sip
{
    public static class DialogicExtensions
    {
        public static string IntPtrToString(this IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }
    }
}
