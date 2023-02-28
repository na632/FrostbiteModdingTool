using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxSubDeformer : FbxObject
    {
        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxSubDeformer@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxSubDeformer@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        internal FbxSubDeformer()
        {
        }

        public FbxSubDeformer(FbxManager manager, string name)
        {
            pHandle = CreateFromManager(manager.Handle, name);
        }

        public FbxSubDeformer(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public FbxSubDeformer(FbxObject obj, string name)
        {
            pHandle = CreateFromObject(obj.Handle, name);
        }
    }
}
