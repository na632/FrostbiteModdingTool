using Frosty.Hash;
using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Frostbite.IO;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using paulv2k4ModdingExecuter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using static paulv2k4ModdingExecuter.FrostyModExecutor;

namespace FIFA21Plugin
{

    public class FIFA21BundleAction
    {
        public class BundleFileEntry
        {
            public int CasIndex;

            public int Offset;

            public int Size;

            public string Name;

            public BundleFileEntry(int inCasIndex, int inOffset, int inSize, string inName = null)
            {
                CasIndex = inCasIndex;
                Offset = inOffset;
                Size = inSize;
                Name = inName;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return $"CASIdx({CasIndex}), Offset({Offset}), Size({Size}), Name({Name})";

            }
        }

        private static readonly object locker = new object();

        public static int CasFileCount = 0;

        private Exception errorException;

        private ManualResetEvent doneEvent;

        private FrostyModExecutor parent;

        private Catalog catalogInfo;

        private Dictionary<int, string> casFiles = new Dictionary<int, string>();

        public Catalog CatalogInfo => catalogInfo;

        public Dictionary<int, string> CasFiles => casFiles;

        public bool HasErrored => errorException != null;

        public Exception Exception => errorException;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inCatalogInfo"></param>
        /// <param name="inDoneEvent"></param>
        /// <param name="inParent"></param>
        //public FIFA21BundleAction(Catalog inCatalogInfo, FrostyModExecutor inParent)
        //{
        //    catalogInfo = inCatalogInfo;
        //    parent = inParent;
        //}

        private readonly bool UseModData;

        public FIFA21BundleAction(FrostyModExecutor inParent, bool useModData = true)
        {
            parent = inParent;
            ErrorCounts.Add(ModType.EBX, 0);
            ErrorCounts.Add(ModType.RES, 0);
            ErrorCounts.Add(ModType.CHUNK, 0);
            UseModData = useModData;
        }

        public enum ModType
        {
            EBX,
            RES,
            CHUNK
        }

        public struct ModdedFile
        {
            public Sha1 Sha1 { get; set; }
            public string NamePath { get; set; }
            public ModType ModType { get; set; }
            public bool IsAdded { get; set; }
            public AssetEntry OriginalEntry { get; set; }

            public ModdedFile(Sha1 inSha1, string inNamePath, ModType inModType, bool inAdded)
            {
                Sha1 = inSha1;
                NamePath = inNamePath;
                ModType = inModType;
                IsAdded = inAdded;
                OriginalEntry = null;
            }

            public ModdedFile(Sha1 inSha1, string inNamePath, ModType inModType, bool inAdded, AssetEntry inOrigEntry)
            {
                Sha1 = inSha1;
                NamePath = inNamePath;
                ModType = inModType;
                IsAdded = inAdded;
                OriginalEntry = inOrigEntry;
            }

            
        }

        public Dictionary<ModType, int> ErrorCounts = new Dictionary<ModType, int>();

