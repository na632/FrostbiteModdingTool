using FrostySdk;
using FrostySdk.IO;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace FIFA21Plugin.Plugin2
{
	public class TocWriter
	{
		public void Write(string path, DbObject DbObject, bool writeHeader = false)
		{
			using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 65536, FileOptions.SequentialScan))
			{
				Write(stream, DbObject, writeHeader);
			}
		}

		public void Write(Stream stream, DbObject DbObject, bool writeHeader = false)
		{
			if (writeHeader)
			{
				NativeWriter nativeWriter = new NativeWriter(stream);
				//Span<byte> span = stackalloc byte[4];
				nativeWriter.WriteInt32BigEndian(13749761);
				
				stream.Seek(552L, SeekOrigin.Current);
			}
			WriteObject(stream, "", DbObject);
		}

		public void WriteObject(Stream stream, string name, object obj)
		{
			NativeWriter fileWriter = new NativeWriter(stream);
			DbObjectType entryType = GetDbObjectType(obj);
			byte noNameFlag = (byte)((name == string.Empty) ? 128 : 0);
			stream.WriteByte((byte)(noNameFlag | (byte)entryType));
			if ((noNameFlag & 0x80) == 0)
			{
				fileWriter.WriteNullTerminatedString(name);
			}
			switch (entryType)
			{
			case DbObjectType.List:
			{
				DbObject obj3 = (DbObject)obj;
				MemoryStream listMs = new MemoryStream();
				foreach (object listObject in obj3.List)
				{
					WriteObject(listMs, string.Empty, listObject);
				}
				fileWriter.Write7BitEncodedLong(listMs.Length + 1);
				listMs.Position = 0L;
				listMs.CopyTo(stream, 65536);
				stream.WriteByte(0);
				break;
			}
			case DbObjectType.Object:
			{
				DbObject obj2 = (DbObject)obj;
				MemoryStream objectMs = new MemoryStream();
				foreach (KeyValuePair<string, object> kvp in obj2.Dictionary)
				{
					WriteObject(objectMs, kvp.Key, kvp.Value);
				}
				fileWriter.Write7BitEncodedLong(objectMs.Length + 1);
				objectMs.Position = 0L;
				objectMs.CopyTo(stream, 65536);
				stream.WriteByte(0);
				break;
			}
			case DbObjectType.Boolean:
				stream.WriteByte((byte)(((bool)obj) ? 1 : 0));
				break;
			case DbObjectType.String:
				fileWriter.WriteNullTerminatedString((string)obj);
				break;
			case DbObjectType.Int:
				fileWriter.WriteInt32LittleEndian((int)obj);
				break;
			case DbObjectType.Long:
				fileWriter.WriteInt64LittleEndian((long)obj);
				break;
			case DbObjectType.Float:
				fileWriter.WriteSingleLittleEndian((float)obj);
				break;
			case DbObjectType.Double:
				fileWriter.WriteDoubleLittleEndian((double)obj);
				break;
			case DbObjectType.Guid:
				fileWriter.WriteGuid((Guid)obj);
				break;
			case DbObjectType.Sha1:
				fileWriter.Write((Sha1)obj);
				break;
			case DbObjectType.ByteArray:
				fileWriter.Write7BitEncodedInt(((byte[])obj).Length);
					fileWriter.Write((byte[])obj);
				break;
			default:
				throw new InvalidDataException("Unsupported TOC entry type.");
			}
		}

		private DbObjectType GetDbObjectType(object obj)
		{
			DbObject DbObject = obj as DbObject;
			if (DbObject == null)
			{
				if (obj is bool)
				{
					return DbObjectType.Boolean;
				}
				if (obj is string)
				{
					return DbObjectType.String;
				}
				if (obj is int)
				{
					return DbObjectType.Int;
				}
				if (obj is long)
				{
					return DbObjectType.Long;
				}
				if (obj is float)
				{
					return DbObjectType.Float;
				}
				if (obj is double)
				{
					return DbObjectType.Double;
				}
				if (obj is Guid)
				{
					return DbObjectType.Guid;
				}
				if (obj is Sha1)
				{
					return DbObjectType.Sha1;
				}
				if (obj is byte[])
				{
					return DbObjectType.ByteArray;
				}
			}
			else
			{
				if (DbObject.IsObject)
				{
					return DbObjectType.Object;
				}
				if (DbObject.IsList)
				{
					return DbObjectType.List;
				}
			}
			return DbObjectType.Invalid;
		}
	}
}
