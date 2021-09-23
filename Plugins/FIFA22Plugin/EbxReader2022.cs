using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.FrostySdk.IO;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using System;
using System.Collections.Generic;
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
					std = (IEbxSharedTypeDescriptor)AssetManager.LoadTypeByName(ProfilesLibrary.EBXTypeDescriptor
						, AssetManager.Instance.fs, "SharedTypeDescriptors.ebx", false);
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

		public long AfterStringsOffset { get; set; }
		public uint FileLength { get; set; }

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

			InStream.Position += 12;
			fileGuid = ReadGuid();
			Efix.file_Guid = fileGuid;
			ReadGuid(); // 16 bytes of random empty data

			ReadLong(); // Get past 02 00 00 00 00 B1 00 00
			var stringPosition = ReadUInt();
			InStream.Position = stringPosition + 72;
			var name = ReadNullTerminatedString();

			// jump to EFIX
			InStream.Position = AfterStringsOffset;
			Pad(2);
			var efixWord = Encoding.UTF8.GetString(ReadBytes(4));// ReadUInt(); // EFIX
			if (efixWord != "EFIX") // This is WRONG 
			{
				throw new Exception("Not proper EBX file");
			}

			Efix.block_size = (uint)ReadUInt();
			Efix.file_Guid = ReadGuid();

			Efix.class_type_count = ReadUInt();
			for (var i = 0; i < Efix.class_type_count; i++)
			{
				Efix.class_types.Add(ReadGuid());
				var t1 = TypeLibrary.GetType(Efix.class_types[i]);
			}

			guidCount = ReadUInt();
			Efix.type_info_Guid_count = guidCount;

			for(var i =0;i< Efix.type_info_Guid_count;i++)
				Efix.type_info_Guids.Add(ReadUInt());

			for (var i = 0; i < Efix.class_type_count; i++)
			{
				if (EBXType == null)
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
					EBXType = TypeLibrary.GetType(tGuid);
				}
			}


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
				if (i == 0) 
				{
					var t1 = TypeLibrary.GetType(Efix.import_reference[0].file_Guid);
					var t2 = TypeLibrary.GetType(Efix.import_reference[0].class_Guid);

				}
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

		public override void InternalReadObjects()
		{
			foreach (EbxInstance ebxInstance in instances)
			{
				Type type = TypeLibrary.GetType(classGuids[ebxInstance.ClassRef]);
				for (int i = 0; i < ebxInstance.Count; i++)
				{
					objects.Add(TypeLibrary.CreateObject(type));
					refCounts.Add(0);
				}
			}
			int num = 0;
			int num2 = 0;
			foreach (EbxInstance ebxInstance2 in instances)
			{
				for (int j = 0; j < ebxInstance2.Count; j++)
				{
					dynamic obj = objects[num++];
					Type objType = obj.GetType();
					EbxClass @class = GetClass(objType);
					while (base.Position % (long)@class.Alignment != 0L)
					{
						base.Position++;
					}
					Guid inGuid = Guid.Empty;
					if (ebxInstance2.IsExported)
					{
						inGuid = ReadGuid();
					}
					if (@class.Alignment != 4)
					{
						base.Position += 8L;
					}
					obj.SetInstanceGuid(new AssetClassGuid(inGuid, num2++));
					this.ReadClass(@class, obj, base.Position - 8);
				}
			}
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
				if (customAttribute != null && customAttribute.Hash == (int)field.NameHash)
				{
					return propertyInfo;
				}
			}
			return null;
		}

		public override EbxClass GetClass(EbxClass? classType, int index)
		{
			EbxClass? ebxClass = null;
			Guid? guid = null;
			int index2 = (short)index + (classType.HasValue ? classType.Value.Index : 0);
			guid = std.GetGuid(index2);
			if (classType.HasValue && classType.Value.SecondSize == 1)
			{
				guid = patchStd.GetGuid(index2);
				ebxClass = patchStd.GetClass(index2);
				if (!ebxClass.HasValue)
				{
					ebxClass = std.GetClass(guid.Value);
				}
			}
			else
			{
				ebxClass = std.GetClass(index2);
			}
			if (ebxClass.HasValue)
			{
				TypeLibrary.AddType(ebxClass.Value.Name, guid);
			}
			return ebxClass.Value;
		}

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
					return ReadCString(ReadUInt());
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
	}

}
