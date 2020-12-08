using FrostySdk;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace FIFA21Plugin.Plugin2
{
	public class TocReader
	{
		public const int HeaderV0 = 13749760;

		public const int HeaderV1 = 13749761;

		public const int HeaderV3 = 13749763;

		private const int TypeBitmask = 31;

		private byte[] key;

		public DbObject Read(string path, bool hasHeader = true)
		{
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan))
			{
				return Read(stream, hasHeader);
			}
		}

		public DbObject Read(Stream stream, bool hasHeader = true)
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
			if (hasHeader)
			{
				key = ReadHeader(stream, reader);
			}
			return (DbObject)ReadEntry(stream, reader).obj;
		}

		public static byte[] ReadHeader(Stream stream, NativeReader reader)
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
			byte[] key = null;
			int header = reader.ReadInt32BigEndian();
			if (header == 13749760 || header == 13749761)
			{
				if (header == 13749761)
				{
					stream.Position = 296L;
					key = reader.ReadBytes(260);
					for (int index = 0; index < key.Length; index++)
					{
						key[index] ^= 123;
					}
				}
				stream.Position = 556L;
			}
			else
			{
				_ = 13749763;
			}
			return key;
		}

		private (object obj, string name) ReadEntry(Stream stream, NativeReader reader)
		{
			string objName = "";
			int prefix = stream.ReadByte();
			DbObjectType type = (DbObjectType)(prefix & 0x1F);
			if (type == DbObjectType.Invalid)
			{
				return (null, objName);
			}
			if ((prefix & 0x80) == 0)
			{
				objName = reader.ReadNullTerminatedString();
			}
			switch (type)
			{
			case DbObjectType.List:
			{
				long listLength = reader.Read7BitEncodedLong();
				long startPosition = stream.Position;
				List<object> objectList = new List<object>();
				while (stream.Position - startPosition < listLength)
				{
					object obj = ReadEntry(stream, reader).obj;
					if (obj == null)
					{
						break;
					}
					objectList.Add(obj);
				}
				return (new DbObject(objectList), objName);
			}
			case DbObjectType.Object:
			{
				long num3 = reader.Read7BitEncodedLong();
				long position2 = stream.Position;
				Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				while (stream.Position - position2 < num3)
				{
					var (obj2, name) = ReadEntry(stream, reader);
					if (obj2 == null)
					{
						break;
					}
					dictionary.Add(name, obj2);
				}
				return (new DbObject(dictionary), objName);
			}
			case DbObjectType.Boolean:
				return (stream.ReadByte() == 1, objName);
			case DbObjectType.String:
				return (reader.ReadSizedString(reader.Read7BitEncodedInt()), objName);
			case DbObjectType.Int:
				return (reader.ReadInt32LittleEndian(), objName);
			case DbObjectType.Long:
				return (reader.ReadInt64LittleEndian(), objName);
			case DbObjectType.Float:
				return (reader.ReadSingleLittleEndian(), objName);
			case DbObjectType.Double:
				return (reader.ReadDoubleLittleEndian(), objName);
			case DbObjectType.Guid:
				return (reader.ReadGuid(), objName);
			case DbObjectType.Sha1:
				return (reader.ReadSha1(), objName);
			case DbObjectType.ByteArray:
				return (reader.ReadBytes(reader.Read7BitEncodedInt()), objName);
			default:
				return (null, objName);
			}
		}
	}
}
