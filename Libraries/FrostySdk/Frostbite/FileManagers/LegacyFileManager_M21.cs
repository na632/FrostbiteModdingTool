using FrostbiteSdk.Frostbite.FileManagers;
using Frosty.Hash;
using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Buffers.Binary;
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

			public IEnumerable<LegacyFileEntry> BatchLegacyFiles
			{
				get
				{
					return LegacyEntries.Values.Where(x => x.ParentGuid == ChunkAssetEntry.Id);
				}
			}

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
				public long NameOffset;
				public string Name;
				public uint Index;
				public uint Active;
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
							LegacyFileEntry legacyFileEntry = new LegacyFileEntry();
							legacyFileEntry.FileNameInBatchOffset = nativeReader.ReadLong(); // 0 - 8
							
							
							//legacyFileEntry.BatchOffset = positionOfItem;
							legacyFileEntry.ParentGuid = chunkAssetEntry.Id;


							legacyFileEntry.CompressedOffsetPosition = nativeReader.Position;
							legacyFileEntry.CompressedOffset = nativeReader.ReadLong();
							legacyFileEntry.CompressedOffsetStart = legacyFileEntry.CompressedOffset;
							legacyFileEntry.CompressedSizePosition = nativeReader.Position;
							legacyFileEntry.CompressedOffsetEnd = nativeReader.ReadLong();
							legacyFileEntry.CompressedSize = legacyFileEntry.CompressedOffsetEnd - legacyFileEntry.CompressedOffset;

							legacyFileEntry.ActualOffsetPosition = (int)nativeReader.Position;
							legacyFileEntry.ExtraData = new AssetExtraData() { DataOffset = (uint)nativeReader.ReadLong() };
							legacyFileEntry.ActualSizePosition = (int)nativeReader.Position;
							legacyFileEntry.Size = nativeReader.ReadLong();

							legacyFileEntry.ChunkIdPosition = nativeReader.Position;
							var chunkId = nativeReader.ReadGuid();
							legacyFileEntry.ChunkId = chunkId;

							nativeReader.Position = legacyFileEntry.FileNameInBatchOffset;
							string name = nativeReader.ReadNullTerminatedString();
							int key = Fnv1.HashString(name);
							if (!LegacyEntries.ContainsKey(key))
							{
								legacyFileEntry.Name = name;
								LegacyEntries.Add(key, legacyFileEntry);
							}
							else
							{
								legacyFileEntry = LegacyEntries[key];
							}

							nativeReader.Position = chunkBatch.FileFirstPosition + ((8 + 8 + 8 + 8 + 8 + 16) * (index + 1));



						}

						if (ProfilesLibrary.IsFIFA21DataVersion())
						{
							nativeReader.Position = chunkBatch.BootableItemOffset;
							for (uint j = 0u; j < chunkBatch.BootableItemCount; j++)
							{
								long nameOffset3 = nativeReader.ReadLong();
								var unknumber1 = nativeReader.ReadUInt();
								var unknumber2 = nativeReader.ReadUInt();
								nativeReader.Position = nameOffset3;
								var unkName2 = nativeReader.ReadNullTerminatedString();
								nativeReader.Position = chunkBatch.BootableItemOffset + (16 * (j + 1));

								ChunkBatch.BootableItem bootableItem = new ChunkBatch.BootableItem()
								{
									NameOffset = nameOffset3,
									Name = unkName2,
									Index = unknumber1,
									Active = unknumber2
								};
								chunkBatch.BootableItems.Add(bootableItem);
							}

							nativeReader.Position = chunkBatch.LinkedChunkOffset;
							for (uint i = 0u; i < chunkBatch.LinkedChunkCount; i++)
							{
								var chunkSize = nativeReader.ReadLong();
								var chunkGuid = nativeReader.ReadGuid();
								var otherChunk = AssetManager.Instance.GetChunkEntry(chunkGuid);
								chunkBatch.LinkedChunks.Add(otherChunk);
							}
						}

						ChunkBatches.Add(chunkBatch);


					}
				}
			}

			logger.Log($"Loaded {LegacyEntries.Count} legacy files");
			LegacyFileManager.Instance = this;
		}

		public void WriteAllLegacy()
		{
			// A chunk batch corresponds to each Ebx in the CFC
			foreach (ChunkBatch chunkBatch in ChunkBatches)
			{
				MemoryStream ms_newChunkBatch = new MemoryStream();
				using(NativeWriter nw = new NativeWriter(ms_newChunkBatch, true))
                {
					Debug.WriteLine("Processing Legacy Item: " + chunkBatch.EbxAssetEntry.Name);
					nw.Write(chunkBatch.UnkCount1);
					nw.Write(chunkBatch.UnkOffset1);
					//nw.Write(chunkBatch.NumberOfFiles);
					var oldNumberOfFiles = chunkBatch.NumberOfFiles;
					var newNumberOfFiles = (uint)chunkBatch.BatchLegacyFiles.Count();
					nw.Write(newNumberOfFiles);
					nw.Write(chunkBatch.FileFirstPosition);
					nw.Write(chunkBatch.BootableItemCount);
					nw.Write(chunkBatch.BootableItemOffset);
					nw.Write(chunkBatch.LinkedChunkCount);
					nw.Write(chunkBatch.LinkedChunkOffset);

					var positionBeforeFiles = nw.Position;
					// Quick write before Strings
					foreach(var item in chunkBatch.BatchLegacyFiles)
                    {
						nw.Write(item.FileNameInBatchOffset);
						nw.Write(item.CompressedOffset);
						nw.Write(item.CompressedOffsetEnd);
						nw.Write(item.ExtraData.DataOffset);
						nw.Write(item.Size);
						nw.Write(item.ChunkId);
					}

					// Strings
					foreach (var item in chunkBatch.BatchLegacyFiles)
					{
						item.FileNameInBatchOffset = nw.Position;
						nw.WriteNullTerminatedString(item.Name);
					}

					var positionBeforeRewrite = nw.Position;
					nw.Position = positionBeforeFiles;
					// Rewrite String and Changed Stuff
					foreach (var item in chunkBatch.BatchLegacyFiles)
					{
						nw.Write(item.FileNameInBatchOffset);
						nw.Write(item.CompressedOffset);
						nw.Write(item.CompressedOffsetEnd);
						nw.Write(item.ExtraData.DataOffset);
						nw.Write(item.Size);
						nw.Write(item.ChunkId);
					}

					nw.Position = positionBeforeRewrite;
                    if (chunkBatch.BootableItems.Any())
                    {

                    }

					if (chunkBatch.LinkedChunks.Any())
					{

					}
				}
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

		public List<LegacyFileEntry> RebuildEntireChunk(Guid chunkId, List<LegacyFileEntry> replaceFileEntries, List<LegacyFileEntry> newFileEntries = null)
		{
			WriteAllLegacy();
            //CompressionType compressionType = ProfilesLibrary.IsMadden21DataVersion() ? CompressionType.LZ4 : CompressionType.Oodle;
            //CompressionType compressionType = ProfilesLibrary.IsMadden21DataVersion() ? CompressionType.LZ4 : CompressionType.ZStd;
            CompressionType compressionType = ProfilesLibrary.IsMadden21DataVersion() ? CompressionType.LZ4 : CompressionType.None;

            ChunkBatch chunkBatch;
			// get the chunk batch (the main batch with offsets etc)
			chunkBatch = ChunkBatches.FirstOrDefault(x => x.ChunkAssetEntry.Id == chunkId);
			if (chunkBatch != null)
			{
				var edited = replaceFileEntries.GroupBy(x => x.ChunkId).ToDictionary(x => x.Key, x => x.ToList());

				var edited2 = chunkBatch.BatchLegacyFiles.Where(x => x.ModifiedEntry != null).GroupBy(x => x.ChunkId).ToDictionary(x => x.Key, x => x.ToList());
				foreach (var gItem in edited)
                {
                    //BuildNewChunkForLegacyItem(gItem);


                    // Easily handle Singular Chunk
                    if (gItem.Value.Count == 1 && chunkBatch.ChunkGroupsInBatch[gItem.Value.First().ChunkId].Count == 1)
                    {
                        var legacyItem = gItem.Value.First();
                        legacyItem.ModifiedEntry.NewOffset = 0;
                        legacyItem.ModifiedEntry.Size = legacyItem.ModifiedEntry.Data.Length;

                        ModifiedChunks.Add(AssetManager.Instance.GetChunkEntry(gItem.Key));


                        AssetManager.Instance.ModifyChunk(gItem.Key, legacyItem.ModifiedEntry.Data, null, compressionType);
                    }
                    // Otherwise handle Chunk Batch
                    else
                    {
                        var groupOfLegacyFiles = chunkBatch.ChunkGroupsInBatch.First(x => x.Key == gItem.Key);
						var groupChunkEntry = AssetManager.Instance.GetChunkEntry(gItem.Key);
						var groupChunk = AssetManager.Instance.GetChunk(groupChunkEntry);


						var ms_newChunkGroup = new MemoryStream();
						using (var nw_newChunkGroup = new NativeWriter(ms_newChunkGroup, leaveOpen: true))
						{
							using (var nr_GroupChunk = new NativeReader(groupChunk))
							{
								foreach (var itemInChunkGroup in groupOfLegacyFiles.Value)
								{
									byte[] d = null;
									
									if (itemInChunkGroup.HasModifiedData)
									{
										itemInChunkGroup.ModifiedEntry.Size = itemInChunkGroup.ModifiedEntry.Data.Length;
										d = itemInChunkGroup.ModifiedEntry.Data;
									}
									else
									{
										nr_GroupChunk.Position = itemInChunkGroup.ExtraData.DataOffset;
										d = nr_GroupChunk.ReadBytes((int)itemInChunkGroup.Size);
										itemInChunkGroup.ModifiedEntry = new ModifiedAssetEntry();
									}
									itemInChunkGroup.ModifiedEntry.NewOffset = (uint)nw_newChunkGroup.Position;
									itemInChunkGroup.ModifiedEntry.Size = d.Length;
									nw_newChunkGroup.Write(d);

								}

							}
						}
						// Modify the Chunk
						ms_newChunkGroup.Position = 0;
						byte[] newChunkGroupData = new NativeReader(ms_newChunkGroup).ReadToEnd();


						//byte[] newChunkGroupData = ms_newChunkGroup.ToArray();

						var oldEntry = AssetManager.Instance.GetChunkEntry(gItem.Key);
						ModifiedChunks.Add(oldEntry);

						//AssetManager.Instance.ModifyChunk(gItem.Key, newChunkGroupData, compressionOverride: CompressionType.Oodle);

						var newChunkAlreadyCompressed = CompressChunkGroup(ms_newChunkGroup, groupOfLegacyFiles.Value, compressionType);
						groupChunkEntry.ModifiedEntry = new ModifiedAssetEntry()
						{
							Data = newChunkAlreadyCompressed.newChunk,
							Size = newChunkAlreadyCompressed.newChunk.Length,
							LogicalSize = (uint)newChunkAlreadyCompressed.newChunk.Length,
							OriginalSize = newChunkAlreadyCompressed.newChunk.Length,
                            Sha1 = AssetManager.Instance.GenerateSha1(newChunkAlreadyCompressed.newChunk),
                            //Sha1 = oldEntry.Sha1
                        };
						
						var unmodifiedfiles = groupOfLegacyFiles.Value.Where(x => x.ModifiedEntry == null);
						var zeroCompLegacyFiles = groupOfLegacyFiles.Value.Where(x => x.ModifiedEntry != null && x.ModifiedEntry.CompressedOffset == 0);

						groupChunkEntry.IsDirty = true;
						groupChunkEntry.ModifiedEntry.AddToChunkBundle = true;

						//AssetManager.Instance.ModifyChunk(gItem.Key, newChunkGroupData, compressionOverride: compressionType);


					}

                }




                byte[] oldBatchData;

				using (var nrOldBatch = new NativeReader(AssetManager.Instance.GetChunk(chunkBatch.ChunkAssetEntry)))
				{
					oldBatchData = nrOldBatch.ReadToEnd();
				}

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
						if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.CompressedOffset != null)
							nwNewBatch.Write((long)lfe.ModifiedEntry.CompressedOffset);
						else
							nwNewBatch.Write((long)lfe.CompressedOffset);

						// Compressed Size
						nwNewBatch.Position = lfe.CompressedSizePosition;
						//if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.CompressedSize != null)
						//	nwNewBatch.Write((long)lfe.ModifiedEntry.CompressedSize);
						if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.CompressedOffsetEnd != null)
							nwNewBatch.Write((long)lfe.ModifiedEntry.CompressedOffsetEnd);
						else
                            //nwNewBatch.Write((long)lfe.CompressedSize);
                            nwNewBatch.Write((long)lfe.CompressedOffsetEnd);

                        nwNewBatch.Position = lfe.ChunkIdPosition;
                        nwNewBatch.Write(lfe.ChunkId);

                    }
					//nwNewBatch.Position = chunkBatch.EndOfStrings;
					//nwNewBatch.WriteNullTerminatedString("boot");
					//nwNewBatch.WriteNullTerminatedString("permanent");

					//nwNewBatch.Position = 24;
					//nwNewBatch.Write((uint)0);
					//nwNewBatch.Write((ulong)0);
					//nwNewBatch.Write((uint)0);
					//nwNewBatch.Write((ulong)0);
				}
				msNewBatch.Position = 0;

				var newBatchData = new NativeReader(msNewBatch).ReadToEnd();

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

				AssetManager.Instance.ModifyChunk(chunkBatch.ChunkAssetEntry.Id, newBatchData, compressionOverride: compressionType);
				var cE = AssetManager.Instance.GetChunkEntry(chunkBatch.ChunkAssetEntry.Id);
				cE.ModifiedEntry.OriginalSize = msNewBatch.Length;
				cE.ModifiedEntry.LogicalSize = Convert.ToUInt32(Utils.CompressFile(newBatchData, null, ResourceType.Invalid, compressionType).Length);
				cE.ModifiedEntry.Size = cE.ModifiedEntry.LogicalSize;
				cE.ModifiedEntry.AddToChunkBundle = true;
				cE.ModifiedEntry.AddToTOCChunks = true;

				msNewBatch.Close();
				msNewBatch.Dispose();
				GC.Collect();
				GC.WaitForPendingFinalizers();

				var allFiles = new List<LegacyFileEntry>();
				if(replaceFileEntries != null)
					allFiles.AddRange(replaceFileEntries);
				if(newFileEntries != null)
					allFiles.AddRange(newFileEntries);
				return allFiles;
			}

			return null;
		}

        private static void BuildNewChunkForLegacyItem(KeyValuePair<Guid, List<LegacyFileEntry>> gItem)
        {
            foreach (var i in gItem.Value)
            {
                var newGuid = GenerateDeterministicGuid(i);
                i.ChunkId = newGuid;
                i.ModifiedEntry.NewOffset = 0;
                i.ModifiedEntry.CompressedOffset = 0;
                i.ModifiedEntry.CompressedOffsetEnd = 0;
                i.ModifiedEntry.AddToChunkBundle = true;
                i.ModifiedEntry.AddToTOCChunks = true;
            }
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

				if (!OriginalLegacyEntries.Contains(legacyFileEntry))
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

		public List<LegacyFileEntry> ModifyAssets(Dictionary<string, byte[]> data, bool rebuildChunk = true)
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
						RangeStart = 0,
						RangeEnd = (uint)dpi.Value.Length,
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
					return RebuildEntireChunk(grpFile.Key, grpFile.Value);
				}
			}
			return filesEdited;
		}

		///// <summary>
		///// Modifies multiple assets and expects the entries to already has a ModifiedEntry object instance
		///// </summary>
		///// <param name="entries"></param>
		//public IEnumerable<ChunkAssetEntry> ModifyAssets(List<LegacyFileEntry> entries)
		//{
		//	var groupedAssets = entries.GroupBy(x => x.ParentGuid).ToDictionary(x => x.Key, x => x.ToList());
		//	foreach (var group in groupedAssets)
		//	{
		//		foreach (var lfe in group.Value)
		//		{
		//			ModifyAsset(lfe.Name, lfe.ModifiedEntry.Data);
		//		}
		//		yield return RebuildEntireChunk(group.Key, group.Value);
		//	}
		//}

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
					};
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
			//if (cacheMode)
			//{
			//	if (!cachedChunks.ContainsKey(lfe.ChunkId))
			//	{
			//		using (Stream stream = AssetManager.GetChunk(AssetManager.GetChunkEntry(lfe.ChunkId)))
			//		{
			//			if (stream == null)
			//			{
			//				return null;
			//			}
			//			cachedChunks.Add(lfe.ChunkId, ((MemoryStream)stream).ToArray());
			//		}
			//	}
			//	return new MemoryStream(cachedChunks[lfe.ChunkId]);
			//}
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
			if (chunkAssetEntry1 != null)
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

			foreach (ChunkAssetEntry assetEntry in ModifiedChunks)
			{
				assetEntry.ModifiedEntry = null;
			}
			ModifiedChunks.Clear();

			// Revert all legacy files
			if (fullReset)
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

		public static (byte[] newChunk, long uncompressedSize) CompressChunkGroup(
			Stream uncompressedChunkStream
			, List<LegacyFileEntry> filesInChunk
			, CompressionType compressionType)
		{
			MemoryStream compressedStream = new MemoryStream();
			var modDCS = new NativeReader(uncompressedChunkStream);
			modDCS.Position = 0L;
			int progress = 0;

			filesInChunk = filesInChunk.OrderBy(x => x.ModifiedEntry.NewOffset).ToList();

			long lastOffsetEnd = 0;
			while (modDCS.Position < modDCS.Length)
			{
				bool isFirstSet = modDCS.Position == 0;

				int iP = Convert.ToInt32(Math.Round((modDCS.Position / (double)modDCS.Length) * 100));
				if (iP != progress)
                {
					progress = iP;
					Debug.WriteLine("Progress of Recompress: " + progress.ToString());
				}

				long remainingInStream = modDCS.Length - modDCS.Position;
				int amountToRead = (int)Math.Min(Utils.MaxBufferSize, remainingInStream);
				List<LegacyFileEntry> list = (from f in filesInChunk
												where f.ModifiedEntry.NewOffset.Value 
												>= modDCS.Position 
												&& f.ModifiedEntry.NewOffset.Value
												<= modDCS.Position + amountToRead
											  select f).ToList();
				List<LegacyFileEntry> filesEndingInBlock = (from f in filesInChunk
															where
                                                            f.ModifiedEntry.NewOffset.Value + f.ModifiedEntry.Size
                                                            >= modDCS.Position
															&& f.ModifiedEntry.NewOffset.Value + f.ModifiedEntry.Size
															<= modDCS.Position + amountToRead
															select f).ToList();
				foreach (LegacyFileEntry item in list)
				{
					item.ModifiedEntry.CompressedOffset = compressedStream.Position;
					item.ModifiedEntry.CompressedOffsetEnd = compressedStream.Position;
				}
				byte[] compressedBlock = Utils.CompressFile(modDCS.ReadBytes(amountToRead), null, ResourceType.Invalid, compressionType);
				compressedStream.Write(compressedBlock);
				lastOffsetEnd = compressedStream.Position;
				foreach (LegacyFileEntry item in list)
				{
					item.ModifiedEntry.CompressedOffsetEnd = lastOffsetEnd;
				}
				foreach (LegacyFileEntry item in filesEndingInBlock)
                {
					item.ModifiedEntry.CompressedOffsetEnd = lastOffsetEnd;
                }
            }
			return (compressedStream.ToArray(), modDCS.Length);
		}


		public static Guid GenerateDeterministicGuid(LegacyFileEntry lfe)
		{
			ulong filenameHash = Murmur2.HashString64(lfe.Filename, 18532uL);
			ulong pathHash = Murmur2.HashString64(lfe.Path, 18532uL);
			int counter = 1;
			Guid guid = Guid.Empty;
			Span<byte> guidSpan = stackalloc byte[16];
			do
			{
				BinaryPrimitives.WriteUInt64LittleEndian(guidSpan, pathHash);
				Span<byte> span = guidSpan;
				BinaryPrimitives.WriteUInt64LittleEndian(span[8..], filenameHash ^ (ulong)counter);
				guidSpan[15] = 1;
				guid = new Guid(guidSpan);
				counter++;
			}
			while (AssetManager.Instance.GetChunkEntry(guid) != null);
			return guid;
		}


	}


}