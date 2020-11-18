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
        public void Load(AssetManager parent, int catalog, int cas, List<CASBundle> casBundles)
        {
            var path = parent.fs.ResolvePath(parent.fs.GetFilePath(catalog, cas, false));// @"E:\Origin Games\FIFA 21\Data\Win32\superbundlelayout\fifa_installpackage_03\cas_03.cas";
            Load(parent, path, casBundles);
        }

        public void Load(AssetManager parent, string path, List<CASBundle> casBundles)
        {
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


                        var size = inner_reader.ReadInt(Endian.Big);
                        var magicStuff = inner_reader.ReadUInt(Endian.Big);
                        if (magicStuff != 3599661469) return;
                            //throw new Exception("Magic/Hash is not right, expecting 3599661469");

                        var totalCount = inner_reader.ReadInt(Endian.Little);
                        var ebxCount = inner_reader.ReadInt(Endian.Little);
                        var resCount = inner_reader.ReadInt(Endian.Little);
                        var chunkCount = inner_reader.ReadInt(Endian.Little);
                        if(ebxCount + resCount + chunkCount != totalCount) return;
                            //throw new Exception("Total Count is not right");

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
                            }
                        }
                        inner_reader.Position = posBeforeChunkMeta;

                        List<int> Sha1Positions = new List<int>();
                        List<Sha1> sha1 = new List<Sha1>();
                        for (int i = 0; i < totalCount; i++)
                        {
                            Sha1Positions.Add((int)inner_reader.Position);
                            sha1.Add(inner_reader.ReadSha1());
                        }

                        List<DbObject> EbxObjectList = new List<DbObject>();
                        for (int i = 0; i < ebxCount; i++)
                        {
                            DbObject dbObject = new DbObject(new Dictionary<string, object>());
                            dbObject.AddValue("AssetType", "EBX");

                            dbObject.AddValue("SB_StringOffsetPosition", inner_reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                            uint num = inner_reader.ReadUInt(Endian.Little);
                            dbObject.AddValue("StringOffsetPos", num);

                            dbObject.AddValue("SB_OriginalSize_Position", inner_reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                            uint originalSize = inner_reader.ReadUInt(Endian.Little);
                            dbObject.AddValue("originalSize", originalSize);
                            dbObject.AddValue("size", originalSize);
                            
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
                            if (casBundle.Offsets.Count - 1 > i )
                            {
                                var new_pos = casBundle.Offsets[i] - casBundle.BundleOffset;
                                if (new_pos < 0 || new_pos > inner_reader.Length)
                                    break;

                                inner_reader.Position = new_pos;
                                EbxObjectList[i].AddValue("offset", casBundle.BundleOffset + inner_reader.Position);
                                EbxObjectList[i].SetValue("size", casBundle.Sizes[i]);

                                using (var vs = inner_reader.CreateViewStream(inner_reader.Position, casBundle.Sizes[i]))
                                {
                                    CasReader casReader = new CasReader(vs);
                                    var b = casReader.ReadBlock();
                                    if (b != null && b.Length > 0)
                                    {
                                        var ms = new MemoryStream();
                                        NativeWriter nativeWriter_ForMS = new NativeWriter(ms, true);
                                        nativeWriter_ForMS.Write(b);
                                        ms.Position = 0;
                                        EbxReader_F21 ebxReader_F21 = new EbxReader_F21(ms, AssetManager.Instance.fs, false);
                                        //ebxReader_F21.InternalReadObjects();
                                        EbxObjectList[i].AddValue("loaded", 1);

                                    }
                                }
                            }
                        }
                        for (var i = 0; i < resCount; i++)
                        {
                            var new_pos = casBundle.Offsets[ebxCount + i] - casBundle.BundleOffset;
                            if (new_pos < 0 || new_pos > inner_reader.Length)
                                break;

                            inner_reader.Position = new_pos;
                            ResObjectList[i].AddValue("offset", casBundle.BundleOffset + inner_reader.Position);
                            ResObjectList[i].SetValue("size", casBundle.Sizes[ebxCount + i]);

                            using (var vs = inner_reader.CreateViewStream(inner_reader.Position, casBundle.Sizes[ebxCount + i]))
                            {
                                CasReader casReader = new CasReader(vs);
                                var b = casReader.ReadBlock();
                                if (b != null && b.Length > 0)
                                {
                                    ResObjectList[i].AddValue("loaded", 1);
                                }
                            }
                        }
                        for (var i = 0; i < chunkCount; i++)
                        {
                            var new_pos = casBundle.Offsets[ebxCount + resCount + i] - casBundle.BundleOffset;
                            if (new_pos < 0 || new_pos > inner_reader.Length)
                                break;

                            inner_reader.Position = new_pos;
                            ChunkObjectList[i].AddValue("offset", casBundle.BundleOffset + inner_reader.Position);
                            ChunkObjectList[i].SetValue("size", casBundle.Sizes[ebxCount + resCount + i]);

                        }

                        FullObjectList.AddValue("ebx", EbxObjectList);
                        FullObjectList.AddValue("res", ResObjectList);
                        FullObjectList.AddValue("chunks", ChunkObjectList);

                        //ReadDataBlock(FullObjectList.GetValue<List<DbObject>>("ebx"), inner_reader, 0);
                        //ReadDataBlock(FullObjectList.GetValue<List<DbObject>>("res"), inner_reader, 0);
                        //ReadDataBlock(FullObjectList.GetValue<List<DbObject>>("chunks"), inner_reader, 0);

                        //if (ChunkObjectList.Count > 0)
                        //{
                        //    var findAChunkGuidByteArray = ChunkObjectList[0].GetValue<Guid>("id").ToByteArray();
                        //    BoyerMoore boyerMoore = new BoyerMoore(findAChunkGuidByteArray);
                        //    inner_reader.Position = 0;
                        //    var listoffindsLit = boyerMoore.SearchAll(inner_reader.ReadToEnd());
                        //    if (listoffindsLit != null && listoffindsLit.Count > 0)
                        //    {

                        //    }
                        //}

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
                            ebxAssetEntry.SBFileLocation = path;

                            if (item.GetValue<int>("loaded", 0) == 1)
                            {
                                if(!parent.ebxList.ContainsKey(ebxAssetEntry.Name))
                                    parent.AddEbx(ebxAssetEntry);
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
                            
                            resAssetEntry.SBFileLocation = path;
                            if (item.GetValue<int>("loaded", 0) == 1)
                            {
                                if (!parent.resList.ContainsKey(resAssetEntry.Name))
                                    parent.AddRes(resAssetEntry);
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
                            chunkAssetEntry.SBFileLocation = path;

                            if(!parent.chunkList.ContainsKey(chunkAssetEntry.Id))
                                parent.AddChunk(chunkAssetEntry);
                        }

                    }
                }
            }


        }


        private void ReadDataBlock(List<DbObject> list, NativeReader reader, int bundleOffset)
        {
            foreach (DbObject item in list)
            {
            //    item.AddValue("offset", bundleOffset + reader.Position);
                long num = item.GetValue("originalSize", 0L);
                long num2 = 0L;
            //    if (true)
            //    {
                    num2 = num;
                    item.AddValue("data", reader.ReadBytes((int)num));
            //    }
            //    else
            //    {
            //        while (num > 0)
            //        {
            //            int num3 = reader.ReadInt(Endian.Big);
            //            ushort num4 = reader.ReadUShort();
            //            int num5 = reader.ReadUShort(Endian.Big);
            //            int num6 = (num4 & 0xFF00) >> 8;
            //            if ((num6 & 0xF) != 0)
            //            {
            //                num5 = ((num6 & 0xF) << 16) + num5;
            //            }
            //            if ((num3 & 4278190080u) != 0L)
            //            {
            //                num3 &= 0xFFFFFF;
            //            }
            //            num -= num3;
            //            if ((ushort)(num4 & 0x7F) == 0)
            //            {
            //                num5 = num3;
            //            }
            //            num2 += num5 + 8;
            //            reader.Position += num5;
            //        }
            //    }
                item.AddValue("size", num2);
                item.AddValue("sb", true);
            }
        }

    }


}
