using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxCluster : FbxSubDeformer
	{
		public enum ELinkMode
		{
			eNormalize,
			eAdditive,
			eTotalOne
		}

		public int ControlPointIndicesCount => GetControlPointIndicesCountInternal(pHandle);

		public ELinkMode LinkMode
		{
			get
			{
				return GetLinkModeInternal(pHandle);
			}
			set
			{
				SetLinkModeInternal(pHandle, value);
			}
		}

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxCluster@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
		private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?Create@FbxCluster@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
		private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?SetLink@FbxCluster@fbxsdk@@QEAAXPEBVFbxNode@2@@Z")]
		private static extern void SetLinkInternal(IntPtr pHandle, IntPtr pNode);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetLinkMode@FbxCluster@fbxsdk@@QEBA?AW4ELinkMode@12@XZ")]
		private static extern ELinkMode GetLinkModeInternal(IntPtr handle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?SetLinkMode@FbxCluster@fbxsdk@@QEAAXW4ELinkMode@12@@Z")]
		private static extern void SetLinkModeInternal(IntPtr pHandle, ELinkMode pMode);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?AddControlPointIndex@FbxCluster@fbxsdk@@QEAAXHN@Z")]
		private static extern void AddControlPointIndexInternal(IntPtr pHandle, int pIndex, double pWeight);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetControlPointIndicesCount@FbxCluster@fbxsdk@@QEBAHXZ")]
		private static extern int GetControlPointIndicesCountInternal(IntPtr handle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetControlPointIndices@FbxCluster@fbxsdk@@QEBAPEAHXZ")]
		private static extern IntPtr GetControlPointIndicesInternal(IntPtr handle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetControlPointWeights@FbxCluster@fbxsdk@@QEBAPEANXZ")]
		private static extern IntPtr GetControlPointWeightsInternal(IntPtr handle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?SetTransformLinkMatrix@FbxCluster@fbxsdk@@QEAAXAEBVFbxAMatrix@2@@Z")]
		private static extern void SetTransformLinkMatrixInternal(IntPtr pHandle, IntPtr pMatrix);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetLink@FbxCluster@fbxsdk@@QEAAPEAVFbxNode@2@XZ")]
		private static extern IntPtr GetLinkInternal(IntPtr pHandle);

		public FbxCluster(IntPtr InHandle)
			: base(InHandle)
		{
		}

		public FbxCluster(FbxManager manager, string name)
			: this(CreateFromManager(manager.Handle, name))
		{
		}

		public FbxCluster(FbxObject obj, string name)
			: this(CreateFromObject(obj.Handle, name))
		{
		}

		public void SetLink(FbxNode node)
		{
			SetLinkInternal(pHandle, node.Handle);
		}

		public FbxNode GetLink()
		{
			return new FbxNode(GetLinkInternal(pHandle));
		}

		public void SetLinkMode(ELinkMode mode)
		{
			SetLinkModeInternal(pHandle, mode);
		}

		public void AddControlPointIndex(int index, double weight)
		{
			AddControlPointIndexInternal(pHandle, index, weight);
		}

		public void SetTransformLinkMatrix(FbxAMatrix matrix)
		{
			SetTransformLinkMatrixInternal(pHandle, matrix.Handle);
		}

		public int[] GetControlPointIndices()
		{
			IntPtr controlPointIndicesInternal = GetControlPointIndicesInternal(pHandle);
			if (controlPointIndicesInternal == IntPtr.Zero)
			{
				return null;
			}
			int[] array = new int[ControlPointIndicesCount];
			Marshal.Copy(controlPointIndicesInternal, array, 0, ControlPointIndicesCount);
			return array;
		}

		public double[] GetControlPointWeights()
		{
			IntPtr controlPointWeightsInternal = GetControlPointWeightsInternal(pHandle);
			if (controlPointWeightsInternal == IntPtr.Zero)
			{
				return null;
			}
			double[] array = new double[ControlPointIndicesCount];
			Marshal.Copy(controlPointWeightsInternal, array, 0, ControlPointIndicesCount);
			return array;
		}
	}
}
