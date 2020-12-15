
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FIFA21Plugin.Plugin2.Handlers;
using Modding.Utilities;
using FrostySdk;
using Frosty.Hash;
using FrostySdk.IO;
//using FrostySdk.Legacy;
using FrostySdk.Managers;
using Frostbite.FileManagers;
using FrostySdk.Frostbite;
using FrostySdk.Interfaces;
using static paulv2k4ModdingExecuter.FrostyModExecutor;

namespace FIFA21Plugin.Plugin2.Execution
{
    public class Plugin2Executer
    {
        private class Fifa21PreAction
        {
            private Exception errorException;

            private Plugin2.Execution.Plugin2Executer parent;

            private string superBundle;

            private Catalog catalog;


            public static ConcurrentDictionary<Guid, object> ChunksToFind
            {
                get;
                set;
            }

            public static ConcurrentDictionary<Guid, (bool patch, MemoryStream data)> Results
            {
                get;
            } = new ConcurrentDictionary<Guid, (bool, MemoryStream)>();


            public string SuperBundle => superBundle;

            public bool HasErrored => errorException != null;

            public Exception Exception => errorException;

            public Fifa21PreAction(string superBundle, ManualResetEvent doneEvent, Plugin2Executer parent)
            {
                this.superBundle = superBundle ?? throw new ArgumentNullException("superBundle");
                this.parent = parent ?? throw new ArgumentNullException("parent");
                catalog = this.parent.fs.GetCatalogObjectFromSuperBundle(this.superBundle);
            }

