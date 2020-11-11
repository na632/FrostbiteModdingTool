using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxIOBase : FbxObject
	{
		public FbxStatus Status => new FbxStatus(GetStatusInternal(pHandle));

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetStatus@FbxIOBase@fbxsdk@@QEAAAEAVFbxStatus@2@XZ")]
		private static extern IntPtr GetStatusInternal(IntPtr handle);
	}
}
