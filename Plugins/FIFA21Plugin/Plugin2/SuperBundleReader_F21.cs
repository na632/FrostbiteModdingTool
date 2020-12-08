using FrostySdk;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FIFA21Plugin.Plugin2
{
	public class SuperBundleReader_F21
	{
		public const int Magic = 32;

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
			int magic = reader.ReadInt32BigEndian();
			if (magic != 32)
			{
				throw new InvalidDataException($"Invalid header magic in superbundle file. Expected \"{32}\" but got \"{magic}\"");
			}
			reader.ReadInt32BigEndian();
			int flagsOffset = reader.ReadInt32BigEndian();
			int count = reader.ReadInt32BigEndian();
			int offset2 = reader.ReadInt32BigEndian();
			reader.ReadInt32BigEndian();
			reader.ReadInt32BigEndian();
			reader.ReadInt32BigEndian();
			reader.ReadInt32BigEndian();
			DbObject dbObject = new BundleReader_F21().Read(stream);
			IEnumerable<DbObject> ebxEntries = dbObject.GetValue<DbObject>("ebx").List.Cast<DbObject>();
			IEnumerable<DbObject> resEntries = dbObject.GetValue<DbObject>("res").List.Cast<DbObject>();
			IEnumerable<DbObject> chunkEntries = dbObject.GetValue<DbObject>("chunks").List.Cast<DbObject>();
			reader.Position = flagsOffset;
			byte[] flags = reader.ReadBytes(count);
			reader.Position = offset2;
			(int, int, bool) currentCasId = default((int, int, bool));
			IEnumerable<DbObject> enumerable = ebxEntries.Cast<DbObject>().Union(resEntries.Cast<DbObject>()).Union(chunkEntries.Cast<DbObject>());
			int i = 0;
			foreach (DbObject entry in enumerable)
			{
				if (flags[i++] == 1)
				{
					TryParseCasIdentifier(reader.ReadInt32BigEndian(), out currentCasId);
				}
				int addr = reader.ReadInt32BigEndian();
				int size = reader.ReadInt32BigEndian();
				entry.AddValue("offset", addr);
				entry.AddValue("size", size);
				entry.AddValue("catalog", currentCasId.Item1);
				entry.AddValue("cas", currentCasId.Item2);
				if (currentCasId.Item3)
				{
					entry.AddValue("patch", true);
				}
			}
			return dbObject;
		}

		private static bool TryParseCasIdentifier(int value, out (int packageIndex, int casIndex, bool isPatch) result)
		{
			int packageIndex = (value >> 8) & 0xFF;
			int casIndex = value & 0xFF;
			int isPatch = value >> 16;
			if (isPatch == 0 || isPatch == 1)
			{
				result = (packageIndex, casIndex, isPatch != 0);
				return true;
			}
			result = default((int, int, bool));
			return false;
		}
	}
}
