using FMT.FileTools;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;

namespace FrostySdk.IO
{
    public class BinarySbReader_M21 : BinarySbReader
    {
        public bool Debug = false;

        public string bundleName = "";

        public string shortBundleName
        {
            get
            {

                return bundleName.Split(@"/")[bundleName.Split(@"/").Length - 1];

            }
        }


        private uint totalCount;

        private uint ebxCount;

        private uint resCount;

        private uint chunkCount;

        private uint stringsOffset;

        private uint metaOffset;

        private uint metaSize;

        private List<FMT.FileTools.Sha1> sha1 = new List<FMT.FileTools.Sha1>();

        private bool containsUncompressedData;

        private long bundleOffset;

        public BinarySbReader_M21(Stream inStream, long inBundleOffset, IDeobfuscator inDeobfuscator)
            : base(inStream, inBundleOffset, inDeobfuscator)
        {
            bundleOffset = inBundleOffset;
        }

        //public BinarySbReader2(Stream inBaseStream, Stream inDeltaStream, IDeobfuscator inDeobfuscator)
        //	: base(inBaseStream, inDeltaStream, inDeobfuscator)
        //{
        //	if (inDeltaStream == null)
        //	{
        //		return;
        //	}
        //	Stream stream = PatchStream(inDeltaStream);
        //	inDeltaStream.Dispose();
        //	inDeltaStream = null;
        //	if (stream != null)
        //	{
        //		if (base.stream != null)
        //		{
        //			base.stream.Dispose();
        //		}
        //		base.stream = stream;
        //		streamLength = base.stream.Length;
        //	}
        //	bundleOffset = 0L;
        //}

        public int HeaderSize;

        public override DbObject ReadDbObject()
        {

            if (Debug)
            {
                if (string.IsNullOrEmpty(this.bundleName))
                    this.bundleName = "test";
                NativeWriter nativeWriter = new NativeWriter(new FileStream($"Debugging/Other/{this.shortBundleName}.dat", FileMode.OpenOrCreate));
                Position = 0;
                var b = ReadToEnd();
                nativeWriter.Write(b);
                nativeWriter.Close();
                nativeWriter.Dispose();
                Position = 0;
            }

            //DbObject dbObject = new DbObject(new Dictionary<string, object>());
            DbObject dbObject = new DbObject();

            Position = bundleOffset;

            uint size = ReadUInt(Endian.Big) + 4; // FIFA20 705
            dbObject.SetValue("dbSize", size - 4);

            uint nofhash = ReadUInt(Endian.Little);
            uint hashynumber = nofhash ^ 0x7065636E; // FIFA 20, Madden 21 3280507699u

            // OK
            totalCount = ReadUInt(Endian.Little); // this should be a nice normal number
                                                  // OK
            ebxCount = ReadUInt(Endian.Little); // this should be a nice normal number
                                                // OK
            resCount = ReadUInt(Endian.Little); // this should be a nice normal number
                                                // OK
            chunkCount = ReadUInt(Endian.Little); // this should be a nice normal number

            if (ebxCount + resCount + chunkCount != totalCount)
                throw new Exception("Bundle counts do not match");

            stringsOffset = ReadUInt(Endian.Little) + 4;

            metaOffset = ReadUInt(Endian.Little) + 4;

            metaSize = ReadUInt(Endian.Little);

            var dSize = (int)(size - Position);

            HeaderSize = (int)Position;

            byte[] array = ReadBytes(dSize);
            using (DbReader dbReader = new DbReader(new MemoryStream(array), null))
            {
                for (int i = 0; i < totalCount; i++)
                {
                    sha1.Add(Sha1.Zero);
                }
                dbObject.AddValue("ebx", new DbObject(ReadEbx(dbReader)));
                dbObject.AddValue("res", new DbObject(ReadRes(dbReader)));
                dbObject.AddValue("chunks", new DbObject(ReadChunks(dbReader)));
                dbObject.AddValue("stringsOffset", (int)(stringsOffset));
                dbObject.AddValue("metaOffset", (int)(metaOffset));
                dbObject.AddValue("metaSize", (int)(metaSize));

                dbObject.AddValue("dataOffset", (int)(size));

                if (chunkCount != 0)
                {
                    dbReader.Position = metaOffset - HeaderSize;
                    var dbObj = dbReader.ReadDbObject();
                    if (dbObj != null)
                    {
                        dbObject.AddValue("chunkMeta", dbObj);
                    }
                }
            }
            Position = size;
            if (Position == Length)
            {
                return dbObject;
            }
            if (hashynumber == 3978096056u)
            {
                return dbObject;
            }
            ReadDataBlock(dbObject.GetValue<DbObject>("ebx"));
            ReadDataBlock(dbObject.GetValue<DbObject>("res"));
            ReadDataBlock(dbObject.GetValue<DbObject>("chunks"));


            return dbObject;
        }

