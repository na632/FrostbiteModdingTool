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

        public FIFA21BundleAction(FrostyModExecutor inParent)
        {
            parent = inParent;
            ErrorCounts.Add(ModType.EBX, 0);
            ErrorCounts.Add(ModType.RES, 0);
            ErrorCounts.Add(ModType.CHUNK, 0);
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
                    if (!parent.modifiedChunks.ContainsKey(modLegChunk.Id))
                    {
                        parent.modifiedChunks.Add(modLegChunk.Id, modLegChunk);
                    }
                    else
                    {
                        parent.modifiedChunks[modLegChunk.Id] = modLegChunk;
                    }
                    countLegacyChunksModified++;
                }

                var modifiedChunks = AssetManager.Instance.EnumerateChunks(true);
                foreach(var chunk in modifiedChunks)
                {
                    if (parent.archiveData.ContainsKey(chunk.Sha1))
                        parent.archiveData[chunk.Sha1] = new ArchiveInfo() { Data = chunk.ModifiedEntry.Data };
                    else
                        parent.archiveData.Add(chunk.Sha1, new ArchiveInfo() { Data = chunk.ModifiedEntry.Data });
                }
                parent.Logger.Log($"Legacy :: Modified {countLegacyChunksModified} associated chunks");
            }
            // ------ End of handling Legacy files ---------

            Dictionary<string, List<ModdedFile>> casToMods = new Dictionary<string, List<ModdedFile>>();
            foreach (var modEBX in parent.modifiedEbx)
            {
                var originalEntry = AssetManager.Instance.GetEbxEntry(modEBX.Value.Name);
                if (originalEntry != null && originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                {
                    var casPath = originalEntry.ExtraData.CasPath;
                    if(casPath.Contains("native_patch"))
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
            foreach (var modChunks in parent.modifiedChunks)
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

        public bool Run()
        {
            try
            {
                parent.Logger.Log("Loading files to know what to change.");

                BuildCache buildCache = new BuildCache();
                buildCache.LoadData(ProfilesLibrary.ProfileName, parent.GamePath, parent.Logger, false, true);

                parent.Logger.Log("Loading Cached Super Bundles.");

                CachingSB.Load();

                parent.Logger.Log("Finished loading files. Enumerating modified bundles.");

                var dictOfModsToCas = GetModdedCasFiles();
                if(dictOfModsToCas != null && dictOfModsToCas.Count > 0)
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
                                , AssetManager.Instance.fs.BasePath + "ModData\\Data");
                        }

                        casPath = casPath.Replace("native_patch"
                            , AssetManager.Instance.fs.BasePath + "ModData\\Patch");

                        if (!casPath.Contains("ModData"))
                        {
                            throw new Exception($"WRONG CAS PATH GIVEN! {casPath}");
                        }

                        Debug.WriteLine($"Modifying CAS file - {casPath}");
                        parent.Logger.Log($"Modifying CAS file - {casPath}");

                        Dictionary<AssetEntry, (long,int,int,Sha1)> EntriesToNewPosition = new Dictionary<AssetEntry, (long, int, int, Sha1)>();

                        using (NativeWriter nwCas = new NativeWriter(new FileStream(casPath, FileMode.Open)))
                        {
                            foreach (var modItem in item.Value)
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

                                if (modItem.NamePath.Contains("tattoo", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                //if (modItem.NamePath.Contains("face", StringComparison.OrdinalIgnoreCase) && modItem.ModType == ModType.RES)
                                //    continue;

                                if (modItem.NamePath.Contains("haircap_", StringComparison.OrdinalIgnoreCase) && modItem.ModType == ModType.RES)
                                    continue;

                                if (modItem.NamePath.Contains("hair_", StringComparison.OrdinalIgnoreCase) && modItem.ModType == ModType.RES)
                                    continue;

                                //if (modItem.NamePath.Contains("head", StringComparison.OrdinalIgnoreCase) && modItem.ModType == ModType.RES)
                                //    continue;

                                if (data.Length > 0)
                                {
                                    var positionOfData = nwCas.Position;
                                    // write the new data to end of the file (this should be fine)
                                    nwCas.Write(data);

                                    var origSize = 0;
                                    var out_data = new CasReader(new MemoryStream(data)).Read();
                                    origSize = out_data.Length;

                                    var useCas = string.IsNullOrEmpty(originalEntry.SBFileLocation);
                                    if (useCas)
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


                                    //parent.Logger.Log("Writing new asset entry for (" + originalEntry.Name + ")");
                                    //Debug.WriteLine("Writing new asset entry for (" + originalEntry.Name + ")");
                                    //EntriesToNewPosition.Add(originalEntry, (positionOfData, data.Length, !useCas ? origSize : 0, !useCas ? modItem.Item1 : Sha1.Zero));
                                    EntriesToNewPosition.Add(originalEntry, (positionOfData, data.Length, origSize, modItem.Sha1));


                                    bool patchedInSb = false;
                                    //patchedInSb = PatchInSb(modItem, data, originalEntry, nwCas);

                                    if (!patchedInSb)
                                    {
                                       
                                        //ChangeSB((int)positionOfData, modItem, data.Length, originalEntry, nwCas);
                                       
                                    }

                                   
                                }
                            }
                            
                        }

                        //if(EntriesToNewPosition.Count != item.Value.Count)
                        //{
                        //    parent.Logger.LogError($"Entry data does not match the count that was inputted by Mod");
                        //    return false;
                        //}

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
                            sbpath = sbpath.ToLower().Replace("\\patch", "\\ModData\\Patch".ToLower(), StringComparison.OrdinalIgnoreCase);
                            sbpath = sbpath.ToLower().Replace("\\data", "\\ModData\\Data".ToLower(), StringComparison.OrdinalIgnoreCase);

                            if (!sbpath.ToLower().Contains("moddata", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new Exception($"WRONG SB PATH GIVEN! {sbpath}");
                            }
                            using (NativeWriter nw_sb = new NativeWriter(new FileStream(sbpath, FileMode.Open)))
                            {
                                foreach(var assetBundle in sbGroup.Value)
                                {
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
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        NativeWriter CurrentCasFile;
        int? LastCatalogUsed;

        private bool PatchInSb(Tuple<Sha1, string, ModType, bool> modItem, byte[] data, AssetEntry originalEntry, NativeWriter nwCas)
        {
           
            
            var sb_cas_size_position = originalEntry.SB_CAS_Size_Position;
            var sb_cas_offset_position = originalEntry.SB_CAS_Offset_Position;
            var sb_sha1_position = originalEntry.SB_Sha1_Position;

            var CasSha1 = false;
            var sbpath = string.Empty;
            //parent.fs.ResolvePath(!string.IsNullOrEmpty(originalEntry.SBFileLocation) ?  originalEntry.SBFileLocation : originalEntry.TOCFileLocation);// ebxObject.GetValue<string>("SBFileLocation");
            if (!string.IsNullOrEmpty(originalEntry.SBFileLocation))
                sbpath = originalEntry.SBFileLocation;
            else if (!string.IsNullOrEmpty(originalEntry.TOCFileLocation))
            {
                sbpath = originalEntry.TOCFileLocation;
                CasSha1 = true;
            }

            if (sbpath.Contains(".toc"))
            {
                var p = sbpath.Replace(".toc", ".sb");
                p = p.Replace("native_data", "native_patch");

                var csbs = CachingSB.CachingSBs;
                var fileToAddTo = csbs.FirstOrDefault(x => x.SBFile == p);
                var lastBundle = fileToAddTo.GetLastBundle();

                var moddata_sbfile = AssetManager.Instance.fs.ResolvePath(fileToAddTo.SBFile).Replace("\\patch\\", "\\moddata\\patch\\");
                byte[] sbFile_data;
                using (NativeReader nativeReader = new NativeReader(new FileStream(moddata_sbfile, FileMode.Open)))
                {
                    sbFile_data = nativeReader.ReadToEnd();
                }

                if (sbFile_data.Length > 0)
                {
                    SBFile sbFile = new SBFile();
                    DbObject dboBundle = new DbObject();
                    sbFile.ReadInternalBundle((int)lastBundle.StartOffset, ref dboBundle, new NativeReader(new MemoryStream(sbFile_data)));
                    byte[] new_data = sbFile.WriteInternalBundle(dboBundle, modItem, parent);
                    if (new_data.Length > 0)
                    {

                    }
          
                    //using (NativeWriter nativeWriter = new NativeWriter(new FileStream(moddata_sbfile, FileMode.Open)))
                    //{
                    //    nativeWriter.BaseStream.Position = lastBundle.StartOffset;
                    //    nativeWriter.BaseStream.Position += lastBundle.BinaryDataOffset;
                    //    nativeWriter.Write(lastBundle.BinaryDataData);
                    //}
                }
                else
                {
                    throw new Exception("Mod Data - SB file data doesn't exist!");
                }

                //return true;
            }
            //sbpath = parent.fs.ResolvePath(sbpath).ToLower();
            //sbpath = sbpath.ToLower().Replace("\\patch", "\\ModData\\Patch".ToLower());
            //sbpath = sbpath.ToLower().Replace("\\data", "\\ModData\\Data".ToLower());

            return false;
        }

        private void ChangeSB(int positionOfNewData, Tuple<Sha1, string, ModType, bool> modItem, int sizeOfData, AssetEntry entry, NativeWriter nwCas)
        {
            //if (modItem.Item3 == ModType.RES)
            //    return;

            var sb_cas_size_position = entry.SB_CAS_Size_Position;
            var sb_cas_offset_position = entry.SB_CAS_Offset_Position;
            var sb_sha1_position = entry.SB_Sha1_Position;

            var CasSha1 = false;
            var sbpath = string.Empty;
            //parent.fs.ResolvePath(!string.IsNullOrEmpty(originalEntry.SBFileLocation) ?  originalEntry.SBFileLocation : originalEntry.TOCFileLocation);// ebxObject.GetValue<string>("SBFileLocation");
            if (!string.IsNullOrEmpty(entry.SBFileLocation))
                sbpath = entry.SBFileLocation;
            else if (!string.IsNullOrEmpty(entry.TOCFileLocation))
            {
                sbpath = entry.TOCFileLocation;
                CasSha1 = true;
            }

            sbpath = parent.fs.ResolvePath(sbpath).ToLower();
            sbpath = sbpath.ToLower().Replace("\\patch", "\\ModData\\Patch".ToLower());
            sbpath = sbpath.ToLower().Replace("\\data", "\\ModData\\Data".ToLower());

            if (!sbpath.ToLower().Contains("moddata"))
            {
                throw new Exception($"WRONG SB PATH GIVEN! {sbpath}");
            }

            //parent.Logger.Log($"Writing new entry in ({sbpath})");
            //Debug.WriteLine($"Writing new entry in ({sbpath})");

            if (CasSha1)
            {
                if (sb_sha1_position != 0)
                {
                    nwCas.BaseStream.Position = sb_sha1_position;
                    var tempCasCheck = Sha1.Zero;

                    using (NativeReader tempNRCAS = new NativeReader(new FileStream(AssetManager.Instance.fs.ResolvePath(entry.ExtraData.CasPath), FileMode.Open)))
                    {
                        tempNRCAS.Position = sb_sha1_position;
                        tempCasCheck = tempNRCAS.ReadSha1();
                    }
                    //nwCas.Write(modItem.Item1);
                }
            }


            //byte[] arrayOfSB = null;
            //using (NativeReader nativeReader = new NativeReader(new FileStream(sbpath, FileMode.Open)))
            //{
            //    arrayOfSB = nativeReader.ReadToEnd();
            //    nativeReader.Position = sb_cas_offset_position;
            //    var originalOffset = nativeReader.ReadInt(Endian.Big);
            //}
            //File.Delete(sbpath);
            using (NativeWriter nw_sb = new NativeWriter(new FileStream(sbpath, FileMode.Open)))
            {
                //nw_sb.Position = 0;
                //nw_sb.Write(arrayOfSB);
                nw_sb.BaseStream.Position = sb_cas_offset_position;
                nw_sb.Write((uint)positionOfNewData, Endian.Big);
                //nw_sb.Flush();

                //nw_sb.BaseStream.Position = sb_cas_size_position;
                nw_sb.Write((uint)sizeOfData, Endian.Big);
                //nw_sb.Flush();

                if (sb_sha1_position != 0 && !CasSha1)
                {
                    nw_sb.BaseStream.Position = sb_sha1_position;
                    nw_sb.Write(modItem.Item1);
                    nw_sb.Flush();
                }

            }

                
        }

        private string GetNewCasPath(
            KeyValuePair<string, List<Tuple<Sha1, string, ModType, bool>>> item,
            out string casPath,
            out string sbFilePath,
            out CachingSBData cachingSBData,
            out CachingSBData.Bundle cachingBundle
            )
        {
            casPath = string.Empty;
            sbFilePath = string.Empty;
            cachingSBData = null;
            cachingBundle = null;

            var lastSb = CachingSB.CachingSBs[CachingSB.CachingSBs.Count - 1];
            if(lastSb != null)
            {
                sbFilePath = lastSb.SBFile;
                cachingSBData = lastSb;
                var lastBundle = lastSb.Bundles[lastSb.Bundles.Count - 1];
                if(lastBundle != null)
                {
                    cachingBundle = lastBundle;
                    casPath = AssetManager.Instance.fs.GetFilePath(lastBundle.LastCatalogId, lastBundle.LastCAS, true);
                }

            }
            return casPath;
        }

        private void BuildNewSB(CachingSBData cachingData, CachingSBData.Bundle cachingBundleDataToUse)
        {
            byte[] oldSBFileData;
            var sbPath = AssetManager.Instance.fs.ResolvePath(cachingData.SBFile);
            using (NativeReader nativeReader = new NativeReader(new FileStream(sbPath, FileMode.Open)))
            {
                oldSBFileData = nativeReader.ReadToEnd();
            }
            if(oldSBFileData.Length > 0)
            {

            }
        }

        private NativeWriter GetNextCas(string catalogName, out int casFileIndex)
        {
            int num = 1;
            string text = parent.fs.BasePath + "ModData\\patch\\" + catalogName + "\\cas_" + num.ToString("D2") + ".cas";
            while (File.Exists(text))
            {
                num++;
                text = parent.fs.BasePath + "ModData\\patch\\" + catalogName + "\\cas_" + num.ToString("D2") + ".cas";
            }
            lock (locker)
            {
                casFiles.Add(++CasFileCount, parent.fs.BasePath + "ModData\\patch\\" + catalogName + "/cas_" + num.ToString("D2") + ".cas");
                AssetManager.Instance.ModCASFiles.Add(CasFileCount, parent.fs.BasePath + "ModData\\patch\\" + catalogName + "/cas_" + num.ToString("D2") + ".cas");
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

}
