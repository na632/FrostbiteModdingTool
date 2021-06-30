using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static FIFA21Plugin.FIFA21AssetLoader;

namespace FIFA21Plugin
{

	public class BundleEntryInfo
    {
		/// <summary>
		/// Is it a EBX, RES or Chunk
		/// </summary>
		public string Type { get; set; }
		public Guid? ChunkGuid { get; set; }
		public Sha1? Sha { get; set; }
		public string Name { get; set; }
		public long Offset { get; set; }
		public long Size { get; set; }
		public int Flag { get; set; }
		public long StringOffset { get; set; }
		public int Index { get; set; }
        public int CasIndex { get; internal set; }
        public int Offset2 { get; internal set; }
        public int OriginalSize { get; internal set; }

        public override string ToString()
        {
			var builtString = string.Empty;

			if (!string.IsNullOrEmpty(Type))
			{
				builtString += Type;
			}

			if (!string.IsNullOrEmpty(Name))
            {
				builtString += " " + Name;
            }
			
			if (Sha.HasValue)
			{
				builtString += " " + Sha.Value.ToString();
			}


			if (!string.IsNullOrEmpty(builtString))
			{
				builtString = base.ToString();
			}

			return builtString;


		}
    }

    public class TOCFile
    {
        public SBFile AssociatedSBFile { get; set; }
        public string FileLocation { get; internal set; }
        public string NativeFileLocation { get; internal set; }

		public bool DoLogging = true;

		public bool ProcessData = true;

		//public int[] ArrayOfInitialHeaderData = new int[12];

		public ContainerMetaData MetaData = new ContainerMetaData();
		public List<BaseBundleInfo> Bundles = new List<BaseBundleInfo>();

		public List<Guid> tocChunkGuids = new List<Guid>();

		public string SuperBundleName;

		private TocSbReader_FIFA21 ParentReader;
		
		/// <summary>
		/// Only for testing
		/// </summary>
		public TOCFile()
        {

        }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		public TOCFile(TocSbReader_FIFA21 parent)
        {
			ParentReader = parent;
        }

		public class ContainerMetaData
        {
			public int Magic { get; set; }
			public int BundleOffset { get; set; }
			public int BundleCount { get; set; }
			public int ChunkOffsetPosition { get; set; }
			public int ChunkGuidOffset { get; set; }
			public int ChunkCount { get; set; }
			public int ChunkEntryOffset { get; set; }
			public int Unk1Offset { get; set; }
			public int Unk7Offset { get; set; }
			public int Offset7 { get; set; }
			public int CountOfSomething { get; set; }
			public int CountOfSomething2 { get; set; }
			public int Unk7Count { get; set; }
			public int Unk12Count { get; set; }
			public int Unk12Offset { get; set; }

			public int SizeOfUnkBlock { get; set; }

			public void Read(NativeReader nativeReader)
			{
				// 0
				Magic = nativeReader.ReadInt(Endian.Big); // 4
				BundleOffset = nativeReader.ReadInt(Endian.Big); // 8
				BundleCount = nativeReader.ReadInt(Endian.Big); // 12
				ChunkOffsetPosition = nativeReader.ReadInt(Endian.Big); // 16
				ChunkGuidOffset = nativeReader.ReadInt(Endian.Big);  // 20
				ChunkCount = nativeReader.ReadInt(Endian.Big);  // 24
				ChunkEntryOffset = nativeReader.ReadInt(Endian.Big); // 28
				Unk1Offset = nativeReader.ReadInt(Endian.Big); // 32
				Unk7Offset = nativeReader.ReadInt(Endian.Big); // 36
				Offset7 = nativeReader.ReadInt(Endian.Big); // 40
				CountOfSomething = nativeReader.ReadInt(Endian.Big); // 44
				CountOfSomething2 = nativeReader.ReadInt(Endian.Big); // 48
				Unk7Count = nativeReader.ReadInt(Endian.Big); // 52
				Unk12Count = nativeReader.ReadInt(Endian.Big); // 56
				Unk12Offset = nativeReader.ReadInt(Endian.Big); // 60

				SizeOfUnkBlock = (BundleOffset - Magic) / 4;
			}

