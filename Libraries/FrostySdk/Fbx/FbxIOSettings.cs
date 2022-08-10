using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	public class FbxIOSettings : FbxObject
	{
		public const string IOSROOT = "IOSRoot";

		public const string IOSN_EXPORT = "Export";

		public const string IOSN_ADV_OPT_GRP = "AdvOptGrp";

		public const string IOSN_FBX = "Fbx";

		public const string IOSN_MATERIAL = "Material";

		public const string IOSN_TEXTURE = "Texture";

		public const string IOSN_GLOBAL_SETTINGS = "Global_Settings";

		public const string IOSN_SHAPE = "Shape";

		public const string EXP_ADV_OPT_GRP = "Export|AdvOptGrp";

		public const string EXP_FBX = "Export|AdvOptGrp|Fbx";

		public const string EXP_FBX_MATERIAL = "Export|AdvOptGrp|Fbx|Material";

		public const string EXP_FBX_TEXTURE = "Export|AdvOptGrp|Fbx|Texture";

		public const string EXP_FBX_GLOBAL_SETTINGS = "Export|AdvOptGrp|Fbx|Global_Settings";

		public const string EXP_FBX_SHAPE = "Export|AdvOptGrp|Fbx|Shape";

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxIOSettings@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
		private static extern IntPtr CreateInternal(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?SetBoolProp@FbxIOSettings@fbxsdk@@QEAAXPEBD_N@Z")]
		private static extern void SetBoolPropInternal(IntPtr InHandle, [MarshalAs(UnmanagedType.LPStr)] string pName, bool pValue);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetBoolProp@FbxIOSettings@fbxsdk@@QEBA_NPEBD_N@Z")]
		private static extern bool GetBoolPropInternal(IntPtr InHandle, [MarshalAs(UnmanagedType.LPStr)] string pName);

		public FbxIOSettings()
		{
		}

		public FbxIOSettings(IntPtr InHandle)
			: base(InHandle)
		{
		}

		public FbxIOSettings(FbxManager Manager, string Name)
		{
			pHandle = CreateInternal(Manager.Handle, Name);
		}

		public void SetBoolProp(string pName, bool pValue)
		{
			SetBoolPropInternal(pHandle, pName, pValue);
		}

		public bool GetBoolProp(string pName)
		{
			return GetBoolPropInternal(pHandle, pName);
		}
	}
}
