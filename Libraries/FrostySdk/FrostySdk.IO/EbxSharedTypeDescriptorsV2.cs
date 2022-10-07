using FrostySdk.Attributes;
using FrostySdk.FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FrostySdk.IO
{
	public class EbxSharedTypeDescriptorV2 : IEbxSharedTypeDescriptor
	{
		private List<EbxClass?> classes = new List<EbxClass?>();
		public List<EbxClass?> Classes { get { return classes; } }
		public Dictionary<Guid, int> mapping = new Dictionary<Guid, int>();
		public Dictionary<Guid, int> Mapping { get { return mapping; } }

		public Dictionary<int, EbxField> NameHashToEbxField = new Dictionary<int, EbxField>();

		private List<EbxField> fields = new List<EbxField>();
		public List<EbxField> Fields { get { return fields; } }

		private List<Guid> guids = new List<Guid>();
		public List<Guid> Guids { get { return guids; } }


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

		public bool ReflectionTypeDescripter { get; set; } = false;
		public readonly bool IsPatch;

		public int TotalFieldCount = 0;

		//public Dictionary<uint, Guid> ClassIdToGuid { get; } = new Dictionary<uint, Guid>();
		//public Dictionary<uint, Guid> GuidSignatures { get; } = new Dictionary<uint, Guid>();

		public List<(EbxClass, int)> BoxedValueToHash { get; } = new List<(EbxClass, int)>();

		public void Read(in byte[] data, in bool patch)
        {
			if (Mapping.Count > 0)
				return;

			using (NativeReader reader = new NativeReader(new MemoryStream(data)))
			{
				reader.Position = 0;
				var magic1 = reader.ReadUInt();
				FourCC riffType = reader.ReadUInt();
				riffType = reader.ReadUInt();
				reader.ReadUInt();
				reader.ReadUInt();
				uint guidCount = reader.ReadUInt();
				for (int m = 0; m < guidCount; m++)
				{
					uint guidHash = reader.ReadUInt();
					Guid guid = reader.ReadGuid();
					Guids.Add(guid);

					//if (!ClassIdToGuid.ContainsKey(guidHash))
					//	ClassIdToGuid.Add(guidHash, guid);

					//// Should I overwrite?
					//ClassIdToGuid[guidHash] = guid;

					//if (!GuidSignatures.ContainsKey(guidHash))
					//	GuidSignatures.Add(guidHash, guid);

					//// Should I overwrite?
					//GuidSignatures[guidHash] = guid;

				}
				var typeCount = reader.ReadUInt();
				for (int l = 0; l < typeCount; l++)
				{
					uint nameHash = reader.ReadUInt();
					uint fieldIndex = reader.ReadUInt();
					ushort fieldCount = reader.ReadUShort();
					ushort classType = reader.ReadUShort();
					ushort size = reader.ReadUShort();
					ushort alignment = reader.ReadUShort();
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
					//ebxClass.Type = ReflectionTypeDescripter ? classType : ((ushort)(classType >> 1));
					ebxClass.Type = classType;
					ebxClass.Index = l;
					ebxClass.Name = Guids[l].ToString();
					EbxClass value = ebxClass;
					if (patch)
					{
						value.SecondSize = 1;
					}
					Mapping.Add(Guids[l], Classes.Count);
					Classes.Add(value);
				}
				var fieldCountInDescriptor = reader.ReadUInt();
				for (int k = 0; k < fieldCountInDescriptor; k++)
				{
					int fieldNameHash = (int)reader.ReadUInt();
					uint dataOffset = reader.ReadUInt();
					ushort type = reader.ReadUShort();
					short classRef = reader.ReadShort();
					EbxField ebxField = default(EbxField);
					ebxField.NameHash = fieldNameHash;
                    ebxField.Type = (ushort)(type >> 1);
                    //var shiftType = (ushort)(type >> 1);
                    //ebxField.Type = type;
                    ebxField.ClassRef = (ushort)classRef;
					ebxField.DataOffset = dataOffset;
					ebxField.SecondOffset = 0u;
					fields.Add(ebxField);
					//NameHashToEbxField.Add(fieldNameHash, ebxField);
				}
				var unkCount = reader.ReadUInt();
				for (int j = 0; j < unkCount; j++)
				{
					int nameHash3 = (int)reader.ReadUInt();
					uint depthOffset = reader.ReadUInt();
					uint unk3 = reader.ReadUInt();
					for (int n = 0; n < Fields.Count; n++)
					{
						EbxField field = Fields[n];
						if (field.NameHash == nameHash3)
						{
							field.SecondOffset = depthOffset;
							field.Unk3 = unk3;
							fields[n] = field;
						}
					}
					//list4.Add((nameHash3, unk2, unk3));
				}
				uint boxedValuesCount = reader.ReadUInt();
				for (int i = 0; i < boxedValuesCount; i++)
				{
					uint fieldNameHash = reader.ReadUInt();
					int classRef2 = reader.ReadInt();
					if (classRef2 < this.classes.Count)
					{
						EbxClass? _class = this.classes[classRef2];
						BoxedValueToHash.Add((_class.Value, (int)fieldNameHash));
					}
				}
				//var arrayValueCount = reader.ReadUInt();
				//for (int i = 0; i < arrayValueCount; i++)
				//{
				//                uint nameHash4 = reader.ReadUInt();
				//                uint dataOffset2 = reader.ReadUInt();
				//                for (int n = 0; n < Fields.Count; n++)
				//                {
				//                    EbxField field = Fields[n];
				//                    if (field.NameHash == nameHash4)
				//                    {
				//                        field.ThirdOffset = dataOffset2;
				//                        fields[n] = field;
				//                    }
				//                }
				//            }
			}
		}

		public EbxSharedTypeDescriptorV2(string name, bool patch) : this(name, patch, true, true)
		{

		}

		public EbxSharedTypeDescriptorV2(FileSystem fs, string name, bool patch) : this(name, patch, true, true)
        {

        }

		public EbxSharedTypeDescriptorV2(string name, bool patch, bool instantRead = true, bool viaReflection = false)
		{
            IsPatch = patch;
            if (IsPatch)
            {

            }

            var ebxtys = FileSystem.Instance.GetFileFromMemoryFs(name);
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
		public bool ReflectionTypeDescripter { get; set; }

		public List<EbxClass?> Classes { get; }

		public Dictionary<Guid, int> Mapping { get; }

		public List<EbxField> Fields { get; }

		public List<Guid> Guids { get; }

		public EbxClass? GetClass(Guid guid);
		public EbxClass? GetClass(int index);
		public Guid? GetGuid(EbxClass classType);
		public Guid? GetGuid(int index);
		public EbxField? GetField(int index);

		public void Read(in byte[] data, in bool patch);
	}
}
