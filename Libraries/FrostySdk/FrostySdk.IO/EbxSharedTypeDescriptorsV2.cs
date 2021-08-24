using FrostySdk.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FrostySdk.IO
{
	public class EbxSharedTypeDescriptorV2 : IEbxSharedTypeDescriptor
	{
		public List<EbxClass?> Classes = new List<EbxClass?>();

		public Dictionary<Guid, int> Mapping = new Dictionary<Guid, int>();

		public List<EbxField> Fields = new List<EbxField>();

		public List<Guid> Guids = new List<Guid>();


		private static Assembly EbxClassesAssembly;
		private static Type[] EbxClassesTypes;

		public static string GetClassName(uint nameHash)
		{
			EbxClassesAssembly = AppDomain.CurrentDomain
									.GetAssemblies().FirstOrDefault(x => x.FullName.Contains("EbxClasses", StringComparison.OrdinalIgnoreCase));
			if(EbxClassesAssembly != null)
			{
				if(EbxClassesTypes == null)
					EbxClassesTypes = EbxClassesAssembly.GetTypes();

				//for (var i = 0; i < EbxClassesTypes.Length; i++)
				//{
					var t = EbxClassesTypes.FirstOrDefault(t =>
							t.GetCustomAttribute<HashAttribute>() != null
							&& t.GetCustomAttribute<HashAttribute>().Hash == nameHash);
					if(t != null)
                    {
						return t.Name;
                    }
					//var hash = t.GetCustomAttribute<HashAttribute>();
					//if(hash != null && (hash.Hash == nameHash || hash.ActualHash == nameHash))
     //               {
					//	return t.Name;
     //               }
				//}
			}

			return null;

		}

		public static string GetPropertyName(uint nameHash)
		{
			if (EbxClassesAssembly != null)
			{
				if (EbxClassesTypes == null)
					EbxClassesTypes = EbxClassesAssembly.GetTypes();

				var types = EbxClassesTypes;
				for (var i = 0; i < types.Length; i++)
				{
					var t = types[i];
					PropertyInfo[] properties = t.GetProperties();
					var hashAttrbProps = properties.ToList().Where(x => x.GetCustomAttribute<HashAttribute>() != null).ToList();
					foreach (PropertyInfo propertyInfo in hashAttrbProps)
					{
						HashAttribute customAttribute = propertyInfo.GetCustomAttribute<HashAttribute>();
						if (customAttribute != null && customAttribute.Hash == nameHash)
						{
							return propertyInfo.Name;
						}
					}
					return null;
				}
			}

			return null;
					
		}

		public readonly bool IsPatch;

		public int TotalFieldCount = 0;

		public void Read(in byte[] data, in bool patch)
        {
			using (NativeReader nativeReader = new NativeReader(new MemoryStream(data)))
			{
				nativeReader.Position = 0;
				var magic1 = nativeReader.ReadUInt();
				var count1 = nativeReader.ReadUInt();
				nativeReader.Position += 8;
				var fileLengthExcludingTop = nativeReader.ReadUInt();
				var fileLength = fileLengthExcludingTop + 20;
				var countGuidClasses = nativeReader.ReadUInt();
				if (count1 > 0 && fileLengthExcludingTop > 0 && countGuidClasses > 0)
				{
					for (var i = 0; i < countGuidClasses; i++)
					{
						var guid1 = nativeReader.ReadGuid();
						var hash1 = nativeReader.ReadUInt();
						Guids.Add(guid1);
					}
					var countClasses = nativeReader.ReadUInt();
					for (var i = 0; i < countClasses; i++)
					{
						uint nameHash2 = nativeReader.ReadUInt();
						uint unkuint2 = nativeReader.ReadUInt();
						int fieldCount = nativeReader.ReadByte();
						byte alignment = nativeReader.ReadByte();
						ushort type = nativeReader.ReadUShort();
						uint size = nativeReader.ReadUInt();
						if ((alignment & 0x80) != 0)
						{
							fieldCount += 256;
							alignment = (byte)(alignment & 0x7F);
						}
						EbxClass value = new EbxClass
						{
							NameHash = nameHash2,
							FieldIndex = TotalFieldCount,
							FieldCount = (byte)fieldCount,
							Alignment = (alignment == 0 ? Convert.ToByte(8) : alignment),
							Size = (ushort)size,
							Type = (ushort)(type >> 1),
							Index = i
						};
						if (patch)
						{
							value.SecondSize = 1;
						}
						Mapping.Add(Guids[i], Classes.Count);
						Classes.Add(value);
						//guids.Add(guid);
						TotalFieldCount += fieldCount;
					}
					var countFields = nativeReader.ReadUInt();
					for (var i = 0; i < countFields; i++)
					{
						// 12 bytes
						//nativeReader.Position += 12;

						uint nameHash = nativeReader.ReadUInt();
						string actualfieldname = GetPropertyName(nameHash);

						var unk1 = nativeReader.ReadUInt();

						// group of 4
						ushort fieldType = (ushort)(nativeReader.ReadUShort() >> 1);
						var fieldClassRef = nativeReader.ReadUShort();
						// 
						Fields.Add(new EbxField() { 
						
							NameHash = nameHash,
							Type = fieldType,
							ClassRef = fieldClassRef,
							//DataOffset = unk1
						
						});
					}
					var countFields2 = nativeReader.ReadUInt();
					for (var i = 0; i < countFields2; i++)
					{
						var hash = nativeReader.ReadUInt();
						var unk1 = nativeReader.ReadUInt();
						var unk2 = nativeReader.ReadUInt();
						if (Fields.Any(x => x.NameHash == hash))
						{

						}
						if (Classes.Any(x => x.Value.NameHash == hash))
						{

						}
					}

					var countFields3 = nativeReader.ReadUInt();
					for (var i = 0; i < countFields3; i++)
					{
						var hash = nativeReader.ReadUInt();
						var offset = nativeReader.ReadUInt();
						if (Fields.Any(x => x.NameHash == hash))
						{
							EbxField f = Fields.FirstOrDefault(x => x.NameHash == hash);
							var nField = new EbxField() { NameHash = hash, DataOffset = offset, Type = f.Type, ClassRef = f.ClassRef };
							Fields = Fields.Where(x=> x.NameHash != hash).ToList();
							Fields.Add(nField);
						}
					}
				}

				//nativeReader.Position = 0;

				//var magic = nativeReader.ReadUInt();
				//ushort stdClassCount = nativeReader.ReadUShort();
				//ushort stdFieldCount = nativeReader.ReadUShort();
				//for (int i = 0; i < stdFieldCount; i++)
				//{
				//	uint nameHash = nativeReader.ReadUInt(); // 4
				//	string actualfieldname = GetPropertyName(nameHash);
				//	EbxField item = new EbxField
				//	{
				//		NameHash = nameHash,
				//		Type = (ushort)(nativeReader.ReadUShort() >> 1), // 6
				//		ClassRef = nativeReader.ReadUShort(), // 8
				//		DataOffset = nativeReader.ReadUInt(), // 12
				//		SecondOffset = nativeReader.ReadUInt() // 16
				//	};
				//	fields.Add(item);
				//}
				//int totalFieldCount = 0;
				//EbxClass lastValue = new EbxClass();
				//for (int j = 0; j < stdClassCount; j++)
				//{
				//	Guid guid = nativeReader.ReadGuid();
				//	Guid b = nativeReader.ReadGuid();
				//	if (guid.Equals(b))
				//	{
				//		mapping.Add(guid, classes.Count);
				//                    classes.Add(null);
				//                    //classes.Add(lastValue);
				//                    guids.Add(guid);
				//		continue;
				//	}
				//	else
				//	{
				//		nativeReader.Position -= 16L;
				//		uint nameHash2 = nativeReader.ReadUInt();
				//		uint unkuint2  = nativeReader.ReadUInt();
				//		int fieldCount = nativeReader.ReadByte();
				//		byte alignment = nativeReader.ReadByte();
				//		ushort type = nativeReader.ReadUShort();
				//		uint size = nativeReader.ReadUInt();
				//		if ((alignment & 0x80) != 0)
				//		{
				//			fieldCount += 256;
				//			alignment = (byte)(alignment & 0x7F);
				//		}
				//		EbxClass value = new EbxClass
				//		{
				//			//Name = GetClassName(nameHash2),
				//			NameHash = nameHash2,
				//			FieldIndex = totalFieldCount,
				//			FieldCount = (byte)fieldCount,
				//			Alignment = alignment,
				//			Size = (ushort)size,
				//			Type = (ushort)(type >> 1),
				//			Index = j
				//		};
				//		lastValue = value;
				//		if (patch)
				//		{
				//			value.SecondSize = 1;
				//		}
				//		mapping.Add(guid, classes.Count);
				//		classes.Add(value);
				//		guids.Add(guid);
				//		totalFieldCount += fieldCount;
				//	}
				//}
			}
		}

		public EbxSharedTypeDescriptorV2(FileSystem fs, string name, bool patch)
		{
            IsPatch = patch;
            if (IsPatch)
            {

            }

            var ebxtys = fs.GetFileFromMemoryFs(name);
            if (File.Exists("Debugging/" + name + ".dat"))
                File.Delete("Debugging/" + name + ".dat");

            using (NativeWriter nativeWriter = new NativeWriter(new FileStream("Debugging/" + name + ".dat", FileMode.OpenOrCreate)))
            {
                nativeWriter.Write(ebxtys);
            }

			Read(ebxtys, false);
		}

		public bool HasClass(Guid guid)
		{
			return Mapping.ContainsKey(guid);
		}

		public EbxClass? GetClass(Guid guid)
		{
			if (!Mapping.ContainsKey(guid))
			{
				return null;
			}
			var mappedGuid = Mapping[guid];
			EbxClass? c = Classes.ElementAt(mappedGuid);
			//if (!c.HasValue)
			//	c = classes.ElementAt(mappedGuid - 1);
			return c;
		}

		public EbxClass? GetClass(int index)
		{
			return Classes[index];
		}

		public Guid? GetGuid(EbxClass classType)
		{
			if(Guids.Count > classType.Index)
				return Guids[classType.Index];

			return null;
		}

		public Guid? GetGuid(int index)
		{
			if(Guids.Count > index)
				return Guids[index];

			return null;
		}

		public EbxField? GetField(int index)
		{
			if(Fields.Count > index)
				return Fields[index];

			return null;
		}
	}

    public interface IEbxSharedTypeDescriptor
    {
		public EbxClass? GetClass(Guid guid);
		public EbxClass? GetClass(int index);
		public Guid? GetGuid(EbxClass classType);
		public Guid? GetGuid(int index);
		public EbxField? GetField(int index);
	}
}
