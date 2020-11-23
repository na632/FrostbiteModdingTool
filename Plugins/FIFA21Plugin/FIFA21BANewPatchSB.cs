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

    public class FIFA21BANewPatchSB
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
        public FIFA21BANewPatchSB(CatalogInfo inCatalogInfo, FrostyModExecutor inParent)
        {
            catalogInfo = inCatalogInfo;
            parent = inParent;
        }

        public FIFA21BANewPatchSB(FrostyModExecutor inParent)
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

                        if (item.Key == string.Empty || item.Key.Contains("native_data"))
                        {
                            GetNewCasPath(item, out casPath, out string sbFilePath, out CachingSBData data, out CachingSBData.Bundle bundle);
                            BuildNewSB(data, bundle, item, casPath);
                            continue;
                        }

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

                                    var CasSB = false;
                                    var sbpath = string.Empty;
                                    //parent.fs.ResolvePath(!string.IsNullOrEmpty(originalEntry.SBFileLocation) ?  originalEntry.SBFileLocation : originalEntry.TOCFileLocation);// ebxObject.GetValue<string>("SBFileLocation");
                                    if (!string.IsNullOrEmpty(originalEntry.SBFileLocation))
                                        sbpath = originalEntry.SBFileLocation;
                                    else if (!string.IsNullOrEmpty(originalEntry.TOCFileLocation))
                                        sbpath = originalEntry.TOCFileLocation;
                                    else if (!string.IsNullOrEmpty(originalEntry.CASFileLocation))
                                    {
                                        sbpath = originalEntry.CASFileLocation;
                                        CasSB = true;
                                    }

                                    sbpath = parent.fs.ResolvePath(sbpath);
                                    sbpath = sbpath.Replace("\\patch", "\\ModData\\Patch");
                                    sbpath = sbpath.Replace("\\data", "\\ModData\\Data");

                                    parent.Logger.Log($"Writing new entry in ({sbpath})");
                                    Debug.WriteLine($"Writing new entry in ({sbpath})");

                                    if (CasSB)
                                    {
                                        //nwCas.BaseStream.Position = sb_cas_offset_position;
                                        //nwCas.Write((uint)positionOfNewData, Endian.Big);

                                        //nwCas.BaseStream.Position = sb_cas_size_position;
                                        //nwCas.Write((uint)data.Length, Endian.Big);

                                        //if (sb_sha1_position != 0)
                                        //{
                                        //    nwCas.BaseStream.Position = sb_sha1_position;
                                        //    nwCas.Write(modItem.Item1);
                                        //}
                                    }
                                    else
                                    {
                                        bool CasSha = false;
                                        if (sbpath.Contains(".toc"))
                                        {
                                            CasSha = true;
                                            //nwCas.BaseStream.Position = sb_sha1_position;
                                            //nwCas.Write(modItem.Item1);
                                        }


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

                                            if (sb_sha1_position != 0 && !CasSha)
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
            KeyValuePair<string, List<Tuple<Sha1, string, ModType, bool>>> modItems,
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


            foreach (var modItem in modItems.Value) 
            {
                AssetEntry originalEntry = null;

                switch (modItem.Item3)
                {
                    case ModType.EBX:
                        originalEntry = AssetManager.Instance.GetEbxEntry(modItem.Item2);
                        break;
                    case ModType.RES:
                        originalEntry = AssetManager.Instance.GetResEntry(modItem.Item2);
                        break;
                    case ModType.CHUNK:
                        originalEntry = AssetManager.Instance.GetChunkEntry(Guid.Parse(modItem.Item2));
                        break;
                }
                if (originalEntry != null)
                {
                    //CachingSB.CachingSBs.Where(x => originalEntry. .SBFile);.

                }
            }


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

        private void BuildNewSB(CachingSBData cachingData
            , CachingSBData.Bundle cachingBundleDataToUse
            , KeyValuePair<string, List<Tuple<Sha1, string, ModType, bool>>> item
            , string casPath
            )
        {
            casPath = AssetManager.Instance.fs.ResolvePath(casPath);
            using (NativeWriter nwCas = new NativeWriter(new FileStream(casPath, FileMode.Open)))
            {
                byte[] oldSBFileData;
                var sbPath = AssetManager.Instance.fs.ResolvePath(cachingData.SBFile);
                sbPath = sbPath.Replace("\\patch", "\\ModData\\Patch");
                sbPath = sbPath.Replace("\\data", "\\ModData\\Data");

                using (NativeReader nativeReader = new NativeReader(new FileStream(sbPath, FileMode.Open)))
                {
                    oldSBFileData = nativeReader.ReadToEnd();
                }
                File.Delete(sbPath);
                if (oldSBFileData.Length > 0)
                {
                    var newLocationOfBundle = 0;
                    using (NativeReader nrOriginalSB = new NativeReader(new MemoryStream(oldSBFileData)))
                    {
                        nrOriginalSB.BaseStream.Position = cachingBundleDataToUse.StartOffset;
                        var bundleData = nrOriginalSB.ReadBytes((int)cachingBundleDataToUse.BooleanOfCasGroupOffsetEnd);

                        using (NativeWriter nwNewSB = new NativeWriter(new FileStream(sbPath, FileMode.CreateNew)))
                        {
                            nwNewSB.Write(oldSBFileData);
                            newLocationOfBundle = (int)nwNewSB.BaseStream.Position;
                            nwNewSB.BaseStream.Position = newLocationOfBundle;

                            /*
                             * 
                                uint CatalogOffset = binarySbReader2.ReadUInt(Endian.Big);
                                uint unk1 = binarySbReader2.ReadUInt(Endian.Big) + CatalogOffset;
                                uint casFileForGroupOffset = binarySbReader2.ReadUInt(Endian.Big);
                                var unk2 = binarySbReader2.ReadUInt(Endian.Big);
                                uint CatalogAndCASOffset = binarySbReader2.ReadUInt(Endian.Big);
                             */
                            var CatalogOffsetPosition = nwNewSB.BaseStream.Position;
                            nwNewSB.Write(cachingBundleDataToUse.BundleHeader.CatalogOffset, Endian.Big);
                            var Unk1Position = nwNewSB.BaseStream.Position;
                            nwNewSB.Write(cachingBundleDataToUse.BundleHeader.unk1, Endian.Big);
                            var CasFileForGroupOffsetPosition = nwNewSB.BaseStream.Position;
                            nwNewSB.Write(cachingBundleDataToUse.BundleHeader.casFileForGroupOffset, Endian.Big);
                            var Unk2Position = nwNewSB.BaseStream.Position;
                            nwNewSB.Write(cachingBundleDataToUse.BundleHeader.unk2, Endian.Big);
                            var CatalogAndCASOffset = nwNewSB.BaseStream.Position;
                            nwNewSB.Write(cachingBundleDataToUse.BundleHeader.CatalogAndCASOffset, Endian.Big);
                            /* 3 unknowns of 4 byte groups (ints?)
                             * 
                             */
                            nwNewSB.BaseStream.Position += 12;
                            // ------------------------------------------------
                            // Binary stuff with all the names of EBX/RES/Chunk

                            /*
                            size = nr.ReadInt(Endian.Big) + AdditionalHeaderLength;
                            magicStuff = nr.ReadUInt(Endian.Big);
                            if (magicStuff != 3599661469)
                                throw new Exception("Magic/Hash is not right, expecting 3599661469");

                            totalCount = nr.ReadInt(Endian.Little);
                            ebxCount = nr.ReadInt(Endian.Little);
                            resCount = nr.ReadInt(Endian.Little);
                            chunkCount = nr.ReadInt(Endian.Little);
                            stringOffset = nr.ReadInt(Endian.Little) + AdditionalHeaderLength;
                            metaOffset = nr.ReadInt(Endian.Little) + AdditionalHeaderLength;
                            metaSize = nr.ReadInt(Endian.Little) + AdditionalHeaderLength;
                            */
                            var additionalEBXCount = item.Value.Count(x => x.Item3 == ModType.EBX);
                            var additionalRESCount = item.Value.Count(x => x.Item3 == ModType.RES);
                            var additionalChunkCount = item.Value.Count(x => x.Item3 == ModType.CHUNK);
                            var additionalTotalCount = additionalEBXCount + additionalRESCount + additionalChunkCount;

                            var BinarySizePosition = nwNewSB.BaseStream.Position;
                            nrOriginalSB.Position = nwNewSB.BaseStream.Position;
                            nwNewSB.Write(0, Endian.Big); // Size  - HeaderLength
                            nwNewSB.Write(3599661469, Endian.Big); // Magic
                            nwNewSB.Write(additionalTotalCount, Endian.Little); // Total Count of EBX/RES/Chunk
                            nwNewSB.Write(additionalEBXCount, Endian.Little); // EBX Count
                            nwNewSB.Write(additionalRESCount, Endian.Little); // RES Count
                            nwNewSB.Write(additionalChunkCount, Endian.Little); // Chunk Count
                            var BinaryStringOffsetPosition = nwNewSB.BaseStream.Position;
                            nwNewSB.Write(0, Endian.Little); // String Offset - HeaderLength
                            var BinaryMetaOffsetPosition = nwNewSB.BaseStream.Position;
                            nwNewSB.Write(0, Endian.Little); // Meta Offset - HeaderLength
                            var BinaryMetaSizePosition = nwNewSB.BaseStream.Position;
                            nwNewSB.Write(0, Endian.Little); // Meta Size - HeaderLength
                            nwNewSB.Flush();

                            var msNewSha1Section = new MemoryStream();
                            using (NativeWriter nwNewSha1Section = new NativeWriter(msNewSha1Section, leaveOpen: true))
                            {
                                foreach (var it in item.Value)
                                {
                                    nwNewSha1Section.Write(it.Item1);
                                }
                            }
                            byte[] NewSha1Section = msNewSha1Section.ToArray();
                            nwNewSB.Write(NewSha1Section);

                            var ObjectsPosition = nwNewSB.BaseStream.Position;
                            var msNewEBXSection = new MemoryStream();
                            using (NativeWriter nwNewEBXSection = new NativeWriter(msNewEBXSection, leaveOpen: true))
                            {
                                foreach (var it in item.Value.Where(x=>x.Item3 == ModType.EBX))
                                {
                                    //nwNewEBXSection.Write((int)Sha1ToStringPosition[it.Item1]);
                                    nwNewEBXSection.Write((int)0);
                                    nwNewEBXSection.Write((int)parent.modifiedEbx[it.Item2].OriginalSize);
                                }
                            }
                            byte[] NewEBXSection = msNewEBXSection.ToArray();
                            nwNewSB.Write(NewEBXSection);

                            var msNewRESSection = new MemoryStream();
                            using (NativeWriter nwNewRESSection = new NativeWriter(msNewRESSection, leaveOpen: true))
                            {
                                foreach (var it in item.Value.Where(x => x.Item3 == ModType.RES))
                                {
                                    nwNewRESSection.Write((int)0);
                                    //nwNewRESSection.Write((int)Sha1ToStringPosition[it.Item1]);
                                }
                            }
                            byte[] NewRESSection = msNewRESSection.ToArray();
                            nwNewSB.Write(NewRESSection);

                            var msNewChunkSection = new MemoryStream();
                            using (NativeWriter nwNewChunkSection = new NativeWriter(msNewChunkSection, leaveOpen: true))
                            {
                                foreach (var it in item.Value.Where(x => x.Item3 == ModType.CHUNK))
                                {
                                }
                            }
                            byte[] NewChunkSection = msNewChunkSection.ToArray();
                            nwNewSB.Write(NewChunkSection);

                            var StringsPosition = nwNewSB.BaseStream.Position;
                            var msNewStringsSection = new MemoryStream();
                            Dictionary<Sha1, long> Sha1ToStringPosition = new Dictionary<Sha1, long>();
                            using (NativeWriter nwNewStringsSection = new NativeWriter(msNewStringsSection, leaveOpen: true))
                            {
                                foreach (var it in item.Value)
                                {
                                    Sha1ToStringPosition.Add(it.Item1, nwNewStringsSection.BaseStream.Position);
                                    nwNewStringsSection.WriteNullTerminatedString(it.Item2);
                                }
                            }
                            byte[] NewStringsSection = msNewStringsSection.ToArray();
                            nwNewSB.Write(NewStringsSection);


                            var DataCatalogPosition = nwNewSB.BaseStream.Position;
                            var msNewDataCatalogSection = new MemoryStream();
                            using (NativeWriter nwNewDataCatalogSection = new NativeWriter(msNewDataCatalogSection, leaveOpen: true))
                            {
                                foreach (var it in item.Value)
                                {
                                    var moddata = parent.archiveData[it.Item1].Data;
                                    // unk
                                    nwNewDataCatalogSection.Write((byte)0);
                                    // patch
                                    nwNewDataCatalogSection.Write((byte)1);
                                    // catalog
                                    nwNewDataCatalogSection.Write((byte)cachingBundleDataToUse.LastCatalogId);
                                    // cas
                                    nwNewDataCatalogSection.Write((byte)cachingBundleDataToUse.LastCAS);
                                    // offset
                                    nwCas.BaseStream.Position = nwCas.BaseStream.Length;
                                    nwNewDataCatalogSection.Write((int)nwCas.BaseStream.Position, Endian.Big);
                                    nwCas.Write(moddata);
                                    // size
                                    nwNewDataCatalogSection.Write((int)moddata.Length, Endian.Big);
                                }
                            }
                            byte[] NewDataCatalogSection = msNewDataCatalogSection.ToArray();
                            nwNewSB.Write(NewDataCatalogSection);

                            var DataCASChangePosition = nwNewSB.BaseStream.Position;
                            var msNewCASChangeSection = new MemoryStream();
                            using (NativeWriter nwNewCASChangeSection = new NativeWriter(msNewCASChangeSection, leaveOpen: true))
                            {
                                foreach (var it in item.Value)
                                {
                                    nwNewCASChangeSection.Write((byte)1);
                                }
                            }
                            byte[] NewCASChangeSection = msNewCASChangeSection.ToArray();
                            nwNewSB.Write(NewCASChangeSection);


                            // Set String Position
                            nwNewSB.BaseStream.Position = BinaryStringOffsetPosition;
                            nwNewSB.Write((int)StringsPosition - cachingBundleDataToUse.StartOffset, Endian.Little);

                            nwNewSB.Flush();
                        }
                    }

                    var tocPath = sbPath.Replace(".sb", ".toc");
                    byte[] originalTOCData;
                    using (NativeReader nrOriginalTOC = new NativeReader(new FileStream(tocPath, FileMode.Open)))
                    {
                        originalTOCData = nrOriginalTOC.ReadToEnd();
                    }
                    File.Delete(tocPath);
                    var locationOfBundleBytes = BitConverter.GetBytes(cachingBundleDataToUse.StartOffset);
                    BoyerMoore boyerMoore = new BoyerMoore(locationOfBundleBytes.Reverse().ToArray());
                    var lstOfLocationsInTOC = boyerMoore.SearchAll(originalTOCData);

                    using (NativeWriter nwNewTOC = new NativeWriter(new FileStream(tocPath, FileMode.CreateNew)))
                    {
                        nwNewTOC.Write(originalTOCData);
                        nwNewTOC.BaseStream.Position = lstOfLocationsInTOC.First();
                        nwNewTOC.Write(newLocationOfBundle, Endian.Big);
                    }
                }
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
