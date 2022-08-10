using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	public class FbxGeometryBase : FbxLayerContainer
	{
		public int ControlPointsCount => GetControlsPointsCount(pHandle);

		public int ElementNormalCount => GetElementNormalCountInternal(pHandle);

		public int ElementTangentCount => GetElementTangentCountInternal(pHandle);

		public int ElementBinormalCount => GetElementBinormalCountInternal(pHandle);

		public int ElementUVCount => GetElementUVCountInternal(pHandle, FbxLayerElement.EType.eUnknown);

		public int ElementVertexColorCount => GetElementVertexColorCountInternal(pHandle);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?InitControlPoints@FbxGeometryBase@fbxsdk@@UEAAXH@Z")]
		private static extern void InitControlPointsInternal(IntPtr InHandle, int pCount);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetControlPoints@FbxGeometryBase@fbxsdk@@UEBAPEAVFbxVector4@2@PEAVFbxStatus@2@@Z")]
		private static extern IntPtr GetControlPointsInternal(IntPtr InHandle, IntPtr pStatus);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetControlPointsCount@FbxGeometryBase@fbxsdk@@UEBAHXZ")]
		private static extern int GetControlsPointsCount(IntPtr handle);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetElementNormalCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
		private static extern int GetElementNormalCountInternal(IntPtr handle);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetElementTangentCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
		private static extern int GetElementTangentCountInternal(IntPtr handle);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetElementBinormalCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
		private static extern int GetElementBinormalCountInternal(IntPtr handle);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetElementUVCount@FbxGeometryBase@fbxsdk@@QEBAHW4EType@FbxLayerElement@2@@Z")]
		private static extern int GetElementUVCountInternal(IntPtr handle, FbxLayerElement.EType type);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetElementVertexColorCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
		private static extern int GetElementVertexColorCountInternal(IntPtr handle);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetElementNormal@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementNormal@2@H@Z")]
		private static extern IntPtr GetElementNormalInternal(IntPtr handle, int pIndex);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetElementTangent@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementTangent@2@H@Z")]
		private static extern IntPtr GetElementTangentInternal(IntPtr handle, int pIndex);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetElementBinormal@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementBinormal@2@H@Z")]
		private static extern IntPtr GetElementBinormalInternal(IntPtr handle, int pIndex);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetElementUV@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementUV@2@HW4EType@FbxLayerElement@2@@Z")]
		private static extern IntPtr GetElementUVInternal(IntPtr handle, int pIndex, FbxLayerElement.EType pType);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetElementVertexColor@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementVertexColor@2@H@Z")]
		private static extern IntPtr GetElementVertexColorInternal(IntPtr handle, int pIndex);

		public FbxGeometryBase()
		{
		}

		public FbxGeometryBase(IntPtr InHandle)
			: base(InHandle)
		{
		}

		public void InitControlPoints(int pCount)
		{
			InitControlPointsInternal(pHandle, pCount);
		}

		public IntPtr GetControlPoints()
		{
			return GetControlPointsInternal(pHandle, IntPtr.Zero);
		}

		public FbxLayerElementTangent GetElementTangent(int index)
		{
			IntPtr elementTangentInternal = GetElementTangentInternal(pHandle, index);
			if (elementTangentInternal == IntPtr.Zero)
			{
				return null;
			}
			return new FbxLayerElementTangent(elementTangentInternal);
		}

		public FbxLayerElementBinormal GetElementBinormal(int index)
		{
			IntPtr elementBinormalInternal = GetElementBinormalInternal(pHandle, index);
			if (elementBinormalInternal == IntPtr.Zero)
			{
				return null;
			}
			return new FbxLayerElementBinormal(elementBinormalInternal);
		}

		public FbxLayerElementNormal GetElementNormal(int index)
		{
			IntPtr elementNormalInternal = GetElementNormalInternal(pHandle, index);
			if (elementNormalInternal == IntPtr.Zero)
			{
				return null;
			}
			return new FbxLayerElementNormal(elementNormalInternal);
		}

		public FbxLayerElementUV GetElementUV(int index, FbxLayerElement.EType type)
		{
			IntPtr elementUVInternal = GetElementUVInternal(pHandle, index, type);
			if (elementUVInternal == IntPtr.Zero)
			{
				return null;
			}
			return new FbxLayerElementUV(elementUVInternal);
		}

		public FbxLayerElementUV GetElementUV(string name)
		{
			for (int i = 0; i < ElementUVCount; i++)
			{
				FbxLayerElementUV elementUV = GetElementUV(i, FbxLayerElement.EType.eUnknown);
				if (elementUV.Name == name)
				{
					return elementUV;
				}
			}
			return null;
		}

		public FbxLayerElementVertexColor GetElementVertexColor(int index)
		{
			IntPtr elementVertexColorInternal = GetElementVertexColorInternal(pHandle, index);
			if (elementVertexColorInternal == IntPtr.Zero)
			{
				return null;
			}
			return new FbxLayerElementVertexColor(elementVertexColorInternal);
		}

		public FbxLayerElementVertexColor GetElementVertexColor(string name)
		{
			for (int i = 0; i < ElementVertexColorCount; i++)
			{
				FbxLayerElementVertexColor elementVertexColor = GetElementVertexColor(i);
				if (elementVertexColor.Name == name)
				{
					return elementVertexColor;
				}
			}
			return null;
		}
	}
}
