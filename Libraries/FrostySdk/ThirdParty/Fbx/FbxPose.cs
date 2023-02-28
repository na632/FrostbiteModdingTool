using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxPose : FbxObject
    {
        private IntPtr mType;

        public unsafe bool IsBindPose
        {
            get
            {
                return *(ushort*)(void*)mType == 98;
            }
            set
            {
                SetIsBindPoseInternal(pHandle, value);
            }
        }

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxPose@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxPose@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?SetIsBindPose@FbxPose@fbxsdk@@QEAAX_N@Z")]
        private static extern void SetIsBindPoseInternal(IntPtr pHandle, bool pIsBindPose);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Add@FbxPose@fbxsdk@@QEAAHPEAVFbxNode@2@AEBVFbxMatrix@2@_N2@Z")]
        private static extern int AddInternal(IntPtr pHandle, IntPtr pNode, IntPtr pMatrix, bool pLocalMatrix, bool pMultipleBindPose);

        public FbxPose(FbxManager manager, string name)
        {
            pHandle = CreateFromManager(manager.Handle, name);
            mType = pHandle + 120;
        }

        public FbxPose(IntPtr InHandle)
            : base(InHandle)
        {
            mType = pHandle + 120;
        }

        public FbxPose(FbxObject obj, string name)
        {
            pHandle = CreateFromObject(obj.Handle, name);
            mType = pHandle + 120;
        }

        public int Add(FbxNode node, FbxMatrix matrix, bool localMatrix = false, bool multipleBindPose = false)
        {
            return AddInternal(pHandle, node.Handle, matrix.Handle, localMatrix, multipleBindPose);
        }
    }
}