            public void Run()
            {
                try
                {
                    if (ChunksToFind.IsEmpty)
                    {
                        return;
                    }
                    string patchTocPath = parent.fs.ResolvePath("native_patch/" + superBundle + ".toc");
                    patchTocPath.Replace("Patch\\Win32", Path.Combine(parent.modDataPath, "Patch", "Win32"));
                    if (!string.IsNullOrEmpty(patchTocPath))
                    {
                        using (FileStream patchTocStream = new FileStream(patchTocPath, FileMode.Open, FileAccess.Read))
                        {
                            TocFile_F21 patchTocFile = new NewTocReader_F21().Read(patchTocStream);
                            string patchSuperBundlePath = parent.fs.ResolvePath("native_patch/" + superBundle + ".sb");
                            patchSuperBundlePath.Replace("Patch\\win32", Path.Combine(parent.modDataPath, "Patch", "Win32"));
                            object value;
                            for (int l = 0; l < patchTocFile.orderedChunkGuids.Count; l++)
                            {
                                Guid guid2 = patchTocFile.orderedChunkGuids[l].id;
                                if (ChunksToFind.TryRemove(guid2, out value))
                                {
                                    MemoryStream resourceData3 = parent.rm.GetResourceData(parent.fs.GetCasFilePath(patchTocFile.Chunks[l].catalog, patchTocFile.Chunks[l].cas, patchTocFile.Chunks[l].patch), patchTocFile.Chunks[l].offset, patchTocFile.Chunks[l].size);
                                    Results[guid2] = (true, resourceData3);
                                }
                            }
                            for (int k = 0; k < patchTocFile.Bundles.Count; k++)
                            {
                                (int, int, long) bundle2 = patchTocFile.Bundles[k];
                                if (ChunksToFind.IsEmpty)
                                {
                                    return;
                                }
                                Stream bundleDataStream2 = GetBundleDataStream(patchSuperBundlePath, bundle2.Item3, bundle2.Item2);
                                if (bundleDataStream2 == null)
                                {
                                    continue;
                                }
                                foreach (DbObject chunk2 in new SuperBundleReader_F21().Read(bundleDataStream2).GetValue<DbObject>("chunks").List.Cast<DbObject>())
                                {
                                    Guid chunkId2 = chunk2.GetValue<Guid>("id");
                                    if (ChunksToFind.TryRemove(chunkId2, out value))
                                    {
                                        MemoryStream resourceData4 = parent.rm.GetResourceData(parent.fs.GetCasFilePath(chunk2.GetValue<int>("catalog"), chunk2.GetValue<int>("cas"), chunk2.HasValue("patch")), chunk2.GetValue<int>("offset"), chunk2.GetValue<int>("size"));
                                        Results[chunkId2] = (true, resourceData4);
                                    }
                                }
                            }
                        }
                    }
                    string text = parent.fs.ResolvePath("native_data/" + superBundle + ".toc");
                    text.Replace("Data\\Win32", Path.Combine(parent.modDataPath, "Data", "Win32"));
                    using (FileStream baseTocStream = new FileStream(text, FileMode.Open, FileAccess.Read))
                    {
                        TocFile_F21 baseTocFile = new NewTocReader_F21().Read(baseTocStream);
                        parent.fs.ResolvePath("native_data/" + superBundle + ".sb");
                        for (int j = 0; j < baseTocFile.orderedChunkGuids.Count; j++)
                        {
                            Guid guid = baseTocFile.orderedChunkGuids[j].id;
                            if (ChunksToFind.ContainsKey(guid))
                            {
                                MemoryStream resourceData = parent.rm.GetResourceData(parent.fs.GetCasFilePath(baseTocFile.Chunks[j].catalog, baseTocFile.Chunks[j].cas, baseTocFile.Chunks[j].patch), baseTocFile.Chunks[j].offset, baseTocFile.Chunks[j].size);
                                Results[guid] = (false, resourceData);
                            }
                        }
                        for (int i = 0; i < baseTocFile.CasBundles.Count; i++)
                        {
                            TocFile_F21.CasBundle bundle = baseTocFile.CasBundles[i];
                            if (ChunksToFind.IsEmpty)
                            {
                                break;
                            }
                            Stream bundleDataStream = GetBundleDataStream(bundle.InPatch, bundle.CasCatalog, bundle.CasIndex, bundle.BundleOffset, bundle.BundleLength);
                            if (bundleDataStream == null)
                            {
                                continue;
                            }
                            foreach (DbObject chunk in new TocBundleReader_F21().Read(bundleDataStream, bundle.Entries).GetValue<DbObject>("chunks").List.Cast<DbObject>())
                            {
                                Guid chunkId = chunk.GetValue<Guid>("id");
                                if (ChunksToFind.ContainsKey(chunkId))
                                {
                                    MemoryStream resourceData2 = parent.rm.GetResourceData(parent.fs.GetCasFilePath(chunk.GetValue<int>("catalog"), chunk.GetValue<int>("cas"), chunk.HasValue("patch")), chunk.GetValue<int>("offset"), chunk.GetValue<int>("size"));
                                    Results[chunkId] = (false, resourceData2);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex2)
                {
                    Exception ex = (errorException = ex2);
                }
            }

            private Stream GetBundleDataStream(bool patch, int catalog, int casIndex, long bundleOffset, int bundleLength)
            {
                using (FileStream casStream = new FileStream(parent.fs.ResolvePath(parent.fs.GetCasFilePath(catalog, casIndex, patch)), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (casStream.Length == 0L)
                    {
                        return null;
                    }
                    MemoryStream bundleData = new MemoryStream(bundleLength - 4);
                    casStream.Position = bundleOffset + 4;
                    casStream.CopyTo(bundleData, bundleLength - 4);
                    bundleData.Position = 0L;
                    return bundleData;
                }
            }


            private Stream GetBundleDataStream(string superBundlePath, long bundleOffset, int bundleLength)
            {
                using (FileStream sbStream = new FileStream(superBundlePath, FileMode.Open, FileAccess.Read))
                {
                    if (sbStream.Length == 0L)
                    {
                        return null;
                    }
                    MemoryStream bundleData = new MemoryStream(bundleLength);
                    sbStream.Position = bundleOffset;
                    sbStream.CopyTo(bundleData, bundleLength);
                    bundleData.Position = 0L;

                    return bundleData;
                }
            }
        }

        private class Fifa21BundleAction
        {
            public static Dictionary<string, (int catalog, int casFileCount)> CasFiles = new Dictionary<string, (int, int)>();

            private static readonly ConcurrentDictionary<Sha1, (int catalog, int cas, long offset, long size)> casModifications = new ConcurrentDictionary<Sha1, (int, int, long, long)>();

            private static readonly object locker = new object();

            private Exception errorException;

            private ManualResetEvent doneEvent;

            private Plugin2Executer parent;

            private string superBundle;

            private Catalog catalog;

            private NativeWriter modCasWriter2;

            public static ConcurrentDictionary<string, EbxAssetEntry> ModifiedEbxAssets
            {
                get;
                set;
            }

            public static ConcurrentDictionary<string, ResAssetEntry> ModifiedResAssets
            {
                get;
                set;
            }

            public static ConcurrentDictionary<Guid, ChunkAssetEntry> ModifiedChunkAssets
            {
                get;
                set;
            }

            public string SuperBundle => superBundle;

            public bool BaseTocModified
            {
                get;
                set;
            }

            public bool PatchTocModified
            {
                get;
                set;
            }

            public bool PatchSbModified
            {
                get;
                set;
            }

            public bool HasErrored => errorException != null;

            public Exception Exception => errorException;

            public Fifa21BundleAction(string superBundle, ManualResetEvent doneEvent, Plugin2Executer parent)
            {
                this.superBundle = superBundle ?? throw new ArgumentNullException("superBundle");
                this.parent = parent ?? throw new ArgumentNullException("parent");
                this.doneEvent = doneEvent ?? throw new ArgumentNullException("doneEvent");
                catalog = this.parent.fs.GetCatalogObjectFromSuperBundle(this.superBundle);
            }

            public void Run()
            {
                NativeWriter modCasWriter = null;
                int casFileIndex = 0;
                try
                {
                    string patchTocPath = parent.fs.ResolvePath("native_patch/" + superBundle + ".toc");
                    string modDataPatchTocPath = patchTocPath.ToLower().Replace(Path.Combine("patch", "win32"), Path.Combine(parent.modDataPath, "patch", "win32"));
                    if (!string.IsNullOrEmpty(patchTocPath))
                    {
                        parent.Logger.Log(patchTocPath);
                        using (FileStream patchTocStream = new FileStream(patchTocPath, FileMode.Open, FileAccess.Read))
                        {
                            TocFile_F21 patchTocFile = new NewTocReader_F21().Read(patchTocStream);
                            string patchSuperBundlePath = parent.fs.ResolvePath("native_patch/" + superBundle + ".sb");
                            string modDataPatchSuperBundlePath = patchSuperBundlePath.ToLower().Replace(Path.Combine("patch", "win32"), Path.Combine(parent.modDataPath, "patch", "win32"));
                            for (int l = 0; l < patchTocFile.orderedChunkGuids.Count; l++)
                            {
                                Guid guid2 = patchTocFile.orderedChunkGuids[l].id;
                                if (ModifiedChunkAssets.TryGetValue(guid2, out var modifiedEntry8))
                                {
                                    _ = modifiedEntry8.IsTocChunk;
                                    byte[] entryData5;
                                    if (modifiedEntry8.ExtraData != null)
                                    {
                                        HandlerExtraData handlerExtraData2 = (HandlerExtraData)modifiedEntry8.ExtraData;
                                        Stream resourceData2 = parent.rm.GetResourceData(parent.fs.GetCasFilePath(patchTocFile.Chunks[l].catalog, patchTocFile.Chunks[l].cas, patchTocFile.Chunks[l].patch), patchTocFile.Chunks[l].offset, patchTocFile.Chunks[l].size);
                                        modifiedEntry8 = (ChunkAssetEntry)handlerExtraData2.Handler.Modify(modifiedEntry8, resourceData2, handlerExtraData2.Data, out entryData5);
                                    }
                                    else
                                    {
                                        entryData5 = parent.archiveData[modifiedEntry8.Sha1].Data;
                                    }
                                    if (modCasWriter == null || modCasWriter.BaseStream.Length + modifiedEntry8.Size > CasFile.MaxCasSize)
                                    {
                                        modCasWriter?.Dispose();
                                        (modCasWriter, casFileIndex) = GetNextCas();
                                    }
                                    patchTocFile.Chunks[l] = (patchTocFile.Chunks[l].unk, true, (byte)catalog.Index.Value, (byte)casFileIndex, (uint)modCasWriter.Position, (uint)modifiedEntry8.Size);
                                    modCasWriter.WriteBytes(entryData5);
                                    PatchTocModified = true;
                                }
                            }
                            for (int k = 0; k < patchTocFile.Bundles.Count; k++)
                            {
                                (int, int, long) bundle2 = patchTocFile.Bundles[k];
                                //parent.Logger.Log(bundle2.ToString());

                                bool bundleModified2 = false;
                                Stream bundleDataStream2 = GetBundleDataStream(patchSuperBundlePath, bundle2.Item3, bundle2.Item2);
                                if (bundleDataStream2 == null|| bundleDataStream2.Length == 0)
                                {
                                    continue;
                                }
                                bundleDataStream2.Position = 0;
                                if(bundle2.Item1 == 1547)
                                {

                                }
                                DbObject parsedBundle2 = new SuperBundleReader_F21().Read(bundleDataStream2);
                                foreach (DbObject item in parsedBundle2.GetValue<DbObject>("ebx").List.Cast<DbObject>())
                                {
                                    string ebxName2 = item.GetValue<string>("name");
                                    if (ModifiedEbxAssets.TryGetValue(ebxName2, out var _))
                                    {
                                        bundleModified2 = true;
                                    }
                                }
                                foreach (DbObject item2 in parsedBundle2.GetValue<DbObject>("res").List.Cast<DbObject>())
                                {
                                    string resName2 = item2.GetValue<string>("name");
                                    if (ModifiedResAssets.TryGetValue(resName2, out var _))
                                    {
                                        bundleModified2 = true;
                                    }
                                }
                                foreach (DbObject item3 in parsedBundle2.GetValue<DbObject>("chunks").List.Cast<DbObject>())
                                {
                                    Guid chunkId2 = item3.GetValue<Guid>("id");
                                    if (ModifiedChunkAssets.TryGetValue(chunkId2, out var _))
                                    {
                                        bundleModified2 = true;
                                    }
                                }
                                if (!bundleModified2)
                                {
                                    continue;
                                }
                                foreach (DbObject ebx2 in parsedBundle2.GetValue<DbObject>("ebx").List.Cast<DbObject>())
                                {
                                    if (modCasWriter == null)
                                    {
                                        modCasWriter?.Dispose();
                                        (modCasWriter, casFileIndex) = GetNextCas();
                                    }
                                    long newOffset6 = modCasWriter.Position;
                                    if (ModifiedEbxAssets.TryGetValue(ebx2.GetValue<string>("name"), out var modifiedEntry11))
                                    {
                                        byte[] entryData8 = parent.archiveData[modifiedEntry11.Sha1].Data;
                                        ebx2.SetValue("sha1", modifiedEntry11.Sha1);
                                        ebx2.SetValue("originalSize", modifiedEntry11.OriginalSize);
                                        ebx2.SetValue("size", modifiedEntry11.Size);
                                        modCasWriter.WriteBytes(entryData8);
                                    }
                                    else
                                    {
                                        CopyEntryToNewCas(modCasWriter.BaseStream, ebx2.GetValue<int>("catalog"), ebx2.GetValue<int>("cas"), ebx2.HasValue("patch"), ebx2.GetValue<int>("offset"), (int)ebx2.GetValue<long>("size"));
                                    }
                                    ebx2.SetValue("catalog", catalog.Index.Value);
                                    ebx2.SetValue("offset", (int)newOffset6);
                                    ebx2.SetValue("cas", casFileIndex);
                                    ebx2.SetValue("patch", true);
                                }
                                foreach (DbObject res2 in parsedBundle2.GetValue<DbObject>("res").List.Cast<DbObject>())
                                {
                                    if (modCasWriter == null)
                                    {
                                        modCasWriter?.Close();
                                        (modCasWriter, casFileIndex) = GetNextCas();
                                    }
                                    long newOffset5 = modCasWriter.Position;
                                    if (ModifiedResAssets.TryGetValue(res2.GetValue<string>("name"), out var modifiedEntry10))
                                    {
                                        byte[] entryData7 = parent.archiveData[modifiedEntry10.Sha1].Data;
                                        res2.SetValue("sha1", modifiedEntry10.Sha1);
                                        res2.SetValue("originalSize", modifiedEntry10.OriginalSize);
                                        res2.SetValue("size", modifiedEntry10.Size);
                                        res2.SetValue("resRid", (long)modifiedEntry10.ResRid);
                                        res2.SetValue("resMeta", modifiedEntry10.ResMeta);
                                        res2.SetValue("resType", modifiedEntry10.ResType);
                                        modCasWriter.WriteBytes(entryData7);
                                    }
                                    else
                                    {
                                        CopyEntryToNewCas(modCasWriter.BaseStream, res2.GetValue<int>("catalog"), res2.GetValue<int>("cas"), res2.HasValue("patch"), res2.GetValue<int>("offset"), (int)res2.GetValue<long>("size"));
                                    }
                                    res2.SetValue("catalog", catalog.Index.Value);
                                    res2.SetValue("offset", (int)newOffset5);
                                    res2.SetValue("cas", casFileIndex);
                                    res2.SetValue("patch", true);
                                }
                                foreach (DbObject chunk2 in parsedBundle2.GetValue<DbObject>("chunks").List.Cast<DbObject>())
                                {
                                    if (modCasWriter == null)
                                    {
                                        modCasWriter?.Close();
                                        (modCasWriter, casFileIndex) = GetNextCas();
                                    }
                                    long newOffset4 = modCasWriter.Position;
                                    if (ModifiedChunkAssets.TryGetValue(chunk2.GetValue<Guid>("id"), out var modifiedEntry9))
                                    {
                                        byte[] entryData6 = parent.archiveData[modifiedEntry9.Sha1].Data;
                                        chunk2.SetValue("sha1", modifiedEntry9.Sha1);
                                        chunk2.SetValue("originalSize", modifiedEntry9.OriginalSize);
                                        chunk2.SetValue("size", modifiedEntry9.Size);
                                        chunk2.SetValue("logicalOffset", modifiedEntry9.LogicalOffset);
                                        chunk2.SetValue("logicalSize", modifiedEntry9.LogicalSize);
                                        modCasWriter.WriteBytes(entryData6);
                                    }
                                    else
                                    {
                                        CopyEntryToNewCas(modCasWriter.BaseStream, chunk2.GetValue<int>("catalog"), chunk2.GetValue<int>("cas"), chunk2.HasValue("patch"), chunk2.GetValue<int>("offset"), (int)chunk2.GetValue<long>("size"));
                                    }
                                    chunk2.SetValue("catalog", catalog.Index.Value);
                                    chunk2.SetValue("offset", (int)newOffset4);
                                    chunk2.SetValue("cas", casFileIndex);
                                    chunk2.SetValue("patch", true);
                                }
                                try
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(modDataPatchTocPath));
                                    File.Copy(patchSuperBundlePath, modDataPatchSuperBundlePath, true);
                                }
                                catch (IOException)
                                {
                                }
                                using (FileStream sbFileStream = new FileStream(modDataPatchSuperBundlePath, FileMode.OpenOrCreate, FileAccess.Write))
                                {
                                    sbFileStream.Position = sbFileStream.Length;
                                    long bundleOffset2 = sbFileStream.Position;
                                    new SuperBundleWriter_F21().Write(sbFileStream, parsedBundle2);
                                    int bundleLength2 = (int)(sbFileStream.Position - bundleOffset2);
                                    patchTocFile.Bundles[k] = (bundle2.Item1, bundleLength2, bundleOffset2);
                                    PatchTocModified = true;
                                    PatchSbModified = true;
                                }
                            }
                            if (PatchTocModified)
                            {
                                new TocWriter_F21().Write(modDataPatchTocPath, patchTocFile, writeHeader: true);
                            }
                        }
                    }
                    string text = parent.fs.ResolvePath("native_data/" + superBundle + ".toc");
                    string modDataBaseTocPath = text.ToLower().Replace(Path.Combine("data", "win32"), Path.Combine(parent.modDataPath, "data", "win32"));
                    using (FileStream baseTocStream = new FileStream(text, FileMode.Open, FileAccess.Read))
                    {
                        TocFile_F21 baseTocFile = new NewTocReader_F21().Read(baseTocStream);
                        var baseSbPath = parent.fs.ResolvePath("native_data/" + superBundle + ".sb");
                        for (int j = 0; j < baseTocFile.orderedChunkGuids.Count; j++)
                        {
                            Guid guid = baseTocFile.orderedChunkGuids[j].id;
                            if (ModifiedChunkAssets.TryGetValue(guid, out var modifiedEntry))
                            {
                                _ = modifiedEntry.IsTocChunk;
                                byte[] entryData;
                                if (modifiedEntry.ExtraData != null)
                                {
                                    HandlerExtraData handlerExtraData = (HandlerExtraData)modifiedEntry.ExtraData;
                                    Stream resourceData = parent.rm.GetResourceData(parent.fs.GetCasFilePath(baseTocFile.Chunks[j].catalog, baseTocFile.Chunks[j].cas, baseTocFile.Chunks[j].patch), baseTocFile.Chunks[j].offset, baseTocFile.Chunks[j].size);
                                    modifiedEntry = (ChunkAssetEntry)handlerExtraData.Handler.Modify(modifiedEntry, resourceData, handlerExtraData.Data, out entryData);
                                }
                                else
                                {
                                    entryData = parent.archiveData[modifiedEntry.Sha1].Data;
                                }
                                if (modCasWriter == null || modCasWriter.BaseStream.Length + modifiedEntry.Size > CasFile.MaxCasSize)
                                {
                                    modCasWriter?.Dispose();
                                    (modCasWriter, casFileIndex) = GetNextCas();
                                }
                                baseTocFile.Chunks[j] = (baseTocFile.Chunks[j].unk, true, (byte)catalog.Index.Value, (byte)casFileIndex, (uint)modCasWriter.Position, (uint)modifiedEntry.Size);
                                modCasWriter.WriteBytes(entryData);
                                BaseTocModified = true;
                            }
                        }
                        for (int i = 0; i < baseTocFile.CasBundles.Count; i++)
                        {
                            if (ModifiedEbxAssets.IsEmpty && ModifiedResAssets.IsEmpty && ModifiedChunkAssets.IsEmpty)
                            {
                                break;
                            }
                            bool bundleModified = false;
                            TocFile_F21.CasBundle bundle = baseTocFile.CasBundles[i];
                            Stream bundleDataStream = GetBundleDataStream(bundle.InPatch, bundle.CasCatalog, bundle.CasIndex, bundle.BundleOffset, bundle.BundleLength);
                            if (bundleDataStream == null || bundleDataStream.Length == 0)
                            {
                                continue;
                            }
                            DbObject parsedBundle = new TocBundleReader_F21().Read(bundleDataStream, bundle.Entries);
                            foreach (DbObject item4 in parsedBundle.GetValue<DbObject>("ebx").List.Cast<DbObject>())
                            {
                                string ebxName = item4.GetValue<string>("name");
                                if (ModifiedEbxAssets.TryGetValue(ebxName, out var _))
                                {
                                    BaseTocModified = true;
                                    bundleModified = true;
                                }
                            }
                            foreach (DbObject item5 in parsedBundle.GetValue<DbObject>("res").List.Cast<DbObject>())
                            {
                                string resName = item5.GetValue<string>("name");
                                if (ModifiedResAssets.TryGetValue(resName, out var _))
                                {
                                    BaseTocModified = true;
                                    bundleModified = true;
                                }
                            }
                            foreach (DbObject item6 in parsedBundle.GetValue<DbObject>("chunks").List.Cast<DbObject>())
                            {
                                Guid chunkId = item6.GetValue<Guid>("id");
                                if (ModifiedChunkAssets.TryGetValue(chunkId, out var _))
                                {
                                    BaseTocModified = true;
                                    bundleModified = true;
                                }
                            }
                            if (!bundleModified)
                            {
                                continue;
                            }
                            if (modCasWriter == null)
                            {
                                modCasWriter?.Close();
                                (modCasWriter, casFileIndex) = GetNextCas();
                            }
                            foreach (DbObject ebx in parsedBundle.GetValue<DbObject>("ebx").List.Cast<DbObject>())
                            {
                                TocFile_F21.CasBundleEntry bundleEntry3 = ebx.GetValue<TocFile_F21.CasBundleEntry>("bundleEntry");
                                long newOffset3 = modCasWriter.Position;
                                if (ModifiedEbxAssets.TryGetValue(ebx.GetValue<string>("name"), out var modifiedEntry4))
                                {
                                    byte[] entryData4 = parent.archiveData[modifiedEntry4.Sha1].Data;
                                    ebx.SetValue("sha1", modifiedEntry4.Sha1);
                                    ebx.SetValue("originalSize", modifiedEntry4.OriginalSize);
                                    bundleEntry3.EntrySize = (int)modifiedEntry4.Size;
                                    modCasWriter.WriteBytes(entryData4);
                                }
                                else
                                {
                                    CopyEntryToNewCas(modCasWriter.BaseStream, bundleEntry3);
                                }
                                bundleEntry3.CasCatalog = catalog.Index.Value;
                                bundleEntry3.EntryOffset = (int)newOffset3;
                                bundleEntry3.CasIndex = casFileIndex;
                                bundleEntry3.InPatch = true;
                            }
                            foreach (DbObject res in parsedBundle.GetValue<DbObject>("res").List.Cast<DbObject>())
                            {
                                TocFile_F21.CasBundleEntry bundleEntry2 = res.GetValue<TocFile_F21.CasBundleEntry>("bundleEntry");
                                long newOffset2 = modCasWriter.Position;
                                if (ModifiedResAssets.TryGetValue(res.GetValue<string>("name"), out var modifiedEntry3))
                                {
                                    byte[] entryData3 = parent.archiveData[modifiedEntry3.Sha1].Data;
                                    res.SetValue("sha1", modifiedEntry3.Sha1);
                                    res.SetValue("originalSize", modifiedEntry3.OriginalSize);
                                    res.SetValue("resRid", (long)modifiedEntry3.ResRid);
                                    res.SetValue("resMeta", modifiedEntry3.ResMeta);
                                    res.SetValue("resType", modifiedEntry3.ResType);
                                    bundleEntry2.EntrySize = (int)modifiedEntry3.Size;
                                    modCasWriter.WriteBytes(entryData3);
                                }
                                else
                                {
                                    CopyEntryToNewCas(modCasWriter.BaseStream, bundleEntry2);
                                }
                                bundleEntry2.CasCatalog = catalog.Index.Value;
                                bundleEntry2.EntryOffset = (int)newOffset2;
                                bundleEntry2.CasIndex = casFileIndex;
                                bundleEntry2.InPatch = true;
                            }
                            foreach (DbObject chunk in parsedBundle.GetValue<DbObject>("chunks").List.Cast<DbObject>())
                            {
                                TocFile_F21.CasBundleEntry bundleEntry = chunk.GetValue<TocFile_F21.CasBundleEntry>("bundleEntry");
                                long newOffset = modCasWriter.Position;
                                if (ModifiedChunkAssets.TryGetValue(chunk.GetValue<Guid>("id"), out var modifiedEntry2))
                                {
                                    byte[] entryData2 = parent.archiveData[modifiedEntry2.Sha1].Data;
                                    chunk.SetValue("sha1", modifiedEntry2.Sha1);
                                    chunk.SetValue("originalSize", modifiedEntry2.OriginalSize);
                                    chunk.SetValue("logicalOffset", modifiedEntry2.LogicalOffset);
                                    chunk.SetValue("logicalSize", modifiedEntry2.LogicalSize);
                                    bundleEntry.EntrySize = (int)modifiedEntry2.Size;
                                    modCasWriter.WriteBytes(entryData2);
                                }
                                else
                                {
                                    CopyEntryToNewCas(modCasWriter.BaseStream, bundleEntry);
                                }
                                bundleEntry.CasCatalog = catalog.Index.Value;
                                bundleEntry.EntryOffset = (int)newOffset;
                                bundleEntry.CasIndex = casFileIndex;
                                bundleEntry.InPatch = true;
                            }
                            long sizeOffset = modCasWriter.Position;
                            modCasWriter.WriteInt32BigEndian(0);
                            BundleWriter_F21 bundleWriter_F = new BundleWriter_F21();
                            long bundleOffset = modCasWriter.Position;
                            bundleWriter_F.Write(modCasWriter.BaseStream, parsedBundle);
                            int bundleLength = (int)(modCasWriter.Position - bundleOffset);
                            modCasWriter.Position = sizeOffset;
                            modCasWriter.WriteInt32BigEndian(bundleLength);
                            modCasWriter.Position = bundleOffset + bundleLength;
                            bundle.BundleLength = bundleLength + 4;
                            bundle.BundleOffset = sizeOffset;
                            bundle.CasIndex = casFileIndex;
                            bundle.InPatch = true;
                        }
                        if (BaseTocModified)
                        {
                            new TocWriter_F21().Write(modDataBaseTocPath, baseTocFile, writeHeader: true);
                        }
                    }
                }
                catch (Exception ex3)
                {
                    Exception ex = (errorException = ex3);
                }
                finally
                {
                    modCasWriter?.Dispose();
                }
            }

            public void Execute()
            {
                try
                {
                    Run();
                }
                finally
                {
                    //if (Interlocked.Decrement(ref parent.numTasks) == 0)
                    //{
                    //    doneEvent.Set();
                    //}
                }
            }

            private void CopyEntryToNewCas(Stream newCasStream, TocFile_F21.CasBundleEntry bundleEntry)
            {
                CopyEntryToNewCas(newCasStream, bundleEntry.CasCatalog, bundleEntry.CasIndex, bundleEntry.InPatch, bundleEntry.EntryOffset, bundleEntry.EntrySize);
            }

            private void CopyEntryToNewCas(Stream newCasStream, int catalog, int casIndex, bool inPatch, long offset, int size)
            {
                var casPath = parent.fs.GetFilePath(catalog, casIndex, inPatch);
                var resolvedPath = parent.fs.ResolvePath(casPath);
                using (NativeWriter nw = new NativeWriter(new FileStream(resolvedPath, FileMode.Open)))
                {
                    nw.Position = offset;
                    var data = new NativeReader(newCasStream).ReadToEnd();
                    nw.Write(data);
                }
            }

            private MemoryStream GetBundleDataStream(bool patch, int catalog, int casIndex, long bundleOffset, int bundleLength)
            {
                var casPath = parent.fs.GetFilePath(catalog, casIndex, patch);
                var resolvedPath = parent.fs.ResolvePath(casPath);
                using (NativeReader nr = new NativeReader(new FileStream(resolvedPath, FileMode.Open)))
                {
                    if (nr.Length == 0L)
                        return null;

                    nr.Position = bundleOffset + 4;
                    var data = nr.ReadBytes(bundleLength - 4);
                    return new MemoryStream(data);
                }
                //using (FileStream casStream = new FileStream(, FileMode.Open, FileAccess.Read))
                //{

                //    if (casStream.Length == 0L)
                //    {
                //        return null;
                //    }
                //    MemoryStream bundleData = new MemoryStream(bundleLength - 4);
                //    casStream.Position = bundleOffset + 4;
                //    casStream.CopyTo(bundleData, bundleLength - 4);
                //    bundleData.Position = 0L;
                //    return bundleData;
                //}

            }

            private Stream GetBundleDataStream(string superBundlePath, long bundleOffset, int bundleLength)
            {
                using (FileStream sbStream = new FileStream(superBundlePath, FileMode.Open, FileAccess.Read))
                {
                    if (sbStream.Length == 0L)
                    {
                        return null;
                    }
                    sbStream.Position = bundleOffset;
                    MemoryStream bundleData = new MemoryStream();
                    using (NativeReader nr = new NativeReader(sbStream))
                    {
                        nr.Position = bundleOffset;
                        return new MemoryStream(nr.ReadBytes(bundleLength));
                    }
                }
            }

            private (NativeWriter casWriter, int casIndex) GetNextCas()
            {
                int casIndex;
                lock (locker)
                {
                    int catalog;
                    (catalog, casIndex) = CasFiles[this.catalog.Name];
                    CasFiles[this.catalog.Name] = (catalog, casIndex + 1);
                }
                FileInfo fileInfo = new FileInfo(Path.Combine(parent.fs.BasePath, parent.modDataPath, "Patch", this.catalog.Name, $"cas_{casIndex:D2}.cas"));
                if (!Directory.Exists(fileInfo.DirectoryName))
                {
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                }

                if (fileInfo.Exists && fileInfo.FullName.Contains("ModData"))
                    fileInfo.Delete();

                return (new NativeWriter(new FileStream(fileInfo.FullName, FileMode.CreateNew)), casIndex);
            }
        }

        //public class ArchiveInfo
        //{
        //    public byte[] Data;

        //    public int RefCount;
        //}

        public class ModBundleInfo
        {
            public class ModBundleAction
            {
                public List<string> Ebx = new List<string>();

                public List<string> Res = new List<string>();

                public List<Guid> Chunks = new List<Guid>();
            }

            public int Name;

            public ModBundleAction Add = new ModBundleAction();

            public ModBundleAction Remove = new ModBundleAction();

            public ModBundleAction Modify = new ModBundleAction();
        }

        private class CasFileEntry
        {
            public ManifestFileInfo FileInfo;

            public ChunkAssetEntry Entry;
        }

        private class CasDataEntry
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
                if (num == -1 || num >= fileInfos.Count)
                {
                    yield break;
                }
                foreach (CasFileEntry item in fileInfos[sha1])
                {
                    yield return item;
                }
            }
        }

