using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxGeometryConverter : FbxNative
    {
        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "??0FbxGeometryConverter@fbxsdk@@QEAA@PEAVFbxManager@1@@Z")]
        private static extern void CreateFromManager(IntPtr handle, IntPtr manager);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "??1FbxGeometryConverter@fbxsdk@@QEAA@XZ")]
        private static extern void DisposeInternal(IntPtr handle);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?ComputeEdgeSmoothingFromNormals@FbxGeometryConverter@fbxsdk@@QEBA_NPEAVFbxMesh@2@@Z")]
        private static extern bool ComputeEdgeSmoothingFromNormalsInternal(IntPtr handle, IntPtr mesh);

        public FbxGeometryConverter(FbxManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            pHandle = FbxUtils.FbxMalloc(16uL);
            CreateFromManager(pHandle, manager.Handle);
        }

        public bool ComputeEdgeSmoothingFromNormals(FbxMesh pMesh)
        {
            return ComputeEdgeSmoothingFromNormalsInternal(pHandle, pMesh.Handle);
        }

        public void Dispose()
        {
            DisposeInternal(pHandle);
            FbxUtils.FbxFree(pHandle);
            pHandle = IntPtr.Zero;
        }
    }
}