        public List<ChunkAssetEntry> AddedChunks = new List<ChunkAssetEntry>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>cas to Tuple of Sha1, Name, Type, IsAdded</returns>
        private Dictionary<string, List<ModdedFile>> GetModdedCasFiles()
        {
            // Handle Legacy first to generate modified chunks
            ProcessLegacyMods();
            // ------ End of handling Legacy files ---------

            Dictionary<string, List<ModdedFile>> casToMods = new Dictionary<string, List<ModdedFile>>();
            foreach (var modEBX in parent.modifiedEbx)
            {
                var originalEntry = AssetManager.Instance.GetEbxEntry(modEBX.Value.Name);
                if (originalEntry != null && originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                {
                    var casPath = originalEntry.ExtraData.CasPath;
                    if (casPath.Contains("native_patch"))
                    {

                    }

                    if (!casToMods.ContainsKey(casPath))
                    {
                        casToMods.Add(casPath, new List<ModdedFile>() { new ModdedFile(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, false, originalEntry) });
                    }
                    else
                    {
                        casToMods[casPath].Add(new ModdedFile(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, false, originalEntry));
                    }
                    //// Is Added
                    //else
                    //{
                    //    if (!casToMods.ContainsKey(string.Empty))
                    //    {
                    //        casToMods.Add(string.Empty, new List<ModdedFile>() { new ModdedFile(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, true) });
                    //    }
                    //    else
                    //    {
                    //        casToMods[string.Empty].Add(new ModdedFile(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, true));
                    //    }
                    //}
                }
                else
                {
                    ErrorCounts[ModType.EBX]++;
                }
            }
            foreach (var modRES in parent.modifiedRes)
            {
                var originalEntry = AssetManager.Instance.GetResEntry(modRES.Value.Name);
                if (originalEntry != null)
                {
                    if (originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                    {

                        var casPath = originalEntry.ExtraData.CasPath;
                        if (!casToMods.ContainsKey(casPath))
                        {
                            casToMods.Add(casPath, new List<ModdedFile>() { new ModdedFile(modRES.Value.Sha1, modRES.Value.Name, ModType.RES, false, originalEntry) });
                        }
                        else
                        {
                            casToMods[casPath].Add(new ModdedFile(modRES.Value.Sha1, modRES.Value.Name, ModType.RES, false, originalEntry));
                        }
                    }
                    //// Is Added
                    //else
                    //{
                    //    if (!casToMods.ContainsKey(string.Empty))
                    //    {
                    //        casToMods.Add(string.Empty, new List<ModdedFile>() { new ModdedFile(modRES.Value.Sha1, modRES.Value.Name, ModType.EBX, true) });
                    //    }
                    //    else
                    //    {
                    //        casToMods[string.Empty].Add(new ModdedFile(modRES.Value.Sha1, modRES.Value.Name, ModType.EBX, true));
                    //    }
                    //}
                }
                else
                {
                    ErrorCounts[ModType.RES]++;
                }
            }
            foreach (var modChunks in parent.ModifiedChunks)
            {
                var originalEntry = AssetManager.Instance.GetChunkEntry(modChunks.Key);

                if (originalEntry != null && originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                {
                    if (originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                    {
                        var casPath = originalEntry.ExtraData.CasPath;
                        if (!casToMods.ContainsKey(casPath))
                        {
                            casToMods.Add(casPath, new List<ModdedFile>() { new ModdedFile(modChunks.Value.Sha1, modChunks.Key.ToString(), ModType.CHUNK, false, originalEntry) });
                        }
                        else
                        {
                            casToMods[casPath].Add(new ModdedFile(modChunks.Value.Sha1, modChunks.Key.ToString(), ModType.CHUNK, false, originalEntry));
                        }
                    }
                }
                else
                {
                    AddedChunks.Add(modChunks.Value);
                    parent.Logger.LogWarning($"This mod compiler cannot handle Added Chunks. {modChunks.Key} will be ignored.");
                    ErrorCounts[ModType.CHUNK]++;

                    //throw new Exception($"Unable to find CAS file to edit for Chunk {originalEntry.Id}");
                    //parent.Logger.LogWarning($"Unable to find CAS file to edit for Chunk {modChunks.Key}");
                    //parent.Logger.LogWarning("Unable to apply Chunk Entry for mod");
                }
            }



            return casToMods;
        }

        private void ProcessLegacyMods()
        {
            if (parent.modifiedLegacy.Count > 0)
            {
                parent.Logger.Log($"Legacy :: {parent.modifiedLegacy.Count} Legacy files found. Modifying associated chunks");

                Dictionary<string, byte[]> legacyData = new Dictionary<string, byte[]>();
                var countLegacyChunksModified = 0;
                foreach (var modLegacy in parent.modifiedLegacy)
                {
                    var originalEntry = AssetManager.Instance.GetCustomAssetEntry("legacy", modLegacy.Key);
                    var data = parent.archiveData[modLegacy.Value.Sha1].Data;
                    legacyData.Add(modLegacy.Key, data);

                }

                AssetManager.Instance.ModifyLegacyAssets(legacyData, true);

                var modifiedLegacyChunks = AssetManager.Instance.EnumerateChunks(true);
                foreach (var modLegChunk in modifiedLegacyChunks)
                {
                    modLegChunk.Sha1 = modLegChunk.ModifiedEntry.Sha1;
                    if (!parent.ModifiedChunks.ContainsKey(modLegChunk.Id))
                    {
                        parent.ModifiedChunks.Add(modLegChunk.Id, modLegChunk);
                    }
                    else
                    {
                        parent.ModifiedChunks[modLegChunk.Id] = modLegChunk;
                    }
                    countLegacyChunksModified++;
                }

                var modifiedChunks = AssetManager.Instance.EnumerateChunks(true);
                foreach (var chunk in modifiedChunks)
                {
                    if (parent.archiveData.ContainsKey(chunk.Sha1))
                        parent.archiveData[chunk.Sha1] = new ArchiveInfo() { Data = chunk.ModifiedEntry.Data };
                    else
                        parent.archiveData.Add(chunk.Sha1, new ArchiveInfo() { Data = chunk.ModifiedEntry.Data });
                }
                parent.Logger.Log($"Legacy :: Modified {countLegacyChunksModified} associated chunks");
            }
        }

        Dictionary<string, DbObject> SbToDbObject = new Dictionary<string, DbObject>();

        public bool Run()
        {
            //try
            //{
                parent.Logger.Log("Loading files to know what to change.");

                BuildCache buildCache = new BuildCache();
                buildCache.LoadData(ProfilesLibrary.ProfileName, parent.GamePath, parent.Logger, false, true);

                parent.Logger.Log("Loading Cached Super Bundles.");

                //CachingSB.Load();

                parent.Logger.Log("Finished loading files. Enumerating modified bundles.");

                var dictOfModsToCas = GetModdedCasFiles();
            if (dictOfModsToCas != null && dictOfModsToCas.Count > 0)
            {
                if (ErrorCounts.Count > 0)
                {
                    if (ErrorCounts[ModType.EBX] > 0)
                        parent.Logger.Log("EBX ERRORS:: " + ErrorCounts[ModType.EBX]);
                    if (ErrorCounts[ModType.RES] > 0)
                        parent.Logger.Log("RES ERRORS:: " + ErrorCounts[ModType.RES]);
                    if (ErrorCounts[ModType.CHUNK] > 0)
                        parent.Logger.Log("Chunk ERRORS:: " + ErrorCounts[ModType.CHUNK]);
                }

                foreach (var item in dictOfModsToCas)
                {
                    var casPath = string.Empty;
                    if (!string.IsNullOrEmpty(item.Key))
                    {
                        casPath = item.Key.Replace("native_data"
                            , AssetManager.Instance.fs.BasePath + "ModData\\Data", StringComparison.OrdinalIgnoreCase);
                    }

                    casPath = casPath.Replace("native_patch"
                        , AssetManager.Instance.fs.BasePath + "ModData\\Patch", StringComparison.OrdinalIgnoreCase);

                    if (UseModData && !casPath.Contains("ModData"))
                    {
                        throw new Exception($"WRONG CAS PATH GIVEN! {casPath}");
                    }

                    if(!UseModData)
                    {
                        casPath = casPath.Replace("ModData\\", "", StringComparison.OrdinalIgnoreCase);
                    }

                    Debug.WriteLine($"Modifying CAS file - {casPath}");
                    parent.Logger.Log($"Modifying CAS file - {casPath}");

                    Dictionary<AssetEntry, (long, int, int, Sha1)> EntriesToNewPosition = new Dictionary<AssetEntry, (long, int, int, Sha1)>();

                    using (NativeWriter nwCas = new NativeWriter(new FileStream(casPath, FileMode.Open)))
                    {
                        foreach (var modItem in item.Value.OrderBy(x => x.NamePath))
                        {
                            nwCas.Position = nwCas.Length;
                            byte[] data = new byte[0];
                            AssetEntry originalEntry = modItem.OriginalEntry;

                            if (originalEntry != null && parent.archiveData.ContainsKey(modItem.Sha1))
                            {
                                data = parent.archiveData[modItem.Sha1].Data;
                            }
                            else
                            {
                                parent.Logger.LogError($"Unable to find original archive data for {modItem.NamePath}");
                                continue;
                                //throw new Exception()
                            }

                            if (data.Length == 0)
                            {
                                parent.Logger.LogError($"Unable to find any data for {modItem.NamePath}");
                                continue;
                            }

                            var origSize = 0;
                            var positionOfData = nwCas.Position;
                            // write the new data to end of the file (this should be fine)
                            nwCas.Write(data);

                            switch (modItem.ModType)
                            {
                                case ModType.EBX:
                                    origSize = Convert.ToInt32(parent.modifiedEbx[modItem.NamePath].OriginalSize);
                                    break;
                                case ModType.RES:
                                    origSize = Convert.ToInt32(parent.modifiedRes[modItem.NamePath].OriginalSize);
                                    break;
                                case ModType.CHUNK:
                                    origSize = Convert.ToInt32(parent.ModifiedChunks[Guid.Parse(modItem.NamePath)].OriginalSize);
                                    break;
                            }
                            if (origSize == 0
                                || origSize == data.Length)
                            {
                                var out_data = new CasReader(new MemoryStream(data)).Read();
                                origSize = out_data.Length;
                            }

                            var useCas = string.IsNullOrEmpty(originalEntry.SBFileLocation);
                            if (useCas && (originalEntry is EbxAssetEntry || originalEntry is ResAssetEntry))
                            {
                                if (originalEntry.SB_OriginalSize_Position != 0 && origSize != 0)
                                {
                                    nwCas.Position = originalEntry.SB_OriginalSize_Position;
                                    nwCas.Write((uint)origSize, Endian.Little);
                                }

                                if (originalEntry.SB_Sha1_Position != 0 && modItem.Sha1 != Sha1.Zero)
                                {
                                    nwCas.Position = originalEntry.SB_Sha1_Position;
                                    nwCas.Write(modItem.Sha1);
                                }
                            }

                            EntriesToNewPosition.Add(originalEntry, (positionOfData, data.Length, origSize, modItem.Sha1));
                        }

                    }

                    var groupedBySB = EntriesToNewPosition.GroupBy(x =>
                                !string.IsNullOrEmpty(x.Key.SBFileLocation)
                                ? x.Key.SBFileLocation
                                : x.Key.TOCFileLocation
                                )
                        .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

                    foreach (var sbGroup in groupedBySB)
                    {
                        var sbpath = sbGroup.Key;
                        sbpath = parent.fs.ResolvePath(sbpath).ToLower();
                        if (UseModData)
                        {
                            sbpath = sbpath.ToLower().Replace("\\patch", "\\ModData\\Patch".ToLower(), StringComparison.OrdinalIgnoreCase);
                            sbpath = sbpath.ToLower().Replace("\\data", "\\ModData\\Data".ToLower(), StringComparison.OrdinalIgnoreCase);
                        }
                        if (UseModData && !sbpath.ToLower().Contains("moddata", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception($"WRONG SB PATH GIVEN! {sbpath}");
                        }


                        DbObject dboOriginal = null;
                        if (!SbToDbObject.ContainsKey(sbGroup.Key) && !sbpath.Contains(".toc", StringComparison.OrdinalIgnoreCase))
                        {
                            var timeStarted = DateTime.Now;
                            var tocSbReader = new TocSbReader_FIFA21();
                            tocSbReader.DoLogging = false;
                            var dboOriginal2 = tocSbReader.Read(sbpath.Replace(".sb", ".toc", StringComparison.OrdinalIgnoreCase), 0, new BinarySbDataHelper(AssetManager.Instance), sbpath);

                            SbToDbObject.Add(sbGroup.Key, new DbObject(dboOriginal2));
                            Debug.WriteLine("Time Taken to Read SB: " + (DateTime.Now - timeStarted).ToString());
                        }

                        if(SbToDbObject.ContainsKey(sbGroup.Key))
                            dboOriginal = SbToDbObject[sbGroup.Key];

                        using (NativeWriter nw_sb = new NativeWriter(new FileStream(sbpath, FileMode.Open)))
                        {
                            foreach (var assetBundle in sbGroup.Value)
                            {
                                if (dboOriginal != null)
                                {
                                    var origEbxBundles = dboOriginal.List.Where(x => ((DbObject)x).HasValue("ebx")).Select(x => ((DbObject)x).GetValue<DbObject>("ebx")).ToList();


                                    var origResBundles = dboOriginal.List.Where(x => ((DbObject)x).HasValue("res")).Select(x => ((DbObject)x).GetValue<DbObject>("res")).ToList();
                                    DbObject origResDbo = null;
                                    foreach (DbObject dbInBundle in origResBundles)
                                    {
                                        origResDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
                                        if (origResDbo != null)
                                            break;
                                    }
                                    
                                    if (origResDbo != null && assetBundle.Key.Type == "MeshSet")
                                    {
                                        nw_sb.BaseStream.Position = origResDbo.GetValue<int>("SB_ResMeta_Position");
                                        nw_sb.WriteBytes(parent.modifiedRes[assetBundle.Key.Name].ResMeta);

                                    }
                                }

                                var positionOfNewData = assetBundle.Value.Item1;
                                var sizeOfData = assetBundle.Value.Item2;
                                var originalSizeOfData = assetBundle.Value.Item3;
                                var sha = assetBundle.Value.Item4;

                                var sb_cas_size_position = assetBundle.Key.SB_CAS_Size_Position;
                                var sb_cas_offset_position = assetBundle.Key.SB_CAS_Offset_Position;
                                nw_sb.BaseStream.Position = sb_cas_offset_position;
                                nw_sb.Write((uint)positionOfNewData, Endian.Big);
                                nw_sb.Write((uint)sizeOfData, Endian.Big);

                                if (!sbpath.Contains(".toc", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (nw_sb.Length > assetBundle.Key.SB_OriginalSize_Position
                                        &&
                                        assetBundle.Key.SB_OriginalSize_Position != 0 && originalSizeOfData != 0)
                                    {
                                        nw_sb.Position = assetBundle.Key.SB_OriginalSize_Position;
                                        nw_sb.Write((uint)originalSizeOfData, Endian.Little);
                                    }

                                    if (nw_sb.Length > assetBundle.Key.SB_OriginalSize_Position
                                        &&
                                        assetBundle.Key.SB_Sha1_Position != 0 && sha != Sha1.Zero)
                                    {
                                        nw_sb.Position = assetBundle.Key.SB_Sha1_Position;
                                        nw_sb.Write(sha);
                                    }
                                }
                            }
                        }
                    }


                }
            }
            return true;
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

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

}
