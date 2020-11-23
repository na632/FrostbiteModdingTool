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

        private CatalogInfo catalogInfo;

        private Dictionary<int, string> casFiles = new Dictionary<int, string>();

        public CatalogInfo CatalogInfo => catalogInfo;

        public Dictionary<int, string> CasFiles => casFiles;

        public bool HasErrored => errorException != null;

        public Exception Exception => errorException;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inCatalogInfo"></param>
        /// <param name="inDoneEvent"></param>
        /// <param name="inParent"></param>
        public FIFA21BundleAction(CatalogInfo inCatalogInfo, FrostyModExecutor inParent)
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
            TocCasReader_M21 tocCasReader_M21 = new TocCasReader_M21();
            tocCasReader_M21.Read(tocPath, 0, new BinarySbDataHelper(AssetManager.Instance));
            return true;
        }

        private enum ModType
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

        public void Run()
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


                        Debug.WriteLine($"Modifying CAS file - {casPath}");
                        parent.Logger.Log($"Modifying CAS file - {casPath}");

                        byte[] originalCASArray = null;
                        using (NativeReader readerOfCas = new NativeReader(new FileStream(casPath, FileMode.Open)))
                        {
                            originalCASArray = readerOfCas.ReadToEnd();
                        }
                        File.Delete(casPath);

                        var positionOfNewData = 0;
                        using (NativeWriter nwCas = new NativeWriter(new FileStream(casPath, FileMode.CreateNew)))
                        {
                            nwCas.Write(originalCASArray);
                            foreach (var modItem in item.Value)
                            {
                                byte[] data = new byte[0];
                                AssetEntry originalEntry = null;
                                List<ChunkAssetEntry> ChunkDups = new List<ChunkAssetEntry>();
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

                                        ChunkDups.AddRange(AssetManager.Instance.GetChunkEntries(Guid.Parse(modItem.Item2)).Where(x => x.ExtraData != null && x.ExtraData.DataOffset > 0));
                                        
                                        break;
                                }
                                if (originalEntry != null)
                                {
                                    data = parent.archiveData[modItem.Item1].Data;
                                }
                               
                                if (data.Length > 0)
                                {
                                    // write the new data to end of the file (this should be fine)
                                    positionOfNewData = (int)nwCas.BaseStream.Position;
                                    nwCas.Write(data);

                                    parent.Logger.Log("Writing new asset entry for (" + originalEntry.Name + ")");
                                    Debug.WriteLine("Writing new asset entry for (" + originalEntry.Name + ")");

                                    var sb_cas_size_position = originalEntry.SB_CAS_Size_Position;
                                    var sb_cas_offset_position = originalEntry.SB_CAS_Offset_Position;
                                    var sb_sha1_position = originalEntry.SB_Sha1_Position;
                                    var sb_original_size_position = originalEntry.SB_OriginalSize_Position;// ebxObject.GetValue<int>("SB_OriginalSize_Position");

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

                                    //if (!string.IsNullOrEmpty(originalEntry.CASFileLocation))
                                    //{
                                    //    CasSB = true;
                                    //}

                                    sbpath = parent.fs.ResolvePath(sbpath).ToLower();
                                    sbpath = sbpath.Replace("\\patch", "\\ModData\\Patch".ToLower());
                                    sbpath = sbpath.Replace("\\data", "\\ModData\\Data".ToLower());

                                    parent.Logger.Log($"Writing new entry in ({sbpath})");
                                    Debug.WriteLine($"Writing new entry in ({sbpath})");

                                    if (CasSha1)
                                    {
                                        //nwCas.BaseStream.Position = sb_cas_offset_position;
                                        //nwCas.Write((uint)positionOfNewData, Endian.Big);

                                        //nwCas.BaseStream.Position = sb_cas_size_position;
                                        //nwCas.Write((uint)data.Length, Endian.Big);

                                        if (sb_sha1_position != 0)
                                        {
                                            nwCas.BaseStream.Position = sb_sha1_position;
                                            nwCas.Write(modItem.Item1);
                                        }
                                    }
                                    //else
                                    
                                    
                                    {
                                        //bool CasSha = false;
                                        //if (sbpath.Contains(".toc"))
                                        //{
                                        //    CasSha = true;
                                        //    //nwCas.BaseStream.Position = sb_sha1_position;
                                        //    //nwCas.Write(modItem.Item1);
                                        //}


                                        byte[] arrayOfSB = null;
                                        using (NativeReader nativeReader = new NativeReader(new FileStream(sbpath, FileMode.Open)))
                                        {
                                            arrayOfSB = nativeReader.ReadToEnd();
                                        }
                                        File.Delete(sbpath);
                                        using (NativeWriter nw_sb = new NativeWriter(new FileStream(sbpath, FileMode.OpenOrCreate)))
                                        {
                                            nw_sb.Write(arrayOfSB);
                                            nw_sb.BaseStream.Position = sb_cas_offset_position;
                                            nw_sb.Write((uint)positionOfNewData, Endian.Big);
                                            nw_sb.Flush();

                                            nw_sb.BaseStream.Position = sb_cas_size_position;
                                            nw_sb.Write((uint)data.Length, Endian.Big);
                                            nw_sb.Flush();

                                            if (sb_sha1_position != 0 && !CasSha1)
                                            {
                                                nw_sb.BaseStream.Position = sb_sha1_position;
                                                nw_sb.Write(modItem.Item1);
                                                nw_sb.Flush();
                                            }

                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
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
                AssetManager.Instance.ModCASFiles.Add(CasFileCount, "/native_data/Patch/" + catalogInfo.Name + "/cas_" + num.ToString("D2") + ".cas");
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
