using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxGlobalSettings : FbxObject
    {
        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetSystemUnit@FbxGlobalSettings@fbxsdk@@QEAAXAEBVFbxSystemUnit@2@@Z")]
        private static extern void SetSystemUnitInternal(IntPtr InHandle, IntPtr pOther);

        public FbxGlobalSettings()
        {
        }

        public FbxGlobalSettings(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public void SetSystemUnit(FbxSystemUnit pOther)
        {
            IntPtr handle = pOther.Handle;
            SetSystemUnitInternal(pHandle, handle);
        }
    }
}
