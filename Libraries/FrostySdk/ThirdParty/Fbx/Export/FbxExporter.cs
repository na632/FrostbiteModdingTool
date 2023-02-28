using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk.FbxExporter
{
    public class FbxExporter : FbxObject
    {
        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxExporter@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPUTF8Str)] string pName);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Initialize@FbxExporter@fbxsdk@@UEAA_NPEBDHPEAVFbxIOSettings@2@@Z")]
        private static extern bool InitializeInternal(IntPtr InHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string pFileName, int pFileFormat, IntPtr pIOSettings);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Export@FbxExporter@fbxsdk@@QEAA_NPEAVFbxDocument@2@_N@Z")]
        private static extern bool ExportInternal(IntPtr InHandle, IntPtr pDocument, bool pNonBlocking);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetFileExportVersion@FbxExporter@fbxsdk@@QEAA_NVFbxString@2@W4ERenamingMode@FbxSceneRenamer@2@@Z")]
        private static extern bool SetFileExportVersionInternal(IntPtr InHandle, IntPtr pVersion, int pRenamingMode);

        public FbxExporter(FbxManager Manager, string Name)
        {
            pHandle = CreateFromManager(Manager.Handle, Name);
        }

        public bool Initialize(string pFileName, int pFileFormat = -1, FbxIOSettings pIOSettings = null)
        {
            IntPtr pIOSettings2 = ((pIOSettings != null) ? pIOSettings.Handle : IntPtr.Zero);
            return InitializeInternal(pHandle, pFileName, pFileFormat, pIOSettings2);
        }

        public bool Export(FbxDocument pDocument, bool pNonBlocking = false)
        {
            return ExportInternal(pHandle, pDocument.Handle, pNonBlocking);
        }

        public bool SetFileExportVersion(string pVersion)
        {
            IntPtr pVersion2 = FbxString.Construct(pVersion);
            return SetFileExportVersionInternal(pHandle, pVersion2, 0);
        }
    }

}
