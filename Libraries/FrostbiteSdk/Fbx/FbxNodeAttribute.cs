using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Mathematics;

namespace FrostbiteSdk
{
	public class FbxNodeAttribute : FbxObject
	{
		public enum EType
		{
			eUnknown,
			eNull,
			eMarker,
			eSkeleton,
			eMesh,
			eNurbs,
			ePatch,
			eCamera,
			eCameraStereo,
			eCameraSwitcher,
			eLight,
			eOpticalReference,
			eOpticalMarker,
			eNurbsCurve,
			eTrimNurbsSurface,
			eBoundary,
			eNurbsSurface,
			eShape,
			eLODGroup,
			eSubDiv,
			eCachedEffect,
			eLine
		}

		private delegate EType GetAttributeTypeDelegate(IntPtr handle);

		private GetAttributeTypeDelegate GetAttributeTypeInternal;

		private IntPtr color;

		public EType AttributeType => GetAttributeTypeInternal(pHandle);

		public int NodeCount => GetNodeCountInternal(pHandle);

		public Color4 Color
		{
			get
			{
				Vector3 @double = FbxProperty.GetDouble3(color);
				return new Color4(@double.X, @double.Y, @double.Z, 1f);
			}
		}

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetNodeCount@FbxNodeAttribute@fbxsdk@@QEBAHXZ")]
		private static extern int GetNodeCountInternal(IntPtr handle);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetNode@FbxNodeAttribute@fbxsdk@@QEBAPEAVFbxNode@2@H@Z")]
		private static extern IntPtr GetNodeInternal(IntPtr handle, int pIndex);

		public FbxNodeAttribute()
		{
		}

		public FbxNodeAttribute(IntPtr InHandle)
			: base(InHandle)
		{
			GetAttributeTypeInternal = Marshal.GetDelegateForFunctionPointer<GetAttributeTypeDelegate>(Marshal.ReadIntPtr(vTable + 184));
			color = pHandle + 120;
		}

		public FbxNode GetNode(int index)
		{
			if (index >= NodeCount)
			{
				return null;
			}
			IntPtr nodeInternal = GetNodeInternal(pHandle, index);
			if (nodeInternal == IntPtr.Zero)
			{
				return null;
			}
			return new FbxNode(nodeInternal);
		}
	}
}