        private class CasDataInfo
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

        private class SymLinkException : Exception
        {
            public override string Message => "One or more symbolic links could not be created, please restart the editor as an Administrator.";
        }

        private class HandlerExtraData : AssetExtraData
        {
            public FIFA21Plugin.Plugin2.Handlers.ICustomBundleActionHandler Handler
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

        private struct SymLinkStruct
        {
            public string dest;

            public string src;

            public bool isFolder;

            public SymLinkStruct(string inDst, string inSrc, bool isFolder)
            {
                dest = inDst;
                src = inSrc;
                this.isFolder = isFolder;
            }
        }

        private class FifaBundleAction
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

            private readonly KeyManager keyManager;

            public static int CasFileCount = 0;

            private Exception errorException;

            private ManualResetEvent doneEvent;

            private Plugin2Executer parent;

            private Catalog catalogInfo;

            private string modDataFolder;

            private Dictionary<int, string> casFiles = new Dictionary<int, string>();

            private CancellationToken cancelToken;

            public Catalog CatalogInfo => catalogInfo;

            public Dictionary<int, string> CasFiles => casFiles;

            public bool HasErrored => errorException != null;

            public Exception Exception => errorException;

            public ILogger Logger { get; set; }


            public FifaBundleAction(KeyManager keyManager, Catalog inCatalogInfo, ManualResetEvent inDoneEvent, Plugin2Executer inParent, CancellationToken inCancelToken, string modDataFolder)
            {
                this.keyManager = keyManager ?? throw new ArgumentNullException("keyManager");
                catalogInfo = inCatalogInfo ?? throw new ArgumentNullException("inCatalogInfo");
                parent = inParent ?? throw new ArgumentNullException("inParent");
                doneEvent = inDoneEvent ?? throw new ArgumentNullException("inDoneEvent");
                cancelToken = inCancelToken;
                this.modDataFolder = modDataFolder ?? throw new ArgumentNullException("modDataFolder");
            }

