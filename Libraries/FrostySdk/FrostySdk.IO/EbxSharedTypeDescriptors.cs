using FrostySdk.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FrostySdk.IO
{
	internal class EbxSharedTypeDescriptors
	{
		private List<EbxClass?> classes = new List<EbxClass?>();

		private Dictionary<Guid, int> mapping = new Dictionary<Guid, int>();

		private List<EbxField> fields = new List<EbxField>();

		private List<Guid> guids = new List<Guid>();

		public int ClassCount => classes.Count;

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

		public EbxSharedTypeDescriptors(FileSystem fs, string name, bool patch)
		{
			IsPatch = patch;
			if(IsPatch)
            {

            }

			var ebxtys = fs.GetFileFromMemoryFs(name);
            if (File.Exists("Debugging/" + name + ".dat"))
                File.Delete("Debugging/" + name + ".dat");

            using (NativeWriter nativeWriter = new NativeWriter(new FileStream("Debugging/" + name + ".dat", FileMode.OpenOrCreate)))
            {
                nativeWriter.Write(ebxtys);
            }

            using (NativeReader nativeReader = new NativeReader(new MemoryStream(ebxtys)))
			{
				nativeReader.Position = 0;

				var magic = nativeReader.ReadUInt();
				ushort stdClassCount = nativeReader.ReadUShort();
				ushort stdFieldCount = nativeReader.ReadUShort();
				for (int i = 0; i < stdFieldCount; i++)
				{
					uint nameHash = nativeReader.ReadUInt();
					string actualfieldname = GetPropertyName(nameHash);
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
				int totalFieldCount = 0;
				EbxClass lastValue = new EbxClass();
				for (int j = 0; j < stdClassCount; j++)
				{
					Guid guid = nativeReader.ReadGuid();
					Guid b = nativeReader.ReadGuid();
					if (guid.Equals(b))
					{
						mapping.Add(guid, classes.Count);
                        classes.Add(null);
                        //classes.Add(lastValue);
                        guids.Add(guid);
						continue;
					}
					else
					{
						nativeReader.Position -= 16L;
						uint nameHash2 = nativeReader.ReadUInt();
						uint unkuint2  = nativeReader.ReadUInt();
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
							//Name = GetClassName(nameHash2),
							NameHash = nameHash2,
							FieldIndex = totalFieldCount,
							FieldCount = (byte)fieldCount,
							Alignment = alignment,
							Size = (ushort)size,
							Type = (ushort)(type >> 1),
							Index = j
						};
						lastValue = value;
						if (patch)
						{
							value.SecondSize = 1;
						}
						mapping.Add(guid, classes.Count);
						classes.Add(value);
						guids.Add(guid);
						totalFieldCount += fieldCount;
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
			var mappedGuid = mapping[guid];
			EbxClass? c = classes.ElementAt(mappedGuid);
			//if (!c.HasValue)
			//	c = classes.ElementAt(mappedGuid - 1);
			return c;
		}

		public EbxClass? GetClass(int index)
		{
			return classes[index];
		}

		public Guid? GetGuid(EbxClass classType)
		{
			if(guids.Count > classType.Index)
				return guids[classType.Index];

			return null;
		}

		public Guid? GetGuid(int index)
		{
			if(guids.Count > index)
				return guids[index];

			return null;
		}

		public EbxField? GetField(int index)
		{
			if(fields.Count > index)
				return fields[index];

			return null;
		}
	}
}
