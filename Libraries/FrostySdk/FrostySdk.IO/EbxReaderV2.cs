using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FrostySdk.IO
{
	public class EbxReaderV2 : EbxReader
	{
		protected List<Guid> classGuids = new List<Guid>();

		internal static EbxSharedTypeDescriptors std;

		internal static EbxSharedTypeDescriptors patchStd;

		protected bool patched;

		public override string RootType
		{
			get
			{
				Type type = TypeLibrary.GetType(classGuids[instances[0].ClassRef]);
				if (type != null)
				{
					return type.Name;
				}
				return "";
			}
		}

		public static void InitialiseStd()
        {
			if (std == null)
			{
				std = new EbxSharedTypeDescriptors(AssetManager.Instance.fs, "SharedTypeDescriptors.ebx", patch: false);
			}

			if(patchStd == null)
			{
				var allSTDs = AssetManager.Instance.fs.memoryFs.Where(x => x.Key.Contains("SharedTypeDescriptors", StringComparison.OrdinalIgnoreCase)).ToList();
				if (AssetManager.Instance.fs.HasFileInMemoryFs("SharedTypeDescriptors_patch.ebx"))
				{
					patchStd = new EbxSharedTypeDescriptors(AssetManager.Instance.fs, "SharedTypeDescriptors_patch.ebx", patch: true);
				}
				if (AssetManager.Instance.fs.HasFileInMemoryFs("SharedTypeDescriptors_Patch.ebx"))
				{
					patchStd = new EbxSharedTypeDescriptors(AssetManager.Instance.fs, "SharedTypeDescriptors_Patch.ebx", patch: true);
				}
			}
		}

		public override void InitialRead(Stream InStream, bool inPatched)
        {
			InitialiseStd();

			patched = inPatched;
			magic = (EbxVersion)ReadUInt();
			if (magic != EbxVersion.Version2 && magic != EbxVersion.Version4)
			{
				return;
			}
			stringsOffset = ReadUInt();
			stringsAndDataLen = ReadUInt();
			guidCount = ReadUInt();
			instanceCount = ReadUShort();
			exportedCount = ReadUShort();
			uniqueClassCount = ReadUShort();
			classTypeCount = ReadUShort();
			fieldTypeCount = ReadUShort();
			typeNamesLen = ReadUShort();
			stringsLen = ReadUInt();
			arrayCount = ReadUInt();
			dataLen = ReadUInt();
			arraysOffset = stringsOffset + stringsLen + dataLen;
			fileGuid = ReadGuid();
			boxedValuesCount = ReadUInt();
			boxedValuesOffset = ReadUInt();
			boxedValuesOffset += stringsOffset + stringsLen;
			for (int i = 0; i < guidCount; i++)
			{
				EbxImportReference item = new EbxImportReference
				{
					FileGuid = ReadGuid(),
					ClassGuid = ReadGuid()
				};
				imports.Add(item);
				if (!dependencies.Contains(item.FileGuid))
				{
					dependencies.Add(item.FileGuid);
				}
			}
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			long position = Position;
			while (Position - position < typeNamesLen)
			{
				string text = ReadNullTerminatedString();
				int key = HashString(text);
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, text);
				}
			}
			for (int j = 0; j < fieldTypeCount; j++)
			{
				EbxField item2 = default(EbxField);
				int key2 = ReadInt();
				item2.Type = ((magic == EbxVersion.Version2) ? ReadUShort() : ((ushort)(ReadUShort() >> 1)));
				item2.ClassRef = ReadUShort();
				item2.DataOffset = ReadUInt();
				item2.SecondOffset = ReadUInt();
				item2.Name = dictionary[key2];
				fieldTypes.Add(item2);
			}
			for (int k = 0; k < classTypeCount; k++)
			{
				Guid item3 = ReadGuid();
				classGuids.Add(item3);
			}
			ushort num = exportedCount;
			for (int l = 0; l < instanceCount; l++)
			{
				EbxInstance item4 = new EbxInstance
				{
					ClassRef = ReadUShort(),
					Count = ReadUShort()
				};
				if (num != 0)
				{
					item4.IsExported = true;
					num = (ushort)(num - 1);
				}
				instances.Add(item4);
			}
			while (Position % 16 != 0L)
			{
				Position++;
			}
			for (int m = 0; m < arrayCount; m++)
			{
				EbxArray item5 = new EbxArray
				{
					Offset = ReadUInt(),
					Count = ReadUInt(),
					ClassRef = ReadInt()
				};
				arrays.Add(item5);
			}
			Position = stringsOffset + stringsLen;
			isValid = true;
		}

		public EbxReaderV2(Stream InStream, bool inPatched)
			: base(InStream, passthru: true)
		{
			InitialRead(InStream, inPatched);
		}

		public override void InternalReadObjects()
		{
			foreach (EbxInstance instance in instances)
			{
				Type type = TypeLibrary.GetType(classGuids[instance.ClassRef]);
				for (int i = 0; i < instance.Count; i++)
				{
					objects.Add(TypeLibrary.CreateObject(type));
					refCounts.Add(0);
				}
			}
			int num = 0;
			int num2 = 0;
			foreach (EbxInstance instance2 in instances)
			{
				for (int j = 0; j < instance2.Count; j++)
				{
					dynamic val = objects[num++];
					Type objType = val.GetType();
					EbxClass @class = GetClass(objType);
					while (Position % (long)@class.Alignment != 0L)
					{
						Position++;
					}
					Guid inGuid = Guid.Empty;
					if (instance2.IsExported)
					{
						inGuid = ReadGuid();
					}
					if (@class.Alignment != 4)
					{
						Position += 8L;
					}
					val.SetInstanceGuid(new AssetClassGuid(inGuid, num2++));
					this.ReadClass(@class, val, Position);
				}
			}
		}

		internal EbxClass GetClass(Type objType)
		{
			EbxClass? ebxClass = null;
			foreach (TypeInfoGuidAttribute customAttribute in objType.GetCustomAttributes<TypeInfoGuidAttribute>())
			{
				if (classGuids.Contains(customAttribute.Guid))
				{
					if (patched && patchStd != null)
					{
						ebxClass = patchStd.GetClass(customAttribute.Guid);
					}
					if (!ebxClass.HasValue)
					{
						ebxClass = std.GetClass(customAttribute.Guid);
					}
					break;
				}
			}
			return ebxClass.Value;
		}

		internal override PropertyInfo GetProperty(Type objType, EbxField field)
		{
			PropertyInfo[] properties = objType.GetProperties();
			var hashAttrbProps = properties.ToList().Where(x => x.GetCustomAttribute<HashAttribute>() != null).ToList();
			foreach (PropertyInfo propertyInfo in properties)
			{
				HashAttribute customAttribute = propertyInfo.GetCustomAttribute<HashAttribute>();
				if (customAttribute != null && customAttribute.Hash == (int)field.NameHash)
				{
					return propertyInfo;
				}
				// =============================
				EbxFieldMetaAttribute ebxFieldMetaAttribute = propertyInfo.GetCustomAttribute<EbxFieldMetaAttribute>();
				if (ebxFieldMetaAttribute != null && field.DebugType == EbxFieldType.Float32 && ebxFieldMetaAttribute.Offset == (int)field.DataOffset)
				{
					return propertyInfo;
				}
			}
			return null;
		}

		internal override EbxClass GetClass(EbxClass? classType, int index)
		{
			EbxClass? ebxClass = null;
			Guid? guid = null;
			var sIndex = (short)index;
			var additionalIndex = (classType.HasValue ? classType.Value.Index : 0);
			int index2 = sIndex + additionalIndex;
			guid = std.GetGuid(index2);

			ebxClass = std.GetClass(index2);

			EbxClass? ebxClassPatch = null;

			if (patchStd != null && patchStd.ClassCount - 1 > index2)
			{
				ebxClassPatch = patchStd.GetClass(index2);
				if(ebxClassPatch != null)
                {

                }
			}

			if (classType.Value.SecondSize == 1)
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
			if (ebxClass.HasValue && !string.IsNullOrEmpty(ebxClass.Value.Name))
			{
				//if(string.IsNullOrEmpty(ebxClass.Value.Name))
    //            {
				//	throw new Exception("Unable to find Ebx Class Name");
    //            }
				TypeLibrary.AddType(ebxClass.Value.Name, guid);
			}
			return ebxClass.Value;
		}

		internal override EbxField GetField(EbxClass classType, int index)
		{
			if (classType.SecondSize == 1)
			{
				return patchStd.GetField(index);
			}
			return std.GetField(index);
		}

		internal override object CreateObject(EbxClass classType)
		{
			if (classType.SecondSize == 1)
			{
				return TypeLibrary.CreateObject(patchStd.GetGuid(classType));
			}
			return TypeLibrary.CreateObject(std.GetGuid(classType));
		}

		internal object ReadClass(EbxClassMetaAttribute classMeta, object obj, Type objType, long startOffset)
		{
			if (obj == null)
			{
				Position += classMeta.Size;
				while (Position % (long)classMeta.Alignment != 0L)
				{
					Position++;
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
				Position = startOffset + customAttribute2.Offset;
				if (customAttribute2.Type == EbxFieldType.Array)
				{
					int index = ReadInt();
					EbxArray ebxArray = arrays[index];
					long position = Position;
					Position = arraysOffset + ebxArray.Offset;
					propertyInfo?.GetValue(obj).GetType().GetMethod("Clear")
						.Invoke(propertyInfo.GetValue(obj), new object[0]);
					for (int j = 0; j < ebxArray.Count; j++)
					{
						object obj2 = ReadField(customAttribute2.ArrayType, customAttribute2.BaseType, customAttribute != null);
						propertyInfo?.GetValue(obj).GetType().GetMethod("Add")
							.Invoke(propertyInfo.GetValue(obj), new object[1]
							{
								obj2
							});
					}
					if (Position > boxedValuesOffset)
					{
						boxedValuesOffset = Position;
					}
					Position = position;
				}
				else
				{
					object value = ReadField(customAttribute2.Type, propertyInfo.PropertyType, customAttribute != null);
					propertyInfo?.SetValue(obj, value);
				}
			}
			while (Position - startOffset != classMeta.Size)
			{
				Position++;
			}
			return null;
		}

		internal object ReadField(EbxFieldType type, Type baseType, bool dontRefCount = false)
		{
			switch (type)
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
				object obj = TypeLibrary.CreateObject(baseType);
				EbxClassMetaAttribute customAttribute = obj.GetType().GetCustomAttribute<EbxClassMetaAttribute>();
				while (Position % (long)customAttribute.Alignment != 0L)
				{
					Position++;
				}
				ReadClass(customAttribute, obj, obj.GetType(), Position);
				return obj;
			}
			case EbxFieldType.Enum:
				return ReadInt();
			case EbxFieldType.Pointer:
			{
				uint num = ReadUInt();
				if (num >> 31 == 1)
				{
					EbxImportReference externalRef = imports[(int)(num & int.MaxValue)];
					if (dontRefCount && !dependencies.Contains(externalRef.FileGuid))
					{
						dependencies.Add(externalRef.FileGuid);
					}
					return new PointerRef(externalRef);
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
			case EbxFieldType.DbObject:
				throw new InvalidDataException("DbObject");
			default:
				throw new InvalidDataException("Unknown");
			}
		}
	}
}