			public void Write(NativeWriter nativeWriter)
            {
				nativeWriter.Write(Magic, Endian.Big);
				nativeWriter.Write(BundleOffset, Endian.Big);
				nativeWriter.Write(BundleCount, Endian.Big);
				nativeWriter.Write(ChunkOffsetPosition, Endian.Big);
				nativeWriter.Write(ChunkGuidOffset, Endian.Big);
				nativeWriter.Write(ChunkCount, Endian.Big);
				nativeWriter.Write(ChunkEntryOffset, Endian.Big);
				nativeWriter.Write(Unk1Offset, Endian.Big);
				nativeWriter.Write(Unk7Offset, Endian.Big);
				nativeWriter.Write(Offset7, Endian.Big);
				nativeWriter.Write(CountOfSomething, Endian.Big);
				nativeWriter.Write(CountOfSomething2, Endian.Big);
				nativeWriter.Write(Unk7Count, Endian.Big);
				nativeWriter.Write(Unk12Count, Endian.Big);
				nativeWriter.Write(Unk12Offset, Endian.Big);
			}
        }

		public int[] tocMetaData = new int[15];

		public List<ChunkAssetEntry> TocChunks = new List<ChunkAssetEntry>();
		public Dictionary<int, Guid> ChunkIndexToChunkId = new Dictionary<int, Guid>();
		public List<int> BundleReferences = new List<int>();
		long actualInternalPos = 556L;

