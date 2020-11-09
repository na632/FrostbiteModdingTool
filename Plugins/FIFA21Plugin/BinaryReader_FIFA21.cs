using Frosty.Hash;
using FrostySdk;
using FrostySdk.Deobfuscators;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using static FIFA21Plugin.FIFA21AssetLoader;

namespace FIFA21Plugin
{
    public class BinaryReader_FIFA21
    {
        public int SBInitialHeaderLength = 32;

        public int SBInformationHeaderLength = 36;

        List<long> Sha1Positions = new List<long>();


        public SBHeaderInformation BinaryRead_FIFA21(
            BaseBundleInfo BaseBundleItem
            , ref DbObject dbObject
            , NativeReader binarySbReader2
            , bool IncludeAdditionalHeaderLength
            )
        {
            // Read out the Header Info
            var SBHeaderInformation = new SBHeaderInformation(binarySbReader2, IncludeAdditionalHeaderLength ? SBInformationHeaderLength : 4);
            //
            List<Sha1> sha1 = new List<Sha1>();
            for (int i = 0; i < SBHeaderInformation.totalCount; i++)
            {
                Sha1Positions.Add(binarySbReader2.Position + BaseBundleItem.Offset);
                sha1.Add(binarySbReader2.ReadSha1());
            }
            dbObject.AddValue("ebx", new DbObject(ReadEbx(SBHeaderInformation, sha1, binarySbReader2, BaseBundleItem)));
            dbObject.AddValue("res", new DbObject(ReadRes(SBHeaderInformation, sha1, binarySbReader2, BaseBundleItem)));
            dbObject.AddValue("chunks", new DbObject(ReadChunks(SBHeaderInformation, sha1, binarySbReader2, BaseBundleItem)));
            dbObject.AddValue("dataOffset", (int)(SBHeaderInformation.size));
            dbObject.AddValue("stringsOffset", (int)(SBHeaderInformation.stringOffset));
            dbObject.AddValue("metaOffset", (int)(SBHeaderInformation.metaOffset));
            dbObject.AddValue("metaSize", (int)(SBHeaderInformation.metaSize));

            if (SBHeaderInformation.chunkCount != 0)
            {
                //using (DbReader dbReader = new DbReader(nativeReader.CreateViewStream(SBHeaderInformation.metaOffset + BaseBundleItem.Offset, nativeReader.Length - binarySbReader2.Position), new NullDeobfuscator()))
                using (DbReader dbReader = new DbReader(binarySbReader2.CreateViewStream(SBHeaderInformation.metaOffset, binarySbReader2.Length - binarySbReader2.Position), new NullDeobfuscator()))
                {
                    var o = dbReader.ReadDbObject();
                    dbObject.AddValue("chunkMeta", o);
                }
            }

            return SBHeaderInformation;
        }



        private List<object> ReadEbx(SBHeaderInformation information, List<Sha1> sha1, NativeReader reader, BaseBundleInfo baseBundleInfo = null)
        {
            List<object> list = new List<object>();
            for (int i = 0; i < information.ebxCount; i++)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());
                uint num = reader.ReadUInt(Endian.Little);

                dbObject.AddValue("SB_OriginalSize_Position", reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                uint originalSize = reader.ReadUInt(Endian.Little);

                long position = reader.Position;
                reader.Position = information.stringOffset + num;


                dbObject.AddValue("SB_StringOffsetPosition", reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));

                //System.Diagnostics.Debug.WriteLine($"EBX::Position::{reader.Position}");
                dbObject.AddValue("SB_Sha1_Position", Sha1Positions[i]);
                dbObject.AddValue("sha1", sha1[i]);
                var name = reader.ReadNullTerminatedString();
                //System.Diagnostics.Debug.WriteLine("EBX:: " + name);
                dbObject.AddValue("name", name);
                dbObject.AddValue("nameHash", Fnv1.HashString(dbObject.GetValue<string>("name")));
                dbObject.AddValue("originalSize", originalSize);
                list.Add(dbObject);
                reader.Position = position;
            }
            return list;
        }
        private List<object> ReadRes(SBHeaderInformation information, List<Sha1> sha1, NativeReader reader, BaseBundleInfo baseBundleInfo = null)
        {
            List<object> list = new List<object>();
            int num = (int)information.ebxCount;
            for (int i = 0; i < information.resCount; i++)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());
                uint stringPosition = reader.ReadUInt(Endian.Little);

                dbObject.AddValue("SB_OriginalSize_Position", reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                uint originalSize = reader.ReadUInt(Endian.Little);

                long position = reader.Position;

                reader.Position = information.stringOffset + stringPosition;
                dbObject.AddValue("SB_StringOffsetPosition", reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));

                dbObject.AddValue("SB_Sha1_Position", Sha1Positions[i]);
                dbObject.AddValue("sha1", sha1[num++]);
                var name = reader.ReadNullTerminatedString();
                //System.Diagnostics.Debug.WriteLine("RES:: " + name);
                dbObject.AddValue("name", name);
                dbObject.AddValue("nameHash", Fnv1.HashString(name));
                dbObject.AddValue("originalSize", originalSize);
                list.Add(dbObject);
                reader.Position = position;
            }
            foreach (DbObject item in list)
            {
                //var type = reader.ReadUInt(Endian.Big);
                var type = reader.ReadUInt(Endian.Little);
                var resType = (ResourceType)type;
                item.AddValue("resType", type);
            }
            foreach (DbObject item2 in list)
            {
                var resMeta = reader.ReadBytes(16);
                item2.AddValue("resMeta", resMeta);
            }
            foreach (DbObject item3 in list)
            {
                var resRid = reader.ReadLong(Endian.Little);
                item3.AddValue("resRid", resRid);
            }
            return list;
        }
        private List<object> ReadChunks(SBHeaderInformation information, List<Sha1> sha1, NativeReader reader, BaseBundleInfo baseBundleInfo = null)
        {
            var currentPostion = reader.Position;

            List<object> list = new List<object>();
            int num = (int)(information.ebxCount + information.resCount);
            for (int i = 0; i < information.chunkCount; i++)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());
                dbObject.AddValue("SB_Guid_Position", reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                Guid guid = reader.ReadGuid(Endian.Little);
                dbObject.AddValue("SB_LogicalOffset_Position", reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                uint logicalOffset = reader.ReadUInt(Endian.Little);
                dbObject.AddValue("SB_OriginalSize_Position", reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                uint size_position = reader.ReadUInt(Endian.Little);
                //dbObject.AddValue("SB_OriginalSize_Position", reader.Position + (baseBundleInfo != null ? baseBundleInfo.Offset : 0));
                long original_size_position = (logicalOffset & 0xFFFF) | size_position;
                dbObject.AddValue("id", guid);
                dbObject.AddValue("SB_Sha1_Position", Sha1Positions[i]);
                dbObject.AddValue("sha1", sha1[num + i]);
                dbObject.AddValue("logicalOffset", logicalOffset);
                dbObject.AddValue("logicalSize", size_position);
                dbObject.AddValue("originalSize", original_size_position);
                list.Add(dbObject);
            }
            return list;
        }

    }
}
