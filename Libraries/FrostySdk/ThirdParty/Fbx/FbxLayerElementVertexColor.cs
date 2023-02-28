using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxLayerElementVertexColor : FbxLayerElement
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

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxLayerElementVertexColor@fbxsdk@@SAPEAV12@PEAVFbxLayerContainer@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pOwner, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetDirectArray@?$FbxLayerElementTemplate@VFbxColor@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@VFbxColor@fbxsdk@@@2@XZ")]
        private static extern IntPtr GetDirectArrayInternal(IntPtr InHandle);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetIndexArray@?$FbxLayerElementTemplate@VFbxColor@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@H@2@XZ")]
        private static extern IntPtr GetIndexArrayInternal(IntPtr InHandle);

        public FbxLayerElementVertexColor(IntPtr handle)
            : base(handle)
        {
        }

        public FbxLayerElementVertexColor(FbxLayerContainer pOwner, string pName)
            : this(CreateFromObject(pOwner.Handle, pName))
        {
        }
    }
}
