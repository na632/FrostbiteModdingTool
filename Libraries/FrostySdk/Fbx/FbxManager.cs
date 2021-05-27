using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	public class FbxManager : FbxNative, IDisposable
	{
		public FbxIOPluginRegistry IOPluginRegistry => new FbxIOPluginRegistry(GetIOPluginRegistryInternal(pHandle));

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxManager@fbxsdk@@SAPEAV12@XZ")]
		private static extern IntPtr CreateInternal();

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Destroy@FbxManager@fbxsdk@@UEAAXXZ")]
		private static extern IntPtr DestroyInternal(IntPtr InHandle);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?SetIOSettings@FbxManager@fbxsdk@@UEAAXPEAVFbxIOSettings@2@@Z")]
		private static extern void SetIOSettingsInternal(IntPtr handle, IntPtr pIOSettings);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetIOSettings@FbxManager@fbxsdk@@UEBAPEAVFbxIOSettings@2@XZ")]
		private static extern IntPtr GetIOSettingsInternal(IntPtr handle);

		[DllImport("ThirdParty/libfbxsdk", CharSet = CharSet.Unicode, EntryPoint = "?GetVersion@FbxManager@fbxsdk@@SAPEBD_N@Z")]
		private static extern IntPtr GetVersionInternal(IntPtr InHandle, bool pFull);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetFileFormatVersion@FbxManager@fbxsdk@@SAXAEAH00@Z")]
		private static extern void GetFileFormatVersionInternal(ref int pMajor, ref int pMinor, ref int pRevision);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetIOPluginRegistry@FbxManager@fbxsdk@@QEBAPEAVFbxIOPluginRegistry@2@XZ")]
		private static extern IntPtr GetIOPluginRegistryInternal(IntPtr InHandle);

		public static void GetFileFormatVersion(out int pMajor, out int pMinor, out int pRevision)
		{
			pMajor = 0;
			pMinor = 0;
			pRevision = 0;
			GetFileFormatVersionInternal(ref pMajor, ref pMinor, ref pRevision);
		}

		public FbxManager()
		{
			pHandle = CreateInternal();
		}

		~FbxManager()
		{
			Dispose(bDisposing: false);
		}

		public void SetIOSettings(FbxIOSettings settings)
		{
			SetIOSettingsInternal(pHandle, settings.Handle);
		}

		public FbxIOSettings GetIOSettings()
		{
			return new FbxIOSettings(GetIOSettingsInternal(pHandle));
		}

		public string GetVersion(bool pFull = true)
		{
			return FbxUtils.IntPtrToString(GetVersionInternal(pHandle, pFull));
		}

		public void Dispose()
		{
			Dispose(bDisposing: true);
		}

		protected virtual void Dispose(bool bDisposing)
		{
			if (pHandle != IntPtr.Zero)
			{
				DestroyInternal(pHandle);
				pHandle = IntPtr.Zero;
			}
			if (bDisposing)
			{
				GC.SuppressFinalize(this);
			}
		}
	}
}
