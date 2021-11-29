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
        private readonly string _scopeName;

        public UnmanagedMemoryService(ILoggerFactory loggerFactory, string scopeName)
        {
            _scopeName = scopeName;
            _logger = loggerFactory.CreateLogger<UnmanagedMemoryService>();
            _logger.LogDebug("ctr({0})", _scopeName);
        }

        public IntPtr Create<T>(string label, T[] structObjects)
        {
            _logger.LogDebug("create<T>({0}, T[]) - scpope: {1}", label, _scopeName);
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
            _logger.LogDebug("create<T>({0}, T) - scpope: {1}", label, _scopeName);
            var structSize = Marshal.SizeOf<T>();
            var pUnmanagedMemory = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(structObject, pUnmanagedMemory, false);

            _pointers.Add(new PtrLabel { Ptr = pUnmanagedMemory, Label = label });
            return pUnmanagedMemory;
        }

        public IntPtr Create<T>(string label, T structObject, int sizeOverride)
        {
            _logger.LogDebug("create<T>({0}, T, {1}) - scpope: {2}", label, sizeOverride, _scopeName);
            var structSize = sizeOverride;
            var pUnmanagedMemory = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(structObject, pUnmanagedMemory, true);

            _pointers.Add(new PtrLabel { Ptr = pUnmanagedMemory, Label = label });
            return pUnmanagedMemory;
        }

        public IntPtr StringToHGlobalAnsi(string label, string text)
        {
            _logger.LogDebug("StringToHGlobalAnsi({0}, string) - scpope: {1}", label, _scopeName);
            var pUnmanagedMemory = Marshal.StringToHGlobalAnsi(text);

            _pointers.Add(new PtrLabel { Ptr = pUnmanagedMemory, Label = label });
            return pUnmanagedMemory;
        }
        public void Push(string label, IntPtr ptr)
        {
            _logger.LogDebug("Push({0}, IntPtr) - scpope: {1}", label, _scopeName);
            _pointers.Add(new PtrLabel { Ptr = ptr, Label = label });
        }

        public void Dispose()
        {
            _logger.LogDebug("Dispose() - scope: {0}", _scopeName);

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
            _logger.LogDebug("Free {0} - scpope: {1}", ptr, _scopeName);

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