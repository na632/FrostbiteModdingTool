using Frosty.Hash;
using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Frostbite.IO;
using FrostySdk.IO;
using FrostySdk.Managers;
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
        public FIFA21BundleAction(Catalog inCatalogInfo, FrostyModExecutor inParent)
        {
            catalogInfo = inCatalogInfo;
            parent = inParent;
        }

        public FIFA21BundleAction(FrostyModExecutor inParent)
        {
            parent = inParent;
        }

        private bool CheckTocCasReadCorrectly(string tocPath)
        {
            //TocCasReader_M21 tocCasReader_M21 = new TocCasReader_M21();
            //tocCasReader_M21.Read(tocPath, 0, new BinarySbDataHelper(AssetManager.Instance));
            return true;
        }

        public enum ModType
        {
            EBX,
            RES,
            CHUNK
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>cas to Tuple of Sha1, Name, Type, IsAdded</returns>
        private Dictionary<string, List<Tuple<Sha1, string, ModType, bool>>> GetModdedCasFiles()
        {
            Dictionary<string, List<Tuple<Sha1, string, ModType, bool>>> casToMods = new Dictionary<string, List<Tuple<Sha1, string, ModType, bool>>>();
            foreach (var modEBX in parent.modifiedEbx)
            {
                var originalEntry = AssetManager.Instance.GetEbxEntry(modEBX.Value.Name);
                if (originalEntry != null)
                {
                    if (originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                    {
                        var casPath = originalEntry.ExtraData.CasPath;
                        if (!casToMods.ContainsKey(casPath))
                        {
                            casToMods.Add(casPath, new List<Tuple<Sha1, string, ModType, bool>>() { new Tuple<Sha1, string, ModType, bool>(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, false) });
                        }
                        else
                        {
                            casToMods[casPath].Add(new Tuple<Sha1, string, ModType, bool>(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, false));
                        }
                    }
                    // Is Added
                    else
                    {
                        if (!casToMods.ContainsKey(string.Empty))
                        {
                            casToMods.Add(string.Empty, new List<Tuple<Sha1, string, ModType, bool>>() { new Tuple<Sha1, string, ModType, bool>(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, true) });
                        }
                        else
                        {
                            casToMods[string.Empty].Add(new Tuple<Sha1, string, ModType, bool>(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, true));
                        }
                    }
                }
            }
            foreach (var modRES in parent.modifiedRes)
            {
                var originalEntry = AssetManager.Instance.GetEbxEntry(modRES.Value.Name);
                if (originalEntry != null)
                {
                    if (originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                    {

                        var casPath = originalEntry.ExtraData.CasPath;
                        if (!casToMods.ContainsKey(casPath))
                        {
                            casToMods.Add(casPath, new List<Tuple<Sha1, string, ModType, bool>>() { new Tuple<Sha1, string, ModType, bool>(modRES.Value.Sha1, modRES.Value.Name, ModType.RES, false) });
                        }
                        else
                        {
                            casToMods[casPath].Add(new Tuple<Sha1, string, ModType, bool>(modRES.Value.Sha1, modRES.Value.Name, ModType.RES, false));
                        }
                    }
                    // Is Added
                    else
                    {
                        if (!casToMods.ContainsKey(string.Empty))
                        {
                            casToMods.Add(string.Empty, new List<Tuple<Sha1, string, ModType, bool>>() { new Tuple<Sha1, string, ModType, bool>(modRES.Value.Sha1, modRES.Value.Name, ModType.EBX, true) });
                        }
                        else
                        {
                            casToMods[string.Empty].Add(new Tuple<Sha1, string, ModType, bool>(modRES.Value.Sha1, modRES.Value.Name, ModType.EBX, true));
                        }
                    }
                }
            }
            foreach (var modChunks in parent.modifiedChunks)
            {
                var originalEntry = AssetManager.Instance.GetChunkEntry(modChunks.Value.Id);
                if (originalEntry != null && originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                {
                    if (originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                    {
                        var casPath = originalEntry.ExtraData.CasPath;
                        if (!casToMods.ContainsKey(casPath))
                        {
                            casToMods.Add(casPath, new List<Tuple<Sha1, string, ModType, bool>>() { new Tuple<Sha1, string, ModType, bool>(modChunks.Value.Sha1, modChunks.Value.Id.ToString(), ModType.CHUNK, false) });
                        }
                        else
                        {
                            casToMods[casPath].Add(new Tuple<Sha1, string, ModType, bool>(modChunks.Value.Sha1, modChunks.Value.Id.ToString(), ModType.CHUNK, false));
                        }
                    }
                    // Is Added
                    else
                    {
                        if (!casToMods.ContainsKey(string.Empty))
                        {
                            casToMods.Add(string.Empty, new List<Tuple<Sha1, string, ModType, bool>>() { new Tuple<Sha1, string, ModType, bool>(modChunks.Value.Sha1, modChunks.Value.Name, ModType.EBX, true) });
                        }
                        else
                        {
                            casToMods[string.Empty].Add(new Tuple<Sha1, string, ModType, bool>(modChunks.Value.Sha1, modChunks.Value.Name, ModType.EBX, true));
                        }
                    }

                }
            }

            return casToMods;
        }

        CachingSBData CachingSBData = new CachingSBData();
        List<ChunkAssetEntry> ChunkDups = new List<ChunkAssetEntry>();

        public bool Run()
        {
            try
            {
                parent.Logger.Log("Loading files to know what to change.");

                //FIFA21AssetLoader assetLoader = new FIFA21AssetLoader();
                //assetLoader.Load(AssetManager.Instance, new BinarySbDataHelper(AssetManager.Instance));
                BuildCache buildCache = new BuildCache();
                buildCache.LoadData(ProfilesLibrary.ProfileName, parent.GamePath, parent.Logger, false, true); ;

                parent.Logger.Log("Loading Cached Super Bundles.");

                CachingSB.Load();

                parent.Logger.Log("Finished loading files. Enumerating modified bundles.");

                var dictOfModsToCas = GetModdedCasFiles();
                if(dictOfModsToCas != null && dictOfModsToCas.Count > 0)
                {
                    foreach (var item in dictOfModsToCas) 
                    {
                        var casPath = string.Empty;
                        if (!string.IsNullOrEmpty(item.Key))
                        {
                            casPath = item.Key.Replace("native_data"
                                , AssetManager.Instance.fs.BasePath + "ModData\\Data");
                        }

                        //if (item.Key == string.Empty || item.Key.Contains("native_data"))
                        //{
                        //    GetNewCasPath(item, out casPath, out string sbFilePath, out CachingSBData data, out CachingSBData.Bundle bundle);
                        //    BuildNewSB(data, bundle);
                        //    continue;
                        //}
                        
                        casPath = casPath.Replace("native_patch"
                            , AssetManager.Instance.fs.BasePath + "ModData\\Patch");


                        if (!casPath.Contains("ModData"))
                        {
                            throw new Exception($"WRONG CAS PATH GIVEN! {casPath}");
                        }

                        Debug.WriteLine($"Modifying CAS file - {casPath}");
                        parent.Logger.Log($"Modifying CAS file - {casPath}");

                        using (NativeWriter nwCas = new NativeWriter(new FileStream(casPath, FileMode.Open)))
                        {
                            foreach (var modItem in item.Value)
                            {
                                nwCas.Position = nwCas.Length;
                                byte[] data = new byte[0];
                                AssetEntry originalEntry = null;
                                switch(modItem.Item3)
                                {
                                    case ModType.EBX:
                                        originalEntry = AssetManager.Instance.GetEbxEntry(modItem.Item2);
                                        break;
                                    case ModType.RES:
                                        originalEntry = AssetManager.Instance.GetResEntry(modItem.Item2);
                                        break;
                                    case ModType.CHUNK:
                                        originalEntry = AssetManager.Instance.GetChunkEntry(Guid.Parse(modItem.Item2));

                                        ChunkDups.Clear();
                                        //ChunkDups.AddRange(
                                        //    AssetManager.Instance.GetChunkEntries(Guid.Parse(modItem.Item2)).Where(x=>x.Sha1 != originalEntry.Sha1));

                                        ChunkDups.AddRange(
                                            AssetManager.Instance.GetChunkEntries(Guid.Parse(modItem.Item2))
                                            .Where(x=>x.SB_CAS_Offset_Position != originalEntry.SB_CAS_Offset_Position));

                                        if(ChunkDups.Count > 0)
                                        {
                                            var otherEntry = ChunkDups[0];
                                        }
                                        break;
                                }
                                if (originalEntry != null)
                                {
                                    data = parent.archiveData[modItem.Item1].Data;
                                    if(modItem.Item3 == ModType.CHUNK)
                                        data = originalEntry.ModifiedEntry.Data;
                                }

                                if (data.Length > 0)
                                {
                                    var positionOfData = nwCas.Position;
                                    // write the new data to end of the file (this should be fine)
                                    nwCas.Write(data);

                                    parent.Logger.Log("Writing new asset entry for (" + originalEntry.Name + ")");
                                    Debug.WriteLine("Writing new asset entry for (" + originalEntry.Name + ")");

                                    bool patchedInSb = false;
                                    //patchedInSb = PatchInSb(modItem, data, originalEntry, nwCas);

                                    if (!patchedInSb)
                                    {
                                        if(ChunkDups.Count > 0)
                                        {
                                            Debug.WriteLine(casPath);
                                            Debug.WriteLine(ChunkDups[0].ExtraData.CasPath);

                                            ChangeSB((int)positionOfData, modItem, data.Length, ChunkDups[0], nwCas);
                                        }
                                        else
                                        {
                                            ChangeSB((int)positionOfData, modItem, data.Length, originalEntry, nwCas);
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

        private void ChangeSB(int positionOfNewData, Tuple<Sha1, string, ModType, bool> modItem, int sizeOfData, AssetEntry originalEntry, NativeWriter nwCas)
        {
            if (modItem.Item3 == ModType.RES)
                return;

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

                    using (NativeReader tempNRCAS = new NativeReader(new FileStream(AssetManager.Instance.fs.ResolvePath(originalEntry.ExtraData.CasPath), FileMode.Open)))
                    {
                        tempNRCAS.Position = sb_sha1_position;
                        tempCasCheck = tempNRCAS.ReadSha1();
                    }
                    //nwCas.Write(modItem.Item1);
                }
            }


            byte[] arrayOfSB = null;
            using (NativeReader nativeReader = new NativeReader(new FileStream(sbpath, FileMode.Open)))
            {
                arrayOfSB = nativeReader.ReadToEnd();
                nativeReader.Position = sb_cas_offset_position;
                var originalOffset = nativeReader.ReadInt(Endian.Big);
            }
            //File.Delete(sbpath);
            using (NativeWriter nw_sb = new NativeWriter(new FileStream(sbpath, FileMode.Open)))
            {
                nw_sb.Position = 0;
                nw_sb.Write(arrayOfSB);
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
