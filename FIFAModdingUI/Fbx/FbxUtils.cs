using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal static class FbxUtils
	{
		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?FbxMalloc@fbxsdk@@YAPEAX_K@Z")]
		private static extern IntPtr FbxMallocInternal(ulong Size);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?FbxFree@fbxsdk@@YAXPEAX@Z")]
		private static extern void FbxFreeInternal(IntPtr Ptr);

		public static IntPtr FbxMalloc(ulong Size)
		{
			return FbxMallocInternal(Size);
		}

		public static void FbxFree(IntPtr Ptr)
		{
			FbxFreeInternal(Ptr);
		}

		public unsafe static string IntPtrToString(IntPtr InPtr)
		{
			string text = "";
			for (byte* ptr = (byte*)(void*)InPtr; *ptr != 0; ptr++)
			{
				string str = text;
				char c = (char)(*ptr);
				text = str + c;
			}
			return text;
		}
	}
}
