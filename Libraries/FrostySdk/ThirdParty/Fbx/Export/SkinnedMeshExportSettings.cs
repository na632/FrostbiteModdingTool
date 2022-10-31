using System.ComponentModel;

namespace FrostbiteSdk.Export
{
	public class SkinnedMeshExportSettings : MeshExportSettings
	{
		[DisplayName("Skeleton")]
		//[Editor("SkeletonEditor")]
		[Description("The skeleton to export with the mesh.")]
		public string SkeletonAsset
		{
			get;
			set;
		} = "";

	}
}
