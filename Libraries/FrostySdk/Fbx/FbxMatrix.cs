using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	public class FbxMatrix : FbxNative
	{
		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "??0FbxMatrix@fbxsdk@@QEAA@AEBVFbxAMatrix@1@@Z")]
		private static extern void ConvertFromAffineInternal(IntPtr pHandle, IntPtr pMatrix);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Get@FbxMatrix@fbxsdk@@QEBANHH@Z")]
		private static extern double GetInternal(IntPtr pHandle, int row, int column);

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

		public Matrix4x4 ToSharpDX()
		{
			return new Matrix4x4((float)GetInternal(pHandle, 0, 0), (float)GetInternal(pHandle, 0, 1), (float)GetInternal(pHandle, 0, 2), (float)GetInternal(pHandle, 0, 3), (float)GetInternal(pHandle, 1, 0), (float)GetInternal(pHandle, 1, 1), (float)GetInternal(pHandle, 1, 2), (float)GetInternal(pHandle, 1, 3), (float)GetInternal(pHandle, 2, 0), (float)GetInternal(pHandle, 2, 1), (float)GetInternal(pHandle, 2, 2), (float)GetInternal(pHandle, 2, 3), (float)GetInternal(pHandle, 3, 0), (float)GetInternal(pHandle, 3, 1), (float)GetInternal(pHandle, 3, 2), (float)GetInternal(pHandle, 3, 3));
		}
	}
}
