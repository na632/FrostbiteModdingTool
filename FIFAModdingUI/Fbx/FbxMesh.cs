using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxMesh : FbxGeometry
	{
		public int PolygonCount => GetPolygonCountInternal(pHandle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxMesh@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
		private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxMesh@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
		private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?BeginPolygon@FbxMesh@fbxsdk@@QEAAXHHH_N@Z")]
		private static extern void BeginPolygonInternal(IntPtr InHandle, int pMaterial, int pTexture, int pGroup, bool bLegacy);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?AddPolygon@FbxMesh@fbxsdk@@QEAAXHH@Z")]
		private static extern void AddPolygonInternal(IntPtr InHandle, int pIndex, int pTextureUVIndex);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?EndPolygon@FbxMesh@fbxsdk@@QEAAXXZ")]
		private static extern void EndPolygonInternal(IntPtr InHandle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetPolygonCount@FbxMesh@fbxsdk@@QEBAHXZ")]
		private static extern int GetPolygonCountInternal(IntPtr handle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetPolygonSize@FbxMesh@fbxsdk@@QEBAHH@Z")]
		private static extern int GetPolygonSizeInternal(IntPtr handle, int pIndex);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetPolygonVertex@FbxMesh@fbxsdk@@QEBAHHH@Z")]
		private static extern int GetPolygonIndexInternal(IntPtr handle, int pPolygonIndex, int pPositionInPolygon);

		public FbxMesh(FbxManager mgr, string name)
			: base(CreateFromManager(mgr.Handle, name))
		{
		}

		public FbxMesh(FbxObject obj, string name)
			: base(CreateFromObject(obj.Handle, name))
		{
		}

		public FbxMesh(FbxNodeAttribute attr)
			: base(attr.Handle)
		{
		}

		public void BeginPolygon(int materialIndex = -1)
		{
			BeginPolygonInternal(pHandle, materialIndex, -1, -1, bLegacy: true);
		}

		public void AddPolygon(int index)
		{
			AddPolygonInternal(pHandle, index, -1);
		}

		public void EndPolygon()
		{
			EndPolygonInternal(pHandle);
		}

		public int GetPolygonSize(int index)
		{
			return GetPolygonSizeInternal(pHandle, index);
		}

		public int GetPolygonIndex(int index, int position)
		{
			return GetPolygonIndexInternal(pHandle, index, position);
		}
	}
}
