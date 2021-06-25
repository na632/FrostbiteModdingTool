using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using paulv2k4ModdingExecuter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FIFA21Plugin
{
    public class FIFA21ContentPatchBuilder
    {
        List<TOCFile> DataTOCFiles = new List<TOCFile>();
        List<CASDataLoader> DataCASLoaders = new List<CASDataLoader>();
        private FrostyModExecutor parent;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inCatalogInfo"></param>
        /// <param name="inDoneEvent"></param>
        /// <param name="inParent"></param>
        public FIFA21ContentPatchBuilder(Catalog inCatalogInfo, FrostyModExecutor inParent)
        {
            //catalogInfo = inCatalogInfo;
            parent = inParent;
        }

        public FIFA21ContentPatchBuilder(FrostyModExecutor inParent)
        {
            parent = inParent;
        }

        private List<FrostySdk.Catalog> _catalogs;
        public List<FrostySdk.Catalog> Catalogs 
        { 
            get
            {
                if(_catalogs == null)
                    _catalogs = AssetManager.Instance.fs.EnumerateCatalogInfos().ToList();

                return _catalogs;
            } 
        }

        public void TransferDataToPatch()
        {
            var basePath = AssetManager.Instance.fs.BasePath;
            var ModData_DataPath = basePath + "\\ModData\\Data\\";
            var ModData_PatchPath = basePath + "\\ModData\\Patch\\";
            var patchPath = basePath + "\\Patch\\";

            parent.Logger.Log("Started Patch Builder");

            foreach (Catalog catalogInfo in AssetManager.Instance.fs.EnumerateCatalogInfos())
            {
                foreach(var sb in catalogInfo.SuperBundles.Keys)
                {
                    string arg = sb;
                    if (catalogInfo.SuperBundles[sb])
                    {
                        arg = sb.Replace("win32", catalogInfo.Name);
                    }
                    string location_toc_file = parent.fs.ResolvePath($"{arg}.toc").ToLower();
                    if (location_toc_file != "")
                    {
                        var modDataTOC = location_toc_file.Replace("\\data", "\\moddata\\patch").Replace("\\patch", "\\moddata\\patch");//, "ModData basePath + "\\ModData\\Patch\\Win32\\contentsb.toc";
                        var modDataSB = modDataTOC.Replace(".toc", ".sb");
                        if (!File.Exists(modDataSB))// || !modDataSB.Contains("contentsb"))
                            continue;

                        parent.Logger.Log($"Working on {modDataSB}");

                        var ebxObjects = AssetManager.Instance.EnumerateEbx().Where(x
                            =>
                        (!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains(arg))
                        || (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains(arg))
                        ).ToList();
                        if (ebxObjects.Any())
                        {
                            var resObjects = AssetManager.Instance.EnumerateRes().Where(x
                                =>
                            ((!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains(arg))
                            || (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains(arg))
                            && x.Sha1 != Sha1.Zero)
                            ).ToList();
                            var chunkObjects = AssetManager.Instance.EnumerateChunks().Where(x
                                =>
                            ((!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains(arg))
                            || (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains(arg))
                            && x.Sha1 != Sha1.Zero)
                            ).ToList();

                            var moddedEBXDict = parent.modifiedEbx;
                            var moddedRESDict = parent.modifiedRes;
                            var moddedChunkDict = parent.ModifiedChunks;

                            var moddedEBX = parent.modifiedEbx.Where(x => ebxObjects.Any(y=>y.Name.ToLower() == x.Key.ToLower())).ToList();
                            //var moddedRES = parent.modifiedRes.Where(x => resObjects.Any(y => y.Name.ToLower() == x.Key.ToLower())).ToList();
                            var moddedRES = new Dictionary<string, ResAssetEntry>().ToList();
                            var moddedChunk = parent.ModifiedChunks.Where(x => chunkObjects.Any(y=>y.Id.ToString() == x.Key.ToString().ToLower())).ToList();

                            var piemontekitmodRes = parent.modifiedRes;
                            var piemontekitResObj = resObjects.Where(x => x.Name.Contains("piemonte_"));
                            var piemontekit = parent.modifiedRes.Where(x => resObjects.Any(y => y.Name.ToLower() == x.Key.ToLower())).ToList();

                            ebxObjects = ebxObjects.Where(x => !moddedEBXDict.ContainsKey(x.Name)).ToList();
                            //resObjects = resObjects.Where(x => !moddedRESDict.ContainsKey(x.Name)).ToList();
                            chunkObjects = chunkObjects.Where(x => !moddedChunkDict.ContainsKey(x.Id)).ToList();


                            if (moddedEBX.Any() || moddedRES.Any() || moddedChunk.Any() )
                            {
                                var SBFileSize = 0;
                                File.Delete(modDataSB);
                                using (FileStream fsSB = new FileStream(modDataSB, FileMode.OpenOrCreate))
                                {
                                    using (var msSB = new MemoryStream())
                                    {
                                        using (NativeWriter nwSB = new NativeWriter(msSB, leaveOpen: true))
                                        {

                                            nwSB.Write((int)32, Endian.Big); // start of bundle  -  0 
                                            nwSB.Write((int)-1, Endian.Big); // End of Meta (dont know yet) - 4
                                            nwSB.Write((int)-1, Endian.Big); // CAS for Group Offset - 8
                                            nwSB.Write((int)0, Endian.Big); // Unknown (Figure Out) - 12
                                            nwSB.Write((int)-1, Endian.Big); // Catalog and CAS Offset - 16
                                            nwSB.Write((int)0, Endian.Big); // Unknown (Figure Out) - 20
                                            nwSB.Write((int)0, Endian.Big); // Unknown (Figure Out) - 24
                                            nwSB.Write((int)0, Endian.Big); // Unknown (Figure Out) - 28

                                            var ebxCount = ebxObjects.Count + moddedEBX.Count();
                                            var resCount = resObjects.Count + moddedRES.Count();
                                            var chunkCount = chunkObjects.Count + moddedChunk.Count();

                                            var totalCount = ebxCount + resCount + chunkCount;

                                            var BaseSizePosition = nwSB.BaseStream.Position;
                                            nwSB.Write((int)-1, Endian.Big); // Binary Size - 32
                                            nwSB.Write((uint)3599661469, Endian.Big); // Magic - 36
                                            nwSB.Write((uint)totalCount, Endian.Little); // Total - 40
                                            nwSB.Write((uint)ebxCount, Endian.Little); // EBX Count - 44 
                                            nwSB.Write((uint)resCount, Endian.Little); // Res Count - 48
                                            nwSB.Write((uint)chunkCount, Endian.Little); // Chunk Count - 52
                                            var StringOffsetPosition = nwSB.BaseStream.Position; 
                                            nwSB.Write((int)-1, Endian.Little); // String Offset - 56
                                            nwSB.Write((int)-1, Endian.Little); // Meta Offset - 60
                                            nwSB.Write((int)-1, Endian.Little); // Meta Size - 64
                                            nwSB.Flush();

                                            using (var msNewDataSha1Section = new MemoryStream())
                                            {
                                                using (NativeWriter nwNewDataStringSection = new NativeWriter(msNewDataSha1Section, leaveOpen: true))
                                                {
                                                    foreach (var ebx in ebxObjects)
                                                    {
                                                        nwNewDataStringSection.Write(ebx.Sha1);
                                                    }
                                                    foreach (var ebx in moddedEBX)
                                                    {
                                                        nwNewDataStringSection.Write(ebx.Value.Sha1);
                                                    }
                                                    foreach (var res in resObjects)
                                                    {
                                                        nwNewDataStringSection.Write(res.Sha1);
                                                    }
                                                    foreach (var res in moddedRES)
                                                    {
                                                        nwNewDataStringSection.Write(res.Value.Sha1);
                                                    }
                                                    foreach (var chunk in chunkObjects)
                                                    {
                                                        nwNewDataStringSection.Write(chunk.Sha1);
                                                    }
                                                    foreach (var chunk in moddedChunk)
                                                    {
                                                        nwNewDataStringSection.Write(chunk.Value.Sha1);
                                                    }
                                                }
                                                byte[] NewDataSha1Section = msNewDataSha1Section.ToArray();
                                                nwSB.Write(NewDataSha1Section);
                                            }

                                            Dictionary<string, int> StringNameToPositionOffset = new Dictionary<string, int>();
                                            var StringPosition = nwSB.BaseStream.Position;
                                            using (var msNewDataStringSection = new MemoryStream())
                                            {
                                                using (NativeWriter nwNewDataStringSection = new NativeWriter(msNewDataStringSection, leaveOpen: true))
                                                {
                                                    foreach (var ebx in ebxObjects)
                                                    {
                                                        StringNameToPositionOffset.Add(ebx.Name + "-EBX", (int)msNewDataStringSection.Position);
                                                        nwNewDataStringSection.WriteNullTerminatedString(ebx.Name);
                                                    }
                                                    foreach (var ebx in moddedEBX)
                                                    {
                                                        StringNameToPositionOffset.Add(ebx.Key + "-EBX", (int)msNewDataStringSection.Position);

                                                        nwNewDataStringSection.WriteNullTerminatedString(ebx.Key);
                                                    }
                                                    foreach (var res in resObjects)
                                                    {
                                                        StringNameToPositionOffset.Add(res.Name + "-RES", (int)msNewDataStringSection.Position);

                                                        nwNewDataStringSection.WriteNullTerminatedString(res.Name);
                                                    }
                                                    foreach (var res in moddedRES)
                                                    {
                                                        StringNameToPositionOffset.Add(res.Key + "-RES", (int)msNewDataStringSection.Position);
                                                        nwNewDataStringSection.WriteNullTerminatedString(res.Key);
                                                    }
                                                }
                                                byte[] NewDataStringsSection = msNewDataStringSection.ToArray();
                                                nwSB.Write(NewDataStringsSection);
                                            }

                                            // --------------------------------------------------
                                            // EBX/RES/Chunk Binary stuff
                                            using (var msNewBinaryDataSection = new MemoryStream())
                                            {
                                                using (NativeWriter nwNewBinaryDataSection = new NativeWriter(msNewBinaryDataSection, leaveOpen: true))
                                                {
                                                    foreach (var ebx in ebxObjects)
                                                    {
                                                        nwNewBinaryDataSection.Write((int)ebx.OriginalSize);
                                                        nwNewBinaryDataSection.Write((int)StringNameToPositionOffset[ebx.Name + "-EBX"]);
                                                    }
                                                    foreach (var ebx in moddedEBX)
                                                    {
                                                        nwNewBinaryDataSection.Write((int)ebx.Value.OriginalSize);
                                                        nwNewBinaryDataSection.Write((int)StringNameToPositionOffset[ebx.Value.Name + "-EBX"]);
                                                    }
                                                    foreach (var res in resObjects)
                                                    {
                                                        nwNewBinaryDataSection.Write((int)res.OriginalSize);
                                                        nwNewBinaryDataSection.Write((int)StringNameToPositionOffset[res.Name + "-RES"]);
                                                    }
                                                    foreach (var res in moddedRES)
                                                    {
                                                        nwNewBinaryDataSection.Write((int)res.Value.OriginalSize);
                                                        nwNewBinaryDataSection.Write((int)StringNameToPositionOffset[res.Value.Name + "-RES"]);
                                                    }
                                                    // ResType
                                                    foreach (var res in resObjects)
                                                    {
                                                        nwNewBinaryDataSection.Write((int)res.ResType);
                                                    }
                                                    foreach (var res in moddedRES)
                                                    {
                                                        nwNewBinaryDataSection.Write((int)res.Value.ResType);
                                                    }
                                                    // ResRid
                                                    foreach (var res in resObjects)
                                                    {
                                                        nwNewBinaryDataSection.Write((int)res.ResRid);
                                                    }
                                                    foreach (var res in moddedRES)
                                                    {
                                                        nwNewBinaryDataSection.Write((int)res.Value.ResRid);
                                                    }
                                                    // ResMeta
                                                    foreach (var res in resObjects)
                                                    {
                                                        nwNewBinaryDataSection.Write(res.ResMeta);
                                                    }
                                                    foreach (var res in moddedRES)
                                                    {
                                                        nwNewBinaryDataSection.Write(res.Value.ResMeta);
                                                    }
                                                    foreach (var chunk in chunkObjects)
                                                    {
                                                        /*
                                                         * id guid = reader.ReadGuid(Endian.Little);
                                dbObject.AddValue("SB_LogicalOffset_Position", reader.Position + baseBundleOffset);
                                uint logicalOffset = reader.ReadUInt(Endian.Little);
                                dbObject.AddValue("SB_OriginalSize_Position", reader.Position + baseBundleOffset);
                                uint size_position = reader.ReadUInt(Endian.Little);*/
                                                        nwNewBinaryDataSection.Write(chunk.Id, Endian.Little);
                                                        nwNewBinaryDataSection.Write((int)chunk.LogicalOffset, Endian.Little);
                                                        nwNewBinaryDataSection.Write((int)chunk.LogicalSize, Endian.Little);
                                                    }
                                                    foreach (var chunk in moddedChunk)
                                                    {
                                                        var originalChunk = AssetManager.Instance.Chunks[chunk.Key];
                                                        nwNewBinaryDataSection.Write(chunk.Value.Id, Endian.Little);
                                                        nwNewBinaryDataSection.Write((int)originalChunk.LogicalOffset, Endian.Little);
                                                        nwNewBinaryDataSection.Write((int)originalChunk.LogicalSize, Endian.Little);
                                                    }
                                                }
                                                byte[] NewBinaryDataSection = msNewBinaryDataSection.ToArray();
                                                nwSB.Write(NewBinaryDataSection);
                                            }




                                            // --------------------------------------------------
                                            // Meta Stuff

                                            var MetaPosition = nwSB.BaseStream.Position;
                                            var MetaPositionEnd = 0;
                                            using (var msNewDataMetaSection = new MemoryStream())
                                            {
                                                using (NativeWriter nwNewDataMetaSection = new NativeWriter(msNewDataMetaSection, leaveOpen: true))
                                                {
                                                    DbObject dbmeta = DbObject.CreateList();
                                                    //nwNewDataMetaSection.WriteFixedSizedString("chunkMeta", 9);

                                                    foreach (var chunk in chunkObjects)
                                                    {
                                                        DbObject innermeta = new DbObject();
                                                        innermeta.AddValue("h32", chunk.H32);
                                                        if (chunk.FirstMip != -1)
                                                        {
                                                            innermeta.AddValue("firstMip", chunk.FirstMip);
                                                        }
                                                        dbmeta.Add(innermeta);
                                                    }
                                                    foreach (var chunk in moddedChunk)
                                                    {
                                                        DbObject innermeta = new DbObject();
                                                        innermeta.AddValue("h32", chunk.Value.H32);
                                                        if (chunk.Value.FirstMip != -1)
                                                        {
                                                            innermeta.AddValue("firstMip", chunk.Value.FirstMip);
                                                        }
                                                        dbmeta.Add(innermeta);
                                                    }

                                                    DbWriter dbWriter = new DbWriter(msNewDataMetaSection);
                                                    nwNewDataMetaSection.Write(dbWriter.WriteDbObject("chunkMeta", dbmeta));
                                                }
                                                byte[] NewDataMetaSection = msNewDataMetaSection.ToArray();
                                                nwSB.Write(NewDataMetaSection);
                                                MetaPositionEnd = (int)nwSB.BaseStream.Position;
                                            }


                                            var CatalogSectionOffsetPosition = nwSB.BaseStream.Position;
                                            using (var msNewDataCatalogSection = new MemoryStream())
                                            {
                                                using (NativeWriter nwNewDataCatalogSection = new NativeWriter(msNewDataCatalogSection, leaveOpen: true))
                                                {
                                                    var lastCasPath = string.Empty;
                                                    int catalog = 1;
                                                    int cas = 1;
                                                    foreach (var ebx in ebxObjects)
                                                    {
                                                        lastCasPath = ebx.ExtraData.CasPath;
                                                        ExtractCatalogAndCasIndex(ebx.ExtraData.CasPath, out catalog, out cas);
                                                        // unk
                                                        nwNewDataCatalogSection.Write((byte)0);
                                                        // patch
                                                        nwNewDataCatalogSection.Write(Convert.ToByte(ebx.ExtraData.IsPatch));
                                                        // catalog
                                                        nwNewDataCatalogSection.Write(Convert.ToByte(catalog));
                                                        // cas
                                                        nwNewDataCatalogSection.Write(Convert.ToByte(cas));
                                                        // offset
                                                        nwNewDataCatalogSection.Write((int)ebx.ExtraData.DataOffset, Endian.Big);
                                                        // size
                                                        nwNewDataCatalogSection.Write((int)ebx.Size, Endian.Big);
                                                    }
                                                    foreach (var res in resObjects)
                                                    {
                                                        lastCasPath = res.ExtraData.CasPath;

                                                        ExtractCatalogAndCasIndex(res.ExtraData.CasPath, out catalog, out cas);
                                                        // unk
                                                        nwNewDataCatalogSection.Write((byte)0);
                                                        // patch
                                                        nwNewDataCatalogSection.Write(Convert.ToByte(res.ExtraData.IsPatch));
                                                        // catalog
                                                        nwNewDataCatalogSection.Write(Convert.ToByte(catalog));
                                                        // cas
                                                        nwNewDataCatalogSection.Write(Convert.ToByte(cas));
                                                        // offset
                                                        nwNewDataCatalogSection.Write((int)res.ExtraData.DataOffset, Endian.Big);
                                                        // size
                                                        nwNewDataCatalogSection.Write((int)res.Size, Endian.Big);
                                                    }
                                                    foreach (var chunk in chunkObjects)
                                                    {
                                                        lastCasPath = chunk.ExtraData.CasPath;

                                                        ExtractCatalogAndCasIndex(chunk.ExtraData.CasPath, out catalog, out cas);
                                                        // unk
                                                        nwNewDataCatalogSection.Write((byte)0);
                                                        // patch
                                                        nwNewDataCatalogSection.Write(Convert.ToByte(chunk.ExtraData.IsPatch));
                                                        // catalog
                                                        nwNewDataCatalogSection.Write(Convert.ToByte(catalog));
                                                        // cas
                                                        nwNewDataCatalogSection.Write(Convert.ToByte(cas));
                                                        // offset
                                                        //nwCas.BaseStream.Position = nwCas.BaseStream.Length;
                                                        nwNewDataCatalogSection.Write((int)chunk.ExtraData.DataOffset, Endian.Big);
                                                        // size
                                                        nwNewDataCatalogSection.Write((int)chunk.Size, Endian.Big);
                                                    }


                                                    var casPath = lastCasPath.Replace("native_data", ModData_DataPath).Replace("native_patch", ModData_PatchPath);// parent.fs.BasePath  parent.fs.ResolvePath(lastCasPath, true);
                                                    using (NativeWriter nwCas = new NativeWriter(new FileStream(casPath, FileMode.Open)))
                                                    {
                                                        nwCas.BaseStream.Position = nwCas.BaseStream.Length;

                                                        foreach (var ebx in moddedEBX)
                                                        {
                                                            var dataPosition = nwCas.BaseStream.Position;
                                                            var mod_data_bytes = parent.archiveData[ebx.Value.Sha1].Data;
                                                            nwCas.Write(mod_data_bytes);

                                                            // unk
                                                            nwNewDataCatalogSection.Write((byte)0);
                                                            // patch
                                                            nwNewDataCatalogSection.Write((byte)1);
                                                            // catalog
                                                            nwNewDataCatalogSection.Write(Convert.ToByte(catalog));
                                                            // cas
                                                            nwNewDataCatalogSection.Write(Convert.ToByte(cas));
                                                            // offset
                                                            nwNewDataCatalogSection.Write(dataPosition, Endian.Big);
                                                            // size
                                                            nwNewDataCatalogSection.Write(mod_data_bytes.Length, Endian.Big);
                                                        }
                                                        foreach (var res in moddedRES)
                                                        {
                                                            var dataPosition = nwCas.BaseStream.Position;
                                                            var mod_data_bytes = parent.archiveData[res.Value.Sha1].Data;
                                                            nwCas.Write(mod_data_bytes);

                                                            // unk
                                                            nwNewDataCatalogSection.Write((byte)0);
                                                            // patch
                                                            nwNewDataCatalogSection.Write((byte)1);
                                                            // catalog
                                                            nwNewDataCatalogSection.Write(Convert.ToByte(catalog));
                                                            // cas
                                                            nwNewDataCatalogSection.Write(Convert.ToByte(cas));
                                                            // offset
                                                            nwNewDataCatalogSection.Write(dataPosition, Endian.Big);
                                                            // size
                                                            nwNewDataCatalogSection.Write(mod_data_bytes.Length, Endian.Big);
                                                        }

                                                        foreach (var chunk in moddedChunk)
                                                        {
                                                            var dataPosition = nwCas.BaseStream.Position;
                                                            var mod_data_bytes = parent.archiveData[chunk.Value.Sha1].Data;
                                                            nwCas.Write(mod_data_bytes);

                                                            // unk
                                                            nwNewDataCatalogSection.Write((byte)0);
                                                            // patch
                                                            nwNewDataCatalogSection.Write((byte)1);
                                                            // catalog
                                                            nwNewDataCatalogSection.Write(Convert.ToByte(catalog));
                                                            // cas
                                                            nwNewDataCatalogSection.Write(Convert.ToByte(cas));
                                                            // offset
                                                            nwNewDataCatalogSection.Write(dataPosition, Endian.Big);
                                                            // size
                                                            nwNewDataCatalogSection.Write(mod_data_bytes.Length, Endian.Big);
                                                        }




                                                    }
                                                }
                                                byte[] NewDataCatalogSection = msNewDataCatalogSection.ToArray();
                                                nwSB.Write(NewDataCatalogSection);
                                            }

                                            var CasGroupSectionOffsetPosition = nwSB.BaseStream.Position;
                                            using (var msNewDataCasGroupSection = new MemoryStream())
                                            {
                                                using (NativeWriter nwNewDataCasGroupSection = new NativeWriter(msNewDataCasGroupSection, leaveOpen: true))
                                                {
                                                    foreach (var ebx in ebxObjects)
                                                    {
                                                        nwNewDataCasGroupSection.Write((byte)1);
                                                    }
                                                    foreach (var res in resObjects)
                                                    {
                                                        nwNewDataCasGroupSection.Write((byte)1);

                                                    }
                                                    foreach (var chunk in chunkObjects)
                                                    {
                                                        nwNewDataCasGroupSection.Write((byte)1);
                                                    }
                                                }
                                                byte[] NewDataCasGroupSection = msNewDataCasGroupSection.ToArray();
                                                nwSB.Write(NewDataCasGroupSection);
                                            }

                                            nwSB.BaseStream.Position = 0;
                                            nwSB.BaseStream.Position = 4;
                                            nwSB.Write((int)MetaPositionEnd, Endian.Big);

                                            nwSB.BaseStream.Position = 8;
                                            nwSB.Write((int)CasGroupSectionOffsetPosition, Endian.Big);

                                            nwSB.BaseStream.Position = 12;
                                            nwSB.Write((int)MetaPosition, Endian.Big);

                                            nwSB.BaseStream.Position = 16;
                                            nwSB.Write((int)CatalogSectionOffsetPosition, Endian.Big);

                                            nwSB.Write((int)MetaPositionEnd, Endian.Big);
                                            nwSB.Write((int)MetaPositionEnd, Endian.Big);

                                            nwSB.BaseStream.Position = BaseSizePosition;
                                            nwSB.Write((int)MetaPositionEnd, Endian.Big);

                                            nwSB.BaseStream.Position = StringOffsetPosition;

                                            // var StringOffsetPosition = nwSB.BaseStream.Position; 
                                            // nwSB.Write((int)-1, Endian.Little); // String Offset - 56
                                            // nwSB.Write((int)-1, Endian.Little); // Meta Offset - 60
                                            // nwSB.Write((int)-1, Endian.Little); // Meta Size - 64
                                            nwSB.Write((int)StringPosition); // String Offset - 56
                                            nwSB.Write((int)MetaPosition); // Meta Offset - 60
                                            nwSB.Write((int)MetaPositionEnd - (int)MetaPosition); // Meta Size - 64
                                        }

                                        msSB.Position = 0;
                                        msSB.CopyTo(fsSB);
                                        SBFileSize = (int)msSB.Length;
                                    }
                                }

                                using (NativeReader nrOriginalTOC = new NativeReader(new FileStream(patchPath + "\\win32\\futsb.toc", FileMode.Open)))
                                {
                                    var startBytes = nrOriginalTOC.ReadBytes(560);
                                    nrOriginalTOC.Position = 636;
                                    var restOfBytes = nrOriginalTOC.ReadBytes((int)nrOriginalTOC.Length - (int)nrOriginalTOC.Position);
                                    var msTOC = new MemoryStream();
                                    using (NativeWriter nwTOC = new NativeWriter(msTOC, leaveOpen: true))
                                    {
                                        nwTOC.Write(startBytes);
                                        nwTOC.Write((int)64, Endian.Big); // 4
                                        nwTOC.Write((int)1, Endian.Big); // 8
                                        nwTOC.Write((int)80, Endian.Big);  // 12
                                        nwTOC.Write((int)80, Endian.Big);  // 16
                                        nwTOC.Write((int)0, Endian.Big);  // 20
                                        nwTOC.Write((int)80, Endian.Big);  // 24
                                        nwTOC.Write((int)80, Endian.Big);  // 28
                                        nwTOC.Write((int)80, Endian.Big);  // 32
                                        nwTOC.Write((int)80, Endian.Big);  // 36
                                        nwTOC.Write((int)0, Endian.Big); // 40

                                        //nwTOC.Write((int)7, Endian.Big);
                                        nwTOC.Write((int)1, Endian.Big); // 44
                                        //nwTOC.Write((int)10, Endian.Big);
                                        nwTOC.Write((int)0, Endian.Big); // 48
                                        //nwTOC.Write((int)46, Endian.Big);
                                        nwTOC.Write((int)0, Endian.Big);
                                        //nwTOC.Write((int)120, Endian.Big);
                                        nwTOC.Write((int)0, Endian.Big);
                                        nwTOC.Write((int)-1, Endian.Big);
                                        nwTOC.Write((int)0, Endian.Big);
                                        nwTOC.Write((int)SBFileSize, Endian.Big);
                                        nwTOC.Write((int)0, Endian.Big);
                                        nwTOC.Write((int)0, Endian.Big);
                                        //nwTOC.Write(restOfBytes);
                                    }
                                    var newTOCData = msTOC.ToArray();
                                    msTOC.Position = 0;
                                    using(NativeReader nrTOC_TEST = new NativeReader(msTOC))
                                    {
                                        TOCFile tocTest = new TOCFile();
                                        tocTest.Read(nrTOC_TEST);
                                    }

                                    File.Delete(modDataTOC);
                                    using (NativeWriter nwTOCFile = new NativeWriter(new FileStream(modDataTOC, FileMode.Create)))
                                    {
                                        nwTOCFile.Write(newTOCData);
                                    }

                                }
                            }


                        }
                    }
                }
                
            }
            //    var basePath = AssetManager.Instance.fs.BasePath;
            //var dataPath = basePath + "\\Data\\";
            //var ModData_DataPath = basePath + "\\ModData\\Data\\";
            //var ModData_PatchPath = basePath + "\\ModData\\Patch\\";

            //var patchContentTOC = basePath + "\\ModData\\Patch\\Win32\\contentsb.toc";
            //var patchContentSB = basePath + "\\ModData\\Patch\\Win32\\contentsb.sb";

            //parent.Logger.Log("Started Content Patch Builder");

            //var ebxObjects = AssetManager.Instance.EnumerateEbx().Where(x
            //    => 
            //(!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains("content"))
            //|| (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains("content"))
            //).ToList();
            //var resObjects = AssetManager.Instance.EnumerateRes().Where(x
            //    =>
            //((!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains("content"))
            //|| (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains("content"))
            //&& x.Sha1 != Sha1.Zero)
            //).ToList();
            //var chunkObjects = AssetManager.Instance.EnumerateChunks().Where(x
            //    =>
            //((!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains("content"))
            //|| (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains("content"))
            //&& x.Sha1 != Sha1.Zero)
            //).ToList();

            

            //var moddedEBX = parent.modifiedEbx;
            //var moddedRES = parent.modifiedRes;
            //var moddedChunk = parent.modifiedChunks;

            //ebxObjects = ebxObjects.Where(x => !moddedEBX.ContainsKey(x.Name)).ToList();
            //resObjects = resObjects.Where(x => !moddedRES.ContainsKey(x.Name)).ToList();
            //chunkObjects = chunkObjects.Where(x => !moddedChunk.ContainsKey(x.Id)).ToList();

            //parent.Logger.Log("Finished gathering files to include into Patch");

            
            parent.Logger.Log("Written Data and Mod files into Patch");


        }

        //private NativeWriter GetNextCas(string originalEntryPath, out int casFileIndex)
        //{

        //    int num = 1;
        //    string text = AssetManager.Instance.fs.BasePath + "ModData\\patch\\" + catalogInfo.Name + "\\cas_" + num.ToString("D2") + ".cas";
        //    while (File.Exists(text))
        //    {
        //        num++;
        //        text = parent.fs.BasePath + "ModData\\patch\\" + catalogInfo.Name + "\\cas_" + num.ToString("D2") + ".cas";
        //    }
        //    lock (locker)
        //    {
        //        casFiles.Add(++CasFileCount, "/native_data/Patch/" + catalogInfo.Name + "/cas_" + num.ToString("D2") + ".cas");
        //        AssetManager.Instance.ModCASFiles.Add(CasFileCount, "/native_data/Patch/" + catalogInfo.Name + "/cas_" + num.ToString("D2") + ".cas");
        //        casFileIndex = CasFileCount;
        //    }
        //    FileInfo fileInfo = new FileInfo(text);
        //    if (!Directory.Exists(fileInfo.DirectoryName))
        //    {
        //        Directory.CreateDirectory(fileInfo.DirectoryName);
        //    }
        //    return new NativeWriter(new FileStream(text, FileMode.Create));
        //}

        private void ExtractCatalogAndCasIndex(string casPath, out int catalog, out int cas)
        {

            catalog = Catalogs.FirstOrDefault(x => casPath.ToLower().Contains(x.Name.ToLower())).PersistentIndex.Value;
            cas = Convert.ToInt32(casPath.ToLower().Substring(casPath.Length - 6, 2));
        }
    }
}
