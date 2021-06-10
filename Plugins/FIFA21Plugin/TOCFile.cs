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
			public int ChunkFlagOffset { get; set; }
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
				Magic = nativeReader.ReadInt(Endian.Big); // 4
				BundleOffset = nativeReader.ReadInt(Endian.Big); // 8
				BundleCount = nativeReader.ReadInt(Endian.Big); // 12
				ChunkFlagOffset = nativeReader.ReadInt(Endian.Big); // 16
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
        }

		public int[] tocMetaData = new int[15];

		public List<ChunkAssetEntry> TocChunks = new List<ChunkAssetEntry>();


		public void Read(NativeReader nativeReader)
		{
			//var startPosition = nativeReader.Position;
			//if (File.Exists("debugToc.dat"))
			//	File.Delete("debugToc.dat");

			//nativeReader.Position = 0;
			//using (NativeWriter writer = new NativeWriter(new FileStream("debugToc.dat", FileMode.OpenOrCreate)))
			//{
			//	writer.Write(nativeReader.ReadToEnd());
			//}
			nativeReader.Position = 0;

			//AssetManager.Instance.logger.Log("Seaching for Internal TOC Bundles");

			//BoyerMoore boyerMoore = new BoyerMoore(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x3C });
			//var findInternalPatterns = boyerMoore.SearchAll(nativeReader.ReadToEnd());
			//nativeReader.Position = startPosition;

			//AssetManager.Instance.logger.Log($"{findInternalPatterns.Count} Internal TOC Bundles found");

			//foreach (var internalPos in findInternalPatterns)
			{
				//var actualInternalPos = internalPos + 4;
				var actualInternalPos = 556L;

				nativeReader.Position = actualInternalPos;
				var magic = nativeReader.ReadInt(Endian.Big);
				if (magic != 0x3c)
					throw new Exception("Magic is not the expected value of 0x3c");

				nativeReader.Position -= 4;

				MetaData.Read(nativeReader);

				List<int> bundleReferences = new List<int>();



				if (MetaData.BundleCount > 0 && MetaData.BundleCount != MetaData.BundleOffset)
				{
					//for (int index = 0; index < MetaData.BundleCount; index++)
					//{
					//	bundleReferences.Add((int)nativeReader.ReadUInt(Endian.Big));
					//}
					nativeReader.Position = actualInternalPos + MetaData.BundleOffset;
					for (int indexOfBundleCount = 0; indexOfBundleCount < MetaData.BundleCount; indexOfBundleCount++)
					{

						//int offset1 = nativeReader.ReadInt(Endian.Big);

						//var tocsizeposition = nativeReader.Position;
						//int size = nativeReader.ReadInt(Endian.Big);

						//int unk1 = nativeReader.ReadInt(Endian.Big); // unknown

						//int dataOffset = nativeReader.ReadInt(Endian.Big);

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


					if (MetaData.ChunkFlagOffset != 0 && MetaData.ChunkFlagOffset != 32)
					{
						if (MetaData.ChunkCount > 0)
						{
							if (DoLogging && AssetManager.Instance != null)
								AssetManager.Instance.logger.Log($"Found {MetaData.ChunkCount} TOC Chunks");

							nativeReader.Position = actualInternalPos + MetaData.ChunkFlagOffset;
							for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
							{
								ListTocChunkPositions.Add(nativeReader.ReadInt(Endian.Big));
							}
							nativeReader.Position = actualInternalPos + MetaData.ChunkGuidOffset;


							for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
							{
								//byte[] array6 = nativeReader.ReadBytes(16);
								//Guid tocChunkGuid = new Guid(new byte[16]
								//{
								//	array6[15],
								//	array6[14],
								//	array6[13],
								//	array6[12],
								//	array6[11],
								//	array6[10],
								//	array6[9],
								//	array6[8],
								//	array6[7],
								//	array6[6],
								//	array6[5],
								//	array6[4],
								//	array6[3],
								//	array6[2],
								//	array6[1],
								//	array6[0]
								//});
								//nativeReader.Position -= 16;
								//Guid value2 = nativeReader.ReadGuid(Endian.Little);
								//nativeReader.Position -= 16;
								//Guid value3 = nativeReader.ReadGuid(Endian.Big);
								Guid tocChunkGuid = nativeReader.ReadGuidReverse();

								int tocChunkIndex = nativeReader.ReadInt(Endian.Big) & 0xFFFFFF;
								TocChunkIndexes.Add(tocChunkIndex);
								while (tocChunkGuids.Count <= tocChunkIndex)
								{
									tocChunkGuids.Add(Guid.Empty);
								}
								tocChunkGuids[tocChunkIndex / 3] = tocChunkGuid;
								//tocChunkGuids = tocChunkGuids.Where(x => x != Guid.Empty).ToList();



							}
							nativeReader.Position = actualInternalPos + MetaData.ChunkEntryOffset;

							for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
							{
								ChunkAssetEntry chunkAssetEntry2 = new ChunkAssetEntry();
								chunkAssetEntry2.TOCFileLocation = this.NativeFileLocation;
								chunkAssetEntry2.IsTocChunk = true;

								var unk2 = nativeReader.ReadByte();
								bool patch2 = nativeReader.ReadBoolean();
								byte catalog2 = nativeReader.ReadByte();
								byte cas2 = nativeReader.ReadByte();

								chunkAssetEntry2.SB_CAS_Offset_Position = (int)nativeReader.Position;
								uint chunkOffset = nativeReader.ReadUInt(Endian.Big);
								chunkAssetEntry2.SB_CAS_Size_Position = (int)nativeReader.Position;
								uint chunkSize = nativeReader.ReadUInt(Endian.Big);
								if (tocChunkGuids[chunkIndex] == Guid.Empty)
								{

								}
								chunkAssetEntry2.Id = tocChunkGuids[chunkIndex];

								// Generate a Sha1 since we dont have one.
								chunkAssetEntry2.Sha1 = Sha1.Create(Encoding.ASCII.GetBytes(chunkAssetEntry2.Id.ToString()));

								chunkAssetEntry2.LogicalOffset = 0;
								chunkAssetEntry2.OriginalSize = (chunkAssetEntry2.LogicalOffset & 0xFFFF) | chunkSize;

								chunkAssetEntry2.Size = chunkSize;
								chunkAssetEntry2.Location = AssetDataLocation.CasNonIndexed;
								chunkAssetEntry2.ExtraData = new AssetExtraData();
								chunkAssetEntry2.ExtraData.CasPath = FileSystem.Instance.GetFilePath(catalog2, cas2, patch2);
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
						if (nativeReader.Position != 556 + MetaData.Unk7Offset)
						{
							nativeReader.Position = 556 + MetaData.Unk7Offset;
						}
						for (int k = 0; k < MetaData.Unk7Count; k++)
						{
							Unk7Values[k] = nativeReader.ReadInt(Endian.Big);
						}
						Unk12Values = new int[MetaData.Unk12Count];
						if (nativeReader.Position != 556 + MetaData.Unk12Offset)
						{
							nativeReader.Position = 556 + MetaData.Unk12Offset;
						}
						for (int j = 0; j < MetaData.Unk12Count; j++)
						{
							Unk12Values[j] = nativeReader.ReadInt(Endian.Big);
						}

						CasBundlePosition = nativeReader.Position;
						if (nativeReader.Position < nativeReader.Length)
						{
							//TOCCasDataLoader casDataLoader = new TOCCasDataLoader(this);
							//casDataLoader.Load2(nativeReader);
							LoadCasBundles(nativeReader);
						}
						var PositionAfterCasBundle = nativeReader.Position;
						nativeReader.Position = CasBundlePosition;
						CasBundleData = nativeReader.ReadBytes(Convert.ToInt32(PositionAfterCasBundle - CasBundlePosition));




					}
				}

			}




		}

		public List<CASBundle> CasBundles = new List<CASBundle>();

		public Dictionary<string, List<CASBundle>> CASToBundles = new Dictionary<string, List<CASBundle>>();

		public long CasBundlePosition { get; set; }
		public byte[] CasBundleData { get; set; }
		public long TocChunkPosition { get; set; }

		public List<int> TocChunkIndexes = new List<int>();

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
					int a = nativeReader.ReadInt(Endian.Big);
					int b = nativeReader.ReadInt(Endian.Big);
					int flagsOffset = nativeReader.ReadInt32BigEndian();
					int entriesCount = nativeReader.ReadInt32BigEndian();
					int entriesOffset = nativeReader.ReadInt32BigEndian();
					int num = nativeReader.ReadInt32BigEndian();
					int e = nativeReader.ReadInt32BigEndian();
					int f = nativeReader.ReadInt32BigEndian();
					bool isInPatch = false;
					byte catalog = 0;
					byte cas = 0;
					nativeReader.Position = startPosition + flagsOffset;
					bundle.Flags = nativeReader.ReadBytes(entriesCount);
					nativeReader.Position = startPosition + entriesOffset;
					for (int j2 = 0; j2 < entriesCount; j2++)
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
					}
					CasBundles.Add(bundle);
					nativeReader.Position = startPosition + flagsOffset + entriesCount;
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

		/*
		public void Write(Stream stream)
        {
			{
				NativeWriter writer = new NativeWriter(stream);
				writer.WriteInt32BigEndian(MetaData.Magic);
				long startPosition = stream.Position;
				writer.WriteInt32BigEndian(60);
				long bundleDataOffsetPosition = writer.Position;
				writer.WriteInt32BigEndian(0);
				writer.WriteInt32BigEndian(Bundles.Count);
				long chunkFlagsOffsetPosition = writer.Position;
				writer.WriteInt32BigEndian(0);
				long chunkGuidOffsetPosition = writer.Position;
				writer.WriteInt32BigEndian(0);
				writer.WriteInt32BigEndian(TocChunks.Count);
				long chunkEntryOffsetPosition = writer.Position;
				writer.WriteInt32BigEndian(0);
				writer.WriteInt32BigEndian(0);
				long offset2Position = writer.Position;
				writer.WriteInt32BigEndian(0);
				writer.WriteInt32BigEndian(0);
				writer.WriteInt32BigEndian(tocFile.UnknownValue4);
				writer.WriteInt32BigEndian(tocFile.UnknownValue5);
				writer.WriteInt32BigEndian(tocFile.offset2Values.Count);
				writer.WriteInt32BigEndian(tocFile.offset8Values.Count);
				long offset8Position = writer.Position;
				writer.WriteInt32BigEndian(0);
				foreach (int bundleFlag in tocFile.bundleFlags)
				{
					writer.WriteInt32BigEndian(bundleFlag);
				}
				while ((writer.Position - startPosition) % 8 != 0L)
				{
					writer.Write((byte)0);
				}
				long bundleDataOffset = writer.Position;
				foreach (var (unk3, length2, offset3) in tocFile.Bundles)
				{
					writer.WriteInt32BigEndian(unk3);
					writer.WriteInt32BigEndian(length2);
					writer.WriteInt64BigEndian(offset3);
				}
				long chunkFlagsOffset = writer.Position;
				foreach (int chunkFlag in tocFile.ChunkFlags)
				{
					writer.WriteInt32BigEndian(chunkFlag);
				}
				long chunkGuidOffset = writer.Position;
				foreach (var (chunkGuid, chunkIndex) in tocFile.chunkGuids)
				{
					foreach (byte guidByte in chunkGuid.ToByteArray().Reverse())
					{
						writer.Write(guidByte);
					}
					writer.WriteInt32BigEndian(chunkIndex);
				}
				long chunkEntryOffset = writer.Position;
				foreach (var (unk2, patch, catalog, cas, offset2, size) in TocChunks)
				{
					writer.Write(unk2);
					writer.Write((sbyte)(patch ? 1 : 0));
					writer.Write(catalog);
					writer.Write(cas);
					writer.WriteUInt32BigEndian(offset2);
					writer.WriteUInt32BigEndian(size);
				}
				long offset4 = writer.Position;
				foreach (int offset2Value in tocFile.offset2Values)
				{
					writer.WriteInt32BigEndian(offset2Value);
				}
				long offset5 = writer.Position;
				foreach (int offset8Value in tocFile.offset8Values)
				{
					writer.WriteInt32BigEndian(offset8Value);
				}
				if (CasBundles.Count > 0)
				{
					List<long> newOffsets = new List<long>(CasBundles.Count);
					foreach (var casBundle in CasBundles)
					{
						byte[] flags = ArrayPool<byte>.Shared.Rent(casBundle.Entries.Count + 1);
						int entryIndex = 0;
						int currentCasIdentifier = CasFile.CreateCasIdentifier(0, casBundle.InPatch, (byte)casBundle.CasCatalog, (byte)casBundle.CasIndex);
						flags[entryIndex] = 1;
						foreach (TocFile_F21.CasBundleEntry entry2 in casBundle.Entries)
						{
							entryIndex++;
							int num = CasFile.CreateCasIdentifier(0, entry2.InPatch, (byte)entry2.CasCatalog, (byte)entry2.CasIndex);
							if (num != currentCasIdentifier)
							{
								flags[entryIndex] = 1;
							}
							else
							{
								flags[entryIndex] = 0;
							}
							currentCasIdentifier = num;
						}
						long placeholderPosition = writer.Position;
						newOffsets.Add(placeholderPosition);
						writer.WriteInt64BigEndian(0L);
						writer.WriteInt32BigEndian(0);
						writer.WriteInt32BigEndian(casBundle.Entries.Count + 1);
						writer.WriteInt32BigEndian(32);
						writer.WriteInt32BigEndian(32);
						writer.WriteInt32BigEndian(32);
						writer.WriteInt32BigEndian(0);
						int casIdentifier = CasFile.CreateCasIdentifier(0, casBundle.InPatch, (byte)casBundle.CasCatalog, (byte)casBundle.CasIndex);
						writer.WriteInt32BigEndian(casIdentifier);
						_ = casBundle.BundleOffset;
						_ = int.MaxValue;
						writer.WriteInt32BigEndian((int)casBundle.BundleOffset);
						writer.WriteInt32BigEndian(casBundle.BundleSize);
						entryIndex = 0;
						foreach (TocFile_F21.CasBundleEntry entry in casBundle.Entries)
						{
							entryIndex++;
							if (flags[entryIndex] == 1)
							{
								casIdentifier = CasFile.CreateCasIdentifier(0, entry.InPatch, (byte)entry.CasCatalog, (byte)entry.CasIndex);
								writer.WriteInt32BigEndian(casIdentifier);
							}
							writer.WriteUInt32BigEndian(entry.EntryOffset);
							writer.WriteInt32BigEndian(entry.EntrySize);
						}
						int flagsOffset = (int)(writer.Position - placeholderPosition);
						writer.WriteBytes(flags, 0, casBundle.Entries.Count + 1);
						long endPosition = writer.Position;
						writer.Position = placeholderPosition;
						writer.WriteInt64BigEndian(0L);
						writer.WriteInt32BigEndian(flagsOffset);
						writer.WriteInt32BigEndian(casBundle.Entries.Count + 1);
						writer.WriteInt32BigEndian(32);
						writer.WriteInt32BigEndian(32);
						writer.WriteInt32BigEndian(32);
						writer.Position = endPosition;
					}
					writer.Position = bundleDataOffset;
					int i = 0;
					foreach (var bundle in Bundles)
					{
						writer.WriteInt32BigEndian(bundle.);
						writer.WriteInt32BigEndian(length);
						long newOffset = newOffsets[i] - 556;
						writer.WriteInt64BigEndian(newOffset);
						i++;
					}
				}
				writer.Position = bundleDataOffsetPosition;
				writer.WriteInt32BigEndian((int)(bundleDataOffset - startPosition));
				writer.Position = chunkFlagsOffsetPosition;
				writer.WriteInt32BigEndian((int)(chunkFlagsOffset - startPosition));
				writer.Position = chunkGuidOffsetPosition;
				writer.WriteInt32BigEndian((int)(chunkGuidOffset - startPosition));
				writer.Position = chunkEntryOffsetPosition;
				writer.WriteInt32BigEndian((int)(chunkEntryOffset - startPosition));
				writer.WriteInt32BigEndian((int)(chunkEntryOffset - startPosition));
				writer.Position = offset2Position;
				writer.WriteInt32BigEndian((int)(offset4 - startPosition));
				writer.WriteInt32BigEndian((int)(chunkEntryOffset - startPosition));
				writer.Position = offset8Position;
				writer.WriteInt32BigEndian((int)(offset5 - startPosition));
			}
		}
		*/

	}

	
}
