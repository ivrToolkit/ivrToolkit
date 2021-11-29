using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ivrToolkit.Core.Util
{
   
    public class UnmanagedMemoryService : IDisposable
    {
        public class PtrLabel
        {
            public IntPtr Ptr { get; set; }
            public string Label { get; set; }
        }

        private readonly ILogger<UnmanagedMemoryService> _logger;
        private readonly List<PtrLabel> _pointers = new List<PtrLabel>();

        public UnmanagedMemoryService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UnmanagedMemoryService>();
            _logger.LogDebug("ctr()");
        }

        public IntPtr Create<T>(string label, T[] structObjects)
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

            _pointers.Add(new PtrLabel { Ptr = pUnmanagedMemory, Label = label });
            return pUnmanagedMemory;
        }

        public IntPtr Create<T>(string label, T structObject)
        {
            _logger.LogDebug("create<T>(T)");
            var structSize = Marshal.SizeOf<T>();
            var pUnmanagedMemory = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(structObject, pUnmanagedMemory, false);

            _pointers.Add(new PtrLabel { Ptr = pUnmanagedMemory, Label = label });
            return pUnmanagedMemory;
        }
        public IntPtr Create<T>(string label, T structObject, int sizeOverride)
        {
            _logger.LogDebug("create<T>(T)");
            var structSize = sizeOverride;
            var pUnmanagedMemory = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(structObject, pUnmanagedMemory, true);

            _pointers.Add(new PtrLabel { Ptr = pUnmanagedMemory, Label = label });
            return pUnmanagedMemory;
        }

        public void Dispose()
        {
            _logger.LogDebug("Dispose()");

            foreach (var ptrLabel in _pointers)
            {
                _logger.LogDebug("    Freeing: {0}", ptrLabel.Label);
                Marshal.FreeHGlobal(ptrLabel.Ptr);
                _logger.LogDebug("    Success");
            }
            _pointers.Clear();
        }

        public void Free(IntPtr ptr)
        {
            _logger.LogDebug("Free {0}", ptr);

            var ptrLabel = _pointers.FirstOrDefault(x => x.Ptr == ptr);

            if (ptrLabel == null) return;

            if (_pointers.Remove(ptrLabel))
            {
                _logger.LogDebug("Freeing: {0}", ptrLabel.Label);
                Marshal.FreeHGlobal(ptr);
                _logger.LogDebug("Success");
            }
        }
    }
}