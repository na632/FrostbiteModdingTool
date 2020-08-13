using System;
using System.Runtime.InteropServices;

namespace FrostySdk
{
	internal static class Oodle
	{
		public delegate int DecompressFunc(IntPtr srcBuffer, long srcSize, IntPtr dstBuffer, long dstSize, int a5 = 0, int a6 = 0, long a7 = 0L, long a8 = 0L, long a9 = 0L, long a10 = 0L, long a11 = 0L, long a12 = 0L, long a13 = 0L, int a14 = 3);

		public delegate long CompressFunc(int cmpCode, IntPtr srcBuffer, long srcSize, IntPtr cmpBuffer, long cmpSize, long dict = 0L, long dictSize = 0L);

		public delegate long CompressFunc2(int cmpCode, IntPtr srcBuffer, long srcSize, IntPtr cmpBuffer, long cmpSize, long dict = 0L, long dictSize = 0L, long a8 = 0L, long a9 = 0L, long a10 = 0L);

		public delegate long MemorySizeNeededFunc(int a1, long a2);

		public static DecompressFunc Decompress;

		public static CompressFunc Compress;

		public static CompressFunc2 Compress2;

		public static MemorySizeNeededFunc MemorySizeNeeded;

		internal static LoadLibraryHandle handle;

		internal static void Bind(string basePath)
		{
			if (ProfilesLibrary.DataVersion != 20170929 && ProfilesLibrary.DataVersion != 20180914 && ProfilesLibrary.DataVersion != 20181207 && ProfilesLibrary.DataVersion != 20190911 && ProfilesLibrary.DataVersion != 20190905)
			{
				return;
			}
			string lib = basePath + "/oo2core_4_win64.dll";
			if (ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905)
			{
				lib = basePath + "/oo2core_6_win64.dll";
			}
			else if (ProfilesLibrary.DataVersion == 20181207)
			{
				lib = basePath + "/oo2core_7_win64.dll";
			}
			handle = new LoadLibraryHandle(lib);
			if (!(handle == IntPtr.Zero))
			{
				Decompress = Marshal.GetDelegateForFunctionPointer<DecompressFunc>(Kernel32.GetProcAddress(handle, "OodleLZ_Decompress"));
				Compress = Marshal.GetDelegateForFunctionPointer<CompressFunc>(Kernel32.GetProcAddress(handle, "OodleLZ_Compress"));
				if (ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905)
				{
					Compress2 = Marshal.GetDelegateForFunctionPointer<CompressFunc2>(Kernel32.GetProcAddress(handle, "OodleLZ_Compress"));
				}
				MemorySizeNeeded = Marshal.GetDelegateForFunctionPointer<MemorySizeNeededFunc>(Kernel32.GetProcAddress(handle, "OodleLZDecoder_MemorySizeNeeded"));
			}
		}
	}
}
