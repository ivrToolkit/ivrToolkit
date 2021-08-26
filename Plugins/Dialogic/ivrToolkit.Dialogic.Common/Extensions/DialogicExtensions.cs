using System;
using ivrToolkit.Dialogic.Common.DialogicDefs;
using ivrToolkit.Dialogic.Common.Exceptions;
using System.Runtime.InteropServices;

namespace ivrToolkit.Dialogic.Common.Extensions
{
    public static class DialogicExtensions
    {
        public static void ThrowIfGlobalCallError(this int returnCode)
        {
            var gcErrorInfo = new GC_INFO();

            var structSize = Marshal.SizeOf<GC_INFO>();
            var pUnmanagedMemory = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(gcErrorInfo, pUnmanagedMemory, false);

            if (returnCode == -1)
            {
                var result = gclib_h.gc_ErrorInfo(pUnmanagedMemory);
                if (result == -1) throw new GlobalCallErrorException();

                gcErrorInfo = Marshal.PtrToStructure<GC_INFO>(pUnmanagedMemory);
                Marshal.FreeHGlobal(pUnmanagedMemory);

                throw new GlobalCallErrorException(gcErrorInfo);
            }
        }

        public static void ThrowIfStandardRuntimeLibraryError(this int returnCode, int devh)
        {
            if (returnCode == -1)
            {
                var errMsgPtr = srllib_h.ATDV_ERRMSGP(devh);
                throw new StandardRuntimeLibraryException(errMsgPtr.IntPtrToString());
            }
        }

        public static string IntPtrToString(this IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }
    }
}
