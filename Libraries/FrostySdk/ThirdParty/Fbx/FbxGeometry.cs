using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxGeometry : FbxGeometryBase
    {
        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?AddDeformer@FbxGeometry@fbxsdk@@QEAAHPEAVFbxDeformer@2@@Z")]
        private static extern int AddDeformerInternal(IntPtr pHandle, IntPtr pDeformer);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetDeformerCount@FbxGeometry@fbxsdk@@QEBAHW4EDeformerType@FbxDeformer@2@@Z")]
        private static extern int GetDeformerCountInternal(IntPtr pHandle, FbxDeformer.EDeformerType pType);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetDeformer@FbxGeometry@fbxsdk@@QEBAPEAVFbxDeformer@2@HW4EDeformerType@32@PEAVFbxStatus@2@@Z")]
        private static extern IntPtr GetDeformerInternal(IntPtr pHandle, int pIndex, FbxDeformer.EDeformerType pType, IntPtr pStatus);

        public FbxGeometry(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public int AddDeformer(FbxDeformer deformer)
        {
            return AddDeformerInternal(pHandle, deformer.Handle);
        }

        public int GetDeformerCount(FbxDeformer.EDeformerType type)
        {
            return GetDeformerCountInternal(pHandle, type);
        }

        public FbxDeformer GetDeformer(int index, FbxDeformer.EDeformerType type)
        {
            IntPtr deformerInternal = GetDeformerInternal(pHandle, index, type, IntPtr.Zero);
            if (deformerInternal == IntPtr.Zero)
            {
                return null;
            }
            return new FbxDeformer(deformerInternal);
        }
    }
}
