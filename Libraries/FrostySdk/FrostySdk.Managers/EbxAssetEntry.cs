using FrostySdk.FrostySdk.Managers;
using System;
using System.Collections.Generic;

namespace FrostySdk.Managers
{
	public class EbxAssetEntry : AssetEntry, IAssetEntry
	{
		public Guid Guid { get; set; }

		public List<Guid> DependentAssets = new List<Guid>();

		public override string AssetType => "ebx";

        public bool IsBinary { get; internal set; }

        public bool ContainsDependency(Guid guid)
		{
			if (base.IsDirectlyModified)
			{
				return ModifiedEntry.DependentAssets.Contains(guid);
			}
			return DependentAssets.Contains(guid);
		}

		public IEnumerable<Guid> EnumerateDependencies()
		{
			if (base.IsDirectlyModified)
			{
				foreach (Guid dependentAsset in ModifiedEntry.DependentAssets)
				{
					yield return dependentAsset;
				}
			}
			else
			{
				foreach (Guid dependentAsset2 in DependentAssets)
				{
					yield return dependentAsset2;
				}
			}
		}
	}
}
