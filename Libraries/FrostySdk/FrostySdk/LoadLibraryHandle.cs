using System;

namespace FrostySdk
{
    internal class LoadLibraryHandle
    {
        private IntPtr handle;

        public LoadLibraryHandle(string lib)
        {
            handle = Kernel32.LoadLibraryEx(lib, IntPtr.Zero, 0u);
        }

        public static implicit operator IntPtr(LoadLibraryHandle value)
        {
            return value.handle;
        }

        ~LoadLibraryHandle()
        {
            Kernel32.FreeLibrary(handle);
        }
    }
}
