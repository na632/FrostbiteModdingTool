using Frosty.Hash;
using FrostySdk;
using FrostySdk.Deobfuscators;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static FIFA23Plugin.AssetLoader_Fifa22;

namespace FIFA23Plugin
{
    public class CASDataLoader
    {
        public TOCFile AssociatedTOCFile { get; set; }
        public string NativeFileLocation { get; set; }

        public List<EbxAssetEntry> CASEBXEntries = new List<EbxAssetEntry>();
        public List<ResAssetEntry> CASRESEntries = new List<ResAssetEntry>();
        public List<ChunkAssetEntry> CASCHUNKEntries = new List<ChunkAssetEntry>();
        public DbObject CASBinaryMeta { get; set; }

        public CASDataLoader(TOCFile inTOC)
        {
            AssociatedTOCFile = inTOC;
        }

        public void Load(int catalog, int cas, List<CASBundle> casBundles)
        {
            NativeFileLocation = AssetManager.Instance.fs.GetFilePath(catalog, cas, false);
            var path = FileSystem.Instance.ResolvePath(NativeFileLocation);// @"E:\Origin Games\FIFA 21\Data\Win32\superbundlelayout\fifa_installpackage_03\cas_03.cas";
            Load(path, casBundles);
        }

        public DbObject Load(string path, List<CASBundle> casBundles)
        {
            DbObject dboAll = new DbObject(false);
            NativeFileLocation = path;
            path = FileSystem.Instance.ResolvePath(NativeFileLocation);
            if (string.IsNullOrEmpty(path))
                return null;

            using (NativeReader nr_cas = new NativeReader(path))
            {
                nr_cas.Position = 0;
                int index = 0;
                foreach (CASBundle casBundle in casBundles.Where(x=>x.TotalSize > 0))
                {
                    if(AssetManager.Instance != null && AssociatedTOCFile != null && AssociatedTOCFile.DoLogging)
                        AssetManager.Instance.logger.Log($"Completed {Math.Round(((double)index / casBundles.Count)*100).ToString()} in {path}");
                    
                    index++;

                    // go back 4 from the magic
                    var actualPos = casBundle.BundleOffset;
                    nr_cas.Position = actualPos;

                    BaseBundleInfo baseBundleInfo = new BaseBundleInfo();
                    baseBundleInfo.Offset = actualPos;
                    baseBundleInfo.Size = casBundle.TotalSize;

                    AssetManager.Instance.bundles.Add(new BundleEntry() { Type = BundleType.None, Name = path + "-" + baseBundleInfo.Offset });

                    //var inner_reader = nr_cas;
                    using (
                        NativeReader inner_reader = new NativeReader(
                        nr_cas.CreateViewStream(baseBundleInfo.Offset, casBundle.TotalSize)
                        ))
                    {
                        var binaryReader = new BinaryReader_Fifa22();
                        var binaryObject = new DbObject();
                        var pos = inner_reader.Position;
                        binaryReader.BinaryRead_FIFA21((int)baseBundleInfo.Offset, ref binaryObject, inner_reader, false);
                        inner_reader.Position = pos;

                        if (AssetManager.Instance != null && AssociatedTOCFile != null)
                        {
                            var EbxObjectList = binaryObject.GetValue<DbObject>("ebx");
                            var ResObjectList = binaryObject.GetValue<DbObject>("res");
                            var ChunkObjectList = binaryObject.GetValue<DbObject>("chunks");

                            var ebxCount = binaryObject.GetValue<DbObject>("ebx").Count;
                            var resCount = binaryObject.GetValue<DbObject>("res").Count;
                            var chunkCount = binaryObject.GetValue<DbObject>("chunks").Count;
                            //
                            for (var i = 0; i < ebxCount; i++)
                            {
                                var ebxobjectinlist = EbxObjectList[i] as DbObject;

                                ebxobjectinlist.SetValue("offset", casBundle.Offsets[i]);
                                ebxobjectinlist.SetValue("size", casBundle.Sizes[i]);

                                ebxobjectinlist.SetValue("CASFileLocation", NativeFileLocation);

                                ebxobjectinlist.SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);
                                ebxobjectinlist.SetValue("SB_CAS_Offset_Position", casBundle.TOCOffsets[i]);
                                ebxobjectinlist.SetValue("SB_CAS_Size_Position", casBundle.TOCSizes[i]);
                                ebxobjectinlist.SetValue("ParentCASBundleLocation", NativeFileLocation);

                                ebxobjectinlist.SetValue("cas", casBundle.TOCCas[i]);
                                ebxobjectinlist.SetValue("catalog", casBundle.TOCCatalog[i]);
                                ebxobjectinlist.SetValue("patch", casBundle.TOCPatch[i]);

                                ebxobjectinlist.SetValue("BundleIndex", BaseBundleInfo.BundleItemIndex);

                            }
                            for (var i = 0; i < resCount; i++)
                            {
                                var resobjectinlist = ResObjectList[i] as DbObject;


                                resobjectinlist.SetValue("offset", casBundle.Offsets[ebxCount + i]);
                                resobjectinlist.SetValue("size", casBundle.Sizes[ebxCount + i]);

                                resobjectinlist.SetValue("CASFileLocation", NativeFileLocation);

                                resobjectinlist.SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);
                                resobjectinlist.SetValue("SB_CAS_Offset_Position", casBundle.TOCOffsets[ebxCount + i]);
                                resobjectinlist.SetValue("SB_CAS_Size_Position", casBundle.TOCSizes[ebxCount + i]);
                                resobjectinlist.SetValue("ParentCASBundleLocation", NativeFileLocation);

                                resobjectinlist.SetValue("cas", casBundle.TOCCas[ebxCount + i]);
                                resobjectinlist.SetValue("catalog", casBundle.TOCCatalog[ebxCount + i]);
                                resobjectinlist.SetValue("patch", casBundle.TOCPatch[ebxCount + i]);

                                resobjectinlist.SetValue("BundleIndex", BaseBundleInfo.BundleItemIndex);

                            }

                            for (var i = 0; i < chunkCount; i++)
                            {
                                var chunkObjectInList = ChunkObjectList[i] as DbObject;

                                chunkObjectInList.SetValue("offset", casBundle.Offsets[ebxCount + resCount + i]);
                                chunkObjectInList.SetValue("size", casBundle.Sizes[ebxCount + resCount + i]);

                                chunkObjectInList.SetValue("CASFileLocation", NativeFileLocation);


                                chunkObjectInList.SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);
                                chunkObjectInList.SetValue("SB_CAS_Offset_Position", casBundle.TOCOffsets[ebxCount + resCount + i]);
                                chunkObjectInList.SetValue("SB_CAS_Size_Position", casBundle.TOCSizes[ebxCount + resCount + i]);
                                chunkObjectInList.SetValue("ParentCASBundleLocation", NativeFileLocation);


                                chunkObjectInList.SetValue("cas", casBundle.TOCCas[ebxCount + resCount + i]);
                                chunkObjectInList.SetValue("catalog", casBundle.TOCCatalog[ebxCount + resCount + i]);
                                chunkObjectInList.SetValue("patch", casBundle.TOCPatch[ebxCount + resCount + i]);

                                chunkObjectInList.SetValue("BundleIndex", BaseBundleInfo.BundleItemIndex);

                            }

                            dboAll.Add(binaryObject);

                            if (AssociatedTOCFile.ProcessData)
                            {


                                foreach (DbObject item in EbxObjectList)
                                {
                                    EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
                                    ebxAssetEntry.Name = item.GetValue<string>("name");
                                    ebxAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
                                    ebxAssetEntry.BaseSha1 = AssetManager.Instance.rm.GetBaseSha1(ebxAssetEntry.Sha1);
                                    ebxAssetEntry.Size = item.GetValue("size", 0L);
                                    ebxAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
                                    ebxAssetEntry.Location = AssetDataLocation.CasNonIndexed;
                                    ebxAssetEntry.ExtraData = new AssetExtraData();
                                    ebxAssetEntry.ExtraData.DataOffset = (uint)item.GetValue("offset", 0L);

                                    int cas = item.GetValue("cas", 0);
                                    int catalog = item.GetValue("catalog", 0);
                                    bool patch = item.GetValue("patch", false);
                                    ebxAssetEntry.ExtraData.CasPath = FileSystem.Instance.GetFilePath(catalog, cas, patch);

                                    ebxAssetEntry.TOCFileLocation = AssociatedTOCFile.NativeFileLocation;
                                    ebxAssetEntry.SB_OriginalSize_Position = item.GetValue("SB_OriginalSize_Position", 0);
                                    ebxAssetEntry.SB_CAS_Offset_Position = item.GetValue("SB_CAS_Offset_Position", 0);
                                    ebxAssetEntry.SB_CAS_Size_Position = item.GetValue("SB_CAS_Size_Position", 0);
                                    ebxAssetEntry.SB_Sha1_Position = item.GetValue("SB_Sha1_Position", 0);

                                    ebxAssetEntry.Type = item.GetValue("Type", string.Empty);
                                    ebxAssetEntry.Bundles.Add(BaseBundleInfo.BundleItemIndex);

                                    //if (AssociatedTOCFile.ProcessData)
                                    //{
                                    //    if (AssetManager.Instance.EBX.ContainsKey(ebxAssetEntry.Name.ToLower()))
                                    //    {
                                    //        var originalEbx = AssetManager.Instance.EBX[ebxAssetEntry.Name.ToLower()];
                                    //        ebxAssetEntry.TOCFileLocations = originalEbx.TOCFileLocations;
                                    //        ebxAssetEntry.TOCFileLocations.Add(AssociatedTOCFile.NativeFileLocation);
                                    //    }
                                        AssetManager.Instance.AddEbx(ebxAssetEntry);
                                    //}
                                }

                                var iRes = 0;
                                foreach (DbObject item in ResObjectList)
                                {
                                    ResAssetEntry resAssetEntry = new ResAssetEntry();
                                    resAssetEntry.Name = item.GetValue<string>("name");
                                    resAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
                                    resAssetEntry.BaseSha1 = AssetManager.Instance.rm.GetBaseSha1(resAssetEntry.Sha1);
                                    resAssetEntry.Size = item.GetValue("size", 0L);
                                    resAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
                                    resAssetEntry.Location = AssetDataLocation.CasNonIndexed;
                                    resAssetEntry.ExtraData = new AssetExtraData();
                                    resAssetEntry.ExtraData.DataOffset = (uint)item.GetValue("offset", 0L);

                                    int cas = item.GetValue("cas", 0);
                                    int catalog = item.GetValue("catalog", 0);
                                    bool patch = item.GetValue("patch", false);
                                    resAssetEntry.ExtraData.CasPath = FileSystem.Instance.GetFilePath(catalog, cas, patch);

                                    resAssetEntry.ResRid = item.GetValue<ulong>("resRid", 0ul);
                                    resAssetEntry.ResType = item.GetValue<uint>("resType", 0);
                                    resAssetEntry.ResMeta = item.GetValue<byte[]>("resMeta", null);

                                    resAssetEntry.CASFileLocation = NativeFileLocation;
                                    resAssetEntry.TOCFileLocation = AssociatedTOCFile.NativeFileLocation;
                                    //resAssetEntry.TOCFileLocations = new HashSet<string>();

                                    resAssetEntry.SB_OriginalSize_Position = item.GetValue("SB_OriginalSize_Position", 0);
                                    resAssetEntry.SB_CAS_Offset_Position = item.GetValue("SB_CAS_Offset_Position", 0);
                                    resAssetEntry.SB_CAS_Size_Position = item.GetValue("SB_CAS_Size_Position", 0);
                                    resAssetEntry.SB_Sha1_Position = item.GetValue("SB_Sha1_Position", 0);

                                    resAssetEntry.Bundles.Add(BaseBundleInfo.BundleItemIndex);
                                    if (AssociatedTOCFile.ProcessData)
                                        AssetManager.Instance.AddRes(resAssetEntry);


                                    iRes++;
                                }

                                var iChunk = 0;
                                foreach (DbObject item in ChunkObjectList)
                                {
                                    ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
                                    chunkAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");

                                    chunkAssetEntry.BaseSha1 = ResourceManager.Instance.GetBaseSha1(chunkAssetEntry.Sha1);
                                    chunkAssetEntry.Size = item.GetValue("size", 0L);
                                    chunkAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
                                    chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
                                    chunkAssetEntry.ExtraData = new AssetExtraData();
                                    chunkAssetEntry.ExtraData.DataOffset = (uint)item.GetValue("offset", 0L);

                                    int cas = item.GetValue("cas", 0);
                                    int catalog = item.GetValue("catalog", 0);
                                    bool patch = item.GetValue("patch", false);
                                    chunkAssetEntry.ExtraData.CasPath = FileSystem.Instance.GetFilePath(catalog, cas, patch);

                                    chunkAssetEntry.Id = item.GetValue<Guid>("id");
                                    chunkAssetEntry.LogicalOffset = item.GetValue<uint>("logicalOffset");
                                    chunkAssetEntry.LogicalSize = item.GetValue<uint>("logicalSize");

                                    chunkAssetEntry.CASFileLocation = NativeFileLocation;
                                    chunkAssetEntry.TOCFileLocation = AssociatedTOCFile.NativeFileLocation;

                                    chunkAssetEntry.SB_OriginalSize_Position = item.GetValue("SB_OriginalSize_Position", 0);
                                    chunkAssetEntry.SB_CAS_Offset_Position = item.GetValue("SB_CAS_Offset_Position", 0);
                                    chunkAssetEntry.SB_CAS_Size_Position = item.GetValue("SB_CAS_Size_Position", 0);
                                    chunkAssetEntry.SB_Sha1_Position = item.GetValue("SB_Sha1_Position", 0);

                                    chunkAssetEntry.Bundles.Add(BaseBundleInfo.BundleItemIndex);
                                    //if (AssociatedTOCFile.ProcessData)
                                        AssetManager.Instance.AddChunk(chunkAssetEntry);

                                    iChunk++;
                                }
                            }
                        }

                    }


                    BaseBundleInfo.BundleItemIndex++;

                }
            }

            return dboAll;
        }




    }


}
