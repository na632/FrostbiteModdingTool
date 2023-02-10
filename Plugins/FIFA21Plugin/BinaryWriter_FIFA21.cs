using FMT.FileTools;
using FrostySdk;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FIFA21Plugin
{
    public class BinaryWriter_FIFA21
    {
        /*
        public long Write(DbObject bundle, Stream stream, bool padEnd = true)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanWrite)
            {
                throw new ArgumentException("Stream must support writing.", "stream");
            }
            if (!stream.CanSeek)
            {
                throw new ArgumentException("Stream must support seeking.", "stream");
            }
            if (bundle == null)
            {
                throw new ArgumentNullException("bundle");
            }
            FileWriter writer = new FileWriter(stream);
            long bundleStartOffset = writer.Position;
            int ebxEntries = bundle.GetValue<DbObject>("ebx").List.Count;
            int resEntries = bundle.GetValue<DbObject>("res").List.Count;
            int chunkEntries = bundle.GetValue<DbObject>("chunks").List.Count;
            int totalCount = checked(ebxEntries + resEntries + chunkEntries);
            writer.WriteUInt32BigEndian(3599661469u);
            writer.WriteInt32LittleEndian(totalCount);
            writer.WriteInt32LittleEndian(ebxEntries);
            writer.WriteInt32LittleEndian(resEntries);
            writer.WriteInt32LittleEndian(chunkEntries);
            long placeholderPosition = writer.Position;
            writer.WriteUInt32LittleEndian(0u);
            writer.WriteUInt32LittleEndian(0u);
            writer.WriteUInt32LittleEndian(0u);
            List<FMT.FileTools.Sha1> sha1s = bundle.GetValue<List<FMT.FileTools.Sha1>>("sha1s");
            for (int i = 0; i < totalCount; i++)
            {
                writer.Write(sha1s[i]);
            }
            uint entryNamesOffset = 0u;
            WriteEbx(writer, bundle.GetValue<DbObject>("ebx"), ref entryNamesOffset);
            WriteRes(writer, bundle.GetValue<DbObject>("res"), ref entryNamesOffset);
            WriteChunks(writer, bundle.GetValue<DbObject>("chunks"));
            long stringsOffset = writer.Position - bundleStartOffset;
            foreach (DbObject entry in bundle.GetValue<DbObject>("ebx"))
            {
                writer.WriteNullTerminatedString(entry.GetValue<string>("name"));
            }
            foreach (DbObject entry in bundle.GetValue<DbObject>("res"))
            {
                writer.WriteNullTerminatedString(entry.GetValue<string>("name"));
            }
            long chunkMetaOffset = 0L;
            long chunkMetaSize = 0L;
            if (chunkEntries > 0)
            {
                chunkMetaOffset = writer.Position - bundleStartOffset;
                //new DbWriter(stream).WriteDbObject("chunkMeta", bundle.GetValue<DbObject>("chunkMeta"));
                writer.Write(WriteChunkMeta(bundle));
                chunkMetaSize = writer.Position - bundleStartOffset - chunkMetaOffset;
            }
            long endPosition = writer.Position;
            writer.Position = placeholderPosition;
            _ = uint.MaxValue;
            writer.WriteUInt32LittleEndian((uint)stringsOffset);
            if (chunkMetaOffset != 0L)
            {
                if (chunkMetaOffset <= uint.MaxValue)
                {
                    _ = uint.MaxValue;
                }
                writer.WriteUInt32LittleEndian((uint)chunkMetaOffset);
                writer.WriteUInt32LittleEndian((uint)chunkMetaSize);
            }
            else
            {
                writer.WriteUInt64LittleEndian(0uL);
            }
            writer.Position = endPosition;
            if (padEnd)
            {
                writer.WritePadding(4);
            }
            return endPosition;
        }

        private void WriteEbx(FileWriter writer, DbObject ebxEntries, ref uint stringsOffset)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (ebxEntries == null)
            {
                throw new ArgumentNullException("ebxEntries");
            }
            checked
            {
                foreach (DbObject ebxEntry in ebxEntries)
                {
                    writer.WriteUInt32LittleEndian(stringsOffset);
                    stringsOffset += (uint)Encoding.ASCII.GetByteCount(ebxEntry.GetValue<string>("name")) + 1u;
                    writer.WriteUInt32LittleEndian(ebxEntry.GetValue<uint>("originalSize"));
                }
            }
        }

        private void WriteRes(FileWriter writer, DbObject resEntries, ref uint stringsOffset)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (resEntries == null)
            {
                throw new ArgumentNullException("resEntries");
            }
            checked
            {
                foreach (DbObject resEntry4 in resEntries)
                {
                    writer.WriteUInt32LittleEndian(stringsOffset);
                    stringsOffset += (uint)Encoding.ASCII.GetByteCount(resEntry4.GetValue<string>("name")) + 1u;
                    writer.WriteUInt32LittleEndian(resEntry4.GetValue<uint>("originalSize"));
                }
            }
            foreach (DbObject resEntry3 in resEntries)
            {
                writer.WriteUInt32LittleEndian((uint)resEntry3.GetValue<long>("resType"));
            }
            foreach (DbObject resEntry2 in resEntries)
            {
                writer.WriteBytes(resEntry2.GetValue<byte[]>("resMeta"));
            }
            foreach (DbObject resEntry in resEntries)
            {
                //writer.WriteInt64LittleEndian(resEntry.GetValue<long>("resRid"));

                var resRidString = resEntry["resRid"].ToString();
                if (ulong.TryParse(resRidString, out ulong resRid))
                {
                    writer.Write(resRid);
                }
                else
                {
                    throw new OverflowException("Unable to Parse resRid: " + resRidString);
                }
            }
        }

        private void WriteChunks(FileWriter writer, DbObject chunkEntries)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (chunkEntries == null)
            {
                throw new ArgumentNullException("chunkEntries");
            }
            foreach (DbObject chunkEntry in chunkEntries)
            {
                writer.WriteGuid(chunkEntry.GetValue<Guid>("id"));
                writer.WriteUInt32LittleEndian(chunkEntry.GetValue<uint>("logicalOffset"));
                writer.WriteUInt32LittleEndian(chunkEntry.GetValue<uint>("logicalSize"));
            }
        }

        private byte[] WriteChunkMeta(DbObject obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            DbObject o = obj.GetValue<DbObject>("chunkMeta");
            DbWriter dbWriter = new DbWriter(memoryStream, leaveOpen: true);
            memoryStream = new MemoryStream(dbWriter.WriteDbObject("chunkMeta", o));
            return memoryStream.ToArray();
        }
        */

        public void Write(DbObject dbObject, MemoryStream ms)
        {
            byte[] bytesToWrite = WriteToBytes(dbObject);
            using (NativeWriter nwfile = new NativeWriter(ms, leaveOpen: true))
            {
                nwfile.Position = nwfile.Length;
                nwfile.Write(bytesToWrite);
            }
        }

        public byte[] WriteToBytes(DbObject obj, bool writeData = true)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (NativeWriter writer = new NativeWriter(memoryStream, true))
            {
                var ebxCount = obj.GetValue<DbObject>("ebx").Count;
                var resCount = obj.GetValue<DbObject>("res").Count;
                var chunkCount = obj.GetValue<DbObject>("chunks").Count;
                writer.Write((int)0, Endian.Big); // size
                writer.Write((uint)3599661469, Endian.Big); // hash
                var totalCount = ebxCount + resCount + chunkCount;
                writer.Write((int)totalCount); // total count
                writer.Write((int)ebxCount); // ebx count
                writer.Write((int)resCount); // res count
                writer.Write((int)chunkCount); // chunk count
                writer.Write((int)0); // string offset
                writer.Write((int)0); // meta offset
                writer.Write((int)0); // meta size

                List<FMT.FileTools.Sha1> sha1s = obj.GetValue<List<FMT.FileTools.Sha1>>("sha1s");
                for (int i = 0; i < totalCount; i++)
                {
                    writer.Write(sha1s[i]);
                }

                WriteEbx(obj, writer);
                WriteRes(obj, writer);
                WriteChunks(obj, writer);
                uint stringOffset = (uint)writer.Position;
                writer.WriteBytes(WriteNames(obj));

                //uint metaOffset = (uint)writer.Position;
                uint metaOffset = 0u;
                byte[] chunkMetaData = null;
                if (obj.HasValue("chunkMeta"))
                {
                    metaOffset = (uint)writer.Position;
                    chunkMetaData = WriteChunkMeta(obj);
                    writer.WriteBytes(chunkMetaData);
                }
                uint length = (uint)writer.Position;
                if (writeData)
                    writer.WriteBytes(WriteData(obj));
                writer.Position = 0;
                writer.Write(length - 4, Endian.Big);

                writer.Position = 24;
                writer.Write(stringOffset - 4);
                writer.Write(metaOffset > 0 ? metaOffset - 4 : 0);
                writer.Write(metaOffset > 0 ? length - metaOffset : 0);

                foreach (DbObject o in obj.GetValue<DbObject>("ebx"))
                {
                    writer.Position = o.GetValue<int>("StringOffsetPosition");
                    writer.Write((int)o.GetValue<int>("StringOffset"));
                }
                foreach (DbObject o in obj.GetValue<DbObject>("res"))
                {
                    writer.Position = o.GetValue<int>("StringOffsetPosition");
                    writer.Write((int)o.GetValue<int>("StringOffset"));
                }
            }

            return memoryStream.ToArray();
        }

        //private byte[] WriteSha1(DbObject obj)
        //{
        //    MemoryStream memoryStream = new MemoryStream();
        //    using (NativeWriter writer = new NativeWriter(memoryStream, true))
        //    {
        //        writer.Write(new FMT.FileTools.Sha1()); // 
        //    }

        //    return memoryStream.ToArray();
        //}

        private void WriteEbx(DbObject obj, NativeWriter writer)
        {
            foreach (DbObject o in obj.GetValue<DbObject>("ebx"))
            {
                o.SetValue("StringOffsetPosition", writer.Position);
                writer.Write((int)0); // string offset
                writer.Write((int)o.GetValue<int>("originalSize")); // original size
            }
        }


        private void WriteRes(DbObject obj, NativeWriter writer)
        {
            foreach (DbObject o in obj.GetValue<DbObject>("res"))
            {
                o.SetValue("StringOffsetPosition", writer.Position);
                writer.Write((uint)o.GetValue<int>("StringOffset")); // string offset
                writer.Write((uint)o.GetValue<int>("originalSize")); // original size
            }

            foreach (DbObject o in obj.GetValue<DbObject>("res"))
            {
                var restypeid = (uint)o.GetValue<uint>("resType");
                writer.Write(restypeid, Endian.Big);
            }

            foreach (DbObject o in obj.GetValue<DbObject>("res"))
            {
                writer.Write((byte[])o.GetValue<byte[]>("resMeta"));
            }

            foreach (DbObject o in obj.GetValue<DbObject>("res"))
            {
                //ulong resRid = o.GetValue<ulong>("resRid");
                var resRidString = o["resRid"].ToString();
                if (ulong.TryParse(resRidString, out ulong resRid))
                {
                    writer.Write(resRid);
                }
                else
                {
                    throw new OverflowException("Unable to Parse resRid: " + resRidString);
                }

            }
        }

        private void WriteChunks(DbObject obj, NativeWriter writer)
        {
            foreach (DbObject o in obj.GetValue<DbObject>("chunks"))
            {
                writer.Write(o.GetValue<Guid>("id")); // guid
                writer.Write((uint)o.GetValue<uint>("logicalOffset")); // logical offset
                writer.Write((uint)o.GetValue<uint>("logicalSize")); // logical size
            }
        }

        private byte[] WriteChunkMeta(DbObject obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            DbObject o = obj.GetValue<DbObject>("chunkMeta");
            DbWriter dbWriter = new DbWriter(memoryStream, leaveOpen: true);
            memoryStream = new MemoryStream(dbWriter.WriteDbObject("chunkMeta", o));
            return memoryStream.ToArray();
        }

        private byte[] WriteNames(DbObject obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (NativeWriter writer = new NativeWriter(memoryStream, true))
            {
                foreach (DbObject o in obj.GetValue<DbObject>("ebx"))
                {
                    o.SetValue("StringOffset", writer.Position);
                    writer.WriteNullTerminatedString(o.GetValue<string>("name"));
                }
                foreach (DbObject o in obj.GetValue<DbObject>("res"))
                {
                    o.SetValue("StringOffset", writer.Position);
                    writer.WriteNullTerminatedString(o.GetValue<string>("name"));
                }
            }

            return memoryStream.ToArray();
        }

        public byte[] WriteData(DbObject obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (NativeWriter writer = new NativeWriter(memoryStream, true))
            {
                if (obj.HasValue("ebx"))
                {
                    foreach (DbObject o in obj.GetValue<DbObject>("ebx"))
                    {
                        if (o.HasValue("data"))
                        {
                            writer.Write(o.GetValue<byte[]>("data"));
                        }
                    }
                }
                if (obj.HasValue("res"))
                {
                    foreach (DbObject o in obj.GetValue<DbObject>("res"))
                    {
                        if (o.HasValue("data"))
                        {
                            writer.Write(o.GetValue<byte[]>("data"));
                        }
                    }
                }
                if (obj.HasValue("chunks"))
                {
                    foreach (DbObject o in obj.GetValue<DbObject>("chunks"))
                    {
                        if (o.HasValue("data"))
                        {
                            writer.Write(o.GetValue<byte[]>("data"));
                        }
                    }
                }
            }

            return memoryStream.ToArray();
        }
    }
}