            public void Run()
            {
                try
                {
                    NativeWriter nativeWriter = null;
                    int casFileIndex = 0;
                    byte[] key = keyManager.GetKey("key2");
                    foreach (string superBundle in catalogInfo.SuperBundleKeys)
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        string arg = superBundle;
                        if (catalogInfo.IsSplitSuperBundle(superBundle))
                        {
                            arg = superBundle.Replace("win32", catalogInfo.Name);
                        }
                        string text = parent.fs.ResolvePath(arg + ".toc");
                        if (text == "")
                        {
                            continue;
                        }
                        uint num = 0u;
                        uint num12 = 0u;
                        byte[] array = null;
                        using (FileStream tocFileStream = new FileStream(text, FileMode.Open, FileAccess.Read))
                        {
                            NativeReader fileReader = new NativeReader(tocFileStream, skipObfuscation: true);
                            uint num23 = fileReader.ReadUInt32LittleEndian();
                            num = fileReader.ReadUInt32LittleEndian();
                            num12 = fileReader.ReadUInt32LittleEndian();
                            array = fileReader.ReadToEnd();
                            if (num23 == 3286619587u)
                            {
                                using (Aes aes = Aes.Create())
                                {
                                    aes.Key = key;
                                    aes.IV = key;
                                    aes.Padding = PaddingMode.None;
                                    ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
                                    using (CryptoStream cryptoStream = new CryptoStream(new MemoryStream(array), transform, CryptoStreamMode.Read))
                                    {
                                        cryptoStream.Read(array, 0, array.Length);
                                    }
                                }
                            }
                        }
                        string text3 = text.Replace("Patch\\Win32", modDataFolder + "\\Patch\\Win32");
                        FileInfo fileInfo = new FileInfo(text3);
                        if (!Directory.Exists(fileInfo.DirectoryName))
                        {
                            Directory.CreateDirectory(fileInfo.DirectoryName);
                        }
                        using (FileStream moddedTocStream = new FileStream(text3, FileMode.Create))
                        {
                            NativeWriter moddedTocWriter = new NativeWriter(moddedTocStream);
                            moddedTocWriter.WriteInt32LittleEndian(30331136);
                            moddedTocWriter.Position += 552L;
                            long position = moddedTocWriter.Position;
                            moddedTocWriter.WriteUInt32LittleEndian(3280507699u);
                            long num33 = 4294967295L;
                            long num34 = 4294967295L;
                            if (array.Length != 0)
                            {
                                moddedTocWriter.WriteUInt32LittleEndian(3735928559u);
                                moddedTocWriter.WriteUInt32LittleEndian(3735928559u);
                                NativeReader nativeReader2 = new NativeReader(new MemoryStream(array));
                                if (num != uint.MaxValue)
                                {
                                    nativeReader2.Position = num - 12;
                                    int num35 = nativeReader2.ReadInt32LittleEndian();
                                    List<int> list = new List<int>();
                                    for (int i = 0; i < num35; i++)
                                    {
                                        list.Add(nativeReader2.ReadInt32LittleEndian());
                                    }
                                    List<int> list4 = new List<int>();
                                    for (int j = 0; j < num35; j++)
                                    {
                                        cancelToken.ThrowIfCancellationRequested();
                                        int num36 = nativeReader2.ReadInt32LittleEndian() - 12;
                                        long position2 = nativeReader2.Position;
                                        nativeReader2.Position = num36;
                                        int num37 = nativeReader2.ReadInt32LittleEndian() - 1;
                                        List<BundleFileEntry> list5 = new List<BundleFileEntry>();
                                        int num38;
                                        do
                                        {
                                            num38 = nativeReader2.ReadInt32LittleEndian();
                                            int inOffset = nativeReader2.ReadInt32LittleEndian();
                                            int inSize = nativeReader2.ReadInt32LittleEndian();
                                            list5.Add(new BundleFileEntry(num38 & 0x7FFFFFFF, inOffset, inSize));
                                        }
                                        while ((num38 & 0x80000000u) != 0L);
                                        nativeReader2.Position = num37 - 12;
                                        int num2 = 0;
                                        string text2 = "";
                                        do
                                        {
                                            text2 = nativeReader2.ReadNullTerminatedString(reverse: true) + text2;
                                            num2 = nativeReader2.ReadInt32LittleEndian() - 1;
                                            if (num2 != -1)
                                            {
                                                nativeReader2.Position = num2 - 12;
                                            }
                                        }
                                        while (num2 != -1);
                                        nativeReader2.Position = position2;
                                        int key2 = Fnv1.HashString(text2.ToLowerInvariant());
                                        if (parent.modifiedBundles.ContainsKey(key2))
                                        {
                                            ModBundleInfo modBundleInfo = parent.modifiedBundles[key2];
                                            MemoryStream memoryStream = new MemoryStream();
                                            foreach (BundleFileEntry item in list5)
                                            {
                                                using (FileStream fileStream2 = new FileStream(parent.fs.ResolvePath(parent.fs.GetCasFilePathFromIndex(item.CasIndex)), FileMode.Open, FileAccess.Read))
                                                {
                                                    NativeReader nativeReader3 = new NativeReader(fileStream2);
                                                    nativeReader3.Position = item.Offset;
                                                    memoryStream.Write(nativeReader3.ReadBytes(item.Size), 0, item.Size);
                                                }
                                            }
                                            DbObject dbObject = null;
                                            dbObject = new SuperBundleReader(memoryStream, 0L, skipObfuscation: true).ReadDbObject(key);
                                            foreach (DbObject item12 in dbObject.GetValue<DbObject>("ebx").List.Cast<DbObject>())
                                            {
                                                item12.GetValue("size", 0);
                                                long value = item12.GetValue("offset", 0L);
                                                long num3 = 0L;
                                                foreach (BundleFileEntry item14 in list5)
                                                {
                                                    if (value < num3 + item14.Size)
                                                    {
                                                        value -= num3;
                                                        value += item14.Offset;
                                                        item12.SetValue("offset", value);
                                                        item12.SetValue("cas", item14.CasIndex);
                                                        break;
                                                    }
                                                    num3 += item14.Size;
                                                }
                                            }
                                            foreach (DbObject item15 in dbObject.GetValue<DbObject>("res").List.Cast<DbObject>())
                                            {
                                                item15.GetValue("size", 0);
                                                long value2 = item15.GetValue("offset", 0L);
                                                long num4 = 0L;
                                                foreach (BundleFileEntry item16 in list5)
                                                {
                                                    if (value2 < num4 + item16.Size)
                                                    {
                                                        value2 -= num4;
                                                        value2 += item16.Offset;
                                                        item15.SetValue("offset", value2);
                                                        item15.SetValue("cas", item16.CasIndex);
                                                        break;
                                                    }
                                                    num4 += item16.Size;
                                                }
                                            }
                                            foreach (DbObject item17 in dbObject.GetValue<DbObject>("chunks").List.Cast<DbObject>())
                                            {
                                                item17.GetValue("size", 0);
                                                long value3 = item17.GetValue("offset", 0L);
                                                long num5 = 0L;
                                                foreach (BundleFileEntry item18 in list5)
                                                {
                                                    if (value3 < num5 + item18.Size)
                                                    {
                                                        value3 -= num5;
                                                        value3 += item18.Offset;
                                                        item17.SetValue("offset", value3);
                                                        item17.SetValue("cas", item18.CasIndex);
                                                        break;
                                                    }
                                                    num5 += item18.Size;
                                                }
                                            }
                                            foreach (DbObject ebx in dbObject.GetValue<DbObject>("ebx").List.Cast<DbObject>())
                                            {
                                                int num6 = modBundleInfo.Modify.Ebx.FindIndex((string a) => a.Equals(ebx.GetValue<string>("name")));
                                                if (num6 != -1)
                                                {
                                                    EbxAssetEntry ebxAssetEntry = parent.modifiedEbx[modBundleInfo.Modify.Ebx[num6]];
                                                    if (nativeWriter == null || nativeWriter.Length + parent.archiveData[ebxAssetEntry.Sha1].Data.Length > 1073741824)
                                                    {
                                                        nativeWriter?.Close();
                                                        nativeWriter = GetNextCas(out casFileIndex);
                                                    }
                                                    ebx.SetValue("originalSize", ebxAssetEntry.OriginalSize);
                                                    ebx.SetValue("size", ebxAssetEntry.Size);
                                                    ebx.SetValue("cas", casFileIndex);
                                                    ebx.SetValue("offset", (int)nativeWriter.Position);
                                                    nativeWriter.WriteBytes(parent.archiveData[ebxAssetEntry.Sha1].Data);
                                                }
                                            }
                                            foreach (string item19 in modBundleInfo.Add.Ebx)
                                            {
                                                EbxAssetEntry ebxAssetEntry2 = parent.modifiedEbx[item19];
                                                if (nativeWriter == null || nativeWriter.Length + parent.archiveData[ebxAssetEntry2.Sha1].Data.Length > 1073741824)
                                                {
                                                    nativeWriter?.Close();
                                                    nativeWriter = GetNextCas(out casFileIndex);
                                                }
                                                DbObject dbObject2 = DbObject.CreateObject();
                                                dbObject2.SetValue("name", ebxAssetEntry2.Name);
                                                dbObject2.SetValue("originalSize", ebxAssetEntry2.OriginalSize);
                                                dbObject2.SetValue("size", ebxAssetEntry2.Size);
                                                dbObject2.SetValue("cas", casFileIndex);
                                                dbObject2.SetValue("offset", (int)nativeWriter.Position);
                                                dbObject.GetValue<DbObject>("ebx").List.Add(dbObject2);
                                                nativeWriter.WriteBytes(parent.archiveData[ebxAssetEntry2.Sha1].Data);
                                            }
                                            foreach (DbObject res in dbObject.GetValue<DbObject>("res").List.Cast<DbObject>())
                                            {
                                                int num7 = modBundleInfo.Modify.Res.FindIndex((string a) => a.Equals(res.GetValue<string>("name")));
                                                if (num7 != -1)
                                                {
                                                    ResAssetEntry resAssetEntry = parent.modifiedRes[modBundleInfo.Modify.Res[num7]];
                                                    if (nativeWriter == null || nativeWriter.Length + parent.archiveData[resAssetEntry.Sha1].Data.Length > 1073741824)
                                                    {
                                                        nativeWriter?.Close();
                                                        nativeWriter = GetNextCas(out casFileIndex);
                                                    }
                                                    res.SetValue("originalSize", resAssetEntry.OriginalSize);
                                                    res.SetValue("size", resAssetEntry.Size);
                                                    res.SetValue("cas", casFileIndex);
                                                    res.SetValue("offset", (int)nativeWriter.Position);
                                                    res.SetValue("resRid", (long)resAssetEntry.ResRid);
                                                    res.SetValue("resMeta", resAssetEntry.ResMeta);
                                                    res.SetValue("resType", resAssetEntry.ResType);
                                                    nativeWriter.WriteBytes(parent.archiveData[resAssetEntry.Sha1].Data);
                                                }
                                            }
                                            foreach (string re in modBundleInfo.Add.Res)
                                            {
                                                ResAssetEntry resAssetEntry2 = parent.modifiedRes[re];
                                                if (nativeWriter == null || nativeWriter.Length + parent.archiveData[resAssetEntry2.Sha1].Data.Length > 1073741824)
                                                {
                                                    nativeWriter?.Close();
                                                    nativeWriter = GetNextCas(out casFileIndex);
                                                }
                                                DbObject dbObject3 = DbObject.CreateObject();
                                                dbObject3.SetValue("name", resAssetEntry2.Name);
                                                dbObject3.SetValue("originalSize", resAssetEntry2.OriginalSize);
                                                dbObject3.SetValue("size", resAssetEntry2.Size);
                                                dbObject3.SetValue("cas", casFileIndex);
                                                dbObject3.SetValue("offset", (int)nativeWriter.Position);
                                                dbObject3.SetValue("resRid", (long)resAssetEntry2.ResRid);
                                                dbObject3.SetValue("resMeta", resAssetEntry2.ResMeta);
                                                dbObject3.SetValue("resType", resAssetEntry2.ResType);
                                                dbObject.GetValue<DbObject>("res").List.Add(dbObject3);
                                                nativeWriter.WriteBytes(parent.archiveData[resAssetEntry2.Sha1].Data);
                                            }
                                            foreach (DbObject chunk in dbObject.GetValue<DbObject>("chunks").List.Cast<DbObject>())
                                            {
                                                int num8 = modBundleInfo.Modify.Chunks.FindIndex((Guid a) => a == chunk.GetValue<Guid>("id"));
                                                if (num8 != -1)
                                                {
                                                    ChunkAssetEntry chunkAssetEntry = parent.modifiedChunks[modBundleInfo.Modify.Chunks[num8]];
                                                    if (nativeWriter == null || nativeWriter.Length + parent.archiveData[chunkAssetEntry.Sha1].Data.Length > 1073741824)
                                                    {
                                                        nativeWriter?.Close();
                                                        nativeWriter = GetNextCas(out casFileIndex);
                                                    }
                                                    chunk.SetValue("originalSize", chunkAssetEntry.OriginalSize);
                                                    chunk.SetValue("size", chunkAssetEntry.Size);
                                                    chunk.SetValue("cas", casFileIndex);
                                                    chunk.SetValue("offset", (int)nativeWriter.Position);
                                                    chunk.SetValue("logicalOffset", chunkAssetEntry.LogicalOffset);
                                                    chunk.SetValue("logicalSize", chunkAssetEntry.LogicalSize);
                                                    nativeWriter.WriteBytes(parent.archiveData[chunkAssetEntry.Sha1].Data);
                                                }
                                            }
                                            foreach (Guid chunk2 in modBundleInfo.Add.Chunks)
                                            {
                                                ChunkAssetEntry chunkAssetEntry2 = parent.modifiedChunks[chunk2];
                                                if (nativeWriter == null || nativeWriter.Length + parent.archiveData[chunkAssetEntry2.Sha1].Data.Length > 1073741824)
                                                {
                                                    nativeWriter?.Close();
                                                    nativeWriter = GetNextCas(out casFileIndex);
                                                }
                                                DbObject dbObject4 = DbObject.CreateObject();
                                                dbObject4.SetValue("id", chunkAssetEntry2.Id);
                                                dbObject4.SetValue("originalSize", chunkAssetEntry2.OriginalSize);
                                                dbObject4.SetValue("size", chunkAssetEntry2.Size);
                                                dbObject4.SetValue("cas", casFileIndex);
                                                dbObject4.SetValue("offset", (int)nativeWriter.Position);
                                                dbObject4.SetValue("logicalOffset", chunkAssetEntry2.LogicalOffset);
                                                dbObject4.SetValue("logicalSize", chunkAssetEntry2.LogicalSize);
                                                dbObject.GetValue<DbObject>("chunks").List.Add(dbObject4);
                                                DbObject dbObject5 = DbObject.CreateObject();
                                                dbObject5.SetValue("h32", chunkAssetEntry2.H32);
                                                DbObject dbObject6 = DbObject.CreateObject();
                                                if (chunkAssetEntry2.FirstMip != -1)
                                                {
                                                    dbObject6.SetValue("firstMip", chunkAssetEntry2.FirstMip);
                                                }
                                                dbObject.GetValue<DbObject>("chunkMeta").List.Add(dbObject5);
                                                nativeWriter.WriteBytes(parent.archiveData[chunkAssetEntry2.Sha1].Data);
                                            }
                                            BundleFileEntry bundleFileEntry = list5[0];
                                            list5.Clear();
                                            list5.Add(bundleFileEntry);
                                            foreach (DbObject item20 in dbObject.GetValue<DbObject>("ebx").List.Cast<DbObject>())
                                            {
                                                list5.Add(new BundleFileEntry(item20.GetValue("cas", 0), item20.GetValue("offset", 0), item20.GetValue("size", 0)));
                                            }
                                            foreach (DbObject item2 in dbObject.GetValue<DbObject>("res").List.Cast<DbObject>())
                                            {
                                                list5.Add(new BundleFileEntry(item2.GetValue("cas", 0), item2.GetValue("offset", 0), item2.GetValue("size", 0)));
                                            }
                                            foreach (DbObject item3 in dbObject.GetValue<DbObject>("chunks").List.Cast<DbObject>())
                                            {
                                                list5.Add(new BundleFileEntry(item3.GetValue("cas", 0), item3.GetValue("offset", 0), item3.GetValue("size", 0)));
                                            }
                                            int count = dbObject.GetValue<DbObject>("ebx").List.Count;
                                            int count2 = dbObject.GetValue<DbObject>("res").List.Count;
                                            int count3 = dbObject.GetValue<DbObject>("chunks").List.Count;
                                            NativeWriter nativeWriter2 = new NativeWriter(new MemoryStream());
                                            nativeWriter2.WriteUInt32BigEndian(3735927486u);
                                            nativeWriter2.WriteUInt32BigEndian(3018715229u);
                                            nativeWriter2.WriteInt32BigEndian(count + count2 + count3);
                                            nativeWriter2.WriteInt32BigEndian(count);
                                            nativeWriter2.WriteInt32BigEndian(count2);
                                            nativeWriter2.WriteInt32BigEndian(count3);
                                            nativeWriter2.WriteUInt32BigEndian(3735927486u);
                                            nativeWriter2.WriteUInt32BigEndian(3735927486u);
                                            nativeWriter2.WriteUInt32BigEndian(3735927486u);
                                            long num9 = 0L;
                                            new Dictionary<uint, long>();
                                            List<string> list6 = new List<string>();
                                            foreach (DbObject item4 in dbObject.GetValue<DbObject>("ebx").List.Cast<DbObject>())
                                            {
                                                Fnv1.HashString(item4.GetValue<string>("name"));
                                                nativeWriter2.WriteUInt32BigEndian((uint)num9);
                                                list6.Add(item4.GetValue<string>("name"));
                                                num9 += item4.GetValue<string>("name").Length + 1;
                                                nativeWriter2.WriteInt32BigEndian(item4.GetValue("originalSize", 0));
                                            }
                                            foreach (DbObject item5 in dbObject.GetValue<DbObject>("res").List.Cast<DbObject>())
                                            {
                                                Fnv1.HashString(item5.GetValue<string>("name"));
                                                nativeWriter2.WriteUInt32BigEndian((uint)num9);
                                                list6.Add(item5.GetValue<string>("name"));
                                                num9 += item5.GetValue<string>("name").Length + 1;
                                                nativeWriter2.WriteInt32BigEndian(item5.GetValue("originalSize", 0));
                                            }
                                            foreach (DbObject item6 in dbObject.GetValue<DbObject>("res").List.Cast<DbObject>())
                                            {
                                                nativeWriter2.WriteUInt32BigEndian((uint)item6.GetValue("resType", 0L));
                                            }
                                            foreach (DbObject item7 in dbObject.GetValue<DbObject>("res").List.Cast<DbObject>())
                                            {
                                                nativeWriter2.WriteBytes(item7.GetValue<byte[]>("resMeta"));
                                            }
                                            foreach (DbObject item8 in dbObject.GetValue<DbObject>("res").List.Cast<DbObject>())
                                            {
                                                nativeWriter2.WriteInt64BigEndian(item8.GetValue("resRid", 0L));
                                            }
                                            foreach (DbObject item9 in dbObject.GetValue<DbObject>("chunks").List.Cast<DbObject>())
                                            {
                                                nativeWriter2.Write(item9.GetValue<Guid>("id"), Endian.Big);
                                                nativeWriter2.WriteInt32BigEndian(item9.GetValue("logicalOffset", 0));
                                                nativeWriter2.WriteInt32BigEndian(item9.GetValue("logicalSize", 0));
                                            }
                                            long position3 = nativeWriter2.Position;
                                            foreach (string item10 in list6)
                                            {
                                                nativeWriter2.WriteNullTerminatedString(item10);
                                            }
                                            long num10 = 0L;
                                            long num11 = 0L;
                                            if (dbObject.GetValue<DbObject>("chunks").List.Count > 0)
                                            {
                                                DbObject value4 = dbObject.GetValue<DbObject>("chunkMeta");
                                                num10 = nativeWriter2.Position;
                                                new TocWriter().WriteObject(nativeWriter2.BaseStream, "chunkMeta", value4);
                                                num11 = nativeWriter2.Position - num10;
                                            }
                                            long num13 = nativeWriter2.Position - 4;
                                            nativeWriter2.Position = 24L;
                                            nativeWriter2.WriteUInt32BigEndian((uint)(position3 - 4));
                                            nativeWriter2.WriteUInt32BigEndian((uint)(num10 - 4));
                                            nativeWriter2.WriteUInt32BigEndian((uint)num11);
                                            nativeWriter2.Position = 0L;
                                            nativeWriter2.WriteUInt32BigEndian((uint)num13);
                                            if (nativeWriter == null || nativeWriter.Length + nativeWriter2.Length > 1073741824)
                                            {
                                                nativeWriter?.Close();
                                                nativeWriter = GetNextCas(out casFileIndex);
                                            }
                                            bundleFileEntry.CasIndex = casFileIndex;
                                            bundleFileEntry.Offset = (int)nativeWriter.Position;
                                            bundleFileEntry.Size = (int)(num13 + 4);
                                            nativeWriter2.Position = 0L;
                                            nativeWriter2.BaseStream.CopyTo(nativeWriter.BaseStream);
                                        }
                                        list4.Add((int)(moddedTocWriter.Position - position));
                                        moddedTocWriter.WriteInt32LittleEndian((int)(moddedTocWriter.Position - position + list5.Count * 3 * 4 + 5));
                                        for (int k = 0; k < list5.Count; k++)
                                        {
                                            uint num14 = (uint)list5[k].CasIndex;
                                            if (k != list5.Count - 1)
                                            {
                                                num14 |= 0x80000000u;
                                            }
                                            moddedTocWriter.WriteUInt32LittleEndian(num14);
                                            moddedTocWriter.WriteInt32LittleEndian(list5[k].Offset);
                                            moddedTocWriter.WriteInt32LittleEndian(list5[k].Size);
                                        }
                                        moddedTocWriter.WriteNullTerminatedString(new string(text2.Reverse().ToArray()));
                                        moddedTocWriter.WriteInt32LittleEndian(0);
                                        int num15 = text2.Length + 5;
                                        for (int l = 0; l < 16 - num15 % 16; l++)
                                        {
                                            moddedTocWriter.Write((byte)0);
                                        }
                                    }
                                    num33 = moddedTocWriter.Position - position;
                                    moddedTocWriter.WriteInt32LittleEndian(num35);
                                    foreach (int item11 in list)
                                    {
                                        moddedTocWriter.WriteInt32LittleEndian(item11);
                                    }
                                    foreach (int item13 in list4)
                                    {
                                        moddedTocWriter.WriteInt32LittleEndian(item13);
                                    }
                                }
                                List<int> list7 = new List<int>();
                                List<uint> list8 = new List<uint>();
                                List<Guid> list9 = new List<Guid>();
                                List<List<Tuple<Guid, int>>> list10 = new List<List<Tuple<Guid, int>>>();
                                int num16 = 0;
                                if (num12 != uint.MaxValue)
                                {
                                    nativeReader2.Position = num12 - 12;
                                    num16 = nativeReader2.ReadInt32LittleEndian();
                                    for (int m = 0; m < num16; m++)
                                    {
                                        list7.Add(nativeReader2.ReadInt32LittleEndian());
                                        list10.Add(new List<Tuple<Guid, int>>());
                                    }
                                    for (int n = 0; n < num16; n++)
                                    {
                                        cancelToken.ThrowIfCancellationRequested();
                                        uint num17 = nativeReader2.ReadUInt32LittleEndian();
                                        long position4 = nativeReader2.Position;
                                        nativeReader2.Position = num17 - 12;
                                        Guid guid = nativeReader2.ReadGuid();
                                        int num18 = nativeReader2.ReadInt32LittleEndian();
                                        int num19 = nativeReader2.ReadInt32LittleEndian();
                                        int num20 = nativeReader2.ReadInt32LittleEndian();
                                        nativeReader2.Position = position4;
                                        list8.Add((uint)(moddedTocWriter.Position - position));
                                        if (parent.modifiedBundles.ContainsKey(chunksBundleHash))
                                        {
                                            ModBundleInfo modBundleInfo2 = parent.modifiedBundles[chunksBundleHash];
                                            int num21 = modBundleInfo2.Modify.Chunks.FindIndex((Guid g) => g == guid);
                                            if (num21 != -1)
                                            {
                                                ChunkAssetEntry chunkAssetEntry3 = parent.modifiedChunks[modBundleInfo2.Modify.Chunks[num21]];
                                                byte[] outData = null;
                                                if (chunkAssetEntry3.ExtraData != null)
                                                {
                                                    HandlerExtraData handlerExtraData = (HandlerExtraData)chunkAssetEntry3.ExtraData;
                                                    Stream resourceData = parent.rm.GetResourceData(parent.fs.GetCasFilePathFromIndex(num18), num19, num20);
                                                    chunkAssetEntry3 = (ChunkAssetEntry)handlerExtraData.Handler.Modify(chunkAssetEntry3, resourceData, handlerExtraData.Data, out outData);
                                                }
                                                else
                                                {
                                                    outData = parent.archiveData[chunkAssetEntry3.Sha1].Data;
                                                }
                                                if (nativeWriter == null || nativeWriter.BaseStream.Length + outData.Length > 1073741824)
                                                {
                                                    nativeWriter?.Close();
                                                    nativeWriter = GetNextCas(out casFileIndex);
                                                }
                                                num18 = casFileIndex;
                                                num19 = (int)nativeWriter.BaseStream.Position;
                                                num20 = (int)chunkAssetEntry3.Size;
                                                nativeWriter.WriteBytes(outData);
                                            }
                                        }
                                        moddedTocWriter.WriteGuid(guid);
                                        moddedTocWriter.WriteInt32LittleEndian(num18);
                                        moddedTocWriter.WriteInt32LittleEndian(num19);
                                        moddedTocWriter.WriteInt32LittleEndian(num20);
                                        list9.Add(guid);
                                    }
                                }
                                if (parent.modifiedBundles.ContainsKey(chunksBundleHash) && text.Contains("globals.toc"))
                                {
                                    foreach (Guid chunk3 in parent.modifiedBundles[chunksBundleHash].Add.Chunks)
                                    {
                                        ChunkAssetEntry chunkAssetEntry4 = parent.modifiedChunks[chunk3];
                                        if (nativeWriter == null || nativeWriter.BaseStream.Length + parent.archiveData[chunkAssetEntry4.Sha1].Data.Length > 1073741824)
                                        {
                                            nativeWriter?.Close();
                                            nativeWriter = GetNextCas(out casFileIndex);
                                        }
                                        int value5 = casFileIndex;
                                        int value6 = (int)nativeWriter.BaseStream.Position;
                                        int value7 = (int)chunkAssetEntry4.Size;
                                        list8.Add((uint)(moddedTocWriter.BaseStream.Position - position));
                                        list10.Add(new List<Tuple<Guid, int>>());
                                        list7.Add(-1);
                                        num16++;
                                        list9.Add(chunk3);
                                        nativeWriter.WriteBytes(parent.archiveData[chunkAssetEntry4.Sha1].Data);
                                        moddedTocWriter.WriteGuid(chunk3);
                                        moddedTocWriter.WriteInt32LittleEndian(value5);
                                        moddedTocWriter.WriteInt32LittleEndian(value6);
                                        moddedTocWriter.WriteInt32LittleEndian(value7);
                                    }
                                }
                                if (num16 > 0)
                                {
                                    num34 = moddedTocWriter.Position - position;
                                    int num22 = 0;
                                    List<int> list11 = new List<int>();
                                    for (int num24 = 0; num24 < num16; num24++)
                                    {
                                        list11.Add(-1);
                                        list7[num24] = -1;
                                        int index = (int)((long)(uint)((int)HashData(list9[num24].ToByteArray()) % 16777619) % (long)num16);
                                        list10[index].Add(new Tuple<Guid, int>(list9[num24], (int)list8[num24]));
                                    }
                                    for (int num25 = 0; num25 < list10.Count; num25++)
                                    {
                                        List<Tuple<Guid, int>> list2 = list10[num25];
                                        if (list2.Count <= 1)
                                        {
                                            continue;
                                        }
                                        uint num26 = 1u;
                                        List<int> list3 = new List<int>();
                                        while (true)
                                        {
                                            bool flag = true;
                                            for (int num27 = 0; num27 < list2.Count; num27++)
                                            {
                                                int num28 = (int)((long)(uint)((int)HashData(list2[num27].Item1.ToByteArray(), num26) % 16777619) % (long)num16);
                                                if (list11[num28] != -1 || list3.Contains(num28))
                                                {
                                                    flag = false;
                                                    break;
                                                }
                                                list3.Add(num28);
                                            }
                                            if (flag)
                                            {
                                                break;
                                            }
                                            num26++;
                                            list3.Clear();
                                        }
                                        for (int num29 = 0; num29 < list2.Count; num29++)
                                        {
                                            list11[list3[num29]] = list2[num29].Item2;
                                        }
                                        list7[num25] = (int)num26;
                                    }
                                    for (int num30 = 0; num30 < list10.Count; num30++)
                                    {
                                        if (list10[num30].Count == 1)
                                        {
                                            for (; list11[num22] != -1; num22++)
                                            {
                                            }
                                            list7[num30] = -1 - num22;
                                            list11[num22] = list10[num30][0].Item2;
                                        }
                                    }
                                    moddedTocWriter.WriteInt32LittleEndian(num16);
                                    for (int num31 = 0; num31 < num16; num31++)
                                    {
                                        moddedTocWriter.WriteInt32LittleEndian(list7[num31]);
                                    }
                                    for (int num32 = 0; num32 < num16; num32++)
                                    {
                                        moddedTocWriter.WriteInt32LittleEndian(list11[num32]);
                                    }
                                }
                                moddedTocWriter.BaseStream.Position = position + 4;
                                moddedTocWriter.WriteInt32LittleEndian((int)num33);
                                moddedTocWriter.WriteInt32LittleEndian((int)num34);
                            }
                            else
                            {
                                moddedTocWriter.WriteUInt32LittleEndian(uint.MaxValue);
                                moddedTocWriter.WriteUInt32LittleEndian(uint.MaxValue);
                            }
                        }
                    }
                    nativeWriter?.Close();
                }
                catch (Exception ex2)
                {
                    Exception ex = (errorException = ex2);
                }
            }

