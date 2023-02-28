using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxIOPluginRegistry : FbxNative
    {
        public int WriterFormatCount => GetWriterFormatCountInternal(pHandle);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetWriterFormatCount@FbxIOPluginRegistry@fbxsdk@@QEBAHXZ")]
        private static extern int GetWriterFormatCountInternal(IntPtr InHandle);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?WriterIsFBX@FbxIOPluginRegistry@fbxsdk@@QEBA_NH@Z")]
        private static extern bool WriterIsFBXInternal(IntPtr InHandle, int pFileFormat);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetWriterFormatDescription@FbxIOPluginRegistry@fbxsdk@@QEBAPEBDH@Z")]
        private static extern IntPtr GetWriterFormatDescriptionInternal(IntPtr InHandle, int pFileFormat);

        public FbxIOPluginRegistry(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public bool WriterIsFBX(int pFileFormat)
        {
            return WriterIsFBXInternal(pHandle, pFileFormat);
        }

        public string GetWriterFormatDescription(int pFileFormat)
        {
            IntPtr writerFormatDescriptionInternal = GetWriterFormatDescriptionInternal(pHandle, pFileFormat);
            if (writerFormatDescriptionInternal == IntPtr.Zero)
            {
                return "";
            }
            return FbxUtils.IntPtrToString(writerFormatDescriptionInternal);
        }
    }
}