		public void Read(NativeReader nativeReader)
		{
			nativeReader.Position = 0;
				//var actualInternalPos = internalPos + 4;

				nativeReader.Position = actualInternalPos;
				var magic = nativeReader.ReadInt(Endian.Big);
				if (magic != 0x3c)
					throw new Exception("Magic is not the expected value of 0x3c");

				nativeReader.Position -= 4;

				MetaData.Read(nativeReader);

			if (MetaData.BundleCount > 0 && MetaData.BundleCount != MetaData.BundleOffset)
			{
				for (int index = 0; index < MetaData.BundleCount; index++)
				{
					BundleReferences.Add((int)nativeReader.ReadUInt(Endian.Big));
				}
				nativeReader.Position = actualInternalPos + MetaData.BundleOffset;
				for (int indexOfBundleCount = 0; indexOfBundleCount < MetaData.BundleCount; indexOfBundleCount++)
				{

					int unk1 = nativeReader.ReadInt(Endian.Big);
					var tocsizeposition = nativeReader.Position;
					int size = nativeReader.ReadInt(Endian.Big);
					long dataOffset = nativeReader.ReadLong(Endian.Big);

					if (dataOffset > 0)
					{
						BaseBundleInfo newBundleInfo = new BaseBundleInfo
						{
							//TocOffset = offset1,
							Unk = unk1,
							Offset = dataOffset,
							Size = size,
							TOCSizePosition = tocsizeposition
						};
						Bundles.Add(newBundleInfo);
					}

				}


				if (MetaData.ChunkOffsetPosition != 0 && MetaData.ChunkOffsetPosition != 32)
				{
					if (MetaData.ChunkCount > 0)
					{
						if (DoLogging && AssetManager.Instance != null)
							AssetManager.Instance.logger.Log($"Found {MetaData.ChunkCount} TOC Chunks");

						nativeReader.Position = actualInternalPos + MetaData.ChunkOffsetPosition;
						for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
						{
							ListTocChunkPositions.Add(nativeReader.ReadInt(Endian.Big));
						}
						nativeReader.Position = actualInternalPos + MetaData.ChunkGuidOffset;


						for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
						{
							Guid tocChunkGuid = nativeReader.ReadGuidReverse();
							int tocChunkIndex = nativeReader.ReadInt(Endian.Big);
							ChunkIndexToChunkId.Add(tocChunkIndex, tocChunkGuid);

							tocChunkIndex = tocChunkIndex & 0xFFFFFF;
							while (tocChunkGuids.Count <= tocChunkIndex)
							{
								tocChunkGuids.Add(Guid.Empty);
							}
							tocChunkGuids[tocChunkIndex / 3] = tocChunkGuid;


						}
						nativeReader.Position = actualInternalPos + MetaData.ChunkEntryOffset;

						for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
						{
							ChunkAssetEntry chunkAssetEntry2 = new ChunkAssetEntry();
							chunkAssetEntry2.TOCFileLocation = this.NativeFileLocation;
							chunkAssetEntry2.IsTocChunk = true;

							var unk2 = nativeReader.ReadByte();
							bool patch = nativeReader.ReadBoolean();
							byte catalog2 = nativeReader.ReadByte();
							byte cas2 = nativeReader.ReadByte();

							chunkAssetEntry2.SB_CAS_Offset_Position = (int)nativeReader.Position;
							uint chunkOffset = nativeReader.ReadUInt(Endian.Big);
							chunkAssetEntry2.SB_CAS_Size_Position = (int)nativeReader.Position;
							uint chunkSize = nativeReader.ReadUInt(Endian.Big);
							
							chunkAssetEntry2.Id = tocChunkGuids[chunkIndex];

							// Generate a Sha1 since we dont have one.
							chunkAssetEntry2.Sha1 = Sha1.Create(Encoding.ASCII.GetBytes(chunkAssetEntry2.Id.ToString()));

							chunkAssetEntry2.LogicalOffset = 0;
							chunkAssetEntry2.OriginalSize = (chunkAssetEntry2.LogicalOffset & 0xFFFF) | chunkSize;
							chunkAssetEntry2.Size = chunkSize;
							chunkAssetEntry2.Location = AssetDataLocation.CasNonIndexed;
							chunkAssetEntry2.ExtraData = new AssetExtraData();
							chunkAssetEntry2.ExtraData.Unk = unk2;
							chunkAssetEntry2.ExtraData.Catalog = catalog2;
							chunkAssetEntry2.ExtraData.Cas = cas2;
							chunkAssetEntry2.ExtraData.IsPatch = patch;
							chunkAssetEntry2.ExtraData.CasPath = FileSystem.Instance.GetFilePath(catalog2, cas2, patch);
							chunkAssetEntry2.ExtraData.DataOffset = chunkOffset;

							TocChunks.Add(chunkAssetEntry2);
						}

						for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
						{
							var chunkAssetEntry = TocChunks[chunkIndex];
							if (AssetManager.Instance != null)
								AssetManager.Instance.AddChunk(chunkAssetEntry);

						}
					}

					Unk7Values = new int[MetaData.Unk7Count];
					nativeReader.Position = 556 + MetaData.Unk7Offset;
					for (int k = 0; k < MetaData.Unk7Count; k++)
					{
						Unk7Values[k] = nativeReader.ReadInt(Endian.Big);
					}

					Unk12Values = new int[MetaData.Unk12Count];
					nativeReader.Position = 556 + MetaData.Unk12Offset;
					for (int j = 0; j < MetaData.Unk12Count; j++)
					{
						Unk12Values[j] = nativeReader.ReadInt(Endian.Big);
					}

					CasBundlePosition = nativeReader.Position;
					if (nativeReader.Position < nativeReader.Length)
					{
						LoadCasBundles(nativeReader);
					}


					var PositionAfterCasBundle = nativeReader.Position;
					nativeReader.Position = CasBundlePosition;
					CasBundleData = nativeReader.ReadBytes(Convert.ToInt32(PositionAfterCasBundle - CasBundlePosition));


				}
			}

		}

		public List<CASBundle> CasBundles = new List<CASBundle>();

		public Dictionary<string, List<CASBundle>> CASToBundles = new Dictionary<string, List<CASBundle>>();

		public long CasBundlePosition { get; set; }
		public byte[] CasBundleData { get; set; }
		public long TocChunkPosition { get; set; }

		public List<int> ListTocChunkPositions = new List<int>();
		public int[] Unk7Values { get; set; }
		public int[] Unk12Values { get; set; }

