using Frostbite.FileManagers;
using FrostbiteSdk.Frostbite.FileManagers;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA22Plugin
{
    public class LFM_Fifa22 : LegacyFileManager_FMTV2, ILegacyFileManager, ICustomAssetManager
    {

		public override void Initialize(ILogger logger)
		{
			logger.Log("Loading legacy files");
			AddedFileEntries = new List<LegacyFileEntry>();

			//ChunkBatches = new List<ChunkBatch>();
			//LegacyEntries = new Dictionary<string, LegacyFileEntry>();

			//var chAttempt1 = AssetManager.Instance.GetChunkEntry(Guid.Parse("FA40D5B2-358C-B8BF-DACA-B7EA34EF1F45"));
			//var chAttempt2 = AssetManager.Instance.GetChunkEntry(Guid.Parse("FA40D5B2-358C-B8BF-DACA-B7EA34EF1F45"));

			foreach (EbxAssetEntry item in AssetManager.EnumerateEbx("ChunkFileCollector"))
			{
				GetChunkAssetForEbx(item, out ChunkAssetEntry chunkAssetEntry, out EbxAsset ebxAsset);
				if (chunkAssetEntry == null)
				{
					//chunkAssetEntry = AssetManager.Instance.GetChunkEntry(Guid.Parse("FA40D5B2-358C-B8BF-DACA-B7EA34EF1F45"));
					continue;
				}
				chunkAssetEntry.IsLegacy = true;

				MemoryStream chunk = new MemoryStream();
				AssetManager.Instance.GetChunk(chunkAssetEntry).CopyTo(chunk);
				if (chunk != null)
				{
					using (NativeReader nativeReader = new NativeReader(chunk))
					{
						File.WriteAllBytes("_debug_legacy_" + item.Name.Replace(@"/", "_") + ".dat", chunk.ToArray());
						nativeReader.Position = 0;

						ChunkBatch chunkBatch = new ChunkBatch()
						{
							EbxAssetEntry = item,
							EbxAsset = ebxAsset,
							ChunkAssetEntry = chunkAssetEntry,
							UnkCount1 = nativeReader.ReadUInt(), // 0
							Offset1_Unk = nativeReader.ReadLong(), // 4
							NumberOfFiles = nativeReader.ReadInt(), // 12
							Offset2_Files = nativeReader.ReadLong(), // 16
							BootableItemCount = nativeReader.ReadUInt(), // 24 
							BootableItemOffset = nativeReader.ReadLong(), // 28
							LinkedChunkCount = nativeReader.ReadUInt(), // 36
							LinkedChunkOffset = nativeReader.ReadLong() // 40
						};
						//nativeReader.Position = 0;
						//chunkBatch.Initial64Data = nativeReader.ReadBytes((int)chunkBatch.FileFirstPosition);
						nativeReader.Position = chunkBatch.Offset1_Unk;
						for (uint index = 0u; index < chunkBatch.UnkCount1; index++)
						{
							var unkLong = nativeReader.ReadLong();
							var name = string.Empty;
							chunkBatch.UnknownItems.Add(new ChunkBatch.UnknownItem() { Offset = unkLong, Name = name });
						}

						chunkBatch.EndOfStrings = 0;
						nativeReader.Position = chunkBatch.Offset2_Files;
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
							if (nativeReader.Position > chunkBatch.EndOfStrings)
								chunkBatch.EndOfStrings = (int)nativeReader.Position;

							if (!LegacyEntries.ContainsKey(name))
							{
								legacyFileEntry.Name = name;
								LegacyEntries.Add(name, legacyFileEntry);
							}
							else
							{
								legacyFileEntry = LegacyEntries[name];
							}

							nativeReader.Position = chunkBatch.Offset2_Files + ((8 + 8 + 8 + 8 + 8 + 16) * (index + 1));

						}

						nativeReader.Position = chunkBatch.BootableItemOffset;
						for (uint j = 0u; j < chunkBatch.BootableItemCount; j++)
						{
							long nameOffset3 = nativeReader.ReadLong();
							var unknumber1 = nativeReader.ReadUInt();
							var unknumber2 = nativeReader.ReadUInt();
							nativeReader.Position = nameOffset3;
							var unkName2 = nativeReader.ReadNullTerminatedString();
							if (nativeReader.Position > chunkBatch.EndOfStrings)
								chunkBatch.EndOfStrings = (int)nativeReader.Position;

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
							var chunkOriginalSize = nativeReader.ReadLong();
							var chunkGuid = nativeReader.ReadGuid();
							var otherChunk = AssetManager.Instance.GetChunkEntry(chunkGuid);
							otherChunk.OriginalSize = chunkOriginalSize;
							chunkBatch.LinkedChunks.Add(otherChunk);
						}

						nativeReader.Position = chunkBatch.EndOfStrings;
						nativeReader.Pad(16);
						double numberOfEndUnkItems = ((double)nativeReader.Length - (double)nativeReader.Position) / 4;
						for (int index = 0; index < numberOfEndUnkItems; index++)
						{
							chunkBatch.BottomUnknownOffsets.Add(nativeReader.ReadInt());
						}

						ChunkBatches.Add(chunkBatch);


					}
				}
			}

			logger.Log($"Loaded {LegacyEntries.Count} legacy files");
			LegacyFileManager.Instance = this;
		}


		/*
		public override List<LegacyFileEntry> RebuildEntireChunk(Guid chunkId, List<LegacyFileEntry> replaceFileEntries, List<LegacyFileEntry> newFileEntries = null)
		{
			replaceFileEntries.ForEach(x => { if (x.ModifiedEntry != null && x.ModifiedEntry.ChunkId.HasValue) x.ModifiedEntry.ChunkId = null; });
			CompressionType compressionType = ProfilesLibrary.GetCompressionType(ProfilesLibrary.CompTypeArea.Legacy);

			// get the chunk batch (the main batch with offsets etc)
			ChunkBatch chunkBatch = ChunkBatches.FirstOrDefault(x => x.ChunkAssetEntry.Id == chunkId);
			if (chunkBatch != null)
			{

				var edited = replaceFileEntries.GroupBy(x => x.ChunkId).ToDictionary(x => x.Key, x => x.ToList());

				var edited2 = chunkBatch.BatchLegacyFiles.Where(x => x.ModifiedEntry != null).GroupBy(x => x.ChunkId).ToDictionary(x => x.Key, x => x.ToList());
				foreach (var gItem in edited)
				{
					// Easily handle Singular Chunk
					if (gItem.Value.Count == 1 && chunkBatch.ChunkGroupsInBatch[gItem.Value.First().ChunkId].Count == 1)
					{
						var chunkEntry = AssetManager.Instance.GetChunkEntry(gItem.Key);
						var legacyItem = gItem.Value.First();
						legacyItem.ModifiedEntry.NewOffset = 0;
						legacyItem.ModifiedEntry.Size = legacyItem.ModifiedEntry.Data.Length;


						ModifiedChunks.Add(chunkEntry);

						AssetManager.Instance.ModifyChunk(gItem.Key, legacyItem.ModifiedEntry.Data, null, compressionType);
						chunkEntry.ModifiedEntry.AddToChunkBundle = true;
						chunkEntry.ModifiedEntry.AddToTOCChunks = true;
					}
					// Otherwise handle Chunk Batch
					else
					{
						var batchGuid = gItem.Key;
						var groupOfLegacyFilesWithOnlyOne = chunkBatch.ChunkGroupsInBatch
								.Where(x => !chunkBatch.ChunkGroupsInBatchModified.ContainsKey(x.Key))
								.Where(x => x.Value.Count == 1)
								.First();

						foreach (var gItem2 in gItem.Value)
						{
							gItem2.ModifiedEntry.ChunkId = groupOfLegacyFilesWithOnlyOne.Key;
						}



						// other way of doing it (add to another file)
						var groupOfLegacyFiles = chunkBatch.ChunkGroupsInBatch.First(x => x.Key == groupOfLegacyFilesWithOnlyOne.Key).Value;
							groupOfLegacyFiles.AddRange(gItem.Value);
							batchGuid = groupOfLegacyFilesWithOnlyOne.Key;

							// standard way of doing it
							//var groupOfLegacyFiles = chunkBatch.ChunkGroupsInBatch.First(x => x.Key == batchGuid).Value;
							var groupChunkEntry = AssetManager.Instance.GetChunkEntry(batchGuid);
							var groupChunk = AssetManager.Instance.GetChunk(groupChunkEntry);

							var ms_newChunkGroup = new MemoryStream();
							using (var nw_newChunkGroup = new NativeWriter(ms_newChunkGroup, leaveOpen: true))
							{
								using (var nr_GroupChunk = new NativeReader(groupChunk))
								{
									long lastOffset = 0;
									foreach (var itemInChunkGroup in groupOfLegacyFiles)
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
										itemInChunkGroup.ModifiedEntry.NewOffset = lastOffset;
										itemInChunkGroup.ModifiedEntry.Size = d.Length;
										lastOffset += d.Length;

										itemInChunkGroup.ModifiedEntry.CompressedOffset = 0;
										nw_newChunkGroup.Write(d);
										itemInChunkGroup.ModifiedEntry.CompressedOffsetEnd = d.Length;


									}

								}
							}
							// Modify the Chunk
							ms_newChunkGroup.Position = 0;
							byte[] newChunkGroupData = new NativeReader(ms_newChunkGroup).ReadToEnd();


							var oldEntry = AssetManager.Instance.GetChunkEntry(batchGuid);
							ModifiedChunks.Add(oldEntry);

							//AssetManager.Instance.ModifyChunk(batchGuid, newChunkGroupData, compressionOverride: compressionType);
							//oldEntry.ModifiedEntry.AddToChunkBundle = true;
							//oldEntry.ModifiedEntry.AddToTOCChunks = true;

							var newChunkAlreadyCompressed = CompressChunkGroup(ms_newChunkGroup, groupOfLegacyFiles, compressionType);
							groupChunkEntry.ModifiedEntry = new ModifiedAssetEntry()
							{
								Data = newChunkAlreadyCompressed.newChunk,
								Size = newChunkAlreadyCompressed.newChunk.Length,
								LogicalSize = (uint)newChunkAlreadyCompressed.newChunk.Length,
								OriginalSize = newChunkAlreadyCompressed.newChunk.Length,
								Sha1 = AssetManager.Instance.GenerateSha1(newChunkAlreadyCompressed.newChunk),
								AddToChunkBundle = true,
								AddToTOCChunks = true,
							};
						//}


						//                  var unmodifiedfiles = groupOfLegacyFiles.Where(x => x.ModifiedEntry == null);
						//var zeroCompLegacyFiles = groupOfLegacyFiles.Where(x => x.ModifiedEntry != null && x.ModifiedEntry.CompressedOffset == 0);

						//groupChunkEntry.IsDirty = true;
						//groupChunkEntry.ModifiedEntry.AddToChunkBundle = true;

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
						if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.NewOffset.HasValue)
							nwNewBatch.Write((long)lfe.ModifiedEntry.NewOffset.Value);
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
						if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.CompressedOffsetEnd != null)
							nwNewBatch.Write((long)lfe.ModifiedEntry.CompressedOffsetEnd);
						else
							nwNewBatch.Write((long)lfe.CompressedOffsetEnd);

						nwNewBatch.Position = lfe.ChunkIdPosition;
						if (lfe.ModifiedEntry != null && lfe.ModifiedEntry.ChunkId.HasValue)
							nwNewBatch.Write(lfe.ModifiedEntry.ChunkId.Value);
						else
							nwNewBatch.Write(lfe.ChunkId);

					}

				}
				msNewBatch.Position = 0;

				var newBatchData = new NativeReader(msNewBatch).ReadToEnd();

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

				var allFiles = new List<LegacyFileEntry>();
				if (replaceFileEntries != null)
					allFiles.AddRange(replaceFileEntries);
				if (newFileEntries != null)
					allFiles.AddRange(newFileEntries);
				return allFiles;
			}

			return null;
		}

		*/
	}
}