            private NativeWriter GetNextCas(out int casFileIndex)
            {
                int num = 1;
                string text = parent.fs.BasePath + modDataFolder + "\\patch\\" + catalogInfo.Name + "\\cas_" + num.ToString("D2") + ".cas";
                while (File.Exists(text))
                {
                    num++;
                    text = parent.fs.BasePath + modDataFolder + "\\patch\\" + catalogInfo.Name + "\\cas_" + num.ToString("D2") + ".cas";
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
                    num = strToHash[i] ^ (16777619 * num);
                }
                return num;
            }

            private static uint HashData(byte[] b, uint initial = 0u)
            {
                uint num = (uint)(sbyte)b[0] ^ 0x50C5D1Fu;
                int num2 = 1;
                if (initial != 0)
                {
                    num = initial;
                    num2 = 0;
                }
                for (int i = num2; i < b.Length; i++)
                {
                    num = (uint)(sbyte)b[i] ^ (16777619 * num);
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

        private readonly KeyManager keyManager;

        private readonly FileSystem fs;

        private readonly ResourceManager rm;

        private readonly string modDataPath;

        private List<string> addedSuperBundles = new List<string>();

        public Dictionary<int, ModBundleInfo> modifiedBundles = new Dictionary<int, ModBundleInfo>();

        public Dictionary<int, List<string>> addedBundles = new Dictionary<int, List<string>>();

        public Dictionary<string, EbxAssetEntry> modifiedEbx = new Dictionary<string, EbxAssetEntry>();

        public Dictionary<string, ResAssetEntry> modifiedRes = new Dictionary<string, ResAssetEntry>();

        public Dictionary<Guid, ChunkAssetEntry> modifiedChunks = new Dictionary<Guid, ChunkAssetEntry>();

        public Dictionary<Sha1, ArchiveInfo> archiveData = new Dictionary<Sha1, ArchiveInfo>();

        private int numArchiveEntries;

        private int numTasks;

        private CasDataInfo casData = new CasDataInfo();

        private static int chunksBundleHash = Fnv1.HashString("chunks");

        private Dictionary<int, Dictionary<int, Dictionary<uint, CatResourceEntry>>> resources = new Dictionary<int, Dictionary<int, Dictionary<uint, CatResourceEntry>>>();

        [DllImport("kernel32.dll")]
        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        public Plugin2Executer(FileSystem fileSystem, KeyManager keyManager, string modDataPath)
        {
            fs = fileSystem ?? throw new ArgumentNullException("fileSystem");
            this.keyManager = keyManager ?? throw new ArgumentNullException("keyManager");
            this.modDataPath = modDataPath ?? throw new ArgumentNullException("modDataPath");
            rm = new ResourceManager(fs);
        }

        private bool EnsureGameNotRunning(string gameName)
        {
            if (gameName == null)
            {
                throw new ArgumentNullException("gameName");
            }
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.Length; i++)
            {
                if (processes[i].ProcessName.Equals(gameName))
                {
                    return false;
                }
            }
            return true;
        }

        private bool SetUpModFolder(string modPath, string patchPath, string modDirName, List<SymLinkStruct> symLinks)
        {
            bool flag = false;
            if (!DeleteSelectFiles(Path.Combine(modPath, patchPath)) && !Directory.Exists(modPath))
            {
                flag = true;
                Directory.CreateDirectory(modPath);
                symLinks.Add(new SymLinkStruct(Path.Combine(modPath, "Data"), Path.Combine(fs.BasePath, "Data"), isFolder: true));
                symLinks.Add(new SymLinkStruct(Path.Combine(modPath, "Update"), Path.Combine(fs.BasePath, "Update"), isFolder: true));
                foreach (string item in Directory.EnumerateFiles(Path.Combine(fs.BasePath, patchPath), "*.cas", SearchOption.AllDirectories))
                {
                    FileInfo fileInfo3 = new FileInfo(item);
                    string text3 = fileInfo3.Directory.FullName.Replace("\\" + patchPath, "\\" + modDirName + "\\" + patchPath);
                    string inDst = Path.Combine(text3, fileInfo3.Name);
                    if (!Directory.Exists(text3))
                    {
                        Directory.CreateDirectory(text3);
                    }
                    symLinks.Add(new SymLinkStruct(inDst, fileInfo3.FullName, isFolder: false));
                }
            }
            foreach (string catalog in fs.Catalogs)
            {
                string catFile = fs.ResolvePath("native_patch/" + catalog + "/cas.cat");
                if (!File.Exists(catFile))
                {
                    continue;
                }
                FileInfo fileInfo5 = new FileInfo(catFile);
                string text4 = fileInfo5.Directory.FullName.Replace("\\" + patchPath, "\\" + modDirName + "\\" + patchPath);
                if (!Directory.Exists(text4))
                {
                    Directory.CreateDirectory(text4);
                }
                FileInfo[] files = fileInfo5.Directory.GetFiles();
                foreach (FileInfo fileInfo4 in files)
                {
                    string text5 = Path.Combine(text4, fileInfo4.Name);
                    if (fileInfo4.Extension == ".cas")
                    {
                        if (!File.Exists(text5))
                        {
                            symLinks.Add(new SymLinkStruct(text5, fileInfo4.FullName, isFolder: false));
                        }
                    }
                    else if (fileInfo4.Extension == ".cat")
                    {
                        fileInfo4.CopyTo(text5, overwrite: false);
                    }
                }
            }
            return flag;
        }

        private bool SetUpModFolderFifa21(string modPath, string patchPath, string modDirName, List<SymLinkStruct> symLinks)
        {
            bool flag = false;
            if (!DeleteSelectFiles(Path.Combine(modPath, patchPath)) && !Directory.Exists(modPath))
            {
                flag = true;
                //Log.Information("Creating mod data directory.");
                Directory.CreateDirectory(modPath);
                symLinks.Add(new SymLinkStruct(Path.Combine(modPath, "Update"), Path.Combine(fs.BasePath, "Update"), isFolder: true));
                foreach (string item in EnumerateFilesWithExclusions(Path.Combine(fs.BasePath, "Data"), ".toc", SearchOption.AllDirectories))
                {
                    FileInfo baseFileInfo2 = new FileInfo(item);
                    string moddedBasePath2 = baseFileInfo2.Directory.FullName.Replace("\\Data", "\\" + modDirName + "\\Data");
                    string moddedBaseFilePath2 = Path.Combine(moddedBasePath2, baseFileInfo2.Name);
                    if (!Directory.Exists(moddedBasePath2))
                    {
                        //Log.Information("Creating base data directory {Directory}.", moddedBasePath2);
                        Directory.CreateDirectory(moddedBasePath2);
                    }
                    symLinks.Add(new SymLinkStruct(moddedBaseFilePath2, baseFileInfo2.FullName, isFolder: false));
                }
                foreach (string item2 in Directory.EnumerateFiles(Path.Combine(fs.BasePath, "Data", "Win32", "superbundlelayout"), "*.toc", SearchOption.AllDirectories))
                {
                    FileInfo baseFileInfo = new FileInfo(item2);
                    string moddedBasePath = baseFileInfo.Directory.FullName.Replace("\\Data", "\\" + modDirName + "\\Data");
                    string moddedBaseFilePath = Path.Combine(moddedBasePath, baseFileInfo.Name);
                    if (!Directory.Exists(moddedBasePath))
                    {
                        //Log.Information("Creating base data superbundlelayout directory {Directory}.", moddedBasePath);
                        Directory.CreateDirectory(moddedBasePath);
                    }
                    symLinks.Add(new SymLinkStruct(moddedBaseFilePath, baseFileInfo.FullName, isFolder: false));
                }
                symLinks.Add(new SymLinkStruct(Path.Combine(modPath, "Data", "layout.toc"), Path.Combine(fs.BasePath, "Data", "layout.toc"), isFolder: false));
                symLinks.Add(new SymLinkStruct(Path.Combine(modPath, "Patch", "layout.toc"), Path.Combine(fs.BasePath, "Patch", "layout.toc"), isFolder: false));
                {
                    foreach (string item3 in Directory.EnumerateFiles(Path.Combine(fs.BasePath, patchPath), "*.cas", SearchOption.AllDirectories))
                    {
                        FileInfo patchCasFileInfo = new FileInfo(item3);
                        string moddedPatchCasPath = patchCasFileInfo.Directory.FullName.Replace("\\" + patchPath, "\\" + modDirName + "\\" + patchPath);
                        string moddedPatchCasFilePath = Path.Combine(moddedPatchCasPath, patchCasFileInfo.Name);
                        if (!Directory.Exists(moddedPatchCasPath))
                        {
                            //Log.Information("Creating Patch directory {Directory}.", moddedPatchCasPath);
                            Directory.CreateDirectory(moddedPatchCasPath);
                        }
                        symLinks.Add(new SymLinkStruct(moddedPatchCasFilePath, patchCasFileInfo.FullName, isFolder: false));
                    }
                    return flag;
                }
            }
            return flag;
        }

        private static IEnumerable<string> EnumerateFilesWithExclusions(string path, string excludeExtension, SearchOption searchOptions)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (excludeExtension == null)
            {
                throw new ArgumentNullException("excludeExtension");
            }
            foreach (string file in Directory.EnumerateFiles(path, "*", searchOptions))
            {
                if (!Path.GetExtension(file).Equals(excludeExtension))
                {
                    yield return file;
                }
            }
        }

