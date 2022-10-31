using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	public class FbxSkin : FbxDeformer
	{
		public int ClusterCount => GetClusterCountInternal(pHandle);

		public IEnumerable<FbxCluster> Clusters
		{
			get
			{
				for (int i = 0; i < ClusterCount; i++)
				{
					yield return GetCluster(i);
				}
			}
		}

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxSkin@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
		private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxSkin@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
		private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?AddCluster@FbxSkin@fbxsdk@@QEAA_NPEAVFbxCluster@2@@Z")]
		private static extern bool AddClusterInternal(IntPtr pHandle, IntPtr pCluster);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetClusterCount@FbxSkin@fbxsdk@@QEBAHXZ")]
		private static extern int GetClusterCountInternal(IntPtr pHandle);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetCluster@FbxSkin@fbxsdk@@QEAAPEAVFbxCluster@2@H@Z")]
		private static extern IntPtr GetClusterInternal(IntPtr pHandle, int pIndex);

		public FbxSkin(IntPtr InHandle)
			: base(InHandle)
		{
		}

		public FbxSkin(FbxDeformer deformer)
			: this(deformer.Handle)
		{
		}

		public FbxSkin(FbxManager manager, string name)
			: this(CreateFromManager(manager.Handle, name))
		{
		}

		public FbxSkin(FbxObject obj, string name)
			: this(CreateFromObject(obj.Handle, name))
		{
		}

		public bool AddCluster(FbxCluster cluster)
		{
			return AddClusterInternal(pHandle, cluster.Handle);
		}

		public FbxCluster GetCluster(int index)
		{
			IntPtr clusterInternal = GetClusterInternal(pHandle, index);
			if (clusterInternal == IntPtr.Zero)
			{
				return null;
			}
			return new FbxCluster(clusterInternal);
		}
	}
}
