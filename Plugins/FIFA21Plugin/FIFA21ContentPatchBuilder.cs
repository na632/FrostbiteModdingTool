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

            var ebxObjects = AssetManager.Instance.EnumerateEbx().Where(x
                => 
            (!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains("content"))
            || (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains("content"))
            ).ToList();
            var resObjects = AssetManager.Instance.EnumerateRes().Where(x
                =>
            (!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains("content"))
            || (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains("content"))
            ).ToList();
            var chunkObjects = AssetManager.Instance.EnumerateChunks().Where(x
                =>
            (!string.IsNullOrEmpty(x.SBFileLocation) && x.SBFileLocation.Contains("content"))
            || (!string.IsNullOrEmpty(x.TOCFileLocation) && x.TOCFileLocation.Contains("content"))
            ).ToList();

            var moddedEBX = parent.modifiedEbx;
            var moddedRES = parent.modifiedRes;
            var moddedChunk = parent.modifiedChunks;

            if (ebxObjects != null && ebxObjects.Any())
            {
                var ebxCount = ebxObjects.Count;
                var resCount = resObjects.Count;
                var chunkCount = chunkObjects.Count;

                var msNewDataSha1Section = new MemoryStream();
                using (NativeWriter nwNewDataStringSection = new NativeWriter(msNewDataSha1Section, leaveOpen: true))
                {
                    foreach (var ebx in ebxObjects.Where(x => !moddedEBX.ContainsKey(x.Name)))
                    {
                        nwNewDataStringSection.Write(ebx.Sha1);
                    }
                    foreach (var res in resObjects.Where(x => !moddedEBX.ContainsKey(x.Name)))
                    {
                        nwNewDataStringSection.Write(res.Sha1);
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

                var msNewDataStringSection = new MemoryStream();
                using (NativeWriter nwNewDataStringSection = new NativeWriter(msNewDataStringSection, leaveOpen: true))
                {
                    foreach (var ebx in ebxObjects.Where(x => !moddedEBX.ContainsKey(x.Name)))
                    {
                        nwNewDataStringSection.WriteNullTerminatedString(ebx.Name);
                    }
                    foreach (var res in resObjects.Where(x => !moddedEBX.ContainsKey(x.Name)))
                    {
                        nwNewDataStringSection.WriteNullTerminatedString(res.Name);
                    }
                    foreach (var ebx in moddedEBX)
                    {
                        nwNewDataStringSection.WriteNullTerminatedString(ebx.Key);
                    }
                    foreach (var res in moddedRES)
                    {
                        nwNewDataStringSection.WriteNullTerminatedString(res.Key);
                    }
                }
                byte[] NewDataStringsSection = msNewDataStringSection.ToArray();

                var msNewDataCatalogSection = new MemoryStream();
                using (NativeWriter nwNewDataCatalogSection = new NativeWriter(msNewDataCatalogSection, leaveOpen: true))
                {
                    foreach (var ebx in ebxObjects.Where(x=> !moddedEBX.ContainsKey(x.Name)))
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

               
            }


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
