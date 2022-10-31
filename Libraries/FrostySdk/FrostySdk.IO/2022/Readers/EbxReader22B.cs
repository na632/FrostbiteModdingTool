using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.FrostySdk.IO;
using FrostySdk.Managers;

namespace FrostySdk.IO._2022.Readers
{
	public class EbxReader22B : EbxReader22A
	{
		internal const int EbxExternalReferenceMask = 1;

		internal static EbxSharedTypeDescriptorV2 std { get; private set; }

		internal static EbxSharedTypeDescriptorV2 patchStd { get; private set; }

		private readonly List<Guid> classGuids = new List<Guid>();

		private readonly List<Guid> typeInfoGuids = new List<Guid>();

		private bool patched;

		private Guid unkGuid;

		private long payloadPosition;

		private long arrayOffset;

		private List<uint> importOffsets;

		private List<uint> dataContainerOffsets;

		public override string RootType
		{
			get
			{


				if (this.typeInfoGuids.Count > 0)
				{
					Type type = TypeLibrary.GetType(this.typeInfoGuids[0]);

					return type?.Name ?? "UnknownType";
				}
				if (base.instances.Count == 0)
				{
					return string.Empty;
				}
				if (this.classGuids.Count <= base.instances[0].ClassRef)
				{
					return string.Empty;
				}
				return TypeLibrary.GetType(this.classGuids[base.instances[0].ClassRef])?.Name ?? string.Empty;
			}
		}

