using System;
using System.Collections.Generic;
using System.Linq;

namespace FIFA21Plugin.Plugin2
{
	public class TocFile_F21
	{
		public class CasBundle
		{
			public int BundleId
			{
				get;
				set;
			}

			public int CasCatalog
			{
				get;
				set;
			}

			public int CasIndex
			{
				get;
				set;
			}

			public bool InPatch
			{
				get;
				set;
			}

			public int BundleLength
			{
				get;
				set;
			}

			public long BundleOffset
			{
				get;
				set;
			}

			public List<CasBundleEntry> Entries
			{
				get;
				set;
			}
		}

		public class CasBundleEntry
		{
			public bool HasCasIdentifier
			{
				get;
				set;
			}

			public int CasCatalog
			{
				get;
				set;
			}

			public int CasIndex
			{
				get;
				set;
			}

			public bool InPatch
			{
				get;
				set;
			}

			public int EntrySize
			{
				get;
				set;
			}

			public int EntryOffset
			{
				get;
				set;
			}
		}

		private int bundleFlagsOffset;

		private int bundleDataOffset;

		private int bundleCount;

		private int chunkFlagsOffset;

		private int chunkGuidOffset;

		private int chunkCount;

		private int chunkEntryOffset;

		private int unknownOffset1;

		private int offset2;

		private int unknownOffset3;

		private int count6;

		private int count7;

		private int offset8;

		private List<(int unk, int length, long offset)> bundles;

		private List<(byte unk, bool patch, byte catalog, byte cas, uint offset, uint size)> chunks;

		public byte[] XorKey
		{
			get;
			private set;
		}

		public int UnknownValue4
		{
			get;
			private set;
		}

		public int UnknownValue5
		{
			get;
			private set;
		}

		public List<int> bundleFlags
		{
			get;
			set;
		}

		public List<int> ChunkFlags
		{
			get;
			set;
		}

		public List<(Guid id, int index)> chunkGuids
		{
			get;
			set;
		}

		public List<(Guid id, int index)> orderedChunkGuids
		{
			get;
			set;
		}

		public List<int> offset2Values
		{
			get;
			set;
		}

		public List<int> offset8Values
		{
			get;
			set;
		}

		public List<CasBundle> CasBundles
		{
			get;
			set;
		}

		public List<(int unk, int length, long offset)> Bundles => bundles;

		public List<(byte unk, bool patch, byte catalog, byte cas, uint offset, uint size)> Chunks => chunks;

		public TocFile_F21(byte[] xorKey, int bundleFlagsOffset, int bundleDataOffset, int bundleCount, int chunkFlagsOffset, int chunkGuidOffset, int chunkCount, int chunkEntryOffset, int unknownOffset1, int offset2, int unknownOffset3, int unknownValue4, int unknownValue5, int count6, int count7, int offset8, List<int> bundleFlags, List<int> chunkFlags, List<(int unk, int length, long offset)> bundles, List<(Guid id, int index)> chunkGuids, List<(byte unk, bool patch, byte catalog, byte cas, uint offset, uint size)> chunks, List<int> offset2Values, List<int> offset8Values, List<(int casCatalog, int casIndex, bool inPatch, int bundleLength, long bundleOffset, List<(bool hasCasIdentifier, int casCatalog, int casIndex, bool inPatch, int entryOffset, int entrySize)> entries)> casBundles)
		{
			XorKey = xorKey;
			this.bundleFlagsOffset = bundleFlagsOffset;
			this.bundleDataOffset = bundleDataOffset;
			this.bundleCount = bundleCount;
			this.chunkFlagsOffset = chunkFlagsOffset;
			this.chunkGuidOffset = chunkGuidOffset;
			this.chunkCount = chunkCount;
			this.chunkEntryOffset = chunkEntryOffset;
			this.unknownOffset1 = unknownOffset1;
			this.offset2 = offset2;
			this.unknownOffset3 = unknownOffset3;
			UnknownValue4 = unknownValue4;
			UnknownValue5 = unknownValue5;
			this.count6 = count6;
			this.count7 = count7;
			this.offset8 = offset8;
			this.bundleFlags = bundleFlags;
			ChunkFlags = chunkFlags;
			this.bundles = bundles;
			this.chunkGuids = chunkGuids;
			orderedChunkGuids = new List<(Guid, int)>(chunkGuids.Count);
			this.chunks = chunks;
			this.offset2Values = offset2Values;
			this.offset8Values = offset8Values;
			CasBundles = casBundles.Select(((int casCatalog, int casIndex, bool inPatch, int bundleLength, long bundleOffset, List<(bool hasCasIdentifier, int casCatalog, int casIndex, bool inPatch, int entryOffset, int entrySize)> entries) b, int index) => new CasBundle
			{
				BundleId = index,
				CasCatalog = b.casCatalog,
				CasIndex = b.casIndex,
				InPatch = b.inPatch,
				BundleLength = b.bundleLength,
				BundleOffset = b.bundleOffset,
				Entries = b.entries.Select(((bool hasCasIdentifier, int casCatalog, int casIndex, bool inPatch, int entryOffset, int entrySize) e) => new CasBundleEntry
				{
					HasCasIdentifier = e.hasCasIdentifier,
					CasCatalog = e.casCatalog,
					CasIndex = e.casIndex,
					InPatch = e.inPatch,
					EntrySize = e.entrySize,
					EntryOffset = e.entryOffset
				}).ToList()
			}).ToList();
			foreach (var chunkGuid in this.chunkGuids)
			{
				Guid guid = chunkGuid.id;
				int index2 = chunkGuid.index;
				int order = index2 & 0xFFFFFF;
				while (orderedChunkGuids.Count <= order / 3)
				{
					orderedChunkGuids.Add(default((Guid, int)));
				}
				orderedChunkGuids[order / 3] = (guid, index2);
			}
		}
	}
}
