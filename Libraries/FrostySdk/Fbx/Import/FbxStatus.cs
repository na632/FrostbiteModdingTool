using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	public class FbxStatus : FbxNative
	{
		public string ErrorString => FbxUtils.IntPtrToString(GetErrorStringInternal(pHandle));

		public EStatusCode Code => GetCodeInternal(pHandle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetErrorString@FbxStatus@fbxsdk@@QEBAPEBDXZ")]
		private static extern IntPtr GetErrorStringInternal(IntPtr handle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetCode@FbxStatus@fbxsdk@@QEBA?AW4EStatusCode@12@XZ")]
		private static extern EStatusCode GetCodeInternal(IntPtr handle);

		public FbxStatus(IntPtr handle)
		{
			pHandle = handle;
		}
	}
}