		public EbxReader22B(Stream ebxDataStream, bool inPatched)
			: base(ebxDataStream, passthru: true)
		{
			if (ebxDataStream == null)
			{
				throw new ArgumentNullException("ebxDataStream");
			}
			if (EbxReader22B.std == null)
			{
				EbxReader22B.std = new EbxSharedTypeDescriptorV2("SharedTypeDescriptors.ebx", false);
				if (FileSystem.Instance.HasFileInMemoryFs("SharedTypeDescriptors_patch.ebx"))
				{
					EbxReader22B.patchStd = new EbxSharedTypeDescriptorV2("SharedTypeDescriptors_patch.ebx", true);
				}
			}
			this.patched = inPatched;
			base.magic = (EbxVersion)base.ReadUInt32LittleEndian();
			//if (base.magic == EbxVersion.Riff)
			//{
			try
			{
				this.LoadRiffEbx();
				this.isValid = true;
				return;
			}
			catch (EndOfStreamException)
			{
				throw;
			}
			//}
			//EbxVersion ebxVersion = base.magic;
			//if (ebxVersion != EbxVersion.Version2 && ebxVersion != EbxVersion.Version4)
			//{
			//	return;
			//}
			//this.importOffsets = new List<uint>();
			//base.stringsOffset = base.ReadUInt32LittleEndian();
			//base.stringsAndDataLen = base.ReadUInt32LittleEndian();
			//base.guidCount = base.ReadUInt32LittleEndian();
			//base.instanceCount = base.ReadUInt16LittleEndian();
			//base.exportedCount = base.ReadUInt16LittleEndian();
			//base.uniqueClassCount = base.ReadUInt16LittleEndian();
			//base.classTypeCount = base.ReadUInt16LittleEndian();
			//base.fieldTypeCount = base.ReadUInt16LittleEndian();
			//base.typeNamesLen = base.ReadUInt16LittleEndian();
			//base.stringsLen = base.ReadUInt32LittleEndian();
			//base.arrayCount = base.ReadUInt32LittleEndian();
			//base.dataLen = base.ReadUInt32LittleEndian();
			//base.arraysOffset = base.stringsOffset + base.stringsLen + base.dataLen;
			//base.fileGuid = base.ReadGuid();
			//base.boxedValuesCount = base.ReadUInt32LittleEndian();
			//base.boxedValuesOffset = base.ReadUInt32LittleEndian();
			//base.boxedValuesOffset += base.stringsOffset + base.stringsLen;
			//for (int num = 0; num < base.guidCount; num++)
			//{
			//	EbxImportReference ebxImportReference = new EbxImportReference
			//	{
			//		FileGuid = base.ReadGuid(),
			//		ClassGuid = base.ReadGuid()
			//	};
			//	base.imports.Add(ebxImportReference);
			//	if (!base.dependencies.Contains(ebxImportReference.FileGuid))
			//	{
			//		base.dependencies.Add(ebxImportReference.FileGuid);
			//	}
			//}
			//Dictionary<int, string> dictionary = new Dictionary<int, string>();
			//long position = base.Position;
			//while (base.Position - position < base.typeNamesLen)
			//{
			//	string text = base.ReadNullTerminatedString();
			//	int key = base.HashString(text);
			//	dictionary.TryAdd(key, text);
			//}
			//for (int j = 0; j < base.fieldTypeCount; j++)
			//{
			//	EbxField item = default(EbxField);
			//	int key2 = base.ReadInt32LittleEndian();
			//	item.Type = (base.magic == EbxVersion.Version2) ? base.ReadUInt16LittleEndian() : ((ushort)(base.ReadUInt16LittleEndian() >> 1));
			//	item.ClassRef = base.ReadUInt16LittleEndian();
			//	item.DataOffset = base.ReadUInt32LittleEndian();
			//	item.SecondOffset = base.ReadUInt32LittleEndian();
			//	item.Name = dictionary[key2];
			//	base.fieldTypes.Add(item);
			//}
			//for (int k = 0; k < base.classTypeCount; k++)
			//{
			//	Guid item2 = base.ReadGuid();
			//	this.classGuids.Add(item2);
			//	this.typeInfoGuids.Add(item2);
			//}
			//ushort num2 = base.exportedCount;
			//for (int l = 0; l < base.instanceCount; l++)
			//{
			//	EbxInstance item3 = new EbxInstance
			//	{
			//		ClassRef = base.ReadUInt16LittleEndian(),
			//		Count = base.ReadUInt16LittleEndian()
			//	};
			//	if (num2 != 0)
			//	{
			//		item3.IsExported = true;
			//		num2 = (ushort)(num2 - 1);
			//	}
			//	base.instances.Add(item3);
			//}
			//base.Pad(16);
			//for (int i = 0; i < base.arrayCount; i++)
			//{
			//	EbxArray item4 = new EbxArray
			//	{
			//		Offset = base.ReadUInt32LittleEndian(),
			//		Count = base.ReadUInt32LittleEndian(),
			//		ClassRef = base.ReadInt32LittleEndian()
			//	};
			//	base.arrays.Add(item4);
			//}
			//base.Pad(16);
			//for (int m = 0; m < base.boxedValuesCount; m++)
			//{
			//	EbxBoxedValue item5 = new EbxBoxedValue
			//	{
			//		Offset = base.ReadUInt32LittleEndian(),
			//		ClassRef = base.ReadUInt16LittleEndian(),
			//		Type = base.ReadUInt16LittleEndian()
			//	};
			//	base.boxedValues.Add(item5);
			//}
			//base.Position = base.stringsOffset + base.stringsLen;
			//base.isValid = true;
		}

