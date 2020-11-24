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
        public FIFA21ContentPatchBuilder(CatalogInfo inCatalogInfo, FrostyModExecutor inParent)
        {
            //catalogInfo = inCatalogInfo;
            parent = inParent;
        }

        public FIFA21ContentPatchBuilder(FrostyModExecutor inParent)
        {
            parent = inParent;
        }

        private List<FrostySdk.CatalogInfo> _catalogs;
        public List<FrostySdk.CatalogInfo> Catalogs 
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
            var dataPath = basePath + "\\Data\\";
            var patchPath = basePath + "\\Patch\\";

            var patchContentTOC = basePath + "\\ModData\\Patch\\Win32\\contentsb.toc";
            var patchContentSB = basePath + "\\ModData\\Patch\\Win32\\contentsb.sb";

            parent.Logger.Log("Started Content Patch Builder");

            var ebxObjects = AssetManager.Instance.EnumerateEbx().Where(x
                => 
            (!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains("content"))
            || (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains("content"))
            ).ToList();
            var resObjects = AssetManager.Instance.EnumerateRes().Where(x
                =>
            ((!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains("content"))
            || (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains("content"))
            && x.Sha1 != Sha1.Zero)
            ).ToList();
            var chunkObjects = AssetManager.Instance.EnumerateChunks().Where(x
                =>
            ((!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains("content"))
            || (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains("content"))
            && x.Sha1 != Sha1.Zero)
            ).ToList();

            var moddedEBX = parent.modifiedEbx;
            var moddedRES = parent.modifiedRes;
            var moddedChunk = parent.modifiedChunks;

            parent.Logger.Log("Finished gathering files to include into Patch");

            if (ebxObjects != null && ebxObjects.Any())
            {
                var SBFileSize = 0;
                File.Delete(patchContentSB);
                using (FileStream fsSB = new FileStream(patchContentSB, FileMode.OpenOrCreate))
                {
                    using (var msSB = new MemoryStream())
                    {
                        using (NativeWriter nwSB = new NativeWriter(msSB, leaveOpen: true))
                        {

                            nwSB.Write((int)32, Endian.Big); // start of bundle
                            nwSB.Write((int)-1, Endian.Big); // End of Meta (dont know yet)
                            nwSB.Write((int)-1, Endian.Big); // CAS for Group Offset
                            nwSB.Write((int)0, Endian.Big); // Unknown (Figure Out)
                            nwSB.Write((int)-1, Endian.Big); // Catalog and CAS Offset
                            nwSB.Write((int)0, Endian.Big); // Unknown (Figure Out)
                            nwSB.Write((int)0, Endian.Big); // Unknown (Figure Out)
                            nwSB.Write((int)0, Endian.Big); // Unknown (Figure Out)

                            var ebxCount = ebxObjects.Count;
                            var resCount = resObjects.Count;
                            var chunkCount = chunkObjects.Count;

                            var totalCount = ebxCount + resCount + chunkCount;

                            var BaseSizePosition = nwSB.BaseStream.Position;
                            nwSB.Write((int)-1, Endian.Big); // Binary Size
                            nwSB.Write((uint)3599661469, Endian.Big); // Magic
                            nwSB.Write((uint)totalCount, Endian.Little); // Total
                            nwSB.Write((uint)ebxCount, Endian.Little); // EBX Count
                            nwSB.Write((uint)resCount, Endian.Little); // Res Count
                            nwSB.Write((uint)chunkCount, Endian.Little); // Chunk Count
                            var StringOffsetPosition = nwSB.BaseStream.Position;
                            nwSB.Write((int)-1, Endian.Little); // String Offset
                            nwSB.Write((int)-1, Endian.Little); // Meta Offset
                            nwSB.Write((int)9, Endian.Little); // Meta Size


                            using (var msNewDataSha1Section = new MemoryStream())
                            {
                                using (NativeWriter nwNewDataStringSection = new NativeWriter(msNewDataSha1Section, leaveOpen: true))
                                {
                                    foreach (var ebx in ebxObjects.Where(x => !moddedEBX.ContainsKey(x.Name)))
                                    {
                                        nwNewDataStringSection.Write(ebx.Sha1);
                                    }
                                    foreach (var res in resObjects.Where(x => !moddedRES.ContainsKey(x.Name)))
                                    {
                                        nwNewDataStringSection.Write(res.Sha1);
                                    }
                                    foreach (var chunk in chunkObjects.Where(x => !moddedChunk.ContainsKey(x.Id)))
                                    {
                                        nwNewDataStringSection.Write(chunk.Sha1);
                                    }
                                    foreach (var ebx in moddedEBX)
                                    {
                                        nwNewDataStringSection.Write(ebx.Value.Sha1);
                                    }
                                    foreach (var res in moddedRES)
                                    {
                                        nwNewDataStringSection.Write(res.Value.Sha1);
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
                                    foreach (var ebx in ebxObjects.Where(x => !moddedEBX.ContainsKey(x.Name)))
                                    {
                                        StringNameToPositionOffset.Add(ebx.Name + "-EBX", (int)msNewDataStringSection.Position);
                                        nwNewDataStringSection.WriteNullTerminatedString(ebx.Name);
                                    }
                                    foreach (var ebx in moddedEBX)
                                    {
                                        StringNameToPositionOffset.Add(ebx.Key + "-EBX", (int)msNewDataStringSection.Position);

                                        nwNewDataStringSection.WriteNullTerminatedString(ebx.Key);
                                    }
                                    foreach (var res in resObjects.Where(x => !moddedRES.ContainsKey(x.Name)))
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
                                    foreach (var ebx in ebxObjects.Where(x => !moddedEBX.ContainsKey(x.Name)))
                                    {
                                        nwNewBinaryDataSection.Write((int)ebx.OriginalSize);
                                        nwNewBinaryDataSection.Write((int)StringNameToPositionOffset[ebx.Name + "-EBX"]);
                                    }
                                    foreach (var ebx in moddedEBX)
                                    {
                                        nwNewBinaryDataSection.Write((int)ebx.Value.OriginalSize);
                                        nwNewBinaryDataSection.Write((int)StringNameToPositionOffset[ebx.Value.Name + "-EBX"]);
                                    }
                                    foreach (var res in resObjects.Where(x => !moddedRES.ContainsKey(x.Name)))
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
                                    foreach (var res in resObjects.Where(x => !moddedRES.ContainsKey(x.Name)))
                                    {
                                        nwNewBinaryDataSection.Write((int)res.ResType);
                                    }
                                    foreach (var res in moddedRES)
                                    {
                                        nwNewBinaryDataSection.Write((int)res.Value.ResType);
                                    }
                                    // ResRid
                                    foreach (var res in resObjects.Where(x => !moddedRES.ContainsKey(x.Name)))
                                    {
                                        nwNewBinaryDataSection.Write((int)res.ResRid);
                                    }
                                    foreach (var res in moddedRES)
                                    {
                                        nwNewBinaryDataSection.Write((int)res.Value.ResRid);
                                    }
                                    // ResMeta
                                    foreach (var res in resObjects.Where(x => !moddedRES.ContainsKey(x.Name)))
                                    {
                                        nwNewBinaryDataSection.Write(res.ResMeta);
                                    }
                                    foreach (var res in moddedRES)
                                    {
                                        nwNewBinaryDataSection.Write(res.Value.ResMeta);
                                    }
                                    foreach (var chunk in chunkObjects.Where(x => !moddedChunk.ContainsKey(x.Id)))
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
                                        var originalChunk = AssetManager.Instance.chunkList[chunk.Key];
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

                                    foreach (var chunk in chunkObjects.Where(x => !moddedChunk.ContainsKey(x.Id)))
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
                                    dbWriter.WriteDbObject("chunkMeta", dbmeta);
                                }
                                byte[] NewDataMetaSection = msNewDataMetaSection.ToArray();
                                //nwSB.Write(NewDataMetaSection);
                                MetaPositionEnd = (int)nwSB.BaseStream.Position;
                            }


                            var CatalogSectionOffsetPosition = nwSB.BaseStream.Position;
                            using (var msNewDataCatalogSection = new MemoryStream())
                            {
                                using (NativeWriter nwNewDataCatalogSection = new NativeWriter(msNewDataCatalogSection, leaveOpen: true))
                                {
                                    foreach (var ebx in ebxObjects.Where(x => !moddedEBX.ContainsKey(x.Name)))
                                    {
                                        ExtractCatalogAndCasIndex(ebx.ExtraData.CasPath, out int catalog, out int cas);
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
                                    foreach (var res in resObjects.Where(x => !moddedRES.ContainsKey(x.Name)))
                                    {
                                        ExtractCatalogAndCasIndex(res.ExtraData.CasPath, out int catalog, out int cas);
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
                                    foreach (var chunk in chunkObjects.Where(x => !moddedChunk.ContainsKey(x.Id)))
                                    {
                                        ExtractCatalogAndCasIndex(chunk.ExtraData.CasPath, out int catalog, out int cas);
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
                                }
                                byte[] NewDataCatalogSection = msNewDataCatalogSection.ToArray();
                                nwSB.Write(NewDataCatalogSection);
                            }

                            var CasGroupSectionOffsetPosition = nwSB.BaseStream.Position;
                            using (var msNewDataCasGroupSection = new MemoryStream())
                            {
                                using (NativeWriter nwNewDataCasGroupSection = new NativeWriter(msNewDataCasGroupSection, leaveOpen: true))
                                {
                                    foreach (var ebx in ebxObjects.Where(x => !moddedEBX.ContainsKey(x.Name)))
                                    {
                                        nwNewDataCasGroupSection.Write((byte)1);
                                    }
                                    foreach (var res in resObjects.Where(x => !moddedRES.ContainsKey(x.Name)))
                                    {
                                        nwNewDataCasGroupSection.Write((byte)1);

                                    }
                                    foreach (var chunk in chunkObjects.Where(x => !moddedChunk.ContainsKey(x.Id)))
                                    {
                                        nwNewDataCasGroupSection.Write((byte)1);
                                    }
                                }
                                byte[] NewDataCasGroupSection = msNewDataCasGroupSection.ToArray();
                                nwSB.Write(NewDataCasGroupSection);
                            }

                            nwSB.BaseStream.Position = 0;
                            nwSB.BaseStream.Position = 4;
                            nwSB.Write((int)MetaPositionEnd);

                            nwSB.BaseStream.Position = 8;
                            nwSB.Write((int)CasGroupSectionOffsetPosition);

                            nwSB.BaseStream.Position = 12;
                            nwSB.Write((int)MetaPosition);

                            nwSB.BaseStream.Position = 16;
                            nwSB.Write((int)CatalogSectionOffsetPosition);

                            nwSB.Write((int)MetaPositionEnd + 36);
                            nwSB.Write((int)MetaPositionEnd + 36);

                            nwSB.BaseStream.Position = BaseSizePosition;
                            nwSB.Write((int)MetaPositionEnd - 36);
                            nwSB.BaseStream.Position = StringOffsetPosition;
                            nwSB.Write((int)StringPosition);
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
                        nwTOC.Write((int)64, Endian.Big);
                        nwTOC.Write((int)1, Endian.Big);
                        nwTOC.Write((int)80, Endian.Big);
                        nwTOC.Write((int)80, Endian.Big);
                        nwTOC.Write((int)0, Endian.Big);
                        nwTOC.Write((int)80, Endian.Big);
                        nwTOC.Write((int)80, Endian.Big);
                        nwTOC.Write((int)80, Endian.Big);
                        nwTOC.Write((int)80, Endian.Big);
                        nwTOC.Write((int)0, Endian.Big);
                        nwTOC.Write((int)7, Endian.Big);
                        nwTOC.Write((int)10, Endian.Big);
                        nwTOC.Write((int)46, Endian.Big);
                        nwTOC.Write((int)120, Endian.Big);
                        nwTOC.Write((int)-1, Endian.Big);
                        nwTOC.Write((int)0, Endian.Big);
                        nwTOC.Write((int)SBFileSize, Endian.Big);
                        nwTOC.Write((int)0, Endian.Big);
                        nwTOC.Write((int)0, Endian.Big);
                        nwTOC.Write(restOfBytes);
                    }
                    var newTOCData = msTOC.ToArray();

                    File.Delete(patchContentTOC);
                    using(NativeWriter nwTOCFile = new NativeWriter(new FileStream(patchContentTOC, FileMode.Create)))
                    {
                        nwTOCFile.Write(newTOCData);
                    }

                }
            }

            
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
