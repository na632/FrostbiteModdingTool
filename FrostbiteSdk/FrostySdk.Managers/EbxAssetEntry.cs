using System;
using System.Collections.Generic;

namespace FrostySdk.Managers
{
	public class EbxAssetEntry : AssetEntry
	{
		public Guid Guid;

		public List<Guid> DependentAssets = new List<Guid>();

		public override string AssetType => "ebx";

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