		private void LoadRiffEbx()
		{
			uint chunkSize = base.ReadUInt32LittleEndian();
			long chunkSizeRelativeToPosition = base.Position;
			uint chunkName = base.ReadUInt32LittleEndian();
			if (chunkName != 5784133 && chunkName != 1398293061)
			{
				throw new InvalidDataException("Incorrectly formatted RIFF detected.");
			}
			chunkName = base.ReadUInt32LittleEndian();
			if (chunkName != 1146634821)
			{
				throw new InvalidDataException("Incorrectly formatted RIFF detected. Expected EBXD.");
			}
			chunkSize = base.ReadUInt32LittleEndian();
			chunkSizeRelativeToPosition = base.Position;
			base.Pad(16);
			long payloadOffset = (this.payloadPosition = base.Position);
			base.Position = chunkSizeRelativeToPosition + chunkSize;
			_ = base.Position;
			base.Pad(2);
			chunkName = base.ReadUInt32LittleEndian();
			if (chunkName != 1481197125)
			{
				throw new InvalidDataException("Incorrectly formatted RIFF detected. Expected EFIX.");
            }
            chunkSize = base.ReadUInt32LittleEndian();
			chunkSizeRelativeToPosition = base.Position;
			Guid partitionGuid = (base.fileGuid = base.ReadGuid());
			uint guidCount = base.ReadUInt32LittleEndian();
			for (int i6 = 0; i6 < guidCount; i6++)
			{
				Guid guid = base.ReadGuid();
				this.classGuids.Add(guid);
			}
			uint signatureCount = base.ReadUInt32LittleEndian();
			List<uint> signatures = new List<uint>((int)signatureCount);
			for (int i5 = 0; i5 < signatureCount; i5++)
			{
				uint ebxSignature = base.ReadUInt32LittleEndian();
				signatures.Add(ebxSignature);
			}
			uint exportedInstancesCount = base.ReadUInt32LittleEndian();
			base.exportedCount = (ushort)exportedInstancesCount;
			uint dataContainerCount = base.ReadUInt32LittleEndian();
			List<uint> dataContainerOffsets = (this.dataContainerOffsets = new List<uint>((int)dataContainerCount));
			for (int i4 = 0; i4 < dataContainerCount; i4++)
			{
				uint dataContainerOffset = base.ReadUInt32LittleEndian();
				dataContainerOffsets.Add(dataContainerOffset);
			}
			uint pointerOffsetsCount = base.ReadUInt32LittleEndian();
			List<uint> pointerOffsets = new List<uint>((int)pointerOffsetsCount);
			for (int i3 = 0; i3 < pointerOffsetsCount; i3++)
			{
				uint pointerOffset = base.ReadUInt32LittleEndian();
				pointerOffsets.Add(pointerOffset);
			}
			uint resourceRefOffsetsCount = base.ReadUInt32LittleEndian();
			List<uint> resourceRefOffsets = new List<uint>((int)resourceRefOffsetsCount);
			for (int i2 = 0; i2 < resourceRefOffsetsCount; i2++)
			{
				uint resourceRefOffset = base.ReadUInt32LittleEndian();
				resourceRefOffsets.Add(resourceRefOffset);
			}
			uint importsCount = base.ReadUInt32LittleEndian();
			for (int n = 0; n < importsCount; n++)
			{
				EbxImportReference ebxImportReference2 = default(EbxImportReference);
				ebxImportReference2.FileGuid = base.ReadGuid();
				ebxImportReference2.ClassGuid = base.ReadGuid();
				EbxImportReference ebxImportReference = ebxImportReference2;
				base.imports.Add(ebxImportReference);
				if (!base.dependencies.Contains(ebxImportReference.FileGuid))
				{
					base.dependencies.Add(ebxImportReference.FileGuid);
				}
			}
			uint importOffsetsCount = base.ReadUInt32LittleEndian();
			List<uint> importOffsets = (this.importOffsets = new List<uint>((int)importOffsetsCount));
			for (int m = 0; m < importOffsetsCount; m++)
			{
				uint importOffset = base.ReadUInt32LittleEndian();
				importOffsets.Add(importOffset);
			}
			uint typeInfoOffsetsCount = base.ReadUInt32LittleEndian();
			List<uint> typeInfoOffsets = new List<uint>((int)typeInfoOffsetsCount);
			for (int l = 0; l < typeInfoOffsetsCount; l++)
			{
				uint typeInfoOffset = base.ReadUInt32LittleEndian();
				typeInfoOffsets.Add(typeInfoOffset);
			}
			uint arrayOffset = base.ReadUInt32LittleEndian();
			this.arrayOffset = arrayOffset;
			base.ReadUInt32LittleEndian();
			uint stringTableOffset = base.ReadUInt32LittleEndian();
			base.stringsOffset = stringTableOffset + payloadOffset;
			chunkName = base.ReadUInt32LittleEndian();
			if (chunkName == 0 && base.ReadUInt32LittleEndian() != 1482179141)
				base.Position -= 4;
			//if (chunkName != 1482179141)
			//{
			//	DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 1);
			//	defaultInterpolatedStringHandler.AppendLiteral("Expected \"EBXX\" four-CC, but instead got ");
			//	defaultInterpolatedStringHandler.AppendFormatted(chunkName);
			//	defaultInterpolatedStringHandler.AppendLiteral(".");
			//	throw new InvalidDataException(defaultInterpolatedStringHandler.ToStringAndClear());
			//}
			chunkSize = base.ReadUInt32LittleEndian();
			if (chunkSize == 1482179141)
                chunkSize = base.ReadUInt32LittleEndian();

            chunkSizeRelativeToPosition = base.Position;
			uint arrayCount = base.ReadUInt32LittleEndian();
			uint boxedValueCount = base.ReadUInt32LittleEndian();
			for (int k = 0; k < arrayCount; k++)
			{
				uint offset = base.ReadUInt32LittleEndian();
				uint elementCount = base.ReadUInt32LittleEndian();
				uint pathDepth = base.ReadUInt32LittleEndian();
				ushort typeFlags = base.ReadUInt16LittleEndian();
				ushort typeId = base.ReadUInt16LittleEndian();
				base.arrays.Add(new EbxArray
				{
					ClassRef = typeId,
					Count = elementCount,
					Offset = offset,
					TypeFlags = typeFlags,
					PathDepth = pathDepth
				});
			}
			for (int j = 0; j < boxedValueCount; j++)
			{
				//base.ReadUInt32LittleEndian();
				//base.ReadUInt32LittleEndian();
				//base.ReadUInt32LittleEndian();
				//base.ReadUInt16LittleEndian();
				//base.ReadUInt16LittleEndian();
                uint offset = ReadUInt();
                uint count = ReadUInt();
                uint hash = ReadUInt();
                ushort type = ReadUShort();
                ushort classRef = ReadUShort();

                boxedValues.Add
                (
                    new EbxBoxedValue
                    {
                        Offset = offset,
                        Type = type,
                        ClassRef = classRef
                    }
                );
            }
			_ = base.Position;
			_ = base.Length;
			foreach (uint dataContainerOffset2 in dataContainerOffsets)
			{
				base.Position = payloadOffset + dataContainerOffset2;
				uint typeInfoIndex = base.ReadUInt32LittleEndian();
				base.instances.Add(new EbxInstance
				{
					ClassRef = (ushort)typeInfoIndex,
					Count = 1,
					IsExported = (base.instances.Count < exportedInstancesCount)
				});
			}
			_ = this.classGuids.Count;
			_ = signatures.Count;
			Span<byte> classGuidBytes = stackalloc byte[20];
			for (int i = 0; i < this.classGuids.Count && i < signatures.Count; i++)
			{
				if (!this.classGuids[i].TryWriteBytes(classGuidBytes))
				{
					throw new InvalidOperationException("Couldn't write class GUID to span.");
				}
				_ = signatures.Count;
				Span<byte> span = classGuidBytes;
				int length = span.Length;
				int num = 16;
				int length2 = length - num;
				BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(num, length2), signatures[i]);
				List<Guid> list = this.typeInfoGuids;
				span = classGuidBytes;
				int length3 = span.Length;
				length2 = 4;
				num = length3 - length2;
				list.Add(new Guid(span.Slice(length2, num)));
			}
			base.Position = payloadOffset;
			base.isValid = true;


#if DEBUG
            //if (RootType.Contains("gp_"))
            //if (RootType.Contains("Hotspot"))
            //if (RootType.Contains("movement"))
            //         {
            //	Position = 0;
            //	var fsDump = new FileStream($"ebx.{RootType}.read.22.dat", FileMode.OpenOrCreate);
            //	base.stream.CopyTo(fsDump);
            //	fsDump.Close();
            //	fsDump.Dispose();
            //	Position = payloadOffset;
            //}

