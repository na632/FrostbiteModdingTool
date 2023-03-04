using FMT.FileTools;
using FrostySdk.Frostbite.IO;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.Frostbite.Loaders
{
    internal class FifaAssetLoader : IAssetLoader
    {
        internal struct BundleFileInfo
        {
            public int Index;

            public int Offset;

            public int Size;

            public BundleFileInfo(int index, int offset, int size)
            {
                Index = index;
                Offset = offset;
                Size = size;
            }
        }

        public void Load(AssetManager parent, BinarySbDataHelper helper)
        {
            byte[] key = KeyManager.Instance.GetKey("Key2");
            foreach (Catalog item2 in parent.FileSystem.EnumerateCatalogInfos())
            {
                foreach (string sbName in item2.SuperBundles.Keys)
                {
                    SuperBundleEntry superBundleEntry = parent.superBundles.Find((SuperBundleEntry a) => a.Name == sbName);
                    int num = -1;
                    if (superBundleEntry != null)
                    {
                        num = parent.superBundles.IndexOf(superBundleEntry);
                    }
                    else
                    {
                        parent.superBundles.Add(new SuperBundleEntry
                        {
                            Name = sbName
                        });
                        num = parent.superBundles.Count - 1;
                    }
                    parent.WriteToLog($"Loading data ({sbName})");
                    parent.superBundles.Add(new SuperBundleEntry
                    {
                        Name = sbName
                    });
                    string arg = sbName;
                    if (item2.SuperBundles[sbName])
                    {
                        arg = sbName.Replace("win32", item2.Name);
                    }

                    string resolvedTocPath = parent.FileSystem.ResolvePath($"{arg}.toc");
                    if (string.IsNullOrEmpty(resolvedTocPath))
                        continue;

                    {
                        int num2 = 0;
                        int num3 = 0;
                        byte[] array = null;
                        using (NativeReader nativeReader = new DeobfuscatedReader(new FileStream(resolvedTocPath, FileMode.Open, FileAccess.Read), parent.FileSystem.CreateDeobfuscator()))
                        {
                            uint num4 = nativeReader.ReadUInt();
                            num2 = nativeReader.ReadInt() - 12;
                            num3 = nativeReader.ReadInt() - 12;
                            array = nativeReader.ReadToEnd();
                            if (num4 == 3286619587u)
                            {
                                using (Aes aes = Aes.Create())
                                {
                                    aes.Key = key;
                                    aes.IV = key;
                                    aes.Padding = PaddingMode.None;
                                    ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
                                    using (MemoryStream stream = new MemoryStream(array))
                                    {
                                        using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
                                        {
                                            cryptoStream.Read(array, 0, array.Length);
                                        }
                                    }
                                }
                            }
                        }
                        if (array.Length != 0)
                        {
                            using (NativeReader nativeReader2 = new NativeReader(new MemoryStream(array)))
                            {
                                List<int> list = new List<int>();
                                if (num2 > 0)
                                {
                                    nativeReader2.Position = num2;
                                    int numberOfBundles = nativeReader2.ReadInt();
                                    for (int i = 0; i < numberOfBundles; i++)
                                    {
                                        list.Add(nativeReader2.ReadInt());
                                    }
                                    DateTime lastLogTime = DateTime.Now;

                                    for (int j = 0; j < numberOfBundles; j++)
                                    {
                                        if (lastLogTime.AddSeconds(15) < DateTime.Now)
                                        {
                                            var percentDone = Math.Round(j / (double)numberOfBundles * 100.0);
                                            parent.Logger.Log($"{arg} Progress: {percentDone}");
                                            lastLogTime = DateTime.Now;
                                        }


                                        int num6 = nativeReader2.ReadInt() - 12;
                                        long position = nativeReader2.Position;
                                        nativeReader2.Position = num6;
                                        int num7 = nativeReader2.ReadInt() - 1;
                                        List<BundleFileInfo> list2 = new List<BundleFileInfo>();
                                        MemoryStream memoryStream = new MemoryStream();
                                        int num8;
                                        do
                                        {
                                            num8 = nativeReader2.ReadInt();
                                            int num9 = nativeReader2.ReadInt();
                                            int num10 = nativeReader2.ReadInt();
                                            using (NativeReader nativeReader3 = new NativeReader(new FileStream(parent.FileSystem.ResolvePath(parent.FileSystem.GetFilePath(num8 & int.MaxValue)), FileMode.Open, FileAccess.Read)))
                                            {
                                                nativeReader3.Position = num9;
                                                memoryStream.Write(nativeReader3.ReadBytes(num10), 0, num10);
                                            }
                                            list2.Add(new BundleFileInfo(num8 & int.MaxValue, num9, num10));
                                        }
                                        while ((num8 & 2147483648u) != 0L);
                                        nativeReader2.Position = num7 - 12;
                                        int num11 = 0;
                                        string text2 = "";
                                        do
                                        {
                                            string str = nativeReader2.ReadNullTerminatedString();
                                            num11 = nativeReader2.ReadInt() - 1;
                                            text2 += str;
                                            if (num11 != -1)
                                            {
                                                nativeReader2.Position = num11 - 12;
                                            }
                                        }
                                        while (num11 != -1);
                                        text2 = Utils.ReverseString(text2);
                                        nativeReader2.Position = position;
                                        BundleEntry item = new BundleEntry
                                        {
                                            Name = text2,
                                            SuperBundleId = num
                                        };
                                        parent.Bundles.Add(item);
                                        BinarySbReader binarySbReader = null;
                                        if (ProfileManager.IsMadden21DataVersion(ProfileManager.Game))
                                            binarySbReader = new BinarySbReaderV2(memoryStream, 0L, parent.FileSystem.CreateDeobfuscator());
                                        else
                                            binarySbReader = new BinarySbReader(memoryStream, 0L, parent.FileSystem.CreateDeobfuscator());

                                        using (binarySbReader)
                                        {
                                            DbObject dbObject = binarySbReader.ReadDbObject();
                                            BundleFileInfo bundleFileInfo = list2[0];
                                            long offset = bundleFileInfo.Offset + (dbObject.GetValue("dataOffset", 0L) + 4);
                                            long currentSize = bundleFileInfo.Size - (dbObject.GetValue("dataOffset", 0L) + 4);
                                            int num14 = 0;
                                            foreach (DbObject item3 in dbObject.GetValue<DbObject>("ebx"))
                                            {
                                                if (currentSize == 0L)
                                                {
                                                    bundleFileInfo = list2[++num14];
                                                    currentSize = bundleFileInfo.Size;
                                                    offset = bundleFileInfo.Offset;
                                                }
                                                int value = item3.GetValue("size", 0);
                                                item3.SetValue("offset", offset);
                                                item3.SetValue("cas", bundleFileInfo.Index);
                                                offset += value;
                                                currentSize -= value;
                                            }
                                            foreach (DbObject item4 in dbObject.GetValue<DbObject>("res"))
                                            {
                                                if (currentSize == 0L)
                                                {
                                                    bundleFileInfo = list2[++num14];
                                                    currentSize = bundleFileInfo.Size;
                                                    offset = bundleFileInfo.Offset;
                                                }
                                                int value2 = item4.GetValue("size", 0);
                                                item4.SetValue("offset", offset);
                                                item4.SetValue("cas", bundleFileInfo.Index);
                                                offset += value2;
                                                currentSize -= value2;
                                            }
                                            foreach (DbObject item5 in dbObject.GetValue<DbObject>("chunks"))
                                            {
                                                if (currentSize == 0L)
                                                {
                                                    bundleFileInfo = list2[++num14];
                                                    currentSize = bundleFileInfo.Size;
                                                    offset = bundleFileInfo.Offset;
                                                }
                                                int value3 = item5.GetValue("size", 0);
                                                item5.SetValue("offset", offset);
                                                item5.SetValue("cas", bundleFileInfo.Index);
                                                offset += value3;
                                                currentSize -= value3;
                                            }
                                            parent.ProcessBundleEbx(dbObject, parent.Bundles.Count - 1, helper);
                                            parent.ProcessBundleRes(dbObject, parent.Bundles.Count - 1, helper);
                                            parent.ProcessBundleChunks(dbObject, parent.Bundles.Count - 1, helper);
                                        }
                                    }
                                }
                                if (num3 > 0)
                                {
                                    nativeReader2.Position = num3;
                                    int num15 = nativeReader2.ReadInt();
                                    list = new List<int>();
                                    for (int k = 0; k < num15; k++)
                                    {
                                        list.Add(nativeReader2.ReadInt());
                                    }
                                    for (int l = 0; l < num15; l++)
                                    {
                                        int num16 = nativeReader2.ReadInt();
                                        long position2 = nativeReader2.Position;
                                        nativeReader2.Position = num16 - 12;
                                        Guid guid = nativeReader2.ReadGuid();
                                        int index = nativeReader2.ReadInt();
                                        int offset = nativeReader2.ReadInt();
                                        int num18 = nativeReader2.ReadInt();
                                        if (!parent.Chunks.ContainsKey(guid))
                                        {
                                            //parent.chunkList.Add(guid, new ChunkAssetEntry());
                                            parent.Chunks.TryAdd(guid, new ChunkAssetEntry());
                                        }
                                        ChunkAssetEntry chunkAssetEntry = parent.Chunks[guid];
                                        chunkAssetEntry.Id = guid;
                                        chunkAssetEntry.Size = num18;
                                        chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
                                        chunkAssetEntry.ExtraData = new AssetExtraData();
                                        chunkAssetEntry.ExtraData.CasPath = parent.FileSystem.GetFilePath(index);
                                        chunkAssetEntry.ExtraData.DataOffset = (uint)offset;
                                        parent.Chunks[guid].IsTocChunk = true;
                                        nativeReader2.Position = position2;
                                    }
                                }
                            }
                        }
                    }
                    num++;
                }
            }
        }
    }

}
