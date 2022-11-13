using Frosty.Hash;
using Frosty.ModSupport.Handlers;
using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Frosty.ModSupport;
using System.IO.Compression;
using FrostySdk.FrostySdk.Deobfuscators;
using FrostySdk.Frosty;
using Newtonsoft.Json;
using FrostbiteSdk.Frosty.Abstract;
using System.Text;
using FrostbiteSdk.FrostbiteSdk.Managers;
using System.Collections.Concurrent;
using FrostbiteSdk;
using v2k4FIFAModdingCL;
using System.Xml;
using FrostySdk.Frostbite.PluginInterfaces;
using System.Runtime.CompilerServices;
using FrostyModManager;
using Microsoft.Win32;
using Frostbite.FileManagers;

namespace ModdingSupport
{
    public class ArchiveInfo
    {
        public byte[] Data;

        public int RefCount;
    }

    public class ModExecutor
    {
        public class ModBundleInfo
        {


            public class ModBundleAction
            {
                public List<string> Ebx = new List<string>();

                public List<string> Res = new List<string>();

                public List<Guid> Chunks = new List<Guid>();

                public List<string> Legacy = new List<string>();

                public void AddEbx(string name)
                {
                    if (!Ebx.Contains(name))
                    {
                        Ebx.Add(name);
                    }
                }

                public void AddRes(string name)
                {
                    if (!Res.Contains(name))
                    {
                        Res.Add(name);
                    }
                }

                public void AddChunk(Guid guid)
                {
                    if (!Chunks.Contains(guid))
                    {
                        Chunks.Add(guid);
                    }
                }

                public void AddLegacy(string name)
                {
                    if (!Legacy.Contains(name))
                    {
                        Legacy.Add(name);
                    }
                }
            }

            public int Name;

            public ModBundleAction Add = new ModBundleAction();

            public ModBundleAction Remove = new ModBundleAction();

            public ModBundleAction Modify = new ModBundleAction();
        }

        public class CasFileEntry
        {
            public ManifestFileInfo FileInfo;

            public ChunkAssetEntry Entry;
        }

        public class CasDataEntry
        {
            private string catalog;

            private List<Sha1> dataRefs = new List<Sha1>();

            private Dictionary<Sha1, List<CasFileEntry>> fileInfos = new Dictionary<Sha1, List<CasFileEntry>>();

            public string Catalog => catalog;

            public bool HasEntries => dataRefs.Count != 0;

            public CasDataEntry(string inCatalog, params Sha1[] sha1)
            {
                catalog = inCatalog;
                if (sha1.Length != 0)
                {
                    dataRefs.AddRange(sha1);
                }
            }

            public void Add(Sha1 sha1, ChunkAssetEntry entry = null, ManifestFileInfo file = null)
            {
                if (!dataRefs.Contains(sha1))
                {
                    dataRefs.Add(sha1);
                    fileInfos.Add(sha1, new List<CasFileEntry>());
                }
                if (entry != null || file != null)
                {
                    fileInfos[sha1].Add(new CasFileEntry
                    {
                        Entry = entry,
                        FileInfo = file
                    });
                }
            }

            public bool Contains(Sha1 sha1)
            {
                return dataRefs.Contains(sha1);
            }

            public IEnumerable<Sha1> EnumerateDataRefs()
            {
                foreach (Sha1 dataRef in dataRefs)
                {
                    yield return dataRef;
                }
            }

            public IEnumerable<CasFileEntry> EnumerateFileInfos(Sha1 sha1)
            {
                int num = dataRefs.IndexOf(sha1);
                if (num != -1 && num < fileInfos.Count)
                {
                    foreach (CasFileEntry item in fileInfos[sha1])
                    {
                        yield return item;
                    }
                }
            }
        }

        public class CasDataInfo
        {
            private Dictionary<string, CasDataEntry> entries = new Dictionary<string, CasDataEntry>();

            public void Add(string catalog, Sha1 sha1, ChunkAssetEntry entry = null, ManifestFileInfo file = null)
            {
                if (!entries.ContainsKey(catalog))
                {
                    entries.Add(catalog, new CasDataEntry(catalog));
                }
                entries[catalog].Add(sha1, entry, file);
            }

            public IEnumerable<CasDataEntry> EnumerateEntries()
            {
                foreach (CasDataEntry value in entries.Values)
                {
                    yield return value;
                }
            }
        }

        public class FrostySymLinkException : Exception
        {
            public override string Message => "One ore more symbolic links could not be created, please restart tool as Administrator.";
        }

        public class HandlerExtraData : AssetExtraData
        {
            public Frosty.ModSupport.Handlers.ICustomActionHandler Handler
            //public FrostySdk.ICustomActionHandler Handler
            {
                get;
                set;
            }

            public object Data
            {
                get;
                set;
            }
        }

        public struct SymLinkStruct
        {
            public string dest;

            public string src;

            public bool isFolder;

            public SymLinkStruct(string inDst, string inSrc, bool inFolder)
            {
                dest = inDst;
                src = inSrc;
                isFolder = inFolder;
            }
        }

        public class FifaBundleAction
        {
            private class BundleFileEntry
            {
                public int CasIndex;

                public int Offset;

                public int Size;

                public BundleFileEntry(int inCasIndex, int inOffset, int inSize)
                {
                    CasIndex = inCasIndex;
                    Offset = inOffset;
                    Size = inSize;
                }
            }

            private static readonly object locker = new object();

            public static int CasFileCount = 0;

            private Exception errorException;

            private ManualResetEvent doneEvent;

            private ModExecutor parent;

            private Catalog catalogInfo;

            private Dictionary<int, string> casFiles = new Dictionary<int, string>();

            public Catalog CatalogInfo => catalogInfo;

            public Dictionary<int, string> CasFiles => casFiles;

            public bool HasErrored => errorException != null;

            public Exception Exception => errorException;

            public FifaBundleAction(Catalog inCatalogInfo, ManualResetEvent inDoneEvent, ModExecutor inParent)
            {
                catalogInfo = inCatalogInfo;
                parent = inParent;
                doneEvent = inDoneEvent;
            }

