using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxDeformer : FbxObject
    {
        public enum EDeformerType
        {
            eUnknown,
            eSkin,
            eBlendShape,
            eVertexCache
        }

        private delegate EDeformerType GetDeformerTypeDelegate(IntPtr handle);

        private GetDeformerTypeDelegate GetDeformerTypeInternal;

        public EDeformerType DeformerType => GetDeformerTypeInternal(pHandle);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxDeformer@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxDeformer@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        public FbxDeformer(IntPtr InHandle)
            : base(InHandle)
        {
            GetDeformerTypeInternal = Marshal.GetDelegateForFunctionPointer<GetDeformerTypeDelegate>(Marshal.ReadIntPtr(vTable + 184));
        }

        public FbxDeformer(FbxManager manager, string name)
            : this(CreateFromManager(manager.Handle, name))
        {
        }

        public FbxDeformer(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }
    }
}
