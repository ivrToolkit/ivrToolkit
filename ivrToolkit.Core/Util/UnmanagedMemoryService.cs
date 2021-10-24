using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ivrToolkit.Core.Util
{
    public class UnmanagedMemoryService : IDisposable
    {
        private readonly ILogger<UnmanagedMemoryService> _logger;
        private readonly IList<IntPtr> _pointers = new List<IntPtr>();

        public UnmanagedMemoryService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UnmanagedMemoryService>();
            _logger.LogDebug("ctr()");
        }

        public IntPtr Create<T>(T[] structObjects)
        {
            _logger.LogDebug("create<T>(T[])");
            var structSize = Marshal.SizeOf<T>();
            var pUnmanagedMemory = Marshal.AllocHGlobal(structSize * structObjects.Length);
            var currentPosition = pUnmanagedMemory;
            foreach (var structObject in structObjects)
            {
                Marshal.StructureToPtr(structObject, currentPosition, false);
                currentPosition += structSize;
            }

            _pointers.Add(pUnmanagedMemory);
            return pUnmanagedMemory;
        }

        public IntPtr Create<T>(T structObject)
        {
            _logger.LogDebug("create<T>(T)");
            var structSize = Marshal.SizeOf<T>();
            var pUnmanagedMemory = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(structObject, pUnmanagedMemory, false);

            _pointers.Add(pUnmanagedMemory);
            return pUnmanagedMemory;
        }
        public IntPtr Create<T>(T structObject, int sizeOverride)
        {
            _logger.LogDebug("create<T>(T)");
            var structSize = sizeOverride;
            var pUnmanagedMemory = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(structObject, pUnmanagedMemory, true);

            _pointers.Add(pUnmanagedMemory);
            return pUnmanagedMemory;
        }

        public void Dispose()
        {
            foreach (var pointer in _pointers)
            {
                Marshal.FreeHGlobal(pointer);
            }
        }
    }
}