using FrostySdk.Managers;

namespace Fifa_Tool.Exporters
{
	public class MeshSetExporter
	{
		public MeshSet LoadMeshSet(AssetManager assetManager, object rootObject)
		{
			ulong resRid = ((dynamic)rootObject).MeshSetResource;
			MeshSet meshSet = new MeshSet(assetManager.GetRes(assetManager.GetResEntry(resRid)));
			//_ = assetManager.GetEbxEntry(meshSet.FullName).Guid;
			return meshSet;
		}
	}
}
