using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxDocument : FbxCollection
	{
		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetDocumentInfo@FbxDocument@fbxsdk@@QEBAPEAVFbxDocumentInfo@2@XZ")]
		protected static extern IntPtr GetDocumentInfoInternal(IntPtr handle);

		public FbxDocument()
		{
		}

		public FbxDocument(IntPtr InHandle)
			: base(InHandle)
		{
		}
	}
}
