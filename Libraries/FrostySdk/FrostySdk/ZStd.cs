using FrostbiteSdk;
using System;
using System.Runtime.InteropServices;

namespace FrostySdk
{
	public static class ZStd
	{
		public enum ZSTD_error
		{
			ZSTD_error_no_error = 0,
			ZSTD_error_GENERIC = 1,
			ZSTD_error_prefix_unknown = 10,
			ZSTD_error_version_unsupported = 12,
			ZSTD_error_frameParameter_unsupported = 14,
			ZSTD_error_frameParameter_windowTooLarge = 0x10,
			ZSTD_error_corruption_detected = 20,
			ZSTD_error_checksum_wrong = 22,
			ZSTD_error_dictionary_corrupted = 30,
			ZSTD_error_dictionary_wrong = 0x20,
			ZSTD_error_dictionaryCreation_failed = 34,
			ZSTD_error_parameter_unsupported = 40,
			ZSTD_error_parameter_outOfBound = 42,
			ZSTD_error_tableLog_tooLarge = 44,
			ZSTD_error_maxSymbolValue_tooLarge = 46,
			ZSTD_error_maxSymbolValue_tooSmall = 48,
			ZSTD_error_stage_wrong = 60,
			ZSTD_error_init_missing = 62,
			ZSTD_error_memory_allocation = 0x40,
			ZSTD_error_dstSize_tooSmall = 70,
			ZSTD_error_srcSize_wrong = 72,
			ZSTD_error_frameIndex_tooLarge = 100,
			ZSTD_error_seekableIO = 102,
			ZSTD_error_maxCode = 120
		}

		public delegate ulong DecompressFunc(IntPtr outputBuffer, ulong outputSize, IntPtr inputBuffer, ulong inputSize);

		public delegate IntPtr CreateFunc();

		public delegate ulong FreeFunc(IntPtr handle);

		public delegate ulong DecompressUsingDictFunc(IntPtr dctx, IntPtr outBuf, ulong outSize, IntPtr inBuf, ulong inSize, IntPtr dict);

		public delegate ulong CompressFunc(IntPtr Dst, ulong DstCapacity, IntPtr Src, ulong SrcSize, int CompressionLevel);

		public delegate ulong CompressBoundFunc(ulong srcSize);

		public delegate bool IsErrorFunc(ulong errorCode);

		public delegate ZSTD_error GetErrorCodeFunc(ulong errorCode);

		public delegate IntPtr GetErrorNameFunc(ulong errorCode);

		public delegate IntPtr CreateDigestedDictFunc(IntPtr dictBuffer, int dictSize);

		public delegate ulong FreeDigestedDictFunc(IntPtr dict);

		public static DecompressFunc Decompress;

		public static CreateFunc Create;

		public static FreeFunc Free;

		public static DecompressUsingDictFunc DecompressUsingDict;

		public static CompressFunc Compress;

		public static CompressBoundFunc CompressBound;

		public static IsErrorFunc IsError;

		public static GetErrorCodeFunc GetErrorCode;

		public static GetErrorNameFunc GetErrorName;

		public static CreateDigestedDictFunc CreateDigestedDict;

		public static FreeDigestedDictFunc FreeDigestedDict;

		internal static LoadLibraryHandle handle;

		private static byte[] digestedDict;

		public static void Bind()
		{
			if (handle != null)
				return;

			//if ((ProfilesLibrary.DataVersion != 20170321
			//	&& ProfilesLibrary.DataVersion != 20160927
			//	&& ProfilesLibrary.DataVersion != 20170929
			//	&& ProfilesLibrary.DataVersion != 20171117
			//	&& ProfilesLibrary.DataVersion != 20180807
			//	&& ProfilesLibrary.DataVersion != 20180914
			//	&& ProfilesLibrary.DataVersion != 20180628
			//	&& !ProfilesLibrary.IsFIFA20DataVersion()
			//	//&& ProfilesLibrary.DataVersion != 20200831
			//	&& !ProfilesLibrary.IsMadden21DataVersion()
			//	&& !ProfilesLibrary.IsFIFA21DataVersion())
			//	|| Compress != null)
			//{
			//	return;
			//}
			string parentDirectory = Utilities.ApplicationDirectory + "thirdparty/";
			//string lib = "libzstd.1.1.5.dll";
			//if (ProfilesLibrary.DataVersion == 20180914 
			//	|| ProfilesLibrary.IsFIFA20DataVersion()
			//	//|| ProfilesLibrary.IsMadden21DataVersion()
			//	//|| ProfilesLibrary.IsFIFA21DataVersion()
			//	)
			//{
			//	lib = "libzstd.1.3.4.dll";
			//}
   //         else if (ProfilesLibrary.IsFIFA21DataVersion()
			//	|| ProfilesLibrary.IsMadden21DataVersion()
			//	)
			//{
                string lib = "libzstd.1.5.0.dll";
            //}
            handle = new LoadLibraryHandle(parentDirectory + lib);
			if (handle == IntPtr.Zero)
			{
				return;
			}
			Create = Marshal.GetDelegateForFunctionPointer<CreateFunc>(Kernel32.GetProcAddress(handle, "ZSTD_createDCtx"));
			Free = Marshal.GetDelegateForFunctionPointer<FreeFunc>(Kernel32.GetProcAddress(handle, "ZSTD_freeDCtx"));
			Decompress = Marshal.GetDelegateForFunctionPointer<DecompressFunc>(Kernel32.GetProcAddress(handle, "ZSTD_decompress"));
			Compress = Marshal.GetDelegateForFunctionPointer<CompressFunc>(Kernel32.GetProcAddress(handle, "ZSTD_compress"));
			CompressBound = Marshal.GetDelegateForFunctionPointer<CompressBoundFunc>(Kernel32.GetProcAddress(handle, "ZSTD_compressBound"));
			IsError = Marshal.GetDelegateForFunctionPointer<IsErrorFunc>(Kernel32.GetProcAddress(handle, "ZSTD_isError"));
			if (ProfilesLibrary.DataVersion != 20160927)
			{
				GetErrorCode = Marshal.GetDelegateForFunctionPointer<GetErrorCodeFunc>(Kernel32.GetProcAddress(handle, "ZSTD_getErrorCode"));
				GetErrorName = Marshal.GetDelegateForFunctionPointer<GetErrorNameFunc>(Kernel32.GetProcAddress(handle, "ZSTD_getErrorName"));
				if (ProfilesLibrary.DataVersion == 20170929
					|| ProfilesLibrary.DataVersion == 20180914 
					|| ProfilesLibrary.IsFIFA20DataVersion() // FIFA 20
					|| ProfilesLibrary.IsFIFA21DataVersion() // FIFA 21
					|| ProfilesLibrary.IsFIFA22DataVersion() // FIFA 22

					)
				{
					DecompressUsingDict = Marshal.GetDelegateForFunctionPointer<DecompressUsingDictFunc>(Kernel32.GetProcAddress(handle, "ZSTD_decompress_usingDDict"));
					CreateDigestedDict = Marshal.GetDelegateForFunctionPointer<CreateDigestedDictFunc>(Kernel32.GetProcAddress(handle, "ZSTD_createDDict"));
					FreeDigestedDict = Marshal.GetDelegateForFunctionPointer<FreeDigestedDictFunc>(Kernel32.GetProcAddress(handle, "ZSTD_freeDDict"));
				}
			}
		}

		internal static void SetDictionary(byte[] data)
		{
			digestedDict = data;
		}

		internal static byte[] GetDictionary()
		{
			return digestedDict;
		}
	}
}
