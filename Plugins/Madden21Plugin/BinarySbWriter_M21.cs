using FMT.FileTools;
using FrostySdk;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Madden21Plugin
{
    public class BinarySbWriter_M21
    {

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
                writer.Write(0, Endian.Big); // size
                writer.Write(3018715229); // hash
                var totalCount = ebxCount + resCount + chunkCount;
                writer.Write(totalCount); // total count
                writer.Write(ebxCount); // ebx count
                writer.Write(resCount); // res count
                writer.Write(chunkCount); // chunk count
                writer.Write(0); // string offset
                writer.Write(0); // meta offset
                writer.Write(0); // meta size

                WriteEbx(obj, writer);
                WriteRes(obj, writer);
                WriteChunks(obj, writer);
                //writer.WriteBytes(WriteEbx(obj));
                //writer.WriteBytes(WriteRes(obj));
                //writer.WriteBytes(WriteChunks(obj));
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
                    writer.Write(o.GetValue<int>("StringOffset"));
                }
                foreach (DbObject o in obj.GetValue<DbObject>("res"))
                {
                    writer.Position = o.GetValue<int>("StringOffsetPosition");
                    writer.Write(o.GetValue<int>("StringOffset"));
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
                writer.Write(0); // string offset
                writer.Write(o.GetValue<int>("originalSize")); // original size
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
                var restypeid = o.GetValue<uint>("resType");
                writer.Write(restypeid, Endian.Big);
            }

            foreach (DbObject o in obj.GetValue<DbObject>("res"))
            {
                writer.Write(o.GetValue<byte[]>("resMeta"));
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
                writer.Write(o.GetValue<uint>("logicalOffset")); // logical offset
                writer.Write(o.GetValue<uint>("logicalSize")); // logical size
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
                        else
                        {
                            throw new KeyNotFoundException("Data was not found for ebx " + o["name"].ToString());
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
                        else
                        {
                            throw new KeyNotFoundException("Data was not found for res " + o["name"].ToString());
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
                        else
                        {
                            throw new KeyNotFoundException("Data was not found for chunk " + obj.GetValue<Guid>("Id").ToString());
                        }
                    }
                }
            }

            return memoryStream.ToArray();
        }

    }
}