        private void ReadDataBlock(DbObject list)
        {
            if (list == null)
            {
                //Debug.WriteLine("[DEBUG] BinarySbReader2::ReadDataBlock()::List is null");
                return;
            }
            foreach (DbObject item in list)
            {
                if (item.HasValue("id"))
                {
                    if (item.GetValue<Guid>("id", Guid.Empty).ToString() == "6f901288-9e31-203b-5470-39ef2669b593")
                    {

                    }
                }
                item.AddValue("offset", bundleOffset + Position);
                long origSize = item.GetValue("originalSize", 0L);
                long size = 0L;
                if (containsUncompressedData)
                {
                    size = origSize;
                    item.AddValue("data", ReadBytes((int)origSize));
                }
                else
                {

                    //CasReader casReader = new CasReader(new MemoryStream(ReadBytes((int)origSize + 9999999)));
                    //CasReader casReader = new CasReader(new MemoryStream(ReadBytes((int)origSize)));
                    //var testOrig = casReader.Read().Length;
                    //size = casReader.Read().Length;

                    //
                    Position = item.GetValue<long>("offset");
                    //Position = item.GetValue<long>("offset") + size;
                    size = 0;

                    while (origSize > 0)
                    {
                        int num = ReadInt(Endian.Big);
                        ushort compressionType = ReadUShort();
                        int LastBufferSize = ReadUShort(Endian.Big);
                        int num4 = (compressionType & 0xFF00) >> 8;
                        if ((num4 & 0xF) != 0)
                        {
                            LastBufferSize = ((num4 & 0xF) << 16) + LastBufferSize;
                        }
                        if ((num & 4278190080u) != 0L)
                        {
                            num &= 0xFFFFFF;
                        }
                        origSize -= num;
                        var sw = (ushort)(compressionType & 0x7F);

                        // Read uncompressed
                        if ((ushort)(compressionType & 0x7F) == 0)
                        {
                            LastBufferSize = num;
                        }

                        size += LastBufferSize + 8;
                        //size += ReadBytes(LastBufferSize).Length;
                        Position += LastBufferSize;
                    }
                }

                //var keepPosition = Position;
                //Position = item.GetValue<long>("offset") - bundleOffset;
                ////item.AddValue("data", ReadBytes((int)size));
                //Position = keepPosition;

                item.AddValue("size", size);
                item.AddValue("sb", true);
            }
        }

        private List<object> ReadEbx(NativeReader reader)
        {
            //if (Debug)
            //{
            //             NativeWriter nativeWriter = new NativeWriter(new FileStream($"Debugging/Ebx/DebuggingBSBR.dat", FileMode.OpenOrCreate));
            //             reader.Position = 0;
            //             var b = reader.ReadToEnd();
            //             nativeWriter.Write(b);
            //             nativeWriter.Close();
            //             nativeWriter.Dispose();
            //         }
            //         reader.Position = 0;

            List<object> list = new List<object>((int)ebxCount);
            for (int i = 0; i < ebxCount; i++)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());
                uint stringOff = reader.ReadUInt(Endian.Little);
                dbObject.SetValue("StringOffset", stringOff);

                dbObject.SetValue("SB_OriginalSize_Position", bundleOffset + reader.Position);
                uint originalSize = reader.ReadUInt(Endian.Little);
                long position = reader.Position;
                reader.Position = stringsOffset + stringOff - 36;
                //System.Diagnostics.Debug.WriteLine($"EBX::Position::{reader.Position}");
                dbObject.AddValue("sha1", sha1[i]);
                var name = reader.ReadNullTerminatedString();
                //System.Diagnostics.Debug.WriteLine("EBX:: " + name);
                dbObject.AddValue("name", name);
                dbObject.AddValue("nameHash", Fnv1.HashString(dbObject.GetValue<string>("name")));
                dbObject.AddValue("originalSize", originalSize);
                dbObject.AddValue("type", "EBX");
                list.Add(dbObject);
                reader.Position = position;
            }
            return list;