		public void LoadCasBundles(NativeReader nativeReader)
		{
			_ = nativeReader.Position;
			if (nativeReader.Position < nativeReader.Length)
			{

				if (AssetManager.Instance != null)
					AssetManager.Instance.logger.Log("Searching for CAS Data from " + FileLocation);

				for (int i = 0; i < MetaData.BundleCount; i++)
				{
					CASBundle bundle = new CASBundle();

					long startPosition = nativeReader.Position;
					bundle.unk1 = nativeReader.ReadInt(Endian.Big);
					bundle.unk2 = nativeReader.ReadInt(Endian.Big);
					bundle.FlagsOffset = nativeReader.ReadInt32BigEndian();
					bundle.EntriesCount = nativeReader.ReadInt32BigEndian();
					bundle.EntriesOffset = nativeReader.ReadInt32BigEndian();
					bundle.unk3 = nativeReader.ReadInt32BigEndian();
					bundle.unk4 = nativeReader.ReadInt32BigEndian();
					bundle.unk5 = nativeReader.ReadInt32BigEndian();
					bool isInPatch = false;
					byte catalog = 0;
					byte cas = 0;
					nativeReader.Position = startPosition + bundle.FlagsOffset;
					bundle.Flags = nativeReader.ReadBytes(bundle.EntriesCount);
					nativeReader.Position = startPosition + bundle.EntriesOffset;
					for (int j2 = 0; j2 < bundle.EntriesCount; j2++)
					{
						bool hasCasIdentifier = bundle.Flags[j2] == 1;
						if (hasCasIdentifier)
						{
							nativeReader.ReadByte();
							isInPatch = nativeReader.ReadBoolean();
							catalog = nativeReader.ReadByte();
							cas = nativeReader.ReadByte();
						}
						long locationOfOffset = nativeReader.Position;
						int bundleOffsetInCas = nativeReader.ReadInt32BigEndian();
						long locationOfSize = nativeReader.Position;
						int bundleSizeInCas = nativeReader.ReadInt32BigEndian();
						if (j2 == 0)
						{
							bundle.BundleOffset = bundleOffsetInCas;
							bundle.BundleSize = bundleSizeInCas;
							bundle.Cas = cas;
							bundle.Catalog = catalog;
							bundle.Patch = isInPatch;
						}
						else
						{
							if (cas != bundle.Cas)
							{
							}
							if (catalog != bundle.Catalog)
							{
							}
							if (isInPatch != bundle.Patch)
							{
							}
							bundle.TOCOffsets.Add(locationOfOffset);
							bundle.Offsets.Add(bundleOffsetInCas);
								
							bundle.TOCSizes.Add(locationOfSize);
							bundle.Sizes.Add(bundleSizeInCas);

							bundle.TOCCas.Add(cas);
							bundle.TOCCatalog.Add(catalog);
							bundle.TOCPatch.Add(isInPatch);
						}
						bundle.Entries.Add(new 
							{
								isInPatch = isInPatch,
							catalog = catalog,
							cas = cas,
							bundleSizeInCas = bundleSizeInCas,
							locationOfSize = locationOfSize,
							bundleOffsetInCas = bundleOffsetInCas,
							locationOfOffset = locationOfOffset 
						}
						);
					}
					CasBundles.Add(bundle);
					nativeReader.Position = startPosition + bundle.FlagsOffset + bundle.EntriesCount;
				}


				if (CasBundles.Count > 0)
				{
					if (AssetManager.Instance != null)
						AssetManager.Instance.logger.Log($"Found {CasBundles.Count} bundles for CasFiles");

					foreach (var bundle in CasBundles)
					{
						var path = FileSystem.Instance.GetFilePath(bundle.Catalog, bundle.Cas, bundle.Patch);
						if (!string.IsNullOrEmpty(path))
						{
							var lstBundles = new List<CASBundle>();
							if (CASToBundles.ContainsKey(path))
							{
								lstBundles = CASToBundles[path];
							}
							else
							{
								CASToBundles.Add(path, lstBundles);
							}

							lstBundles.Add(bundle);
							CASToBundles[path] = lstBundles;
						}
					}

					if (ProcessData)
					{
						foreach (var ctb in CASToBundles)
						{
							CASDataLoader casDataLoader = new CASDataLoader(this);
							casDataLoader.Load(ctb.Key, ctb.Value);
						}
					}
				}
			}
		}

