using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxSkeleton : FbxNodeAttribute
	{
		public new enum EType
		{
			eRoot,
			eLimb,
			eLimbNode,
			eEffector
		}

		private IntPtr size;

		public double Size
		{
			get
			{
				return FbxProperty.GetDouble(size);
			}
			set
			{
				FbxProperty.Set(size, value);
			}
		}

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxSkeleton@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
		private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxSkeleton@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
		private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?SetSkeletonType@FbxSkeleton@fbxsdk@@QEAAXW4EType@12@@Z")]
		private static extern void SetSkeletonTypeInternal(IntPtr inHandle, EType pSkeletonType);

		public FbxSkeleton(FbxManager Manager, string pName)
		{
			pHandle = CreateFromManager(Manager.Handle, pName);
			size = pHandle + 136;
		}

		public FbxSkeleton(IntPtr InHandle)
			: base(InHandle)
		{
			size = pHandle + 136;
		}

		public FbxSkeleton(FbxObject Object, string pName)
		{
			pHandle = CreateFromObject(Object.Handle, pName);
			size = pHandle + 136;
		}

		public void SetSkeletonType(EType skeletonType)
		{
			SetSkeletonTypeInternal(pHandle, skeletonType);
		}
	}
}
