using Frosty.Hash;
using System;
using System.Collections.Generic;

namespace FrostySdk.Managers
{
	public class AssetEntry
	{
		public Sha1 Sha1;

		public Sha1 BaseSha1;

		public long Size;

		public long OriginalSize;

		public bool IsInline;

		public AssetDataLocation Location;

		public AssetExtraData ExtraData;

		public List<int> Bundles = new List<int>();

		public List<int> AddBundles = new List<int>();

		public List<int> RemBundles = new List<int>();

		public ModifiedAssetEntry ModifiedEntry;

		public List<AssetEntry> LinkedAssets = new List<AssetEntry>();

		private bool dirty;

		public virtual string Name
		{
			get;
			set;
		}

		public virtual string Type
		{
			get;
			set;
		}

		public virtual string AssetType
		{
			get;
		}

		public virtual string DisplayName => Filename + (IsDirty ? "*" : "");

		public virtual string Filename
		{
			get
			{
				int num = Name.LastIndexOf('/');
				if (num == -1)
				{
					return Name;
				}
				return Name.Substring(num + 1);
			}
		}

		public virtual string Path
		{
			get
			{
				int num = Name.LastIndexOf('/');
				if (num == -1)
				{
					return "";
				}
				return Name.Substring(0, num);
			}
		}

		public bool IsAdded
		{
			get;
			set;
		}

		public virtual bool IsModified
		{
			get
			{
				if (!IsDirectlyModified)
				{
					return IsIndirectlyModified;
				}
				return true;
			}
		}

		public bool IsDirectlyModified
		{
			get
			{
				if (ModifiedEntry == null && AddBundles.Count == 0)
				{
					return RemBundles.Count != 0;
				}
				return true;
			}
		}

		public bool HasModifiedData
		{
			get
			{
				if (ModifiedEntry != null)
				{
					if (ModifiedEntry.Data == null)
					{
						return ModifiedEntry.DataObject != null;
					}
					return true;
				}
				return false;
			}
		}

		public bool IsIndirectlyModified
		{
			get
			{
				foreach (AssetEntry linkedAsset in LinkedAssets)
				{
					if (linkedAsset.IsModified)
					{
						return true;
					}
				}
				return false;
			}
		}

		public virtual bool IsDirty
		{
			get
			{
				if (dirty)
				{
					return true;
				}
				foreach (AssetEntry linkedAsset in LinkedAssets)
				{
					if (linkedAsset.IsDirty)
					{
						return true;
					}
				}
				return false;
			}
			set
			{
				if (dirty != value)
				{
					dirty = value;
					if (dirty)
					{
						OnModified();
					}
				}
			}
		}

		public event EventHandler AssetModified;

		public void LinkAsset(AssetEntry assetToLink)
		{
			if (!LinkedAssets.Contains(assetToLink))
			{
				LinkedAssets.Add(assetToLink);
			}
			if (assetToLink is ChunkAssetEntry)
			{
				if (assetToLink.HasModifiedData)
				{
					(assetToLink as ChunkAssetEntry).ModifiedEntry.H32 = Fnv1.HashString(Name);
				}
				else
				{
					(assetToLink as ChunkAssetEntry).H32 = Fnv1.HashString(Name);
				}
			}
		}

		public void AddToBundle(int bid)
		{
			AddBundles.Add(bid);
			IsDirty = true;
		}

		public bool AddToBundles(IEnumerable<int> bundles)
		{
			bool result = false;
			foreach (int bundle in bundles)
			{
				if (!Bundles.Contains(bundle) && !AddBundles.Contains(bundle))
				{
					AddBundles.Add(bundle);
					IsDirty = true;
					result = true;
				}
			}
			return result;
		}

		public IEnumerable<int> EnumerateBundles(bool addedOnly = false)
		{
			if (!addedOnly)
			{
				for (int j = 0; j < Bundles.Count; j++)
				{
					if (!RemBundles.Contains(Bundles[j]))
					{
						yield return Bundles[j];
					}
				}
			}
			for (int j = 0; j < AddBundles.Count; j++)
			{
				yield return AddBundles[j];
			}
		}

		public virtual void ClearModifications()
		{
			ModifiedEntry = null;
		}

		public void OnModified()
		{
			this.AssetModified?.Invoke(this, new EventArgs());
		}
	}
}
