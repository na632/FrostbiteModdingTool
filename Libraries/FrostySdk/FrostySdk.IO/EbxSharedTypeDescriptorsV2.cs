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
				try
				{
					var t = EbxClassesTypes.FirstOrDefault(t =>
							t.GetCustomAttribute<HashAttribute>() != null
							&& Convert.ToUInt32(t.GetCustomAttribute<HashAttribute>().Hash) == nameHash);
					if (t != null)
					{
						return t.Name;
					}
				}
                catch { }
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
			if(EbxClassesAssembly == null)
				EbxClassesAssembly = AppDomain.CurrentDomain
									.GetAssemblies().FirstOrDefault(x => x.FullName.Contains("EbxClasses", StringComparison.OrdinalIgnoreCase));

			if (EbxClassesAssembly != null)
			{
				if (EbxClassesTypes == null)
					EbxClassesTypes = EbxClassesAssembly.GetTypes();

				if (EbxClassesTypes != null && EbxClassesTypes.Any())
				{
					foreach(var t in EbxClassesTypes) 
					{ 
						if(t.Name.Contains("ballrules", StringComparison.OrdinalIgnoreCase))
                        {

                        }

						PropertyInfo[] properties = t.GetProperties();
                        var hashAttrbProps = properties.Where(x => x.GetCustomAttribute<HashAttribute>() != null).ToList();
                        //var hashAttrbProps = properties.ToList();
						foreach (PropertyInfo propertyInfo in hashAttrbProps)
						{
							HashAttribute customAttribute = propertyInfo.GetCustomAttribute<HashAttribute>();
							if (customAttribute != null && Convert.ToUInt32(customAttribute.Hash) == nameHash)
							{
								return propertyInfo.Name;
							}
						}

						List<MemberInfo> members = t.GetMembers().Where(x => x.GetCustomAttribute<HashAttribute>() != null).ToList();
						foreach (MemberInfo memberInfo in members)
						{
							HashAttribute customAttribute = memberInfo.GetCustomAttribute<HashAttribute>();
							if (customAttribute != null && Convert.ToUInt32(customAttribute.Hash) == nameHash)
							{
								return memberInfo.Name;
							}
						}

						return null;
					}
				}
			}

			return null;
					
		}

		public bool ReflectionTypeDescripter = false;
		public readonly bool IsPatch;

		public int TotalFieldCount = 0;

		public Dictionary<uint, Guid> ClassIdToGuid = new Dictionary<uint, Guid>();

		public void Read(in byte[] data, in bool patch)
        {
			using (NativeReader reader = new NativeReader(new MemoryStream(data)))
			{
				reader.Position = 0;
				var magic1 = reader.ReadUInt();
				reader.ReadUInt();
				reader.ReadUInt();
				reader.ReadUInt();
				reader.ReadUInt();
				//List<(Guid, uint)> list1 = new List<(Guid, uint)>();
				uint count = reader.ReadUInt();
				for (int m = 0; m < count; m++)
				{
					uint unk1 = reader.ReadUInt();
					Guid guid = reader.ReadGuid();
					//list1.Add((guid, unk1));
					Guids.Add(guid);

					ClassIdToGuid.Add(unk1, guid);
				}
				//List<(uint, uint, int, ushort, uint, byte)> list2 = new List<(uint, uint, int, ushort, uint, byte)>();
				count = reader.ReadUInt();
				for (int l = 0; l < count; l++)
				{
					uint nameHash = reader.ReadUInt();
					uint fieldIndex = reader.ReadUInt();
					int fieldCount = reader.ReadByte();
					byte alignment = reader.ReadByte();
					ushort classType = reader.ReadUShort();
					uint size = reader.ReadUInt();
					//list2.Add((nameHash, fieldIndex, fieldCount, classType, size, alignment));
					if ((alignment & 0x80u) != 0)
					{
						fieldCount += 256;
						alignment = (byte)(alignment & 0x7Fu);
					}
					EbxClass ebxClass = default(EbxClass);
					ebxClass.NameHash = nameHash;
					ebxClass.FieldIndex = (int)fieldIndex;
					ebxClass.FieldCount = (byte)fieldCount;
					ebxClass.Alignment = (byte)((alignment == 0) ? 8 : alignment);
					ebxClass.Size = (ushort)size;
					//ebxClass.Type = (forSdkGen ? classType : ((ushort)(classType >> 1)));
					ebxClass.Type = ReflectionTypeDescripter ? ((ushort)(classType >> 1)) : classType;
					ebxClass.Index = l;
					EbxClass value = ebxClass;
					if (patch)
					{
						value.SecondSize = 1;
					}
					Mapping.Add(Guids[l], Classes.Count);
					Classes.Add(value);
				}
				//List<(uint, uint, ushort, short)> list3 = new List<(uint, uint, ushort, short)>();
				count = reader.ReadUInt();
				for (int k = 0; k < count; k++)
				{
					uint fieldNameHash = reader.ReadUInt();
					uint dataOffset = reader.ReadUInt();
					ushort type = reader.ReadUShort();
					short classRef = reader.ReadShort();// reader.ReadInt16LittleEndian();
					//list3.Add((nameHash2, dataOffset, type, classRef));
					EbxField ebxField = default(EbxField);
					ebxField.NameHash = fieldNameHash;
					//ebxField.Type = (ushort)(type >> 1);
					ebxField.Type = ReflectionTypeDescripter ? ((ushort)(type >> 1)) : type;
					ebxField.ClassRef = (ushort)classRef;
					ebxField.DataOffset = dataOffset;
					ebxField.SecondOffset = 0u;
					EbxField item = ebxField;
					Fields.Add(item);
				}
				//List<(uint, uint, uint)> list4 = new List<(uint, uint, uint)>();
				count = reader.ReadUInt();
				for (int j = 0; j < count; j++)
				{
					uint nameHash3 = reader.ReadUInt();
					uint dataOffset2 = reader.ReadUInt();
					uint unk3 = reader.ReadUInt();
					for (int n = 0; n < Fields.Count; n++)
					{
						EbxField field = Fields[n];
						if (field.NameHash == nameHash3)
						{
							field.SecondOffset = dataOffset2;
							Fields[n] = field;
						}
					}
					//list4.Add((nameHash3, unk2, unk3));
				}
				count = reader.ReadUInt();
				for (int i = 0; i < count; i++)
				{
                    uint nameHash4 = reader.ReadUInt();
                    uint dataOffset2 = reader.ReadUInt();
                    for (int n = 0; n < Fields.Count; n++)
                    {
                        EbxField field = Fields[n];
                        if (field.NameHash == nameHash4)
                        {
                            field.ThirdOffset = dataOffset2;
                            Fields[n] = field;
                        }
                    }
                }
			}
		}

		public EbxSharedTypeDescriptorV2(FileSystem fs, string name, bool patch, bool instantRead = true, bool viaReflection = false)
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

			ReflectionTypeDescripter = viaReflection;

			if (instantRead)
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
			return Classes.Count > index ? Classes[index] : null;
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
