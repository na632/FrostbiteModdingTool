using FrostySdk.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace FrostySdk.IO
{
	public class DbReader : NativeReader
	{
		public DbReader(Stream inStream, IDeobfuscator inDeobfuscator)
			: base(inStream, inDeobfuscator)
		{
		}

		public virtual DbObject ReadDbObject()
		{
			return (DbObject)ReadDbObject(out string objName);
		}

		protected object ReadDbObject(out string objName)
		{
			objName = "";
			byte b = ReadByte();
			DbType dbType = (DbType)(b & 0x1F);
			if (dbType == DbType.Invalid)
			{
				return null;
			}
			if ((b & 0x80) == 0)
			{
				objName = ReadNullTerminatedString();
			}
			switch (dbType)
			{
			case DbType.List:
			{
				long num2 = Read7BitEncodedLong();
				long position2 = Position;
				List<object> list = new List<object>();
				while (Position - position2 < num2)
				{
					string objName3 = "";
					object obj2 = ReadDbObject(out objName3);
					if (obj2 == null)
					{
						break;
					}
					list.Add(obj2);
				}
				return new DbObject(list);
			}
			case DbType.Object:
			{
				long num = Read7BitEncodedLong();
				long position = Position;
				Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				while (Position - position < num)
				{
					string objName2 = "";
					object obj = ReadDbObject(out objName2);
					if (obj == null)
					{
						break;
					}
					dictionary.Add(objName2, obj);
				}
				return new DbObject(dictionary);
			}
			case DbType.Boolean:
				return (ReadByte() == 1) ? true : false;
			case DbType.String:
				return ReadSizedString(Read7BitEncodedInt());
			case DbType.Int:
				return ReadInt();
			case DbType.Long:
				return ReadLong();
			case DbType.Float:
				return ReadFloat();
			case DbType.Double:
				return ReadDouble();
			case DbType.Guid:
				return ReadGuid();
			case DbType.Sha1:
				return ReadSha1();
			case DbType.ByteArray:
				return ReadBytes(Read7BitEncodedInt());
			default:
				return null;
			}
		}
	}
}