            if (RootType.Contains("Mesh", StringComparison.OrdinalIgnoreCase))
            {
                Position = 0;
                var fsDump = new FileStream($"ebx.{RootType}.read.22.dat", FileMode.OpenOrCreate);
                base.stream.CopyTo(fsDump);
                fsDump.Close();
                fsDump.Dispose();
                Position = payloadOffset;
            }
#endif
		}

        public override EbxAsset ReadAsset(EbxAssetEntry entry = null)
        {
            return ReadAsset<EbxAsset>();
        }

        public override T ReadAsset<T>()
		{
			T val = new T();
			this.InternalReadObjects();
			val.fileGuid = base.fileGuid;
			val.objects = base.objects;
			val.dependencies = base.dependencies;
			val.refCounts = base.refCounts;
			val.arrays = base.arrays;
			return val;
		}

		public override void InternalReadObjects()
		{
			foreach (EbxInstance ebxInstance in base.instances)
			{
				Type type = TypeLibrary.GetType(this.typeInfoGuids[ebxInstance.ClassRef]);
				for (int i = 0; i < ebxInstance.Count; i++)
				{
					base.objects.Add(TypeLibrary.CreateObject(type));
					base.refCounts.Add(0);
				}
			}
			int num = 0;
			int num2 = 0;
			foreach (EbxInstance ebxInstance2 in base.instances)
			{
				for (int j = 0; j < ebxInstance2.Count; j++)
				{
					dynamic obj = base.objects[num++];
					Type objType = obj.GetType();
					EbxClass @class = this.GetClass(objType);
					base.Pad(@class.Alignment);
					Guid inGuid = Guid.Empty;
					if (ebxInstance2.IsExported)
					{
						inGuid = base.ReadGuid();
					}
					long classPosition = base.Position;
					if (base.magic == EbxVersion.Riff)
					{
						base.ReadInt32LittleEndian();
						base.Position += 12L;
					}
					if (@class.Alignment != 4)
					{
						base.Position += 8L;
					}
					obj.SetInstanceGuid(new AssetClassGuid(inGuid, num2++));
					long startOffset = ((base.magic == EbxVersion.Riff) ? (base.Position - 24) : (base.Position - 8));
					this.ReadClass(@class, obj, startOffset);
					base.Position = classPosition + @class.Size;
				}
			}
		}

