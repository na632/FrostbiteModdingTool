using System.Collections.Generic;

namespace FrostySdk.Managers
{
	public class AssetManagerImportResult
	{
		public bool InvalidatedDueToPatch
		{
			get;
			internal set;
		}

		public List<EbxAssetEntry> AddedAssets
		{
			get;
			internal set;
		}

		public List<EbxAssetEntry> ModifiedAssets
		{
			get;
			internal set;
		}

		public List<EbxAssetEntry> RemovedAssets
		{
			get;
			internal set;
		}
	}
}