        public ILogger Logger { get; set; }

        //public async Task<bool> Run(string gameName, CancellationToken cancelToken, string rootPath, string additionalArgs, params string[] modPaths, ILogger logger)
        //{
        //    SetUpModFolderFifa21("ModData", patchPath, modDirName);
                
        //    await ApplyFifa21Mods(patchPath, modPath, modDirName, cancelToken);
               
        //}

        public async Task<bool> BuildFIFA21Mods(string patchPath, string modPath)
        {
            Fifa21BundleAction.CasFiles.Clear();
            foreach (Catalog catalog in fs.CatalogObjects)
            {
                int casFileNumber = 1;
                string path2 = Path.Combine(fs.BasePath, "Patch", catalog.Name, $"cas_{casFileNumber:D2}.cas");
                while (File.Exists(path2))
                {
                    casFileNumber++;
                    path2 = Path.Combine(fs.BasePath, "Patch", catalog.Name, $"cas_{casFileNumber:D2}.cas");
                }
                Fifa21BundleAction.CasFiles.Add(catalog.Name, (catalog.Index.Value, casFileNumber));
            }
            await PrepareLegacyChangesForFifa21().ConfigureAwait(continueOnCapturedContext: false);
            Fifa21BundleAction.ModifiedEbxAssets = new ConcurrentDictionary<string, EbxAssetEntry>(modifiedEbx, StringComparer.OrdinalIgnoreCase);
            Fifa21BundleAction.ModifiedResAssets = new ConcurrentDictionary<string, ResAssetEntry>(modifiedRes, StringComparer.OrdinalIgnoreCase);
            Fifa21BundleAction.ModifiedChunkAssets = new ConcurrentDictionary<Guid, ChunkAssetEntry>(modifiedChunks);
            List<Fifa21BundleAction> bundleActions = new List<Fifa21BundleAction>();
            ManualResetEvent inDoneEvent = new ManualResetEvent(initialState: false);
            int num7 = 0;
            foreach (string superBundle in fs.SuperBundles)
            {
                if (fs.ResolvePath(superBundle + ".toc") != "")
                {
                    Fifa21BundleAction bundleAction2 = new Fifa21BundleAction(superBundle, inDoneEvent, this);
                    //await Task.Run(delegate
                    //{
                        bundleAction2.Execute();
                    //});
                    bundleActions.Add(bundleAction2);
                    numTasks++;
                    num7++;
                }
            }
            //inDoneEvent.WaitOne();
            //foreach (Fifa21BundleAction bundleAction in bundleActions)
            //{
            //    if (bundleAction.HasErrored)
            //    {
            //        ExceptionDispatchInfo.Capture(bundleAction.Exception).Throw();
            //        throw bundleAction.Exception;
            //    }
            //    if (!bundleAction.BaseTocModified)
            //    {
            //        string inSrc3 = fs.ResolvePath("native_data/" + bundleAction.SuperBundle + ".toc");
            //        if (!string.IsNullOrEmpty(inSrc3))
            //        {
            //            FileInfo fileInfo7 = new FileInfo(Path.Combine(modPath, "Data", bundleAction.SuperBundle + ".toc"));
            //            if (!Directory.Exists(fileInfo7.DirectoryName))
            //            {
            //                Directory.CreateDirectory(fileInfo7.DirectoryName);
            //            }
            //        }
            //    }
            //    if (!bundleAction.PatchTocModified)
            //    {
            //        string inSrc2 = fs.ResolvePath("native_patch/" + bundleAction.SuperBundle + ".toc");
            //        if (!string.IsNullOrEmpty(inSrc2))
            //        {
            //            FileInfo fileInfo6 = new FileInfo(Path.Combine(modPath, patchPath, bundleAction.SuperBundle + ".toc"));
            //            if (!Directory.Exists(fileInfo6.DirectoryName))
            //            {
            //                Directory.CreateDirectory(fileInfo6.DirectoryName);
            //            }
            //        }
            //    }
            //    if (bundleAction.PatchSbModified)
            //    {
            //        continue;
            //    }
            //    string inSrc = fs.ResolvePath("native_patch/" + bundleAction.SuperBundle + ".sb");
            //    if (!string.IsNullOrEmpty(inSrc))
            //    {
            //        FileInfo fileInfo8 = new FileInfo(Path.Combine(modPath, patchPath, bundleAction.SuperBundle + ".sb"));
            //        if (!Directory.Exists(fileInfo8.DirectoryName))
            //        {
            //            Directory.CreateDirectory(fileInfo8.DirectoryName);
            //        }
            //    }
            //}
            return true;
        }

