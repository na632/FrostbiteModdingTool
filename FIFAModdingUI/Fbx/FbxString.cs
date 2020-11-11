using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxString
	{
		[DllImport("thirdparty/libfbxsdk", EntryPoint = "??0FbxString@fbxsdk@@QEAA@PEBD@Z")]
		private static extern void ConstructInternal(IntPtr Handle, [MarshalAs(UnmanagedType.LPStr)] string pParam);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "??4FbxString@fbxsdk@@QEAAAEBV01@PEBD@Z")]
		private static extern void AssignInternal(IntPtr Handle, [MarshalAs(UnmanagedType.LPStr)] string pParam);

		public static IntPtr Construct(string InitialValue = "")
		{
			IntPtr intPtr = FbxUtils.FbxMalloc(8uL);
			ConstructInternal(intPtr, InitialValue);
			return intPtr;
		}

		public static void Assign(IntPtr InHandle, string pParam)
		{
			AssignInternal(InHandle, pParam);
		}

		public unsafe static string Get(IntPtr InHandle)
		{
			IntPtr intPtr = new IntPtr(*(long*)(void*)InHandle);
			if (!(intPtr != IntPtr.Zero))
			{
				return "";
			}
			return FbxUtils.IntPtrToString(intPtr);
		}
	}
}