            public void Run()
            {
                try
                {
                    NativeWriter writer_new_cas_file = null;
                    int casFileIndex = 0;
                    byte[] key_2_from_key_manager = KeyManager.Instance.GetKey("Key2");
                    foreach (string key3 in catalogInfo.SuperBundles.Keys)
                    {
                        string arg = key3;
                        if (catalogInfo.SuperBundles[key3])
                        {
                            arg = key3.Replace("win32", catalogInfo.Name);
                        }
                        string location_toc_file = parent.fs.ResolvePath($"{arg}.toc").ToLower();
                        if (location_toc_file != "")
                        {
                            uint orig_toc_file_num1 = 0u;
                            uint tocchunkposition = 0u;
                            byte[] byte_array_of_original_toc_file = null;
                            using (NativeReader reader_original_toc_file = new NativeReader(new FileStream(location_toc_file, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
                            {
                                uint orig_toc_file_num = reader_original_toc_file.ReadUInt();
                                orig_toc_file_num1 = reader_original_toc_file.ReadUInt();
                                tocchunkposition = reader_original_toc_file.ReadUInt();
                                byte_array_of_original_toc_file = reader_original_toc_file.ReadToEnd();
                                if (orig_toc_file_num == 3286619587u)
                                {
                                    using (Aes aes = Aes.Create())
                                    {
                                        aes.Key = key_2_from_key_manager;
                                        aes.IV = key_2_from_key_manager;
                                        aes.Padding = PaddingMode.None;
                                        ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
                                        using (MemoryStream stream = new MemoryStream(byte_array_of_original_toc_file))
                                        {
                                            using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
                                            {
                                                cryptoStream.Read(byte_array_of_original_toc_file, 0, byte_array_of_original_toc_file.Length);
                                            }
                                        }
                                    }
                                }
                            }
                            string location_toc_file_mod_data = location_toc_file.Replace("patch\\win32", "moddata\\patch\\win32");
                            FileInfo fi_toc_file_mod_data = new FileInfo(location_toc_file_mod_data);
                            if (!Directory.Exists(fi_toc_file_mod_data.DirectoryName))
                            {
                                Directory.CreateDirectory(fi_toc_file_mod_data.DirectoryName);
                            }
                            using (NativeWriter writer_new_toc_file_mod_data = new NativeWriter(new FileStream(location_toc_file_mod_data, FileMode.Create)))
                            {
                                writer_new_toc_file_mod_data.Write(30331136);
                                writer_new_toc_file_mod_data.Write(new byte[552]);
                                long position = writer_new_toc_file_mod_data.BaseStream.Position;
                                writer_new_toc_file_mod_data.Write(3280507699u);
                                long num4 = 4294967295L;
                                long num5 = 4294967295L;
                                if (byte_array_of_original_toc_file.Length != 0)
                                {
                                    writer_new_toc_file_mod_data.Write(3735928559u);
                                    writer_new_toc_file_mod_data.Write(3735928559u);
                                    using (NativeReader reader_of_original_toc_file_array = new NativeReader(new MemoryStream(byte_array_of_original_toc_file)))
                                    {
                                        if (orig_toc_file_num1 != uint.MaxValue)
                                        {
                                            reader_of_original_toc_file_array.Position = orig_toc_file_num1 - 12;
                                            int bundleCount = reader_of_original_toc_file_array.ReadInt();
                                            List<int> list = new List<int>();
                                            for (int i = 0; i < bundleCount; i++)
                                            {
                                                list.Add(reader_of_original_toc_file_array.ReadInt());
                                            }
                                            List<int> list2 = new List<int>();
                                            for (int j = 0; j < bundleCount; j++)
                                            {
                                                int num7 = reader_of_original_toc_file_array.ReadInt() - 12;
                                                long position2 = reader_of_original_toc_file_array.Position;
                                                reader_of_original_toc_file_array.Position = num7;
                                                int num8 = reader_of_original_toc_file_array.ReadInt() - 1;
                                                List<BundleFileEntry> lstBundleFiles3 = new List<BundleFileEntry>();
                                                int num9;
                                                do
                                                {
                                                    num9 = reader_of_original_toc_file_array.ReadInt();
                                                    int inOffset = reader_of_original_toc_file_array.ReadInt();
                                                    int inSize = reader_of_original_toc_file_array.ReadInt();
                                                    lstBundleFiles3.Add(new BundleFileEntry(num9 & int.MaxValue, inOffset, inSize));
                                                }
                                                while ((num9 & 2147483648u) != 0L);
                                                reader_of_original_toc_file_array.Position = num8 - 12;
                                                int num10 = 0;
                                                string text3 = "";
                                                do
                                                {
                                                    string str = reader_of_original_toc_file_array.ReadNullTerminatedString();
                                                    num10 = reader_of_original_toc_file_array.ReadInt() - 1;
                                                    text3 = Utils.ReverseString(str) + text3;
                                                    if (num10 != -1)
                                                    {
                                                        reader_of_original_toc_file_array.Position = num10 - 12;
                                                    }
                                                }
                                                while (num10 != -1);
                                                reader_of_original_toc_file_array.Position = position2;
                                                int key2 = Fnv1.HashString(text3.ToLower());
                                                if (parent.modifiedBundles.ContainsKey(key2))
                                                {
                                                    ModBundleInfo modBundleInfo = parent.modifiedBundles[key2];
                                                    MemoryStream memoryStream = new MemoryStream();
                                                    foreach (BundleFileEntry item in lstBundleFiles3)
                                                    {
                                                        using (NativeReader nativeReader3 = new NativeReader(new FileStream(parent.fs.ResolvePath(parent.fs.GetFilePath(item.CasIndex)), FileMode.Open, FileAccess.Read)))
                                                        {
                                                            nativeReader3.Position = item.Offset;
                                                            memoryStream.Write(nativeReader3.ReadBytes(item.Size), 0, item.Size);
                                                        }
                                                    }
                                                    DbObject dbObject = null;
                                                    using (BinarySbReader binarySbReader = new BinarySbReader(memoryStream, 0L, parent.fs.CreateDeobfuscator()))
                                                    {
                                                        dbObject = binarySbReader.ReadDbObject();
                                                        foreach (DbObject ebxItem in dbObject.GetValue<DbObject>("ebx"))
                                                        {
                                                            ebxItem.GetValue("size", 0);
                                                            long value = ebxItem.GetValue("offset", 0L);
                                                            long num11 = 0L;
                                                            foreach (BundleFileEntry item3 in lstBundleFiles3)
                                                            {
                                                                if (value < num11 + item3.Size)
                                                                {
                                                                    value -= num11;
                                                                    value += item3.Offset;
                                                                    ebxItem.SetValue("offset", value);
                                                                    ebxItem.SetValue("cas", item3.CasIndex);
                                                                    break;
                                                                }
                                                                num11 += item3.Size;
                                                            }
                                                        }
                                                        foreach (DbObject item4 in dbObject.GetValue<DbObject>("res"))
                                                        {
                                                            item4.GetValue("size", 0);
                                                            long value2 = item4.GetValue("offset", 0L);
                                                            long num12 = 0L;
                                                            foreach (BundleFileEntry item5 in lstBundleFiles3)
                                                            {
                                                                if (value2 < num12 + item5.Size)
                                                                {
                                                                    value2 -= num12;
                                                                    value2 += item5.Offset;
                                                                    item4.SetValue("offset", value2);
                                                                    item4.SetValue("cas", item5.CasIndex);
                                                                    break;
                                                                }
                                                                num12 += item5.Size;
                                                            }
                                                        }
                                                        foreach (DbObject item6 in dbObject.GetValue<DbObject>("chunks"))
                                                        {
                                                            item6.GetValue("size", 0);
                                                            long value3 = item6.GetValue("offset", 0L);
                                                            long num13 = 0L;
                                                            foreach (BundleFileEntry item7 in lstBundleFiles3)
                                                            {
                                                                if (value3 < num13 + item7.Size)
                                                                {
                                                                    value3 -= num13;
                                                                    value3 += item7.Offset;
                                                                    item6.SetValue("offset", value3);
                                                                    item6.SetValue("cas", item7.CasIndex);
                                                                    break;
                                                                }
                                                                num13 += item7.Size;
                                                            }
                                                        }
                                                    }
                                                    foreach (DbObject ebx in dbObject.GetValue<DbObject>("ebx"))
                                                    {
                                                        int num14 = modBundleInfo.Modify.Ebx.FindIndex((string a) => a.Equals(ebx.GetValue<string>("name")));
                                                        if (num14 != -1)
                                                        {
                                                            EbxAssetEntry ebxAssetEntry = parent.modifiedEbx[modBundleInfo.Modify.Ebx[num14]];
                                                            if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + parent.archiveData[ebxAssetEntry.Sha1].Data.Length > 1073741824)
                                                            {
                                                                writer_new_cas_file?.Close();
                                                                writer_new_cas_file = GetNextCas(out casFileIndex);
                                                            }
                                                            ebx.SetValue("originalSize", ebxAssetEntry.OriginalSize);
                                                            ebx.SetValue("size", ebxAssetEntry.Size);
                                                            ebx.SetValue("cas", casFileIndex);
                                                            ebx.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                                            var ebxData = parent.archiveData[ebxAssetEntry.Sha1].Data;
                                                            //using (FileStream fileStream = new FileStream("test.xmx", FileMode.OpenOrCreate))
                                                            //{
                                                            //    fileStream.Write(ebxData, 0, ebxData.Length);
                                                            //}

                                                            writer_new_cas_file.Write(ebxData);
                                                        }
                                                    }
                                                    foreach (string item8 in modBundleInfo.Add.Ebx)
                                                    {
                                                        EbxAssetEntry ebxAssetEntry2 = parent.modifiedEbx[item8];
                                                        if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + parent.archiveData[ebxAssetEntry2.Sha1].Data.Length > 1073741824)
                                                        {
                                                            writer_new_cas_file?.Close();
                                                            writer_new_cas_file = GetNextCas(out casFileIndex);
                                                        }
                                                        DbObject dbObject5 = DbObject.CreateObject();
                                                        dbObject5.SetValue("name", ebxAssetEntry2.Name);
                                                        dbObject5.SetValue("originalSize", ebxAssetEntry2.OriginalSize);
                                                        dbObject5.SetValue("size", ebxAssetEntry2.Size);
                                                        dbObject5.SetValue("cas", casFileIndex);
                                                        dbObject5.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                                        dbObject.GetValue<DbObject>("ebx").Add(dbObject5);
                                                        writer_new_cas_file.Write(parent.archiveData[ebxAssetEntry2.Sha1].Data);
                                                    }
                                                    foreach (DbObject res in dbObject.GetValue<DbObject>("res"))
                                                    {
                                                        int num15 = modBundleInfo.Modify.Res.FindIndex((string a) => a.Equals(res.GetValue<string>("name")));
                                                        if (num15 != -1)
                                                        {
                                                            ResAssetEntry resAssetEntry = parent.modifiedRes[modBundleInfo.Modify.Res[num15]];
                                                            if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + parent.archiveData[resAssetEntry.Sha1].Data.Length > 1073741824)
                                                            {
                                                                writer_new_cas_file?.Close();
                                                                writer_new_cas_file = GetNextCas(out casFileIndex);
                                                            }
                                                            res.SetValue("originalSize", resAssetEntry.OriginalSize);
                                                            res.SetValue("size", resAssetEntry.Size);
                                                            res.SetValue("cas", casFileIndex);
                                                            res.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                                            res.SetValue("resRid", (long)resAssetEntry.ResRid);
                                                            res.SetValue("resMeta", resAssetEntry.ResMeta);
                                                            res.SetValue("resType", resAssetEntry.ResType);
                                                            writer_new_cas_file.Write(parent.archiveData[resAssetEntry.Sha1].Data);
                                                        }
                                                    }
                                                    foreach (string re in modBundleInfo.Add.Res)
                                                    {
                                                        ResAssetEntry resAssetEntry2 = parent.modifiedRes[re];
                                                        if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + parent.archiveData[resAssetEntry2.Sha1].Data.Length > 1073741824)
                                                        {
                                                            writer_new_cas_file?.Close();
                                                            writer_new_cas_file = GetNextCas(out casFileIndex);
                                                        }
                                                        DbObject dbObject6 = DbObject.CreateObject();
                                                        dbObject6.SetValue("name", resAssetEntry2.Name);
                                                        dbObject6.SetValue("originalSize", resAssetEntry2.OriginalSize);
                                                        dbObject6.SetValue("size", resAssetEntry2.Size);
                                                        dbObject6.SetValue("cas", casFileIndex);
                                                        dbObject6.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                                        dbObject6.SetValue("resRid", (long)resAssetEntry2.ResRid);
                                                        dbObject6.SetValue("resMeta", resAssetEntry2.ResMeta);
                                                        dbObject6.SetValue("resType", resAssetEntry2.ResType);
                                                        dbObject.GetValue<DbObject>("res").Add(dbObject6);
                                                        writer_new_cas_file.Write(parent.archiveData[resAssetEntry2.Sha1].Data);
                                                    }
                                                    foreach (DbObject chunk in dbObject.GetValue<DbObject>("chunks"))
                                                    {
                                                        int num16 = modBundleInfo.Modify.Chunks.FindIndex((Guid a) => a == chunk.GetValue<Guid>("id"));
                                                        if (num16 != -1)
                                                        {
                                                            ChunkAssetEntry chunkAssetEntry = parent.ModifiedChunks[modBundleInfo.Modify.Chunks[num16]];
                                                            if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + parent.archiveData[chunkAssetEntry.Sha1].Data.Length > 1073741824)
                                                            {
                                                                writer_new_cas_file?.Close();
                                                                writer_new_cas_file = GetNextCas(out casFileIndex);
                                                            }
                                                            chunk.SetValue("originalSize", chunkAssetEntry.OriginalSize);
                                                            chunk.SetValue("size", chunkAssetEntry.Size);
                                                            chunk.SetValue("cas", casFileIndex);
                                                            chunk.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                                            chunk.SetValue("logicalOffset", chunkAssetEntry.LogicalOffset);
                                                            chunk.SetValue("logicalSize", chunkAssetEntry.LogicalSize);
                                                            writer_new_cas_file.Write(parent.archiveData[chunkAssetEntry.Sha1].Data);
                                                        }
                                                    }
                                                    foreach (Guid chunk2 in modBundleInfo.Add.Chunks)
                                                    {
                                                        ChunkAssetEntry chunkAssetEntry2 = parent.ModifiedChunks[chunk2];
                                                        if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + parent.archiveData[chunkAssetEntry2.Sha1].Data.Length > 1073741824)
                                                        {
                                                            writer_new_cas_file?.Close();
                                                            writer_new_cas_file = GetNextCas(out casFileIndex);
                                                        }
                                                        DbObject dbObject7 = DbObject.CreateObject();
                                                        dbObject7.SetValue("id", chunkAssetEntry2.Id);
                                                        dbObject7.SetValue("originalSize", chunkAssetEntry2.OriginalSize);
                                                        dbObject7.SetValue("size", chunkAssetEntry2.Size);
                                                        dbObject7.SetValue("cas", casFileIndex);
                                                        dbObject7.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                                        dbObject7.SetValue("logicalOffset", chunkAssetEntry2.LogicalOffset);
                                                        dbObject7.SetValue("logicalSize", chunkAssetEntry2.LogicalSize);
                                                        dbObject.GetValue<DbObject>("chunks").Add(dbObject7);
                                                        DbObject dbObject8 = DbObject.CreateObject();
                                                        dbObject8.SetValue("h32", chunkAssetEntry2.H32);
                                                        DbObject dbObject9 = DbObject.CreateObject();
                                                        if (chunkAssetEntry2.FirstMip != -1)
                                                        {
                                                            dbObject9.SetValue("firstMip", chunkAssetEntry2.FirstMip);
                                                        }
                                                        dbObject.GetValue<DbObject>("chunkMeta").Add(dbObject8);
                                                        writer_new_cas_file.Write(parent.archiveData[chunkAssetEntry2.Sha1].Data);
                                                    }
                                                    BundleFileEntry bundleFileEntry = lstBundleFiles3[0];
                                                    lstBundleFiles3.Clear();
                                                    lstBundleFiles3.Add(bundleFileEntry);
                                                    foreach (DbObject item9 in dbObject.GetValue<DbObject>("ebx"))
                                                    {
                                                        lstBundleFiles3.Add(new BundleFileEntry(item9.GetValue("cas", 0), item9.GetValue("offset", 0), item9.GetValue("size", 0)));
                                                    }
                                                    foreach (DbObject item10 in dbObject.GetValue<DbObject>("res"))
                                                    {
                                                        lstBundleFiles3.Add(new BundleFileEntry(item10.GetValue("cas", 0), item10.GetValue("offset", 0), item10.GetValue("size", 0)));
                                                    }
                                                    foreach (DbObject item11 in dbObject.GetValue<DbObject>("chunks"))
                                                    {
                                                        lstBundleFiles3.Add(new BundleFileEntry(item11.GetValue("cas", 0), item11.GetValue("offset", 0), item11.GetValue("size", 0)));
                                                    }
                                                    int count = dbObject.GetValue<DbObject>("ebx").Count;
                                                    int count2 = dbObject.GetValue<DbObject>("res").Count;
                                                    int count3 = dbObject.GetValue<DbObject>("chunks").Count;
                                                    using (NativeWriter nwCas2 = new NativeWriter(new MemoryStream()))
                                                    {
                                                        nwCas2.Write(3735927486u, Endian.Big);
                                                        nwCas2.Write(3018715229u, Endian.Big);
                                                        nwCas2.Write(count + count2 + count3, Endian.Big);
                                                        nwCas2.Write(count, Endian.Big);
                                                        nwCas2.Write(count2, Endian.Big);
                                                        nwCas2.Write(count3, Endian.Big);
                                                        nwCas2.Write(3735927486u, Endian.Big);
                                                        nwCas2.Write(3735927486u, Endian.Big);
                                                        nwCas2.Write(3735927486u, Endian.Big);
                                                        long num17 = 0L;
                                                        new Dictionary<uint, long>();
                                                        List<string> list4 = new List<string>();
                                                        foreach (DbObject item12 in dbObject.GetValue<DbObject>("ebx"))
                                                        {
                                                            Fnv1.HashString(item12.GetValue<string>("name"));
                                                            nwCas2.Write((uint)num17, Endian.Big);
                                                            list4.Add(item12.GetValue<string>("name"));
                                                            num17 += item12.GetValue<string>("name").Length + 1;
                                                            nwCas2.Write(item12.GetValue("originalSize", 0), Endian.Big);
                                                        }
                                                        foreach (DbObject item13 in dbObject.GetValue<DbObject>("res"))
                                                        {
                                                            Fnv1.HashString(item13.GetValue<string>("name"));
                                                            nwCas2.Write((uint)num17, Endian.Big);
                                                            list4.Add(item13.GetValue<string>("name"));
                                                            num17 += item13.GetValue<string>("name").Length + 1;
                                                            nwCas2.Write(item13.GetValue("originalSize", 0), Endian.Big);
                                                        }
                                                        foreach (DbObject item14 in dbObject.GetValue<DbObject>("res"))
                                                        {
                                                            nwCas2.Write((uint)item14.GetValue("resType", 0L), Endian.Big);
                                                        }
                                                        foreach (DbObject item15 in dbObject.GetValue<DbObject>("res"))
                                                        {
                                                            nwCas2.Write(item15.GetValue<byte[]>("resMeta"));
                                                        }
                                                        foreach (DbObject item16 in dbObject.GetValue<DbObject>("res"))
                                                        {
                                                            nwCas2.Write(item16.GetValue("resRid", 0L), Endian.Big);
                                                        }
                                                        foreach (DbObject item17 in dbObject.GetValue<DbObject>("chunks"))
                                                        {
                                                            nwCas2.Write(item17.GetValue<Guid>("id"), Endian.Big);
                                                            nwCas2.Write(item17.GetValue("logicalOffset", 0), Endian.Big);
                                                            nwCas2.Write(item17.GetValue("logicalSize", 0), Endian.Big);
                                                        }
                                                        long position3 = nwCas2.BaseStream.Position;
                                                        foreach (string item18 in list4)
                                                        {
                                                            nwCas2.WriteNullTerminatedString(item18);
                                                        }
                                                        long num18 = 0L;
                                                        long num19 = 0L;
                                                        if (dbObject.GetValue<DbObject>("chunks").Count > 0)
                                                        {
                                                            DbObject value4 = dbObject.GetValue<DbObject>("chunkMeta");
                                                            num18 = nwCas2.BaseStream.Position;
                                                            using (DbWriter dbWriter = new DbWriter(new MemoryStream()))
                                                            {
                                                                nwCas2.Write(dbWriter.WriteDbObject("chunkMeta", value4));
                                                            }
                                                            num19 = nwCas2.BaseStream.Position - num18;
                                                        }
                                                        long num20 = nwCas2.BaseStream.Position - 4;
                                                        nwCas2.BaseStream.Position = 24L;
                                                        nwCas2.Write((uint)(position3 - 4), Endian.Big);
                                                        nwCas2.Write((uint)(num18 - 4), Endian.Big);
                                                        nwCas2.Write((uint)num19, Endian.Big);
                                                        nwCas2.BaseStream.Position = 0L;
                                                        nwCas2.Write((uint)num20, Endian.Big);
                                                        if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + nwCas2.BaseStream.Length > 1073741824)
                                                        {
                                                            writer_new_cas_file?.Close();
                                                            writer_new_cas_file = GetNextCas(out casFileIndex);
                                                        }
                                                        bundleFileEntry.CasIndex = casFileIndex;
                                                        bundleFileEntry.Offset = (int)writer_new_cas_file.BaseStream.Position;
                                                        bundleFileEntry.Size = (int)(num20 + 4);
                                                        writer_new_cas_file.Write(((MemoryStream)nwCas2.BaseStream).ToArray());
                                                    }
                                                }
                                                list2.Add((int)(writer_new_toc_file_mod_data.BaseStream.Position - position));
                                                writer_new_toc_file_mod_data.Write((int)(writer_new_toc_file_mod_data.BaseStream.Position - position + lstBundleFiles3.Count * 3 * 4 + 5));
                                                for (int k = 0; k < lstBundleFiles3.Count; k++)
                                                {
                                                    uint num21 = (uint)lstBundleFiles3[k].CasIndex;
                                                    if (k != lstBundleFiles3.Count - 1)
                                                    {
                                                        num21 = (uint)((int)num21 | int.MinValue);
                                                    }
                                                    writer_new_toc_file_mod_data.Write(num21);
                                                    writer_new_toc_file_mod_data.Write(lstBundleFiles3[k].Offset);
                                                    writer_new_toc_file_mod_data.Write(lstBundleFiles3[k].Size);
                                                }
                                                writer_new_toc_file_mod_data.WriteNullTerminatedString(new string(text3.Reverse().ToArray()));
                                                writer_new_toc_file_mod_data.Write(0);
                                                int num22 = text3.Length + 5;
                                                for (int l = 0; l < 16 - num22 % 16; l++)
                                                {
                                                    writer_new_toc_file_mod_data.Write((byte)0);
                                                }
                                            }
                                            num4 = writer_new_toc_file_mod_data.BaseStream.Position - position;
                                            writer_new_toc_file_mod_data.Write(bundleCount);
                                            foreach (int item19 in list)
                                            {
                                                writer_new_toc_file_mod_data.Write(item19);
                                            }
                                            foreach (int item20 in list2)
                                            {
                                                writer_new_toc_file_mod_data.Write(item20);
                                            }
                                        }
                                        List<int> list5 = new List<int>();
                                        List<uint> list6 = new List<uint>();
                                        List<Guid> list7 = new List<Guid>();
                                        List<List<Tuple<Guid, int>>> list8 = new List<List<Tuple<Guid, int>>>();
                                        int num23 = 0;
                                        if (tocchunkposition != uint.MaxValue)
                                        {
                                            _ = writer_new_toc_file_mod_data.BaseStream.Position;
                                            _ = reader_of_original_toc_file_array.Position;
                                            reader_of_original_toc_file_array.Position = tocchunkposition - 12;
                                            num23 = reader_of_original_toc_file_array.ReadInt();
                                            for (int m = 0; m < num23; m++)
                                            {
                                                list5.Add(reader_of_original_toc_file_array.ReadInt());
                                                list8.Add(new List<Tuple<Guid, int>>());
                                            }
                                            for (int n = 0; n < num23; n++)
                                            {
                                                uint num24 = reader_of_original_toc_file_array.ReadUInt();
                                                long position4 = reader_of_original_toc_file_array.Position;
                                                reader_of_original_toc_file_array.Position = num24 - 12;
                                                Guid guid = reader_of_original_toc_file_array.ReadGuid();
                                                int num25 = reader_of_original_toc_file_array.ReadInt();
                                                int num26 = reader_of_original_toc_file_array.ReadInt();
                                                int num27 = reader_of_original_toc_file_array.ReadInt();
                                                reader_of_original_toc_file_array.Position = position4;
                                                list6.Add((uint)(writer_new_toc_file_mod_data.BaseStream.Position - position));
                                                if (parent.modifiedBundles.ContainsKey(chunksBundleHash))
                                                {
                                                    ModBundleInfo modBundleInfo2 = parent.modifiedBundles[chunksBundleHash];
                                                    int num28 = modBundleInfo2.Modify.Chunks.FindIndex((Guid g) => g == guid);
                                                    if (num28 != -1)
                                                    {
                                                        ChunkAssetEntry chunkAssetEntry3 = parent.ModifiedChunks[modBundleInfo2.Modify.Chunks[num28]];
                                                        byte[] outData = null;
                                                        if (chunkAssetEntry3.ExtraData != null)
                                                        {
                                                            HandlerExtraData handlerExtraData = (HandlerExtraData)chunkAssetEntry3.ExtraData;
                                                            Stream resourceData = parent.rm.GetResourceData(parent.fs.GetFilePath(num25), num26, num27);
                                                            chunkAssetEntry3 = (ChunkAssetEntry)handlerExtraData.Handler.Modify(chunkAssetEntry3, resourceData, handlerExtraData.Data, out outData);
                                                        }
                                                        else
                                                        {
                                                            outData = parent.archiveData[chunkAssetEntry3.Sha1].Data;
                                                        }
                                                        if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + outData.Length > 1073741824)
                                                        {
                                                            writer_new_cas_file?.Close();
                                                            writer_new_cas_file = GetNextCas(out casFileIndex);
                                                        }
                                                        num25 = casFileIndex;
                                                        num26 = (int)writer_new_cas_file.BaseStream.Position;
                                                        num27 = (int)chunkAssetEntry3.Size;
                                                        writer_new_cas_file.Write(outData);
                                                    }
                                                }
                                                writer_new_toc_file_mod_data.Write(guid);
                                                writer_new_toc_file_mod_data.Write(num25);
                                                writer_new_toc_file_mod_data.Write(num26);
                                                writer_new_toc_file_mod_data.Write(num27);
                                                list7.Add(guid);
                                            }
                                        }
                                        if (parent.modifiedBundles.ContainsKey(chunksBundleHash) && location_toc_file.ToLower().Contains("globals.toc"))
                                        {
                                            foreach (Guid chunk3 in parent.modifiedBundles[chunksBundleHash].Add.Chunks)
                                            {
                                                ChunkAssetEntry chunkAssetEntry4 = parent.ModifiedChunks[chunk3];
                                                if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + parent.archiveData[chunkAssetEntry4.Sha1].Data.Length > 1073741824)
                                                {
                                                    writer_new_cas_file?.Close();
                                                    writer_new_cas_file = GetNextCas(out casFileIndex);
                                                }
                                                int value5 = casFileIndex;
                                                int value6 = (int)writer_new_cas_file.BaseStream.Position;
                                                int value7 = (int)chunkAssetEntry4.Size;
                                                list6.Add((uint)(writer_new_toc_file_mod_data.BaseStream.Position - position));
                                                list8.Add(new List<Tuple<Guid, int>>());
                                                list5.Add(-1);
                                                num23++;
                                                list7.Add(chunk3);
                                                writer_new_cas_file.Write(parent.archiveData[chunkAssetEntry4.Sha1].Data);
                                                writer_new_toc_file_mod_data.Write(chunk3);
                                                writer_new_toc_file_mod_data.Write(value5);
                                                writer_new_toc_file_mod_data.Write(value6);
                                                writer_new_toc_file_mod_data.Write(value7);
                                            }
                                        }
                                        if (num23 > 0)
                                        {
                                            num5 = writer_new_toc_file_mod_data.BaseStream.Position - position;
                                            int num29 = 0;
                                            List<int> list9 = new List<int>();
                                            for (int num30 = 0; num30 < num23; num30++)
                                            {
                                                list9.Add(-1);
                                                list5[num30] = -1;
                                                int index = (int)((long)(uint)((int)HashData(list7[num30].ToByteArray()) % 16777619) % (long)num23);
                                                list8[index].Add(new Tuple<Guid, int>(list7[num30], (int)list6[num30]));
                                            }
                                            for (int num31 = 0; num31 < list8.Count; num31++)
                                            {
                                                List<Tuple<Guid, int>> list10 = list8[num31];
                                                if (list10.Count > 1)
                                                {
                                                    uint num32 = 1u;
                                                    List<int> list11 = new List<int>();
                                                    while (true)
                                                    {
                                                        bool flag = true;
                                                        for (int num33 = 0; num33 < list10.Count; num33++)
                                                        {
                                                            int num34 = (int)((long)(uint)((int)HashData(list10[num33].Item1.ToByteArray(), num32) % 16777619) % (long)num23);
                                                            if (list9[num34] != -1 || list11.Contains(num34))
                                                            {
                                                                flag = false;
                                                                break;
                                                            }
                                                            list11.Add(num34);
                                                        }
                                                        if (flag)
                                                        {
                                                            break;
                                                        }
                                                        num32++;
                                                        list11.Clear();
                                                    }
                                                    for (int num35 = 0; num35 < list10.Count; num35++)
                                                    {
                                                        list9[list11[num35]] = list10[num35].Item2;
                                                    }
                                                    list5[num31] = (int)num32;
                                                }
                                            }
                                            for (int num36 = 0; num36 < list8.Count; num36++)
                                            {
                                                if (list8[num36].Count == 1)
                                                {
                                                    for (; list9[num29] != -1; num29++)
                                                    {
                                                    }
                                                    list5[num36] = -1 - num29;
                                                    list9[num29] = list8[num36][0].Item2;
                                                }
                                            }
                                            writer_new_toc_file_mod_data.Write(num23);
                                            for (int num37 = 0; num37 < num23; num37++)
                                            {
                                                writer_new_toc_file_mod_data.Write(list5[num37]);
                                            }
                                            for (int num38 = 0; num38 < num23; num38++)
                                            {
                                                writer_new_toc_file_mod_data.Write(list9[num38]);
                                            }
                                        }
                                        writer_new_toc_file_mod_data.BaseStream.Position = position + 4;
                                        writer_new_toc_file_mod_data.Write((int)num4);
                                        writer_new_toc_file_mod_data.Write((int)num5);
                                    }
                                }
                                else
                                {
                                    writer_new_toc_file_mod_data.Write(uint.MaxValue);
                                    writer_new_toc_file_mod_data.Write(uint.MaxValue);
                                }
                            }
                        }
                    }
                    if(writer_new_cas_file!=null)
                    {
                        writer_new_cas_file?.Close();
                    }
                }
                catch (Exception ex)
                {
                    Exception ex2 = errorException = ex;
                }
            }

            private NativeWriter GetNextCas(out int casFileIndex)
            {
                int num = 1;
                string text = parent.fs.BasePath + "ModData\\patch\\" + catalogInfo.Name + "\\cas_" + num.ToString("D2") + ".cas";
                while (File.Exists(text))
                {
                    num++;
                    text = parent.fs.BasePath + "ModData\\patch\\" + catalogInfo.Name + "\\cas_" + num.ToString("D2") + ".cas";
                }
                lock (locker)
                {
                    casFiles.Add(++CasFileCount, "/native_data/Patch/" + catalogInfo.Name + "/cas_" + num.ToString("D2") + ".cas");
                    casFileIndex = CasFileCount;
                }
                FileInfo fileInfo = new FileInfo(text);
                if (!Directory.Exists(fileInfo.DirectoryName))
                {
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                }
                return new NativeWriter(new FileStream(text, FileMode.Create));
            }

            private uint HashString(string strToHash, uint initial = 0u)
            {
                uint num = 2166136261u;
                if (initial != 0)
                {
                    num = initial;
                }
                for (int i = 0; i < strToHash.Length; i++)
                {
                    num = (strToHash[i] ^ (16777619 * num));
                }
                return num;
            }

            private static uint HashData(byte[] b, uint initial = 0u)
            {
                uint num = (uint)((sbyte)b[0] ^ 0x50C5D1F);
                int num2 = 1;
                if (initial != 0)
                {
                    num = initial;
                    num2 = 0;
                }
                for (int i = num2; i < b.Length; i++)
                {
                    num = (uint)((int)(sbyte)b[i] ^ (int)(16777619 * num));
                }
                return num;
            }

            public void ThreadPoolCallback(object threadContext)
            {
                Run();
                if (Interlocked.Decrement(ref parent.numTasks) == 0)
                {
                    doneEvent.Set();
                }
            }
        }
    //private class ManifestBundleAction
    //    {
    //        private static readonly object resourceLock = new object();

    //        private List<ModBundleInfo> bundles;

    //        private ManualResetEvent doneEvent;

    //        private ModExecutor parent;

    //        private Exception errorException;

    //        private List<Sha1> dataRefs = new List<Sha1>();

    //        private List<Sha1> bundleRefs = new List<Sha1>();

    //        private List<CasFileEntry> fileInfos = new List<CasFileEntry>();

    //        private List<byte[]> bundleBuffers = new List<byte[]>();

    //        public List<Sha1> DataRefs => dataRefs;

    //        public List<Sha1> BundleRefs => bundleRefs;

    //        public List<CasFileEntry> FileInfos => fileInfos;

    //        public List<byte[]> BundleBuffers => bundleBuffers;

    //        public bool HasErrored => errorException != null;

    //        public Exception Exception => errorException;

    //        public ManifestBundleAction(List<ModBundleInfo> inBundles, ManualResetEvent inDoneEvent, ModExecutor inParent)
    //        {
    //            bundles = inBundles;
    //            doneEvent = inDoneEvent;
    //            parent = inParent;
    //        }

    //        public void Run()
    //        {
    //            try
    //            {
    //                FileSystem fs = parent.fs;
    //                foreach (ModBundleInfo bundle in bundles)
    //                {
    //                    ManifestBundleInfo manifestBundle = fs.GetManifestBundle(bundle.Name);
    //                    ManifestFileInfo manifestFileInfo = null;
    //                    DbObject dbObject = null;
    //                    if (manifestBundle.files.Count == 0)
    //                    {
    //                        manifestFileInfo = new ManifestFileInfo
    //                        {
    //                            file = new ManifestFileRef(1, inPatch: false, inCasIndex: 0)
    //                        };
    //                        manifestBundle.files.Add(manifestFileInfo);
    //                        dbObject = new DbObject();
    //                        dbObject.SetValue("ebx", DbObject.CreateList());
    //                        dbObject.SetValue("res", DbObject.CreateList());
    //                        dbObject.SetValue("chunks", DbObject.CreateList());
    //                        dbObject.SetValue("chunkMeta", DbObject.CreateList());
    //                    }
    //                    else
    //                    {
    //                        manifestFileInfo = manifestBundle.files[0];
    //                        fs.GetCatalog(manifestFileInfo.file);
    //                        List<ManifestFileInfo> list = new List<ManifestFileInfo>();
    //                        for (int i = 1; i < manifestBundle.files.Count; i++)
    //                        {
    //                            ManifestFileInfo manifestFileInfo2 = manifestBundle.files[i];
    //                            int key = Fnv1.HashString(fs.ResolvePath((manifestFileInfo2.file.IsInPatch ? "native_patch/" : "native_data/") + fs.GetCatalog(manifestFileInfo2.file) + "/cas.cat").ToLower());
    //                            Dictionary<uint, CatResourceEntry> dictionary = parent.resources[key][manifestFileInfo2.file.CasIndex];
    //                            List<uint> list2 = dictionary.Keys.ToList();
    //                            uint num = 0u;
    //                            uint num2 = manifestFileInfo2.offset;
    //                            list.Add(manifestFileInfo2);
    //                            if (!dictionary.ContainsKey(num2))
    //                            {
    //                                num2 = (uint)((int)num2 + (int)manifestFileInfo2.size);
    //                                int num3 = (!dictionary.ContainsKey(num2)) ? dictionary.Count : list2.BinarySearch(num2);
    //                                while (num2 > manifestFileInfo2.offset)
    //                                {
    //                                    num3--;
    //                                    num2 = list2[num3];
    //                                }
    //                                manifestFileInfo2.size += manifestFileInfo2.offset - num2;
    //                            }
    //                            CatResourceEntry catResourceEntry = dictionary[num2];
    //                            num += catResourceEntry.Size;
    //                            num2 += catResourceEntry.Size;
    //                            long size = manifestFileInfo2.size;
    //                            manifestFileInfo2.size = num2 - manifestFileInfo2.offset;
    //                            while (num != size)
    //                            {
    //                                CatResourceEntry catResourceEntry2 = dictionary[num2];
    //                                ManifestFileInfo manifestFileInfo3 = new ManifestFileInfo();
    //                                manifestFileInfo3.file = new ManifestFileRef(manifestFileInfo2.file.CatalogIndex, manifestFileInfo2.file.IsInPatch, manifestFileInfo2.file.CasIndex);
    //                                manifestFileInfo3.offset = catResourceEntry2.Offset;
    //                                manifestFileInfo3.size = catResourceEntry2.Size;
    //                                list.Add(manifestFileInfo3);
    //                                num += catResourceEntry2.Size;
    //                                num2 += catResourceEntry2.Size;
    //                            }
    //                        }
    //                        manifestBundle.files.Clear();
    //                        manifestBundle.files.Add(manifestFileInfo);
    //                        manifestBundle.files.AddRange(list);
    //                        using (NativeReader nativeReader = new NativeReader(new FileStream(fs.ResolvePath(manifestFileInfo.file), FileMode.Open, FileAccess.Read)))
    //                        {
    //                            using (BinarySbReader binarySbReader = new BinarySbReader(nativeReader.CreateViewStream(manifestFileInfo.offset, manifestFileInfo.size), 0L, null))
    //                            {
    //                                dbObject = binarySbReader.ReadDbObject();
    //                            }
    //                        }
    //                    }
    //                    int num4 = 1;
    //                    foreach (DbObject item2 in dbObject.GetValue<DbObject>("ebx"))
    //                    {
    //                        string value = item2.GetValue<string>("name");
    //                        if (bundle.Modify.Ebx.Contains(value))
    //                        {
    //                            ManifestFileInfo fileInfo = manifestBundle.files[num4];
    //                            EbxAssetEntry ebxAssetEntry = parent.modifiedEbx[value];
    //                            item2.SetValue("sha1", ebxAssetEntry.Sha1);
    //                            item2.SetValue("originalSize", ebxAssetEntry.OriginalSize);
    //                            dataRefs.Add(ebxAssetEntry.Sha1);
    //                            fileInfos.Add(new CasFileEntry
    //                            {
    //                                Entry = null,
    //                                FileInfo = fileInfo
    //                            });
    //                        }
    //                        num4++;
    //                    }
    //                    foreach (string item3 in bundle.Add.Ebx)
    //                    {
    //                        EbxAssetEntry ebxAssetEntry2 = parent.modifiedEbx[item3];
    //                        DbObject dbObject3 = new DbObject();
    //                        dbObject3.SetValue("name", ebxAssetEntry2.Name);
    //                        dbObject3.SetValue("sha1", ebxAssetEntry2.Sha1);
    //                        dbObject3.SetValue("originalSize", ebxAssetEntry2.OriginalSize);
    //                        dbObject.GetValue<DbObject>("ebx").Add(dbObject3);
    //                        ManifestFileInfo manifestFileInfo4 = new ManifestFileInfo();
    //                        manifestFileInfo4.file = new ManifestFileRef(manifestFileInfo.file.CatalogIndex, inPatch: false, inCasIndex: 0);
    //                        manifestBundle.files.Insert(num4++, manifestFileInfo4);
    //                        dataRefs.Add(ebxAssetEntry2.Sha1);
    //                        fileInfos.Add(new CasFileEntry
    //                        {
    //                            Entry = null,
    //                            FileInfo = manifestFileInfo4
    //                        });
    //                    }
    //                    foreach (DbObject item4 in dbObject.GetValue<DbObject>("res"))
    //                    {
    //                        string value2 = item4.GetValue<string>("name");
    //                        if (bundle.Modify.Res.Contains(value2))
    //                        {
    //                            ManifestFileInfo manifestFileInfo5 = manifestBundle.files[num4];
    //                            ResAssetEntry resAssetEntry = parent.modifiedRes[value2];
    //                            if (resAssetEntry.ExtraData != null)
    //                            {
    //                                lock (resourceLock)
    //                                {
    //                                    HandlerExtraData handlerExtraData = (HandlerExtraData)resAssetEntry.ExtraData;
    //                                    if (handlerExtraData != null)
    //                                    {
    //                                        byte[] outData = null;
    //                                        Stream resourceData = parent.rm.GetResourceData(parent.fs.GetFilePath(manifestFileInfo5.file.CatalogIndex, manifestFileInfo5.file.CasIndex, manifestFileInfo5.file.IsInPatch), manifestFileInfo5.offset, manifestFileInfo5.size);
    //                                        ResAssetEntry resAssetEntry2 = (ResAssetEntry)handlerExtraData.Handler.Modify(resAssetEntry, resourceData, handlerExtraData.Data, out outData);
    //                                        if (!parent.archiveData.ContainsKey(resAssetEntry2.Sha1))
    //                                        {
    //                                            parent.archiveData.TryAdd(resAssetEntry2.Sha1, new ArchiveInfo
    //                                            {
    //                                                Data = outData,
    //                                                RefCount = 1
    //                                            });
    //                                        }
    //                                        resAssetEntry.Sha1 = resAssetEntry2.Sha1;
    //                                        resAssetEntry.OriginalSize = resAssetEntry2.OriginalSize;
    //                                        resAssetEntry.ResMeta = resAssetEntry2.ResMeta;
    //                                        resAssetEntry.ExtraData = null;
    //                                    }
    //                                }
    //                            }
    //                            item4.SetValue("sha1", resAssetEntry.Sha1);
    //                            item4.SetValue("originalSize", resAssetEntry.OriginalSize);
    //                            if (resAssetEntry.ResMeta != null)
    //                            {
    //                                item4.SetValue("resMeta", resAssetEntry.ResMeta);
    //                            }
    //                            dataRefs.Add(resAssetEntry.Sha1);
    //                            fileInfos.Add(new CasFileEntry
    //                            {
    //                                Entry = null,
    //                                FileInfo = manifestFileInfo5
    //                            });
    //                        }
    //                        num4++;
    //                    }
    //                    foreach (string re in bundle.Add.Res)
    //                    {
    //                        ResAssetEntry resAssetEntry3 = parent.modifiedRes[re];
    //                        DbObject dbObject5 = new DbObject();
    //                        dbObject5.SetValue("name", resAssetEntry3.Name);
    //                        dbObject5.SetValue("sha1", resAssetEntry3.Sha1);
    //                        dbObject5.SetValue("originalSize", resAssetEntry3.OriginalSize);
    //                        dbObject5.SetValue("resRid", (long)resAssetEntry3.ResRid);
    //                        dbObject5.SetValue("resType", resAssetEntry3.ResType);
    //                        dbObject5.SetValue("resMeta", resAssetEntry3.ResMeta);
    //                        dbObject.GetValue<DbObject>("res").Add(dbObject5);
    //                        ManifestFileInfo manifestFileInfo6 = new ManifestFileInfo();
    //                        manifestFileInfo6.file = new ManifestFileRef(manifestFileInfo.file.CatalogIndex, inPatch: false, inCasIndex: 0);
    //                        manifestBundle.files.Insert(num4++, manifestFileInfo6);
    //                        dataRefs.Add(resAssetEntry3.Sha1);
    //                        fileInfos.Add(new CasFileEntry
    //                        {
    //                            Entry = null,
    //                            FileInfo = manifestFileInfo6
    //                        });
    //                    }
    //                    DbObject value3 = dbObject.GetValue<DbObject>("chunkMeta");
    //                    int num5 = 0;
    //                    List<int> list3 = new List<int>();
    //                    foreach (DbObject item5 in dbObject.GetValue<DbObject>("chunks"))
    //                    {
    //                        Guid value4 = item5.GetValue<Guid>("id");
    //                        if (bundle.Remove.Chunks.Contains(value4))
    //                        {
    //                            list3.Add(num5);
    //                        }
    //                        else if (bundle.Modify.Chunks.Contains(value4))
    //                        {
    //                            ChunkAssetEntry entry = parent.ModifiedChunks[value4];
    //                            DbObject dbObject7 = value3.Find<DbObject>((object a) => (a as DbObject).GetValue("h32", 0) == entry.H32);
    //                            item5.SetValue("sha1", entry.Sha1);
    //                            item5.SetValue("logicalOffset", entry.LogicalOffset);
    //                            item5.SetValue("logicalSize", entry.LogicalSize);
    //                            if (entry.FirstMip != -1)
    //                            {
    //                                item5.SetValue("rangeStart", entry.RangeStart);
    //                                item5.SetValue("rangeEnd", entry.RangeEnd);
    //                                dbObject7?.GetValue<DbObject>("meta").SetValue("firstMip", entry.FirstMip);
    //                            }
    //                            if (num4 < manifestBundle.files.Count)
    //                            {
    //                                dataRefs.Add(entry.Sha1);
    //                                ManifestFileInfo fileInfo2 = manifestBundle.files[num4];
    //                                fileInfos.Add(new CasFileEntry
    //                                {
    //                                    Entry = entry,
    //                                    FileInfo = fileInfo2
    //                                });
    //                            }
    //                        }
    //                        num4++;
    //                        num5++;
    //                    }
    //                    list3.Reverse();
    //                    foreach (int item6 in list3)
    //                    {
    //                        dbObject.GetValue<DbObject>("chunks").RemoveAt(item6);
    //                        dbObject.GetValue<DbObject>("chunkMeta").RemoveAt(item6);
    //                        manifestBundle.files.RemoveAt(item6 + num4);
    //                    }
    //                    foreach (Guid chunk in bundle.Add.Chunks)
    //                    {
    //                        ChunkAssetEntry chunkAssetEntry = parent.ModifiedChunks[chunk];
    //                        DbObject dbObject8 = new DbObject();
    //                        dbObject8.SetValue("id", chunk);
    //                        dbObject8.SetValue("sha1", chunkAssetEntry.Sha1);
    //                        dbObject8.SetValue("logicalOffset", chunkAssetEntry.LogicalOffset);
    //                        dbObject8.SetValue("logicalSize", chunkAssetEntry.LogicalSize);
    //                        DbObject dbObject9 = new DbObject();
    //                        dbObject9.SetValue("h32", chunkAssetEntry.H32);
    //                        dbObject9.SetValue("meta", new DbObject());
    //                        value3.Add(dbObject9);
    //                        if (chunkAssetEntry.FirstMip != -1)
    //                        {
    //                            dbObject8.SetValue("rangeStart", chunkAssetEntry.RangeStart);
    //                            dbObject8.SetValue("rangeEnd", chunkAssetEntry.RangeEnd);
    //                            dbObject9.GetValue<DbObject>("meta").SetValue("firstMip", chunkAssetEntry.FirstMip);
    //                        }
    //                        dbObject.GetValue<DbObject>("chunks").Add(dbObject8);
    //                        ManifestFileInfo manifestFileInfo7 = new ManifestFileInfo();
    //                        manifestFileInfo7.file = new ManifestFileRef(manifestFileInfo.file.CatalogIndex, inPatch: false, inCasIndex: 0);
    //                        manifestBundle.files.Insert(num4++, manifestFileInfo7);
    //                        dataRefs.Add(chunkAssetEntry.Sha1);
    //                        fileInfos.Add(new CasFileEntry
    //                        {
    //                            Entry = chunkAssetEntry,
    //                            FileInfo = manifestFileInfo7
    //                        });
    //                    }
    //                    MemoryStream memoryStream = new MemoryStream();
    //                    using (NativeWriter nativeWriter = new NativeWriter(memoryStream, leaveOpen: true))
    //                    {
    //                        nativeWriter.Write(3735927486u, Endian.Big);
    //                        long position = nativeWriter.BaseStream.Position;
    //                        nativeWriter.Write(2641989333u, Endian.Big);
    //                        nativeWriter.Write(dbObject.GetValue<DbObject>("ebx").Count + dbObject.GetValue<DbObject>("res").Count + dbObject.GetValue<DbObject>("chunks").Count, Endian.Big);
    //                        nativeWriter.Write(dbObject.GetValue<DbObject>("ebx").Count, Endian.Big);
    //                        nativeWriter.Write(dbObject.GetValue<DbObject>("res").Count, Endian.Big);
    //                        nativeWriter.Write(dbObject.GetValue<DbObject>("chunks").Count, Endian.Big);
    //                        nativeWriter.Write(3735927486u, Endian.Big);
    //                        nativeWriter.Write(3735927486u, Endian.Big);
    //                        nativeWriter.Write(3735927486u, Endian.Big);
    //                        foreach (DbObject item7 in dbObject.GetValue<DbObject>("ebx"))
    //                        {
    //                            nativeWriter.Write(item7.GetValue<Sha1>("sha1"));
    //                        }
    //                        foreach (DbObject item8 in dbObject.GetValue<DbObject>("res"))
    //                        {
    //                            nativeWriter.Write(item8.GetValue<Sha1>("sha1"));
    //                        }
    //                        foreach (DbObject item9 in dbObject.GetValue<DbObject>("chunks"))
    //                        {
    //                            nativeWriter.Write(item9.GetValue<Sha1>("sha1"));
    //                        }
    //                        long num6 = 0L;
    //                        Dictionary<uint, long> dictionary2 = new Dictionary<uint, long>();
    //                        List<string> list4 = new List<string>();
    //                        foreach (DbObject item10 in dbObject.GetValue<DbObject>("ebx"))
    //                        {
    //                            uint key2 = (uint)Fnv1.HashString(item10.GetValue<string>("name"));
    //                            if (!dictionary2.ContainsKey(key2))
    //                            {
    //                                list4.Add(item10.GetValue<string>("name"));
    //                                dictionary2.Add(key2, num6);
    //                                num6 += item10.GetValue<string>("name").Length + 1;
    //                            }
    //                            nativeWriter.Write((uint)dictionary2[key2], Endian.Big);
    //                            nativeWriter.Write(item10.GetValue("originalSize", 0), Endian.Big);
    //                        }
    //                        foreach (DbObject item11 in dbObject.GetValue<DbObject>("res"))
    //                        {
    //                            uint key3 = (uint)Fnv1.HashString(item11.GetValue<string>("name"));
    //                            if (!dictionary2.ContainsKey(key3))
    //                            {
    //                                list4.Add(item11.GetValue<string>("name"));
    //                                dictionary2.Add(key3, num6);
    //                                num6 += item11.GetValue<string>("name").Length + 1;
    //                            }
    //                            nativeWriter.Write((uint)dictionary2[key3], Endian.Big);
    //                            nativeWriter.Write(item11.GetValue("originalSize", 0), Endian.Big);
    //                        }
    //                        foreach (DbObject item12 in dbObject.GetValue<DbObject>("res"))
    //                        {
    //                            nativeWriter.Write(item12.GetValue("resType", 0), Endian.Big);
    //                        }
    //                        foreach (DbObject item13 in dbObject.GetValue<DbObject>("res"))
    //                        {
    //                            nativeWriter.Write(item13.GetValue<byte[]>("resMeta"));
    //                        }
    //                        foreach (DbObject item14 in dbObject.GetValue<DbObject>("res"))
    //                        {
    //                            nativeWriter.Write(item14.GetValue("resRid", 0L), Endian.Big);
    //                        }
    //                        foreach (DbObject item15 in dbObject.GetValue<DbObject>("chunks"))
    //                        {
    //                            nativeWriter.Write(item15.GetValue<Guid>("id"), Endian.Big);
    //                            nativeWriter.Write(item15.GetValue("logicalOffset", 0), Endian.Big);
    //                            nativeWriter.Write(item15.GetValue("logicalSize", 0), Endian.Big);
    //                        }
    //                        long num7 = 0L;
    //                        long num8 = 0L;
    //                        if (dbObject.GetValue<DbObject>("chunkMeta") != null && dbObject.GetValue<DbObject>("chunks").Count != 0)
    //                        {
    //                            num7 = nativeWriter.BaseStream.Position - position;
    //                            using (DbWriter dbWriter = new DbWriter(new MemoryStream()))
    //                            {
    //                                nativeWriter.Write(dbWriter.WriteDbObject("chunkMeta", dbObject.GetValue<DbObject>("chunkMeta")));
    //                            }
    //                            num8 = nativeWriter.BaseStream.Position - position - num7;
    //                        }
    //                        long num9 = nativeWriter.BaseStream.Position - position;
    //                        foreach (string item16 in list4)
    //                        {
    //                            nativeWriter.WriteNullTerminatedString(item16);
    //                        }
    //                        while ((nativeWriter.BaseStream.Position - (position - 4)) % 16 != 0L)
    //                        {
    //                            nativeWriter.Write((byte)0);
    //                        }
    //                        long position2 = nativeWriter.BaseStream.Position;
    //                        nativeWriter.BaseStream.Position = position + 20;
    //                        nativeWriter.Write((uint)num9, Endian.Big);
    //                        nativeWriter.Write((uint)num7, Endian.Big);
    //                        nativeWriter.Write((uint)num8, Endian.Big);
    //                        nativeWriter.BaseStream.Position = position - 4;
    //                        nativeWriter.Write((uint)(position2 - 4), Endian.Big);
    //                    }
    //                    byte[] array = memoryStream.ToArray();
    //                    Sha1 item = Utils.GenerateSha1(array);
    //                    bundleRefs.Add(item);
    //                    dataRefs.Add(item);
    //                    fileInfos.Add(new CasFileEntry
    //                    {
    //                        Entry = null,
    //                        FileInfo = manifestFileInfo
    //                    });
    //                    bundleBuffers.Add(array);
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                Exception ex2 = errorException = ex;
    //            }
    //        }

    //        public void ThreadPoolCallback(object threadContext)
    //        {
    //            Run();
    //            if (Interlocked.Decrement(ref parent.numTasks) == 0)
    //            {
    //                doneEvent.Set();
    //            }
    //        }
    //    }

        private class SuperBundleAction
        {
            internal class BaseBundleInfo
            {
                public string Name;

                public long Offset;

                public long Size;
            }

            internal class AssetInfo
            {
                public string Name;

                public int NameHash;

                public Guid Id;

                public bool Modified;

                public bool Inserted;

                public bool Removed;

                public DbObject Asset;

                public DbObject BaseAsset;

                public DbObject Meta;
            }

            public bool TocModified;

            public bool SbModified;

            private string superBundle;

            private ManualResetEvent doneEvent;

            private ModExecutor parent;

            private List<Sha1> casRefs = new List<Sha1>();

            private List<ChunkAssetEntry> chunkEntries = new List<ChunkAssetEntry>();

            private Exception errorException;

            private string modPath;

            public string SuperBundle => superBundle;

            public List<Sha1> CasRefs => casRefs;

            public List<ChunkAssetEntry> ChunkEntries => chunkEntries;

            public bool HasErrored => errorException != null;

            public Exception Exception => errorException;

            public SuperBundleAction(string inSuperBundle, ManualResetEvent inDoneEvent, ModExecutor inParent, string inModPath)
            {
                superBundle = inSuperBundle;
                doneEvent = inDoneEvent;
                parent = inParent;
                modPath = inModPath;
            }

            public void Run()
            {
                string text = parent.fs.ResolvePath(superBundle + ".toc");
                try
                {
                    new List<Sha1>();
                    DbObject dbObject = null;
                    bool flag = false;
                    bool flag2 = false;
                    bool flag3 = false;
                    string text2 = parent.fs.ResolvePath("native_data/" + superBundle + ".toc");
                    if (text2.Equals(text))
                    {
                        flag3 = true;
                    }
                    if (ProfileManager.DataVersion != 20141118 && ProfileManager.DataVersion != 20141117 && ProfileManager.DataVersion != 20151103 && ProfileManager.DataVersion != 20150223 && ProfileManager.DataVersion != 20131115)
                    {
                        goto IL_0105;
                    }
                    if (!(text2 == ""))
                    {
                        using (DbReader dbReader = new DbReader(new FileStream(text2, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
                        {
                            dbObject = dbReader.ReadDbObject();
                        }
                        if (dbObject.GetValue("alwaysEmitSuperBundle", defaultValue: false) || ProfileManager.DataVersion == 20150223)
                        {
                            flag2 = true;
                        }
                        goto IL_0105;
                    }
                    goto end_IL_0021;
                IL_0105:
                    if (!(text != ""))
                    {
                        dbObject = new DbObject();
                        dbObject.SetValue("bundles", new DbObject(bObject: false));
                        dbObject.SetValue("chunks", new DbObject(bObject: false));
                        dbObject.SetValue("cas", true);
                        dbObject.SetValue("name", superBundle);
                        dbObject.SetValue("alwaysEmitSuperbundle", false);
                        flag = true;
                        goto IL_01c1;
                    }
                    if (File.Exists(text.Replace(".toc", ".sb")))
                    {
                        using (DbReader dbReader2 = new DbReader(new FileStream(text, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
                        {
                            dbObject = dbReader2.ReadDbObject();
                        }
                        goto IL_01c1;
                    }
                    goto end_IL_0021;
                IL_01c1:
                    bool flag4 = false;
                    bool flag5 = false;
                    if (!dbObject.HasValue("chunks") || !parent.modifiedBundles.ContainsKey(chunksBundleHash))
                    {
                        goto IL_0d26;
                    }
                    FileInfo fileInfo = new FileInfo(parent.fs.BasePath + "/" + modPath + "/" + superBundle + ".sb");
                    ModBundleInfo modBundleInfo = parent.modifiedBundles[chunksBundleHash];
                    if (!flag2)
                    {
                        if (ProfileManager.DataVersion == 20141118 || ProfileManager.DataVersion == 20141117 || ProfileManager.DataVersion == 20151103 || ProfileManager.DataVersion == 20131115)
                        {
                            if (flag3)
                            {
                                dbObject = new DbObject();
                                dbObject.AddValue("bundles", new DbObject(bObject: false));
                                dbObject.AddValue("chunks", new DbObject(bObject: false));
                                dbObject.AddValue("cas", true);
                            }
                            DbObject dbObject2 = null;
                            using (DbReader dbReader3 = new DbReader(new FileStream(text2, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
                            {
                                dbObject2 = dbReader3.ReadDbObject();
                            }
                            DbObject value = dbObject.GetValue<DbObject>("chunks");
                            foreach (DbObject item in dbObject2.GetValue<DbObject>("chunks"))
                            {
                                Guid value2 = item.GetValue<Guid>("id");
                                if (modBundleInfo.Modify.Chunks.Contains(value2))
                                {
                                    DbObject dbObject4 = item;
                                    bool flag6 = false;
                                    foreach (DbObject item2 in dbObject.GetValue<DbObject>("chunks"))
                                    {
                                        if (item2.GetValue<Guid>("id") == value2)
                                        {
                                            dbObject4 = item2;
                                            flag6 = true;
                                            break;
                                        }
                                    }
                                    if (!flag6)
                                    {
                                        value.Insert(0, dbObject4);
                                    }
                                    ChunkAssetEntry chunkAssetEntry = parent.ModifiedChunks[value2];
                                    dbObject4.SetValue("sha1", chunkAssetEntry.Sha1);
                                    dbObject4.SetValue("delta", true);
                                    if (chunkAssetEntry.IsTocChunk && !casRefs.Contains(chunkAssetEntry.Sha1))
                                    {
                                        casRefs.Add(chunkAssetEntry.Sha1);
                                        chunkEntries.Add(chunkAssetEntry);
                                    }
                                }
                                flag4 = true;
                            }
                            if (superBundle.Contains("chunks"))
                            {
                                foreach (Guid chunk2 in modBundleInfo.Add.Chunks)
                                {
                                    ChunkAssetEntry chunkAssetEntry2 = parent.ModifiedChunks[chunk2];
                                    DbObject dbObject6 = new DbObject();
                                    dbObject6.SetValue("id", chunkAssetEntry2.Id);
                                    dbObject6.SetValue("sha1", chunkAssetEntry2.Sha1);
                                    value.Add(dbObject6);
                                    if (chunkAssetEntry2.IsTocChunk && !casRefs.Contains(chunkAssetEntry2.Sha1))
                                    {
                                        casRefs.Add(chunkAssetEntry2.Sha1);
                                        chunkEntries.Add(chunkAssetEntry2);
                                    }
                                    flag4 = true;
                                }
                            }
                        }
                        else
                        {
                            foreach (DbObject item3 in dbObject.GetValue<DbObject>("chunks"))
                            {
                                Guid value3 = item3.GetValue<Guid>("id");
                                if (modBundleInfo.Modify.Chunks.Contains(value3))
                                {
                                    ChunkAssetEntry chunkAssetEntry3 = parent.ModifiedChunks[value3];
                                    if (chunkAssetEntry3.ExtraData != null)
                                    {
                                        HandlerExtraData handlerExtraData = (HandlerExtraData)chunkAssetEntry3.ExtraData;
                                        Stream resourceData = parent.rm.GetResourceData(item3.GetValue<Sha1>("sha1"));
                                        byte[] outData = null;
                                        chunkAssetEntry3 = (ChunkAssetEntry)handlerExtraData.Handler.Modify(chunkAssetEntry3, resourceData, handlerExtraData.Data, out outData);
                                        parent.archiveData.TryAdd(chunkAssetEntry3.Sha1, new ArchiveInfo
                                        {
                                            Data = outData,
                                            RefCount = 1
                                        });
                                    }
                                    item3.SetValue("sha1", chunkAssetEntry3.Sha1);
                                    if (chunkAssetEntry3.IsTocChunk && !casRefs.Contains(chunkAssetEntry3.Sha1))
                                    {
                                        casRefs.Add(chunkAssetEntry3.Sha1);
                                        chunkEntries.Add(chunkAssetEntry3);
                                    }
                                    flag4 = true;
                                }
                            }
                            if (superBundle.Contains("chunks"))
                            {
                                foreach (Guid chunk3 in modBundleInfo.Add.Chunks)
                                {
                                    ChunkAssetEntry chunkAssetEntry4 = parent.ModifiedChunks[chunk3];
                                    DbObject dbObject8 = new DbObject();
                                    dbObject8.SetValue("id", chunkAssetEntry4.Id);
                                    dbObject8.SetValue("sha1", chunkAssetEntry4.Sha1);
                                    dbObject.GetValue<DbObject>("chunks").Add(dbObject8);
                                    if (chunkAssetEntry4.IsTocChunk && !casRefs.Contains(chunkAssetEntry4.Sha1))
                                    {
                                        casRefs.Add(chunkAssetEntry4.Sha1);
                                        chunkEntries.Add(chunkAssetEntry4);
                                    }
                                    flag4 = true;
                                }
                            }
                        }
                        goto IL_0d26;
                    }
                    byte[] array = null;
                    if (flag3)
                    {
                        dbObject = new DbObject();
                        dbObject.AddValue("bundles", new DbObject(bObject: false));
                        dbObject.AddValue("chunks", new DbObject(bObject: false));
                        array = new byte[0];
                    }
                    else
                    {
                        using (NativeReader nativeReader = new NativeReader(new FileStream(text.Replace(".toc", ".sb"), FileMode.Open, FileAccess.Read)))
                        {
                            array = nativeReader.ReadToEnd();
                        }
                    }
                    bool flag7 = false;
                    DbObject dbObject9 = null;
                    using (DbReader dbReader4 = new DbReader(new FileStream(text2, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
                    {
                        dbObject9 = dbReader4.ReadDbObject();
                    }
                    DbObject dbObject10 = new DbObject(bObject: false);
                    foreach (DbObject item4 in dbObject9.GetValue<DbObject>("chunks"))
                    {
                        dbObject10.Add(item4);
                    }
                    foreach (DbObject item5 in dbObject.GetValue<DbObject>("chunks"))
                    {
                        Guid chunkId = item5.GetValue<Guid>("id");
                        int num = dbObject10.FindIndex((object o) => ((DbObject)o).GetValue<Guid>("id") == chunkId);
                        if (num != -1)
                        {
                            item5.SetValue("modified", true);
                            dbObject10.SetAt(num, item5);
                        }
                        else
                        {
                            item5.SetValue("modified", true);
                            dbObject10.Add(item5);
                        }
                    }
                    foreach (DbObject item6 in dbObject10)
                    {
                        Guid value5 = item6.GetValue<Guid>("id");
                        if (modBundleInfo.Modify.Chunks.Contains(value5))
                        {
                            flag7 = true;
                            break;
                        }
                        if (modBundleInfo.Add.Chunks.Count > 0)
                        {
                            flag7 = true;
                            break;
                        }
                    }
                    if (!flag7)
                    {
                        goto IL_0d26;
                    }
                    if (!Directory.Exists(fileInfo.DirectoryName))
                    {
                        Directory.CreateDirectory(fileInfo.DirectoryName);
                    }
                    using (NativeWriter nativeWriter = new NativeWriter(new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write)))
                    {
                        long num2 = array.Length;
                        nativeWriter.Write(array);
                        dbObject.SetValue("chunks", new DbObject(bObject: false));
                        foreach (DbObject item7 in dbObject10)
                        {
                            Guid value6 = item7.GetValue<Guid>("id");
                            if (modBundleInfo.Modify.Chunks.Contains(value6))
                            {
                                flag7 = true;
                                ChunkAssetEntry chunkAssetEntry5 = parent.ModifiedChunks[value6];
                                item7.RemoveValue("modified");
                                item7.SetValue("sha1", chunkAssetEntry5.Sha1);
                                item7.SetValue("size", chunkAssetEntry5.Size);
                                item7.SetValue("offset", num2);
                                dbObject.GetValue<DbObject>("chunks").Add(item7);
                                num2 += chunkAssetEntry5.Size;
                                nativeWriter.Write(parent.archiveData[chunkAssetEntry5.Sha1].Data);
                            }
                            else if (item7.GetValue("modified", defaultValue: false))
                            {
                                item7.RemoveValue("modified");
                                dbObject.GetValue<DbObject>("chunks").Add(item7);
                            }
                        }
                        if (superBundle.Contains("chunks"))
                        {
                            foreach (Guid chunk4 in modBundleInfo.Add.Chunks)
                            {
                                flag7 = true;
                                ChunkAssetEntry chunkAssetEntry6 = parent.ModifiedChunks[chunk4];
                                DbObject dbObject13 = new DbObject();
                                dbObject13.SetValue("id", chunkAssetEntry6.Id);
                                dbObject13.SetValue("sha1", chunkAssetEntry6.Sha1);
                                dbObject13.SetValue("size", chunkAssetEntry6.Size);
                                dbObject13.SetValue("offset", num2);
                                dbObject.GetValue<DbObject>("chunks").Add(dbObject13);
                                num2 += chunkAssetEntry6.Size;
                                nativeWriter.Write(parent.archiveData[chunkAssetEntry6.Sha1].Data);
                            }
                        }
                        using (DbWriter dbWriter = new DbWriter(new FileStream(fileInfo.FullName.Replace(".sb", ".toc"), FileMode.Create), inWriteHeader: true))
                        {
                            dbWriter.Write(dbObject);
                        }
                        TocModified = true;
                        SbModified = true;
                    }
                    goto end_IL_0021;
                IL_0d26:
                    foreach (DbObject item8 in dbObject.GetValue<DbObject>("bundles"))
                    {
                        int key = Fnv1.HashString(item8.GetValue<string>("id").ToLower());
                        if (parent.modifiedBundles.ContainsKey(key))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag && flag3)
                    {
                        TocModified = true;
                        SbModified = true;
                    }
                    else
                    {
                        if (flag)
                        {
                            if (flag2)
                            {
                                Dictionary<int, BaseBundleInfo> dictionary = new Dictionary<int, BaseBundleInfo>();
                                NativeReader nativeReader2 = new NativeReader(new FileStream(parent.fs.ResolvePath("native_data/" + superBundle + ".sb"), FileMode.Open, FileAccess.Read));
                                NativeReader nativeReader3 = null;
                                DbObject dbObject14 = null;
                                using (DbReader dbReader5 = new DbReader(new FileStream(text2, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
                                {
                                    dbObject14 = dbReader5.ReadDbObject();
                                }
                                foreach (DbObject item9 in dbObject14.GetValue<DbObject>("bundles"))
                                {
                                    BaseBundleInfo baseBundleInfo = new BaseBundleInfo();
                                    baseBundleInfo.Name = item9.GetValue<string>("id");
                                    baseBundleInfo.Offset = item9.GetValue("offset", 0L);
                                    baseBundleInfo.Size = item9.GetValue("size", 0L);
                                    dictionary.Add(Fnv1.HashString(baseBundleInfo.Name.ToLower()), baseBundleInfo);
                                }
                                if (!flag3)
                                {
                                    nativeReader3 = new NativeReader(new FileStream(parent.fs.ResolvePath("native_patch/" + superBundle + ".sb"), FileMode.Open, FileAccess.Read));
                                }
                                Directory.CreateDirectory(new FileInfo(parent.fs.BasePath + "/" + modPath + "/" + superBundle + ".sb").DirectoryName);
                                long num3 = 0L;
                                foreach (DbObject item10 in dbObject.GetValue<DbObject>("bundles"))
                                {
                                    int key2 = Fnv1.HashString(item10.GetValue<string>("id").ToLower());
                                    List<AssetInfo> list = new List<AssetInfo>();
                                    List<AssetInfo> list2 = new List<AssetInfo>();
                                    List<AssetInfo> list3 = new List<AssetInfo>();
                                    List<AssetInfo> list4 = new List<AssetInfo>();
                                    bool value7 = item10.GetValue("delta", defaultValue: false);
                                    long num4 = 0L;
                                    bool flag8 = false;
                                    if (value7)
                                    {
                                        if (parent.modifiedBundles.ContainsKey(key2))
                                        {
                                            flag8 = true;
                                            BaseBundleInfo baseBundleInfo2 = null;
                                            Stream stream = null;
                                            DbObject dbObject17 = null;
                                            if (dictionary.ContainsKey(key2))
                                            {
                                                baseBundleInfo2 = dictionary[key2];
                                                stream = nativeReader2.CreateViewStream(baseBundleInfo2.Offset, baseBundleInfo2.Size);
                                                using (BinarySbReader binarySbReader = new BinarySbReader(stream, 0L, parent.fs.CreateDeobfuscator()))
                                                {
                                                    dbObject17 = binarySbReader.ReadDbObject();
                                                }
                                                num4 = dbObject17.GetValue("dataOffset", 0L);
                                                stream = nativeReader2.CreateViewStream(baseBundleInfo2.Offset, baseBundleInfo2.Size);
                                            }
                                            Stream stream2 = nativeReader3.CreateViewStream(item10.GetValue("offset", 0L), item10.GetValue("size", 0L));
                                            DbObject dbObject18 = null;
                                            using (BinarySbReader binarySbReader2 = new BinarySbReader(stream, stream2, parent.fs.CreateDeobfuscator()))
                                            {
                                                dbObject18 = binarySbReader2.ReadDbObject();
                                            }
                                            stream?.Dispose();
                                            stream2.Dispose();
                                            if (dbObject17 != null)
                                            {
                                                foreach (DbObject item11 in dbObject17.GetValue<DbObject>("ebx"))
                                                {
                                                    bool flag9 = false;
                                                    bool modified = false;
                                                    DbObject dbObject20 = null;
                                                    int num5 = Fnv1.HashString(item11.GetValue<string>("name"));
                                                    foreach (DbObject item12 in dbObject18.GetValue<DbObject>("ebx"))
                                                    {
                                                        if (num5 == item12.GetValue("nameHash", 0))
                                                        {
                                                            flag9 = true;
                                                            modified = (item11.GetValue<Sha1>("sha1") != item12.GetValue<Sha1>("sha1"));
                                                            dbObject20 = item12;
                                                            break;
                                                        }
                                                    }
                                                    AssetInfo assetInfo = new AssetInfo();
                                                    assetInfo.Name = item11.GetValue<string>("name");
                                                    assetInfo.NameHash = Fnv1.HashString(assetInfo.Name);
                                                    assetInfo.Removed = !flag9;
                                                    assetInfo.Modified = modified;
                                                    assetInfo.Asset = (flag9 ? dbObject20 : item11);
                                                    assetInfo.BaseAsset = item11;
                                                    list2.Add(assetInfo);
                                                }
                                            }
                                            foreach (DbObject item13 in dbObject18.GetValue<DbObject>("ebx"))
                                            {
                                                int hash2 = Fnv1.HashString(item13.GetValue<string>("name"));
                                                if (list2.FindIndex((AssetInfo a) => a.NameHash == hash2) == -1)
                                                {
                                                    AssetInfo assetInfo2 = new AssetInfo();
                                                    assetInfo2.Name = item13.GetValue<string>("name");
                                                    assetInfo2.NameHash = Fnv1.HashString(assetInfo2.Name);
                                                    assetInfo2.Inserted = true;
                                                    assetInfo2.Asset = item13;
                                                    list2.Add(assetInfo2);
                                                }
                                            }
                                            if (dbObject17 != null)
                                            {
                                                foreach (DbObject item14 in dbObject17.GetValue<DbObject>("res"))
                                                {
                                                    bool flag10 = false;
                                                    bool modified2 = false;
                                                    DbObject dbObject24 = null;
                                                    int num6 = Fnv1.HashString(item14.GetValue<string>("name"));
                                                    foreach (DbObject item15 in dbObject18.GetValue<DbObject>("res"))
                                                    {
                                                        if (num6 == item15.GetValue("nameHash", 0))
                                                        {
                                                            flag10 = true;
                                                            modified2 = (item14.GetValue<Sha1>("sha1") != item15.GetValue<Sha1>("sha1"));
                                                            dbObject24 = item15;
                                                            break;
                                                        }
                                                    }
                                                    AssetInfo assetInfo3 = new AssetInfo();
                                                    assetInfo3.Name = item14.GetValue<string>("name");
                                                    assetInfo3.NameHash = Fnv1.HashString(assetInfo3.Name);
                                                    assetInfo3.Removed = !flag10;
                                                    assetInfo3.Modified = modified2;
                                                    assetInfo3.Asset = (flag10 ? dbObject24 : item14);
                                                    assetInfo3.BaseAsset = item14;
                                                    list3.Add(assetInfo3);
                                                }
                                            }
                                            foreach (DbObject item16 in dbObject18.GetValue<DbObject>("res"))
                                            {
                                                int hash = Fnv1.HashString(item16.GetValue<string>("name"));
                                                if (list3.FindIndex((AssetInfo a) => a.NameHash == hash) == -1)
                                                {
                                                    AssetInfo assetInfo4 = new AssetInfo();
                                                    assetInfo4.Name = item16.GetValue<string>("name");
                                                    assetInfo4.NameHash = Fnv1.HashString(assetInfo4.Name);
                                                    assetInfo4.Inserted = true;
                                                    assetInfo4.Asset = item16;
                                                    list3.Add(assetInfo4);
                                                }
                                            }
                                            int num7 = 0;
                                            if (dbObject17 != null)
                                            {
                                                foreach (DbObject item17 in dbObject17.GetValue<DbObject>("chunks"))
                                                {
                                                    bool flag11 = false;
                                                    bool modified3 = false;
                                                    DbObject dbObject28 = null;
                                                    DbObject meta = (DbObject)dbObject17.GetValue<DbObject>("chunkMeta")[num7];
                                                    int num8 = 0;
                                                    foreach (DbObject item18 in dbObject18.GetValue<DbObject>("chunks"))
                                                    {
                                                        if (item17.GetValue<Guid>("id") == item18.GetValue<Guid>("id"))
                                                        {
                                                            flag11 = true;
                                                            modified3 = (item17.GetValue<Sha1>("sha1") != item18.GetValue<Sha1>("sha1"));
                                                            dbObject28 = item18;
                                                            meta = (DbObject)dbObject18.GetValue<DbObject>("chunkMeta")[num8];
                                                            break;
                                                        }
                                                        num8++;
                                                    }
                                                    AssetInfo assetInfo5 = new AssetInfo();
                                                    assetInfo5.Id = item17.GetValue<Guid>("id");
                                                    assetInfo5.Removed = !flag11;
                                                    assetInfo5.Modified = modified3;
                                                    assetInfo5.Asset = (flag11 ? dbObject28 : item17);
                                                    assetInfo5.BaseAsset = item17;
                                                    assetInfo5.Meta = meta;
                                                    list4.Add(assetInfo5);
                                                    num7++;
                                                }
                                            }
                                            num7 = 0;
                                            foreach (DbObject chunk in dbObject18.GetValue<DbObject>("chunks"))
                                            {
                                                if (list4.FindIndex((AssetInfo a) => a.Id == chunk.GetValue<Guid>("id")) == -1)
                                                {
                                                    AssetInfo assetInfo6 = new AssetInfo();
                                                    assetInfo6.Id = chunk.GetValue<Guid>("id");
                                                    assetInfo6.Inserted = true;
                                                    assetInfo6.Asset = chunk;
                                                    assetInfo6.Meta = (DbObject)dbObject18.GetValue<DbObject>("chunkMeta")[num7];
                                                    list4.Add(assetInfo6);
                                                }
                                                num7++;
                                            }
                                        }
                                        else
                                        {
                                            nativeReader3.Position = item10.GetValue("offset", 0L);
                                            using (NativeWriter nativeWriter2 = new NativeWriter(new FileStream(parent.fs.BasePath + "/" + modPath + "/" + superBundle + ".sb", FileMode.Append, FileAccess.Write)))
                                            {
                                                byte[] array2 = new byte[1048576];
                                                long num9 = item10.GetValue("size", 0L);
                                                while (num9 > 0)
                                                {
                                                    int num10 = (int)((num9 > 1048576) ? 1048576 : num9);
                                                    nativeReader3.Read(array2, 0, num10);
                                                    nativeWriter2.Write(array2, 0, num10);
                                                    num9 -= num10;
                                                }
                                            }
                                            item10.SetValue("offset", num3);
                                            num3 += item10.GetValue("size", 0);
                                        }
                                    }
                                    else if (parent.modifiedBundles.ContainsKey(key2))
                                    {
                                        flag8 = true;
                                        BaseBundleInfo baseBundleInfo3 = dictionary[key2];
                                        Stream stream3 = nativeReader2.CreateViewStream(baseBundleInfo3.Offset, baseBundleInfo3.Size);
                                        DbObject dbObject30 = null;
                                        using (BinarySbReader binarySbReader3 = new BinarySbReader(stream3, 0L, parent.fs.CreateDeobfuscator()))
                                        {
                                            dbObject30 = binarySbReader3.ReadDbObject();
                                        }
                                        num4 = dbObject30.GetValue("dataOffset", 0L);
                                        foreach (DbObject item19 in dbObject30.GetValue<DbObject>("ebx"))
                                        {
                                            AssetInfo assetInfo7 = new AssetInfo();
                                            assetInfo7.Name = item19.GetValue<string>("name");
                                            assetInfo7.NameHash = Fnv1.HashString(assetInfo7.Name);
                                            assetInfo7.Asset = item19;
                                            assetInfo7.BaseAsset = item19;
                                            list2.Add(assetInfo7);
                                        }
                                        foreach (DbObject item20 in dbObject30.GetValue<DbObject>("res"))
                                        {
                                            AssetInfo assetInfo8 = new AssetInfo();
                                            assetInfo8.Name = item20.GetValue<string>("name");
                                            assetInfo8.NameHash = Fnv1.HashString(assetInfo8.Name);
                                            assetInfo8.Asset = item20;
                                            assetInfo8.BaseAsset = item20;
                                            list3.Add(assetInfo8);
                                        }
                                        int num11 = 0;
                                        foreach (DbObject item21 in dbObject30.GetValue<DbObject>("chunks"))
                                        {
                                            AssetInfo assetInfo9 = new AssetInfo();
                                            assetInfo9.Id = item21.GetValue<Guid>("id");
                                            assetInfo9.Asset = item21;
                                            assetInfo9.BaseAsset = item21;
                                            assetInfo9.Meta = (DbObject)dbObject30.GetValue<DbObject>("chunkMeta")[num11++];
                                            list4.Add(assetInfo9);
                                        }
                                        stream3.Dispose();
                                    }
                                    if (flag8)
                                    {
                                        flag5 = true;
                                        if (parent.modifiedBundles.ContainsKey(key2))
                                        {
                                            ModBundleInfo modBundleInfo2 = parent.modifiedBundles[key2];
                                            foreach (AssetInfo item22 in list2)
                                            {
                                                if (modBundleInfo2.Modify.Ebx.Contains(item22.Name.ToLower()) && !item22.Removed)
                                                {
                                                    EbxAssetEntry ebxAssetEntry = parent.modifiedEbx[item22.Name.ToLower()];
                                                    DbObject dbObject34 = new DbObject();
                                                    dbObject34.SetValue("name", item22.Name);
                                                    dbObject34.SetValue("sha1", item22.Asset.GetValue<Sha1>("sha1"));
                                                    dbObject34.SetValue("originalSize", ebxAssetEntry.OriginalSize);
                                                    dbObject34.SetValue("data", parent.archiveData[ebxAssetEntry.Sha1].Data);
                                                    dbObject34.SetValue("dataCompressed", true);
                                                    item22.BaseAsset = (item22.Modified ? item22.BaseAsset : item22.Asset);
                                                    item22.Modified = true;
                                                    item22.Asset = dbObject34;
                                                    flag8 = true;
                                                }
                                            }
                                            foreach (string item23 in modBundleInfo2.Add.Ebx)
                                            {
                                                EbxAssetEntry ebxAssetEntry2 = parent.modifiedEbx[item23];
                                                DbObject dbObject35 = DbObject.CreateObject();
                                                AssetInfo assetInfo10 = new AssetInfo();
                                                assetInfo10.Name = ebxAssetEntry2.Name;
                                                assetInfo10.NameHash = Fnv1.HashString(assetInfo10.Name);
                                                dbObject35.SetValue("name", ebxAssetEntry2.Name);
                                                dbObject35.SetValue("sha1", ebxAssetEntry2.Sha1);
                                                dbObject35.SetValue("originalSize", ebxAssetEntry2.OriginalSize);
                                                dbObject35.SetValue("data", parent.archiveData[ebxAssetEntry2.Sha1].Data);
                                                dbObject35.SetValue("dataCompressed", true);
                                                assetInfo10.BaseAsset = dbObject35;
                                                assetInfo10.Modified = true;
                                                assetInfo10.Inserted = true;
                                                assetInfo10.Asset = dbObject35;
                                                flag8 = true;
                                                list2.Add(assetInfo10);
                                            }
                                            foreach (AssetInfo item24 in list3)
                                            {
                                                if (modBundleInfo2.Modify.Res.Contains(item24.Name.ToLower()) && !item24.Removed)
                                                {
                                                    ResAssetEntry resAssetEntry = parent.modifiedRes[item24.Name.ToLower()];
                                                    DbObject dbObject36 = new DbObject();
                                                    dbObject36.SetValue("name", item24.Name);
                                                    dbObject36.SetValue("sha1", item24.Asset.GetValue<Sha1>("sha1"));
                                                    dbObject36.SetValue("originalSize", resAssetEntry.OriginalSize);
                                                    dbObject36.SetValue("data", parent.archiveData[resAssetEntry.Sha1].Data);
                                                    dbObject36.SetValue("dataCompressed", true);
                                                    dbObject36.SetValue("resRid", resAssetEntry.ResRid);
                                                    dbObject36.SetValue("resMeta", resAssetEntry.ResMeta);
                                                    dbObject36.SetValue("resType", resAssetEntry.ResType);
                                                    item24.BaseAsset = (item24.Modified ? item24.BaseAsset : item24.Asset);
                                                    item24.Modified = true;
                                                    item24.Asset = dbObject36;
                                                    flag8 = true;
                                                }
                                            }
                                            foreach (AssetInfo item25 in list4)
                                            {
                                                if (modBundleInfo2.Modify.Chunks.Contains(item25.Id) && !item25.Removed)
                                                {
                                                    ChunkAssetEntry chunkAssetEntry7 = parent.ModifiedChunks[item25.Id];
                                                    DbObject dbObject37 = new DbObject();
                                                    byte[] array3 = parent.archiveData[chunkAssetEntry7.Sha1].Data;
                                                    if (chunkAssetEntry7.LogicalOffset != 0)
                                                    {
                                                        array3 = new byte[chunkAssetEntry7.RangeEnd - chunkAssetEntry7.RangeStart];
                                                        Array.Copy(parent.archiveData[chunkAssetEntry7.Sha1].Data, chunkAssetEntry7.RangeStart, array3, 0L, array3.Length);
                                                    }
                                                    dbObject37.SetValue("id", item25.Id);
                                                    dbObject37.SetValue("sha1", item25.Asset.GetValue<Sha1>("sha1"));
                                                    dbObject37.SetValue("logicalOffset", chunkAssetEntry7.LogicalOffset);
                                                    dbObject37.SetValue("logicalSize", chunkAssetEntry7.LogicalSize);
                                                    dbObject37.SetValue("originalSize", chunkAssetEntry7.LogicalSize);
                                                    dbObject37.SetValue("data", array3);
                                                    dbObject37.SetValue("dataCompressed", true);
                                                    item25.BaseAsset = (item25.Modified ? item25.BaseAsset : item25.Asset);
                                                    item25.Modified = true;
                                                    item25.Asset = dbObject37;
                                                    flag8 = true;
                                                }
                                            }
                                        }
                                        list.AddRange(list2);
                                        list.AddRange(list3);
                                        list.AddRange(list4);
                                        int num12 = 0;
                                        foreach (AssetInfo item26 in list2)
                                        {
                                            if (!item26.Removed)
                                            {
                                                num12++;
                                            }
                                        }
                                        int num13 = 0;
                                        foreach (AssetInfo item27 in list3)
                                        {
                                            if (!item27.Removed)
                                            {
                                                num13++;
                                            }
                                        }
                                        int num14 = 0;
                                        foreach (AssetInfo item28 in list4)
                                        {
                                            if (!item28.Removed)
                                            {
                                                num14++;
                                            }
                                        }
                                        using (NativeWriter nativeWriter3 = new NativeWriter(new FileStream(parent.fs.BasePath + "/" + modPath + "/" + superBundle + ".sb", FileMode.Append, FileAccess.Write)))
                                        {
                                            nativeWriter3.Write(1, Endian.Big);
                                            nativeWriter3.Write(0, Endian.Big);
                                            nativeWriter3.Write(3735927486u, Endian.Big);
                                            nativeWriter3.Write(3735927486u, Endian.Big);
                                            nativeWriter3.Write(3735927486u, Endian.Big);
                                            nativeWriter3.Write(3735927486u, Endian.Big);
                                            long position = nativeWriter3.BaseStream.Position;
                                            nativeWriter3.Write(2641989333u, Endian.Big);
                                            nativeWriter3.Write(num12 + num13 + num14, Endian.Big);
                                            nativeWriter3.Write(num12, Endian.Big);
                                            nativeWriter3.Write(num13, Endian.Big);
                                            nativeWriter3.Write(num14, Endian.Big);
                                            nativeWriter3.Write(3735927486u, Endian.Big);
                                            nativeWriter3.Write(3735927486u, Endian.Big);
                                            nativeWriter3.Write(3735927486u, Endian.Big);
                                            foreach (AssetInfo item29 in list)
                                            {
                                                if (!item29.Removed)
                                                {
                                                    nativeWriter3.Write(item29.Asset.GetValue<Sha1>("sha1"));
                                                }
                                            }
                                            long num15 = 0L;
                                            Dictionary<uint, long> dictionary2 = new Dictionary<uint, long>();
                                            List<string> list5 = new List<string>();
                                            foreach (AssetInfo item30 in list)
                                            {
                                                if (!item30.Removed && item30.Name != null)
                                                {
                                                    uint key3 = (uint)Fnv1.HashString(item30.Asset.GetValue<string>("name"));
                                                    if (!dictionary2.ContainsKey(key3))
                                                    {
                                                        list5.Add(item30.Asset.GetValue<string>("name"));
                                                        dictionary2.Add(key3, num15);
                                                        num15 += item30.Asset.GetValue<string>("name").Length + 1;
                                                    }
                                                    nativeWriter3.Write((uint)dictionary2[key3], Endian.Big);
                                                    nativeWriter3.Write(item30.Asset.GetValue("originalSize", 0), Endian.Big);
                                                }
                                            }
                                            foreach (AssetInfo item31 in list3)
                                            {
                                                if (!item31.Removed)
                                                {
                                                    nativeWriter3.Write(item31.Asset.GetValue("resType", 0), Endian.Big);
                                                }
                                            }
                                            foreach (AssetInfo item32 in list3)
                                            {
                                                if (!item32.Removed)
                                                {
                                                    nativeWriter3.Write(item32.Asset.GetValue<byte[]>("resMeta"));
                                                }
                                            }
                                            foreach (AssetInfo item33 in list3)
                                            {
                                                if (!item33.Removed)
                                                {
                                                    nativeWriter3.Write(item33.Asset.GetValue("resRid", 0L), Endian.Big);
                                                }
                                            }
                                            foreach (AssetInfo item34 in list4)
                                            {
                                                if (!item34.Removed)
                                                {
                                                    nativeWriter3.Write(item34.Asset.GetValue<Guid>("id"), Endian.Big);
                                                    nativeWriter3.Write(item34.Asset.GetValue("logicalOffset", 0), Endian.Big);
                                                    nativeWriter3.Write(item34.Asset.GetValue("logicalSize", 0), Endian.Big);
                                                }
                                            }
                                            long num16 = 0L;
                                            long num17 = 0L;
                                            if (list4.Count > 0)
                                            {
                                                DbObject dbObject38 = new DbObject(bObject: false);
                                                foreach (AssetInfo item35 in list4)
                                                {
                                                    if (!item35.Removed)
                                                    {
                                                        dbObject38.Add(item35.Meta);
                                                    }
                                                }
                                                num16 = nativeWriter3.BaseStream.Position - position;
                                                using (DbWriter dbWriter2 = new DbWriter(new MemoryStream()))
                                                {
                                                    nativeWriter3.Write(dbWriter2.WriteDbObject("chunkMeta", dbObject38));
                                                }
                                                num17 = nativeWriter3.BaseStream.Position - position - num16;
                                            }
                                            long num18 = nativeWriter3.BaseStream.Position - position;
                                            foreach (string item36 in list5)
                                            {
                                                nativeWriter3.WriteNullTerminatedString(item36);
                                            }
                                            while ((nativeWriter3.BaseStream.Position - (position - 4)) % 16 != 0L)
                                            {
                                                nativeWriter3.Write((byte)0);
                                            }
                                            if (num4 > 0)
                                            {
                                                nativeWriter3.Write((uint)((int)num4 | 0x40000000), Endian.Big);
                                            }
                                            long num19 = nativeWriter3.BaseStream.Position - position;
                                            for (int i = 0; i < list.Count; i++)
                                            {
                                                AssetInfo assetInfo11 = list[i];
                                                long value8 = assetInfo11.Asset.GetValue("originalSize", 0L);
                                                uint num20 = (uint)(value8 / 65536 + ((value8 % 65536 != 0L) ? 1 : 0));
                                                if (assetInfo11.Removed)
                                                {
                                                    if (num20 != 0)
                                                    {
                                                        nativeWriter3.Write(num20 | 0x40000000, Endian.Big);
                                                    }
                                                }
                                                else if (assetInfo11.Inserted || assetInfo11.Modified)
                                                {
                                                    byte[] buffer = assetInfo11.Asset.GetValue("dataCompressed", defaultValue: false) ? assetInfo11.Asset.GetValue<byte[]>("data") : Utils.CompressFile(assetInfo11.Asset.GetValue<byte[]>("data"), null, (ResourceType)assetInfo11.Asset.GetValue("resType", -1));
                                                    if (!assetInfo11.Inserted)
                                                    {
                                                        long value9 = assetInfo11.BaseAsset.GetValue("originalSize", 0L);
                                                        uint num21 = (uint)(value9 / 65536 + ((value9 % 65536 != 0L) ? 1 : 0));
                                                        if (num21 != 0)
                                                        {
                                                            nativeWriter3.Write(num21 | 0x40000000, Endian.Big);
                                                        }
                                                    }
                                                    nativeWriter3.Write(num20 | 0x30000000, Endian.Big);
                                                    nativeWriter3.Write(buffer);
                                                }
                                                else
                                                {
                                                    nativeWriter3.Write(num20, Endian.Big);
                                                }
                                            }
                                            long num22 = nativeWriter3.BaseStream.Position - position - num19;
                                            item10.RemoveValue("base");
                                            item10.SetValue("delta", true);
                                            item10.SetValue("offset", num3);
                                            item10.SetValue("size", (int)(nativeWriter3.BaseStream.Position - (position - 24)));
                                            nativeWriter3.BaseStream.Position = position + 20;
                                            nativeWriter3.Write((uint)num18, Endian.Big);
                                            nativeWriter3.Write((uint)num16, Endian.Big);
                                            nativeWriter3.Write((uint)num17, Endian.Big);
                                            nativeWriter3.BaseStream.Position = position - 16;
                                            nativeWriter3.Write((uint)(num19 + 8), Endian.Big);
                                            nativeWriter3.Write((uint)num22, Endian.Big);
                                            if (num4 != 0L)
                                            {
                                                num19 -= 4;
                                            }
                                            nativeWriter3.Write((uint)num19, Endian.Big);
                                            nativeWriter3.Write((uint)(num19 | 2147483648u), Endian.Big);
                                            num3 += item10.GetValue("size", 0);
                                        }
                                    }
                                }
                                nativeReader2?.Dispose();
                                nativeReader3?.Dispose();
                                dbObject.RemoveValue("tag");
                                dbObject.RemoveValue("name");
                                dbObject.RemoveValue("totalSize");
                                dbObject.RemoveValue("alwaysEmitSuperbundle");
                                flag4 = true;
                            }
                            else
                            {
                                if (parent.addedBundles.ContainsKey(superBundle))
                                {
                                    foreach (string item37 in parent.addedBundles[superBundle])
                                    {
                                        DbObject dbObject39 = new DbObject();
                                        dbObject39.SetValue("id", item37);
                                        dbObject39.SetValue("offset", 3735928559L);
                                        dbObject39.SetValue("size", 0L);
                                        dbObject.GetValue<DbObject>("bundles").Add(dbObject39);
                                        flag4 = true;
                                    }
                                }
                                MemoryStream memoryStream = new MemoryStream();
                                List<long> list6 = new List<long>();
                                new List<long>();
                                Stream stream4 = null;
                                foreach (DbObject item38 in dbObject.GetValue<DbObject>("bundles"))
                                {
                                    bool flag12 = false;
                                    if (ProfileManager.DataVersion == 20141118 || ProfileManager.DataVersion == 20141117 || ProfileManager.DataVersion == 20151103 || ProfileManager.DataVersion == 20131115)
                                    {
                                        flag12 = !item38.GetValue("delta", defaultValue: false);
                                    }
                                    DbObject dbObject41 = null;
                                    if (item38.GetValue("offset", 0L) == 3735928559u)
                                    {
                                        dbObject41 = new DbObject();
                                        dbObject41.SetValue("path", item38.GetValue<string>("id"));
                                        dbObject41.SetValue("magicSalt", 1885692781);
                                        dbObject41.SetValue("ebx", new DbObject(bObject: false));
                                        dbObject41.SetValue("res", new DbObject(bObject: false));
                                        dbObject41.SetValue("chunks", new DbObject(bObject: false));
                                        dbObject41.SetValue("chunkMeta", new DbObject(bObject: false));
                                        dbObject41.SetValue("alignMembers", false);
                                        dbObject41.SetValue("ridSupport", true);
                                        dbObject41.SetValue("storeCompressedSizes", false);
                                    }
                                    else
                                    {
                                        stream4 = null;
                                        stream4 = ((!flag12) ? new FileStream(flag3 ? parent.fs.ResolvePath("native_data/" + superBundle + ".sb") : parent.fs.ResolvePath("native_patch/" + superBundle + ".sb"), FileMode.Open, FileAccess.Read) : new FileStream(parent.fs.ResolvePath("native_data/" + superBundle + ".sb"), FileMode.Open, FileAccess.Read));
                                        using (DbReader dbReader6 = new DbReader(stream4, parent.fs.CreateDeobfuscator()))
                                        {
                                            stream4.Position = item38.GetValue("offset", 0L);
                                            dbObject41 = dbReader6.ReadDbObject();
                                        }
                                    }
                                    bool flag13 = false;
                                    int key4 = Fnv1.HashString(item38.GetValue<string>("id").ToLower());
                                    if (parent.modifiedBundles.ContainsKey(key4))
                                    {
                                        flag5 = true;
                                        flag13 = true;
                                        ModBundleInfo modBundleInfo3 = parent.modifiedBundles[key4];
                                        long num23 = 0L;
                                        long num24 = 0L;
                                        long num25 = 0L;
                                        dbObject41.RemoveValue("bmm");
                                        dbObject41.RemoveValue("dbx");
                                        _ = parent.modifiedBundles[key4];
                                        int num26 = 0;
                                        List<int> list7 = new List<int>();
                                        foreach (DbObject item39 in dbObject41.GetValue<DbObject>("ebx"))
                                        {
                                            string value10 = item39.GetValue<string>("name");
                                            num26++;
                                            if (modBundleInfo3.Remove.Ebx.Contains(value10))
                                            {
                                                list7.Add(num26 - 1);
                                                num25 -= item39.GetValue("size", 0L);
                                            }
                                            else
                                            {
                                                if (modBundleInfo3.Modify.Ebx.Contains(value10))
                                                {
                                                    EbxAssetEntry ebxAssetEntry3 = parent.modifiedEbx[value10];
                                                    item39.SetValue("sha1", ebxAssetEntry3.Sha1);
                                                    item39.SetValue("size", ebxAssetEntry3.Size);
                                                    item39.SetValue("originalSize", ebxAssetEntry3.OriginalSize);
                                                    if (ebxAssetEntry3.IsInline)
                                                    {
                                                        item39.SetValue("idata", parent.archiveData[ebxAssetEntry3.Sha1].Data);
                                                    }
                                                    if (ProfileManager.DataVersion == 20141118 || ProfileManager.DataVersion == 20141117 || ProfileManager.DataVersion == 20151103 || ProfileManager.DataVersion == 20131115)
                                                    {
                                                        item39.SetValue("casPatchType", 1);
                                                        item39.RemoveValue("baseSha1");
                                                        item39.RemoveValue("deltaSha1");
                                                    }
                                                    if (!casRefs.Contains(ebxAssetEntry3.Sha1))
                                                    {
                                                        casRefs.Add(ebxAssetEntry3.Sha1);
                                                        chunkEntries.Add(null);
                                                    }
                                                }
                                                num25 += item39.GetValue("size", 0L);
                                            }
                                        }
                                        foreach (int item40 in list7)
                                        {
                                            dbObject41.GetValue<DbObject>("ebx").RemoveAt(item40);
                                        }
                                        foreach (string item41 in modBundleInfo3.Add.Ebx)
                                        {
                                            flag13 = true;
                                            EbxAssetEntry ebxAssetEntry4 = parent.modifiedEbx[item41];
                                            DbObject dbObject43 = new DbObject();
                                            dbObject43.SetValue("name", ebxAssetEntry4.Name);
                                            dbObject43.SetValue("sha1", ebxAssetEntry4.Sha1);
                                            dbObject43.SetValue("size", ebxAssetEntry4.Size);
                                            dbObject43.SetValue("originalSize", ebxAssetEntry4.OriginalSize);
                                            if (!casRefs.Contains(ebxAssetEntry4.Sha1))
                                            {
                                                casRefs.Add(ebxAssetEntry4.Sha1);
                                                chunkEntries.Add(null);
                                            }
                                            dbObject41.GetValue<DbObject>("ebx").Add(dbObject43);
                                            num25 += dbObject43.GetValue("size", 0L);
                                        }
                                        int num27 = 0;
                                        List<int> list8 = new List<int>();
                                        foreach (DbObject item42 in dbObject41.GetValue<DbObject>("res"))
                                        {
                                            string value11 = item42.GetValue<string>("name");
                                            num27++;
                                            if (modBundleInfo3.Remove.Res.Contains(value11))
                                            {
                                                list8.Add(num27 - 1);
                                                num24 -= item42.GetValue("size", 0L);
                                            }
                                            else
                                            {
                                                if (modBundleInfo3.Modify.Res.Contains(value11))
                                                {
                                                    ResAssetEntry resAssetEntry2 = parent.modifiedRes[value11];
                                                    item42.SetValue("sha1", resAssetEntry2.Sha1);
                                                    item42.SetValue("size", resAssetEntry2.Size);
                                                    item42.SetValue("originalSize", resAssetEntry2.OriginalSize);
                                                    item42.SetValue("resRid", (long)resAssetEntry2.ResRid);
                                                    item42.SetValue("resMeta", resAssetEntry2.ResMeta);
                                                    if (resAssetEntry2.IsInline)
                                                    {
                                                        item42.SetValue("idata", parent.archiveData[resAssetEntry2.Sha1].Data);
                                                    }
                                                    if (ProfileManager.DataVersion == 20141118 || ProfileManager.DataVersion == 20141117 || ProfileManager.DataVersion == 20151103 || ProfileManager.DataVersion == 20131115)
                                                    {
                                                        item42.SetValue("casPatchType", 1);
                                                        item42.RemoveValue("baseSha1");
                                                        item42.RemoveValue("deltaSha1");
                                                    }
                                                    if (!casRefs.Contains(resAssetEntry2.Sha1))
                                                    {
                                                        casRefs.Add(resAssetEntry2.Sha1);
                                                        chunkEntries.Add(null);
                                                    }
                                                }
                                                num24 += item42.GetValue("size", 0L);
                                            }
                                        }
                                        foreach (int item43 in list8)
                                        {
                                            dbObject41.GetValue<DbObject>("res").RemoveAt(item43);
                                        }
                                        foreach (string re in modBundleInfo3.Add.Res)
                                        {
                                            flag13 = true;
                                            ResAssetEntry resAssetEntry3 = parent.modifiedRes[re];
                                            DbObject dbObject45 = new DbObject();
                                            dbObject45.SetValue("name", resAssetEntry3.Name);
                                            dbObject45.SetValue("sha1", resAssetEntry3.Sha1);
                                            dbObject45.SetValue("size", resAssetEntry3.Size);
                                            dbObject45.SetValue("originalSize", resAssetEntry3.OriginalSize);
                                            dbObject45.SetValue("resType", resAssetEntry3.ResType);
                                            dbObject45.SetValue("resMeta", resAssetEntry3.ResMeta);
                                            dbObject45.SetValue("resRid", (long)resAssetEntry3.ResRid);
                                            if (resAssetEntry3.IsInline)
                                            {
                                                dbObject45.SetValue("idata", parent.archiveData[resAssetEntry3.Sha1].Data);
                                            }
                                            if (!casRefs.Contains(resAssetEntry3.Sha1))
                                            {
                                                casRefs.Add(resAssetEntry3.Sha1);
                                                chunkEntries.Add(null);
                                            }
                                            dbObject41.GetValue<DbObject>("res").Add(dbObject45);
                                            num24 += dbObject45.GetValue("size", 0L);
                                        }
                                        List<int> list9 = new List<int>();
                                        if (dbObject41.GetValue<DbObject>("chunks") != null)
                                        {
                                            int num28 = 0;
                                            foreach (DbObject item44 in dbObject41.GetValue<DbObject>("chunks"))
                                            {
                                                Guid value12 = item44.GetValue<Guid>("id");
                                                DbObject dbObject47 = dbObject41.GetValue<DbObject>("chunkMeta")[num28++] as DbObject;
                                                if (modBundleInfo3.Remove.Chunks.Contains(value12))
                                                {
                                                    list9.Add(num28 - 1);
                                                    num23 -= item44.GetValue("bundledSize", 0L);
                                                }
                                                else
                                                {
                                                    if (modBundleInfo3.Modify.Chunks.Contains(value12))
                                                    {
                                                        ChunkAssetEntry chunkAssetEntry8 = parent.ModifiedChunks[value12];
                                                        item44.SetValue("sha1", chunkAssetEntry8.Sha1);
                                                        item44.SetValue("size", (int)chunkAssetEntry8.Size);
                                                        if (chunkAssetEntry8.FirstMip != -1)
                                                        {
                                                            item44.SetValue("rangeStart", (int)chunkAssetEntry8.RangeStart);
                                                            item44.SetValue("rangeEnd", (int)chunkAssetEntry8.RangeEnd);
                                                            dbObject47.GetValue<DbObject>("meta").RemoveValue("firstMip");
                                                            if (ProfileManager.DataVersion == 20170321 || ProfileManager.DataVersion == 20170929 || ProfileManager.DataVersion == 20171117 || ProfileManager.DataVersion == 20180807)
                                                            {
                                                                item44.SetValue("bundledSize", (int)(chunkAssetEntry8.RangeEnd - chunkAssetEntry8.RangeStart));
                                                            }
                                                            item44.SetValue("bundledSize", chunkAssetEntry8.Size);
                                                        }
                                                        else
                                                        {
                                                            if (ProfileManager.DataVersion == 20141118 || ProfileManager.DataVersion == 20141117 || ProfileManager.DataVersion == 20151103 || ProfileManager.DataVersion == 20131115)
                                                            {
                                                                item44.RemoveValue("rangeStart");
                                                                item44.RemoveValue("rangeEnd");
                                                            }
                                                            if (ProfileManager.DataVersion == 20170321 || ProfileManager.DataVersion == 20170929 || ProfileManager.DataVersion == 20171117 || ProfileManager.DataVersion == 20180807)
                                                            {
                                                                item44.SetValue("bundledSize", (int)chunkAssetEntry8.Size);
                                                            }
                                                        }
                                                        item44.SetValue("logicalOffset", (int)chunkAssetEntry8.LogicalOffset);
                                                        item44.SetValue("logicalSize", (int)chunkAssetEntry8.LogicalSize);
                                                        if (chunkAssetEntry8.IsInline)
                                                        {
                                                            item44.SetValue("idata", parent.archiveData[chunkAssetEntry8.Sha1].Data);
                                                        }
                                                        else
                                                        {
                                                            item44.RemoveValue("idata");
                                                        }
                                                        if (ProfileManager.DataVersion == 20141118 || ProfileManager.DataVersion == 20141117 || ProfileManager.DataVersion == 20151103 || ProfileManager.DataVersion == 20131115)
                                                        {
                                                            item44.SetValue("casPatchType", 1);
                                                        }
                                                        if (!casRefs.Contains(chunkAssetEntry8.Sha1))
                                                        {
                                                            casRefs.Add(chunkAssetEntry8.Sha1);
                                                            chunkEntries.Add(chunkAssetEntry8);
                                                        }
                                                    }
                                                    num23 += ((ProfileManager.DataVersion == 20170321 || ProfileManager.DataVersion == 20170929 || ProfileManager.DataVersion == 20171117 || ProfileManager.DataVersion == 20180807) ? item44.GetValue("bundledSize", 0L) : item44.GetValue("size", 0L));
                                                }
                                            }
                                        }
                                        foreach (int item45 in list9)
                                        {
                                            dbObject41.GetValue<DbObject>("chunks").RemoveAt(item45);
                                        }
                                        if (modBundleInfo3.Add.Chunks.Count != 0 && dbObject41.GetValue<DbObject>("chunks") == null)
                                        {
                                            dbObject41.AddValue("chunks", new DbObject(bObject: false));
                                            dbObject41.AddValue("chunkMeta", new DbObject(bObject: false));
                                        }
                                        foreach (Guid chunk5 in modBundleInfo3.Add.Chunks)
                                        {
                                            flag13 = true;
                                            ChunkAssetEntry chunkAssetEntry9 = parent.ModifiedChunks[chunk5];
                                            DbObject dbObject48 = new DbObject();
                                            dbObject48.AddValue("h32", chunkAssetEntry9.H32);
                                            dbObject48.AddValue("meta", new DbObject());
                                            DbObject dbObject49 = new DbObject();
                                            dbObject49.SetValue("id", chunkAssetEntry9.Id);
                                            dbObject49.SetValue("sha1", chunkAssetEntry9.Sha1);
                                            dbObject49.SetValue("size", (int)chunkAssetEntry9.Size);
                                            if (chunkAssetEntry9.FirstMip != -1)
                                            {
                                                dbObject49.SetValue("rangeStart", (int)chunkAssetEntry9.RangeStart);
                                                dbObject49.SetValue("rangeEnd", (int)chunkAssetEntry9.RangeEnd);
                                                dbObject48.GetValue<DbObject>("meta").SetValue("firstMip", chunkAssetEntry9.FirstMip);
                                                if (ProfileManager.DataVersion == 20170321 || ProfileManager.DataVersion == 20170929 || ProfileManager.DataVersion == 20171117 || ProfileManager.DataVersion == 20180807)
                                                {
                                                    dbObject49.SetValue("bundledSize", (int)(chunkAssetEntry9.RangeEnd - chunkAssetEntry9.RangeStart));
                                                }
                                            }
                                            else if (ProfileManager.DataVersion == 20170321 || ProfileManager.DataVersion == 20170929 || ProfileManager.DataVersion == 20171117 || ProfileManager.DataVersion == 20180807)
                                            {
                                                dbObject49.SetValue("bundledSize", (int)chunkAssetEntry9.Size);
                                            }
                                            dbObject49.SetValue("logicalOffset", (int)chunkAssetEntry9.LogicalOffset);
                                            dbObject49.SetValue("logicalSize", (int)chunkAssetEntry9.LogicalSize);
                                            if (chunkAssetEntry9.IsInline)
                                            {
                                                dbObject49.SetValue("idata", parent.archiveData[chunkAssetEntry9.Sha1].Data);
                                            }
                                            if (!casRefs.Contains(chunkAssetEntry9.Sha1))
                                            {
                                                casRefs.Add(chunkAssetEntry9.Sha1);
                                                chunkEntries.Add(chunkAssetEntry9);
                                            }
                                            dbObject41.GetValue<DbObject>("chunks").Add(dbObject49);
                                            dbObject41.GetValue<DbObject>("chunkMeta").Add(dbObject48);
                                            num23 += ((ProfileManager.DataVersion == 20170321 || ProfileManager.DataVersion == 20170929 || ProfileManager.DataVersion == 20171117 || ProfileManager.DataVersion == 20180807) ? dbObject49.GetValue("bundledSize", 0L) : dbObject49.GetValue("size", 0L));
                                        }
                                        if (ProfileManager.DataVersion != 20141118 && ProfileManager.DataVersion != 20141117 && ProfileManager.DataVersion != 20151103 && ProfileManager.DataVersion != 20131115)
                                        {
                                            dbObject41.SetValue("chunkBundleSize", num23);
                                            dbObject41.SetValue("resBundleSize", num24);
                                            dbObject41.SetValue("ebxBundleSize", num25);
                                            dbObject41.SetValue("dbxBundleSize", 0L);
                                        }
                                        dbObject41.SetValue("totalSize", num23 + num24 + num25);
                                        if (ProfileManager.DataVersion != 20141118 && ProfileManager.DataVersion != 20141117 && ProfileManager.DataVersion != 20151103 && ProfileManager.DataVersion != 20131115)
                                        {
                                            dbObject41.SetValue("dbxTotalSize", 0L);
                                        }
                                    }
                                    if ((flag12 && flag13) || !flag12)
                                    {
                                        list6.Add(memoryStream.Position);
                                        using (DbWriter dbWriter3 = new DbWriter(memoryStream, inWriteHeader: false, leaveOpen: true))
                                        {
                                            dbWriter3.Write(dbObject41);
                                        }
                                        item38.SetValue("size", (int)(memoryStream.Position - list6[list6.Count - 1]));
                                        if (flag12)
                                        {
                                            item38.RemoveValue("base");
                                            item38.AddValue("delta", true);
                                        }
                                    }
                                    else
                                    {
                                        list6.Add(item38.GetValue("offset", 0L));
                                        if (flag12)
                                        {
                                            item38.AddValue("base", true);
                                        }
                                    }
                                }
                                if (flag5)
                                {
                                    FileInfo fileInfo2 = new FileInfo(parent.fs.BasePath + "/" + modPath + "/" + superBundle + ".sb");
                                    Directory.CreateDirectory(fileInfo2.DirectoryName);
                                    int num29 = (int)(memoryStream.Length + 1);
                                    int value13 = Calc7BitEncodedIntSize(num29) + num29 + 10;
                                    long num30 = 0L;
                                    memoryStream.Position = 0L;
                                    using (NativeWriter nativeWriter4 = new NativeWriter(new FileStream(fileInfo2.FullName, FileMode.Create)))
                                    {
                                        nativeWriter4.Write((byte)130);
                                        nativeWriter4.Write7BitEncodedInt(value13);
                                        nativeWriter4.Write((byte)1);
                                        nativeWriter4.WriteNullTerminatedString("bundles");
                                        nativeWriter4.Write7BitEncodedInt(num29);
                                        num30 = nativeWriter4.BaseStream.Position;
                                        nativeWriter4.Write(memoryStream.ToArray());
                                        nativeWriter4.Write((ushort)0);
                                    }
                                    memoryStream.Dispose();
                                    int num31 = 0;
                                    foreach (DbObject item46 in dbObject.GetValue<DbObject>("bundles"))
                                    {
                                        item46.SetValue("offset", num30 + list6[num31++]);
                                    }
                                    flag4 = true;
                                }
                            }
                        }
                        if (flag4)
                        {
                            FileInfo fileInfo3 = new FileInfo(parent.fs.BasePath + "/" + modPath + "/" + superBundle + ".toc");
                            Directory.CreateDirectory(fileInfo3.DirectoryName);
                            using (DbWriter dbWriter4 = new DbWriter(new FileStream(fileInfo3.FullName, FileMode.Create), inWriteHeader: true))
                            {
                                dbWriter4.Write(dbObject);
                            }
                        }
                        TocModified = flag4;
                        SbModified = flag5;
                    }
                end_IL_0021:;
                }
                catch (Exception ex)
                {
                    Exception ex2 = errorException = ex;
                }
            }

            public void ThreadPoolCallback(object threadContext)
            {
                Run();
                if (Interlocked.Decrement(ref parent.numTasks) == 0)
                {
                    doneEvent.Set();
                }
            }

            private int Calc7BitEncodedIntSize(int value)
            {
                int num = 0;
                for (uint num2 = (uint)value; num2 >= 128; num2 >>= 7)
                {
                    num++;
                }
                return num + 1;
            }
        }

        public FileSystem fs;

        public ResourceManager rm;

        private ILogger logger;

        public List<string> addedSuperBundles = new List<string>();

        public Dictionary<int, ModBundleInfo> modifiedBundles = new Dictionary<int, ModBundleInfo>();

        public Dictionary<string, List<string>> addedBundles = new Dictionary<string, List<string>>();

        public Dictionary<string, AssetEntry> ModifiedAssets
        {
            get
            {
                Dictionary<string, AssetEntry> entries = new Dictionary<string, AssetEntry>();
                foreach(var item in modifiedEbx)
                {
                    entries.Add(item.Key, item.Value);
                }
                foreach (var item in modifiedRes)
                {
                    entries.Add(item.Value.ToString(), item.Value);
                }
                foreach (var item in ModifiedChunks)
                {
                    entries.Add(item.Key.ToString(), item.Value);
                }
                return entries;
            }
        }


        public Dictionary<string, EbxAssetEntry> modifiedEbx { get; } = new Dictionary<string, EbxAssetEntry>();

        public Dictionary<string, ResAssetEntry> modifiedRes { get; } = new Dictionary<string, ResAssetEntry>();

        public Dictionary<Guid, ChunkAssetEntry> ModifiedChunks { get; } = new Dictionary<Guid, ChunkAssetEntry>();

        /// <summary>
        /// Added by PG 24 May 2021 to add (not modify) TOC Chunks
        /// </summary>
        public Dictionary<Guid, ChunkAssetEntry> AddedChunks = new Dictionary<Guid, ChunkAssetEntry>();

        public Dictionary<string, LegacyFileEntry> modifiedLegacy = new Dictionary<string, LegacyFileEntry>();

        public Dictionary<Sha1, ArchiveInfo> archiveData { get; } = new Dictionary<Sha1, ArchiveInfo>();


        public int numTasks;

        public CasDataInfo casData = new CasDataInfo();

        public static int chunksBundleHash = Fnv1.HashString("chunks");

        //public Dictionary<int, Dictionary<int, Dictionary<uint, CatResourceEntry>>> resources = new Dictionary<int, Dictionary<int, Dictionary<uint, CatResourceEntry>>>();

        public ILogger Logger
        {
            get
            {
                return logger;
            }
            set
            {
                logger = value;
            }
        }

        public string GamePath { get { return fs.BasePath; } }
        public string GameEXEPath { get { return Path.Combine(GamePath, ProfileManager.ProfileName + ".exe"); } }
        public string GameEXEPathNoExtension { get { return Path.Combine(GamePath, ProfileManager.ProfileName); } }

        public bool EADesktopIsInstalled
        {
            get
            {

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Electronic Arts\\EA Desktop"))
                {
                    if (key != null)
                    {
                        string installDir = key.GetValue("InstallLocation").ToString();
                        string installSuccessful = key.GetValue("InstallSuccessful").ToString();
                        return !string.IsNullOrEmpty(installDir) && !string.IsNullOrEmpty(installSuccessful);
                    }
                }

                return Process.GetProcessesByName("EADesktop.exe").Any();
            }
        }
        public bool LaunchedViaEADesktop { get; set; } = false;

        public static bool UseACBypass { get; set; }
            = ProfileManager.IsFIFA23DataVersion()
            //&& FileSystem.Instance.Head <= 1572210
            && ProfileManager.LoadedProfile.UseACBypass;
        //public bool UseACBypass { get; set; } = false;

        public bool UseSymbolicLinks = false;

        [DllImport("kernel32.dll")]
        protected static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        public Dictionary<int, Dictionary<uint, CatResourceEntry>> LoadCatalog(FileSystem fs, string filename, out int catFileHash)
        {

            catFileHash = 0;
            string text = fs.ResolvePath(filename);
            if (!File.Exists(text))
            {
                return null;
            }
            catFileHash = Fnv1.HashString(text.ToLower());
            Dictionary<int, Dictionary<uint, CatResourceEntry>> dictionary = new Dictionary<int, Dictionary<uint, CatResourceEntry>>();
            using (CatReader catReader = new CatReader(new FileStream(text, FileMode.Open, FileAccess.Read), fs.CreateDeobfuscator()))
            {
                for (int i = 0; i < catReader.ResourceCount; i++)
                {
                    CatResourceEntry value = catReader.ReadResourceEntry();
                    if (!dictionary.ContainsKey(value.ArchiveIndex))
                    {
                        dictionary.Add(value.ArchiveIndex, new Dictionary<uint, CatResourceEntry>());
                    }
                    dictionary[value.ArchiveIndex].Add(value.Offset, value);
                }
                return dictionary;
            }
        }
        string modDirName = "ModData";

        public bool UseLegacyLauncher = false;

        List<Assembly> PluginAssemblies = new List<Assembly>();

        private bool FileIsSymbolic(string path)
        {
            FileInfo pathInfo = new FileInfo(path);
            return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        public async Task<bool> BuildModData(FileSystem inFs, ILogger inLogger, string rootPath, string additionalArgs, params string[] modPaths)
        {
            fs = inFs;
            Logger = inLogger;

            if (!AssetManager.InitialisePlugins())
            {
                throw new Exception("Unable to initialize Plugins");
            }
            string modPath = fs.BasePath + modDirName + "\\";
            string patchPath = "Patch";

            string profileName = ProfileManager.ProfileName;
            if(Process.GetProcesses().Any(x=>x.ProcessName.Equals(profileName, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("Game process is already running, please close and relaunch");

            if (ResourceManager.Instance == null)
            {
                rm = new ResourceManager(fs);
                rm.Initialize();
            }
            else
            {
                rm = ResourceManager.Instance;
            }


            bool FrostyModsFound = false;

            {
                string[] allModPaths = modPaths;
                var frostyMods = new Dictionary<Stream, IFrostbiteMod>();

                // Sort out Zipped Files
                //if (allModPaths.Contains(".zip"))
                //{

                Logger.Log("Deleting cached mods");

                if (Directory.Exists(ApplicationDirectory + "TempMods"))
                    Directory.Delete(ApplicationDirectory + "TempMods", true);

                var compatibleModExtensions = new List<string>() { ".fbmod", ".fifamod" };
                Logger.Log("Loading mods");

                foreach (var f in allModPaths.Select(x=> new FileInfo(x)))
                {
                    if (f.Extension.Contains("zip", StringComparison.OrdinalIgnoreCase))
                    {
                        var z = f.FullName;

                        Logger.Log("Loading mods from " + z);

                        using (FileStream fsModZipped = new FileStream(z, FileMode.Open))
                        //FileStream fsModZipped = new FileStream(z, FileMode.Open);
                        {
                            ZipArchive zipArchive = new ZipArchive(fsModZipped);
                            foreach (var zaentr in zipArchive.Entries
                                .Where(x =>
                                x.FullName.Contains(".fbmod", StringComparison.OrdinalIgnoreCase)
                                || x.FullName.Contains(".fifamod", StringComparison.OrdinalIgnoreCase)))
                            {
                                Logger.Log("Loading mod " + zaentr.Name);
                                FrostyModsFound = true;
                                MemoryStream memoryStream = new MemoryStream();
                                if (zaentr.Length > 1024 * 1024 || zaentr.Name.Contains(".fifamod"))
                                {
                                    if (!Directory.Exists(ApplicationDirectory + "TempMods"))
                                        Directory.CreateDirectory(ApplicationDirectory + "TempMods");
                                    zaentr.ExtractToFile(ApplicationDirectory + "TempMods/" + zaentr.Name);
                                    GatherFrostbiteMods(ApplicationDirectory + "TempMods/" + zaentr.Name, ref FrostyModsFound, ref frostyMods);
                                }
                                else
                                {
                                    zaentr.Open().CopyTo(memoryStream);
                                    frostyMods.TryAdd(new MemoryStream(memoryStream.ToArray()), new FrostbiteMod(new MemoryStream(memoryStream.ToArray())));
                                }
                            }
                        }
                    }
                    else
                        GatherFrostbiteMods(rootPath + f, ref FrostyModsFound, ref frostyMods);
                }

                foreach (KeyValuePair<Stream, IFrostbiteMod> kvpMods in frostyMods)
                {
                    //Logger.Log("Compiling mod " + kvpMods.Value.Filename);


                    int indexCompleted = -1;
                    var frostbiteMod = kvpMods.Value;
                    //Parallel.ForEach(frostbiteMod.Resources, (BaseModResource resource) =>
                    foreach (
                        (BaseModResource, byte[]) r 
                        in 
                        frostbiteMod.Resources
                        .Select(x => (x, frostbiteMod.GetResourceData(x)))
                        .Where(x => x.Item2 != null)
                        .OrderBy(x => x.Item2.Length)
                        )
                    {
                        indexCompleted++;

                        // ------------------------------------------------------------------
                        // Get the Resource Data out of the mod
                        BaseModResource resource = r.Item1;
                        byte[] resourceData = r.Item2;
                        // ------------------------------------------------------------------
                        // Embedded Files
                        // Export to the Game Directory and create sub folders if neccessary
                        if (resource is BaseModReader.EmbeddedFileResource)
                        {
                            EmbeddedFileEntry efAssetEntry = new EmbeddedFileEntry();
                            resource.FillAssetEntry(efAssetEntry);

                            var parentDirectoryPath = Directory.GetParent(GamePath + "//" + efAssetEntry.ExportedRelativePath).FullName;
                            if (!Directory.Exists(parentDirectoryPath))
                                Directory.CreateDirectory(parentDirectoryPath);

                            var exportedFilePath = GamePath + "//" + efAssetEntry.ExportedRelativePath;
                            var exportedFileBackupPath = GamePath + "//" + efAssetEntry.ExportedRelativePath + ".bak";
                            //File.WriteAllBytes(GamePath + "//" + efAssetEntry.Name, resourceData);
                            if (!File.Exists(exportedFileBackupPath)
                                && File.Exists(exportedFilePath)
                                )
                                File.Move(exportedFilePath, exportedFileBackupPath);

                            File.WriteAllBytes(exportedFilePath, resourceData);

                        }
                        //
                        // ------------------------------------------------------------------


                        foreach (int modifiedBundle in resource.ModifiedBundles)
                        {
                            if (!modifiedBundles.ContainsKey(modifiedBundle))
                            {
                                modifiedBundles.Add(modifiedBundle, new ModBundleInfo
                                {
                                    Name = modifiedBundle
                                });
                            }
                            ModBundleInfo modBundleInfo = modifiedBundles[modifiedBundle];
                            switch (resource.Type)
                            {
                                case ModResourceType.Ebx:
                                    modBundleInfo.Modify.AddEbx(resource.Name);
                                    break;
                                case ModResourceType.Res:
                                    modBundleInfo.Modify.AddRes(resource.Name);
                                    break;
                                case ModResourceType.Chunk:
                                    modBundleInfo.Modify.AddChunk(new Guid(resource.Name));
                                    break;
                                case ModResourceType.Legacy:
                                    modBundleInfo.Modify.AddLegacy(resource.Name);
                                    break;
                            }
                        }


                        foreach (int addedBundle in resource.AddedBundles)
                        {
                            if (!modifiedBundles.ContainsKey(addedBundle))
                            {
                                modifiedBundles.Add(addedBundle, new ModBundleInfo
                                {
                                    Name = addedBundle
                                });
                            }
                            ModBundleInfo modBundleInfo2 = modifiedBundles[addedBundle];
                            switch (resource.Type)
                            {
                                case ModResourceType.Ebx:
                                    modBundleInfo2.Add.AddEbx(resource.Name);
                                    break;
                                case ModResourceType.Res:
                                    modBundleInfo2.Add.AddRes(resource.Name);
                                    break;
                                case ModResourceType.Chunk:
                                    modBundleInfo2.Add.AddChunk(new Guid(resource.Name));
                                    break;
                            }
                        }


                        switch (resource.Type)
                        {
                            case ModResourceType.Ebx:
                                if (modifiedEbx.ContainsKey(resource.Name))
                                {
                                    modifiedEbx.Remove(resource.Name);
                                    if (archiveData.ContainsKey(resource.Sha1))
                                        archiveData.Remove(resource.Sha1, out ArchiveInfo _);
                                }
                                EbxAssetEntry ebxEntry = new EbxAssetEntry();
                                resource.FillAssetEntry(ebxEntry);
                                ebxEntry.Size = resourceData.Length;
                                modifiedEbx.Add(ebxEntry.Name, ebxEntry);
                                if (!archiveData.ContainsKey(ebxEntry.Sha1))
                                    archiveData.Add(ebxEntry.Sha1, new ArchiveInfo
                                    {
                                        Data = resourceData,
                                        RefCount = 1
                                    });


                                archiveData[ebxEntry.Sha1].Data = resourceData;

                                break;
                            case ModResourceType.Res:
                                if (modifiedRes.ContainsKey(resource.Name))
                                {
                                    modifiedRes.Remove(resource.Name);
                                    if (archiveData.ContainsKey(resource.Sha1))
                                        archiveData.Remove(resource.Sha1, out ArchiveInfo _);

                                }
                                ResAssetEntry resEntry = new ResAssetEntry();
                                resource.FillAssetEntry(resEntry);
                                resEntry.Size = resourceData.Length;
                                modifiedRes.Add(resEntry.Name, resEntry);
                                if (!archiveData.ContainsKey(resEntry.Sha1))
                                    archiveData.Add(resEntry.Sha1, new ArchiveInfo
                                    {
                                        Data = resourceData,
                                        RefCount = 1
                                    });


                                archiveData[resEntry.Sha1].Data = resourceData;

                                break;
                        }

                        
                        if (resource.Type == ModResourceType.Chunk)
                        {
                            Guid guid = new Guid(resource.Name);
                            if (ModifiedChunks.ContainsKey(guid))
                            {
                                ModifiedChunks.Remove(guid);
                            }
                            ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
                            resource.FillAssetEntry(chunkAssetEntry);
                            chunkAssetEntry.Size = resourceData.Length;

                            ModifiedChunks.Add(guid, chunkAssetEntry);
                            if (!archiveData.ContainsKey(chunkAssetEntry.Sha1))
                            {
                                archiveData.TryAdd(chunkAssetEntry.Sha1, new ArchiveInfo
                                {
                                    Data = resourceData,
                                });
                            }
                            else
                            {
                                archiveData[chunkAssetEntry.Sha1].Data = resourceData;
                            }
                        }

                        else if (resource.Type == ModResourceType.Legacy)
                        {
                            LegacyFileEntry legacyAssetEntry = new LegacyFileEntry();
                            resource.FillAssetEntry(legacyAssetEntry);
                            legacyAssetEntry.ModifiedEntry = new FrostySdk.FrostbiteSdk.Managers.ModifiedLegacyAssetEntry() { Data = resourceData };
                            legacyAssetEntry.Size = resourceData.Length;

                            if (!modifiedLegacy.ContainsKey(legacyAssetEntry.Name))
                                modifiedLegacy.Add(legacyAssetEntry.Name, legacyAssetEntry);
                            else
                                modifiedLegacy[legacyAssetEntry.Name] = legacyAssetEntry;
                        }
                    }

                }

                // ----------------------------------------------------------------
                // Clear out memory and mods
                foreach (KeyValuePair<Stream, IFrostbiteMod> kvpMods in frostyMods)
                {
                    kvpMods.Key.Dispose();
                    if (kvpMods.Value is FrostbiteMod)
                    {
                        kvpMods.Value.ModBytes = null;
                    }
                }
                frostyMods.Clear();

                //Logger.Log("Cleaning up mod data directory");
                //List<SymLinkStruct> SymbolicLinkList = new List<SymLinkStruct>();
                fs.ResetManifest();


                //Logger.Log("Creating mod data directory");

                // ----------------------------------------------------------------
                // Create ModData Directory in the Game Path
                Directory.CreateDirectory(modPath);
                Directory.CreateDirectory(Path.Combine(modPath, "Data"));
                Directory.CreateDirectory(Path.Combine(modPath, "Patch"));
                Directory.CreateDirectory(Path.Combine(modPath, "Update"));


                int workerThreads = 0;
                int completionPortThreads = 0;
                ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
                ThreadPool.SetMaxThreads(Environment.ProcessorCount, completionPortThreads);
                //Logger.Log("Applying mods");
                //SymbolicLinkList.Clear();


                var pluginCompiler = AssetManager.LoadTypeFromPlugin2(ProfileManager.AssetCompilerName);
                if (pluginCompiler == null && !string.IsNullOrEmpty(ProfileManager.AssetCompilerName))
                    throw new NotImplementedException($"Could not find class {ProfileManager.AssetCompilerName} in any plugin! Remember this is case sensitive!!");

                if (pluginCompiler != null)
                {
                    ((IAssetCompiler)pluginCompiler).Compile(fs, logger, this);
                }
                else
                {

                    foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()
                        .Where(x => x.FullName.ToLower().Contains("plugin")))
                    {
                        foreach (Type t in a.GetTypes())
                        {
                            if (t.GetInterface("IAssetCompiler") != null)
                            {
                                //try
                                //{
                                if (t.Name == ProfileManager.AssetCompilerName)
                                {
                                    Logger.Log("Attempting to load Compiler for " + GameEXEPath);

                                    if (!((IAssetCompiler)Activator.CreateInstance(t)).Compile(fs, Logger, this))
                                    {
                                        Logger.LogError("Unable to load Compiler. Stopping");
                                        return false;
                                    }
                                }
                                //}
                                //catch (Exception e)
                                //{
                                //    Logger.LogError($"Error in Compiler :: {e.Message}");

                                //}
                            }
                        }
                    }
                }

                if (ProfileManager.IsFIFA20DataVersion())
                {
                    DbObject layoutToc = null;


                    using (DbReader dbReaderOfLayoutTOC = new DbReader(new FileStream(fs.BasePath + patchPath + "/layout.toc", FileMode.Open, FileAccess.Read), fs.CreateDeobfuscator()))
                    {
                        layoutToc = dbReaderOfLayoutTOC.ReadDbObject();
                    }


                    FifaBundleAction.CasFileCount = fs.CasFileCount;
                    List<FifaBundleAction> fifaBundleActions = new List<FifaBundleAction>();
                    ManualResetEvent inDoneEvent = new ManualResetEvent(initialState: false);

                    var numberOfCatalogs = fs.Catalogs.Count();
                    var numberOfCatalogsCompleted = 0;

                    foreach (Catalog catalogItem in fs.EnumerateCatalogInfos())
                    {
                        FifaBundleAction fifaBundleAction = new FifaBundleAction(catalogItem, inDoneEvent, this);
                        fifaBundleAction.Run();
                        numberOfCatalogsCompleted++;
                        logger.Log($"Compiling Mod Progress: {Math.Round((double)numberOfCatalogsCompleted / numberOfCatalogs, 2) * 100} %");

                        fifaBundleActions.Add(fifaBundleAction);
                    }

                    foreach (FifaBundleAction bundleAction in fifaBundleActions.Where(x => !x.HasErrored && x.CasFiles.Count > 0))
                    {
                        if (bundleAction.HasErrored)
                        {
                            throw bundleAction.Exception;
                        }
                        if (bundleAction.CasFiles.Count > 0)
                        {
                            foreach (DbObject installManifestChunks in layoutToc.GetValue<DbObject>("installManifest").GetValue<DbObject>("installChunks"))
                            {
                                if (bundleAction.CatalogInfo.Name.Equals("win32/" + installManifestChunks.GetValue<string>("name")))
                                {
                                    foreach (int key in bundleAction.CasFiles.Keys)
                                    {
                                        DbObject dbObject6 = DbObject.CreateObject();
                                        dbObject6.SetValue("id", key);
                                        dbObject6.SetValue("path", bundleAction.CasFiles[key]);
                                        installManifestChunks.GetValue<DbObject>("files").Add(dbObject6);


                                    }
                                    break;
                                }
                            }
                        }
                    }

                    logger.Log("Writing new Layout file to Game");
                    using (DbWriter dbWriter = new DbWriter(new FileStream(modPath + patchPath + "/layout.toc", FileMode.Create), inWriteHeader: true))
                    {
                        dbWriter.Write(layoutToc);
                    }
                }


                if (UseModData)
                {
                    logger.Log("Copying initfs_win32");
                    Directory.CreateDirectory(modPath + patchPath);
                    CopyFileIfRequired(fs.BasePath + patchPath + "/initfs_win32", modPath + patchPath + "/initfs_win32");
                }

            }

           

            return FrostyModsFound;
        }


        //private void GatherFrostbiteMods(string modPath, ref bool FrostyModsFound, ref ConcurrentDictionary<Stream, IFrostbiteMod> frostyMods)
        private void GatherFrostbiteMods(string modPath, ref bool FrostyModsFound, ref Dictionary<Stream, IFrostbiteMod> frostyMods)
        {
            FileInfo fileInfo = new FileInfo(modPath);
            Stream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);

            Stream modStream = null;
            Stream ms = new MemoryStream();
            var maxMemorySize = 512 * 1024 * 1024;
            //if (fs.Length < maxMemorySize)
            //{
            //    fs.CopyTo(ms);
            //    modStream = ms;
            //}
            //else
            //{
                modStream = fs;
            //}
            Logger.Log("Loading mod " + fileInfo.Name);
            if (modPath.Contains(".fbmod", StringComparison.OrdinalIgnoreCase))
            {
                FrostyModsFound = true;
                //frostyMods.TryAdd(ms, new FrostbiteMod(fs));
                //frostyMods.TryAdd(modStream, new FrostbiteMod(fs));
                frostyMods.Add(modStream, new FrostbiteMod(fs));
            }
            if (modPath.Contains(".fifamod", StringComparison.OrdinalIgnoreCase))
            {
                FrostyModsFound = true;
                //frostyMods.TryAdd(ms, new FIFAMod(string.Empty, fileInfo.FullName));
                //frostyMods.TryAdd(modStream, new FIFAMod(string.Empty, fileInfo.FullName));
                frostyMods.Add(modStream, new FIFAMod(string.Empty, fileInfo.FullName));
            }
        }

        public bool ForceRebuildOfMods = false;

        public static string ApplicationDirectory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            }
        }

        public string LastLaunchedModsPath
        {
            get
            {
                return fs.BasePath + "\\" + "FMT.LastLaunchedMods.json";
            }
        }

        public string LastPatchedVersionPath
        {
            get
            {
                return fs.BasePath + "\\" + "FMT.LastPatchedVersion.json";
            }
        }


        public static bool UseModData { get; set; } = true;

        public bool GameWasPatched { get; set; }

        public bool DeleteLiveUpdates { get; set; } = true;

        public bool LowMemoryMode { get; set; } = false;

        public async Task<bool> Run(ILogger inLogger, string gameRootPath, string modsRootPath, params string[] modPaths)
        {
            // -----------------------------------------------------------------------------------
            // Reset the GAME_DATA_DIR
            Environment.SetEnvironmentVariable("dataPath", "", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("GAME_DATA_DIR", "", EnvironmentVariableTarget.User);

            Logger = inLogger;

            if (FileSystem.Instance != null)
            {
                fs = FileSystem.Instance;
            }
            else
            {
                fs = new FileSystem(gameRootPath);
                fs.Initialize();
            }

            // -----------------------------------------------------------------------------------
            // Always uninstall InstallerData.xml change
            if (ProfileManager.IsFIFA23DataVersion())
            {
                ConfigureInstallerDataXml(false);
            }

            //string modPath = fs.BasePath + modDirName + "\\";
            string modPath = "\\" + modDirName + "\\";

            var foundMods = false;
            var lastModPaths = new Dictionary<string, DateTime>();
            if (File.Exists(LastLaunchedModsPath))
            {
                var LastLaunchedModsData = File.ReadAllText(LastLaunchedModsPath);
                lastModPaths = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(LastLaunchedModsData);
            }
            var sameCountAsLast = lastModPaths.Count == modPaths.Count();
            var sameAsLast = sameCountAsLast;// && lastModPaths.Equals(modPaths);
            if (sameCountAsLast)
            {
                foreach (FileInfo f in modPaths.Select(x => new FileInfo(x)))
                {
                    if (f.Exists)
                    {
                        if (f.Extension.Contains("fbmod", StringComparison.OrdinalIgnoreCase) || f.Extension.Contains("fifamod", StringComparison.OrdinalIgnoreCase))
                            foundMods = true;

                        if (lastModPaths.ContainsKey(f.FullName))
                        {
                            sameAsLast = (f.LastWriteTime == lastModPaths[f.FullName]);
                            if (!sameAsLast)
                                break;
                        }
                        else
                        {
                            sameAsLast = false;
                        }
                    }
                    else
                    {
                        sameAsLast = false;
                        break;
                    }
                }
            }
            else
            {
                sameAsLast = false;
            }

            // Delete the Live Updates
            RunDeleteLiveUpdates();

            // ---------------------------------------------
            // Load Last Patched Version
            uint? lastHead = null;
            var LastHeadData = new Dictionary<string, uint>();
            if (File.Exists(LastPatchedVersionPath))
            {
                LastHeadData = JsonConvert.DeserializeObject<Dictionary<string, uint>>(File.ReadAllText(LastPatchedVersionPath));
                if (LastHeadData.ContainsKey(fs.BasePath))
                {
                    lastHead = LastHeadData[fs.BasePath];
                }
            }

            //// Notify if new Patch detected
            if (fs.Head != lastHead)
            {
                Logger.Log("Detected New Version of " + ProfileManager.ProfileName + ".exe, rebuilding mods");
                // If new patch detected, force rebuild of mods
                sameAsLast = false;
                GameWasPatched = true;
                await Task.Delay(1000);
            }

            {
                // Notify if NO changes are made to mods
                if (sameAsLast && !ForceRebuildOfMods)
                {
                    Logger.Log("Detected NO changes in mods for " + ProfileManager.ProfileName + ".exe");
                    await Task.Delay(1000);
                }
                // Rebuild mods
                else
                {
                    foundMods = await BuildModData(fs, inLogger, modsRootPath, "", modPaths);
                    lastModPaths.Clear();
                    foreach (FileInfo f in modPaths.Select(x => new FileInfo(x)))
                    {
                        lastModPaths.Add(f.FullName, f.LastWriteTime);
                    }

                    // Save Last Launched Mods
                    File.WriteAllText(LastLaunchedModsPath, JsonConvert.SerializeObject(lastModPaths));
                    // ----------

                    // ---------------------------------------------
                    // Save Last Patched Version
                    lastHead = fs.Head;

                    if (LastHeadData.ContainsKey(fs.BasePath))
                        LastHeadData[fs.BasePath] = lastHead.Value;
                    else
                        LastHeadData.Add(fs.BasePath, lastHead.Value);

                    File.WriteAllText(LastPatchedVersionPath, JsonConvert.SerializeObject(LastHeadData));

                    // ---------------------------------------------

                }
            }

            //- -----------------------
            // Clear out the memory of archive data after compilation and before launching the game
            archiveData.Clear();

            RunFIFA23Setup();

            // Delete the Live Updates
            RunDeleteLiveUpdates();


            //RunSetupFIFAConfig();
            //RunPowershellToUnblockDLLAtLocation(fs.BasePath);
            var fifaconfigexelocation = fs.BasePath + "\\FIFASetup\\fifaconfig.exe";
            var fifaconfigexe_origlocation = fs.BasePath + "\\FIFASetup\\fifaconfig_orig.exe";
            FileInfo fIGameExe = new FileInfo(GameEXEPath);
            FileInfo fiFifaConfig = new FileInfo(Path.Combine(AppContext.BaseDirectory, "thirdparty", "fifaconfig.exe"));

            //if (ProfileManager.IsFIFA21DataVersion()
            //    || ProfileManager.IsFIFA22DataVersion()
            //    //|| ProfilesLibrary.IsFIFA23DataVersion()
            //    )
            //{
            //    CopyFileIfRequired("ThirdParty/CryptBase.dll", fs.BasePath + "CryptBase.dll");
            //}

            CopyFileIfRequired(fs.BasePath + "user.cfg", modPath + "user.cfg");
            if ((ProfileManager.IsFIFADataVersion()
                || ProfileManager.IsFIFA21DataVersion()
                || ProfileManager.IsFIFA22DataVersion())
                && UseModData)
            {
                if (!new FileInfo(fs.BasePath + "\\FIFASetup\\fifaconfig_orig.exe").Exists)
                {
                    FileInfo fileInfo10 = new FileInfo(fs.BasePath + "\\FIFASetup\\fifaconfig.exe");
                    fileInfo10.MoveTo(fileInfo10.FullName.Replace(".exe", "_orig.exe"));
                }
                CopyFileIfRequired("thirdparty/fifaconfig.exe", fs.BasePath + "\\FIFASetup\\fifaconfig.exe");
            }
            else if (ProfileManager.IsFIFA23DataVersion())
            {
                //var origPath = Path.Combine(fs.BasePath, ProfilesLibrary.ProfileName + "_orig.exe");
                //var fiOriginal = new FileInfo(origPath);
                //// if original exists but Game EXE has been patched
                //if (GameWasPatched && fiOriginal.Exists
                //    || (
                //    fiOriginal.Exists
                //    && fIGameExe.Length != fiFifaConfig.Length
                //    && fIGameExe.LastWriteTime > fiOriginal.LastWriteTime))
                //{
                //    fiOriginal.Delete();
                //}
                //else if (!fiOriginal.Exists)
                //{
                //    fIGameExe.MoveTo(fIGameExe.FullName.Replace(".exe", "_orig.exe"));
                //    CopyFileIfRequired(fiFifaConfig.FullName, GameEXEPath);
                //}
            }
            else if (new FileInfo(fifaconfigexe_origlocation).Exists)
            {
                File.Delete(fifaconfigexelocation); // delete the addon
                File.Move(fifaconfigexe_origlocation, fifaconfigexelocation); // replace
            }

            //if (foundMods && UseModData)// || sameAsLast)
            //{
            //    Logger.Log("Launching game: " + fs.BasePath + ProfilesLibrary.ProfileName + ".exe (with Frostbite Mods in ModData)");
            //    ExecuteProcess(fs.BasePath + ProfilesLibrary.ProfileName + ".exe", "-dataPath \"" + modPath.Trim('\\') + "\" " + "");
            //}
            //else 
            //var dataPathArgument = "-dataPath \"" + modPath.Trim('\\') + "\" " + "";
            var dataPathArgument = "-dataPath ModData";
            var fifaNonRetailArgument = "-FIFA.EnableLocalDiskAssetStream";
            var dataModulesPathArgument = "-dataModulesPath ModData";
            var noConfigArgument = "-noconfig";
            var arguments = dataPathArgument
                + " " + fifaNonRetailArgument
                + " " + dataModulesPathArgument
                + " " + noConfigArgument
                ;



            if (foundMods && !UseModData)
            {
                Logger.Log("Launching game: " + fs.BasePath + ProfileManager.ProfileName + ".exe (with Frostbite Mods)");
                ExecuteProcess(fs.BasePath + ProfileManager.ProfileName + ".exe", "");
            }
            else if (UseModData)
            {
                
                //if (ProfilesLibrary.IsFIFA23DataVersion())
                //{
                    if (EADesktopIsInstalled)
                    {
                        Logger.Log("Launching EADesktop. Please set the Advanced Launch Option to -dataPath ModData and launch the game through the App.");

                        RunEADesktop();
                        LaunchedViaEADesktop = true;

                        //https://answers.ea.com/t5/Bug-Reports-Technical-Issues/How-I-add-command-line-in-game-launch-option-like-in-old-version/m-p/11032763/highlight/true#M4972



                        // Other Alternative Route
                        //Logger.Log("[INSTALLED] You have installed your mods, please Start the Game via EA Desktop!");
                        //Logger.Log("[NOTE] To run mods, you must have -dataPath ModData in your Advanced Launch Options.");
                        //Logger.Log("[NOTE] Open EA Desktop, click My Collections, click the 3-dots, type -dataPath ModData in under Advanced Launch Options.");
                        //Logger.Log("[NOTE] This launcher will wait for you =)");


                        //var cmdParams = Uri.EscapeDataString(arguments);
                        //var cmdParams = Uri.EscapeDataString("-dataPath ModData"); // arguments;
                        //                                                           //Process.Start(new ProcessStartInfo($"origin2://game/launch/?offerIds={ProfilesLibrary.LoadedProfile.GameIdentifier}&cmdParams=-dataPath%20%22ModData%22")
                        //Process.Start(new ProcessStartInfo($"origin2://game/launch/?offerIds={ProfilesLibrary.LoadedProfile.GameIdentifier}" +
                        //    $"&cmdParams={cmdParams}")
                        //{
                        //    WorkingDirectory = fs.BasePath,
                        //    UseShellExecute = true,
                        //    Arguments = arguments,
                        //});
                    }
                    else
                    {
                        if (!foundMods)
                            Logger.Log("Launching game: " + fs.BasePath + ProfileManager.ProfileName + ".exe");
                        else
                            Logger.Log("Launching game: " + fs.BasePath + ProfileManager.ProfileName + ".exe (with Frostbite Mods in ModData)");

                        ExecuteProcess(GameEXEPath, arguments);
                    }
                //}
                //else
                //{
                //    if (!foundMods)
                //        Logger.Log("Launching game: " + fs.BasePath + ProfilesLibrary.ProfileName + ".exe");
                //    else
                //        Logger.Log("Launching game: " + fs.BasePath + ProfilesLibrary.ProfileName + ".exe (with Frostbite Mods in ModData)");

                //    ExecuteProcess(GameEXEPath, arguments);
                //}
            }
            else
            {
                Logger.Log("Launching game: " + fs.BasePath + ProfileManager.ProfileName + ".exe");
                ExecuteProcess(fs.BasePath + ProfileManager.ProfileName + ".exe", "");
            }

            if (UseACBypass && ProfileManager.IsFIFA23DataVersion())
            {
                var r = GameInstanceSingleton.InjectDLL(new FileInfo(@"ThirdParty\\FIFA23\\FIFA.dll").FullName, true).Result;
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // Run any Plugin defined cleanup operations
            //var pluginCompiler = AssetManager.LoadTypeFromPlugin2(ProfileManager.AssetCompilerName);
            //if (pluginCompiler == null && !string.IsNullOrEmpty(ProfileManager.AssetCompilerName))
            //    throw new NotImplementedException($"Could not find class {ProfileManager.AssetCompilerName} in any plugin! Remember this is case sensitive!!");

            //if (pluginCompiler != null)
            //{
            //    ((IAssetCompiler)pluginCompiler).Cleanup(fs, logger, this);
            //}

            return true;
        }

        private void RunEADesktop()
        {
            Process p = new();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/C start \"\" \"";
            p.StartInfo.Arguments += Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Electronic Arts\EA Desktop")?.GetValue("ClientPath")?.ToString(); ;
            p.StartInfo.Arguments += "\"";
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Electronic Arts\EA Desktop")?.GetValue("ClientPath")?.ToString());
            p.Start();
        }

        private void RunFIFA23Setup()
        {
            if (!ProfileManager.IsFIFA23DataVersion())
                return;

            // --------------------------------------------------------------
            // Unistall that crappy dxgi "fix" ------------------------------
            if (File.Exists(FileSystem.Instance.BasePath + "\\dxgi.dll"))
                File.Delete(FileSystem.Instance.BasePath + "\\dxgi.dll");

            // --------------------------------------------------------------
            // Cryptbase.dll no longer used ------------------------------
            if (File.Exists(fs.BasePath + "CryptBase.dll"))
                File.Delete(fs.BasePath + "CryptBase.dll");

            // --------------------------------------------------------------
            // 
            if (UseACBypass)
            {
                ConfigureInstallerDataXml(true);
            }
            else
            {
                ConfigureInstallerDataXml(false);
            }
        }

        private static void ConfigureInstallerDataXml(bool install = true)
        {
            var installerXmlPath = FileSystem.Instance.BasePath + "\\__Installer\\installerdata.xml";

            if (!File.Exists(installerXmlPath))
                throw new FileNotFoundException($"Unable to find installer data for {ProfileManager.DisplayName} at path {installerXmlPath}");

            if (install && !File.Exists(installerXmlPath + ".bak"))
                File.Copy(installerXmlPath, installerXmlPath + ".bak", false);
            // Uninstalling -------------------------------------------------
            else if (!install && File.Exists(installerXmlPath + ".bak"))
            {
                File.Copy(installerXmlPath + ".bak", installerXmlPath, true);
                File.Delete(installerXmlPath + ".bak");
            }

            // Load from file -----------------------------------------------
            XmlDocument xmldoc = new XmlDocument();
            using (FileStream fs = new FileStream(installerXmlPath, FileMode.Open, FileAccess.Read))
            {
                xmldoc.Load(fs);
            }
            XmlNode xmlnode = xmldoc.GetElementsByTagName("runtime").Item(0);
            var secondNode = xmlnode.ChildNodes.Item(1);

            // Installing ---------------------------------------------------
            if (install)
            {
                secondNode.InnerXml = secondNode.InnerXml.Replace("]EAAntiCheat.GameServiceLauncher", "]FIFA23", StringComparison.OrdinalIgnoreCase);
            }
            // Uninstalling -------------------------------------------------
            else
            {
                secondNode.InnerXml = secondNode.InnerXml.Replace("]FIFA23.exe", "]EAAntiCheat.GameServiceLauncher", StringComparison.OrdinalIgnoreCase);
            }
            // Save to file -------------------------------------------------
            using (FileStream fs = new FileStream(installerXmlPath, FileMode.Open, FileAccess.Write))
            {
                xmldoc.Save(fs);
            }
        }

        /// <summary>
        /// Deletes the Temporary folder with updates from EA in it
        /// </summary>
        private void RunDeleteLiveUpdates()
        {
            if (!DeleteLiveUpdates)
                return;

            try
            {
                if (ProfileManager.IsMadden20DataVersion() || ProfileManager.IsMadden21DataVersion(ProfileManager.Game))
                {
                    string path = Environment.ExpandEnvironmentVariables("%ProgramData%\\Frostbite\\Madden NFL 20");
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(path);
                        directoryInfo.Delete(true);
                    }

                    path = Environment.ExpandEnvironmentVariables("%ProgramData%\\Frostbite\\Madden NFL 21");
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(path);
                        // delete or throw???
                        directoryInfo.Delete(true);
                    }
                }

                var pathToFIFATempCacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", ProfileManager.DisplayName);
                if (Directory.Exists(pathToFIFATempCacheFolder))
                    Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", ProfileManager.DisplayName), recursive: true);
                
                Logger.Log("Successfully deleted the Live Updates folder.");
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR] Failed to delete Live Updates folder with message: {ex.Message}.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RunSetupFIFAConfig()
        {
            if (ProfileManager.IsFIFA21DataVersion())
            {
                var configIni = new FileInfo(fs.BasePath + "\\FIFASetup\\config.ini");
                if (configIni.Exists)
                {
                    StringBuilder newConfig = new StringBuilder();
                    newConfig.AppendLine("LAUNCH_EXE = fifa21.exe");
                    newConfig.AppendLine("SETTING_FOLDER = 'FIFA 21'");
                    newConfig.AppendLine("AUTO_LAUNCH = 1");
                    File.WriteAllText(configIni.FullName, newConfig.ToString());
                }

            }

            if (ProfileManager.IsFIFA22DataVersion())
            {
                var configIni = new FileInfo(fs.BasePath + "\\FIFASetup\\config.ini");
                if (configIni.Exists)
                {
                    StringBuilder newConfig = new StringBuilder();
                    newConfig.AppendLine($"LAUNCH_EXE = {ProfileManager.CacheName.ToLower()}.exe");
                    newConfig.AppendLine("SETTING_FOLDER = 'FIFA 22'");
                    newConfig.AppendLine("AUTO_LAUNCH = 1");
                    File.WriteAllText(configIni.FullName, newConfig.ToString());
                }


            }
        }

        //private void RecursiveDeleteFiles(string path)
        //{
        //    DirectoryInfo directoryInfo = new DirectoryInfo(path);
        //    DirectoryInfo[] directories = directoryInfo.GetDirectories();
        //    FileInfo[] files = directoryInfo.GetFiles();
        //    foreach (FileInfo fileInfo in files)
        //    {
        //        if ((fileInfo.Extension == ".cat" || fileInfo.Extension == ".toc" || fileInfo.Extension == ".sb" || fileInfo.Name.ToLower() == "mods.txt") && !(fileInfo.Name.ToLower() == "layout.toc"))
        //        {
        //            fileInfo.Delete();
        //        }
        //    }
        //    DirectoryInfo[] array = directories;
        //    foreach (DirectoryInfo directoryInfo2 in array)
        //    {
        //        string path2 = Path.Combine(path, directoryInfo2.Name);
        //        RecursiveDeleteFiles(path2);
        //    }
        //}

        public void ExecuteProcess(string processName, string args, bool waitForExit = false, bool asAdmin = false)
        {
            //Process p = new Process();
            //p.StartInfo.FileName = "cmd.exe";
            //p.StartInfo.Arguments = $"/K \"\"{processName}\" \"{args}\"\"";
            //p.Start();

            //using (Process process = new Process())
            //{
            //    FileInfo fileInfo = new FileInfo(processName);
            //    process.StartInfo.FileName = processName;
            //    process.StartInfo.WorkingDirectory = fileInfo.DirectoryName;
            //    process.StartInfo.Arguments = args;
            //    //process.StartInfo.UseShellExecute = false;
            //    //if (asAdmin)
            //    {
            //        process.StartInfo.UseShellExecute = true;
            //        process.StartInfo.Verb = "runas";
            //    }
            //    process.Start();
            //    if (waitForExit)
            //    {
            //        process.WaitForExit();
            //    }
            //}
            FileInfo fileInfo = new FileInfo(processName);
            Process.Start(new ProcessStartInfo
            {
                FileName = fileInfo.FullName,
                WorkingDirectory = fileInfo.DirectoryName,
                Arguments = args,
                UseShellExecute = false
            });
        }

        private byte[] GetResourceData(string modFilename, int archiveIndex, long offset, int size)
        {
            string path = modFilename.Replace(".fbmod", "_" + archiveIndex.ToString("D2") + ".archive");
            if (!File.Exists(path))
            {
                return null;
            }
            using (NativeReader nativeReader = new NativeReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
            {
                nativeReader.Position = offset;
                return nativeReader.ReadBytes(size);
            }
        }

        private byte[] GetResourceData(Stream stream, long offset, int size)
        {
            using (NativeReader nativeReader = new NativeReader(stream))
            {
                nativeReader.Position = offset;
                return nativeReader.ReadBytes(size);
            }
        }

        private void CopyFileIfRequired(string source, string dest)
        {
            FileInfo fileInfo = new FileInfo(source);
            FileInfo fileInfo2 = new FileInfo(dest);
            if (fileInfo.Exists && (!fileInfo2.Exists || (fileInfo2.Exists && fileInfo.LastWriteTimeUtc > fileInfo2.LastWriteTimeUtc) || fileInfo.Length != fileInfo2.Length))
            {
                File.Copy(fileInfo.FullName, fileInfo2.FullName, overwrite: true);
            }
        }

        public static async void RunPowershellToUnblockDLLAtLocation(string loc)
        {
            var psCommmand = $"dir \"{loc}\" -Recurse|Unblock-File";
            var psCommandBytes = System.Text.Encoding.Unicode.GetBytes(psCommmand);
            var psCommandBase64 = Convert.ToBase64String(psCommandBytes);

            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy unrestricted -WindowStyle hidden -EncodedCommand {psCommandBase64}",
                UseShellExecute = true
            };
            startInfo.Verb = "runAs";
            Process.Start(startInfo);

            await Task.Delay(9000);
        }
    }
}
