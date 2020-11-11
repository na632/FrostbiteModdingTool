using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxMatrix : FbxNative
	{
		[DllImport("thirdparty/libfbxsdk", EntryPoint = "??0FbxMatrix@fbxsdk@@QEAA@AEBVFbxAMatrix@1@@Z")]
		private static extern void ConvertFromAffineInternal(IntPtr pHandle, IntPtr pMatrix);

		public FbxMatrix(IntPtr ptr)
			: base(ptr)
		{
		}

		public FbxMatrix(FbxAMatrix affineMatrix)
		{
			pHandle = FbxUtils.FbxMalloc(128uL);
			ConvertFromAffineInternal(pHandle, affineMatrix.Handle);
			bNeedsFreeing = true;
		}
	}
}
