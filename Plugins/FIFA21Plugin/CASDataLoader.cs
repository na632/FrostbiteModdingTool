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
using static FIFA21Plugin.FIFA21AssetLoader;

namespace FIFA21Plugin
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

        public void Load(AssetManager parent, int catalog, int cas, List<CASBundle> casBundles)
        {
            NativeFileLocation = parent.fs.GetFilePath(catalog, cas, false);
            var path = parent.fs.ResolvePath(NativeFileLocation);// @"E:\Origin Games\FIFA 21\Data\Win32\superbundlelayout\fifa_installpackage_03\cas_03.cas";
            Load(parent, path, casBundles);
        }

        public void Load(AssetManager parent, string path, List<CASBundle> casBundles)
        {
            NativeFileLocation = path;
            path = parent.fs.ResolvePath(NativeFileLocation);// @"E:\Origin Games\FIFA 21\Data\Win32\superbundlelayout\fifa_installpackage_03\cas_03.cas";

            //var mem_stream = new MemoryStream();
            //using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            //{
            //    fs.CopyTo(mem_stream);
            //}
            //mem_stream.Position = 0;

            using (NativeReader nr_cas = new NativeReader(
                //mem_stream
                new FileStream(path, FileMode.Open, FileAccess.Read)
                )
                )
            {

                //List<int> PositionOfReadableItems = new List<int>() { 445679993, 448409056 };  // SearchBytePattern(new byte[] { 0xD6, 0x8E, 0x79, 0x9D }, nr_cas.ReadToEnd()).ToList();
                //List<int> PositionOfReadableItems = new List<int>() { 443937737 };  // SearchBytePattern(new byte[] { 0xD6, 0x8E, 0x79, 0x9D }, nr_cas.ReadToEnd()).ToList();
                //List<int> PositionOfReadableItems = new List<int>() { 445678420 };  // SearchBytePattern(new byte[] { 0xD6, 0x8E, 0x79, 0x9D }, nr_cas.ReadToEnd()).ToList();

                nr_cas.Position = 0;
                int index = 0;
                foreach (var casBundle in casBundles)
                {
                    AssetManager.Instance.logger.Log($"Completed {Math.Round(((double)index / casBundles.Count)*100,2).ToString()} in {path}");
                    index++;

                    // go back 4 from the magic
                    var actualPos = casBundle.BundleOffset;
                    //var nextActualPos = PositionOfReadableItems.Count > index + 1 ? PositionOfReadableItems[index + 1] - 4 : nr_cas.Length - actualPos;
                    nr_cas.Position = actualPos;

                    BaseBundleInfo baseBundleInfo = new BaseBundleInfo();
                    baseBundleInfo.Offset = actualPos;
                    //baseBundleInfo.Size = nextActualPos;
                    baseBundleInfo.Size = casBundle.TotalSize;

                    //var inner_reader = nr_cas;
                    using (
                        NativeReader inner_reader = new NativeReader(
                        nr_cas.CreateViewStream(baseBundleInfo.Offset, casBundle.TotalSize)
                        ))
                    {
                        DbObject obj = new DbObject();

                        var dbObj = new DbObject();

                        var startPosition = inner_reader.Position;
                        if (File.Exists("debugCASViewStream.dat"))
                            File.Delete("debugCASViewStream.dat");
                        using (NativeWriter writer = new NativeWriter(new FileStream("debugCASViewStream.dat", FileMode.OpenOrCreate)))
                        {
                            writer.Write(inner_reader.ReadToEnd());
                        }
                        inner_reader.Position = startPosition;


                        var size = inner_reader.ReadInt(Endian.Big) + 4;
                        var magicStuff = inner_reader.ReadUInt(Endian.Big);
                        if (magicStuff != 3599661469) 
                            return;
                            //throw new Exception("Magic/Hash is not right, expecting 3599661469");

                        var totalCount = inner_reader.ReadInt(Endian.Little);
                        var ebxCount = inner_reader.ReadInt(Endian.Little);
                        var resCount = inner_reader.ReadInt(Endian.Little);
                        var chunkCount = inner_reader.ReadInt(Endian.Little);
                        if(ebxCount + resCount + chunkCount != totalCount) return;
                        //throw new Exception("Total Count is not right");

                        if (totalCount != casBundle.Offsets.Count) 
                            return;

                        var stringOffset = inner_reader.ReadInt(Endian.Little) + 4;
                        var metaOffset = inner_reader.ReadInt(Endian.Little) + 4;
                        var metaSize = inner_reader.ReadInt(Endian.Little) + 4;

                        DbObject FullObjectList = new DbObject();
                        FullObjectList.AddValue("dataOffset", size);
                        FullObjectList.AddValue("stringsOffset", stringOffset);
                        FullObjectList.AddValue("metaOffset", metaOffset);
                        FullObjectList.AddValue("metaSize", metaSize);

                        var posBeforeChunkMeta = inner_reader.Position;
                        if (chunkCount != 0)
                        {
                            //using (DbReader dbReader = new DbReader(nativeReader.CreateViewStream(SBHeaderInformation.metaOffset + BaseBundleItem.Offset, nativeReader.Length - binarySbReader2.Position), new NullDeobfuscator()))
                            using (DbReader dbReader = new DbReader(inner_reader.CreateViewStream(metaOffset, inner_reader.Length - metaSize), new NullDeobfuscator()))
                            {
                                var o = dbReader.ReadDbObject();
                                FullObjectList.AddValue("chunkMeta", o);

                                CASBinaryMeta = o;
                            }
                        }
                        inner_reader.Position = posBeforeChunkMeta;

                        List<long> Sha1Positions = new List<long>();
                        List<Sha1> sha1 = new List<Sha1>();
                        for (int i = 0; i < totalCount; i++)
                        {
                            Sha1Positions.Add(inner_reader.Position + baseBundleInfo.Offset);
                            sha1.Add(inner_reader.ReadSha1());
                        }

                        List<DbObject> EbxObjectList = new List<DbObject>();
                        for (int i = 0; i < ebxCount; i++)
                        {
                            DbObject dbObject = new DbObject(new Dictionary<string, object>());
                            dbObject.AddValue("AssetType", "EBX");

                            dbObject.AddValue("SB_StringOffsetPosition", inner_reader.Position + baseBundleInfo.Offset);
                            uint num = inner_reader.ReadUInt(Endian.Little);
                            dbObject.AddValue("StringOffsetPos", num);

                            dbObject.AddValue("SB_OriginalSize_Position", inner_reader.Position + baseBundleInfo.Offset);
                            uint originalSize = inner_reader.ReadUInt(Endian.Little);
                            dbObject.AddValue("originalSize", originalSize);
                            //dbObject.AddValue("size", originalSize);
                            
                            dbObject.AddValue("cas", casBundle.Cas);
                            dbObject.AddValue("catalog", casBundle.Catalog);

                            dbObject.AddValue("SB_Sha1_Position", Sha1Positions[i]);
                            dbObject.AddValue("sha1", sha1[i]);

                            EbxObjectList.Add(dbObject);
                        }
                        List<DbObject> ResObjectList = new List<DbObject>();
                        for (int i = 0; i < resCount; i++)
                        {
                            DbObject dbObject = new DbObject(new Dictionary<string, object>());
                            dbObject.AddValue("AssetType", "RES");

                            dbObject.AddValue("SB_StringOffsetPosition", inner_reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                            uint num = inner_reader.ReadUInt(Endian.Little);
                            dbObject.AddValue("StringOffsetPos", num);

                            dbObject.AddValue("SB_OriginalSize_Position", inner_reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                            uint originalSize = inner_reader.ReadUInt(Endian.Little);
                            dbObject.AddValue("originalSize", originalSize);
                            dbObject.AddValue("size", originalSize);

                            dbObject.AddValue("cas", casBundle.Cas);
                            dbObject.AddValue("catalog", casBundle.Catalog);

                            dbObject.AddValue("SB_Sha1_Position", Sha1Positions[ebxCount + i]);
                            dbObject.AddValue("sha1", sha1[ebxCount + i]);

                            ResObjectList.Add(dbObject);
                        }
                        // RES Types
                        for (var i = 0; i < resCount; i++)
                        {
                            var type = inner_reader.ReadUInt(Endian.Little);
                            var resType = (ResourceType)type;
                            ResObjectList[i].AddValue("resType", type);
                            ResObjectList[i].AddValue("resType2", resType);
                        }
                        // RES Meta ??? 
                        for (var i = 0; i < resCount; i++)
                        {
                            var resMeta = inner_reader.ReadBytes(16);
                            ResObjectList[i].AddValue("resMeta", resMeta);
                        }

                        List<ulong> ResIdList = new List<ulong>();
                        // ResId ??
                        for (int i = 0; i < resCount; i++)
                        {
                            var resRid = inner_reader.ReadULong(Endian.Little);
                            ResIdList.Add(resRid);
                        }


                        List<DbObject> ChunkObjectList = new List<DbObject>();
                        for (int i = 0; i < chunkCount; i++)
                        {
                            DbObject dbObject = new DbObject(new Dictionary<string, object>());
                            dbObject.AddValue("AssetType", "CHUNK");

                            dbObject.AddValue("SB_Guid_Position", inner_reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                            Guid guid = inner_reader.ReadGuid(Endian.Little);
                            if (guid.ToString() == "c03a15a9-6747-22dd-c760-af2e149e6223") // Juventus Test
                            {

                            }
                            dbObject.AddValue("SB_LogicalOffset_Position", inner_reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                            uint logicalOffset = inner_reader.ReadUInt(Endian.Little);
                            dbObject.AddValue("SB_OriginalSize_Position", inner_reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                            uint chunkSize = inner_reader.ReadUInt(Endian.Little);
                            long origSize = (logicalOffset & 0xFFFF) | chunkSize;
                            dbObject.AddValue("id", guid);
                            dbObject.AddValue("logicalOffset", logicalOffset);
                            dbObject.AddValue("logicalSize", chunkSize);
                            dbObject.AddValue("originalSize", origSize);

                            dbObject.AddValue("cas", casBundle.Cas);
                            dbObject.AddValue("catalog", casBundle.Catalog);

                            dbObject.AddValue("SB_Sha1_Position", Sha1Positions[ebxCount + resCount + i]);
                            dbObject.AddValue("sha1", sha1[ebxCount + resCount + i]);

                            if (AssetManager.Instance.chunkList.ContainsKey(guid))
                            {
                                AssetManager.Instance.chunkList[guid].SB_Sha1_Position = (int)Sha1Positions[ebxCount + resCount + i];
                                AssetManager.Instance.chunkList[guid].Sha1 = sha1[ebxCount + resCount + i];
                                AssetManager.Instance.chunkList[guid].CASFileLocation = NativeFileLocation;
                            }
                            ChunkObjectList.Add(dbObject);
                        }

                        var positionBeforeStringSearch = inner_reader.Position;
                        var stringsLength = 0;
                        for (var i = 0; i < ebxCount; i++)
                        {
                            var string_offset = EbxObjectList[i].GetValue<int>("StringOffsetPos");

                            inner_reader.Position = stringOffset + string_offset;
                            var name = inner_reader.ReadNullTerminatedString();
                            stringsLength += name.Length + 1;
                            EbxObjectList[i].AddValue("name", name);
                            EbxObjectList[i].AddValue("nameHash", Fnv1.HashString(name));

                        }

                        for (var i = 0; i < resCount; i++)
                        {
                            var string_offset = ResObjectList[i].GetValue<int>("StringOffsetPos");

                            inner_reader.Position = stringOffset + string_offset;
                            var name = inner_reader.ReadNullTerminatedString();
                            stringsLength += name.Length + 1;

                            ResObjectList[i].AddValue("name", name);
                            ResObjectList[i].AddValue("nameHash", Fnv1.HashString(name));
                        }

                        //inner_reader.Position = metaOffset + metaSize - 4;

                        //FullObjectList.AddValue("dataOffset", inner_reader.Position);
                        inner_reader.Position = positionBeforeStringSearch + stringsLength;
                        //inner_reader.Position = 387;

                        // 
                        // This length should be 1186 for Man City Blueprints at 445678420
                        //var fndEBXLength = 0;
                        //for (var i = 0; i < (ebxCount); i++)
                        //{
                        //    fndEBXLength += EbxObjectList[i].GetValue("size", 1);
                        //}
                        //BoyerMoore boyerSearch = new BoyerMoore();

                        //EbxObjectList[0].SetValue("size", 198);
                        //EbxObjectList[1].SetValue("size", 341);
                        //EbxObjectList[2].SetValue("size", 647);

                        var dataOffset = baseBundleInfo.Offset + inner_reader.Position;


                        //var positionBeforeLoad = inner_reader.Position;
                        for (var i = 0; i < ebxCount; i++)
                        {
                            var ebxobjectinlist = EbxObjectList[i];

                            EbxObjectList[i].SetValue("offset", casBundle.Offsets[i]);
                            EbxObjectList[i].SetValue("size", casBundle.Sizes[i]);

                            EbxObjectList[i].SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);
                            EbxObjectList[i].SetValue("SB_CAS_Offset_Position", casBundle.TOCOffsets[i]);
                            EbxObjectList[i].SetValue("SB_CAS_Size_Position", casBundle.TOCSizes[i]);

                            var bundleCheck = casBundle.Offsets[i] - casBundle.BundleOffset;
                            bundleCheck = bundleCheck > 0 ? bundleCheck : casBundle.Offsets[i];

                            using (var vs = inner_reader.CreateViewStream(bundleCheck, casBundle.Sizes[i]))
                            {
                                CasReader casReader = new CasReader(vs);
                                var b = casReader.ReadBlock();
                                if (b != null && b.Length > 0)
                                {
                                    //var ms = new MemoryStream();
                                    //NativeWriter nativeWriter_ForMS = new NativeWriter(ms, true);
                                    //nativeWriter_ForMS.Write(b);
                                    //ms.Position = 0;
                                    //EbxReader_F21 ebxReader_F21 = new EbxReader_F21(ms, AssetManager.Instance.fs, false);
                                    //ebxReader_F21.InternalReadObjects();
                                    EbxObjectList[i].AddValue("loaded", 1);

                                }
                            }
                        }
                        for (var i = 0; i < resCount; i++)
                        {
                            ResObjectList[i].SetValue("offset", casBundle.Offsets[ebxCount + i]);
                            ResObjectList[i].SetValue("size", casBundle.Sizes[ebxCount + i]);

                            ResObjectList[i].SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);
                            ResObjectList[i].SetValue("SB_CAS_Offset_Position", casBundle.TOCOffsets[ebxCount + i]);
                            ResObjectList[i].SetValue("SB_CAS_Size_Position", casBundle.TOCSizes[ebxCount + i]);


                            var bundleCheck = casBundle.Offsets[i] - casBundle.BundleOffset;
                            bundleCheck = bundleCheck > 0 ? bundleCheck : casBundle.Offsets[i];

                            using (var vs = inner_reader.CreateViewStream(bundleCheck, casBundle.Sizes[i]))
                            {
                                CasReader casReader = new CasReader(vs);
                                var b = casReader.ReadBlock();
                                if (b != null && b.Length > 0)
                                {
                                    ResObjectList[i].AddValue("loaded", 1);
                                }
                            }
                           
                        }
                        //for (var i = 0; i < chunkCount && casBundle.Offsets.Count - 1 > ebxCount + resCount + i; i++)
                        for (var i = 0; i < chunkCount; i++)
                        {
                            if(ChunkObjectList[i].GetValue<Guid>("id").ToString() == "64c1f350-a32a-8b77-d962-af3887e656d5")
                            {

                         
                            }
                            if (ChunkObjectList[i].GetValue<Guid>("id").ToString() == "c03a15a9-6747-22dd-c760-af2e149e6223") // Juventus Test
                            {

                            }

                            if (casBundle.Offsets.Count > ebxCount + resCount + i)
                            {
                                //var new_pos = casBundle.Offsets[ebxCount + resCount + i] - casBundle.BundleOffset;
                                //if ((new_pos < 0))//|| new_pos > inner_reader.Length))
                                //    continue;

                                //ChunkObjectList[i].SetValue("offset", casBundle.BundleOffset + inner_reader.Position);
                                ChunkObjectList[i].SetValue("offset", casBundle.Offsets[ebxCount + resCount + i]);
                                ChunkObjectList[i].SetValue("size", casBundle.Sizes[ebxCount + resCount + i]);

                                ChunkObjectList[i].SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);
                                ChunkObjectList[i].SetValue("SB_CAS_Offset_Position", casBundle.TOCOffsets[ebxCount + resCount + i]);
                                ChunkObjectList[i].SetValue("SB_CAS_Size_Position", casBundle.TOCSizes[ebxCount + resCount + i]);
                            }
                            //else if(parent.chunkList.ContainsKey(ChunkObjectList[i].GetValue<Guid>("id")))
                            //{
                            //    var originalChunk = parent.chunkList[ChunkObjectList[i].GetValue<Guid>("id")];
                            //    ChunkObjectList[i].SetValue("offset", originalChunk.ExtraData.DataOffset);
                            //    ChunkObjectList[i].SetValue("size", originalChunk.Size);

                            //    ChunkObjectList[i].SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);
                            //    ChunkObjectList[i].SetValue("SB_CAS_Offset_Position", originalChunk.SB_CAS_Offset_Position);
                            //    ChunkObjectList[i].SetValue("SB_CAS_Size_Position", originalChunk.SB_CAS_Size_Position);
                            //}

                        }

                        //var posBeforeReadBlock = inner_reader.Position;
                        //inner_reader.Position = size;
                        //ReadDataBlock(EbxObjectList, inner_reader, (int)baseBundleInfo.Offset, (int)inner_reader.Position);
                        //ReadDataBlock(ResObjectList, inner_reader, (int)baseBundleInfo.Offset, (int)inner_reader.Position);
                        //ReadDataBlock(ChunkObjectList, inner_reader, (int)baseBundleInfo.Offset, (int)inner_reader.Position);
                        //inner_reader.Position = posBeforeReadBlock;


                        FullObjectList.AddValue("ebx", EbxObjectList);
                        FullObjectList.AddValue("res", ResObjectList);
                        FullObjectList.AddValue("chunks", ChunkObjectList);

                        foreach (var item in EbxObjectList)
                        {
                            EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
                            ebxAssetEntry.Name = item.GetValue<string>("name");
                            ebxAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
                            ebxAssetEntry.BaseSha1 = parent.rm.GetBaseSha1(ebxAssetEntry.Sha1);
                            ebxAssetEntry.Size = item.GetValue("size", 0L);
                            ebxAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
                            ebxAssetEntry.Location = AssetDataLocation.CasNonIndexed;
                            ebxAssetEntry.ExtraData = new AssetExtraData();
                            ebxAssetEntry.ExtraData.DataOffset = item.GetValue("offset", 0L);
                            ebxAssetEntry.ExtraData.CasPath = (item.HasValue("catalog") ? parent.fs.GetFilePath(item.GetValue("catalog", 0), item.GetValue("cas", 0), item.HasValue("patch")) : parent.fs.GetFilePath(item.GetValue("cas", 0)));
                            ebxAssetEntry.Guid = Guid.NewGuid(); // this is not right!

                            //ebxAssetEntry.CASFileLocation = NativeFileLocation;
                            //ebxAssetEntry.SBFileLocation = AssociatedTOCFile.NativeFileLocation;
                            ebxAssetEntry.TOCFileLocation = AssociatedTOCFile.NativeFileLocation;
                            ebxAssetEntry.SB_CAS_Offset_Position = item.GetValue("SB_CAS_Offset_Position", 0);
                            ebxAssetEntry.SB_CAS_Size_Position = item.GetValue("SB_CAS_Size_Position", 0);
                            ebxAssetEntry.SB_Sha1_Position = item.GetValue("SB_Sha1_Position", 0);

                            if (item.GetValue<int>("loaded", 0) == 1)
                            {
                                if(!parent.ebxList.ContainsKey(ebxAssetEntry.Name))
                                    parent.AddEbx(ebxAssetEntry);

                                CASEBXEntries.Add(ebxAssetEntry);
                            }
                        }

                        var iRes = 0;
                        foreach (var item in ResObjectList)
                        {
                            ResAssetEntry resAssetEntry = new ResAssetEntry();
                            resAssetEntry.Name = item.GetValue<string>("name");
                            resAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
                            resAssetEntry.BaseSha1 = parent.rm.GetBaseSha1(resAssetEntry.Sha1);
                            resAssetEntry.Size = item.GetValue("size", 0L);
                            resAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
                            resAssetEntry.Location = AssetDataLocation.CasNonIndexed;
                            resAssetEntry.ExtraData = new AssetExtraData();
                            resAssetEntry.ExtraData.DataOffset = item.GetValue("offset", 0L);
                            resAssetEntry.ExtraData.CasPath = (item.HasValue("catalog") ? parent.fs.GetFilePath(item.GetValue("catalog", 0), item.GetValue("cas", 0), item.HasValue("patch")) : parent.fs.GetFilePath(item.GetValue("cas", 0)));
                            var resRid = ResIdList[iRes];
                            resAssetEntry.ResRid = resRid;
                            resAssetEntry.ResType = (uint)item.GetValue<int>("resType", 0);
                            resAssetEntry.ResMeta = item.GetValue<byte[]>("resMeta", null);

                            resAssetEntry.CASFileLocation = NativeFileLocation;
                            //resAssetEntry.SBFileLocation = AssociatedTOCFile.NativeFileLocation;
                            resAssetEntry.TOCFileLocation = AssociatedTOCFile.NativeFileLocation;
                            resAssetEntry.SB_CAS_Offset_Position = item.GetValue("SB_CAS_Offset_Position", 0);
                            resAssetEntry.SB_CAS_Size_Position = item.GetValue("SB_CAS_Size_Position", 0);
                            resAssetEntry.SB_Sha1_Position = item.GetValue("SB_Sha1_Position", 0);


                            if (item.GetValue<int>("loaded", 0) == 1)
                            {
                                if (!parent.resList.ContainsKey(resAssetEntry.Name) && !parent.resRidList.ContainsKey(resAssetEntry.ResRid))
                                    parent.AddRes(resAssetEntry);

                                CASRESEntries.Add(resAssetEntry);

                            }
                            iRes++;
                        }

                        foreach (var item in ChunkObjectList)
                        {
                            ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
                            chunkAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
                            chunkAssetEntry.BaseSha1 = parent.rm.GetBaseSha1(chunkAssetEntry.Sha1);
                            chunkAssetEntry.Size = item.GetValue("size", 0L);
                            chunkAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
                            chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
                            chunkAssetEntry.ExtraData = new AssetExtraData();
                            chunkAssetEntry.ExtraData.DataOffset = item.GetValue("offset", 0L);
                            
                            chunkAssetEntry.ExtraData.CasPath = (item.HasValue("catalog") ? parent.fs.GetFilePath(item.GetValue("catalog", 0), item.GetValue("cas", 0), item.HasValue("patch")) : parent.fs.GetFilePath(item.GetValue("cas", 0)));

                            chunkAssetEntry.Id = item.GetValue<Guid>("id");
                            chunkAssetEntry.LogicalOffset = item.GetValue<uint>("logicalOffset");
                            chunkAssetEntry.LogicalSize = item.GetValue<uint>("logicalSize");

                            chunkAssetEntry.CASFileLocation = NativeFileLocation;
                            //chunkAssetEntry.SBFileLocation = AssociatedTOCFile.NativeFileLocation;
                            chunkAssetEntry.TOCFileLocation = AssociatedTOCFile.NativeFileLocation;
                            chunkAssetEntry.SB_CAS_Offset_Position = item.GetValue("SB_CAS_Offset_Position", 0);
                            chunkAssetEntry.SB_CAS_Size_Position = item.GetValue("SB_CAS_Size_Position", 0);
                            chunkAssetEntry.SB_Sha1_Position = item.GetValue("SB_Sha1_Position", 0);

                            //if (parent.chunkList.ContainsKey(chunkAssetEntry.Id) && chunkAssetEntry.ExtraData.DataOffset > 0)
                            //    parent.chunkList.Remove(chunkAssetEntry.Id);
                            
                            //if (!parent.chunkList.ContainsKey(chunkAssetEntry.Id))
                                parent.AddChunk(chunkAssetEntry);

                            CASCHUNKEntries.Add(chunkAssetEntry);


                        }

                    }
                }
            }


        }



        private void ReadDataBlock(List<DbObject> list, NativeReader reader, int bundleOffset, int Position)
        {
            if (list == null)
            {
                return;
            }

            foreach (DbObject item in list)
            {
                item.SetValue("offset", bundleOffset + Position);
                long num = item.GetValue("originalSize", 0L);
                long num2 = 0L;
               
                while (num > 0)
                {
                    int num3 = reader.ReadInt(Endian.Big);
                    ushort num4 = reader.ReadUShort();
                    int num5 = reader.ReadUShort(Endian.Big);
                    int num6 = (num4 & 0xFF00) >> 8;
                    if ((num6 & 0xF) != 0)
                    {
                        num5 = ((num6 & 0xF) << 16) + num5;
                    }
                    if ((num3 & 4278190080u) != 0L)
                    {
                        num3 &= 0xFFFFFF;
                    }
                    num -= num3;
                    if ((ushort)(num4 & 0x7F) == 0)
                    {
                        num5 = num3;
                    }
                    num2 += num5 + 8;
                    Position += num5;
                }
                item.AddValue("size", num2);
                item.AddValue("sb", true);
            }
        }



    }


}