            //reader.Position = stringsOffset; //  4 + stringsOffset + num;
            //List<object> list = new List<object>();
            //for (int i = 0; i < ebxCount; i++)
            //{
            //    DbObject dbObject = new DbObject(new Dictionary<string, object>());
            //    dbObject.AddValue("sha1", sha1[i]);
            //    var name = reader.ReadNullTerminatedString();
            //    System.Diagnostics.Debug.WriteLine("EBX:: " + name);
            //    dbObject.AddValue("name", name);
            //    dbObject.AddValue("nameHash", Fnv1.HashString(dbObject.GetValue<string>("name")));
            //    //dbObject.AddValue("originalSize", num2);
            //    dbObject.AddValue("originalSize", 0); // TODO: Find this
            //    list.Add(dbObject);
            //}
            //return list;
        }

        private List<object> ReadRes(NativeReader reader)
        {
            //var pos = reader.Position;

            //         NativeWriter nativeWriter = new NativeWriter(new FileStream("testRes.txt", FileMode.OpenOrCreate));
            //         reader.Position = 0;
            //         var b = reader.ReadToEnd();
            //         nativeWriter.Write(b);
            //         nativeWriter.Close();
            //         nativeWriter.Dispose();
            //reader.Position = pos;

            //reader.Position = 1874;

            //var ebxHeader = reader.ReadNullTerminatedString();
            //System.Diagnostics.Debug.WriteLine("EBX Header:: " + ebxHeader);

            List<object> list = new List<object>((int)resCount);
            int num = (int)ebxCount;
            for (int i = 0; i < resCount; i++)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());
                uint stringOff = reader.ReadUInt(Endian.Little);
                dbObject.SetValue("StringOffset", stringOff);

                dbObject.AddValue("SB_OriginalSize_Position", bundleOffset + reader.Position);
                uint originalSize = reader.ReadUInt(Endian.Little);
                long position = reader.Position;
                reader.Position = (-36) + stringsOffset + stringOff;
                dbObject.AddValue("sha1", sha1[num++]);
                var name = reader.ReadNullTerminatedString();
                //System.Diagnostics.Debug.WriteLine("RES:: " + name);
                dbObject.AddValue("name", name);
                dbObject.AddValue("nameHash", Fnv1.HashString(name));
                dbObject.AddValue("originalSize", originalSize);
                dbObject.AddValue("type", "RES");
                list.Add(dbObject);
                reader.Position = position;
            }
            foreach (DbObject item in list)
            {
                var type = reader.ReadUInt(Endian.Big);
                //var type = reader.ReadInt();
                if (type < 0)
                {
                    throw new ArgumentOutOfRangeException("Must be over 0");
                }
                var resType = (ResourceType)type;
                item.SetValue("resType", type);
                item.SetValue("resTypeSolid", resType);
            }
            foreach (DbObject item2 in list)
            {
                var resMeta = reader.ReadBytes(16);
                item2.AddValue("resMeta", resMeta);
            }
            foreach (DbObject item3 in list)
            {
                ulong resRid = reader.ReadULong(Endian.Little);
                //item3.AddValue("resRid", reader.ReadLong(Endian.Big));
                item3.AddValue("resRid", resRid, false);
            }
            return list;
        }

        private List<object> ReadChunks(NativeReader reader)
        {
            List<object> list = new List<object>((int)chunkCount);
            int startIndex = (int)(ebxCount + resCount);
            for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());
                Guid guid = reader.ReadGuid(Endian.Little);

                if (guid.ToString() == "6f901288-9e31-203b-5470-39ef2669b593")
                {

                }

                //dbObject.AddValue("SB_LogicalOffset_Position", bundleOffset + reader.Position);
                uint logicalOffset = reader.ReadUInt(Endian.Little);
                //dbObject.AddValue("SB_OriginalSize_Position", bundleOffset + reader.Position);
                uint logicalSize = reader.ReadUInt(Endian.Little);
                //long originalSize = (logicalOffset & 0xFFFF) | logicalSize;
                long originalSize = (logicalOffset & 0xFFFF) | logicalSize;
                //long originalSize2 = logicalSize + logicalOffset;
                dbObject.AddValue("id", guid);
                dbObject.AddValue("name", guid);
                dbObject.AddValue("sha1", sha1[startIndex + chunkIndex]);
                dbObject.AddValue("logicalOffset", logicalOffset);
                //dbObject.AddValue("logicalSize", logicalSize);
                dbObject.AddValue("originalSize", originalSize);
                dbObject.AddValue("Bundle", shortBundleName);
                dbObject.AddValue("type", "CHUNKS");
                list.Add(dbObject);

                // colts uniform
                if (guid.ToString() == "e35237c0-ebbe-bc31-1504-aaf3eb947c9b")
                {

                }
            }
            return list;
        }
    }
}
