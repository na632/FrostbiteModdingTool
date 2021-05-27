using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FrostySdk.IO
{
	public class EbxWriter_F21 : EbxWriterV2
	{
		public EbxWriter_F21(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.IncludeTransient, bool leaveOpen = false)
			: base(inStream, inFlags, leaveOpen)
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
			}
			else
			{
				List<object> list = new List<object>();
				foreach (object rootObject in asset.RootObjects)
				{
					list.Add(rootObject);
				}
				WriteEbxObjects(list, asset.FileGuid);
			}


		}

		public void WriteEbxObject(object inObj, Guid fileGuid)
		{
			List<object> inObjects = new List<object>
			{
				inObj
			};
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
			var startPosition = BaseStream.Position;

			uint stringPosition = 0u;
			uint currentminusstring = 0u;
			uint currentminusstringminussomeother = 0u;
			uint arrayWritePosition = 0u;
			foreach (object item in objsToProcess)
			{
				ProcessClass(item.GetType());
			}
			for (int i = 0; i < typesToProcess.Count; i++)
			{
				ProcessType(i);
			}
			ProcessData();
			// Write Magic for Version 4
			Write(263508430);
			// Strings Offset
			Write(0);
			// Strings and Data Length
			Write(0);
			// guid count
			Write(imports.Count);
			// instance count
			Write((ushort)instances.Count);
			// export count
			Write(exportedCount);
			// unique class count
			Write(uniqueClassCount);
			// class type count
			Write((ushort)classGuids.Count);
			// field type count
			Write((ushort)0);
			// type names length
			Write((ushort)0);
			// string length
			Write((uint)0);
            // array length

            Write(arrays.Count == 0 ? 0 : arrays.Count + 1);
            //Write(arrays.Count);
            //Write(arrays.Count+1);
            // data length
            Write(0);
			// file guid
			Write(fileGuid);
			// boxed values count 
			Write((int)0);
			// boxed value offset
			Write((int)0);

			// guid Count
			foreach (EbxImportReference import in imports)
			{
				Write(import.FileGuid);
				Write(import.ClassGuid);
			}
			long position = BaseStream.Position;
			foreach (Guid classGuid in classGuids)
			{
				Write(classGuid);
			}
			foreach (EbxInstance instance in instances)
			{
				Write(instance.ClassRef);
				Write(instance.Count);
			}

			//Write((uint)0);
			//Write((uint)0);
			WritePadding(16);

			// write the empty array
			long arrayposition = BaseStream.Position;
            if (arrays.Count > 0 && arrays[0].Offset != 0)
            {
                Write(0);
                Write(0);
                Write(0);
            }
            for (int j = 0; j < arrays.Count; j++)
			{
				Write(0);
				Write(0);
				Write(0);
			}
            if (arrays.Count > 0 && arrays[0].Offset == 0)
            {
                Write(0);
                Write(0);
                Write(0);
            }
            //Write((uint)0);
            //Write((uint)0);
            //for (int j = 0; j < boxedValueRefs.Count; j++)
            //{
            //	Write(0);
            //	Write(0);
            //}

            WritePadding(16);

			//Write(0ul);
			//Write(0ul);
			stringPosition = (uint)BaseStream.Position;
			foreach (string @string in strings)
			{
				WriteNullTerminatedString(@string);
			}
			WritePadding(16);
			stringsLength = (uint)(BaseStream.Position - stringPosition);

			//WritePadding(8);
			// 4 byte padding
			//Write(0u);
			//Write(0u);
			//Write(0ul);
			//stringsLength = (uint)(BaseStream.Position - stringPosition);

			position = BaseStream.Position;
			Write(data);
			Write((byte)0);
			WritePadding(16);

			// v2k4
			arrayWritePosition = (uint)(BaseStream.Position - position);
			if (arrays.Count > 0)
			{
				position = BaseStream.Position;
				for (int k = 0; k < arrays.Count; k++)
				{
					EbxArray value = arrays[k];
					Write(value.Count);
					value.Offset = (uint)(BaseStream.Position - position);
					Write(arrayData[k]);
					if (k != arrays.Count - 1)
					{
						Write(0);
					}
					WritePadding(16);
					BaseStream.Position -= 4L;
					arrays[k] = value;
				}
				BaseStream.Position += 4L;
				WritePadding(16);
			}
			currentminusstring = (uint)(BaseStream.Position - stringPosition);
			currentminusstringminussomeother = (uint)(BaseStream.Position - stringsLength - stringPosition);
			if (boxedValueRefs.Count > 0)
			{
				for (int l = 0; l < boxedValueRefs.Count; l++)
				{
					Write(boxedValueRefs[l].GetData());
				}
				WritePadding(16);
			}
			BaseStream.Position = 4L;
			Write(stringPosition);
			//Write(stringPosition);
			Write(currentminusstring);
			BaseStream.Position = 26L;
			Write((ushort)0);
			Write(stringsLength);
			BaseStream.Position = 36L;

			Write(arrayWritePosition);
			//Write(arrayWritePosition - 12);
			//BaseStream.Position = arrayposition;
			BaseStream.Position = arrayposition + 12;
			for (int m = 0; m < arrays.Count; m++)
			{
				Write(arrays[m].Offset);
				Write(arrays[m].Count);
				Write(arrays[m].ClassRef);
			}

			BaseStream.Position = 56L;
			Write(boxedValueRefs.Count);
			Write(currentminusstringminussomeother);



			BaseStream.Position = 0;

			if (!Directory.Exists("Debbuging/EBX/Export/"))
				Directory.CreateDirectory("Debugging/EBX/Export/");

			var splitName = strings[0].Split('/');
			var n = splitName[splitName.Length - 1];

			if (File.Exists($"Debugging/EBX/{n}.dat"))
			{
				using (
					NativeReader nr_original = new NativeReader(new FileStream($"Debugging/EBX/{n}.dat", FileMode.OpenOrCreate)))
				{
					// Fix missing data
					if (nr_original.Length > BaseStream.Length)
					{
						BaseStream.Position = BaseStream.Length;

						nr_original.Position = BaseStream.Length;
						var restoffile = nr_original.ReadToEnd();
						Write(restoffile);
					}

					// Fix data arrays
					nr_original.Position = 4;
					var orig_arrayPos = nr_original.ReadInt();
					nr_original.Position = orig_arrayPos;

				}
			}


			BaseStream.Position = 0;

			if (File.Exists($"Debugging/EBX/Export/{n}.dat"))
				File.Delete($"Debugging/EBX/Export/{n}.dat");
			using (FileStream fileStream = new FileStream($"Debugging/EBX/Export/{n}.dat", FileMode.OpenOrCreate))
				this.BaseStream.CopyTo(fileStream);

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
				else if (propertyInfo.PropertyType.Namespace == "FrostySdk.Ebx" && propertyInfo.PropertyType.BaseType != typeof(Enum))
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
					int num = (int)propertyType.GetMethod("get_Count").Invoke(propertyInfo.GetValue(obj), null);
					if (num <= 0)
					{
						continue;
					}
					if (propertyType.GenericTypeArguments[0] == typeof(PointerRef))
					{
						for (int j = 0; j < num; j++)
						{
							PointerRef pointerRef2 = (PointerRef)propertyType.GetMethod("get_Item").Invoke(propertyInfo.GetValue(obj), new object[1]
							{
								j
							});
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
					else if (propertyType.GenericTypeArguments[0].Namespace == "FrostySdk.Ebx" && propertyType.GenericTypeArguments[0].BaseType != typeof(Enum))
					{
						for (int k = 0; k < num; k++)
						{
							object obj2 = propertyType.GetMethod("get_Item").Invoke(propertyInfo.GetValue(obj), new object[1]
							{
								k
							});
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
			if (objType.BaseType.Namespace == "FrostySdk.Ebx")
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
			//PropertyInfo[] properties = objType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
			List<PropertyInfo> listOfProperties = new List<PropertyInfo>();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (propertyInfo.GetCustomAttribute<IsTransientAttribute>() == null || flags.HasFlag(EbxWriteFlags.IncludeTransient))
				{
					listOfProperties.Add(propertyInfo);
				}
			}
			num = AddClass(objType.Name, fieldTypes.Count, (byte)(listOfProperties.Count + (flag ? 1 : 0)), customAttribute.Alignment, customAttribute.Flags, customAttribute.Size, 0, objType);
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
				foreach (PropertyInfo item in listOfProperties)
				{
					EbxFieldMetaAttribute ebxfieldmeta = item.GetCustomAttribute<EbxFieldMetaAttribute>();
					var variableName = item.Name;
					var switchVar = (byte)((ebxfieldmeta.Flags >> 4) & 0x1F);
					switch (switchVar)
					{
						case 2:
							{
								Type propertyType2 = item.PropertyType;
								ProcessClass(propertyType2);
								break;
							}
						case 4:
							{
								EbxFieldType ebxFieldType = (EbxFieldType)((ebxfieldmeta.ArrayFlags >> 4) & 0x1F);
								Type propertyType = item.PropertyType;
								if (FindExistingClass(propertyType) == -1)
								{
									if (!typesToProcess.Contains(propertyType))
									{
										arrayTypes.Add(ebxfieldmeta);
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
						case 8:
							{
								Type propertyType3 = item.PropertyType;
								ProcessClass(propertyType3);
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
				//if (ProfilesLibrary.EbxVersion == 2)
				//{
				//	AddField("$", 0, classRef, (uint)((ebxClass.Alignment < 8) ? 8 : ebxClass.Alignment), 0u);
				//}
				//else
				//{
				AddField("$", 0, classRef, 8u, 0u);
				//}
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
			_ = (byte)((customAttribute.Flags >> 4) & 0x1F);
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
			list2.Sort((Comparison<object>)delegate (dynamic a, dynamic b)
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
			using (NativeWriter nativeWriter = new NativeWriter(memoryStream))
			{
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
						nativeWriter.Write(assetClassGuid.ExportedGuid);
					}
					if (ebxClass.Alignment != 4)
					{
						nativeWriter.Write(0uL);
					}
					WriteClass(sortedObjs[j], type, nativeWriter);
					num2 = (ushort)(num2 + 1);
				}
				item2.Count = num2;
				instances.Add(item2);
			}
			data = memoryStream.ToArray();
			uniqueClassCount = (ushort)list.Count;
		}

		private void WriteClass(object obj, Type objType, NativeWriter writer)
		{
			if (objType.BaseType.Namespace == "FrostySdk.Ebx")
			{
				WriteClass(obj, objType.BaseType, writer);
			}
			PropertyInfo[] properties = objType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			EbxClass classType = classTypes[FindExistingClass(objType)];
			for (int i = 0; i < classType.FieldCount; i++)
			{
				//EbxField field = GetField(classType, classType.FieldIndex + i);
				EbxField field = GetField(classType, classType.FieldIndex + i);
				if (field.DebugType == EbxFieldType.Inherited)
				{
					continue;
				}
				PropertyInfo propertyInfo = null;
				PropertyInfo[] propertyArray = properties;
				foreach (PropertyInfo propertyInfo2 in propertyArray)
				{
					HashAttribute customAttribute = propertyInfo2.GetCustomAttribute<HashAttribute>();
					if (customAttribute != null && customAttribute.Hash == (int)field.NameHash)
					{
						propertyInfo = propertyInfo2;
						break;
					}
				}
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
							writer.Write(0uL);
							break;
						case EbxFieldType.FileRef:
							writer.Write(0uL);
							break;
						case EbxFieldType.CString:
							writer.Write(0);
							break;
						case EbxFieldType.Pointer:
							writer.Write(0);
							break;
						case EbxFieldType.Struct:
							{
								EbxClass value = EbxReader_F21.std.GetClass(classType.Index + (short)field.ClassRef).Value;
								writer.WritePadding(value.Alignment);
								writer.Write(new byte[value.Size]);
								break;
							}
						case EbxFieldType.Array:
							writer.Write(0);
							break;
						case EbxFieldType.Enum:
							writer.Write(0);
							break;
						case EbxFieldType.Float32:
							writer.Write(0f);
							break;
						case EbxFieldType.Float64:
							writer.Write(0.0);
							break;
						case EbxFieldType.Boolean:
							writer.Write((byte)0);
							break;
						case EbxFieldType.Int8:
							writer.Write((sbyte)0);
							break;
						case EbxFieldType.UInt8:
							writer.Write((byte)0);
							break;
						case EbxFieldType.Int16:
							writer.Write((short)0);
							break;
						case EbxFieldType.UInt16:
							writer.Write((ushort)0);
							break;
						case EbxFieldType.Int32:
							writer.Write(0);
							break;
						case EbxFieldType.UInt32:
							writer.Write(0u);
							break;
						case EbxFieldType.Int64:
							writer.Write(0L);
							break;
						case EbxFieldType.UInt64:
							writer.Write(0uL);
							break;
						case EbxFieldType.Guid:
							writer.Write(Guid.Empty);
							break;
						case EbxFieldType.Sha1:
							writer.Write(Sha1.Zero);
							break;
						case EbxFieldType.String:
							writer.WriteFixedSizedString("", 32);
							break;
						case EbxFieldType.ResourceRef:
							writer.Write(0uL);
							break;
						case EbxFieldType.BoxedValueRef:
							writer.Write(Guid.Empty);
							break;
					}
				}
				else
				{
					EbxFieldMetaAttribute customAttribute2 = propertyInfo.GetCustomAttribute<EbxFieldMetaAttribute>();
					bool isReference = propertyInfo.GetCustomAttribute<IsReferenceAttribute>() != null;
					var fieldName = propertyInfo.Name;
					EbxFieldType ebxType = (EbxFieldType)((customAttribute2.Flags >> 4) & 0x1F);
					var value = propertyInfo.GetValue(obj);
					WriteField(value, ebxType, classType.Alignment, writer, isReference);
				}
			}
			//writer.WritePadding(classType.Alignment);
		}

		private void WriteField(object obj, EbxFieldType ebxType, byte classAlignment, NativeWriter writer, bool isReference)
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
					//writer.WritePadding(8);
					break;
				case EbxFieldType.Pointer:
				case EbxFieldType.Array:
					// Remove Push
					//writer.WritePadding(4);
					break;
			}
			switch (ebxType)
			{
				case EbxFieldType.TypeRef:
					writer.Write((ulong)AddString((TypeRef)obj));
					break;
				case EbxFieldType.FileRef:
					writer.Write((ulong)AddString((FileRef)obj));
					break;
				case EbxFieldType.CString:
					writer.Write(AddString((CString)obj));
					break;
				case EbxFieldType.Pointer:
					{
						PointerRef pointer = (PointerRef)obj;
						uint value2 = 0u;
						if (pointer.Type == PointerRefType.External)
						{
							int num = imports.FindIndex((EbxImportReference value) => value == pointer.External);
							value2 = (uint)(num | 2147483648u);
							if (isReference && !dependencies.Contains(imports[num].FileGuid))
							{
								dependencies.Add(imports[num].FileGuid);
							}
						}
						else if (pointer.Type == PointerRefType.Internal)
						{
							value2 = (uint)(sortedObjs.FindIndex((object value) => value == pointer.Internal) + 1);
						}
						// written after the string section
						writer.Write(value2);
						break;
					}
				case EbxFieldType.Struct:
					{
						object obj3 = obj;
						Type type2 = obj3.GetType();
						EbxClass ebxClass = classTypes[FindExistingClass(type2)];
						writer.WritePadding(ebxClass.Alignment);
						WriteClass(obj3, type2, writer);
						break;
					}
				case EbxFieldType.Array:

					{
						var currentPositionOfMainReader = writer.BaseStream.Position;

						int typeIndex = typesToProcess.FindIndex((Type item) => item == obj.GetType());
						//int arrayCount = 0;
						EbxClass classType = classTypes[typeIndex];
						ebxType = GetField(classType, classType.FieldIndex).DebugType;
						Type type = obj.GetType();
						int arrayLength = (int)type.GetMethod("get_Count").Invoke(obj, null);
						MemoryStream memoryStream = new MemoryStream();
						using (NativeWriter nw_array = new NativeWriter(memoryStream))
						{
							for (int i = 0; i < arrayLength; i++)
							{
								object get_item = type.GetMethod("get_Item").Invoke(obj, new object[1]
								{
							i
								});
								var arT = get_item.GetType();
								WriteField(get_item, ebxType, classAlignment, nw_array, isReference);
							}
						}
						//arrayCount = arrays.Count;
						arrays.Add(new EbxArray
						{
							Count = (uint)arrayLength,
							ClassRef = typeIndex
						});
						arrayData.Add(memoryStream.ToArray());
                        //writer.Write((uint)arrayCount);
                        //writer.Write((uint)arrays.Count-1);

						// FIFA 21 indexing starts at 1
						//writer.Write((uint)(arrayCount + 1));
						writer.Write((uint)(arrays.Count));
						break;
					}
				case EbxFieldType.Enum:
					writer.Write((int)obj);
					break;
				case EbxFieldType.Float32:
					writer.Write((float)obj);
					break;
				case EbxFieldType.Float64:
					writer.Write((double)obj);
					break;
				case EbxFieldType.Boolean:
					writer.Write((byte)(((bool)obj) ? 1 : 0));
					break;
				case EbxFieldType.Int8:
					writer.Write((sbyte)obj);
					break;
				case EbxFieldType.UInt8:
					writer.Write((byte)obj);
					break;
				case EbxFieldType.Int16:
					writer.Write((short)obj);
					break;
				case EbxFieldType.UInt16:
					writer.Write((ushort)obj);
					break;
				case EbxFieldType.Int32:
					writer.Write((int)obj);
					break;
				case EbxFieldType.UInt32:
					writer.Write((uint)obj);
					break;
				case EbxFieldType.Int64:
					writer.Write((long)obj);
					break;
				case EbxFieldType.UInt64:
					writer.Write((ulong)obj);
					break;
				case EbxFieldType.Guid:
					writer.Write((Guid)obj);
					break;
				case EbxFieldType.Sha1:
					writer.Write((Sha1)obj);
					break;
				case EbxFieldType.String:
					writer.WriteFixedSizedString((string)obj, 32);
					break;
				case EbxFieldType.ResourceRef:
					writer.Write((ResourceRef)obj);
					break;
				case EbxFieldType.BoxedValueRef:
					boxedValueRefs.Add((BoxedValueRef)obj);
					writer.Write((BoxedValueRef)obj);
					break;
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
			EbxClass @class = GetClass(pi.GetCustomAttribute<GuidAttribute>().Guid);
			classTypes.Add(@class);
			typesToProcess.Add(classType);
			classGuids.Add(pi.GetCustomAttribute<GuidAttribute>().Guid);
			return classTypes.Count - 1;
		}

		private int AddClass(string name, int fieldIndex, byte fieldCount, byte alignment, ushort type, ushort size, ushort secondSize, Type classType)
		{
			EbxClass? @class = GetClass(classType);
			if (@class.HasValue)
			{
				classTypes.Add(@class.Value);
				if (EbxReader_F21.std != null)
					classGuids.Add(EbxReader_F21.std.GetGuid(@class.Value));
				else if (EbxReader_M21.std != null)
					classGuids.Add(EbxReader_M21.std.GetGuid(@class.Value));

				AddTypeName(name);
				typesToProcess.Add(classType);
			}
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
				num = ((num * 33) ^ b);
			}
			return num;
		}

		private uint AddString(string stringToAdd)
		{
			if (stringToAdd == "")
			{
				return uint.MaxValue;
			}
			uint num = 0u;
			if (strings.Contains(stringToAdd))
			{
				for (int i = 0; i < strings.Count && !(strings[i] == stringToAdd); i++)
				{
					num = (uint)((int)num + (strings[i].Length + 1));
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

		internal EbxClass? GetClass(Type objType)
		{
			EbxClass? ebxClass = null;
			using (IEnumerator<TypeInfoGuidAttribute> enumerator = objType.GetCustomAttributes<TypeInfoGuidAttribute>().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					TypeInfoGuidAttribute current = enumerator.Current;
					if (!ebxClass.HasValue)
					{
						if(EbxReader_F21.std != null)
							ebxClass = EbxReader_F21.std.GetClass(current.Guid);

						if (!ebxClass.HasValue && EbxReader_F21.patchStd != null)
							ebxClass = EbxReader_F21.patchStd.GetClass(current.Guid);

						if (!ebxClass.HasValue && EbxReader_F21.std != null)
							ebxClass = EbxReader_F21.std.GetClass(current.Guid);
						
						if (!ebxClass.HasValue && EbxReader_M21.std != null)
							ebxClass = EbxReader_M21.std.GetClass(current.Guid);
					}
				}
			}
			return ebxClass;
		}

		internal EbxClass GetClass(Guid guid)
		{
			EbxClass? ebxClass = null;

			if(EbxReader_F21.std != null)
				ebxClass = EbxReader_F21.std.GetClass(guid);

			if (!ebxClass.HasValue && EbxReader_F21.patchStd != null)
				ebxClass = EbxReader_F21.patchStd.GetClass(guid);

			if (!ebxClass.HasValue && EbxReader_F21.std != null)
				ebxClass = EbxReader_F21.std.GetClass(guid); 

			if (!ebxClass.HasValue && EbxReader_M21.std != null)
				ebxClass = EbxReader_M21.std.GetClass(guid);

			return ebxClass.Value;
		}

		internal EbxField GetField(EbxClass classType, int index)
		{
			EbxField? ebxField = null;

			if (EbxReader_F21.std != null)
				ebxField = EbxReader_F21.std.GetField(index);

			if (!ebxField.HasValue && EbxReader_F21.patchStd != null)
				ebxField = EbxReader_F21.patchStd.GetField(index);

			if (!ebxField.HasValue && EbxReader_F21.std != null)
				ebxField = EbxReader_F21.std.GetField(index);

			if (!ebxField.HasValue && EbxReader_M21.std != null)
				ebxField = EbxReader_M21.std.GetField(index);

			return ebxField.Value;
		}
	}
}
