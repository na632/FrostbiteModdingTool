using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxLayerElementUserData : FbxLayerElement
    {
        public FbxLayerElementArray DirectArray
        {
            get
            {
                IntPtr directArrayInternal = GetDirectArrayInternal(pHandle);
                if (directArrayInternal == IntPtr.Zero)
                {
                    return null;
                }
                return new FbxLayerElementArray(directArrayInternal);
            }
        }

        public FbxLayerElementArray IndexArray
        {
            get
            {
                IntPtr indexArrayInternal = GetIndexArrayInternal(pHandle);
                if (indexArrayInternal == IntPtr.Zero)
                {
                    return null;
                }
                return new FbxLayerElementArray(indexArrayInternal);
            }
        }

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxLayerElementUserData@fbxsdk@@SAPEAV12@PEAVFbxLayerContainer@2@AEBV12@@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pOwner, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetDirectArray@?$FbxLayerElementTemplate@VFbxColor@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@VFbxColor@fbxsdk@@@2@XZ")]
        private static extern IntPtr GetDirectArrayInternal(IntPtr InHandle);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetIndexArray@?$FbxLayerElementTemplate@VFbxColor@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@H@2@XZ")]
        private static extern IntPtr GetIndexArrayInternal(IntPtr InHandle);

        public FbxLayerElementUserData(FbxLayerContainer pOwner, string pName)
            : base(CreateFromObject(pOwner.Handle, pName))
        {
        }
    }
}
