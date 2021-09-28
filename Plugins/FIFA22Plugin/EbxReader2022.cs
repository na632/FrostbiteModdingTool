using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.FrostySdk.IO;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FIFA22Plugin
{

	
	public class EbxReader2022 : EbxReader
	{
		public static IEbxSharedTypeDescriptor std;

		public static IEbxSharedTypeDescriptor patchStd;

		public List<Guid> classGuids = new List<Guid>();

		public bool patched;

		public Type EBXType = null;

		//public override string RootType => classGuids.Count > 0 && instances.Count > 0 
		//					? TypeLibrary.GetType(classGuids[instances[0].ClassRef])?.Name 
		//					: string.Empty;
		public override string RootType => EBXType != null ? EBXType.Name : string.Empty;

		public EFIX Efix = new EFIX();

		public string fileName { get; set; }

		public static void InitialiseStd()
		{
			if (!string.IsNullOrEmpty(ProfilesLibrary.EBXTypeDescriptor))
			{
				if (std == null)
				{
					std = (IEbxSharedTypeDescriptor)AssetManager.Instance.LoadTypeByName(ProfilesLibrary.EBXTypeDescriptor
						, AssetManager.Instance.fs, "SharedTypeDescriptors.ebx", false, true, true);
				}
			}
			else
			{
				if (std == null)
				{
					std = new EbxSharedTypeDescriptors(FileSystem.Instance, "SharedTypeDescriptors.ebx", patch: false);
				}

				if (patchStd == null)
				{
					var allSTDs = FileSystem.Instance.memoryFs.Where(x => x.Key.Contains("SharedTypeDescriptors", StringComparison.OrdinalIgnoreCase)).ToList();
					if (FileSystem.Instance.HasFileInMemoryFs("SharedTypeDescriptors_patch.ebx"))
					{
						patchStd = new EbxSharedTypeDescriptors(FileSystem.Instance, "SharedTypeDescriptors_patch.ebx", patch: true);
					}
					if (FileSystem.Instance.HasFileInMemoryFs("SharedTypeDescriptors_Patch.ebx"))
					{
						patchStd = new EbxSharedTypeDescriptors(FileSystem.Instance, "SharedTypeDescriptors_Patch.ebx", patch: true);
					}
				}
			}
		}

		public long PositionBeforeStringSearch { get; set; }
		public long AfterStringsOffset { get; set; }
		public uint FileLength { get; set; }

		public List<string> Strings = new List<string>();
		public string FirstStringName => Strings[0];

		public EbxReader2022(Stream InStream, bool inPatched)
			: base(InStream, passthru: true)
		{
			InitialiseStd();

			//var fsDump = new FileStream("ebxV4.dat", FileMode.OpenOrCreate);
			//InStream.CopyTo(fsDump);
			//fsDump.Close();
			//fsDump.Dispose();

			InStream.Position = 0;

			var headerLength = 16;

			patched = inPatched;
			// RIFF
			magic = (EbxVersion)ReadUInt();
			Efix.block_header = (uint)magic;

			FileLength = (uint)ReadUInt();
			Efix.block_size = FileLength;

			var ebxHeaderName = ReadUInt(); // EBX Bla

			var ebxHeaderEndBit = ReadUInt(); // EBXD

			// 
			AfterStringsOffset = (uint)ReadULong() + 20;

			InStream.Position += 8;
			fileGuid = ReadGuid();
			Efix.file_Guid = fileGuid;
			ReadBytes(16); // 16 bytes of random empty data

			ReadLong(); // Get past 02 00 00 00 00 B1 00 00
			PositionBeforeStringSearch = InStream.Position;
			var stringPosition = ReadUInt();
			InStream.Position = stringPosition + 72;
			var name = ReadNullTerminatedString();
			Strings.Add(name);
			if (FirstStringName.Contains("gp_") || FirstStringName.Contains("ChunkFileCollector"))
			{
				var currentPositionBeforeDump = InStream.Position;
				InStream.Position = 0;
				var fsDump = new FileStream("ebxV4.dat", FileMode.OpenOrCreate);
				InStream.CopyTo(fsDump);
				fsDump.Close();
				fsDump.Dispose();
				InStream.Position = currentPositionBeforeDump;
			}


			// jump to EFIX
			InStream.Position = AfterStringsOffset;
			Pad(2);
			var efixWord = Encoding.UTF8.GetString(ReadBytes(4));// ReadUInt(); // EFIX
			if (efixWord != "EFIX")
			{
				throw new Exception("Not proper EBX file");
			}

			Efix.block_size = (uint)ReadUInt();
			Efix.file_Guid = ReadGuid();

			Efix.class_type_count = ReadUInt();
			for (var i = 0; i < Efix.class_type_count; i++)
			{
				Efix.class_types.Add(ReadGuid());
				classGuids.Add(Efix.class_types[i]);
			}

			guidCount = ReadUInt();
			Efix.type_info_Guid_count = guidCount;

			for (var i = 0; i < Efix.type_info_Guid_count; i++)
				Efix.type_info_Guids.Add(ReadUInt());

			CreateTypeFromEfix();
			//RealClassGuids.Reverse();

			Efix.data_offset_count = ReadUInt();
			Efix.unk3_count = ReadUInt();

			for (var i = 0; i < Efix.data_offset_count; i++)
				Efix.data_offsets.Add(ReadUInt());

			for (var i = 0; i < Efix.unk3_count - Efix.data_offset_count; i++)
				Efix.unk3s.Add(ReadUInt());

			Efix.unk4_count = ReadUInt();
			for (var i = 0; i < Efix.unk4_count; i++)
				Efix.unk4s.Add(ReadUInt());

			Efix.unk5_count = ReadUInt();
			for (var i = 0; i < Efix.unk5_count; i++)
				Efix.unk5s.Add(ReadUInt());

			Efix.import_reference_count = ReadUInt();
			for (var i = 0; i < Efix.import_reference_count; i++)
			{
				Efix.import_reference.Add(new EFIX.IMPORT_REFERENCE()
				{
					file_Guid = ReadGuid(),
					class_Guid = ReadGuid(),
				});
				imports.Add(new EbxImportReference() { FileGuid = Efix.import_reference[i].file_Guid, ClassGuid = Efix.import_reference[i].class_Guid });
			}

			Efix.unk6_count = ReadUInt();
			for (var i = 0; i < Efix.unk6_count; i++)
				Efix.unk6s.Add(ReadUInt());

			Efix.unk7_count = ReadUInt();
			for (var i = 0; i < Efix.unk7_count; i++)
				Efix.unk7s.Add(ReadUInt());

			Efix.data_size = ReadUInt();
			Efix.total_ebx_data_size = ReadUInt();
			Efix.total_ebx_data_size_2 = ReadUInt();


			
		}
		public List<Guid> RealClassGuids = new List<Guid>();

        private void CreateTypeFromEfix()
        {
            for (var i = 0; i < Efix.class_type_count; i++)
            {
				if (Efix.class_types.Count > i && Efix.type_info_Guids.Count > i)
				{
					var bytesOfGuidA = Efix.class_types[i].ToByteArray();
					var bytesOfGuidB = BitConverter.GetBytes(Efix.type_info_Guids[i]);

					bytesOfGuidA = bytesOfGuidA.AsSpan(4).ToArray();
					var byteGuid = new byte[16];
					for (var b = 0; b < 16; b++)
					{
						if (b > 11)
						{
							byteGuid[b] = bytesOfGuidB[b - 12];
						}
						else
						{
							byteGuid[b] = bytesOfGuidA[b];
						}

					}

					var tGuid = new Guid(byteGuid);
					if (EBXType == null)
					{
						EBXType = TypeLibrary.GetType(tGuid);
					}
					RealClassGuids.Add(tGuid);
				}
            }
        }

        public override void InternalReadObjects()
		{
			EbxClass? ebxClass = default(EbxClass);
			for (var i = 0; i < RealClassGuids.Count; i++)
			{
				ebxClass = std.GetClass(RealClassGuids[i]);
				Type type = TypeLibrary.GetType(ebxClass.Value.NameHash);
				objects.Add(TypeLibrary.CreateObject(type));
			}
			dynamic obj = objects[0];
			obj.Name = FirstStringName;

			//for (var i = 0; i < RealClassGuids.Count; i++)
			//{

			// read core class
			ebxClass = std.GetClass(RealClassGuids[0]);
				Position = PositionBeforeStringSearch;
				this.ReadClass(ebxClass.Value, objects[0], 32 + Efix.data_offsets[0]);

			//}

			//if (EBXType.Name.Contains("ChunkFileCollector"))
			//{
			//	Position = 96;
			//	obj.Manifest.ChunkId = ReadGuid();
			//}

			//foreach (EbxInstance ebxInstance in instances)
			//{
			//	Type type = TypeLibrary.GetType(classGuids[ebxInstance.ClassRef]);
			//	for (int i = 0; i < ebxInstance.Count; i++)
			//	{
			//		objects.Add(TypeLibrary.CreateObject(type));
			//		refCounts.Add(0);
			//	}
			//}
			//int num = 0;
			//int num2 = 0;
			//foreach (EbxInstance ebxInstance2 in instances)
			//{
			//	for (int j = 0; j < ebxInstance2.Count; j++)
			//	{
			//		dynamic obj = objects[num++];
			//		Type objType = obj.GetType();
			//		EbxClass @class = GetClass(objType);
			//		while (base.Position % (long)@class.Alignment != 0L)
			//		{
			//			base.Position++;
			//		}
			//		Guid inGuid = Guid.Empty;
			//		if (ebxInstance2.IsExported)
			//		{
			//			inGuid = ReadGuid();
			//		}
			//		if (@class.Alignment != 4)
			//		{
			//			base.Position += 8L;
			//		}
			//		obj.SetInstanceGuid(new AssetClassGuid(inGuid, num2++));
			//		this.ReadClass(@class, obj, base.Position - 8);
			//	}
			//}
		}

		public EbxClass GetClass(Type objType)
		{
			EbxClass? ebxClass = null;
			foreach (TypeInfoGuidAttribute typeInfoGuidAttribute in objType.GetCustomAttributes(typeof(TypeInfoGuidAttribute), inherit: true).Cast<TypeInfoGuidAttribute>())
			{
				if (classGuids.Contains(typeInfoGuidAttribute.Guid))
				{
					if (patched && patchStd != null)
					{
						ebxClass = patchStd.GetClass(typeInfoGuidAttribute.Guid);
					}
					if (!ebxClass.HasValue)
					{
						ebxClass = std.GetClass(typeInfoGuidAttribute.Guid);
					}
					break;
				}
			}
			return ebxClass.Value;
		}

		public override PropertyInfo GetProperty(Type objType, EbxField field)
		{
			if (field.NameHash == 3109710567u)
			{
				return null;
			}
			PropertyInfo[] properties = objType.GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				HashAttribute customAttribute = propertyInfo.GetCustomAttribute<HashAttribute>();
				if (customAttribute != null && (uint)customAttribute.Hash == (uint)field.NameHash)
				{
					return propertyInfo;
				}
			}
			return null;
		}

		public override EbxClass GetClass(EbxClass? parentClass, int index)
		{
			int index2 = (short)index;// + parentClass.Value.Index;
			return std.GetClass(index2).Value;
		}

		//public override EbxClass GetClass(EbxClass? classType, int index)
		//{
		//	EbxClass? ebxClass = null;
		//	Guid? guid = null;
		//	int index2 = (short)index + (classType.HasValue ? classType.Value.Index : 0);
		//	guid = std.GetGuid(index2);
		//	if (classType.HasValue && classType.Value.SecondSize == 1)
		//	{
		//		guid = patchStd.GetGuid(index2);
		//		ebxClass = patchStd.GetClass(index2);
		//		if (!ebxClass.HasValue)
		//		{
		//			ebxClass = std.GetClass(guid.Value);
		//		}
		//	}
		//	else
		//	{
		//		ebxClass = std.GetClass(index2);
		//	}
		//	if (ebxClass.HasValue)
		//	{
		//		TypeLibrary.AddType(ebxClass.Value.Name, guid);
		//	}
		//	return ebxClass.HasValue ? ebxClass.Value : default(EbxClass);
		//}

		public override EbxField GetField(EbxClass classType, int index)
		{
			if (classType.SecondSize == 1)
			{
				return patchStd.GetField(index).Value;
			}
			return std.GetField(index).Value;
		}

		public override object CreateObject(EbxClass classType)
		{
			if (classType.SecondSize == 1)
			{
				return TypeLibrary.CreateObject(patchStd.GetGuid(classType).Value);
			}
			return TypeLibrary.CreateObject(std.GetGuid(classType).Value);
		}

		public object ReadClass(EbxClassMetaAttribute classMeta, object obj, Type objType, long startOffset)
		{
			if (obj == null)
			{
				base.Position += classMeta.Size;
				while (base.Position % (long)classMeta.Alignment != 0L)
				{
					base.Position++;
				}
				return null;
			}
			if (objType.BaseType != typeof(object))
			{
				ReadClass(objType.BaseType.GetCustomAttribute<EbxClassMetaAttribute>(), obj, objType.BaseType, startOffset);
			}
			PropertyInfo[] properties = objType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.GetCustomAttribute<IsTransientAttribute>() != null)
				{
					continue;
				}
				IsReferenceAttribute customAttribute = propertyInfo.GetCustomAttribute<IsReferenceAttribute>();
				EbxFieldMetaAttribute customAttribute2 = propertyInfo.GetCustomAttribute<EbxFieldMetaAttribute>();
				base.Position = startOffset + customAttribute2.Offset;
				if (customAttribute2.Type == EbxFieldType.Array)
				{
					int index = ReadInt32LittleEndian();
					EbxArray ebxArray = arrays[index];
					long position2 = base.Position;
					base.Position = arraysOffset + ebxArray.Offset;
					propertyInfo?.GetValue(obj).GetType().GetMethod("Clear")
						.Invoke(propertyInfo.GetValue(obj), new object[0]);
					for (int i = 0; i < ebxArray.Count; i++)
					{
						object obj2 = ReadField(customAttribute2.ArrayType, customAttribute2.BaseType, customAttribute != null);
						propertyInfo?.GetValue(obj).GetType().GetMethod("Add")
							.Invoke(propertyInfo.GetValue(obj), new object[1] { obj2 });
					}
					base.Position = position2;
				}
				else
				{
					object value = ReadField(customAttribute2.Type, propertyInfo.PropertyType, customAttribute != null);
					propertyInfo?.SetValue(obj, value);
				}
			}
			while (base.Position - startOffset != classMeta.Size)
			{
				base.Position++;
			}
			return null;
		}

		public object ReadField(EbxFieldType type, Type baseType, bool dontRefCount = false)
		{
			switch (type)
			{
				case EbxFieldType.DbObject:
					throw new InvalidDataException("DbObject");
				case EbxFieldType.Struct:
					{
						object obj = TypeLibrary.CreateObject(baseType);
						EbxClassMetaAttribute customAttribute = obj.GetType().GetCustomAttribute<EbxClassMetaAttribute>();
						while (base.Position % (long)customAttribute.Alignment != 0L)
						{
							base.Position++;
						}
						ReadClass(customAttribute, obj, obj.GetType(), base.Position);
						return obj;
					}
				case EbxFieldType.Pointer:
					{
						uint num = ReadUInt();
						if (num >> 31 == 1)
						{
							EbxImportReference ebxImportReference = imports[(int)(num & 0x7FFFFFFF)];
							if (dontRefCount && !dependencies.Contains(ebxImportReference.FileGuid))
							{
								dependencies.Add(ebxImportReference.FileGuid);
							}
							return new PointerRef(ebxImportReference);
						}
						if (num == 0)
						{
							return default(PointerRef);
						}
						if (!dontRefCount)
						{
							refCounts[(int)(num - 1)]++;
						}
						return new PointerRef(objects[(int)(num - 1)]);
					}
				case EbxFieldType.String:
					return ReadSizedString(32);
				case EbxFieldType.CString:
					//return ReadCString(ReadUInt());
					return ReadCString(ReadUInt() + 48);
				case EbxFieldType.Enum:
					return ReadInt32LittleEndian();
				case EbxFieldType.FileRef:
					return ReadFileRef();
				case EbxFieldType.Boolean:
					return ReadByte() > 0;
				case EbxFieldType.Int8:
					return (sbyte)ReadByte();
				case EbxFieldType.UInt8:
					return ReadByte();
				case EbxFieldType.Int16:
					return ReadInt16LittleEndian();
				case EbxFieldType.UInt16:
					return ReadUShort();
				case EbxFieldType.Int32:
					return ReadInt32LittleEndian();
				case EbxFieldType.UInt32:
					return ReadUInt();
				case EbxFieldType.UInt64:
					return ReadUInt64LittleEndian();
				case EbxFieldType.Int64:
					return ReadInt64LittleEndian();
				case EbxFieldType.Float32:
					return ReadSingleLittleEndian();
				case EbxFieldType.Float64:
					return ReadDoubleLittleEndian();
				case EbxFieldType.Guid:
					return ReadGuid();
				case EbxFieldType.Sha1:
					return ReadSha1();
				case EbxFieldType.ResourceRef:
					return ReadResourceRef();
				case EbxFieldType.TypeRef:
					return ReadTypeRef();
				case EbxFieldType.BoxedValueRef:
					return ReadBoxedValueRef();
				default:
					throw new InvalidDataException("Unknown");
			}
		}


		public override object ReadClass(EbxClass classType, object obj, long startOffset)
		{
			Position = startOffset;

			/// DEBUG STREAM OUT
			//var pos = stream.Position;
			//var out_file = new FileStream("testReadClass.dat", FileMode.OpenOrCreate);
			//stream.CopyTo(out_file);
			//out_file.Close();
			//out_file.Dispose();
			//stream.Position = pos;

			// END OF DEBUG
			Type type = obj.GetType();
			
				List<Tuple<EbxField, PropertyInfo>> properties = new List<Tuple<EbxField, PropertyInfo>>();
				for (int i = 0; i < classType.FieldCount; i++)
				{
					EbxField field = GetField(classType, classType.FieldIndex + i);
					PropertyInfo property = GetProperty(type, field);
					properties.Add(new Tuple<EbxField, PropertyInfo>(field, property));
				}

			//for (int i = 0; i < classType.FieldCount; i++)
			//{
			//	EbxField field = GetField(classType, classType.FieldIndex + i);
			//	PropertyInfo property = GetProperty(type, field);
			foreach (var t in properties)
			{
				try
				{

					EbxField field = t.Item1;
					PropertyInfo property = t.Item2;
					Position = startOffset + field.DataOffset;

					IsReferenceAttribute isReferenceAttribute = (property != null) ? property.GetCustomAttribute<IsReferenceAttribute>() : null;
					if (field.DebugType == EbxFieldType.Inherited)
					{
						var eClass = GetClass(classType, field.ClassRef);
						if (string.IsNullOrEmpty(eClass.Name))
							continue;

						ReadClass(eClass, obj, startOffset);
						continue;
					}
					if (property != null)
					{
						if (field.DebugType == EbxFieldType.ResourceRef || field.DebugType == EbxFieldType.TypeRef || field.DebugType == EbxFieldType.FileRef || field.DebugType == EbxFieldType.BoxedValueRef || field.DebugType == EbxFieldType.UInt64 || field.DebugType == EbxFieldType.Int64 || field.DebugType == EbxFieldType.Float64)
						{
							while (Position % 8 != 0L)
							{
								Position++;
							}
						}
						else if (field.DebugType == EbxFieldType.Array || field.DebugType == EbxFieldType.Pointer)
						{
							while (Position % 4 != 0L)
							{
								Position++;
							}
						}
						if (field.DebugType == EbxFieldType.Array)
						{
							EbxClass @class = GetClass(classType, field.ClassRef);
							int index = 0;
							do
							{
								index = ReadInt();
							} while (index > arrays.Count - 1 || index < 0);
							EbxArray ebxArray = arrays[index];
							long position = Position;
							Position = arraysOffset + ebxArray.Offset;
							for (int j = 0; j < ebxArray.Count; j++)
							{
								object obj2 = ReadField(@class, GetField(@class, @class.FieldIndex), isReferenceAttribute != null);

								property.GetValue(obj).GetType().GetMethod("Add")
									.Invoke(property.GetValue(obj), new object[1]
									{
									obj2
									});
							}
							Position = position;

							if (Position > boxedValuesOffset)
							{
								boxedValuesOffset = Position;
							}
						}
						else
						{
							object value = ReadField(classType, field, true);
							property.SetValue(obj, value);
						}
					}
					else
					{
						Position += 4;
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
				}
			}
				while (Position % (long)classType.Alignment != 0L)
				{
					Position++;
				}
			
			return null;
		}

		public override object ReadField(EbxClass parentClass, EbxField fieldType, bool dontRefCount = false)
		{
			switch (fieldType.DebugType)
			{
				case EbxFieldType.Boolean:
					return (ReadByte() > 0) ? true : false;
				case EbxFieldType.Int8:
					return (sbyte)ReadByte();
				case EbxFieldType.UInt8:
					return ReadByte();
				case EbxFieldType.Int16:
					return ReadShort();
				case EbxFieldType.UInt16:
					return ReadUShort();
				case EbxFieldType.Int32:
					return ReadInt();
				case EbxFieldType.UInt32:
					return ReadUInt();
				case EbxFieldType.Int64:
					return ReadLong();
				case EbxFieldType.UInt64:
					return ReadULong();
				case EbxFieldType.Float32:
					return ReadFloat();
				case EbxFieldType.Float64:
					return ReadDouble();
				case EbxFieldType.Guid:
					return ReadGuid();
				case EbxFieldType.ResourceRef:
					return ReadResourceRef();
				case EbxFieldType.Sha1:
					return ReadSha1();
				case EbxFieldType.String:
					return ReadSizedString(32);
				case EbxFieldType.CString:
					return ReadCString(ReadUInt());
				case EbxFieldType.FileRef:
					return ReadFileRef();
				case EbxFieldType.TypeRef:
					return ReadTypeRef();
				case EbxFieldType.BoxedValueRef:
					return ReadBoxedValueRef();
				case EbxFieldType.Struct:
					{
						EbxClass @class = GetClass(parentClass, fieldType.ClassRef);
						while (Position % (long)@class.Alignment != 0L)
						{
							Position++;
						}
						object obj = CreateObject(@class);
						ReadClass(@class, obj, Position);
						return obj;
					}
				case EbxFieldType.Enum:
					return ReadInt();
				case EbxFieldType.Pointer:
					{
						uint num = ReadUInt();
						int importsV = (int)(num & int.MaxValue);
						if (num >> 31 == 1 && imports.Count > importsV) // temp measure
						{
							return new PointerRef(imports[importsV]);
						}
						else if (num == 0)
						{
							return default(PointerRef);
						}
						if (!dontRefCount)
						{
							refCounts[(int)(num - 1)]++;
						}
						if (objects.Count > num)
						{
							return new PointerRef(objects[(int)(num - 1)]);
						}
						else
                        {
							return default(PointerRef);
						}
					}
				case EbxFieldType.DbObject:
					throw new InvalidDataException("DbObject");
				default:
					throw new InvalidDataException("Unknown");
			}
		}

	}

}
