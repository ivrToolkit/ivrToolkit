using System;
using System.Runtime.InteropServices;
using ivrToolkit.Plugin.Dialogic.Common.DialogicDefs;
using ivrToolkit.Plugin.Dialogic.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Plugin.Dialogic.Common.Extensions
{
    public static class DialogicExtensions
    {
        public static void LogIfGlobalCallError<T>(this int returnCode, ILogger<T> logger)
        {
            try
            {
                returnCode.ThrowIfGlobalCallError();
            }
            catch (GlobalCallErrorException ex)
            {
                logger.LogWarning("GlobalCallError: {0} : {1}", returnCode, ex.Message);
            }
        }
        public static void ThrowIfGlobalCallError(this int returnCode)
        {
            if (returnCode >= 0) return; // no error

            var gcErrorInfo = new GC_INFO();

            var structSize = Marshal.SizeOf<GC_INFO>();
            var pUnmanagedMemory = Marshal.AllocHGlobal(structSize);

            try
            {
                Marshal.StructureToPtr(gcErrorInfo, pUnmanagedMemory, false);

                var result = gclib_h.gc_ErrorInfo(pUnmanagedMemory);
                if (result < 0) throw new GlobalCallErrorException();

                gcErrorInfo = Marshal.PtrToStructure<GC_INFO>(pUnmanagedMemory);

                throw new GlobalCallErrorException(gcErrorInfo);
            }
            finally
            {
                Marshal.FreeHGlobal(pUnmanagedMemory);
            }

        }

        public static void ThrowIfStandardRuntimeLibraryError(this int returnCode, int devh)
        {
            if (returnCode < 0)
            {
                var errMsgPtr = srllib_h.ATDV_ERRMSGP(devh);
                throw new StandardRuntimeLibraryException(errMsgPtr.IntPtrToString());
            }
        }

        public static void LogIfStandardRuntimeLibraryError<T>(this int returnCode, int devh, ILogger<T> logger)
        {
            if (returnCode == -1)
            {
                var errMsgPtr = srllib_h.ATDV_ERRMSGP(devh);
                logger.LogWarning("RuntimeLibraryError: {0} : {1}", returnCode, errMsgPtr.IntPtrToString());
            }
        }

        public static string IntPtrToString(this IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }
    }
}
