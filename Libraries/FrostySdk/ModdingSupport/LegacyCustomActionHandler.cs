using FMT.FileTools;
using FrostySdk;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Frosty.ModSupport.Handlers
{
    [ActionHandler(3181116261u)]
    public class LegacyCustomActionHandler : Frosty.ModSupport.Handlers.ICustomActionHandler
    {
        private class LegacyFileEntry
        {
            public int Hash
            {
                get;
                set;
            }

            public Guid ChunkId
            {
                get;
                set;
            }

            public long Offset
            {
                get;
                set;
            }

            public long CompressedOffset
            {
                get;
                set;
            }

            public long CompressedSize
            {
                get;
                set;
            }

            public long Size
            {
                get;
                set;
            }
        }

        public object Load(object existing, byte[] newData)
        {
            List<LegacyFileEntry> list = (List<LegacyFileEntry>)existing;
            if (list == null)
            {
                list = new List<LegacyFileEntry>();
            }
            using (NativeReader nativeReader = new NativeReader(new MemoryStream(newData)))
            {
                while (nativeReader.Position < nativeReader.Length)
                {
                    int hash = nativeReader.ReadInt();
                    int num = list.FindIndex((LegacyFileEntry a) => a.Hash == hash);
                    if (num != -1)
                    {
                        list.RemoveAt(num);
                    }
                    LegacyFileEntry legacyFileEntry = new LegacyFileEntry();
                    legacyFileEntry.Hash = hash;
                    legacyFileEntry.ChunkId = nativeReader.ReadGuid();
                    legacyFileEntry.Offset = nativeReader.ReadLong();
                    legacyFileEntry.CompressedOffset = nativeReader.ReadLong();
                    legacyFileEntry.CompressedSize = nativeReader.ReadLong();
                    legacyFileEntry.Size = nativeReader.ReadLong();
                    list.Add(legacyFileEntry);
                }
                return list;
            }
        }

        public AssetEntry Modify(AssetEntry origEntry, Stream baseStream, object data, out byte[] outData)
        {
            List<LegacyFileEntry> list = (List<LegacyFileEntry>)data;
            using (NativeReader nativeReader = new NativeReader(baseStream))
            {
                using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
                {
                    int num = nativeReader.ReadInt();
                    nativeWriter.Write(num);
                    long num2 = nativeReader.ReadLong();
                    nativeWriter.Write(num2);
                    nativeWriter.Write(nativeReader.ReadBytes((int)(num2 - 12)));
                    for (int i = 0; i < num; i++)
                    {
                        long num3 = nativeReader.ReadLong();
                        long position = nativeReader.Position;
                        nativeReader.Position = num3;
                        string data2 = nativeReader.ReadNullTerminatedString();
                        int hash = Fnv1.HashString(data2);
                        nativeReader.Position = position;
                        int num4 = list.FindIndex((LegacyFileEntry a) => a.Hash == hash);
                        if (num4 != -1)
                        {
                            LegacyFileEntry legacyFileEntry = list[num4];
                            nativeWriter.Write(num3);
                            nativeWriter.Write(legacyFileEntry.CompressedOffset);
                            nativeWriter.Write(legacyFileEntry.CompressedSize);
                            nativeWriter.Write(legacyFileEntry.Offset);
                            nativeWriter.Write(legacyFileEntry.Size);
                            nativeWriter.Write(legacyFileEntry.ChunkId);
                            nativeReader.Position += 48L;
                        }
                        else
                        {
                            nativeWriter.Write(num3);
                            nativeWriter.Write(nativeReader.ReadBytes(48));
                        }
                    }
                    nativeWriter.Write(nativeReader.ReadToEnd());
                    outData = Utils.CompressFile(((MemoryStream)nativeWriter.BaseStream).ToArray());
                    return new ChunkAssetEntry
                    {
                        Sha1 = FMT.FileTools.Sha1.Create(outData),
                        Size = outData.Length,
                        IsTocChunk = true
                    };
                }
            }
        }
    }
}