		public static IEnumerable<int> PatternAt(byte[] source, byte[] pattern)
		{
			for (int i = 0; i < source.Length; i++)
			{
				if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
				{
					yield return i;
				}
			}
		}

		public void Write(Stream stream)
		{
			NativeWriter writer = new NativeWriter(stream, leaveOpen: true);
			writer.Write((long)30331136);
			writer.WriteBytes(new byte[548]);
			
			MetaData.Write(writer);
			foreach (int bundleRef in BundleReferences)
			{
				writer.Write((int)bundleRef, Endian.Big);
			}
			while ((writer.Position - actualInternalPos) % 8 != 0L)
			{
				writer.Write((byte)0);
			}
			long bundleDataOffset = writer.Position;
			foreach (var bundle in Bundles)
			{
				writer.Write((int)bundle.Unk, Endian.Big);
				writer.Write((int)bundle.Size, Endian.Big);
				writer.Write((long)bundle.Offset, Endian.Big);
			}
			long chunkFlagsOffset = writer.Position;
			foreach (int chunkFlag in ListTocChunkPositions)
			{
				writer.Write((int)chunkFlag, Endian.Big);
			}
			long chunkGuidOffset = writer.Position;
			foreach (var chunk in ChunkIndexToChunkId)
			{
				foreach (byte guidByte in chunk.Value.ToByteArray().Reverse())
				{
					writer.Write(guidByte);
				}
				writer.Write(chunk.Key, Endian.Big);
			}
			long chunkEntryOffset = writer.Position;
			foreach (var chunk in TocChunks)
			{
				writer.Write((byte)chunk.ExtraData.Unk);
				writer.Write((byte)(chunk.ExtraData.IsPatch ? 1 : 0));
				writer.Write((byte)chunk.ExtraData.Catalog.Value);
				writer.Write((byte)chunk.ExtraData.Cas.Value);
				writer.Write((uint)chunk.ExtraData.DataOffset, Endian.Big);
				writer.Write((uint)chunk.Size, Endian.Big);
			}
			long offset4 = writer.Position;
			foreach (int offset2Value in Unk7Values)
			{
				writer.Write(offset2Value, Endian.Big);
			}
			long offset5 = writer.Position;
			foreach (int offset8Value in Unk12Values)
			{
				writer.Write(offset8Value, Endian.Big);
			}
			if (CasBundles.Count > 0)
			{
				foreach (var cBundle in CasBundles)
				{
					writer.Write(cBundle.unk1, Endian.Big);
					writer.Write(cBundle.unk2, Endian.Big);
					long FlagsOffsetLocation = writer.Position;
					writer.Write(cBundle.FlagsOffset, Endian.Big);
					writer.Write(cBundle.EntriesCount, Endian.Big);
					long EntriesOffsetLocation = writer.Position;
					writer.Write(cBundle.EntriesOffset, Endian.Big);
					writer.Write(cBundle.unk3, Endian.Big);
					writer.Write(cBundle.unk4, Endian.Big);
					writer.Write(cBundle.unk5, Endian.Big);

					var currentCas = -1;
					var currentCatalog = -1;
					var newFlags = new List<byte>();
					for (int j2 = 0; j2 < cBundle.EntriesCount; j2++)
					{
						var entry = cBundle.Entries[j2];


						bool hasCasIdentifier = currentCas != entry.cas && currentCatalog != entry.catalog;
						if (hasCasIdentifier)
						{
							writer.Write((byte)0);
							writer.Write((bool)entry.isInPatch);
							writer.Write((byte)entry.catalog);
							writer.Write((byte)entry.cas);

						}
						newFlags.Add(Convert.ToByte(hasCasIdentifier ? 0x1 : 0x0));
						writer.Write(entry.bundleOffsetInCas, Endian.Big);
						writer.Write(entry.bundleSizeInCas, Endian.Big);

						currentCas = entry.cas;
						currentCatalog = entry.catalog;
					}
					cBundle.Flags = newFlags.ToArray();
					writer.WriteBytes(cBundle.Flags);
				}
			}

			writer.Position = 0;
			using(var fs = new FileStream("_TestNewToc.dat", FileMode.Create))
				writer.BaseStream.CopyTo(fs);
		}

	}

	
}
