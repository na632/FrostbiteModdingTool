using System;
using System.Collections.Generic;
using System.IO;

namespace FrostySdk.IO
{
	internal class EbxSharedTypeDescriptors
	{
		private List<EbxClass?> classes = new List<EbxClass?>();

		private Dictionary<Guid, int> mapping = new Dictionary<Guid, int>();

		private List<EbxField> fields = new List<EbxField>();

		private List<Guid> guids = new List<Guid>();

		public int ClassCount => classes.Count;

		public EbxSharedTypeDescriptors(FileSystem fs, string name, bool patch)
		{
			using (NativeReader nativeReader = new NativeReader(new MemoryStream(fs.GetFileFromMemoryFs(name))))
			{
				nativeReader.ReadUInt();
				ushort num = nativeReader.ReadUShort();
				ushort num2 = nativeReader.ReadUShort();
				for (int i = 0; i < num2; i++)
				{
					uint nameHash = nativeReader.ReadUInt();
					EbxField item = new EbxField
					{
						NameHash = nameHash,
						Type = (ushort)(nativeReader.ReadUShort() >> 1),
						ClassRef = nativeReader.ReadUShort(),
						DataOffset = nativeReader.ReadUInt(),
						SecondOffset = nativeReader.ReadUInt()
					};
					fields.Add(item);
				}
				int num3 = 0;
				for (int j = 0; j < num; j++)
				{
					Guid guid = nativeReader.ReadGuid();
					Guid b = nativeReader.ReadGuid();
					if (guid == b)
					{
						mapping.Add(guid, classes.Count);
						classes.Add(null);
						guids.Add(guid);
					}
					else
					{
						nativeReader.Position -= 16L;
						uint nameHash2 = nativeReader.ReadUInt();
						nativeReader.ReadUInt();
						int num4 = nativeReader.ReadByte();
						byte b2 = nativeReader.ReadByte();
						ushort num5 = nativeReader.ReadUShort();
						uint num6 = nativeReader.ReadUInt();
						if ((b2 & 0x80) != 0)
						{
							num4 += 256;
							b2 = (byte)(b2 & 0x7F);
						}
						EbxClass value = new EbxClass
						{
							NameHash = nameHash2,
							FieldIndex = num3,
							FieldCount = (byte)num4,
							Alignment = b2,
							Size = (ushort)num6,
							Type = (ushort)(num5 >> 1),
							Index = j
						};
						if (patch)
						{
							value.SecondSize = 1;
						}
						mapping.Add(guid, classes.Count);
						classes.Add(value);
						guids.Add(guid);
						num3 += num4;
					}
				}
			}
		}

		public bool HasClass(Guid guid)
		{
			return mapping.ContainsKey(guid);
		}

		public EbxClass? GetClass(Guid guid)
		{
			if (!mapping.ContainsKey(guid))
			{
				return null;
			}
			return classes[mapping[guid]];
		}

		public EbxClass? GetClass(int index)
		{
			return classes[index];
		}

		public Guid GetGuid(EbxClass classType)
		{
			return guids[classType.Index];
		}

		public Guid GetGuid(int index)
		{
			return guids[index];
		}

		public EbxField GetField(int index)
		{
			return fields[index];
		}
	}
}
