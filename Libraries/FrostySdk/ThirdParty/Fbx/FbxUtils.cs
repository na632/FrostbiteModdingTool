using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    internal static class FbxUtils
    {
        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?FbxMalloc@fbxsdk@@YAPEAX_K@Z")]
        private static extern IntPtr FbxMallocInternal(ulong Size);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?FbxFree@fbxsdk@@YAXPEAX@Z")]
        private static extern void FbxFreeInternal(IntPtr Ptr);

        public static IntPtr FbxMalloc(ulong Size)
        {
            return FbxMallocInternal(Size);
        }

        public static void FbxFree(IntPtr Ptr)
        {
            FbxFreeInternal(Ptr);
        }

        public static string IntPtrToString(IntPtr InPtr)
        {
            return Marshal.PtrToStringAnsi(InPtr);
        }
    }
}
