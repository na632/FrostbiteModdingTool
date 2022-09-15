using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FrostySdk.FrostySdk.IO
{
	public class EbxWriterV3 : EbxBaseWriter
	{
		private List<object> objsToProcess = new List<object>();

		private List<Type> typesToProcess = new List<Type>();

		private List<EbxFieldMetaAttribute> arrayTypes = new List<EbxFieldMetaAttribute>();

		private List<object> objs = new List<object>();

		private List<object> sortedObjs = new List<object>();

		private List<Guid> dependencies = new List<Guid>();

		private List<EbxClass> classTypes = new List<EbxClass>();

		private List<Guid> classGuids = new List<Guid>();

		private List<EbxField> fieldTypes = new List<EbxField>();

		private List<string> typeNames = new List<string>();

		private List<EbxImportReference> imports = new List<EbxImportReference>();

		private new List<string> strings = new List<string>();

		private byte[] data;

		private List<EbxInstance> instances = new List<EbxInstance>();

		private List<EbxArray> arrays = new List<EbxArray>();

		private List<byte[]> arrayData = new List<byte[]>();

		private ushort uniqueClassCount;

		private ushort exportedCount;

		public List<object> Objects => sortedObjs;

		public List<Guid> Dependencies => dependencies;

		//public EbxWriterV3(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.None)
		//	: base(inStream, inFlags)
		//{
		//}
		public EbxWriterV3(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.None, bool leaveOpen = false)
			: base(inStream, inFlags, true)
		{
		}

		public override void WriteAsset(EbxAsset asset)
		{
			if (flags.HasFlag(EbxWriteFlags.DoNotSort))
			{
				foreach (object @object in asset.Objects)
				{
					ExtractClass(@object.GetType(), @object);
				}
				WriteEbx(asset.FileGuid);
				return;
			}
			List<object> list = new List<object>();
			foreach (object rootObject in asset.RootObjects)
			{
				list.Add(rootObject);
			}
			WriteEbxObjects(list, asset.FileGuid);
		}

		public void WriteEbxObject(object inObj, Guid fileGuid)
		{
			List<object> inObjects = new List<object> { inObj };
			WriteEbxObjects(inObjects, fileGuid);
		}

		public void WriteEbxObjects(List<object> inObjects, Guid fileGuid)
		{
			List<object> list = new List<object>();
			list.AddRange(inObjects);
			while (list.Count > 0)
			{
				object obj = list[0];
				list.RemoveAt(0);
				list.AddRange(ExtractClass(obj.GetType(), obj));
			}
			WriteEbx(fileGuid);
		}

		private void WriteEbx(Guid fileGuid)
		{
			uint num = 0u;
			uint num2 = 0u;
			uint num3 = 0u;
			uint num4 = 0u;
			foreach (object item in objsToProcess)
			{
				ProcessClass(item.GetType());
			}
			for (int i = 0; i < typesToProcess.Count; i++)
			{
				ProcessType(i);
			}
			ProcessData();
			Write((int)263508430);
			Write((int)0);
			Write((int)0);
			Write((int)imports.Count);
			Write((ushort)(ushort)instances.Count);
			Write((ushort)exportedCount);
			Write((ushort)uniqueClassCount);
			Write((ushort)(ushort)classGuids.Count);
			Write((ushort)0);
			Write((ushort)0);
			Write((int)0);
			Write((int)arrays.Count);
			Write((int)0);
			WriteGuid(fileGuid);
			Write((uint)3735928559u);
			Write((uint)3735928559u);
			foreach (EbxImportReference import in imports)
			{
				WriteGuid(import.FileGuid);
				WriteGuid(import.ClassGuid);
			}
			long position = base.Position;
			foreach (Guid classGuid in classGuids)
			{
				WriteGuid(classGuid);
			}
			foreach (EbxInstance instance in instances)
			{
				Write((ushort)instance.ClassRef);
				Write((ushort)instance.Count);
			}
			WritePadding(16);
			long position2 = base.Position;
			for (int j = 0; j < arrays.Count; j++)
			{
				Write((int)0);
				Write((int)0);
				Write((int)0);
			}
			WritePadding(16);
			long position3 = base.Position;
			for (int k = 0; k < boxedValues.Count; k++)
			{
				Write((int)0);
				Write((int)0);
			}
			WritePadding(16);
			num = (uint)base.Position;
			foreach (string @string in strings)
			{
				WriteNullTerminatedString(@string);
			}
			WritePadding(16);
			stringsLength = (uint)(base.Position - num);
			position = base.Position;
			WriteBytes(data);
			Write((byte)0);
			WritePadding(16);
			num4 = (uint)(base.Position - position);
			if (arrays.Count > 0)
			{
				position = base.Position;
				for (int l = 0; l < arrays.Count; l++)
				{
					EbxArray value = arrays[l];
					Write((uint)value.Count);
					value.Offset = (uint)(base.Position - position);
					WriteBytes(arrayData[l]);
					if (l != arrays.Count - 1)
					{
						Write(0);
					}
					WritePadding(16);
					base.Position -= 4L;
					arrays[l] = value;
				}
				base.Position += 4L;
				WritePadding(16);
			}
			num3 = (uint)(base.Position - stringsLength - num);
			if (boxedValueData.Count > 0)
			{
				for (int m = 0; m < boxedValueData.Count; m++)
				{
					EbxBoxedValue value2 = boxedValues[m];
					Write(0);
					value2.Offset = (uint)(base.Position - num3);
					WriteBytes(boxedValueData[m]);
					boxedValues[m] = value2;
				}
				WritePadding(16);
			}
			num2 = (uint)(base.Position - num);
			base.Position = 4L;
			Write((uint)num);
			Write((uint)num2);
			base.Position = 26L;
			Write((ushort)0);
			Write((uint)stringsLength);
			base.Position = 36L;
			Write((uint)num4);
			base.Position = position2;
			for (int n = 0; n < arrays.Count; n++)
			{
				Write((uint)arrays[n].Offset);
				Write((uint)arrays[n].Count);
				Write((int)arrays[n].ClassRef);
			}
			base.Position = 56L;
			Write((int)boxedValueData.Count);
			Write((uint)num3);
			base.Position = position3;
			for (int num5 = 0; num5 < boxedValues.Count; num5++)
			{
				Write((uint)boxedValues[num5].Offset);
				Write((ushort)boxedValues[num5].ClassRef);
				Write((ushort)boxedValues[num5].Type);
			}
		}

		private List<object> ExtractClass(Type type, object obj, bool add = true)
		{
			if (add)
			{
				if (objsToProcess.Contains(obj))
				{
					return new List<object>();
				}
				objsToProcess.Add(obj);
				objs.Add(obj);
			}
			PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			List<object> list = new List<object>();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (propertyInfo.GetCustomAttribute<IsTransientAttribute>() != null && !flags.HasFlag(EbxWriteFlags.IncludeTransient))
				{
					continue;
				}
				if (propertyInfo.PropertyType == typeof(PointerRef))
				{
					PointerRef pointerRef = (PointerRef)propertyInfo.GetValue(obj);
					if (pointerRef.Type == PointerRefType.Internal)
					{
						list.Add(pointerRef.Internal);
					}
					else if (pointerRef.Type == PointerRefType.External && !imports.Contains(pointerRef.External))
					{
						imports.Add(pointerRef.External);
					}
				}
				else if (propertyInfo.PropertyType.Namespace == "Sdk.Ebx" && propertyInfo.PropertyType.BaseType != typeof(Enum))
				{
					object value = propertyInfo.GetValue(obj);
					list.AddRange(ExtractClass(value.GetType(), value, add: false));
				}
				else
				{
					if (!(propertyInfo.PropertyType.Name == "List`1"))
					{
						continue;
					}
					Type propertyType = propertyInfo.PropertyType;
					int num = (int)propertyType.GetMethod("get_Count")!.Invoke(propertyInfo.GetValue(obj), null);
					if (num <= 0)
					{
						continue;
					}
					if (propertyType.GenericTypeArguments[0] == typeof(PointerRef))
					{
						for (int i = 0; i < num; i++)
						{
							PointerRef pointerRef2 = (PointerRef)propertyType.GetMethod("get_Item")!.Invoke(propertyInfo.GetValue(obj), new object[1] { i });
							if (pointerRef2.Type == PointerRefType.Internal)
							{
								list.Add(pointerRef2.Internal);
							}
							else if (pointerRef2.Type == PointerRefType.External && !imports.Contains(pointerRef2.External))
							{
								imports.Add(pointerRef2.External);
							}
						}
					}
					else if (propertyType.GenericTypeArguments[0].Namespace == "Sdk.Ebx" && propertyType.GenericTypeArguments[0].BaseType != typeof(Enum))
					{
						for (int j = 0; j < num; j++)
						{
							object obj2 = propertyType.GetMethod("get_Item")!.Invoke(propertyInfo.GetValue(obj), new object[1] { j });
							list.AddRange(ExtractClass(obj2.GetType(), obj2, add: false));
						}
					}
				}
			}
			if (type.BaseType != typeof(object) && type.BaseType != typeof(ValueType))
			{
				list.AddRange(ExtractClass(type.BaseType, obj, add: false));
			}
			return list;
		}

		private ushort ProcessClass(Type objType)
		{
			bool flag = false;
			if (objType.BaseType!.Namespace == "Sdk.Ebx")
			{
				ProcessClass(objType.BaseType);
				flag = true;
			}
			int num = FindExistingClass(objType);
			if (num != -1)
			{
				return (ushort)num;
			}
			EbxClassMetaAttribute customAttribute = objType.GetCustomAttribute<EbxClassMetaAttribute>();
			PropertyInfo[] properties = objType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			List<PropertyInfo> list = new List<PropertyInfo>();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (propertyInfo.GetCustomAttribute<IsTransientAttribute>() == null || flags.HasFlag(EbxWriteFlags.IncludeTransient))
				{
					list.Add(propertyInfo);
				}
			}
			num = AddClass(objType.Name, fieldTypes.Count, (byte)(list.Count + (flag ? 1 : 0)), customAttribute.Alignment, customAttribute.Flags, customAttribute.Size, 0, objType);
			if (flag)
			{
				AddTypeName("$");
			}
			if (objType.IsEnum)
			{
				uint num2 = 0u;
				string[] enumNames = objType.GetEnumNames();
				foreach (string inName in enumNames)
				{
					AddTypeName(inName);
					num2++;
				}
			}
			else
			{
				foreach (PropertyInfo item in list)
				{
					EbxFieldMetaAttribute customAttribute2 = item.GetCustomAttribute<EbxFieldMetaAttribute>();
					switch ((byte)((customAttribute2.Flags >> 4) & 0x1F))
					{
						case 2:
							{
								Type propertyType2 = item.PropertyType;
								ProcessClass(propertyType2);
								break;
							}
						case 8:
							{
								Type propertyType3 = item.PropertyType;
								ProcessClass(propertyType3);
								break;
							}
						case 4:
							{
								EbxFieldType ebxFieldType = (EbxFieldType)((uint)(customAttribute2.ArrayFlags >> 4) & 0x1Fu);
								Type propertyType = item.PropertyType;
								if (FindExistingClass(propertyType) == -1)
								{
									if (!typesToProcess.Contains(propertyType))
									{
										arrayTypes.Add(customAttribute2);
										AddClass(item, propertyType);
									}
									switch (ebxFieldType)
									{
										case EbxFieldType.Struct:
											{
												Type objType3 = propertyType.GenericTypeArguments[0];
												ProcessClass(objType3);
												break;
											}
										case EbxFieldType.Enum:
											{
												Type objType2 = propertyType.GenericTypeArguments[0];
												ProcessClass(objType2);
												break;
											}
									}
									AddTypeName("member");
								}
								break;
							}
					}
					AddTypeName(item.Name);
				}
			}
			return (ushort)num;
		}

		private void ProcessType(int index)
		{
			Type type = typesToProcess[index];
			EbxClass ebxClass = classTypes[index];
			if (ebxClass.DebugType == EbxFieldType.Array)
			{
				EbxFieldMetaAttribute ebxFieldMetaAttribute = arrayTypes[0];
				arrayTypes.RemoveAt(0);
				ushort num = (ushort)FindExistingClass(type.GenericTypeArguments[0]);
				if (num == ushort.MaxValue)
				{
					num = 0;
				}
				AddField("member", ebxFieldMetaAttribute.ArrayFlags, num, 0u, 0u);
				return;
			}
			if (ebxClass.DebugType == EbxFieldType.Enum)
			{
				string[] enumNames = type.GetEnumNames();
				Array enumValues = type.GetEnumValues();
				for (int i = 0; i < enumNames.Length; i++)
				{
					int num2 = (int)enumValues.GetValue(i);
					AddField(enumNames[i], 0, 0, (uint)num2, (uint)num2);
				}
				return;
			}
			PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			List<PropertyInfo> list = new List<PropertyInfo>();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if ((propertyInfo.GetCustomAttribute<IsTransientAttribute>() == null || flags.HasFlag(EbxWriteFlags.IncludeTransient)) && !propertyInfo.Name.Equals("__InstanceGuid"))
				{
					list.Add(propertyInfo);
				}
			}
			if (type.BaseType != typeof(object) && type.BaseType != typeof(ValueType))
			{
				ushort classRef = (ushort)FindExistingClass(type.BaseType);
				AddField("$", 0, classRef, 8u, 0u);
			}
			foreach (PropertyInfo item in list)
			{
				ProcessField(item);
			}
		}

		private void ProcessField(PropertyInfo pi)
		{
			ushort num = 0;
			EbxFieldMetaAttribute customAttribute = pi.GetCustomAttribute<EbxFieldMetaAttribute>();
			Type propType = pi.PropertyType;
			num = (ushort)typesToProcess.FindIndex((Type value) => value == propType);
			if (num == ushort.MaxValue)
			{
				num = 0;
			}
			AddField(pi.Name, customAttribute.Flags, num, customAttribute.Offset, 0u);
		}

		private void ProcessData()
		{
			List<Type> list = new List<Type>();
			List<object> list2 = new List<object>();
			List<object> list3 = new List<object>();
			for (int i = 0; i < objs.Count; i++)
			{
				dynamic val = objs[i];
				if (((AssetClassGuid)val.GetInstanceGuid()).IsExported)
				{
					list2.Add(val);
				}
				else
				{
					list3.Add(val);
				}
			}
			object item = list2[0];
			list2.RemoveAt(0);
			list2.Sort(delegate (dynamic a, dynamic b)
			{
				AssetClassGuid assetClassGuid2 = a.GetInstanceGuid();
				AssetClassGuid assetClassGuid3 = b.GetInstanceGuid();
				byte[] array = assetClassGuid2.ExportedGuid.ToByteArray();
				byte[] array2 = assetClassGuid3.ExportedGuid.ToByteArray();
				uint num3 = (uint)((array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3]);
				uint value = (uint)((array2[0] << 24) | (array2[1] << 16) | (array2[2] << 8) | array2[3]);
				return num3.CompareTo(value);
			});
			list3.Sort((object a, object b) => a.GetType().Name.CompareTo(b.GetType().Name));
			sortedObjs.Add(item);
			sortedObjs.AddRange(list2);
			sortedObjs.AddRange(list3);
			MemoryStream memoryStream = new MemoryStream();
			FileWriter nativeWriter = new FileWriter(memoryStream);
			Type type = sortedObjs[0].GetType();
			int num = FindExistingClass(type);
			EbxClass ebxClass = classTypes[num];
			EbxInstance ebxInstance = default(EbxInstance);
			ebxInstance.ClassRef = (ushort)num;
			ebxInstance.Count = 0;
			ebxInstance.IsExported = true;
			EbxInstance item2 = ebxInstance;
			ushort num2 = 0;
			exportedCount++;
			for (int j = 0; j < sortedObjs.Count; j++)
			{
				AssetClassGuid assetClassGuid = ((dynamic)sortedObjs[j]).GetInstanceGuid();
				type = sortedObjs[j].GetType();
				num = FindExistingClass(type);
				ebxClass = classTypes[num];
				if (!list.Contains(type))
				{
					list.Add(type);
				}
				if (num != item2.ClassRef || (item2.IsExported && !assetClassGuid.IsExported))
				{
					item2.Count = num2;
					instances.Add(item2);
					item2 = default(EbxInstance);
					item2.ClassRef = (ushort)num;
					item2.IsExported = assetClassGuid.IsExported;
					exportedCount += (ushort)(item2.IsExported ? 1 : 0);
					num2 = 0;
				}
				nativeWriter.WritePadding(ebxClass.Alignment);
				if (assetClassGuid.IsExported)
				{
					nativeWriter.WriteGuid(assetClassGuid.ExportedGuid);
				}
				if (ebxClass.Alignment != 4)
				{
					nativeWriter.WriteUInt64LittleEndian(0uL);
				}
				WriteClass(sortedObjs[j], type, nativeWriter);
				num2 = (ushort)(num2 + 1);
			}
			item2.Count = num2;
			instances.Add(item2);
			data = memoryStream.ToArray();
			uniqueClassCount = (ushort)list.Count;
		}

		private void WriteClass(object obj, Type objType, FileWriter writer)
		{
			if (objType.BaseType!.Namespace == "Sdk.Ebx")
			{
				WriteClass(obj, objType.BaseType, writer);
			}
			PropertyInfo[] properties = objType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			EbxClass classType = classTypes[FindExistingClass(objType)];
			for (int i = 0; i < classType.FieldCount; i++)
			{
				EbxField field = GetField(classType, classType.FieldIndex + i);
				if (field.DebugType == EbxFieldType.Inherited)
				{
					continue;
				}
				PropertyInfo propertyInfo = Array.Find(properties, (PropertyInfo p) => p.GetCustomAttribute<HashAttribute>()?.Hash == (int?)field.NameHash);

				//PropertyInfo propertyInfo = null;
				//PropertyInfo[] array = properties;
				//foreach (PropertyInfo propertyInfo2 in array)
				//{
				//	HashAttribute customAttribute = propertyInfo2.GetCustomAttribute<HashAttribute>();
				//	if (customAttribute != null && customAttribute.Hash == (int)field.NameHash)
				//	{
				//		propertyInfo = propertyInfo2;
				//		break;
				//	}
				//}
				if (propertyInfo == null)
				{
					if (field.DebugType == EbxFieldType.ResourceRef || field.DebugType == EbxFieldType.TypeRef || field.DebugType == EbxFieldType.FileRef || field.DebugType == EbxFieldType.BoxedValueRef || field.DebugType == EbxFieldType.UInt64 || field.DebugType == EbxFieldType.Int64 || field.DebugType == EbxFieldType.Float64)
					{
						writer.WritePadding(8);
					}
					else if (field.DebugType == EbxFieldType.Array || field.DebugType == EbxFieldType.Pointer)
					{
						writer.WritePadding(4);
					}
					switch (field.DebugType)
					{
						case EbxFieldType.TypeRef:
							writer.WriteUInt64LittleEndian(0uL);
							break;
						case EbxFieldType.FileRef:
							writer.WriteUInt64LittleEndian(0uL);
							break;
						case EbxFieldType.CString:
							writer.Write((int)0);
							break;
						case EbxFieldType.Pointer:
							writer.Write((int)0);
							break;
						case EbxFieldType.Struct:
							{
								EbxClass value = EbxReader2021.std.GetClass(classType.Index + (short)field.ClassRef).Value;
								writer.WritePadding(value.Alignment);
								writer.WriteBytes(new byte[value.Size]);
								break;
							}
						case EbxFieldType.Array:
							writer.Write((int)0);
							break;
						case EbxFieldType.Enum:
							writer.Write((int)0);
							break;
						case EbxFieldType.Float32:
							writer.WriteSingleLittleEndian(0f);
							break;
						case EbxFieldType.Float64:
							writer.WriteDoubleLittleEndian(0.0);
							break;
						case EbxFieldType.Boolean:
							writer.Write((byte)0);
							break;
						case EbxFieldType.Int8:
							writer.Write(0);
							break;
						case EbxFieldType.UInt8:
							writer.Write((byte)0);
							break;
						case EbxFieldType.Int16:
							writer.WriteInt16LittleEndian(0);
							break;
						case EbxFieldType.UInt16:
							writer.Write((ushort)0);
							break;
						case EbxFieldType.Int32:
							writer.Write((int)0);
							break;
						case EbxFieldType.UInt32:
							writer.Write((uint)0u);
							break;
						case EbxFieldType.Int64:
							writer.WriteInt64LittleEndian(0L);
							break;
						case EbxFieldType.UInt64:
							writer.WriteUInt64LittleEndian(0uL);
							break;
						case EbxFieldType.Guid:
							writer.WriteGuid(Guid.Empty);
							break;
						case EbxFieldType.Sha1:
							writer.Write(Sha1.Zero);
							break;
						case EbxFieldType.String:
							writer.WriteFixedSizedString("", 32);
							break;
						case EbxFieldType.ResourceRef:
							writer.WriteUInt64LittleEndian(0uL);
							break;
						case EbxFieldType.BoxedValueRef:
							writer.WriteGuid(Guid.Empty);
							break;
					}
				}
				else
				{
					EbxFieldMetaAttribute? customAttribute2 = propertyInfo.GetCustomAttribute<EbxFieldMetaAttribute>();
					bool isReference = propertyInfo.GetCustomAttribute<IsReferenceAttribute>() != null;
					EbxFieldType ebxType = (EbxFieldType)((uint)(customAttribute2!.Flags >> 4) & 0x1Fu);
					WriteField(propertyInfo.GetValue(obj), ebxType, classType.Alignment, writer, isReference);
				}
			}
			writer.WritePadding(classType.Alignment);
		}

		private void WriteField(object obj, EbxFieldType ebxType, byte classAlignment, FileWriter writer, bool isReference)
		{
			switch (ebxType)
			{
				case EbxFieldType.FileRef:
				case EbxFieldType.UInt64:
				case EbxFieldType.Int64:
				case EbxFieldType.Float64:
				case EbxFieldType.ResourceRef:
				case EbxFieldType.TypeRef:
				case EbxFieldType.BoxedValueRef:
					writer.WritePadding(8);
					break;
				case EbxFieldType.Pointer:
				case EbxFieldType.Array:
					writer.WritePadding(4);
					break;
			}
			switch (ebxType)
			{
				case EbxFieldType.TypeRef:
					writer.WriteUInt64LittleEndian(AddString((TypeRef)obj));
					break;
				case EbxFieldType.FileRef:
					writer.WriteUInt64LittleEndian(AddString((FileRef)obj));
					break;
				case EbxFieldType.CString:
					writer.Write((uint)AddString((CString)obj));
					break;
				case EbxFieldType.Pointer:
					{
						PointerRef pointer = (PointerRef)obj;
						uint value2 = 0u;
						if (pointer.Type == PointerRefType.External)
						{
							int num = imports.FindIndex((EbxImportReference value) => value == pointer.External);
							value2 = (uint)(num | 0x80000000u);
							if (isReference && !dependencies.Contains(imports[num].FileGuid))
							{
								dependencies.Add(imports[num].FileGuid);
							}
						}
						else if (pointer.Type == PointerRefType.Internal)
						{
							value2 = (uint)(sortedObjs.FindIndex((object value) => value == pointer.Internal) + 1);
						}
						writer.Write((uint)value2);
						break;
					}
				case EbxFieldType.Struct:
					{
						object obj3 = obj;
						Type type2 = obj3.GetType();
						writer.WritePadding(classTypes[FindExistingClass(type2)].Alignment);
						WriteClass(obj3, type2, writer);
						break;
					}
				case EbxFieldType.Array:
					{
						int num2 = typesToProcess.FindIndex((Type item) => item == obj.GetType());
						int num3 = 0;
						EbxClass classType = classTypes[num2];
						ebxType = GetField(classType, classType.FieldIndex).DebugType;
						Type type = obj.GetType();
						int num4 = (int)type.GetMethod("get_Count")!.Invoke(obj, null);
						MemoryStream memoryStream = new MemoryStream();
						FileWriter writer2 = new FileWriter(memoryStream);
						for (int i = 0; i < num4; i++)
						{
							object obj2 = type.GetMethod("get_Item")!.Invoke(obj, new object[1] { i });
							obj2.GetType();
							WriteField(obj2, ebxType, classAlignment, writer2, isReference);
						}
						num3 = arrays.Count;
						arrays.Add(new EbxArray
						{
							Count = (uint)num4,
							ClassRef = num2
						});
						arrayData.Add(memoryStream.ToArray());
						writer.Write((int)num3);
						break;
					}
				case EbxFieldType.Enum:
					writer.Write((int)(int)obj);
					break;
				case EbxFieldType.Float32:
					writer.WriteSingleLittleEndian((float)obj);
					break;
				case EbxFieldType.Float64:
					writer.WriteDoubleLittleEndian((double)obj);
					break;
				case EbxFieldType.Boolean:
					writer.Write((byte)(((bool)obj) ? 1u : 0u));
					break;
				case EbxFieldType.Int8:
					writer.Write((sbyte)obj);
					break;
				case EbxFieldType.UInt8:
					writer.Write((byte)obj);
					break;
				case EbxFieldType.Int16:
					writer.WriteInt16LittleEndian((short)obj);
					break;
				case EbxFieldType.UInt16:
					writer.Write((ushort)(ushort)obj);
					break;
				case EbxFieldType.Int32:
					writer.Write((int)(int)obj);
					break;
				case EbxFieldType.UInt32:
					writer.Write((uint)(uint)obj);
					break;
				case EbxFieldType.Int64:
					writer.WriteInt64LittleEndian((long)obj);
					break;
				case EbxFieldType.UInt64:
					writer.WriteUInt64LittleEndian((ulong)obj);
					break;
				case EbxFieldType.Guid:
					writer.WriteGuid((Guid)obj);
					break;
				case EbxFieldType.Sha1:
					writer.Write((Sha1)obj);
					break;
				case EbxFieldType.String:
					writer.WriteFixedSizedString((string)obj, 32);
					break;
				case EbxFieldType.ResourceRef:
					writer.WriteUInt64LittleEndian((ResourceRef)obj);
					break;
				case EbxFieldType.BoxedValueRef:
					{
						BoxedValueRef boxedValueRef = (BoxedValueRef)obj;
						int count = boxedValues.Count;
						EbxBoxedValue ebxBoxedValue = default(EbxBoxedValue);
						ebxBoxedValue.Offset = 0u;
						ebxBoxedValue.Type = (ushort)boxedValueRef.Type;
						EbxBoxedValue item2 = ebxBoxedValue;
						boxedValues.Add(item2);
						boxedValueData.Add(WriteBoxedValueRef(boxedValueRef));
						Write((int)count);
						WriteUInt64LittleEndian(0uL);
						Write((uint)0u);
						break;
					}
				default:
					throw new InvalidDataException("Error");
			}
		}

		private int FindExistingClass(Type inType)
		{
			return typesToProcess.FindIndex((Type value) => value == inType);
		}

		private void AddTypeName(string inName)
		{
			if (!typeNames.Contains(inName))
			{
				typeNames.Add(inName);
			}
		}

		private int AddClass(PropertyInfo pi, Type classType)
		{
			EbxClass @class = GetClass(pi.GetCustomAttribute<GuidAttribute>()!.Guid);
			classTypes.Add(@class);
			typesToProcess.Add(classType);
			classGuids.Add(pi.GetCustomAttribute<GuidAttribute>()!.Guid);
			return classTypes.Count - 1;
		}

		private int AddClass(string name, int fieldIndex, byte fieldCount, byte alignment, ushort type, ushort size, ushort secondSize, Type classType)
		{
			EbxClass @class = GetClass(classType);
			classTypes.Add(@class);
			classGuids.Add(EbxReaderV3.std.GetGuid(@class).Value);
			AddTypeName(name);
			typesToProcess.Add(classType);
			return classTypes.Count - 1;
		}

		private void AddField(string name, ushort type, ushort classRef, uint dataOffset, uint secondOffset)
		{
			AddTypeName(name);
		}

		private int HashString(string strToHash)
		{
			int num = 5381;
			for (int i = 0; i < strToHash.Length; i++)
			{
				byte b = (byte)strToHash[i];
				num = (num * 33) ^ b;
			}
			return num;
		}

		private new uint AddString(string stringToAdd)
		{
			if (stringToAdd == "")
			{
				return uint.MaxValue;
			}
			uint num = 0u;
			if (strings.Contains(stringToAdd))
			{
				for (int i = 0; i < strings.Count && strings[i] != stringToAdd; i++)
				{
					num += (uint)(strings[i].Length + 1);
				}
			}
			else
			{
				num = stringsLength;
				strings.Add(stringToAdd);
				stringsLength += (uint)(stringToAdd.Length + 1);
			}
			return num;
		}

		internal EbxClass GetClass(Type objType)
		{
			EbxClass? ebxClass = null;
			using (IEnumerator<TypeInfoGuidAttribute> enumerator = objType.GetCustomAttributes<TypeInfoGuidAttribute>().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					TypeInfoGuidAttribute current = enumerator.Current;
					if (!ebxClass.HasValue)
					{
						ebxClass = EbxReaderV3.std.GetClass(current.Guid);
					}
				}
			}
			return ebxClass.Value;
		}

		internal EbxClass GetClass(Guid guid)
		{
			return EbxReaderV3.std.GetClass(guid).Value;
		}

		internal EbxField GetField(EbxClass classType, int index)
		{
			return EbxReaderV3.std.GetField(index).Value;
		}
	}

}
