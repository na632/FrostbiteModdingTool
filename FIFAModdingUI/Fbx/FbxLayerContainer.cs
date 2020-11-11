using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxLayerContainer : FbxNodeAttribute
	{
		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetLayer@FbxLayerContainer@fbxsdk@@QEBAPEBVFbxLayer@2@H@Z")]
		private static extern IntPtr GetLayerInternal(IntPtr InHandle, int pIndex);

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?CreateLayer@FbxLayerContainer@fbxsdk@@QEAAHXZ")]
		private static extern int CreateLayerInternal(IntPtr InHandle);

		public FbxLayerContainer()
		{
		}

		public FbxLayerContainer(IntPtr InHandle)
			: base(InHandle)
		{
		}

		public FbxLayer GetLayer(int pIndex)
		{
			IntPtr layerInternal = GetLayerInternal(pHandle, pIndex);
			if (layerInternal == IntPtr.Zero)
			{
				return null;
			}
			return new FbxLayer(layerInternal);
		}

		public int CreateLayer()
		{
			return CreateLayerInternal(pHandle);
		}
	}
}
