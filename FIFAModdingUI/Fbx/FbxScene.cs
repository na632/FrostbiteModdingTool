using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxScene : FbxDocument
	{
		public FbxGlobalSettings GlobalSettings => new FbxGlobalSettings(GetGlobalSettingsInternal(pHandle));

		public FbxNode RootNode => new FbxNode(GetRootNodeInternal(pHandle));

		public FbxDocumentInfo SceneInfo
		{
			get
			{
				if (!(GetSceneInfoInternal(pHandle) != IntPtr.Zero))
				{
					return null;
				}
				return new FbxDocumentInfo(GetSceneInfoInternal(pHandle));
			}
			set
			{
				SetSceneInfoInternal(pHandle, value.Handle);
			}
		}

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxScene@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
		private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxScene@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
		private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?SetSceneInfo@FbxScene@fbxsdk@@QEAAXPEAVFbxDocumentInfo@2@@Z")]
		private static extern void SetSceneInfoInternal(IntPtr InHandle, IntPtr pSceneInfo);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetGlobalSettings@FbxScene@fbxsdk@@QEBAAEBVFbxGlobalSettings@2@XZ")]
		private static extern IntPtr GetGlobalSettingsInternal(IntPtr InHandle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetRootNode@FbxScene@fbxsdk@@QEBAPEAVFbxNode@2@XZ")]
		private static extern IntPtr GetRootNodeInternal(IntPtr InHandle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetSceneInfo@FbxScene@fbxsdk@@QEAAPEAVFbxDocumentInfo@2@XZ")]
		private static extern IntPtr GetSceneInfoInternal(IntPtr handle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?AddPose@FbxScene@fbxsdk@@QEAA_NPEAVFbxPose@2@@Z")]
		private static extern bool AddPoseInternal(IntPtr pHandle, IntPtr pPose);

		public FbxScene(FbxManager Manager, string Name)
		{
			pHandle = CreateFromManager(Manager.Handle, Name);
		}

		public FbxScene(FbxObject Object, string Name)
		{
			pHandle = CreateFromObject(Object.Handle, Name);
		}

		public bool AddPose(FbxPose pose)
		{
			return AddPoseInternal(pHandle, pose.Handle);
		}
	}
}