		public EbxClass GetClass(Type objType)
		{
			EbxClass? ebxClass = null;
			var nameHashAttribute = objType.GetCustomAttribute<HashAttribute>();
			var ebxclassmeta = objType.GetCustomAttribute<EbxClassMetaAttribute>();
			if (nameHashAttribute != null && ebxclassmeta != null)
			{
				var nHClass = EbxReader22B.patchStd.Classes
						  .Union(EbxReader22B.std.Classes).FirstOrDefault(x => x.HasValue && x.Value.NameHash == nameHashAttribute.Hash);
				if(nHClass.HasValue)
					return nHClass.Value;
			}

			foreach (TypeInfoGuidAttribute typeInfoGuidAttribute in objType.GetCustomAttributes(typeof(TypeInfoGuidAttribute), inherit: true).Cast<TypeInfoGuidAttribute>())
			{
				if (this.typeInfoGuids.Contains(typeInfoGuidAttribute.Guid))
				{
					//if (this.patched && EbxReader22B.patchStd != null)
					if (EbxReader22B.patchStd != null)
					{
						ebxClass = EbxReader22B.patchStd.GetClass(typeInfoGuidAttribute.Guid);
					}
					if (!ebxClass.HasValue)
					{
						ebxClass = EbxReader22B.std.GetClass(typeInfoGuidAttribute.Guid);
					}
					break;
				}
			}
			return ebxClass.Value;
		}

		public override PropertyInfo GetProperty(Type objType, EbxField field)
		{
			PropertyInfo[] properties = objType.GetProperties();
			PropertyInfo propertyInfo = properties.SingleOrDefault(x =>
				x.GetCustomAttribute<HashAttribute>() != null
				&& (uint)x.GetCustomAttribute<HashAttribute>().Hash == (uint)field.NameHash);
			//if(propertyInfo == null)
   //         {
			//	foreach(var property in properties.Where(x => x.GetCustomAttribute<HashAttribute>() != null))
   //             {
			//		Debug.WriteLine(property.Name + " " + property.GetCustomAttribute<HashAttribute>().Hash + " v " + field.NameHash);
			//	}

			//}
			return propertyInfo;
		}

		public override EbxClass GetClass(EbxClass? classType, int index)
		{
			Guid? guid;
			EbxClass? ebxClass;
			if (!classType.HasValue)
			{
				guid = this.classGuids[index];
				ebxClass = EbxReader22B.patchStd?.GetClass(guid.Value) ?? EbxReader22B.std.GetClass(guid.Value);
			}
			else
			{
				int index2 = ((base.magic != EbxVersion.Riff) ? ((short)index + (classType?.Index ?? 0)) : index);
				guid = EbxReader22B.std.GetGuid(index2);
				if (classType.Value.SecondSize >= 1)
				{
					guid = EbxReader22B.patchStd.GetGuid(index2);
					ebxClass = EbxReader22B.patchStd.GetClass(index2) ?? EbxReader22B.std.GetClass(guid.Value);
				}
				else
				{
					ebxClass = EbxReader22B.std.GetClass(index2);
				}
			}
			if (ebxClass.HasValue)
			{
				TypeLibrary.AddType(ebxClass.Value.Name, guid);
			}
			return ebxClass.HasValue ? ebxClass.Value : default(EbxClass);
		}

