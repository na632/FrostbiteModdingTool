using Frosty.Hash;
using FrostySdk.FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostySdk.Managers
{
	[Serializable]
	public class AssetEntry : IAssetEntry, IEqualityComparer<AssetEntry>
	{
		public CompressionType OriginalCompressionType = CompressionType.Default;

		public virtual Sha1 Sha1 { get; set; }

		public virtual Sha1 BaseSha1 { get; set; }

		private long _size;

		public long Size
        {
            get
            {
		
				return _size;
            }
			set
            {
				_size = value;
            }
        }

		public long OriginalSize { get; set; }

		public bool IsInline { get; set; }

		public AssetDataLocation Location { get; set; }

		public AssetExtraData ExtraData { get; set; }

		private List<int> listBundles = new List<int>();

        public List<int> Bundles
        {
            get { return listBundles; }
            set { listBundles = value; }
        }

		public IEnumerable<string> BundleNames
		{
			get
			{
				List<string> bNames = new List<string>();
				foreach (var i in Bundles)
				{
					bNames.Add(AssetManager.Instance.bundles[i].Name);
				}
				return bNames;
			}
		}


		public List<int> AddBundles = new List<int>();

		public List<int> RemBundles = new List<int>();

		public ModifiedAssetEntry ModifiedEntry { get; set; }

		private List<AssetEntry> linkedAssets = new List<AssetEntry>();

        public List<AssetEntry> LinkedAssets
		{
            get { return linkedAssets; }
            set { linkedAssets = value; }
        }

		private bool dirty;

		/// <summary>
		/// Name is the Full Path
		/// </summary>
		public virtual string Name
		{
			get;
			set;
		}

		public virtual string DuplicatedFromName
        {
			get;
			set;
        }

		public string FullPath
        {
			get
            {
				return Name;
            }
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


		/// <summary>
		/// The Actual FileName?
		/// </summary>
		public virtual string Filename
		{
			get
			{
				if (!string.IsNullOrEmpty(Name))
				{
					int num = Name.LastIndexOf('/');
					if (num == -1)
					{
						return Name;
					}
					return Name.Substring(num + 1);
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// The Virtual Path to the File
		/// </summary>
		public virtual string Path
		{
			get
			{
				if (!string.IsNullOrEmpty(Name))
				{
					int num = Name.LastIndexOf('/');
					if (num == -1)
					{
						return "";
					}
					return Name.Substring(0, num);
				}
				return string.Empty;
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
					return ModifiedEntry.Data != null;
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

		// ---- -----------------------------------------------------------------
		// v2k4 helpers for different approach to mod data generation

		private string _casfilelocation;
		public string CASFileLocation
        {
			get
            {
				if (string.IsNullOrEmpty(_casfilelocation) && ExtraData != null)
					_casfilelocation = ExtraData.CasPath;

				return _casfilelocation;
            }
			set { _casfilelocation = value; }
        }
		public bool IsInPatch
        {
			get
            {
				if (ExtraData != null)
					return ExtraData.IsPatch;

				return false;
            }
        }

		public string Bundle { get; set; }
		public string SBFileLocation { get; set; }


        private string tocFileLocation;

        public string TOCFileLocation
        {
            get 
			{
				if (string.IsNullOrEmpty(tocFileLocation) && TOCFileLocations != null && TOCFileLocations.Count > 0)
					tocFileLocation = TOCFileLocations.First();

				return tocFileLocation; 
			}
            set { tocFileLocation = value; }
        }

        public HashSet<string> TOCFileLocations { get; set; }

		public int SB_CAS_Offset_Position { get; set; }
		public int SB_CAS_Size_Position { get; set; }
		public int SB_Sha1_Position { get; set; }
		public int SB_OriginalSize_Position { get; set; }

		public int ParentBundleOffset { get; set; }
		public int ParentBundleSize { get; set; }


		public bool IsLegacy = false;


		// ---- -----------------------------------------------------------------
		// ---- -----------------------------------------------------------------

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

        public bool Equals(AssetEntry x, AssetEntry y)
        {
			if (x == null)
				return false;

			if (y == null)
				return false;

			if (x.Sha1 == y.Sha1)
				return true;

			if (x.Name == y.Name)
				return true;

			return false;
        }

        public int GetHashCode(AssetEntry obj)
        {
			return BitConverter.ToInt32(Sha1.ToByteArray());
        }

		public virtual AssetEntry Clone()
        {
			return this.MemberwiseClone() as AssetEntry;
        }

        public override string ToString()
        {
			if(!string.IsNullOrEmpty(Name))
            {
				return Name;
            }
            return base.ToString();
        }
    }
}
