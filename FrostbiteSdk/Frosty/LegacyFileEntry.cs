using Frosty.Hash;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;

namespace FrostySdk
{
	public class LegacyFileEntry : AssetEntry
	{
        public class ChunkCollectorInstance
		{
			public EbxAssetEntry Entry;

			public Guid ChunkId;

			public long Offset;

			public long CompressedOffset;

			public long CompressedSize;

			public long Size;

			public ChunkCollectorInstance ModifiedEntry
			{
				get;
				set;
			}

			public bool IsModified => ModifiedEntry != null;
		}

		public List<ChunkCollectorInstance> CollectorInstances = new List<ChunkCollectorInstance>();

		public Guid ChunkId
		{
			get
			{
				if (CollectorInstances.Count == 0)
				{
					return Guid.Empty;
				}
				if (CollectorInstances[0].IsModified)
				{
					return CollectorInstances[0].ModifiedEntry.ChunkId;
				}
				return CollectorInstances[0].ChunkId;
			}
		}

		public int NameHash => Fnv1.HashString(Name);

		public override string AssetType => "legacy";

		public override string Type
		{
			get
			{
				int num = Name.LastIndexOf('.');
				if (num == -1)
				{
					return "";
				}
				return Name.Substring(num + 1).ToUpper();
			}
			set
			{
			}
		}

		public override string Filename
		{
			get
			{
				int num = base.Filename.LastIndexOf('.');
				if (num == -1)
				{
					return base.Filename;
				}
				return base.Filename.Substring(0, num);
			}
		}

		public override bool IsModified
		{
			get
			{
				if (CollectorInstances.Count == 0)
				{
					return false;
				}
				return CollectorInstances[0].IsModified;
			}
		}

		public override bool IsDirty
		{
			get
			{
				if (LegacyFileManager.AssetManager.GetChunkEntry(ChunkId).IsDirty)
				{
					return true;
				}
				return false;
			}
		}

		public override void ClearModifications()
		{
			foreach (ChunkCollectorInstance collectorInstance in CollectorInstances)
			{
				collectorInstance.ModifiedEntry = null;
			}
		}
	}
}
