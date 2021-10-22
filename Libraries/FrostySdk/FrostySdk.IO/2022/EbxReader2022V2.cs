using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostySdk.IO
{
	public class EbxReader2022V2 : EbxReader
	{
        public static IEbxSharedTypeDescriptor std => EbxReaderV2.std;

        public static IEbxSharedTypeDescriptor patchStd => EbxReaderV2.patchStd;

		private readonly List<Guid> classGuids = new List<Guid>();

		private readonly List<Guid> typeInfoGuids = new List<Guid>();

		private bool patched;

		private long payloadPosition;

		private List<uint> importOffsets;

		private List<uint> dataOffsets;

		private string rootType = string.Empty;

		public override string RootType
		{
			get
			{
				if (string.IsNullOrEmpty(rootType) && typeInfoGuids.Count > 0)
				{
					Type type = TypeLibrary.GetType(typeInfoGuids[0]);
					rootType = type?.Name ?? string.Empty;
				}
				return rootType;
			}
		}

		public string FileName { get; set; }

		public EbxReader2022V2(Stream ebxDataStream, bool inPatched)
			: base(ebxDataStream, passthru: true)
		{

			EbxReaderV2.InitialiseStd();
			
			patched = inPatched;
			ebxVersion = (EbxVersion)ReadUInt();
			
			LoadRiff();
			
		}

		private void LoadRiff()
		{
			uint chunkSize = ReadUInt();
			long chunkSizeRelativeToPosition = base.Position;
			FourCC chunkName = ReadUInt();
			
			chunkName = ReadUInt();
			
			chunkSize = ReadUInt();
			chunkSizeRelativeToPosition = base.Position;
			Pad(16);
			long payloadOffset = (payloadPosition = base.Position);
			base.Position = chunkSizeRelativeToPosition + chunkSize;
			Pad(2);
			chunkName = ReadUInt();
			
			chunkSize = ReadUInt();
			chunkSizeRelativeToPosition = base.Position;
			Guid partitionGuid = (fileGuid = ReadGuid());
			uint guidCount = ReadUInt();
			for (int i6 = 0; i6 < guidCount; i6++)
			{
				Guid guid = ReadGuid();
				classGuids.Add(guid);
			}
			uint signatureCount = ReadUInt();
			List<uint> signatures = new List<uint>((int)signatureCount);
			for (int i5 = 0; i5 < signatureCount; i5++)
			{
				uint ebxSignature = ReadUInt();
				signatures.Add(ebxSignature);
			}
			uint exportedInstancesCount = ReadUInt();
			exportedCount = (ushort)exportedInstancesCount;
			uint dataContainerCount = ReadUInt();
			List<uint> dataContainerOffsets = (this.dataOffsets = new List<uint>((int)dataContainerCount));
			for (int i4 = 0; i4 < dataContainerCount; i4++)
			{
				uint dataContainerOffset = ReadUInt();
				dataContainerOffsets.Add(dataContainerOffset);
			}
			uint pointerOffsetsCount = ReadUInt();
			List<uint> pointerOffsets = new List<uint>((int)pointerOffsetsCount);
			for (int i3 = 0; i3 < pointerOffsetsCount; i3++)
			{
				uint pointerOffset = ReadUInt();
				pointerOffsets.Add(pointerOffset);
			}
			uint resourceRefOffsetsCount = ReadUInt();
			List<uint> resourceRefOffsets = new List<uint>((int)resourceRefOffsetsCount);
			for (int i2 = 0; i2 < resourceRefOffsetsCount; i2++)
			{
				uint resourceRefOffset = ReadUInt();
				resourceRefOffsets.Add(resourceRefOffset);
			}
			uint importsCount = ReadUInt();
			for (int n = 0; n < importsCount; n++)
			{
				EbxImportReference ebxImportReference2 = default(EbxImportReference);
				ebxImportReference2.FileGuid = ReadGuid();
				ebxImportReference2.ClassGuid = ReadGuid();
				EbxImportReference ebxImportReference = ebxImportReference2;
				imports.Add(ebxImportReference);
				if (!dependencies.Contains(ebxImportReference.FileGuid))
				{
					dependencies.Add(ebxImportReference.FileGuid);
				}
			}
			uint importOffsetsCount = ReadUInt();
			List<uint> importOffsets = (this.importOffsets = new List<uint>((int)importOffsetsCount));
			for (int m = 0; m < importOffsetsCount; m++)
			{
				uint importOffset = ReadUInt();
				importOffsets.Add(importOffset);
			}
			uint typeInfoOffsetsCount = ReadUInt();
			List<uint> typeInfoOffsets = new List<uint>((int)typeInfoOffsetsCount);
			for (int l = 0; l < typeInfoOffsetsCount; l++)
			{
				uint typeInfoOffset = ReadUInt();
				typeInfoOffsets.Add(typeInfoOffset);
			}
			arraysOffset = ReadUInt();
			var nameOfEbxItemMinus32 = ReadUInt();
			uint stringTableOffset = ReadUInt();
			stringsOffset = stringTableOffset + payloadOffset;

			// -------
			// EBXX // 
			chunkName = ReadUInt();
			
			chunkSize = ReadUInt();
			chunkSizeRelativeToPosition = base.Position;
			uint arrayCount = ReadUInt();
			uint boxedValueCount = ReadUInt();
			for (int k = 0; k < arrayCount; k++)
			{
				uint offset = ReadUInt();
				uint internalCount = ReadUInt();
				uint offset2 = ReadUInt();
				ushort arrType = ReadUShort();
				ushort typeClassRef = ReadUShort();
				arrays.Add(new EbxArray
				{
					ClassRef = typeClassRef,
					Count = internalCount,
					Offset = offset,
					TypeFlags = arrType,
					PathDepth = offset2
				});
			}
			for (int j = 0; j < boxedValueCount; j++)
			{
				var bvr_unk1 = ReadUInt();
				var bvr_unk2 = ReadUInt();
				var bvr_unk3 = ReadUInt();
				var bvr_unk4 = ReadUShort();
				var bvr_unk5 = ReadUShort();
				boxedValueRefs.Add(new BoxedValueRef { 
					 
				});
			}
			foreach (uint dataContainerOffset2 in dataContainerOffsets)
			{
				base.Position = payloadOffset + dataContainerOffset2;
				uint typeInfoIndex = ReadUInt();
				instances.Add(new EbxInstance
				{
					ClassRef = (ushort)typeInfoIndex,
					Count = 1,
					IsExported = (instances.Count < exportedInstancesCount)
				});
			}
			Span<byte> classGuidBytes = stackalloc byte[20];
			for (int i = 0; i < classGuids.Count && i < signatures.Count; i++)
			{
				if (!classGuids[i].TryWriteBytes(classGuidBytes))
				{
					throw new InvalidOperationException("Couldn't write class GUID to span.");
				}
				Span<byte> span = classGuidBytes;
				BinaryPrimitives.WriteUInt32LittleEndian(span[16..], signatures[i]);
				List<Guid> list = typeInfoGuids;
				span = classGuidBytes;
				list.Add(new Guid(span[4..]));
			}

			base.Position = nameOfEbxItemMinus32 + payloadOffset;
			FileName = ReadNullTerminatedString();
			base.Position = chunkSize;

			base.Position = payloadOffset;


			isValid = true;
		}

		public override void InternalReadObjects()
		{
			for(var iInstance = 0; iInstance < instances.Count; iInstance++)
			{
				EbxInstance ebxInstance = instances[iInstance];
				Type type = TypeLibrary.GetType(typeInfoGuids[ebxInstance.ClassRef]);
				for (int i = 0; i < ebxInstance.Count; i++)
				{
					var newObj = TypeLibrary.CreateObject(type);
					objects.Add(newObj);
					// quick and dirty
					ebxInstance.InstanceObject = newObj;
					refCounts.Add(0);
				}
			}
			int num = 0;
			int num2 = 0;
			for (var iInstance = 0; iInstance < instances.Count; iInstance++)
			{
				EbxInstance ebxInstance = instances[iInstance];

				for (int j = 0; j < ebxInstance.Count; j++)
				{
					dynamic obj = objects[num++];
					Type objType = obj.GetType();
					EbxClass @class = GetClass(objType);
					Pad(@class.Alignment);
					Guid inGuid = Guid.Empty;
					if (ebxInstance.IsExported)
					{
						inGuid = ReadGuid();
					}
					//long classPosition = base.Position;
					//if (magic == EbxVersion.Riff)
					//{
						//ReadInt();
						//base.Position += 12L;
					//}
					//if (@class.Alignment != 4)
					//{
					//	base.Position += 8L;
					//}
					obj.SetInstanceGuid(new AssetClassGuid(inGuid, num2++));

					if(objType.Name.Contains("FloatCurve", StringComparison.OrdinalIgnoreCase))
                    {

                    }

					//long startOffset = ((magic == EbxVersion.Riff) ? (base.Position - 24) : (base.Position - 8));
					//long startOffset = base.Position - 24;
					long startOffset = dataOffsets[iInstance] + 32;
					Position = startOffset;
					Pad(@class.Alignment);

					this.ReadClass(@class, obj, startOffset);
					ebxInstance.InstanceObject = obj;
					//if (magic == EbxVersion.Riff)
					//{
						//base.Position = classPosition + @class.Size;
					//}
				}
			}
		}

		public override object ReadClass(EbxClass classType, object obj, long startOffset)
		{
			Position = startOffset;
			Pad(classType.Alignment);
			startOffset = base.Position;

			Dictionary<EbxField, PropertyInfo> FieldProperties = new Dictionary<EbxField, PropertyInfo>();
			Type type = obj.GetType();
			for (int i = 0; i < classType.FieldCount; i++)
			{
				EbxField field = GetField(classType, classType.FieldIndex + i);
				PropertyInfo property = GetProperty(type, field);
				FieldProperties.Add(field, property);
			}
			FieldProperties = FieldProperties.OrderBy(x => x.Key.DataOffset).ToDictionary(x=> x.Key, x => x.Value);

			//for (int i = 0; i < classType.FieldCount; i++)
			foreach(var fp in FieldProperties)
			{
				EbxField field = fp.Key;
				PropertyInfo property = fp.Value;
				//EbxField field = GetField(classType, classType.FieldIndex + i);
				//PropertyInfo property = GetProperty(type, field);
				//if (property != null)
				//{
				//	var propName = property.Name;
				//	if (propName.Contains("ATTR_AnimationPlaybackTimeRatioScaleByHeightDribbling"))
				//	{

				//	}
				//}
				if(field.DebugType == EbxFieldType.CString)
                {

                }


				IsReferenceAttribute isReferenceAttribute = ((property != null) ? property.GetCustomAttribute<IsReferenceAttribute>() : null);
				if (field.DebugType == EbxFieldType.Inherited)
				{
					ReadClass(GetClass(classType, field.ClassRef), obj, startOffset);
					continue;
				}
				
					base.Position = field.DataOffset + startOffset;
				if (IsFieldInClassAnArray(classType, field))
				{
					ReadArray(obj, property, classType, field, isReferenceAttribute != null);
					continue;
				}
				object value = ReadField(classType, field.DebugType, field.ClassRef, isReferenceAttribute != null);
				if (property != null)
				{
					if (value != null)
                    {
						if(field.InternalType == EbxFieldType.Float32 && value.ToString().Contains("E", StringComparison.OrdinalIgnoreCase))
                        {
							base.Position = field.DataOffset + startOffset;
							value = Convert.ToSingle(ReadField(classType, EbxFieldType.Int32, field.ClassRef, isReferenceAttribute != null));
						}
						else if(field.InternalType == EbxFieldType.Enum 
							&& ((int)value > 100 || (int)value < 0)) 
                        {
							//throw new ArgumentOutOfRangeException("Incorrect Enum type used");
							base.Position -= 8;
							value = ReadField(classType, field.DebugType, field.ClassRef, isReferenceAttribute != null);
						}
					}
					//try
					//{
						property.SetValue(obj, value);
					//}
					//catch (Exception)
					//{
					//}
				}
			}
			//if (magic == EbxVersion.Riff)
			//{
				base.Position = startOffset + classType.Size;
			//}
			Pad(classType.Alignment);
			return null;
		}

		public EbxClass GetClass(Type objType)
		{
			EbxClass? ebxClass = null;
			foreach (TypeInfoGuidAttribute typeInfoGuidAttribute in objType.GetCustomAttributes(typeof(TypeInfoGuidAttribute), inherit: true).Cast<TypeInfoGuidAttribute>())
			{
				if (typeInfoGuids.Contains(typeInfoGuidAttribute.Guid))
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
			Guid? guid;
			EbxClass? ebxClass;
			if (!classType.HasValue)
			{
				guid = classGuids[index];
				ebxClass = patchStd?.GetClass(guid.Value) ?? std.GetClass(guid.Value);
			}
			else
			{
				int index2 = ((ebxVersion != EbxVersion.Riff) ? ((short)index + (classType?.Index ?? 0)) : index);
				guid = std.GetGuid(index2);
				if (classType.Value.SecondSize == 1)
				{
					guid = patchStd.GetGuid(index2);
					ebxClass = patchStd.GetClass(index2) ?? std.GetClass(guid.Value);
				}
				else
				{
					ebxClass = std.GetClass(index2);
				}
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
				//return patchStd.GetField(index);
			}
			return std.GetField(index).Value;
		}

		public override object CreateObject(EbxClass classType)
		{
			if (classType.SecondSize == 1)
			{
				//return TypeLibrary.CreateObject(patchStd.GetGuid(classType));
			}
			return TypeLibrary.CreateObject(std.GetGuid(classType).Value);
		}

		public Type GetType(EbxClass classType)
		{
			//return TypeLibrary.GetType((classType.SecondSize == 1) ? patchStd.GetGuid(classType) : std.GetGuid(classType));
			return TypeLibrary.GetType(std.GetGuid(classType).Value);
		}

		public object ReadClass(EbxClassMetaAttribute classMeta, object obj, Type objType, long startOffset)
		{
			if (obj == null)
			{
				base.Position += classMeta.Size;
				Pad(classMeta.Alignment);
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
					int index = ReadInt();
					long position2 = base.Position;
					EbxArray ebxArray;
					//if (magic == EbxVersion.Riff)
					//{
						long offsetPosition = base.Position - 4;
						long offsetRelativeToPayload = offsetPosition - payloadPosition + index;
						ebxArray = arrays.Find((EbxArray a) => a.Offset == offsetRelativeToPayload);
						base.Position = ebxArray.Offset + payloadPosition;
					//}
					//else
					//{
					//	ebxArray = arrays[index];
					//	base.Position = arraysOffset + ebxArray.Offset;
					//}
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
			long amountRead = base.Position - (startOffset + 16);
			if (amountRead > classMeta.Size)
			{
			}
			while (amountRead < classMeta.Size)
			{
				base.Position++;
				amountRead = base.Position - (startOffset + 16);
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
						Pad(customAttribute.Alignment);
						ReadClass(customAttribute, obj, obj.GetType(), base.Position);
						return obj;
					}
				case EbxFieldType.Pointer:
					{
						int num = ReadInt();
						if (num == 0)
						{
							return default(PointerRef);
						}
						if (num - 1 >= 0 && num - 1 < objects.Count)
						{
							if (!dontRefCount)
							{
								refCounts[num - 1]++;
							}
							return new PointerRef(objects[num - 1]);
						}
						long offset = base.Position - 4 + num;
						offset -= payloadPosition;
						return new PointerRef(importOffsets.Find((uint o) => o == offset));
						
					}
				case EbxFieldType.String:
					return ReadSizedString(32);
				case EbxFieldType.CString:
					return ReadCString(ReadUInt(), false);
				case EbxFieldType.Enum:
					return ReadInt();
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
					return ReadInt();
				case EbxFieldType.UInt32:
					return ReadUInt();
				case EbxFieldType.UInt64:
					return ReadULong();
				case EbxFieldType.Int64:
					return ReadLong();
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
					throw new InvalidDataException($"Unknown field type {type}");
			}
		}

		public override object ReadField(EbxClass? parentClass, EbxFieldType fieldType, ushort fieldClassRef, bool dontRefCount = false)
		{
			switch (fieldType) 
			{
				case EbxFieldType.Pointer:
					int num = ReadInt();
					if (num == 0)
					{
						return default(PointerRef);
					}
					if ((num & 1) == 1)
					{
						return new PointerRef(imports[num >> 1]);
					}
					long offset = base.Position - 4 + num - payloadPosition;
					int dc = dataOffsets.IndexOf((uint)offset);
					if (dc == -1)
					{
						return default(PointerRef);
					}
					if (!dontRefCount)
					{
						refCounts[dc]++;
					}
					return new PointerRef(objects[dc]);
				case EbxFieldType.Struct:
					var startPosition = base.Position;
					EbxClass @class = GetClass(parentClass, fieldClassRef);
					//Pad(@class.Alignment);
					object obj = CreateObject(@class);
					ReadClass(@class, obj, base.Position);
					base.Position = startPosition + @class.Size;
					return obj;
				case EbxFieldType.CString:
					var cPos = ReadUInt();
					return ReadCString(cPos, false);
			}
			return base.ReadField(parentClass, fieldType, fieldClassRef, dontRefCount);
		}

		protected override void ReadArray(object obj, PropertyInfo property, EbxClass classType, EbxField field, bool isReference)
		{
			
			long readArrayPosition = base.Position;
			int aOffset = ReadInt();

			var ebxArray = arrays.FirstOrDefault(x => x.Offset == aOffset + (readArrayPosition - payloadPosition));

			base.Position += aOffset - 8;
			//base.Position -= 4L;
			uint arrayCount = ReadUInt();
			var fieldOffset = field.DataOffset;
			for (int i = 0; i < arrayCount; i++)
			{
				object obj2 = ReadField(classType, field.DebugType, field.ClassRef, isReference);
				if (property != null)
				{
					property.GetValue(obj)!.GetType().GetMethod("Add")!.Invoke(property.GetValue(obj), new object[1] { obj2 });
				}
				//EbxFieldType debugType = field.DebugType;
				//if (debugType == EbxFieldType.Pointer || debugType == EbxFieldType.CString)
				//{
				//	Pad(8);
				//}
			}
			//base.Position = readArrayPosition;
		}
	}
}