        private async Task PrepareLegacyChangesForFifa21()
        {
            if (!modifiedBundles.ContainsKey(chunksBundleHash))
            {
                return;
            }
            Dictionary<int, Guid> lfeNameHashesToNewChunkGuid = new Dictionary<int, Guid>();
            Dictionary<Guid, HandlerExtraData> originalGuidToLegacyHandlerExtraData = new Dictionary<Guid, HandlerExtraData>();
            foreach (KeyValuePair<Guid, ChunkAssetEntry> modifiedChunk in modifiedChunks)
            {
                HandlerExtraData handlerExtraData = modifiedChunk.Value.ExtraData as HandlerExtraData;
                if (handlerExtraData == null || !(handlerExtraData.Handler is LegacyCustomActionHandler))
                {
                    continue;
                }
                foreach (LegacyCustomActionHandler.LegacyFileEntry lfe3 in (List<LegacyCustomActionHandler.LegacyFileEntry>)handlerExtraData.Data)
                {
                    originalGuidToLegacyHandlerExtraData[lfe3.OriginalChunkGuid] = handlerExtraData;
                    if (lfeNameHashesToNewChunkGuid.TryGetValue(lfe3.Hash, out var existingChunkId) && existingChunkId != lfe3.ChunkId)
                    {
                        Debug.WriteLine(string.Format("Legacy file with hash {Hash} has been modified twice, to different chunks ({ChunkId1} and {ChunkId2}", lfe3.Hash, existingChunkId, lfe3.ChunkId));
                        //Log.Warning("Legacy file with hash {Hash} has been modified twice, to different chunks ({ChunkId1} and {ChunkId2}", lfe3.Hash, existingChunkId, lfe3.ChunkId);
                    }
                    lfeNameHashesToNewChunkGuid[lfe3.Hash] = lfe3.ChunkId;
                }
            }
            IEnumerable<Guid> chunkCollectorGuids = (from c in modifiedChunks.Where(delegate (KeyValuePair<Guid, ChunkAssetEntry> c)
            {
                HandlerExtraData handlerExtraData4 = c.Value.ExtraData as HandlerExtraData;
                return handlerExtraData4 != null && handlerExtraData4.Handler is LegacyCustomActionHandler;
            })
                                                     select c.Key).Distinct();
            IEnumerable<Guid> originalChunkGuids = (from c in (from c in modifiedChunks.Where(delegate (KeyValuePair<Guid, ChunkAssetEntry> c)
            {
                HandlerExtraData handlerExtraData3 = c.Value.ExtraData as HandlerExtraData;
                return handlerExtraData3 != null && handlerExtraData3.Handler is LegacyCustomActionHandler;
            })
                                                               select (List<LegacyCustomActionHandler.LegacyFileEntry>)((HandlerExtraData)c.Value.ExtraData).Data).SelectMany((List<LegacyCustomActionHandler.LegacyFileEntry> c) => c)
                                                    select c.OriginalChunkGuid).Distinct();
            IEnumerable<Guid> source = chunkCollectorGuids.Union(originalChunkGuids);
            IEnumerable<(int Hash, Guid OriginalChunkGuid)> originalChunkGuidsAndFileNameHashes = (from c in (from c in modifiedChunks.Where(delegate (KeyValuePair<Guid, ChunkAssetEntry> c)
            {
                HandlerExtraData handlerExtraData2 = c.Value.ExtraData as HandlerExtraData;
                return handlerExtraData2 != null && handlerExtraData2.Handler is LegacyCustomActionHandler;
            })
                                                                                                              select (List<LegacyCustomActionHandler.LegacyFileEntry>)((HandlerExtraData)c.Value.ExtraData).Data).SelectMany((List<LegacyCustomActionHandler.LegacyFileEntry> c) => c)
                                                                                                   select (c.Hash, c.OriginalChunkGuid)).Distinct();
            Fifa21PreAction.Results.Clear();
            Fifa21PreAction.ChunksToFind = new ConcurrentDictionary<Guid, object>(source.Select((Guid guid) => new KeyValuePair<Guid, object>(guid, null)));
            List<Task> tasks = new List<Task>();
            foreach (string superBundle in fs.SuperBundles)
            {
                if (fs.ResolvePath(superBundle + ".toc") != "")
                {
                    Fifa21PreAction bundleAction = new Fifa21PreAction(superBundle, null, this);
                    Task task = Task.Run(delegate
                    {
                        bundleAction.Run();
                    });
                    tasks.Add(task);
                }
            }
            await Task.WhenAll(tasks);
            LegacyFileManager_F21.ChunkFileCollectorParser collectorParser = new LegacyFileManager_F21.ChunkFileCollectorParser(null);
            Dictionary<int, Guid> fileNameHashToChunkId = new Dictionary<int, Guid>();
            Dictionary<Guid, List<(LegacyFileEntry, LegacyFileEntry.ChunkCollectorInstance)>> groupedByChunkId = new Dictionary<Guid, List<(LegacyFileEntry, LegacyFileEntry.ChunkCollectorInstance)>>();
            foreach (Guid chunkCollectorGuid in chunkCollectorGuids)
            {
                if (!Fifa21PreAction.Results.TryGetValue(chunkCollectorGuid, out var resultData2))
                {
                    continue;
                }
                MemoryStream collectorStream = resultData2.Item2;
                Dictionary<int, LegacyFileEntry> entries = new Dictionary<int, LegacyFileEntry>();
                collectorParser.Parse(collectorStream, null, entries);
                foreach (KeyValuePair<int, LegacyFileEntry> entry3 in entries)
                {
                    foreach (LegacyFileEntry.ChunkCollectorInstance collectorInstance in entry3.Value.CollectorInstances)
                    {
                        fileNameHashToChunkId[entry3.Key] = collectorInstance.ChunkId;
                        if (!groupedByChunkId.TryGetValue(collectorInstance.ChunkId, out var list))
                        {
                            list = (groupedByChunkId[collectorInstance.ChunkId] = new List<(LegacyFileEntry, LegacyFileEntry.ChunkCollectorInstance)>());
                        }
                        list.Add((entry3.Value, collectorInstance));
                    }
                }
            }
            HashSet<Guid> createdChunks = new HashSet<Guid>();
            foreach (var (fileNameHash, originalChunkGuid) in originalChunkGuidsAndFileNameHashes.ToList())
            {
                if (!fileNameHashToChunkId.TryGetValue(fileNameHash, out var chunkId) || !groupedByChunkId.TryGetValue(chunkId, out var fileEntries))
                {
                    continue;
                }
                (bool, MemoryStream) resultData = default((bool, MemoryStream));
                if ((chunkId == originalChunkGuid && !Fifa21PreAction.Results.TryGetValue(originalChunkGuid, out resultData)) || (chunkId != originalChunkGuid && !Fifa21PreAction.Results.TryGetValue(chunkId, out resultData)) || createdChunks.Contains(originalChunkGuid))
                {
                    continue;
                }
                Dictionary<int, byte[]> modifiedEntries = new Dictionary<int, byte[]>();
                foreach (var entry2 in fileEntries)
                {
                    if (lfeNameHashesToNewChunkGuid.TryGetValue(entry2.Item1.NameHash, out var newChunkId))
                    {
                        MemoryStream decompressedData = new MemoryStream(new CasReader(new MemoryStream(archiveData[modifiedChunks[newChunkId].Sha1].Data)).Read());
                        modifiedEntries[entry2.Item1.NameHash] = decompressedData.ToArray();
                    }
                }
                (byte[] newChunk, long uncompressedSize) tuple2 = LegacyFileManager_F21.RecompressChunk(resultData.Item2, fileEntries, modifiedEntries);
                byte[] modifiedChunkData = tuple2.newChunk;
                long uncompressedSize = tuple2.uncompressedSize;
                Sha1 hash = Sha1.Create(modifiedChunkData);
                modifiedChunks[originalChunkGuid] = new ChunkAssetEntry
                {
                    Size = modifiedChunkData.Length,
                    OriginalSize = uncompressedSize,
                    LogicalOffset = 0u,
                    LogicalSize = (uint)uncompressedSize,
                    Sha1 = hash
                };
                archiveData[hash] = new ArchiveInfo
                {
                    Data = modifiedChunkData
                };
                List<LegacyCustomActionHandler.LegacyFileEntry> previousHandlerExtraData = (List<LegacyCustomActionHandler.LegacyFileEntry>)originalGuidToLegacyHandlerExtraData[originalChunkGuid].Data;
                foreach (var entry in fileEntries)
                {
                    int indexInPreviousHandlerExtraData = previousHandlerExtraData.FindIndex((LegacyCustomActionHandler.LegacyFileEntry lfe) => lfe.Hash == entry.Item1.NameHash);
                    LegacyCustomActionHandler.LegacyFileEntry lfe2 = ((indexInPreviousHandlerExtraData == -1) ? new LegacyCustomActionHandler.LegacyFileEntry() : previousHandlerExtraData[indexInPreviousHandlerExtraData]);
                    if (indexInPreviousHandlerExtraData == -1)
                    {
                        previousHandlerExtraData.Add(lfe2);
                    }
                    lfe2.ChunkId = originalChunkGuid;
                    lfe2.Hash = entry.Item1.NameHash;
                    lfe2.Offset = entry.Item2.ModifiedEntry.Offset;
                    lfe2.Size = entry.Item2.ModifiedEntry.Size;
                    lfe2.CompressedOffset = entry.Item2.ModifiedEntry.CompressedOffset;
                    lfe2.CompressedSize = entry.Item2.ModifiedEntry.CompressedSize;
                }
                originalGuidToLegacyHandlerExtraData[originalChunkGuid].Data = previousHandlerExtraData;
                createdChunks.Add(originalChunkGuid);
            }
        }

