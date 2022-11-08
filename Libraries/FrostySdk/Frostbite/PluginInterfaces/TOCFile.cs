﻿using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostbiteSdk;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static FrostySdk.Frostbite.PluginInterfaces.TOCFile;

namespace FrostySdk.Frostbite.PluginInterfaces
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

    public class TOCFile : IDisposable
    {
      
        [Flags]
        public enum TocFlags : byte
        {
            HasBaseBundles = 1,
            HasBaseChunks = 2,
            CompressedStrings = 4
        }
      
        [Flags]
        public enum AssetUsage : byte
        {
            NotUsedInGame,
            UsedInGame
        }

        public string FileLocation { get; set; }
        public string NativeFileLocation { get; set; }

		public bool DoLogging = true;

		public bool ProcessData = true;

		//public int[] ArrayOfInitialHeaderData = new int[12];

		public ContainerMetaData MetaData = new ContainerMetaData();
		public List<BaseBundleInfo> Bundles = new List<BaseBundleInfo>();

		public Guid[] TocChunkGuids { get; set; }

		public string SuperBundleName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nativeFilePath"></param>
		/// <param name="log"></param>
		/// <param name="process"></param>
        public TOCFile(string nativeFilePath, bool log = true, bool process = true, bool modDataPath = false)
        {
			NativeFileLocation = nativeFilePath;
            FileLocation = FileSystem.Instance.ResolvePath(nativeFilePath, modDataPath);
            DoLogging = log;
			ProcessData = process;
            using (NativeReader reader = new NativeReader(new FileStream(FileLocation, FileMode.Open)))
                Read(reader);
        }

        public class ContainerMetaData
        {
			public int Magic { get; set; }
			public int BundleOffset { get; set; }
			public int BundleCount { get; set; }
			public int ChunkFlagOffsetPosition { get; set; }
			public int ChunkGuidOffset { get; set; }
			public int ChunkCount { get; set; }
			public int ChunkEntryOffset { get; set; }
			public int Unk1_Offset { get; set; }
			public int NameOffset { get; set; }
			public int DataOffset { get; set; }
			public int Unk9_Count { get; set; }
			public TocFlags TOCFlags { get; set; }
			public int CompressedStringCount { get; set; }
			public int CompressedStringSize { get; set; }
			public int CompressedStringOffset { get; set; }

			public int SizeOfUnkBlock { get; set; }

			public void Read(NativeReader nativeReader)
			{
				// 0
				Magic = nativeReader.ReadInt(Endian.Big); // 4
				BundleOffset = nativeReader.ReadInt(Endian.Big); // 8
				BundleCount = nativeReader.ReadInt(Endian.Big); // 12
				ChunkFlagOffsetPosition = nativeReader.ReadInt(Endian.Big); // 16
				ChunkGuidOffset = nativeReader.ReadInt(Endian.Big);  // 20
				ChunkCount = nativeReader.ReadInt(Endian.Big);  // 24
				ChunkEntryOffset = nativeReader.ReadInt(Endian.Big); // 28
				Unk1_Offset = nativeReader.ReadInt(Endian.Big); // 32
				NameOffset = nativeReader.ReadInt(Endian.Big); // 36
				DataOffset = nativeReader.ReadInt(Endian.Big); // 40
				Unk9_Count = nativeReader.ReadInt(Endian.Big); // 44 // seems to be the same as bundles count
				TOCFlags = (TocFlags)nativeReader.ReadInt(Endian.Big); // 48
				if (TOCFlags.HasFlag(TocFlags.CompressedStrings))
				{
					CompressedStringCount = nativeReader.ReadInt(Endian.Big); // 52
					CompressedStringSize = nativeReader.ReadInt(Endian.Big); // 56
					CompressedStringOffset = nativeReader.ReadInt(Endian.Big); // 60
				}

				SizeOfUnkBlock = (BundleOffset - Magic) / 4;
			}

			public void Write(NativeWriter nativeWriter)
            {
				nativeWriter.Write(Magic, Endian.Big);
				nativeWriter.Write(BundleOffset, Endian.Big);
				nativeWriter.Write(BundleCount, Endian.Big);
				nativeWriter.Write(ChunkFlagOffsetPosition, Endian.Big);
				nativeWriter.Write(ChunkGuidOffset, Endian.Big);
				nativeWriter.Write(ChunkCount, Endian.Big);
				nativeWriter.Write(ChunkEntryOffset, Endian.Big);
				nativeWriter.Write(Unk1_Offset, Endian.Big);
				nativeWriter.Write(NameOffset, Endian.Big);
				nativeWriter.Write(DataOffset, Endian.Big);
				nativeWriter.Write(Unk9_Count, Endian.Big);
				nativeWriter.Write((int)TOCFlags, Endian.Big);
				nativeWriter.Write(CompressedStringCount, Endian.Big);
				nativeWriter.Write(CompressedStringSize, Endian.Big);
				nativeWriter.Write(CompressedStringOffset, Endian.Big);
			}
        }

		public DbObject TOCObjects { get; } = new DbObject(false);
		public int[] tocMetaData { get; } = new int[15];

		public List<ChunkAssetEntry> TocChunks { get; } = new List<ChunkAssetEntry>();
		public Dictionary<int, Guid> ChunkIndexToChunkId { get; } = new Dictionary<int, Guid>();
		public List<int> BundleReferences { get; } = new List<int>();
		long actualInternalPos { get; } = 556L;

		private byte[] ToCVersion;
		private byte[] ToCSig;
		private byte[] ToCXor;
        //private byte[] InitialHeaderData;

		public Dictionary<Guid, DbObject> TocChunkInfo = new Dictionary<Guid, DbObject>();

		public List<DbObject> Read(in NativeReader nativeReader)
		{
			TocChunks.Clear();
			ChunkIndexToChunkId.Clear();
			TocChunkInfo.Clear();
			BundleReferences.Clear();
			Bundles.Clear();

			nativeReader.Position = 0;

			ToCVersion = nativeReader.ReadBytes(8);
			ToCSig = nativeReader.ReadBytes(256);
			ToCXor = nativeReader.ReadBytes(289);

			nativeReader.Position = actualInternalPos;
			var magic = nativeReader.ReadInt(Endian.Big);
			if (magic != 0x3c)
				throw new Exception("Magic is not the expected value of 0x3c");

			nativeReader.Position -= 4;

			MetaData.Read(nativeReader);
			if (MetaData.BundleCount == 0)
				return TOCObjects.List.Select(o => (DbObject)o).ToList();

            ReadBundleData(nativeReader);
            ReadChunkData(nativeReader);
			ReadCompressedStringData(nativeReader);

			CasBundlePosition = nativeReader.Position;
			if (Bundles.Count > 0 && nativeReader.Position < nativeReader.Length)
			{
				LoadCasBundles(nativeReader);
			}

			return TOCObjects.List.Select(o => (DbObject)o).ToList();
		}

        void ReadCompressedStringData(NativeReader nativeReader)
        {
            CompressedStringNames = new int[MetaData.CompressedStringCount];
            nativeReader.Position = 556 + MetaData.NameOffset;
            for (int k = 0; k < MetaData.CompressedStringCount; k++)
            {
                CompressedStringNames[k] = (int)nativeReader.ReadUInt(Endian.Big);
            }
            CompressedStringTable = new int[MetaData.CompressedStringSize];
            nativeReader.Position = 556 + MetaData.CompressedStringOffset;
            for (int j = 0; j < MetaData.CompressedStringSize; j++)
            {
                CompressedStringTable[j] = (int)nativeReader.ReadUInt(Endian.Big);
            }
        }

        private void ReadChunkData(NativeReader nativeReader)
		{
			nativeReader.Position = actualInternalPos + MetaData.ChunkFlagOffsetPosition;
			if (MetaData.ChunkCount > 0)
            {
				if (DoLogging && AssetManager.Instance != null)
					AssetManager.Instance.logger.Log($"Found {MetaData.ChunkCount} TOC Chunks");

				nativeReader.Position = actualInternalPos + MetaData.ChunkFlagOffsetPosition;
				for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
				{
					ListTocChunkFlags.Add(nativeReader.ReadInt(Endian.Big));
				}
				nativeReader.Position = actualInternalPos + MetaData.ChunkGuidOffset;

				TocChunkGuids = new Guid[MetaData.ChunkCount];
				for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
				{
					Guid tocChunkGuid = nativeReader.ReadGuidReverse();
					int origIndex = nativeReader.ReadInt(Endian.Big);
					ChunkIndexToChunkId.Add(origIndex, tocChunkGuid);

					var actualIndex = origIndex & 0xFFFFFF;
					var actualIndexDiv3 = actualIndex / 3;
					TocChunkGuids[actualIndexDiv3] = tocChunkGuid;
				}

				nativeReader.Position = actualInternalPos + MetaData.ChunkEntryOffset;
				for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
				//foreach(var itemChunk in ChunkIndexToChunkId)
				{
					DbObject dboChunk = new DbObject();

					ChunkAssetEntry chunkAssetEntry2 = new ChunkAssetEntry();
					chunkAssetEntry2.TOCFileLocation = this.NativeFileLocation;
					chunkAssetEntry2.IsTocChunk = true;

					var unk2 = nativeReader.ReadByte();
					dboChunk.SetValue("patchPosition", (int)nativeReader.Position);
					bool patch = nativeReader.ReadBoolean();
					dboChunk.SetValue("catalogPosition", (int)nativeReader.Position);
					byte catalog2 = nativeReader.ReadByte();
					dboChunk.SetValue("casPosition", (int)nativeReader.Position);
					byte cas2 = nativeReader.ReadByte();

					chunkAssetEntry2.SB_CAS_Offset_Position = (int)nativeReader.Position;
					dboChunk.SetValue("chunkOffsetPosition", (int)nativeReader.Position);
					uint chunkOffset = nativeReader.ReadUInt(Endian.Big);
					dboChunk.SetValue("chunkSizePosition", (int)nativeReader.Position);
					chunkAssetEntry2.SB_CAS_Size_Position = (int)nativeReader.Position;
					uint chunkSize = nativeReader.ReadUInt(Endian.Big);

					chunkAssetEntry2.Id = TocChunkGuids[chunkIndex];
					var ActualTocChunkIndexId = CalculateChunkIndexFromListIndex(chunkIndex);
					dboChunk.SetValue("TocChunkIndexId", ActualTocChunkIndexId);
					dboChunk.SetValue("TocChunkFlag", ListTocChunkFlags[chunkIndex]);
					dboChunk.SetValue("RealTocChunkGuidId", ChunkIndexToChunkId[ActualTocChunkIndexId]);

					if (chunkAssetEntry2.Id == Guid.Empty)
					{
						throw new ArgumentException("");
					}

					// Generate a Sha1 since we dont have one.
					chunkAssetEntry2.Sha1 = Sha1.Create(Encoding.ASCII.GetBytes(chunkAssetEntry2.Id.ToString()));

					chunkAssetEntry2.LogicalOffset = chunkOffset;
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

					TocChunkInfo.Add(chunkAssetEntry2.Id, dboChunk);
				}

				for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
				{
					var chunkAssetEntry = TocChunks[chunkIndex];
					if (AssetManager.Instance != null && ProcessData)
					{
						AssetManager.Instance.AddChunk(chunkAssetEntry);
					}

				}
			}
		}

		private void ReadBundleData(NativeReader nativeReader)
		{
            nativeReader.Position = actualInternalPos + MetaData.BundleOffset;
            if (MetaData.BundleCount > 0 && MetaData.BundleCount != MetaData.BundleOffset)
			{
				for (int index = 0; index < MetaData.BundleCount; index++)
				{
					BundleReferences.Add((int)nativeReader.ReadUInt(Endian.Big));
					
				}
				nativeReader.Position = actualInternalPos + MetaData.BundleOffset;
				for (int indexOfBundleCount = 0; indexOfBundleCount < MetaData.BundleCount; indexOfBundleCount++)
				{
					int bundleNameOffset = nativeReader.ReadInt(Endian.Big);
					int size = nativeReader.ReadInt(Endian.Big);
					long dataOffset = nativeReader.ReadLong(Endian.Big);
					BaseBundleInfo newBundleInfo = new BaseBundleInfo
					{
						BundleNameOffset = bundleNameOffset,
						Offset = dataOffset,
						Size = size,
						TocBundleIndex = indexOfBundleCount
					};
					Bundles.Add(newBundleInfo);
				}
			}
		}

		public static int CalculateChunkIndexFromListIndex(int listIndex)
		{
			return ((listIndex * 3) + 16777216);
		}

        private List<CASBundle> casBundles = new List<CASBundle>();

        public List<CASBundle> CasBundles
        {
            get { return casBundles; }
            set { casBundles = value; }
        }

        public Dictionary<string, List<CASBundle>> CASToBundles = new Dictionary<string, List<CASBundle>>();

		public long CasBundlePosition { get; set; }
		public byte[] CasBundleData { get; set; }
		public long TocChunkPosition { get; set; }

		public List<int> ListTocChunkFlags = new List<int>();
		public int[] CompressedStringNames { get; set; }
		public int[] CompressedStringTable { get; set; }

		public void LoadCasBundles(NativeReader nativeReader)
		{
			_ = nativeReader.Position;
			if (nativeReader.Position < nativeReader.Length)
			{

				if (AssetManager.Instance != null && DoLogging)
					AssetManager.Instance.logger.Log("Searching for CAS Data from " + FileLocation);

				for (int i = 0; i < MetaData.BundleCount; i++)
				{
					CASBundle bundle = new CASBundle();

					long startPosition = nativeReader.Position;
					bundle.unk1 = nativeReader.ReadInt(Endian.Big);
					bundle.unk2 = nativeReader.ReadInt(Endian.Big);
					bundle.FlagsOffset = nativeReader.ReadInt(Endian.Big);
					bundle.EntriesCount = nativeReader.ReadInt(Endian.Big);
					bundle.EntriesOffset = nativeReader.ReadInt(Endian.Big);
					bundle.unk3 = nativeReader.ReadInt(Endian.Big);
					bundle.unk4 = nativeReader.ReadInt(Endian.Big);
					bundle.unk5 = nativeReader.ReadInt(Endian.Big);
					byte unk = 0;
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
							unk = nativeReader.ReadByte();
							isInPatch = nativeReader.ReadBoolean();
							catalog = nativeReader.ReadByte();
							cas = nativeReader.ReadByte();
						}
						long locationOfOffset = nativeReader.Position;
						uint bundleOffsetInCas = nativeReader.ReadUInt(Endian.Big);
						long locationOfSize = nativeReader.Position;
						uint bundleSizeInCas = nativeReader.ReadUInt(Endian.Big);
						if (j2 == 0)
						{
							bundle.Unk = unk;
							bundle.BundleOffset = bundleOffsetInCas;
							bundle.BundleSize = bundleSizeInCas;
							bundle.Cas = cas;
							bundle.Catalog = catalog;
							bundle.Patch = isInPatch;
						}
						else
						{
							bundle.TOCOffsets.Add(locationOfOffset);
							bundle.Offsets.Add(bundleOffsetInCas);
								
							bundle.TOCSizes.Add(locationOfSize);
							bundle.Sizes.Add(bundleSizeInCas);

							bundle.TOCCas.Add(cas);
							bundle.TOCCatalog.Add(catalog);
							bundle.TOCPatch.Add(isInPatch);
						}
						bundle.Entries.Add(new CASBundleEntry()
						{
							unk = unk,
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
					if (AssetManager.Instance != null && DoLogging)
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

					foreach (var ctb in CASToBundles)
					{
						DbObject dbo = new DbObject(false);
						CASDataLoader casDataLoader = new CASDataLoader(this);
						dbo = casDataLoader.Load(ctb.Key, ctb.Value);
						foreach(var d in dbo)
                        {
							TOCObjects.Add(d);
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
			writer.Write(ToCVersion);
			var sigPosition = writer.Position;
			writer.Write(ToCSig);
			writer.Write(ToCXor);
			while (writer.Position != 556)
				writer.Write((byte)0);

            long metaDataOffset = writer.Position;
			MetaData.Write(writer);
			foreach (int bundleRef in BundleReferences)
			{
				writer.Write((int)bundleRef, Endian.Big);
			}
			while ((writer.Position - actualInternalPos) % 8 != 0L)
			{
				writer.Write((byte)0);
			}
			MetaData.BundleOffset = (int)writer.Position - (int)actualInternalPos;
			foreach (var bundle in Bundles)
			{
				writer.Write((int)bundle.BundleNameOffset, Endian.Big);
				writer.Write((int)bundle.Size, Endian.Big);
				writer.Write((long)bundle.Offset, Endian.Big);
			}
            MetaData.ChunkFlagOffsetPosition = (int)writer.Position - (int)actualInternalPos;
            foreach (int chunkFlag in ListTocChunkFlags)
			{
				writer.Write((int)chunkFlag, Endian.Big);
			}
            MetaData.ChunkGuidOffset = (int)writer.Position - (int)actualInternalPos;
            foreach (var chunk in ChunkIndexToChunkId)
			{
				foreach (byte guidByte in chunk.Value.ToByteArray().Reverse())
				{
					writer.Write(guidByte);
				}
				writer.Write(chunk.Key, Endian.Big);
			}
            MetaData.ChunkEntryOffset = (int)writer.Position - (int)actualInternalPos;
            foreach (var chunk in TocChunks)
			{
				writer.Write((byte)chunk.ExtraData.Unk);
				writer.Write((byte)(chunk.ExtraData.IsPatch ? 1 : 0));
				writer.Write((byte)chunk.ExtraData.Catalog.Value);
				writer.Write((byte)chunk.ExtraData.Cas.Value);
				writer.Write((uint)chunk.ExtraData.DataOffset, Endian.Big);
				writer.Write((uint)chunk.Size, Endian.Big);
			}
            MetaData.NameOffset = (int)writer.Position - (int)actualInternalPos;
            foreach (int offset2Value in CompressedStringNames)
			{
				writer.Write((int)offset2Value, Endian.Big);
			}
            MetaData.CompressedStringOffset = (int)writer.Position - (int)actualInternalPos;
            foreach (int offset12Value in CompressedStringTable)
			{
				writer.Write((int)offset12Value, Endian.Big);
			}
			if (CasBundles.Count > 0)
			{
				List<long> newCasBundleOffsets = new List<long>(CasBundles.Count);

                foreach (var cBundle in CasBundles)
                {
                    long casBundleOffsetPosition = writer.Position;
                    newCasBundleOffsets.Add(casBundleOffsetPosition);

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
                    bool? currentPatch = null;
                    var newFlags = new List<byte>();
                    for (int j2 = 0; j2 < cBundle.EntriesCount; j2++)
                    {
                        var entry = cBundle.Entries[j2];

                        bool hasCasIdentifier =
                            j2 == 0
                            || currentCas != entry.cas
                            || currentCatalog != entry.catalog
                            || !currentPatch.HasValue
                            || currentPatch.Value != entry.isInPatch;
                        if (hasCasIdentifier)
                        {
                            writer.Write((byte)0);
                            writer.Write((bool)entry.isInPatch);
                            writer.Write((byte)entry.catalog);
                            writer.Write((byte)entry.cas);

                        }
                        newFlags.Add(Convert.ToByte(hasCasIdentifier ? 0x1 : 0x0));
                        writer.Write((uint)entry.bundleOffsetInCas, Endian.Big);
                        writer.Write((uint)entry.bundleSizeInCas, Endian.Big);

                        currentCas = entry.cas;
                        currentCatalog = entry.catalog;
                        currentPatch = entry.isInPatch;
                    }
                    cBundle.Flags = newFlags.ToArray();
                    cBundle.FlagsOffset = (int)writer.Position - (int)casBundleOffsetPosition;
                    writer.WriteBytes(cBundle.Flags);
                    long endOfCasBundleOffsetPosition = writer.Position;
                    writer.Position = FlagsOffsetLocation;
                    writer.Write(cBundle.FlagsOffset, Endian.Big);
                    writer.Position = endOfCasBundleOffsetPosition;
                }
			}

			writer.Position = metaDataOffset;
            MetaData.Write(writer);

			writer.Position = 556;
			byte[] streamBuffer = new byte[writer.Length - 556];
			writer.BaseStream.Read(streamBuffer, 0, (int)writer.Length - 556);
			var newTocSig = streamBuffer.ToTOCSignature();
			writer.Position = 8;
			writer.Write(newTocSig);

			//writer.Position = 0;
			//using (var fs = new FileStream("_TestNewToc.dat", FileMode.Create))
   //             writer.BaseStream.CopyTo(fs);
		}

		public static void RebuildTOCSignatureOnly(Stream stream)
		{
			if (!stream.CanWrite)
				throw new IOException("Unable to Write to this Stream!");

            if (!stream.CanRead)
                throw new IOException("Unable to Read to this Stream!");

            if (!stream.CanSeek)
                throw new IOException("Unable to Seek this Stream!");

            byte[] streamBuffer = new byte[stream.Length - 556];
			stream.Position = 556;
            stream.Read(streamBuffer, 0, (int)stream.Length - 556);
            var newTocSig = streamBuffer.ToTOCSignature();
            stream.Position = 8;
            stream.Write(newTocSig);
        }

		public static void RebuildTOCSignatureOnly(string filePath)
		{
			using (var fsTocSig = new FileStream(filePath, FileMode.Open))
				TOCFile.RebuildTOCSignatureOnly(fsTocSig);
		}

		public static int CreateCasInt(byte unk, bool isPatch, byte catalog, byte cas)
		{
			return (unk << 24) | ((isPatch ? 1 : 0) << 16) | (catalog << 8) | cas;
		}

        public void Dispose()
        {
			if(CASToBundles != null && CASToBundles.Count > 0)
				CASToBundles.Clear();

			CASToBundles = null;

			if (Bundles != null && Bundles.Count > 0)
				Bundles.Clear();

			Bundles = null;

			if(TOCObjects != null)
            {
				if(TOCObjects.Dictionary != null)
					TOCObjects.Dictionary.Clear();

				if (TOCObjects.List != null)
					TOCObjects.List.Clear();

				//TOCObjects = null;
			}

			GC.Collect();
			GC.WaitForPendingFinalizers();

		}
    }


}
