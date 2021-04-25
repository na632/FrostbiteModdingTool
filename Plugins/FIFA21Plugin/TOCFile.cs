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

        //public int[] ArrayOfInitialHeaderData = new int[12];

        public ContainerMetaData MetaData = new ContainerMetaData();
		public List<BaseBundleInfo> Bundles = new List<BaseBundleInfo>();

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
			public int unk7Offset { get; set; }
			public int Offset7 { get; set; }
			public int CountOfSomething { get; set; }
			public int CountOfSomething2 { get; set; }
			public int unk11Count { get; set; }
			public int unk12Count { get; set; }
			public int unk13Offset { get; set; }

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
				unk7Offset = nativeReader.ReadInt(Endian.Big); // 36
				Offset7 = nativeReader.ReadInt(Endian.Big); // 40
				CountOfSomething = nativeReader.ReadInt(Endian.Big); // 44
				CountOfSomething2 = nativeReader.ReadInt(Endian.Big); // 48
				unk11Count = nativeReader.ReadInt(Endian.Big); // 52
				unk12Count = nativeReader.ReadInt(Endian.Big); // 56
				unk13Offset = nativeReader.ReadInt(Endian.Big); // 60
			}
        }

		public int[] tocMetaData = new int[15];

		public void Read(NativeReader nativeReader)
		{
			var startPosition = nativeReader.Position;
			if (File.Exists("debugToc.dat"))
				File.Delete("debugToc.dat");

			//nativeReader.Position = 0;
			//using (NativeWriter writer = new NativeWriter(new FileStream("debugToc.dat", FileMode.OpenOrCreate)))
			//{
			//	writer.Write(nativeReader.ReadToEnd());
			//}
			nativeReader.Position = 0;

			//AssetManager.Instance.logger.Log("Seaching for Internal TOC Bundles");

			BoyerMoore boyerMoore = new BoyerMoore(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x3C });
			var findInternalPatterns = boyerMoore.SearchAll(nativeReader.ReadToEnd());
			nativeReader.Position = startPosition;

			//AssetManager.Instance.logger.Log($"{findInternalPatterns.Count} Internal TOC Bundles found");

			foreach (var internalPos in findInternalPatterns)
			{
				var actualInternalPos = internalPos + 4;
				//var actualInternalPos = 556L;

				nativeReader.Position = actualInternalPos;
				var magic = nativeReader.ReadInt(Endian.Big);
				if (magic != 0x3c)
					throw new Exception("Magic is not the expected value of 0x3c");

				nativeReader.Position -= 4;

				MetaData.Read(nativeReader);

				List<int> bundleReferences = new List<int>();

				// Obviously not usable in standard way
				//if (MetaData.BundleCount == 32)
				{
					//nativeReader.Position = 556 + MetaData.Offset6;
					//var newOffset = nativeReader.ReadInt(Endian.Big) + 556;
					//nativeReader.Position = newOffset;
					//var nextOffset = nativeReader.ReadInt(Endian.Big) + 556;

				}
				//else
				{


					if (MetaData.BundleCount > 0 && MetaData.BundleCount != MetaData.BundleOffset)
					{
                        //for (int index = 0; index < MetaData.BundleCount; index++)
                        //{
                        //	bundleReferences.Add((int)nativeReader.ReadUInt(Endian.Big));
                        //}
                        nativeReader.Position = actualInternalPos + MetaData.BundleOffset;
                        for (int indexOfBundleCount = 0; indexOfBundleCount < MetaData.BundleCount; indexOfBundleCount++)
                        {

                            int offset1 = nativeReader.ReadInt(Endian.Big);


							var tocsizeposition = nativeReader.Position;
                            int size = nativeReader.ReadInt(Endian.Big);

                            nativeReader.ReadInt(Endian.Big); // unknown

                            int dataOffset = nativeReader.ReadInt(Endian.Big);

							BaseBundleInfo newBundleInfo = new BaseBundleInfo
							{
								TocOffset = offset1,
								Offset = dataOffset,
								Size = size,
								TOCSizePosition = tocsizeposition
							};
                            Bundles.Add(newBundleInfo);
                        }

                        var chunks = new List<ChunkAssetEntry>();

						if (MetaData.ChunkFlagOffset != 0 && MetaData.ChunkFlagOffset != 32)
						{
							if (MetaData.ChunkCount > 0)
							{
								AssetManager.Instance.logger.Log($"Found {MetaData.ChunkCount} TOC Chunks");

								nativeReader.Position = actualInternalPos + MetaData.ChunkFlagOffset;
								List<int> list7 = new List<int>();
								for (int num13 = 0; num13 < MetaData.ChunkCount; num13++)
								{
									list7.Add(nativeReader.ReadInt(Endian.Big));
								}
								nativeReader.Position = actualInternalPos + MetaData.ChunkGuidOffset;


								List<Guid> tocChunkGuids = new List<Guid>();
								for (int num14 = 0; num14 < MetaData.ChunkCount; num14++)
								{
									byte[] array6 = nativeReader.ReadBytes(16);
									Guid tocChunkGuid = new Guid(new byte[16]
									{
										array6[15],
										array6[14],
										array6[13],
										array6[12],
										array6[11],
										array6[10],
										array6[9],
										array6[8],
										array6[7],
										array6[6],
										array6[5],
										array6[4],
										array6[3],
										array6[2],
										array6[1],
										array6[0]
									});
									//nativeReader.Position -= 16;
									//Guid value2 = nativeReader.ReadGuid(Endian.Little);
									//nativeReader.Position -= 16;
									//Guid value3 = nativeReader.ReadGuid(Endian.Big);
									
									int num15 = nativeReader.ReadInt(Endian.Big) & 0xFFFFFF;
									while (tocChunkGuids.Count <= num15)
									{
										tocChunkGuids.Add(Guid.Empty);
									}
									tocChunkGuids[num15 / 3] = tocChunkGuid;

									if(tocChunkGuid.ToString() == "dbb8c69e-38fa-eeff-3dd5-cebb88ca6df9")
                                    {

                                    }
								}
								nativeReader.Position = actualInternalPos + MetaData.ChunkEntryOffset;


								//ParentReader.AssetManager.logger.Log($"Found {MetaData.ChunkCount} Chunks in TOC");

								//if (NativeFileLocation.Contains("matchcinematicssba.toc"))
								//	return;

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
									chunkAssetEntry2.Id = tocChunkGuids[chunkIndex];
									if (chunkAssetEntry2.Id.ToString() == "dbb8c69e-38fa-eeff-3dd5-cebb88ca6df9")
									{

									}
									// Generate a Sha1 since we dont have one.
									chunkAssetEntry2.Sha1 = Sha1.Create(Encoding.ASCII.GetBytes(chunkAssetEntry2.Id.ToString()));

									chunkAssetEntry2.LogicalOffset = 0;
									chunkAssetEntry2.OriginalSize = (chunkAssetEntry2.LogicalOffset & 0xFFFF) | chunkSize;

									chunkAssetEntry2.Size = chunkSize;
									chunkAssetEntry2.Location = AssetDataLocation.CasNonIndexed;
									chunkAssetEntry2.ExtraData = new AssetExtraData();
									chunkAssetEntry2.ExtraData.CasPath = AssetManager.Instance.fs.GetFilePath(catalog2, cas2, patch2);
									chunkAssetEntry2.ExtraData.DataOffset = chunkOffset;

									chunks.Add(chunkAssetEntry2);
								}

								_ = nativeReader.Position;
								for (int chunkIndex = 0; chunkIndex < MetaData.ChunkCount; chunkIndex++)
								{
									
									var chunkAssetEntry = chunks[chunkIndex];
									AssetManager.Instance.AddChunk(chunkAssetEntry);

								}
							}
							int[] unk7Values = new int[MetaData.unk11Count];
							if (nativeReader.Position != 556 + MetaData.unk7Offset)
							{
								nativeReader.Position = 556 + MetaData.unk7Offset;
							}
							for (int k = 0; k < MetaData.unk11Count; k++)
							{
								unk7Values[k] = nativeReader.ReadInt32BigEndian();
							}
                            int[] unk13Values = new int[MetaData.unk12Count];
                            if (nativeReader.Position != 556 + MetaData.unk13Offset)
                            {
                                nativeReader.Position = 556 + MetaData.unk13Offset;
                            }
                            for (int j = 0; j < MetaData.unk12Count; j++)
                            {
                                unk13Values[j] = nativeReader.ReadInt32BigEndian();
                            }

                            _ = nativeReader.Position;
							if(nativeReader.Position < nativeReader.Length)
                            {
								TOCCasDataLoader casDataLoader = new TOCCasDataLoader(this);
								casDataLoader.Load2(nativeReader);
							}



						}
					}

					//if (MetaData.Offset5 > 128)
					//{
					//	SBFile sbFile = new SBFile(ParentReader, this, 0);
					//	BaseBundleInfo newBundleInfo = new BaseBundleInfo
					//	{
					//		TocOffset = 0,
					//		Offset = MetaData.Offset5,
					//		Size = MetaData.Offset6
					//	};
					//	//Bundles.Add(newBundleInfo);
					//}
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
	}

	
}
