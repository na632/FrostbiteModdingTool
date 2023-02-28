using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxNative
    {
        protected IntPtr pHandle;

        protected IntPtr vTable;

        protected bool bNeedsFreeing;

        internal IntPtr Handle => pHandle;

        public FbxNative()
        {
        }

        public FbxNative(IntPtr InHandle)
        {
            pHandle = InHandle;
            vTable = Marshal.ReadIntPtr(pHandle, 0);
        }

        ~FbxNative()
        {
            if (bNeedsFreeing)
            {
                FbxUtils.FbxFree(pHandle);
            }
        }
    }
}