		public override EbxField GetField(EbxClass classType, int index)
		{
			if (classType.SecondSize >= 1)
			{
				return EbxReader22B.patchStd.GetField(index).Value;
			}
			return EbxReader22B.std.GetField(index).Value;
		}

		public override object CreateObject(EbxClass classType)
		{
			if (classType.SecondSize == 1)
			{
				return TypeLibrary.CreateObject(EbxReader22B.patchStd.GetGuid(classType).Value);
			}
			return TypeLibrary.CreateObject(EbxReader22B.std.GetGuid(classType).Value);
		}

		public override Type GetType(EbxClass classType)
		{
			return TypeLibrary.GetType((classType.SecondSize == 1) ? EbxReader22B.patchStd.GetGuid(classType).Value : EbxReader22B.std.GetGuid(classType).Value);
		}

		public object ReadClass(EbxClassMetaAttribute classMeta, object obj, Type objType, long startOffset)
		{
			if (obj == null)
			{
				base.Position += classMeta.Size;
				base.Pad(classMeta.Alignment);
				return null;
			}
			if (objType.BaseType != typeof(object))
			{
				this.ReadClass(objType.BaseType.GetCustomAttribute<EbxClassMetaAttribute>(), obj, objType.BaseType, startOffset);
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
					int index = base.ReadInt32LittleEndian();
					long position2 = base.Position;
					EbxArray ebxArray;
					if (base.magic == EbxVersion.Riff)
					{
						long offsetPosition = base.Position - 4;
						long offsetRelativeToPayload = offsetPosition - this.payloadPosition + index;
						ebxArray = base.arrays.Find((EbxArray a) => a.Offset == offsetRelativeToPayload);
						base.Position = ebxArray.Offset + this.payloadPosition;
					}
					else
					{
						ebxArray = base.arrays[index];
						base.Position = base.arraysOffset + ebxArray.Offset;
					}
					propertyInfo?.GetValue(obj).GetType().GetMethod("Clear")
						.Invoke(propertyInfo.GetValue(obj), new object[0]);
					for (int i = 0; i < ebxArray.Count; i++)
					{
						object obj2 = this.ReadField(customAttribute2.ArrayType, customAttribute2.BaseType, customAttribute != null);
						propertyInfo?.GetValue(obj).GetType().GetMethod("Add")
							.Invoke(propertyInfo.GetValue(obj), new object[1] { obj2 });
					}
					base.Position = position2;
				}
				else
				{
					object value = this.ReadField(customAttribute2.Type, propertyInfo.PropertyType, customAttribute != null);
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

		internal object ReadField(EbxFieldType type, Type baseType, bool dontRefCount = false)
		{
			switch (type)
			{
				case EbxFieldType.DbObject:
					throw new InvalidDataException("DbObject");
				case EbxFieldType.Struct:
					{
						object obj = TypeLibrary.CreateObject(baseType);
						EbxClassMetaAttribute customAttribute = obj.GetType().GetCustomAttribute<EbxClassMetaAttribute>();
						base.Pad(customAttribute.Alignment);
						this.ReadClass(customAttribute, obj, obj.GetType(), base.Position);
						return obj;
					}
				case EbxFieldType.Pointer:
					{
						int num = base.ReadInt32LittleEndian();
						//if (base.magic == EbxVersion.Riff)
						//{
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
                        //}
						//if (num >> 31 == 1)
						//{
						//	EbxImportReference ebxImportReference = base.imports[(int)((long)num & 0x7FFFFFFFL)];
						//	if (dontRefCount && !base.dependencies.Contains(ebxImportReference.FileGuid))
						//	{
						//		base.dependencies.Add(ebxImportReference.FileGuid);
						//	}
						//	return new PointerRef(ebxImportReference);
						//}
						//if (num == 0)
						//{
						//	return default(PointerRef);
						//}
						//if (!dontRefCount)
						//{
						//	base.refCounts[num - 1]++;
						//}
						//return new PointerRef(base.objects[num - 1]);
					}
				case EbxFieldType.String:
					return base.ReadSizedString(32);
				case EbxFieldType.CString:
					{
						uint stringOffset = base.ReadUInt32LittleEndian();
						return base.ReadCString(stringOffset);
					}
				case EbxFieldType.Enum:
					return base.ReadInt32LittleEndian();
				case EbxFieldType.FileRef:
					return base.ReadFileRef();
				case EbxFieldType.Boolean:
					return base.ReadByte() > 0;
				case EbxFieldType.Int8:
					return (sbyte)base.ReadByte();
				case EbxFieldType.UInt8:
					return base.ReadByte();
				case EbxFieldType.Int16:
					return base.ReadInt16LittleEndian();
				case EbxFieldType.UInt16:
					return base.ReadUInt16LittleEndian();
				case EbxFieldType.Int32:
					return base.ReadInt32LittleEndian();
				case EbxFieldType.UInt32:
					return base.ReadUInt32LittleEndian();
				case EbxFieldType.UInt64:
					return base.ReadUInt64LittleEndian();
				case EbxFieldType.Int64:
					return base.ReadInt64LittleEndian();
				case EbxFieldType.Float32:
					return base.ReadSingleLittleEndian();
				case EbxFieldType.Float64:
					return base.ReadDoubleLittleEndian();
				case EbxFieldType.Guid:
					return base.ReadGuid();
				case EbxFieldType.Sha1:
					return base.ReadSha1();
				case EbxFieldType.ResourceRef:
					return base.ReadResourceRef();
				case EbxFieldType.TypeRef:
					return base.ReadTypeRef();
				case EbxFieldType.BoxedValueRef:
					return base.ReadBoxedValueRef();
				default:
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Unknown field type ");
						defaultInterpolatedStringHandler.AppendFormatted(type);
						throw new InvalidDataException(defaultInterpolatedStringHandler.ToStringAndClear());
					}
			}
		}

