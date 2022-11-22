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
using FrostySdk.Frostbite.Compilers;

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

        public FileSystem fs;

        //public ResourceManager rm;

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

            //if (ResourceManager.Instance == null)
            //{
            //    rm = new ResourceManager(fs);
            //    rm.Initialize();
            //}
            //else
            //{
            //    rm = ResourceManager.Instance;
            //}


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
                    // -------------------------------------------------------------------------------------------------
                    // TODO: Remove
                    // 23.16 - This is now handled by the Launch Window, which extracts mods to a temp file to load in.
                    //if (f.Extension.Contains("zip", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    var z = f.FullName;

                    //    Logger.Log("Loading mods from " + z);

                    //    using (FileStream fsModZipped = new FileStream(z, FileMode.Open))
                    //    //FileStream fsModZipped = new FileStream(z, FileMode.Open);
                    //    {
                    //        ZipArchive zipArchive = new ZipArchive(fsModZipped);
                    //        foreach (var zaentr in zipArchive.Entries
                    //            .Where(x =>
                    //            x.FullName.Contains(".fbmod", StringComparison.OrdinalIgnoreCase)
                    //            || x.FullName.Contains(".fifamod", StringComparison.OrdinalIgnoreCase)))
                    //        {
                    //            Logger.Log("Loading mod " + zaentr.Name);
                    //            FrostyModsFound = true;
                    //            MemoryStream memoryStream = new MemoryStream();
                    //            if (zaentr.Length > 1024 * 1024 || zaentr.Name.Contains(".fifamod"))
                    //            {
                    //                if (!Directory.Exists(ApplicationDirectory + "TempMods"))
                    //                    Directory.CreateDirectory(ApplicationDirectory + "TempMods");
                    //                zaentr.ExtractToFile(ApplicationDirectory + "TempMods/" + zaentr.Name);
                    //                GatherFrostbiteMods(ApplicationDirectory + "TempMods/" + zaentr.Name, ref FrostyModsFound, ref frostyMods);
                    //            }
                    //            else
                    //            {
                    //                zaentr.Open().CopyTo(memoryStream);
                    //                frostyMods.TryAdd(new MemoryStream(memoryStream.ToArray()), new FrostbiteMod(new MemoryStream(memoryStream.ToArray())));
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                        ReadFrostbiteMods(rootPath + f, ref FrostyModsFound, ref frostyMods);
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

                            await File.WriteAllBytesAsync(exportedFilePath, resourceData);

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
                            case ModResourceType.Chunk:
                                Guid guid = new Guid(resource.Name);
                                if (ModifiedChunks.ContainsKey(guid))
                                {
                                    ModifiedChunks.Remove(guid);
                                }
                                ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
                                resource.FillAssetEntry(chunkAssetEntry);
                                chunkAssetEntry.Size = resourceData.Length;

                                // -------------------------------------------------------------------------
                                // If this is a legacy file here, it likely means its a *.fifamod
                                // Ignore this chunk and send to the Legacy Mod system
                                if (resource.IsLegacyFile && frostbiteMod is FIFAMod)
                                {
                                    // -------------------------------------------------------------------------------------
                                    // Remove the Chunk File Collector changes. This is done ourselves via the Legacy System
                                    if (resource.LegacyFullName.Contains("CFC", StringComparison.OrdinalIgnoreCase)
                                        || resource.LegacyFullName.Contains("Collector", StringComparison.OrdinalIgnoreCase)
                                        //||
                                        //    // -------------------------------------------------------------------------
                                        //    // 
                                        //    ProfileManager.CheckIsFIFA(ProfileManager.Game)
                                        //    && 
                                        //    (
                                        //        resource.LegacyFullName.Contains("player.lua", StringComparison.OrdinalIgnoreCase)
                                        //        || resource.LegacyFullName.Contains("player_kit.lua", StringComparison.OrdinalIgnoreCase)
                                        //    )
                                        )
                                        continue;

                                    // -------------------------------------------------------------------------
                                    // Create the Legacy Files from the Compressed Chunks
                                    LegacyFileEntry legacyAssetEntry = new LegacyFileEntry();
                                    legacyAssetEntry.Name = resource.LegacyFullName;
                                    legacyAssetEntry.Sha1 = resource.Sha1;
                                    // -------------------------------------------------------------------------
                                    // Decompress the Chunks back to their normal format
                                    var decompressedChunk = new CasReader(new MemoryStream(resourceData)).Read();
                                    legacyAssetEntry.ModifiedEntry = new FrostySdk.FrostbiteSdk.Managers.ModifiedLegacyAssetEntry() { Data = decompressedChunk };
                                    // -------------------------------------------------------------------------
                                    // Actual Size is the Decompressed Size
                                    legacyAssetEntry.Size = decompressedChunk.Length;

                                    if (!modifiedLegacy.ContainsKey(legacyAssetEntry.Name))
                                        modifiedLegacy.Add(legacyAssetEntry.Name, legacyAssetEntry);
                                    else
                                        modifiedLegacy[legacyAssetEntry.Name] = legacyAssetEntry;
                                }
                                else
                                {
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
                                break;
                        }

                        
                        //if (resource.Type == ModResourceType.Chunk)
                        //{
                        //    Guid guid = new Guid(resource.Name);
                        //    if (ModifiedChunks.ContainsKey(guid))
                        //    {
                        //        ModifiedChunks.Remove(guid);
                        //    }
                        //    ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
                        //    resource.FillAssetEntry(chunkAssetEntry);
                        //    chunkAssetEntry.Size = resourceData.Length;

                        //    ModifiedChunks.Add(guid, chunkAssetEntry);
                        //    if (!archiveData.ContainsKey(chunkAssetEntry.Sha1))
                        //    {
                        //        archiveData.TryAdd(chunkAssetEntry.Sha1, new ArchiveInfo
                        //        {
                        //            Data = resourceData,
                        //        });
                        //    }
                        //    else
                        //    {
                        //        archiveData[chunkAssetEntry.Sha1].Data = resourceData;
                        //    }
                        //}

                        //else 
                        if (resource.Type == ModResourceType.Legacy)
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
                    if(!((IAssetCompiler)pluginCompiler).Compile(fs, logger, this))
                    {
                        Logger.LogError("An error occurred within the Plugin Compiler. Stopping.");
                        return false;
                    }
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

        /// <summary>
        /// Constructs the IFrostbiteMod into the Dictionary
        /// </summary>
        /// <param name="modPath"></param>
        /// <param name="FrostyModsFound"></param>
        /// <param name="frostbiteMods"></param>
        private void ReadFrostbiteMods(string modPath, ref bool FrostyModsFound, ref Dictionary<Stream, IFrostbiteMod> frostbiteMods)
        {
            FileInfo fileInfo = new FileInfo(modPath);
            Stream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);

            Stream modStream = null;
            modStream = fs;

            Logger.Log("Loading mod " + fileInfo.Name);
            if (modPath.Contains(".fbmod", StringComparison.OrdinalIgnoreCase))
            {
                FrostyModsFound = true;
                frostbiteMods.Add(modStream, new FrostbiteMod(fs));
            }
            if (modPath.Contains(".fifamod", StringComparison.OrdinalIgnoreCase))
            {
                FrostyModsFound = true;
                frostbiteMods.Add(modStream, new FIFAMod(string.Empty, fileInfo.FullName));
            }
        }

        public bool ForceRebuildOfMods = false;

        public static string ApplicationDirectory
        {
            get
            {
                //return System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                return AppContext.BaseDirectory;
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
                if (EADesktopIsInstalled)
                {
                    Logger.Log("Launching EADesktop. Please set the Advanced Launch Option to -dataPath ModData and launch the game through the App.");

                    RunEADesktop();
                    LaunchedViaEADesktop = true;
                }
                else
                {
                    if (!foundMods)
                        Logger.Log("Launching game: " + fs.BasePath + ProfileManager.ProfileName + ".exe");
                    else
                        Logger.Log("Launching game: " + fs.BasePath + ProfileManager.ProfileName + ".exe (with Frostbite Mods in ModData)");

                    ExecuteProcess(GameEXEPath, arguments);
                }
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
                secondNode.InnerXml = secondNode.InnerXml.Replace("]EAAntiCheat.GameServiceLauncher.exe", "]FIFA23.exe", StringComparison.OrdinalIgnoreCase);
            }
            // Uninstalling -------------------------------------------------
            else
            {
                secondNode.InnerXml = secondNode.InnerXml.Replace("]FIFA23.exe", "]EAAntiCheat.GameServiceLauncher.exe", StringComparison.OrdinalIgnoreCase);
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
