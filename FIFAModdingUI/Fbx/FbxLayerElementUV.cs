using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxLayerElementUV : FbxLayerElement
	{
		public FbxLayerElementArray DirectArray
		{
			get
			{
				IntPtr directArrayInternal = GetDirectArrayInternal(pHandle);
				if (directArrayInternal == IntPtr.Zero)
				{
					return null;
				}
				return new FbxLayerElementArray(directArrayInternal);
			}
		}

		public FbxLayerElementArray IndexArray
		{
			get
			{
				IntPtr indexArrayInternal = GetIndexArrayInternal(pHandle);
				if (indexArrayInternal == IntPtr.Zero)
				{
					return null;
				}
				return new FbxLayerElementArray(indexArrayInternal);
			}
		}

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxLayerElementUV@fbxsdk@@SAPEAV12@PEAVFbxLayerContainer@2@PEBD@Z")]
		private static extern IntPtr CreateFromObject(IntPtr pOwner, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetDirectArray@?$FbxLayerElementTemplate@VFbxVector2@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@VFbxVector2@fbxsdk@@@2@XZ")]
		private static extern IntPtr GetDirectArrayInternal(IntPtr InHandle);

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetIndexArray@?$FbxLayerElementTemplate@VFbxVector2@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@H@2@XZ")]
		private static extern IntPtr GetIndexArrayInternal(IntPtr InHandle);

		public FbxLayerElementUV(IntPtr handle)
			: base(handle)
		{
		}

		public FbxLayerElementUV(FbxLayerContainer pOwner, string pName)
			: this(CreateFromObject(pOwner.Handle, pName))
		{
		}
	}
}
