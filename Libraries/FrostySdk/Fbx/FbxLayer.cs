using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	public class FbxLayer : FbxNative
	{
		[DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetNormals@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementNormal@2@@Z")]
		private static extern void SetNormalsInternal(IntPtr InHandle, IntPtr pNormals);

		[DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetTangents@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementTangent@2@@Z")]
		private static extern void SetTangentsInternal(IntPtr InHandle, IntPtr pTangents);

		[DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetBinormals@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementBinormal@2@@Z")]
		private static extern void SetBinormalsInternal(IntPtr InHandle, IntPtr pBinormals);

		[DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetUVs@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementUV@2@W4EType@FbxLayerElement@2@@Z")]
		private static extern void SetUVsInternal(IntPtr InHandle, IntPtr pUVs, FbxLayerElement.EType pTypeIdentifier);

		[DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetVertexColors@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementVertexColor@2@@Z")]
		private static extern void SetVertexColorsInternal(IntPtr InHandle, IntPtr pVertexColors);

		[DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetUserData@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementUserData@2@@Z")]
		private static extern void SetUserDataInternal(IntPtr InHandle, IntPtr pUserData);

		public FbxLayer(IntPtr InHandle)
			: base(InHandle)
		{
		}

		public void SetNormals(FbxLayerElementNormal pNormals)
		{
			SetNormalsInternal(pHandle, pNormals.Handle);
		}

		public void SetTangents(FbxLayerElementTangent pTangents)
		{
			SetTangentsInternal(pHandle, pTangents.Handle);
		}

		public void SetBinormals(FbxLayerElementBinormal pBinormals)
		{
			SetBinormalsInternal(pHandle, pBinormals.Handle);
		}

		public void SetUVs(FbxLayerElementUV pUVs)
		{
			SetUVsInternal(pHandle, pUVs.Handle, FbxLayerElement.EType.eTextureDiffuse);
		}

		public void SetVertexColors(FbxLayerElementVertexColor pVertexColors)
		{
			SetVertexColorsInternal(pHandle, pVertexColors.Handle);
		}

		public void SetUserData(FbxLayerElementUserData pUserData)
		{
			SetUserDataInternal(pHandle, pUserData.Handle);
		}
	}
}
