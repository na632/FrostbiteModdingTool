using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using FrostySdk;
using FrostySdk.IO;
using Frosty.Hash;

namespace FIFA21Plugin.Plugin2
{
	public class SuperBundleReader
	{
		public const uint BinarySbMagic = 2641989333u;

		private readonly Stream stream;

		private readonly NativeReader reader;

		private readonly long streamLength;

		private uint ebxCount;

		private uint resCount;

		private uint chunkCount;

		private uint stringsOffset;

		private uint metaOffset;

		private uint metaSize;

		private List<Sha1> sha1 = new List<Sha1>();

		private bool containsUncompressedData;

		private long bundleOffset;

		public uint TotalCount
		{
			get;
			private set;
		}

		public SuperBundleReader(Stream inStream, long inBundleOffset, bool skipObfuscation = false)
		{
			stream = inStream ?? throw new ArgumentNullException("inStream");
			reader = new NativeReader(stream, skipObfuscation);
			bundleOffset = inBundleOffset;
		}

		public DbObject ReadDbObject(byte[] key)
		{
			uint num = reader.ReadUInt32BigEndian() + 4;
			uint num2 = reader.ReadUInt32BigEndian() ^ 0x7065636Eu;
			bool flag = true;
			if (num2 == 3280507699u || num2 == 3286619587u)
			{
				flag = false;
			}
			TotalCount = reader.ReadUInt32BigEndian();
			ebxCount = reader.ReadUInt32BigEndian();
			resCount = reader.ReadUInt32BigEndian();
			chunkCount = reader.ReadUInt32BigEndian();
			stringsOffset = reader.ReadUInt32BigEndian() - 36;
			metaOffset = reader.ReadUInt32BigEndian() - 36;
			metaSize = reader.ReadUInt32BigEndian();
			byte[] array = reader.ReadBytes((int)(num - stream.Position));
			switch (num2)
			{
			case 3280507699u:
				flag = false;
				break;
			case 3286619587u:
			{
				flag = false;
				using (Aes aes = Aes.Create())
				{
					aes.Key = key;
					aes.IV = key;
					aes.Padding = PaddingMode.None;
					ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
					using (MemoryStream memoryStream = new MemoryStream(array)) 
					{
						using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read))
						{
							cryptoStream.Read(array, 0, array.Length);
						}
					}
				}
				break;
			}
			}
			DbObject dbObject = new DbObject(new Dictionary<string, object>());
			MemoryStream ms2 = new MemoryStream(array);
			NativeReader NativeReader = new NativeReader(ms2);
			TocReader dbReader = new TocReader();
			for (int num3 = 0; num3 < TotalCount; num3++)
			{
				sha1.Add(flag ? NativeReader.ReadSha1() : Sha1.Zero);
			}
			dbObject.AddValue("ebx", new DbObject(ReadEbx(NativeReader)));
			dbObject.AddValue("res", new DbObject(ReadRes(NativeReader)));
			dbObject.AddValue("chunks", new DbObject(ReadChunks(NativeReader)));
			dbObject.AddValue("dataOffset", (int)(num - 4));
			if (chunkCount != 0)
			{
				ms2.Position = metaOffset + 4;
				dbObject.AddValue("chunkMeta", dbReader.Read(ms2, hasHeader: false));
			}
			stream.Position = num;
			if (stream.Position == streamLength)
			{
				return dbObject;
			}
			if (num2 == 3978096056u)
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
				throw new ArgumentNullException("list");
			}
			if (!list.IsList)
			{
				throw new ArgumentException("The provided DbObject must be a list (IsList must be true).", "list");
			}
			foreach (DbObject dbObject in list.List.Cast<DbObject>())
			{
				dbObject.AddValue("offset", bundleOffset + stream.Position);
				long originalSize = dbObject.GetValue<long>("originalSize");
				long size = 0L;
				if (containsUncompressedData)
				{
					size = originalSize;
					dbObject.AddValue("data", reader.ReadBytes((int)originalSize));
				}
				else
				{
					while (originalSize > 0)
					{
						int num3 = reader.ReadInt32BigEndian();
						ushort num6 = reader.ReadUInt16LittleEndian();
						int num4 = reader.ReadUInt16BigEndian();
						int num5 = (num6 & 0xFF00) >> 8;
						if (((uint)num5 & 0xFu) != 0)
						{
							num4 = ((num5 & 0xF) << 16) + num4;
						}
						if (((uint)num3 & 0xFF000000u) != 0)
						{
							num3 &= 0xFFFFFF;
						}
						originalSize -= num3;
						if ((num6 & 0x7F) == 0)
						{
							num4 = num3;
						}
						size += num4 + 8;
						stream.Position += num4;
					}
				}
				dbObject.AddValue("size", size);
				dbObject.AddValue("sb", true);
			}
		}

		private List<object> ReadEbx(NativeReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			List<object> list = new List<object>();
			for (int readCount = 0; readCount < ebxCount; readCount++)
			{
				DbObject dbObject = new DbObject(new Dictionary<string, object>());
				uint ebxOffset = reader.ReadUInt32BigEndian();
				uint originalSize = reader.ReadUInt32BigEndian();
				long position = reader.Position;
				stream.Position = 4 + stringsOffset + ebxOffset;
				dbObject.AddValue("sha1", sha1[readCount]);
				dbObject.AddValue("name", reader.ReadNullTerminatedString());
				dbObject.AddValue("nameHash", Fnv1.HashString(dbObject.GetValue<string>("name")));
				dbObject.AddValue("originalSize", originalSize);
				list.Add(dbObject);
				reader.Position = position;
			}
			return list;
		}

		private List<object> ReadRes(NativeReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			List<object> list = new List<object>();
			int ebxCount = (int)this.ebxCount;
			for (int num2 = 0; num2 < resCount; num2++)
			{
				DbObject dbObject = new DbObject(new Dictionary<string, object>());
				uint num3 = reader.ReadUInt32BigEndian();
				uint num4 = reader.ReadUInt32BigEndian();
				long position = reader.Position;
				stream.Position = 4 + stringsOffset + num3;
				dbObject.AddValue("sha1", sha1[ebxCount++]);
				dbObject.AddValue("name", reader.ReadNullTerminatedString());
				dbObject.AddValue("nameHash", Fnv1.HashString(dbObject.GetValue<string>("name", null)));
				dbObject.AddValue("originalSize", num4);
				list.Add(dbObject);
				reader.Position = position;
			}
			foreach (DbObject item in list.Cast<DbObject>())
			{
				item.AddValue("resType", reader.ReadUInt32BigEndian());
			}
			foreach (DbObject item2 in list.Cast<DbObject>())
			{
				item2.AddValue("resMeta", reader.ReadBytes(16));
			}
			foreach (DbObject item3 in list.Cast<DbObject>())
			{
				item3.AddValue("resRid", reader.ReadInt64BigEndian());
			}
			return list;
		}

		private List<object> ReadChunks(NativeReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			List<object> list = new List<object>();
			int num = (int)(ebxCount + resCount);
			for (int num2 = 0; num2 < chunkCount; num2++)
			{
				DbObject dbObject = new DbObject(new Dictionary<string, object>());
				Guid guid = reader.ReadGuid(Endian.Big);
				uint num3 = reader.ReadUInt32BigEndian();
				uint num4 = reader.ReadUInt32BigEndian();
				long num5 = (num3 & 0xFFFF) | num4;
				dbObject.AddValue("id", guid);
				dbObject.AddValue("sha1", sha1[num + num2]);
				dbObject.AddValue("logicalOffset", num3);
				dbObject.AddValue("logicalSize", num4);
				dbObject.AddValue("originalSize", num5);
				list.Add(dbObject);
			}
			return list;
		}
	}
}
