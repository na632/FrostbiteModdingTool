using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frosty.Hash;
using FrostySdk.IO;
using FrostySdk.Managers;

namespace FrostySdk.Frostbite
{
    /*
    public class LegacyFileManager_F21 : ILegacyFileManager
    {
        public class ChunkFileCollectorParser
        {
            private readonly AssetManager assetManager;

            public ChunkFileCollectorParser(AssetManager assetManager)
            {
                this.assetManager = assetManager;
            }

            public void Parse(Stream stream, EbxAssetEntry ebxAssetEntry, Dictionary<int, LegacyFileEntry> outputDictionary)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("stream");
                }
                if (outputDictionary == null)
                {
                    throw new ArgumentNullException("outputDictionary");
                }
                NativeReader nativeReader = new NativeReader(stream);
                uint count1 = nativeReader.ReadUInt32LittleEndian();
                long offset1 = nativeReader.ReadInt64LittleEndian();
                uint filesCount = nativeReader.ReadUInt32LittleEndian();
                long fileEntriesOffset = nativeReader.ReadInt64LittleEndian();
                uint count2 = nativeReader.ReadUInt32LittleEndian();
                long offset2 = nativeReader.ReadInt64LittleEndian();
                uint chunkCount = nativeReader.ReadUInt32LittleEndian();
                long chunkOffset = nativeReader.ReadInt64LittleEndian();
                nativeReader.Position = offset1;
                for (uint l = 0u; l < count1; l++)
                {
                    long nameOffset = nativeReader.ReadInt64LittleEndian();
                    long currentPosition = nativeReader.Position;
                    nativeReader.Position = nameOffset;
                    nativeReader.ReadNullTerminatedString();
                    nativeReader.Position = currentPosition;
                }
                nativeReader.Position = fileEntriesOffset;
                for (uint k = 0u; k < filesCount; k++)
                {
                    long nameOffset2 = nativeReader.ReadInt64LittleEndian();
                    long currentPosition2 = nativeReader.Position;
                    nativeReader.Position = nameOffset2;
                    string text = nativeReader.ReadNullTerminatedString();
                    nativeReader.Position = currentPosition2;
                    int key = Fnv1.HashString(text);
                    if (!outputDictionary.TryGetValue(key, out var legacyFileEntry))
                    {
                        legacyFileEntry = (outputDictionary[key] = new LegacyFileEntry(assetManager)
                        {
                            Name = text
                        });
                    }
                    LegacyFileEntry.ChunkCollectorInstance chunkCollectorInstance = new LegacyFileEntry.ChunkCollectorInstance
                    {
                        CompressedStartOffset = nativeReader.ReadInt64LittleEndian(),
                        CompressedEndOffset = nativeReader.ReadInt64LittleEndian(),
                        Offset = nativeReader.ReadInt64LittleEndian(),
                        Size = nativeReader.ReadInt64LittleEndian(),
                        ChunkId = nativeReader.ReadGuid(),
                        Entry = ebxAssetEntry
                    };
                    legacyFileEntry.CollectorInstances.Add(chunkCollectorInstance);
                }
                nativeReader.Position = offset2;
                for (uint j = 0u; j < count2; j++)
                {
                    long nameOffset3 = nativeReader.ReadInt64LittleEndian();
                    long currentPosition3 = nativeReader.Position;
                    nativeReader.Position = nameOffset3;
                    nativeReader.ReadNullTerminatedString();
                    nativeReader.Position = currentPosition3;
                    nativeReader.ReadUInt32LittleEndian();
                    nativeReader.ReadUInt32LittleEndian();
                }
                nativeReader.Position = chunkOffset;
                for (uint i = 0u; i < chunkCount; i++)
                {
                    nativeReader.ReadInt64LittleEndian();
                    nativeReader.ReadGuid();
                }
            }
        }

        private readonly AssetManager assetManager;

        private readonly Dictionary<int, LegacyFileEntry> legacyEntries = new Dictionary<int, LegacyFileEntry>();

        private readonly Dictionary<Guid, byte[]> cachedChunks = new Dictionary<Guid, byte[]>();

        private bool cacheMode;

        public LegacyFileManager_F21(AssetManager assetManager)
        {
            this.assetManager = assetManager ?? throw new ArgumentNullException("assetManager");
        }

        public void Initialize()
        {
            ChunkFileCollectorParser parser = new ChunkFileCollectorParser(assetManager);
            foreach (EbxAssetEntry item in assetManager.EnumerateEbx("ChunkFileCollector"))
            {
                EbxAsset ebx = assetManager.GetEbx(item);
                if (ebx == null)
                {
                    continue;
                }
                dynamic rootObject = ebx.RootObject;
                dynamic val = rootObject.Manifest;
                ChunkAssetEntry chunkAssetEntry = assetManager.GetChunkEntry(val.ChunkId);
                if (chunkAssetEntry != null)
                {
                    Stream chunk = assetManager.GetChunk(chunkAssetEntry);
                    if (chunk != null)
                    {
                        parser.Parse(chunk, item, legacyEntries);
                    }
                }
            }
        }

        public void SetCacheModeEnabled(bool enabled)
        {
            cacheMode = enabled;
        }

        public void FlushCache()
        {
            cachedChunks.Clear();
        }

        public IEnumerable<AssetEntry> EnumerateAssets(bool modifiedOnly)
        {
            foreach (LegacyFileEntry value in legacyEntries.Values)
            {
                if (!modifiedOnly || value.IsModified)
                {
                    yield return value;
                }
            }
        }

        public AssetEntry GetAssetEntry(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            int key2 = Fnv1.HashString(key);
            legacyEntries.TryGetValue(key2, out var result);
            return result;
        }

        public Stream GetAsset(AssetEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            LegacyFileEntry legacyFileEntry = entry as LegacyFileEntry;
            if (legacyFileEntry == null)
            {
                throw new ArgumentException("entry must be a LegacyFileEntry (got " + entry.GetType().Name + ")", "entry");
            }
            Stream chunkStream = GetChunkStream(legacyFileEntry);
            if (chunkStream == null)
            {
                return null;
            }
            NativeReader fileReader = new NativeReader(chunkStream);
            LegacyFileEntry.ChunkCollectorInstance chunkCollectorInstance = (legacyFileEntry.IsModified ? legacyFileEntry.CollectorInstances[0].ModifiedEntry : legacyFileEntry.CollectorInstances[0]);
            fileReader.Position = chunkCollectorInstance.Offset;
            return new MemoryStream(fileReader.ReadBytes((int)chunkCollectorInstance.Size));
        }

        public void ModifyAsset(string key, byte[] data)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            int key2 = Fnv1.HashString(key);
            if (!legacyEntries.TryGetValue(key2, out var legacyFileEntry))
            {
                return;
            }
            assetManager.RevertAsset(legacyFileEntry);
            Guid guid = assetManager.AddChunk(data, GenerateDeterministicGuid(assetManager, legacyFileEntry), null);
            foreach (LegacyFileEntry.ChunkCollectorInstance collectorInstance in legacyFileEntry.CollectorInstances)
            {
                ChunkAssetEntry chunkEntry = assetManager.GetChunkEntry(guid);
                collectorInstance.ModifiedEntry = new LegacyFileEntry.ChunkCollectorInstance
                {
                    ChunkId = guid,
                    Offset = 0L,
                    CompressedStartOffset = 0L,
                    Size = data.Length,
                    CompressedEndOffset = chunkEntry.ModifiedEntry.Data.Length
                };
                chunkEntry.ModifiedEntry.AddToChunkBundle = true;
                chunkEntry.ModifiedEntry.UserData = "legacy;" + legacyFileEntry.Name;
                legacyFileEntry.LinkAsset(chunkEntry);
                collectorInstance.Entry.LinkAsset(legacyFileEntry);
            }
            legacyFileEntry.IsDirty = true;
        }

        public static (byte[] newChunk, long uncompressedSize) RecompressChunk(Stream originalDecompressedChunkStream, List<(LegacyFileEntry lfe, LegacyFileEntry.ChunkCollectorInstance collectorInstance)> filesInChunk, Dictionary<int, byte[]> modifiedEntries)
        {
            using (NativeWriter modifiedDecompressedChunkStream = new NativeWriter(new MemoryStream((int)originalDecompressedChunkStream.Length)))
            {
                int offsetAdjustment = 0;
                foreach (var fileInChunk in filesInChunk)
                {
                    if (modifiedEntries.TryGetValue(fileInChunk.lfe.NameHash, out var modifiedData))
                    {
                        long oldSize2 = ((fileInChunk.collectorInstance.ModifiedEntry == null) ? fileInChunk.collectorInstance.Size : fileInChunk.collectorInstance.ModifiedEntry.Size);
                        fileInChunk.collectorInstance.ModifiedEntry = new LegacyFileEntry.ChunkCollectorInstance
                        {
                            ChunkId = fileInChunk.collectorInstance.ChunkId,
                            Offset = ((fileInChunk.collectorInstance.ModifiedEntry == null) ? fileInChunk.collectorInstance.Offset : fileInChunk.collectorInstance.ModifiedEntry.Offset) + offsetAdjustment,
                            Size = modifiedData.Length
                        };
                        modifiedDecompressedChunkStream.Write(modifiedData);
                        offsetAdjustment += (int)(fileInChunk.collectorInstance.ModifiedEntry.Size - oldSize2);
                    }
                    else
                    {
                        long oldOffset = ((fileInChunk.collectorInstance.ModifiedEntry == null) ? fileInChunk.collectorInstance.Offset : fileInChunk.collectorInstance.ModifiedEntry.Offset);
                        long oldSize = ((fileInChunk.collectorInstance.ModifiedEntry == null) ? fileInChunk.collectorInstance.Size : fileInChunk.collectorInstance.ModifiedEntry.Size);
                        fileInChunk.collectorInstance.ModifiedEntry = new LegacyFileEntry.ChunkCollectorInstance
                        {
                            ChunkId = fileInChunk.collectorInstance.ChunkId,
                            Offset = oldOffset + offsetAdjustment,
                            Size = oldSize
                        };
                        originalDecompressedChunkStream.Position = oldOffset;
                        originalDecompressedChunkStream.CopyTo(modifiedDecompressedChunkStream.BaseStream, (int)oldSize);
                    }
                }

                using (NativeWriter compressedStream = new NativeWriter(new MemoryStream()))
                {
                    modifiedDecompressedChunkStream.Position = 0L;
                    while (modifiedDecompressedChunkStream.Position < modifiedDecompressedChunkStream.Length)
                    {
                        long remainingInStream = modifiedDecompressedChunkStream.Length - modifiedDecompressedChunkStream.Position;
                        int amountToRead = (int)Math.Min(Utils.MaxBufferSize, remainingInStream);
                        List<LegacyFileEntry.ChunkCollectorInstance> list = (from f in filesInChunk
                                                                             select f.collectorInstance.ModifiedEntry into f
                                                                             where f.Offset >= modifiedDecompressedChunkStream.Position && f.Offset <= modifiedDecompressedChunkStream.Position + amountToRead
                                                                             select f).ToList();
                        List<LegacyFileEntry.ChunkCollectorInstance> filesEndingInBlock = (from f in filesInChunk
                                                                                           select f.collectorInstance.ModifiedEntry into f
                                                                                           where f.Offset + f.Size >= modifiedDecompressedChunkStream.Position && f.Offset + f.Size <= modifiedDecompressedChunkStream.Position + amountToRead
                                                                                           select f).ToList();
                        foreach (LegacyFileEntry.ChunkCollectorInstance item in list)
                        {
                            item.CompressedStartOffset = compressedStream.Position;
                        }

                        using (NativeReader nr_modDCS = new NativeReader(modifiedDecompressedChunkStream.BaseStream))
                        {
                            byte[] compressedBlock = Utils.CompressFile(nr_modDCS.ReadBytes(amountToRead), null, ResourceType.Invalid, CompressionType.Oodle);
                            compressedStream.Write(compressedBlock);
                            foreach (LegacyFileEntry.ChunkCollectorInstance item2 in filesEndingInBlock)
                            {
                                item2.CompressedEndOffset = compressedStream.Position;
                            }
                        }
                    }
                    return (((MemoryStream)compressedStream.BaseStream).ToArray(), modifiedDecompressedChunkStream.Length);
                }
            }
        }

        private List<(LegacyFileEntry lfe, LegacyFileEntry.ChunkCollectorInstance collectorInstance)> GetOtherFilesInSameChunk(Guid chunkId)
        {
            List<(LegacyFileEntry, LegacyFileEntry.ChunkCollectorInstance)> results = new List<(LegacyFileEntry, LegacyFileEntry.ChunkCollectorInstance)>();
            foreach (LegacyFileEntry value in legacyEntries.Values)
            {
                foreach (LegacyFileEntry.ChunkCollectorInstance collectorInstance in value.CollectorInstances)
                {
                    if (collectorInstance.ChunkId == chunkId)
                    {
                        results.Add((value, collectorInstance));
                    }
                }
            }
            return results;
        }

        private Stream GetChunkStream(LegacyFileEntry lfe)
        {
            if (lfe == null)
            {
                throw new ArgumentNullException("lfe");
            }
            if (cacheMode)
            {
                if (!cachedChunks.TryGetValue(lfe.ChunkId, out var cachedChunk))
                {
                    Stream stream = assetManager.GetChunk(assetManager.GetChunkEntry(lfe.ChunkId));
                    if (stream == null)
                    {
                        return null;
                    }
                    cachedChunk = ((MemoryStream)stream).ToArray();
                    cachedChunks[lfe.ChunkId] = cachedChunk;
                }
                return new MemoryStream(cachedChunk);
            }
            return assetManager.GetChunk(assetManager.GetChunkEntry(lfe.ChunkId));
        }

        private Stream GetCompressedChunkStream(LegacyFileEntry lfe)
        {
            if (lfe == null)
            {
                throw new ArgumentNullException("lfe");
            }
            return assetManager.GetCompressedChunk(assetManager.GetChunkEntry(lfe.ChunkId));
        }

        public static Guid GenerateDeterministicGuid(AssetManager assetManager, LegacyFileEntry lfe)
        {
            ulong num = Murmur2.HashString64(lfe.Filename, 18532uL);
            ulong value = Murmur2.HashString64(lfe.Path, 18532uL);
            int num2 = 1;
            Guid guid = Guid.Empty;
            do
            {
                using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
                {
                    nativeWriter.Write(value);
                    nativeWriter.Write((ulong)((long)num ^ (long)num2));
                    byte[] array = ((MemoryStream)nativeWriter.BaseStream).ToArray();
                    array[15] = 1;
                    guid = new Guid(array);
                }
                num2++;
            }
            while (AssetManager.Instance.GetChunkEntry(guid) != null);
            return guid;
        }
    }
    */
}
