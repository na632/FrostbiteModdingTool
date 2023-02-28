using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxTime : FbxNative
    {
        public static FbxTime FBXSDK_TIME_INFINITE = new FbxTime(long.MaxValue);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "??0FbxTime@fbxsdk@@QEAA@_J@Z")]
        private static extern void ConstructInternal(IntPtr inHandle, long pTime);

        public FbxTime(long time)
        {
            pHandle = FbxUtils.FbxMalloc(8uL);
            ConstructInternal(pHandle, time);
            bNeedsFreeing = true;
        }
    }
}
