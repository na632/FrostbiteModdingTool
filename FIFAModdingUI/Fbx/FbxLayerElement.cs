using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxLayerElement : FbxNative
	{
		internal enum EType
		{
			eUnknown,
			eNormal,
			eBiNormal,
			eTangent,
			eMaterial,
			ePolygonGroup,
			eUV,
			eVertexColor,
			eSmoothing,
			eVertexCrease,
			eEdgeCrease,
			eHole,
			eUserData,
			eVisibility,
			eTextureDiffuse,
			eTextureDiffuseFactor,
			eTextureEmissive,
			eTextureEmissiveFactor,
			eTextureAmbient,
			eTextureAmbientFactor,
			eTextureSpecular,
			eTextureSpecularFactor,
			eTextureShininess,
			eTextureNormalMap,
			eTextureBump,
			eTextureTransparency,
			eTextureTransparencyFactor,
			eTextureReflection,
			eTextureReflectionFactor,
			eTextureDisplacement,
			eTextureDisplacementVector,
			eTypeCount
		}

		private IntPtr mName;

		public EMappingMode MappingMode
		{
			get
			{
				return GetMappingModeInternal(pHandle);
			}
			set
			{
				SetMappingModeInternal(pHandle, value);
			}
		}

		public EReferenceMode ReferenceMode
		{
			get
			{
				return GetReferenceModeInternal(pHandle);
			}
			set
			{
				SetReferenceModeInternal(pHandle, value);
			}
		}

		public string Name
		{
			get
			{
				return FbxString.Get(mName);
			}
			set
			{
				mName = FbxString.Construct(value);
			}
		}

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetMappingMode@FbxLayerElement@fbxsdk@@QEAAXW4EMappingMode@12@@Z")]
		private static extern void SetMappingModeInternal(IntPtr InHandle, EMappingMode pMappingMode);

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetReferenceMode@FbxLayerElement@fbxsdk@@QEAAXW4EReferenceMode@12@@Z")]
		private static extern void SetReferenceModeInternal(IntPtr InHandle, EReferenceMode pReferenceMode);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetMappingMode@FbxLayerElement@fbxsdk@@QEBA?AW4EMappingMode@12@XZ")]
		private static extern EMappingMode GetMappingModeInternal(IntPtr handle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetReferenceMode@FbxLayerElement@fbxsdk@@QEBA?AW4EReferenceMode@12@XZ")]
		private static extern EReferenceMode GetReferenceModeInternal(IntPtr handle);

		private FbxLayerElement()
		{
		}

		public FbxLayerElement(IntPtr handle)
			: this()
		{
			pHandle = handle;
			mName = pHandle + 16;
		}
	}
}