        private void WriteArchiveData(string catalog, CasDataEntry casDataEntry)
        {
            List<int> list = new List<int>();
            int num = 1;
            int num2 = 0;
            while (File.Exists(string.Format("{0}\\cas_{1}.cas", catalog, num.ToString("D2"))))
            {
                num++;
            }
            Stream stream = null;
            foreach (Sha1 item in casDataEntry.EnumerateDataRefs())
            {
                ArchiveInfo archiveInfo = archiveData[item];
                if (stream == null || num2 + archiveInfo.Data.Length > 1073741824)
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                        num++;
                    }
                    FileInfo fileInfo3 = new FileInfo(string.Format("{0}\\cas_{1}.cas", catalog, num.ToString("D2")));
                    Directory.CreateDirectory(fileInfo3.DirectoryName);
                    stream = new FileStream(fileInfo3.FullName, FileMode.Create, FileAccess.Write);
                    num2 = 0;
                }
                foreach (CasFileEntry item2 in casDataEntry.EnumerateFileInfos(item))
                {
                    if (item2.Entry != null && item2.Entry.RangeStart != 0 && !item2.FileInfo.IsChunk)
                    {
                        item2.FileInfo.Offset = (uint)(stream.Position + item2.Entry.RangeStart);
                        item2.FileInfo.Size = item2.Entry.RangeEnd - item2.Entry.RangeStart;
                    }
                    else
                    {
                        item2.FileInfo.Offset = (uint)stream.Position;
                        item2.FileInfo.Size = archiveInfo.Data.Length;
                    }
                    item2.FileInfo.FileReference = new ManifestFileRef(item2.FileInfo.FileReference.CatalogIndex, inPatch: true, num);
                }
                stream.Write(archiveInfo.Data, 0, archiveInfo.Data.Length);
                list.Add(num);
                num2 += archiveInfo.Data.Length;
            }
            stream.Dispose();
        }

        private bool DeleteSelectFiles(string modPath)
        {
            if (!Directory.Exists(modPath))
            {
                return false;
            }
            RecursiveDeleteFiles(modPath);
            foreach (string catalog in fs.Catalogs)
            {
                string path = modPath + "/" + catalog;
                if (!Directory.Exists(path))
                {
                    continue;
                }
                string str = fs.ResolvePath("native_patch/" + catalog);
                foreach (string item in Directory.EnumerateFiles(path))
                {
                    FileInfo fileInfo = new FileInfo(item);
                    if (!File.Exists(str + "/" + fileInfo.Name) || (fileInfo.Attributes & FileAttributes.ReparsePoint) == 0)
                    {
                        fileInfo.Delete();
                    }
                }
            }
            return true;
        }

        private bool IsSamePatch(string modPath)
        {
            string originalLayoutTocPath = fs.ResolvePath("native_patch/layout.toc");
            string modLayoutTocPath = modPath + "/layout.toc";
            if (!File.Exists(originalLayoutTocPath))
            {
                return false;
            }
            if (!File.Exists(modLayoutTocPath))
            {
                return false;
            }
            DbObject dbObject = null;
            using (FileStream stream2 = new FileStream(originalLayoutTocPath, FileMode.Open, FileAccess.Read))
            {
                dbObject = new TocReader().Read(stream2);
            }
            DbObject dbObject2 = null;
            using (FileStream stream = new FileStream(modLayoutTocPath, FileMode.Open, FileAccess.Read))
            {
                dbObject2 = new TocReader().Read(stream);
            }
            int value = dbObject.GetValue("head", 0);
            if (dbObject2.GetValue("head", 0) != value)
            {
                Directory.Delete(modPath + "/../", recursive: true);
                return false;
            }
            return true;
        }

        private void RecursiveDeleteFiles(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles())
            {
                if ((fileInfo.Extension == ".cat" || fileInfo.Extension == ".toc" || fileInfo.Extension == ".sb" || fileInfo.Name.Equals("mods.txt")) && !fileInfo.Name.Equals("layout.toc"))
                {
                    fileInfo.Delete();
                }
            }
            foreach (DirectoryInfo subDirInfo in directoryInfo.EnumerateDirectories())
            {
                string subDirPath = Path.Combine(path, subDirInfo.Name);
                RecursiveDeleteFiles(subDirPath);
            }
        }

        private async Task CopyFileIfRequired(string source, string dest)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (dest == null)
            {
                throw new ArgumentNullException("dest");
            }
            if (File.Exists(source))
            {
                bool flag = !File.Exists(dest);
                if (!flag)
                {
                    flag = await CheckCopyRequired().ConfigureAwait(continueOnCapturedContext: false);
                }
                if (flag)
                {
                    File.Copy(source, dest, overwrite: true);
                }
            }
            async Task<bool> CheckCopyRequired()
            {
                using (FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 131072, useAsync: true))
                {
                    using (FileStream destinationStream = new FileStream(dest, FileMode.Open, FileAccess.Read, FileShare.Read, 131072, useAsync: true))
                    {
                        return await AreFilesDifferent(sourceStream, destinationStream);
                    }
                }
            }
        }

        private async Task ExtractEmbeddedResourceIfRequired(string resourceName, string destinationFile)
        {
            if (resourceName == null)
            {
                throw new ArgumentNullException("resourceName");
            }
            if (destinationFile == null)
            {
                throw new ArgumentNullException("destinationFile");
            }
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            using (Stream resourceStream = executingAssembly.GetManifestResourceStream(typeof(Plugin2Executer), resourceName))
            {
                if (resourceStream == null)
                {
                    throw new ArgumentException("Specified embedded resource was not found: " + resourceName + ".", "resourceName");
                }
                using (FileStream destinationStream = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 131072, useAsync: true))
                {

                    bool flag = destinationStream.Length == 0;
                    if (!flag)
                    {
                        flag = await AreFilesDifferent(resourceStream, destinationStream).ConfigureAwait(continueOnCapturedContext: false);
                    }
                    if (flag)
                    {
                        resourceStream.Position = 0L;
                        destinationStream.Position = 0L;
                        destinationStream.SetLength(resourceStream.Length);
                        await resourceStream.CopyToAsync(destinationStream);
                    }
                }
            }
        }

        private async Task<bool> AreFilesDifferent(Stream sourceStream, Stream destinationStream)
        {
            if (sourceStream == null)
            {
                throw new ArgumentNullException("sourceStream");
            }
            if (destinationStream == null)
            {
                throw new ArgumentNullException("destinationStream");
            }
            if (sourceStream.Length != destinationStream.Length)
            {
                return true;
            }
            return false;
        }
    }
}