		public override object ReadField(EbxClass? parentClass, EbxFieldType fieldType, ushort fieldClassRef, bool dontRefCount = false)
		{
			if (fieldType == EbxFieldType.Pointer)
			{
				int num = base.ReadInt32LittleEndian();
				if (num == 0)
				{
					return default(PointerRef);
				}
				if ((num & 1) == 1)
				{
					return new PointerRef(base.imports[num >> 1]);
				}
				long offset = base.Position - 4 + num - this.payloadPosition;
				int dc = this.dataContainerOffsets.IndexOf((uint)offset);
				if (dc == -1)
				{
					return default(PointerRef);
				}
				if (!dontRefCount)
				{
					base.refCounts[dc]++;
				}
				return new PointerRef(objects[dc]);
			}
			return base.ReadField(parentClass, fieldType, fieldClassRef, dontRefCount);
		}

		

		protected override void ReadArray(object obj, PropertyInfo property, EbxClass classType, EbxField field, bool isReference)
		{
            //EbxClass @class = this.GetClass(classType, field.ClassRef);

            long position = base.Position;
			int arrayOffset = base.ReadInt32LittleEndian();
			base.Position += arrayOffset - 4;
			base.Position -= 4L;
			uint arrayCount = base.ReadUInt32LittleEndian();
			for (int i = 0; i < arrayCount; i++)
			{
				//if (field.DebugType == EbxFieldType.Inherited)
				//{

				//}
				//object obj2 = this.ReadField(classType, field.DebugType, field.ClassRef, isReference);
				object obj2 = this.ReadField(classType, field.InternalType, field.ClassRef, isReference);
				if (property != null)
				{
					try
					{
						property.GetValue(obj).GetType().GetMethod("Add")
							.Invoke(property.GetValue(obj), new object[1] { obj2 });
					}
					catch (Exception)
					{
					}
				}
				EbxFieldType debugType = field.DebugType;
				if (debugType == EbxFieldType.Pointer || debugType == EbxFieldType.CString)
				{
					base.Pad(8);
				}
			}
			base.Position = position;
		}
	}
}