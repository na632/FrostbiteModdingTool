using FrostbiteSdk.Frostbite.FileManagers;
using Frosty.Hash;
using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frostbite.FileManagers
{
	public class LegacyFileManager_M21 : ILegacyFileManager, ICustomAssetManager
	{

		public List<LegacyFileEntry> AddedFileEntries { get; set; }

		public static List<LegacyFileEntry> OriginalLegacyEntries = new List<LegacyFileEntry>();

		public static Dictionary<int, LegacyFileEntry> LegacyEntries = new Dictionary<int, LegacyFileEntry>();

		public static List<ChunkAssetEntry> ModifiedChunks = new List<ChunkAssetEntry>();

		//private Dictionary<Guid, byte[]> cachedChunks = new Dictionary<Guid, byte[]>();

		//private bool cacheMode;

		Dictionary<Guid, List<LegacyFileEntry>> LegacyChunksToParent = new Dictionary<Guid, List<LegacyFileEntry>>();

		public AssetManager AssetManager => AssetManager.Instance;

		public class ChunkBatch
		{
			public uint UnkCount1 { get; set; }
			public long UnkOffset1 { get; set; }
			public int NumberOfFiles { get; set; }

			public long FileFirstPosition { get; set; }

			public uint BootableItemCount { get; set; }


			public byte[] Initial64Data { get; set; }

			public EbxAssetEntry EbxAssetEntry { get; set; }

			public ChunkAssetEntry ChunkAssetEntry { get; set; }

			//       public IEnumerable<LegacyFileEntry> BatchLegacyFiles
			//       {
			//           get
			//           {
			//return LegacyEntries.Values.Where(x => x.ParentGuid == ChunkAssetEntry.Id);
			//           }
			//       }

			public List<LegacyFileEntry> BatchLegacyFiles = new List<LegacyFileEntry>();

			public ChunkBatch() 
			{
			}

			public override string ToString()
			{
				return ChunkAssetEntry != null ? ChunkAssetEntry.Id.ToString() : base.ToString();
			}

			public override bool Equals(object obj)
			{
				return base.Equals(obj);
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public byte[] WriteChunkBatch()
			{
				return null;
			}

			public Dictionary<Guid, List<LegacyFileEntry>> ChunkGroupsInBatch
			{
				get
				{
					return BatchLegacyFiles.GroupBy(x => x.ChunkId).ToDictionary(x => x.Key, x => x.ToList());
				}
			}

			public Dictionary<Guid, ChunkAssetEntry> ChunkAssetEntriesInBatch
			{
				get
				{
					return BatchLegacyFiles.GroupBy(x => x.ChunkId).ToDictionary(x => x.Key, x => AssetManager.Instance.GetChunkEntry(x.Key));
				}
			}

			public LegacyFileEntry GetLegacyFileByName(string name)
			{
				return BatchLegacyFiles.FirstOrDefault(x => x.Name == name);
			}

			public int EndOfStrings { get; internal set; }
			public byte[] EndData { get; internal set; }
            public long BootableItemOffset { get; internal set; }
            public uint LinkedChunkCount { get; internal set; }
            public long LinkedChunkOffset { get; internal set; }
            public EbxAsset EbxAsset { get; internal set; }

            public List<ChunkAssetEntry> CompressedItemChunks = new List<ChunkAssetEntry>();

			public List<BootableItem> BootableItems = new List<BootableItem>();

			public List<ChunkAssetEntry> LinkedChunks = new List<ChunkAssetEntry>();

			public class BootableItem
            {
				public long nameOffset3;
				public string unkName2;
				public uint unknumber1;
				public uint unknumber2;
			}
        }

		/// <summary>
		/// Chunk Batches are the entire batch of chunks and locations of the chunks. Each is an EBX
		/// </summary>
		public static List<ChunkBatch> ChunkBatches = new List<ChunkBatch>();

        public LegacyFileManager_M21()
        {
        }

        public void Initialize(ILogger logger)
		{
			logger.Log("Loading legacy files");
			AddedFileEntries = new List<LegacyFileEntry>();

			ChunkBatches = new List<ChunkBatch>();

            foreach (EbxAssetEntry item in AssetManager.EnumerateEbx("ChunkFileCollector"))
            //foreach (EbxAssetEntry item in AssetManager.EnumerateEbx("CFC_GM_Launch"))
            {
				GetChunkAssetForEbx(item, out ChunkAssetEntry chunkAssetEntry, out EbxAsset ebxAsset);
				if (chunkAssetEntry == null)
					continue;

				Stream chunk = GetChunkStreamForEbx(item);
				ReadChunk(chunk, ebxAsset, item, chunkAssetEntry);
			}

			logger.Log($"Loaded {LegacyEntries.Count} legacy files");
			LegacyFileManager.Instance = this;
		}

		public void ReadChunk(Stream chunk, EbxAsset ebxAsset, EbxAssetEntry item, ChunkAssetEntry chunkAssetEntry)
        {
			if (chunk != null)
			{
				using (NativeReader nativeReader = new NativeReader(chunk))
				{
					byte[] allbytesofbatch = nativeReader.ReadToEnd();
					nativeReader.Position = 0;

					var chunkBatch = new ChunkBatch()
					{
						EbxAssetEntry = item,
						EbxAsset = ebxAsset,
						ChunkAssetEntry = chunkAssetEntry,
						UnkCount1 = nativeReader.ReadUInt(), // 0
						UnkOffset1 = nativeReader.ReadLong(), // 4
						NumberOfFiles = nativeReader.ReadInt(), // 12
						FileFirstPosition = nativeReader.ReadLong(), // 16
						BootableItemCount = nativeReader.ReadUInt(), // 24 
						BootableItemOffset = nativeReader.ReadLong(), // 28
						LinkedChunkCount = nativeReader.ReadUInt(), // 36
						LinkedChunkOffset = nativeReader.ReadLong() // 40
					};
					//nativeReader.Position = 0;
					//chunkBatch.Initial64Data = nativeReader.ReadBytes((int)chunkBatch.FileFirstPosition);

					chunkBatch.EndOfStrings = 0;
					nativeReader.Position = chunkBatch.FileFirstPosition;
					for (uint index = 0u; index < chunkBatch.NumberOfFiles; index++)
					{
						var positionOfText = nativeReader.ReadLong();
						var positionOfItem = nativeReader.Position;

						nativeReader.Position = positionOfText;

						string name = nativeReader.ReadNullTerminatedString();
						if (nativeReader.Position > chunkBatch.EndOfStrings)
							chunkBatch.EndOfStrings = (int)nativeReader.Position;

						nativeReader.Position = positionOfItem;
						int key = Fnv1.HashString(name);
						LegacyFileEntry legacyFileEntry = null;
						if (!LegacyEntries.ContainsKey(key))
						{
							legacyFileEntry = new LegacyFileEntry();
							legacyFileEntry.Name = name;
							LegacyEntries.Add(key, legacyFileEntry);
						}
						else
						{
							legacyFileEntry = LegacyEntries[key];
						}
						legacyFileEntry.BatchOffset = positionOfItem;
						legacyFileEntry.ParentGuid = chunkAssetEntry.Id;


						legacyFileEntry.CompressedOffsetPosition = nativeReader.Position;
						legacyFileEntry.CompressedOffset = nativeReader.ReadLong();
						legacyFileEntry.CompressedOffsetStart = legacyFileEntry.CompressedOffset;
						legacyFileEntry.CompressedSizePosition = nativeReader.Position;
						//legacyFileEntry.CompressedOffsetEnd = nativeReader.ReadLong();
						//legacyFileEntry.CompressedSize = legacyFileEntry.CompressedOffsetEnd - legacyFileEntry.CompressedOffset;
						legacyFileEntry.CompressedSize = nativeReader.ReadLong() - legacyFileEntry.CompressedOffset;

						legacyFileEntry.ActualOffsetPosition = (int)nativeReader.Position;
						legacyFileEntry.ExtraData = new AssetExtraData() { DataOffset = (uint)nativeReader.ReadLong() };
						legacyFileEntry.ActualSizePosition = (int)nativeReader.Position;
						legacyFileEntry.Size = nativeReader.ReadLong();

						legacyFileEntry.ChunkIdPosition = nativeReader.Position;
						var chunkId = nativeReader.ReadGuid();
						legacyFileEntry.ChunkId = chunkId;

						chunkBatch.BatchLegacyFiles.Add(legacyFileEntry);
					}

					if (ProfilesLibrary.IsFIFA21DataVersion())
					{
						nativeReader.Position = chunkBatch.BootableItemOffset;
						for (uint j = 0u; j < chunkBatch.BootableItemCount; j++)
						{
							long nameOffset3 = nativeReader.ReadLong();
							long currentPosition3 = nativeReader.Position;
							nativeReader.Position = nameOffset3;
							var unkName2 = nativeReader.ReadNullTerminatedString();
							nativeReader.Position = currentPosition3;
							var unknumber1 = nativeReader.ReadUInt();
							var unknumber2 = nativeReader.ReadUInt();

							ChunkBatch.BootableItem bootableItem = new ChunkBatch.BootableItem()
							{
								nameOffset3 = nameOffset3,
								unkName2 = unkName2,
								unknumber1 = unknumber1,
								unknumber2 = unknumber2
							};
							chunkBatch.BootableItems.Add(bootableItem);

						}

						nativeReader.Position = chunkBatch.LinkedChunkOffset;
						for (uint i = 0u; i < chunkBatch.LinkedChunkCount; i++)
						{
							var chunkSize = nativeReader.ReadLong();
							var chunkid2 = nativeReader.ReadGuid();
							var otherChunk = AssetManager.Instance.GetChunkEntry(chunkid2);
							otherChunk.OriginalSize = chunkSize;

							chunkBatch.LinkedChunks.Add(otherChunk);
						}
					}

					ChunkBatches.Add(chunkBatch);




				}
			}
        }

        public void WriteAllLegacy()
		{
			// A chunk batch corresponds to each Ebx in the CFC
			foreach (ChunkBatch chunkBatch in ChunkBatches)
			{
				//AssetManager.Instance.ModifyEbx(chunkBatch, chunkBatch);
			}
		}

		public static EbxAsset GetEbxAssetForEbx(EbxAssetEntry ebxAssetEntry)
        {
			EbxAsset ebx = AssetManager.Instance.GetEbx(ebxAssetEntry);
			return ebx;
		}

		public static void GetChunkAssetForEbx(EbxAssetEntry ebxAssetEntry, out ChunkAssetEntry chunkAssetEntry, out EbxAsset ebxAsset)
		{
			chunkAssetEntry = null;
			ebxAsset = AssetManager.Instance.GetEbx(ebxAssetEntry);
			if (ebxAsset != null)
			{
				dynamic rootObject = ebxAsset.RootObject;
				if (rootObject != null)
				{
					dynamic val = rootObject.Manifest;
					chunkAssetEntry = AssetManager.Instance.GetChunkEntry(val.ChunkId);
				}
			}
		}

		public Stream GetChunkStreamForEbx(EbxAssetEntry ebxAssetEntry)
		{
			GetChunkAssetForEbx(ebxAssetEntry, out ChunkAssetEntry chunkAssetEntry, out EbxAsset asset);
			if (chunkAssetEntry != null)
				return AssetManager.GetChunk(chunkAssetEntry);

			return null;
		}

		public void SetCacheModeEnabled(bool enabled)
		{
			//cacheMode = enabled;
		}

		public void FlushCache()
		{
			//cachedChunks.Clear();
		}

		public IEnumerable<AssetEntry> EnumerateAssets(bool modifiedOnly)
		{
			var lE = LegacyEntries.Where(x =>
				!modifiedOnly
				|| x.Value.HasModifiedData);
			return lE.Select(x => x.Value);
		}

		public AssetEntry GetAssetEntry(string key)
		{
			LegacyFileEntry legacyFileEntry = null;
			int key2 = Fnv1.HashString(key);
			if (LegacyEntries.ContainsKey(key2))
			{
				legacyFileEntry = LegacyEntries[key2];
			}
			return legacyFileEntry;
		}

		public LegacyFileEntry GetLFEntry(string key)
		{
			return GetAssetEntry(key) as LegacyFileEntry;
		}

		long? testDO;

		public Stream GetAsset(AssetEntry entry)
		{
			LegacyFileEntry legacyFileEntry = entry as LegacyFileEntry;
			if (legacyFileEntry.HasModifiedData)
			{
				return new MemoryStream(legacyFileEntry.ModifiedEntry.Data);
			}

			Stream chunkStream = GetChunkStream(legacyFileEntry);
			if (chunkStream == null)
			{
				return null;
			}
			using (NativeReader nativeReader = new NativeReader(chunkStream))
			{
				// 
				if (legacyFileEntry.Name.Contains("voicecommand"))
                {
					if (!testDO.HasValue)
						testDO = legacyFileEntry.ExtraData.DataOffset;
					else if (testDO.Value != legacyFileEntry.ExtraData.DataOffset)
						throw new Exception("these should be the same");
					
				}
				nativeReader.Position = legacyFileEntry.ExtraData.DataOffset;
				return new MemoryStream(nativeReader.ReadBytes((int)legacyFileEntry.Size));
			}
		}

		//public void RebuildOneFile(LegacyFileEntry legacyFileEntry)
		//{
		//	AssetManager.ModifyChunk(legacyFileEntry.ChunkId, legacyFileEntry.ModifiedEntry.Data);
		//}

		public async Task<ChunkAssetEntry> RebuildEntireChunkAsync(Guid chunkId, List<LegacyFileEntry> replaceFileEntries, List<LegacyFileEntry> newFileEntries = null)
		{
			return await Task.Run(() => { return RebuildEntireChunk(chunkId, replaceFileEntries, newFileEntries); });
		}

		public ChunkAssetEntry RebuildEntireChunk(Guid chunkId, List<LegacyFileEntry> replaceFileEntries, List<LegacyFileEntry> newFileEntries = null)
		{
			//CompressionType compressionType = ProfilesLibrary.IsMadden21DataVersion() ? CompressionType.LZ4 : CompressionType.Oodle;
			CompressionType compressionType = ProfilesLibrary.IsMadden21DataVersion() ? CompressionType.LZ4 : CompressionType.ZStd;

			ChunkBatch chunkBatch;
            // get the chunk batch (the main batch with offsets etc)
            chunkBatch = ChunkBatches.FirstOrDefault(x => x.ChunkAssetEntry.Id == chunkId);
			if (chunkBatch != null)
			{
				var edited = replaceFileEntries.GroupBy(x => x.ChunkId).ToDictionary(x => x.Key, x => x.ToList());

				var edited2 = chunkBatch.BatchLegacyFiles.Where(x => x.ModifiedEntry != null).GroupBy(x => x.ChunkId).ToDictionary(x => x.Key, x => x.ToList());
				foreach (var gItem in edited)
				{
					// Easily handle Singular Chunk
					if (gItem.Value.Count == 1 && chunkBatch.ChunkGroupsInBatch[gItem.Value.First().ChunkId].Count == 1)
					{
						var legacyItem = gItem.Value.First();
						//legacyItem.CompressedOffset = 0;
						//legacyItem.CompressedSize = 0;
						//legacyItem.ExtraData.DataOffset = 0;
						//legacyItem.Size = legacyItem.ModifiedEntry.Data.Length;
						legacyItem.ModifiedEntry.NewOffset = 0;
						legacyItem.ModifiedEntry.Size = legacyItem.ModifiedEntry.Data.Length;
						legacyItem.ModifiedEntry.CompressedOffset = 0;
						ModifiedChunks.Add(AssetManager.Instance.GetChunkEntry(gItem.Key));
						AssetManager.Instance.ModifyChunk(gItem.Key, legacyItem.ModifiedEntry.Data, null, compressionType,  addToChunkBundle: true);
						legacyItem.ModifiedEntry.CompressedSize = AssetManager.Instance.GetChunk(gItem.Key).Length;

					}
					// Otherwise handle Chunk Batch
					else
					{
						var groupOfLegacyFiles = chunkBatch.ChunkGroupsInBatch.First(x => x.Key == gItem.Key);
						var groupChunkEntry = AssetManager.Instance.GetChunkEntry(gItem.Key);
						var groupChunk = AssetManager.Instance.GetChunk(groupChunkEntry);

						MemoryStream ms_newChunkGroup = new MemoryStream();
						MemoryStream ms_newChunkGroupCompressed = new MemoryStream();

						using (var nw_newChunkGroupCompressed = new NativeWriter(ms_newChunkGroupCompressed, leaveOpen: true))
						{
							using (var nw_newChunkGroup = new NativeWriter(ms_newChunkGroup, leaveOpen: true))
							{
								using (var nr_GroupChunk = new NativeReader(groupChunk))
								{
									int currentCompressedLocation = 0;
									foreach (var itemInChunkGroup in groupOfLegacyFiles.Value.OrderBy(x => x.ModifiedEntry == null))
									{
										byte[] origData = null;

										if (itemInChunkGroup.HasModifiedData)
										{
											itemInChunkGroup.ModifiedEntry.NewOffset = (uint)nw_newChunkGroup.Position;
											itemInChunkGroup.ModifiedEntry.Size = itemInChunkGroup.ModifiedEntry.Data.Length;
											nw_newChunkGroup.Write(itemInChunkGroup.ModifiedEntry.Data);

										}
										else
										{
											nr_GroupChunk.Position = itemInChunkGroup.ExtraData.DataOffset;
											origData = nr_GroupChunk.ReadBytes((int)itemInChunkGroup.Size);
											itemInChunkGroup.ModifiedEntry = new ModifiedAssetEntry()
											{
												NewOffset = (uint)nw_newChunkGroup.Position
											};
											nw_newChunkGroup.Write(origData);
										}

										itemInChunkGroup.ModifiedEntry.CompressedOffset = currentCompressedLocation;
										var compressedData = itemInChunkGroup.ModifiedEntry.Data != null ? itemInChunkGroup.ModifiedEntry.CompressedDataZstd
											: Utils.CompressFile(origData, compressionOverride: CompressionType.ZStd);

										currentCompressedLocation += compressedData.Length;

										itemInChunkGroup.ModifiedEntry.CompressedSize = currentCompressedLocation - itemInChunkGroup.ModifiedEntry.CompressedOffset;
										if (Utils.MaxBufferSize < currentCompressedLocation)
										{
											currentCompressedLocation = 0;
										}

										nw_newChunkGroupCompressed.Write(compressedData);

									}
								}
							}
						}

						//var recompressedChunk = RecompressChunk(ms_newChunkGroup, groupOfLegacyFiles.Value,
						//	replaceFileEntries.ToDictionary(x => x.NameHash, x => x.ModifiedEntry.Data)
						//	);
						//AssetManager.Instance.ModifyChunk(gItem.Key, newChunkGroupData, compressionOverride: compressionType);
						//AssetManager.Instance.ModifyChunk(gItem.Key, ms_newChunkGroupCompressed.ToArray(), compressionOverride: compressionType);
						AssetManager.Instance.ModifyChunk(gItem.Key, ms_newChunkGroup.ToArray(), compressionOverride: compressionType, addToChunkBundle: true);

						//var groupOfLegacyFiles = chunkBatch.ChunkGroupsInBatch.First(x => x.Key == gItem.Key);

						//var ms_newChunkGroup = new MemoryStream();

						//using (var nw_newChunkGroup = new NativeWriter(ms_newChunkGroup, leaveOpen: true))
						//{
						//	var groupChunkEntry = AssetManager.Instance.GetChunkEntry(gItem.Key);
						//	var groupChunk = AssetManager.Instance.GetChunk(groupChunkEntry);

						//	using (var nr_GroupChunk = new NativeReader(groupChunk))
						//	{
						//                          foreach (var itemInChunkGroup in groupOfLegacyFiles.Value.OrderBy(x => x.ModifiedEntry == null))
						//                          {
						//                              if (itemInChunkGroup.HasModifiedData)
						//                              {
						//                                  itemInChunkGroup.ModifiedEntry.NewOffset = (uint)nw_newChunkGroup.Position;
						//                                  itemInChunkGroup.ModifiedEntry.Size = itemInChunkGroup.ModifiedEntry.Data.Length;
						//				nw_newChunkGroup.Write(itemInChunkGroup.ModifiedEntry.Data);
						//                              }
						//                              else
						//                              {
						//                                  nr_GroupChunk.Position = itemInChunkGroup.ExtraData.DataOffset;
						//                                  var d = nr_GroupChunk.ReadBytes((int)itemInChunkGroup.Size);
						//				itemInChunkGroup.ModifiedEntry = new ModifiedAssetEntry()
						//                                  {
						//                                      NewOffset = (uint)nw_newChunkGroup.Position
						//				};
						//                                  nw_newChunkGroup.Write(d);
						//                              }
						//                          }
						//	}
						//}


						//// Modify the Chunk
						//ms_newChunkGroup.Position = 0;
						//byte[] newChunkGroupData = ms_newChunkGroup.ToArray();


						//ModifiedChunks.Add(AssetManager.Instance.GetChunkEntry(gItem.Key));

						////AssetManager.Instance.ModifyChunk(gItem.Key, newChunkGroupData, compressionOverride: CompressionType.Oodle);
						//AssetManager.Instance.ModifyChunk(gItem.Key, newChunkGroupData, compressionOverride: compressionType);


						//var compressedSize = AssetManager.Instance.GetChunkEntry(gItem.Key).ModifiedEntry.Data.Length;

						//foreach (var itemInChunkGroup in groupOfLegacyFiles.Value.OrderBy(x => x.ModifiedEntry == null))
						//{
						//	//itemInChunkGroup.ModifiedEntry.CompressedSize = compressedSize;
						//}


					}
				}

				byte[] oldBatchData = ((MemoryStream)AssetManager.Instance.GetChunk(chunkBatch.ChunkAssetEntry)).ToArray();

                var msNewBatch = new MemoryStream();
                using (var nwNewBatch = new NativeWriter(msNewBatch, leaveOpen: true))
                {
                    nwNewBatch.Write(oldBatchData);
                    foreach (var lfe in chunkBatch.BatchLegacyFiles)
                    {
                        nwNewBatch.Position = lfe.ActualOffsetPosition;
                        if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.NewOffset != null)
                            nwNewBatch.Write((long)lfe.ModifiedEntry.NewOffset);
                        else
                            nwNewBatch.Write((long)lfe.ExtraData.DataOffset);
                        nwNewBatch.Position = lfe.ActualSizePosition;
                        if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.Data != null)
                            nwNewBatch.Write((long)lfe.ModifiedEntry.Data.Length);
                        else
                            nwNewBatch.Write((long)lfe.Size);

						// Compressed Offset
                        nwNewBatch.Position = lfe.CompressedOffsetPosition;
                        //if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.Data != null)
                        if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.CompressedOffset != null)
							nwNewBatch.Write((long)lfe.ModifiedEntry.CompressedOffset);
						else
							nwNewBatch.Write((long)lfe.CompressedOffset);

						// Compressed Size
						nwNewBatch.Position = lfe.CompressedSizePosition;
						if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.CompressedSize != null)
							nwNewBatch.Write((long)lfe.ModifiedEntry.CompressedSize);
						else
							nwNewBatch.Write((long)lfe.CompressedSize);
                    }
				
				}
                msNewBatch.Position = 0;

				//var newBatchData = new NativeReader(msNewBatch).ReadToEnd();
				var newBatchData = msNewBatch.ToArray();

				//AssetManager.Instance.ModifyChunk(chunkBatch.ChunkAssetEntry.Id, newBatchData, compressionOverride: CompressionType.Oodle);

				//if (ProfilesLibrary.IsMadden21DataVersion())
				//{
				//	AssetManager.Instance.ModifyChunk(chunkBatch.ChunkAssetEntry.Id, newBatchData, compressionOverride: CompressionType.LZ4);
				//}
				//else
				//{
				//	AssetManager.Instance.ModifyChunk(chunkBatch.ChunkAssetEntry.Id, newBatchData, compressionOverride: CompressionType.Oodle);
				//}
				ModifiedChunks.Add(AssetManager.Instance.GetChunkEntry(chunkBatch.ChunkAssetEntry.Id));

				AssetManager.Instance.ModifyChunk(chunkBatch.ChunkAssetEntry.Id, newBatchData, compressionOverride: compressionType, addToChunkBundle: true);
				msNewBatch.Close();
                msNewBatch.Dispose();
                GC.Collect();
				GC.WaitForPendingFinalizers();
                return chunkBatch.ChunkAssetEntry;
			}

			return null;
		}

		public void ModifyAsset(string key, byte[] data)
		{
			ModifyAsset(key, data, true);
		}

		public void ModifyAsset(string key, byte[] data, bool rebuildChunk = true)
		{
			int key2 = Fnv1.HashString(key);
			if (LegacyEntries.ContainsKey(key2))
			{
				LegacyFileEntry legacyFileEntry = LegacyEntries[key2];

				if(!OriginalLegacyEntries.Contains(legacyFileEntry))
					OriginalLegacyEntries.Add(legacyFileEntry);

				legacyFileEntry.ModifiedEntry = new ModifiedAssetEntry()
				{
					Data = data,
					RangeStart = 0,
					RangeEnd = (uint)data.Length,
				};

				legacyFileEntry.IsDirty = true;

				if (rebuildChunk)
				{
					RebuildEntireChunk(legacyFileEntry.ParentGuid, new List<LegacyFileEntry>() { legacyFileEntry }, null);
				}
			}
		}

		public void ModifyAssets(Dictionary<string, byte[]> data, bool rebuildChunk = true)
		{
			List<LegacyFileEntry> filesEdited = new List<LegacyFileEntry>();

			foreach (var dpi in data)
			{
				int key2 = Fnv1.HashString(dpi.Key);
				if (LegacyEntries.ContainsKey(key2))
				{
					LegacyFileEntry legacyFileEntry = LegacyEntries[key2];
					legacyFileEntry.ModifiedEntry = new ModifiedAssetEntry()
					{
						Data = dpi.Value,
						CompressedOffset = 0,
						CompressedSize = (uint)dpi.Value.Length,
					};

					legacyFileEntry.IsDirty = true;
					filesEdited.Add(legacyFileEntry);
				}
			}

			if (rebuildChunk)
			{
				var groupedFiles = filesEdited.GroupBy(x => x.ParentGuid).ToDictionary(x => x.Key, x => x.ToList());
				foreach (var grpFile in groupedFiles)
				{
					RebuildEntireChunk(grpFile.Key, grpFile.Value);
				}
			}
		}

		

		/// <summary>
		/// Modifies multiple assets and expects the entries to already has a ModifiedEntry object instance
		/// </summary>
		/// <param name="entries"></param>
		public void LoadEntriesModifiedFromProject(List<LegacyFileEntry> entries)
		{
			var groupedAssets = entries.GroupBy(x => x.ParentGuid).ToDictionary(x => x.Key, x => x.ToList());
			foreach (var group in groupedAssets)
			{
				foreach (var lfe in group.Value)
				{
					int key2 = Fnv1.HashString(lfe.Name);
					LegacyEntries[key2].ModifiedEntry = new ModifiedAssetEntry()
					{
						Data = lfe.ModifiedEntry.Data
						//,
						//NewOffset = lfe.ModifiedEntry.NewOffset
					};
					//LegacyEntries[key2].ExtraData.DataOffset = Convert.ToUInt32(lfe.ModifiedEntry.NewOffset.HasValue ? lfe.ModifiedEntry.NewOffset.Value : lfe.ExtraData.DataOffset);
				}
			}
		}

		/// <summary>
		/// Adds multiple assets and expects the entries to already have a ModifiedEntry object instance
		/// </summary>
		/// <param name="entries"></param>
		public void LoadEntriesAddedFromProject(List<LegacyFileEntry> entries)
		{
			var groupedAssets = entries.GroupBy(x => x.ParentGuid).ToDictionary(x => x.Key, x => x.ToList());
			foreach (var group in groupedAssets)
			{
				foreach (var lfe in group.Value)
				{
					int key2 = Fnv1.HashString(lfe.Name);
					LegacyEntries[key2].ModifiedEntry = new ModifiedAssetEntry()
					{
						Data = lfe.ModifiedEntry.Data
						,
						NewOffset = lfe.ModifiedEntry.NewOffset
					};
					LegacyEntries[key2].ExtraData.DataOffset = Convert.ToUInt32(lfe.ModifiedEntry.NewOffset.HasValue ? lfe.ModifiedEntry.NewOffset.Value : lfe.ExtraData.DataOffset);

				}
			}
			AddedFileEntries = entries;
		}

		private Stream GetChunkStream(LegacyFileEntry lfe)
		{
		
			return AssetManager.GetChunk(AssetManager.GetChunkEntry(lfe.ChunkId));
		}

		public void OnCommand(string command, params object[] value)
		{
			if (!(command == "SetCacheModeEnabled"))
			{
				if (command == "FlushCache")
				{
					FlushCache();
				}
			}
			else
			{
				SetCacheModeEnabled((bool)value[0]);
			}
		}

		

		/// <summary>
		/// Cleans up asset and associated chunks (chunks should not be modified but this checks them over)
		/// </summary>
		/// <param name="entry"></param>
		public void RevertAsset(AssetEntry entry)
		{
			LegacyFileEntry legacyFileEntry = entry as LegacyFileEntry;
			if (legacyFileEntry != null)
			{
				legacyFileEntry.ModifiedEntry = null;
				legacyFileEntry.IsDirty = false;
				var chunkEntry = AssetManager.Instance.GetChunkEntry(legacyFileEntry.ParentGuid);
				AssetManager.Instance.RevertAsset(chunkEntry);
				foreach (EbxAssetEntry item in AssetManager.EnumerateEbx("ChunkFileCollector"))
				{
					GetChunkAssetForEbx(item, out ChunkAssetEntry chunkAssetEntry, out EbxAsset ebxAsset);
					if (chunkAssetEntry == null || (chunkAssetEntry != null && !chunkAssetEntry.HasModifiedData))
						continue;
					AssetManager.Instance.RevertAsset(chunkAssetEntry);
				}
			}

			ChunkAssetEntry chunkAssetEntry1 = entry as ChunkAssetEntry;
			if(chunkAssetEntry1 != null)
            {
				AssetManager.Instance.RevertAsset(chunkAssetEntry1);
			}

		}

		public static void CleanUpChunks(bool fullReset = false)
        {
			var movedEntries = LegacyEntries.Where(x => x.Value.ModifiedEntry != null
					&& x.Value.ModifiedEntry.Data == null
					&& x.Value.ModifiedEntry.NewOffset.HasValue
					);

			// Revert all the moved files
			foreach (var lfe in movedEntries)
            {
				lfe.Value.ModifiedEntry = null;
            }

			// Revert all the chunks
			foreach (EbxAssetEntry item in AssetManager.Instance.EnumerateEbx("ChunkFileCollector"))
			{
				GetChunkAssetForEbx(item, out ChunkAssetEntry chunkAssetEntry, out EbxAsset ebxAsset);
				if (chunkAssetEntry != null)
                {
					AssetManager.Instance.RevertAsset(chunkAssetEntry);
                }
				AssetManager.Instance.RevertAsset(item);
			}

			foreach(ChunkAssetEntry assetEntry in ModifiedChunks)
            {
				assetEntry.ModifiedEntry = null;
            }
			ModifiedChunks.Clear();

			// Revert all legacy files
			if(fullReset)
            {
				foreach (var lfe in LegacyEntries)
				{
					lfe.Value.ModifiedEntry = null;
				}
			}
		}

        public void AddAsset(string key, LegacyFileEntry lfe)
        {
			if (!LegacyEntries.ContainsKey(Fnv1.HashString(key)))
			{
				lfe.IsAdded = true;
				lfe.ModifiedEntry = new ModifiedAssetEntry() { Data = ((MemoryStream)GetChunkStream(lfe)).ToArray() };
				LegacyEntries.Add(Fnv1.HashString(key), lfe);
			}
        }


		public static (byte[] newChunk, long uncompressedSize) RecompressChunk(Stream originalDecompressedChunkStream, List<LegacyFileEntry> filesInChunk, Dictionary<int, byte[]> modifiedEntries)
		{
			MemoryStream modifiedDecompressedChunkStream = new MemoryStream((int)originalDecompressedChunkStream.Length);
			new MemoryStream();
			int offsetAdjustment = 0;
			foreach (var fileInChunk in filesInChunk)
			{
				if (modifiedEntries.TryGetValue(fileInChunk.NameHash, out var modifiedData))
				{
					long oldSize2 = ((fileInChunk.ModifiedEntry == null) ? fileInChunk.Size : fileInChunk.ModifiedEntry.Size);
					fileInChunk.ModifiedEntry = new ModifiedAssetEntry
					{
						//ChunkId = fileInChunk.ChunkId,
						NewOffset = Convert.ToUInt32( ((fileInChunk.ModifiedEntry == null) ? fileInChunk.ExtraData.DataOffset : fileInChunk.ModifiedEntry.NewOffset.Value) + offsetAdjustment),
						Size = modifiedData.Length
					};
					modifiedDecompressedChunkStream.Write(modifiedData);
					offsetAdjustment += (int)(fileInChunk.ModifiedEntry.Size - oldSize2);
				}
				else
				{
					long oldOffset = ((fileInChunk.ModifiedEntry == null) ? fileInChunk.ExtraData.DataOffset : fileInChunk.ModifiedEntry.NewOffset.Value);
					long oldSize = ((fileInChunk.ModifiedEntry != null && fileInChunk.ModifiedEntry.Size != 0) ? fileInChunk.ModifiedEntry.Size : fileInChunk.Size);
					fileInChunk.ModifiedEntry = new ModifiedAssetEntry
					{
						NewOffset = Convert.ToUInt32(oldOffset + offsetAdjustment),
						Size = Convert.ToInt32(oldSize)
					};
					originalDecompressedChunkStream.Position = oldOffset;
					//originalDecompressedChunkStream.CopyCountTo(modifiedDecompressedChunkStream, (int)oldSize);
					originalDecompressedChunkStream.CopyTo(modifiedDecompressedChunkStream, (int)oldSize);
				}
			}
			MemoryStream compressedStream = new MemoryStream();


			var modDCS = new NativeReader(modifiedDecompressedChunkStream);
			modDCS.Position = 0L;
			int progress = 0;

			while (modDCS.Position < modDCS.Length)
			{
				int iP = Convert.ToInt32(Math.Round((modDCS.Position / (double)modDCS.Length) * 100));
				if (iP != progress)
                {
					progress = iP;
					Debug.WriteLine("Progress of Recompress: " + progress.ToString());
				}

				long remainingInStream = modDCS.Length - modDCS.Position;
				int amountToRead = (int)Math.Min(Utils.MaxBufferSize, remainingInStream);
				List<LegacyFileEntry> list = (from f in filesInChunk
																	 where f.ExtraData.DataOffset >= modDCS.Position && f.ExtraData.DataOffset <= modDCS.Position + amountToRead
																	 select f).ToList();
				List<LegacyFileEntry> filesEndingInBlock = (from f in filesInChunk
																	where f.ExtraData.DataOffset + f.Size >= modDCS.Position && f.ExtraData.DataOffset + f.Size <= modDCS.Position + amountToRead
																	select f).ToList();
				foreach (LegacyFileEntry item in list)
				{
					item.CompressedOffset = compressedStream.Position;
				}
				byte[] compressedBlock = Utils.CompressFile(modDCS.ReadBytes(amountToRead), null, ResourceType.Invalid, CompressionType.ZStd);
				compressedStream.Write(compressedBlock);
				foreach (LegacyFileEntry item2 in filesEndingInBlock)
				{
					item2.CompressedSize = compressedStream.Position;
				}
			}
			return (compressedStream.ToArray(), modDCS.Length);
		}


	}
}