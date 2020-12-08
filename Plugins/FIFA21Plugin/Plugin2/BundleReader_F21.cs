using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frosty.Hash;
using FrostySdk;
using FrostySdk.IO;

namespace FIFA21Plugin.Plugin2
{
	public class BundleReader_F21
	{
		public const uint HeaderMagic = 3599661469u;

		public int TotalCount
		{
			get;
			private set;
		}

		public int EbxCount
		{
			get;
			private set;
		}

		public int ResCount
		{
			get;
			private set;
		}

		public int ChunkCount
		{
			get;
			private set;
		}

		public DbObject Read(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("Stream must support reading.", "stream");
			}
			if (!stream.CanSeek)
			{
				throw new ArgumentException("Stream must support seeking.", "stream");
			}
			NativeReader reader = new NativeReader(stream);
			long bundleStartOffset = reader.Position;
			uint headerMagic = reader.ReadUInt32BigEndian();
			if (headerMagic != 3599661469u)
			{
				throw new InvalidDataException($"Invalid header magic in bundle meta. Expected \"{3599661469u}\" but got \"{headerMagic}\"");
			}
			TotalCount = reader.ReadInt32LittleEndian();
			EbxCount = reader.ReadInt32LittleEndian();
			ResCount = reader.ReadInt32LittleEndian();
			ChunkCount = reader.ReadInt32LittleEndian();
			long stringOffset = reader.ReadInt32LittleEndian() + bundleStartOffset;
			long chunkMetaOffset = reader.ReadInt32LittleEndian() + bundleStartOffset;
			reader.ReadInt32LittleEndian();
			List<Sha1> Sha1es = new List<Sha1>(TotalCount);
			for (int i = 0; i < TotalCount; i++)
			{
				Sha1es.Add(reader.ReadSha1());
			}
			DbObject dbObject = new DbObject(new Dictionary<string, object>());
			List<object> ebxEntries = ReadEbx(EbxCount, Sha1es, stringOffset, reader);
			dbObject.AddValue("ebx", new DbObject(ebxEntries));
			List<object> resEntries = ReadRes(Sha1es, stringOffset, reader);
			dbObject.AddValue("res", new DbObject(resEntries));
			List<object> chunkEntries = ReadChunks(Sha1es, reader);
			dbObject.AddValue("chunks", new DbObject(chunkEntries));
			if (ChunkCount > 0)
			{
				reader.Position = chunkMetaOffset;
				DbObject chunkMetaData = new TocReader().Read(stream, hasHeader: false);
				dbObject.AddValue("chunkMeta", chunkMetaData);
			}
			return dbObject;
		}

		private List<object> ReadEbx(int ebxCount, List<Sha1> inShas, long stringOffset, NativeReader reader)
		{
			if (inShas == null)
			{
				throw new ArgumentNullException("No Sha1 found");
			}
			if (stringOffset < 0)
			{
				throw new ArgumentOutOfRangeException("stringOffset", stringOffset, "String offset cannot be negative.");
			}
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			List<object> list = new List<object>();
			for (int i = 0; i < ebxCount; i++)
			{
				DbObject dbObject = new DbObject(new Dictionary<string, object>());
				uint nameOffset = reader.ReadUInt32LittleEndian();
				uint originalSize = reader.ReadUInt32LittleEndian();
				long storedPosition = reader.Position;
				reader.Position = stringOffset + nameOffset;
				string ebxName = reader.ReadNullTerminatedString();
				reader.Position = storedPosition;
				dbObject.AddValue("sha1", inShas[i]);
				dbObject.AddValue("name", ebxName);
				dbObject.AddValue("nameHash", Fnv1.HashString(ebxName));
				dbObject.AddValue("originalSize", originalSize);
				list.Add(dbObject);
			}
			return list;
		}

		private List<object> ReadRes(List<Sha1> Sha1es, long stringOffset, NativeReader reader)
		{
			if (Sha1es == null)
			{
				throw new ArgumentNullException("Sha1es");
			}
			if (stringOffset < 0)
			{
				throw new ArgumentOutOfRangeException("stringOffset", stringOffset, "String offset cannot be negative.");
			}
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			List<object> list = new List<object>();
			for (int i = 0; i < ResCount; i++)
			{
				DbObject dbObject = new DbObject(new Dictionary<string, object>());
				uint nameOffset = reader.ReadUInt32LittleEndian();
				uint originalSize = reader.ReadUInt32LittleEndian();
				long storedPosition = reader.Position;
				reader.Position = stringOffset + nameOffset;
				string resName = reader.ReadNullTerminatedString();
				reader.Position = storedPosition;
				dbObject.AddValue("sha1", Sha1es[EbxCount + i]);
				dbObject.AddValue("name", resName);
				dbObject.AddValue("nameHash", Fnv1.HashString(resName));
				dbObject.AddValue("originalSize", originalSize);
				list.Add(dbObject);
			}
			foreach (DbObject item in list.Cast<DbObject>())
			{
				item.AddValue("resType", reader.ReadUInt32LittleEndian());
			}
			foreach (DbObject item2 in list.Cast<DbObject>())
			{
				item2.AddValue("resMeta", reader.ReadBytes(16));
			}
			foreach (DbObject item3 in list.Cast<DbObject>())
			{
				item3.AddValue("resRid", reader.ReadInt64LittleEndian());
			}
			return list;
		}

		private List<object> ReadChunks(List<Sha1> Sha1es, NativeReader reader)
		{
			if (Sha1es == null)
			{
				throw new ArgumentNullException("Sha1es");
			}
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			List<object> list = new List<object>();
			for (int i = 0; i < ChunkCount; i++)
			{
				DbObject dbObject = new DbObject(new Dictionary<string, object>());
				Guid id = reader.ReadGuid();
				uint logicalOffset = reader.ReadUInt32LittleEndian();
				uint logicalSize = reader.ReadUInt32LittleEndian();
				long originalSize = (logicalOffset & 0xFFFF) | logicalSize;
				dbObject.AddValue("id", id);
				dbObject.AddValue("sha1", Sha1es[EbxCount + ResCount + i]);
				dbObject.AddValue("logicalOffset", logicalOffset);
				dbObject.AddValue("logicalSize", logicalSize);
				dbObject.AddValue("originalSize", originalSize);
				list.Add(dbObject);
			}
			return list;
		}
	}
}
