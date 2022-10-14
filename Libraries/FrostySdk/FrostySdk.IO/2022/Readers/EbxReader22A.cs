using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.FrostySdk.IO;
using FrostySdk.FrostySdk.IO._2022.Readers;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace FrostySdk.IO._2022.Readers
{
	public class EbxReader22A : EbxReader
	{
		private List<EbxClass> classTypes = new List<EbxClass>();

		internal byte[] boxedValueBuffer;

		//internal EbxVersion magic;

		public override string RootType => this.classTypes[this.instances[0].ClassRef].Name;

		internal EbxReader22A(Stream inStream, bool passthru)
			: base(inStream)
		{
			Position = 0;
			if (inStream == null)
			{
				throw new ArgumentNullException("inStream");
			}
		}

        public override EbxAsset ReadAsset(EbxAssetEntry entry = null)
        {
			EbxAsset ebxAsset = new EbxAsset();
			ebxAsset.ParentEntry = entry;
			this.InternalReadObjects();
			ebxAsset.fileGuid = fileGuid;
			ebxAsset.objects = objects;
			ebxAsset.dependencies = dependencies;
			ebxAsset.refCounts = refCounts;
			return ebxAsset;
		}

        public virtual T ReadAsset<T>() where T : EbxAsset, new()
		{
			T val = new T();
			this.InternalReadObjects();
			val.fileGuid = this.fileGuid;
			val.objects = this.objects;
			val.dependencies = this.dependencies;
			val.refCounts = this.refCounts;
			return val;
		}

		public new dynamic ReadObject()
		{
			this.InternalReadObjects();
			return this.objects[0];
		}

		public override List<object> ReadObjects()
		{
			this.InternalReadObjects();
			return this.objects;
		}

		public override List<object> GetUnreferencedObjects()
		{
			List<object> list = new List<object> { this.objects[0] };
			for (int i = 1; i < this.objects.Count; i++)
			{
				if (this.refCounts[i] == 0)
				{
					list.Add(this.objects[i]);
				}
			}
			return list;
		}

		public override void InternalReadObjects()
		{
			foreach (EbxInstance instance in this.instances)
			{
				EbxClass @class = this.GetClass(null, instance.ClassRef);
				for (int i = 0; i < instance.Count; i++)
				{
					Type inType = this.ParseClass(@class);
					this.objects.Add(TypeLibrary.CreateObject(inType));
					this.refCounts.Add(0);
				}
			}
			int num = 0;
			int num2 = 0;
			foreach (EbxInstance instance2 in this.instances)
			{
				EbxClass class2 = this.GetClass(null, instance2.ClassRef);
				for (int j = 0; j < instance2.Count; j++)
				{
					base.Pad(class2.Alignment);
					Guid inGuid = Guid.Empty;
					if (instance2.IsExported)
					{
						inGuid = base.ReadGuid();
					}
					if (class2.Alignment != 4)
					{
						base.Position += 8L;
					}
					dynamic val = this.objects[num++];
					val.SetInstanceGuid(new AssetClassGuid(inGuid, num2++));
					this.ReadClass(class2, val, base.Position - 8);
				}
			}
			if (this.boxedValuesCount != 0)
			{
				base.Position = this.boxedValuesOffset;
				this.boxedValueBuffer = base.ReadToEnd();
			}
		}

		public override object ReadClass(EbxClass classType, object obj, long startOffset)
		{
			if (obj == null)
			{
				base.Position += classType.Size;
				base.Pad(classType.Alignment);
				return null;
			}
			Type type = obj.GetType();

			Dictionary<PropertyInfo, EbxFieldMetaAttribute> properties = new Dictionary<PropertyInfo, EbxFieldMetaAttribute>();
			foreach(var prp in obj.GetType().GetProperties())
            {
				var ebxfieldmeta = prp.GetCustomAttribute<EbxFieldMetaAttribute>();
				if(ebxfieldmeta != null)
                {
					properties.Add(prp, ebxfieldmeta);
				}
            }

            foreach (var property in properties)
            {
                var isTransient = property.Key.GetCustomAttribute<IsTransientAttribute>();
				if (isTransient != null)
					continue;

                var propFieldIndex = property.Key.GetCustomAttribute<FieldIndexAttribute>();
                var propNameHash = property.Key.GetCustomAttribute<HashAttribute>();
                if (propFieldIndex == null && propNameHash == null)
                    continue;

                EbxField field = default(EbxField);

                if (propNameHash != null)
                {
                    field = EbxReader22B.patchStd.Fields
                      .Union(EbxReader22B.std.Fields).FirstOrDefault(x => x.NameHash == propNameHash.Hash);
                }
                else if (propFieldIndex != null)
                {
                    field = this.GetField(classType, classType.FieldIndex + propFieldIndex.Index);
                }

                EbxFieldType debugType = (EbxFieldType)((property.Value.Flags >> 4) & 0x1Fu);
                var classRef = field.ClassRef;

                // Variable from SDK is King here! Override DebugType.
                //if (field.DebugType != debugType) 
                //	field.Type = property.Value.Flags;

                if (debugType == EbxFieldType.Inherited)
                {
                    ReadClass(GetClass(classType, field.ClassRef), obj, startOffset);
                    continue;
                }
                if (debugType == EbxFieldType.ResourceRef
                    || debugType == EbxFieldType.TypeRef
                    || debugType == EbxFieldType.FileRef
                    || debugType == EbxFieldType.BoxedValueRef
                    || debugType == EbxFieldType.UInt64
                    || debugType == EbxFieldType.Int64
                    || debugType == EbxFieldType.Float64)
                {
                    base.Pad(8);
                }
                else
                {
                    if (debugType == EbxFieldType.Array
                        || debugType == EbxFieldType.Pointer
                        )
                    {
                        base.Pad(4);
                    }
                }
                base.Position = property.Value.Offset + startOffset;
                if (debugType == EbxFieldType.Array)
                {
                    //ReadArray(obj, property.Key, classType, field, false);
                    ReadArray(obj, property.Key, classType, false);
                    continue;
                }
                try
                {
                    object value = ReadField(classType, debugType, field.ClassRef, false);

                    property.Key.SetValue(obj, value);
                }
                catch (Exception ex)
                {
					Debug.WriteLine(ex.Message);
                }
            }

            //for (int i = 0; i < classType.FieldCount; i++)
            //{
            //    EbxField field = GetField(classType, classType.FieldIndex + i);
            //    PropertyInfo property = GetProperty(type, field);
            //    IsReferenceAttribute isReferenceAttribute = ((property != null) ? property.GetCustomAttribute<IsReferenceAttribute>() : null);
            //    if (field.DebugType == EbxFieldType.Inherited)
            //    {
            //        ReadClass(GetClass(classType, field.ClassRef), obj, startOffset);
            //        continue;
            //    }
            //    EbxFieldType debugType = field.DebugType;
            //    if (debugType == EbxFieldType.ResourceRef
            //        || debugType == EbxFieldType.TypeRef
            //        || debugType == EbxFieldType.FileRef
            //        || debugType == EbxFieldType.BoxedValueRef
            //        || debugType == EbxFieldType.UInt64
            //        || debugType == EbxFieldType.Int64
            //        || debugType == EbxFieldType.Float64)
            //    {
            //        base.Pad(8);
            //    }
            //    else
            //    {
            //        debugType = field.DebugType;
            //        if (debugType == EbxFieldType.Array
            //            || debugType == EbxFieldType.Pointer
            //            || field.Category == EbxFieldCategory.Array
            //            || field.DebugType22 == EbxFieldType22.ArrayOfStructs
            //            || field.DebugType22 == EbxFieldType22.Pointer
            //            )
            //        {
            //            base.Pad(4);
            //        }
            //    }
            //    base.Position = field.DataOffset + startOffset;
            //    if (IsFieldInClassAnArray(classType, field))
            //    {
            //        ReadArray(obj, property, classType, field, isReferenceAttribute != null);
            //        continue;
            //    }
            //    object value = ReadField(classType, field.DebugType, field.ClassRef, isReferenceAttribute != null);
            //    if (property != null)
            //    {
            //        try
            //        {
            //            property.SetValue(obj, value);
            //        }
            //        catch (Exception)
            //        {
            //        }
            //    }
            //}
            //         if (this.magic == EbxVersion.Riff)
            //{
            base.Position = startOffset + classType.Size;
			//}
			base.Pad(classType.Alignment);
			return null;
		}

		protected virtual void ReadArray(object obj, PropertyInfo property, EbxClass classType, bool isReference)
		{
			if(obj.GetType().Name == "FloatCurve")
            {

            }
			long position = base.Position;
			int arrayOffset = base.ReadInt32LittleEndian();
			base.Position += arrayOffset - 4;
			base.Position -= 4L;
			uint arrayCount = base.ReadUInt32LittleEndian();
			var genericType = property.PropertyType.GetGenericArguments()[0];
            EbxField field = default(EbxField);

			var nameHashAttribute = genericType.GetCustomAttribute<HashAttribute>();
			var ebxfieldmeta = genericType.GetCustomAttribute<EbxFieldMetaAttribute>();
			var ebxclassmeta = genericType.GetCustomAttribute<EbxClassMetaAttribute>();
			if (nameHashAttribute != null && ebxfieldmeta != null)
				field = EbxReader22B.patchStd.Fields
						  .Union(EbxReader22B.std.Fields).FirstOrDefault(x => x.NameHash == nameHashAttribute.Hash);
			else if (nameHashAttribute != null && ebxclassmeta != null)
				classType = EbxReader22B.patchStd.Classes
						  .Union(EbxReader22B.std.Classes).FirstOrDefault(x => x.HasValue && x.Value.NameHash == nameHashAttribute.Hash).Value;

			for (int i = 0; i < arrayCount; i++)
			{
				EbxFieldType ebxFieldType = EbxFieldType.Struct;
				if (!Enum.TryParse<EbxFieldType>(genericType.Name, out ebxFieldType)) 
				{
					ebxFieldType = EbxFieldType.Struct;
                }
				field.DebugTypeOverride = genericType.Name == "Single" ? EbxFieldType.Float32 : ebxFieldType;
				classType.DebugTypeOverride = genericType.Name == "Single" ? EbxFieldType.Float32 : ebxFieldType;
				field.ClassRef = 65535;
				try
				{
					object obj2 = null;
					if(field.DebugType == EbxFieldType.Struct)
                    {
						obj2 = CreateObject(classType);
						this.ReadClass(classType, obj2, base.Position);
					}
					else
						obj2 = this.ReadField(classType, field.DebugType, field.ClassRef, isReference);
					if (property != null)
					{
                    
							property.GetValue(obj).GetType().GetMethod("Add")
								.Invoke(property.GetValue(obj), new object[1] { obj2 });
                    
					}
				}
				catch (Exception)
				{
				}
				//EbxFieldType debugType = field.DebugType;
				//if (debugType == EbxFieldType.Pointer || debugType == EbxFieldType.CString)
				//{
				//	base.Pad(8);
				//}
			}
			base.Position = position;
		}

		protected virtual void ReadArray(object obj, PropertyInfo property, EbxClass classType, EbxField field, bool isReference)
		{
			EbxClass @class = this.GetClass(classType, field.ClassRef);
			int index = base.ReadInt32LittleEndian();
			EbxArray ebxArray = this.arrays[index];
			long position = base.Position;
			base.Position = this.arraysOffset + ebxArray.Offset;
			for (int i = 0; i < ebxArray.Count; i++)
			{
				EbxField field2 = this.GetField(@class, @class.FieldIndex);
				object obj2 = this.ReadField(@class, field2.DebugType, field2.ClassRef, isReference);
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
			}
			base.Position = position;
		}

		protected bool IsFieldInClassAnArray(EbxClass @class, EbxField field)
		{
			return field.TypeCategory == EbxTypeCategory.ArrayType
				|| field.DebugType == EbxFieldType.Array;
				//|| field.DebugType22 == EbxFieldType22.ArrayOfStructs;
		}

		public override PropertyInfo GetProperty(Type objType, EbxField field)
		{
			return objType.GetProperty(field.Name);
		}

		public override EbxClass GetClass(EbxClass? parentClass, int index)
		{
			return this.classTypes[index];
		}

		public override EbxField GetField(EbxClass classType, int index)
		{
			return this.fieldTypes[index];
		}

		public override object CreateObject(EbxClass classType)
		{
			return TypeLibrary.CreateObject(classType.Name);
		}

		public virtual Type GetType(EbxClass classType)
		{
			return TypeLibrary.GetType(classType.Name);
		}

		public override object ReadField(EbxClass? parentClass, EbxFieldType fieldType, ushort fieldClassRef, bool dontRefCount = false)
		{
			if (buffer == null || BaseStream == null)
				return null;

			switch (fieldType)
			{
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
				case EbxFieldType.Int64:
					return base.ReadInt64LittleEndian();
				case EbxFieldType.UInt64:
					return base.ReadUInt64LittleEndian();
				case EbxFieldType.Float32:
					return base.ReadSingleLittleEndian();
				case EbxFieldType.Float64:
					return base.ReadDoubleLittleEndian();
				case EbxFieldType.Guid:
					return base.ReadGuid();
				case EbxFieldType.ResourceRef:
					return this.ReadResourceRef();
				case EbxFieldType.Sha1:
					return base.ReadSha1();
				case EbxFieldType.String:
					return base.ReadSizedString(32);
				case EbxFieldType.CString:
					return this.ReadCString(base.ReadUInt32LittleEndian());
				case EbxFieldType.FileRef:
					return this.ReadFileRef();
				case EbxFieldType.TypeRef:
					return this.ReadTypeRef();
				case EbxFieldType.BoxedValueRef:
					return this.ReadBoxedValueRef();
				case EbxFieldType.Struct:
					{
                        EbxClass @class = GetClass(parentClass, fieldClassRef);
                        base.Pad(@class.Alignment);
                        object obj = CreateObject(@class);
                        this.ReadClass(@class, obj, base.Position);
                        return obj;
                        //return null;
                    }
				case EbxFieldType.Enum:
					return base.ReadInt32LittleEndian();
				case EbxFieldType.Pointer:
					{
						uint num = base.ReadUInt32LittleEndian();
						if (num >> 31 == 1)
						{
							return new PointerRef(this.imports[(int)(num & 0x7FFFFFFF)]);
						}
						if (num == 0)
						{
							return default(PointerRef);
						}
						if (!dontRefCount)
						{
							this.refCounts[(int)(num - 1)]++;
						}
						return new PointerRef(this.objects[(int)(num - 1)]);
					}
				case EbxFieldType.DbObject:
					throw new InvalidDataException("DbObject");
				default:
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Unknown field type ");
						defaultInterpolatedStringHandler.AppendFormatted(fieldType);
						throw new InvalidDataException(defaultInterpolatedStringHandler.ToStringAndClear());
					}
			}
		}

		internal Type ParseClass(EbxClass classType)
		{
			Type type = TypeLibrary.AddType(classType.Name);
			if (type != null)
			{
				return type;
			}
			List<FieldType> list = new List<FieldType>();
			Type parentType = null;
			for (int i = 0; i < classType.FieldCount; i++)
			{
				EbxField ebxField = this.fieldTypes[classType.FieldIndex + i];
				if (ebxField.DebugType == EbxFieldType.Inherited)
				{
					parentType = this.ParseClass(this.classTypes[ebxField.ClassRef]);
					continue;
				}
				Type typeFromEbxField = this.GetTypeFromEbxField(ebxField);
				list.Add(new FieldType(ebxField.Name, typeFromEbxField, null, ebxField, (ebxField.DebugType == EbxFieldType.Array) ? new EbxField?(this.fieldTypes[this.classTypes[ebxField.ClassRef].FieldIndex]) : null));
			}
			if (classType.DebugType == EbxFieldType.Struct)
			{
				return TypeLibrary.FinalizeStruct(classType.Name, list, classType);
			}
			return TypeLibrary.FinalizeClass(classType.Name, list, parentType, classType);
		}

		internal Type GetTypeFromEbxField(EbxField fieldType)
		{
			switch (fieldType.DebugType)
			{
				case EbxFieldType.DbObject:
					return null;
				case EbxFieldType.Struct:
					return this.ParseClass(this.classTypes[fieldType.ClassRef]);
				case EbxFieldType.Pointer:
					return typeof(PointerRef);
				case EbxFieldType.Array:
					{
						EbxClass ebxClass = this.classTypes[fieldType.ClassRef];
						return typeof(List<>).MakeGenericType(this.GetTypeFromEbxField(this.fieldTypes[ebxClass.FieldIndex]));
					}
				case EbxFieldType.String:
					return typeof(string);
				case EbxFieldType.CString:
					return typeof(CString);
				case EbxFieldType.Enum:
					{
						EbxClass classInfo = this.classTypes[fieldType.ClassRef];
						List<Tuple<string, int>> list = new List<Tuple<string, int>>();
						for (int i = 0; i < classInfo.FieldCount; i++)
						{
							list.Add(new Tuple<string, int>(this.fieldTypes[classInfo.FieldIndex + i].Name, (int)this.fieldTypes[classInfo.FieldIndex + i].DataOffset));
						}
						return TypeLibrary.AddEnum(classInfo.Name, list, classInfo);
					}
				case EbxFieldType.FileRef:
					return typeof(FileRef);
				case EbxFieldType.Boolean:
					return typeof(bool);
				case EbxFieldType.Int8:
					return typeof(sbyte);
				case EbxFieldType.UInt8:
					return typeof(byte);
				case EbxFieldType.Int16:
					return typeof(short);
				case EbxFieldType.UInt16:
					return typeof(ushort);
				case EbxFieldType.Int32:
					return typeof(int);
				case EbxFieldType.UInt32:
					return typeof(uint);
				case EbxFieldType.UInt64:
					return typeof(ulong);
				case EbxFieldType.Int64:
					return typeof(long);
				case EbxFieldType.Float32:
					return typeof(float);
				case EbxFieldType.Float64:
					return typeof(double);
				case EbxFieldType.Guid:
					return typeof(Guid);
				case EbxFieldType.Sha1:
					return typeof(Sha1);
				case EbxFieldType.ResourceRef:
					return typeof(ResourceRef);
				case EbxFieldType.TypeRef:
					return typeof(TypeRef);
				case EbxFieldType.BoxedValueRef:
					return typeof(ulong);
				default:
					return null;
			}
		}

		internal string ReadString(uint offset)
		{
			if (offset == uint.MaxValue)
			{
				return string.Empty;
			}
			long position = base.Position;
			if (this.magic == EbxVersion.Riff)
			{
				base.Position = position + offset - 4;
			}
			else
			{
				base.Position = this.stringsOffset + offset;
			}
			string result = base.ReadNullTerminatedString();
			base.Position = position;
			return result;
		}

		internal CString ReadCString(uint offset)
		{
			return new CString(this.ReadString(offset));
		}

		internal ResourceRef ReadResourceRef()
		{
			return new ResourceRef(base.ReadUInt64LittleEndian());
		}

		internal FileRef ReadFileRef()
		{
			uint offset = base.ReadUInt32LittleEndian();
			base.Position += 4L;
			return new FileRef(this.ReadString(offset));
		}

		internal TypeRef ReadTypeRef()
		{
			if (this.magic == EbxVersion.Riff)
			{
				return new TypeRef(base.ReadUInt32LittleEndian().ToString(CultureInfo.InvariantCulture));
			}
			string text = this.ReadString(base.ReadUInt32LittleEndian());
			base.Position += 4L;
			if (text == "")
			{
				return new TypeRef();
			}
			if (Guid.TryParse(text, out var result) && result != Guid.Empty)
			{
				return new TypeRef(result);
			}
			return new TypeRef(text);
		}

		internal BoxedValueRef ReadBoxedValueRef()
		{
			if (this.magic == EbxVersion.Riff)
			{
				uint value = base.ReadUInt32LittleEndian();
				int unk = base.ReadInt32LittleEndian();
				long offset = base.ReadInt64LittleEndian();
				long restorePosition = base.Position;
				try
				{
					_ = -1;
					if ((value & 0x80000000u) == 2147483648u)
					{
						value &= 0x7FFFFFFFu;
						EbxFieldType typeCode = (EbxFieldType)((value >> 5) & 0x1Fu);
						base.Position += offset - 8;
						return new BoxedValueRef(this.ReadField(null, typeCode, ushort.MaxValue), typeCode);
					}
					return new BoxedValueRef();
				}
				finally
				{
					base.Position = restorePosition;
				}
			}
			int num = base.ReadInt32LittleEndian();
			base.Position += 12L;
			if (num == -1)
			{
				return new BoxedValueRef();
			}
			EbxBoxedValue ebxBoxedValue = this.boxedValues[num];
			EbxFieldType subType = EbxFieldType.Inherited;
			long position = base.Position;
			base.Position = this.boxedValuesOffset + ebxBoxedValue.Offset;
			object inval;
			if ((byte)ebxBoxedValue.Type == 4)
			{
				EbxClass @class = this.GetClass(null, ebxBoxedValue.ClassRef);
				EbxField field = this.GetField(@class, @class.FieldIndex);
				inval = Activator.CreateInstance(typeof(List<>).MakeGenericType(this.GetTypeFromEbxField(field)));
				num = base.ReadInt32LittleEndian();
				EbxArray ebxArray = this.arrays[num];
				long position2 = base.Position;
				base.Position = this.arraysOffset + ebxArray.Offset;
				for (int i = 0; i < ebxArray.Count; i++)
				{
					object obj2 = this.ReadField(@class, field.DebugType, field.ClassRef);
					inval.GetType().GetMethod("Add").Invoke(inval, new object[1] { obj2 });
				}
				subType = field.DebugType;
				base.Position = position2;
			}
			else
			{
				inval = this.ReadField(null, (EbxFieldType)ebxBoxedValue.Type, ebxBoxedValue.ClassRef);
				if ((byte)ebxBoxedValue.Type == 8)
				{
					object obj3 = inval;
					EbxClass class2 = this.GetClass(null, ebxBoxedValue.ClassRef);
					inval = Enum.Parse(this.GetType(class2), obj3.ToString());
				}
			}
			base.Position = position;
			return new BoxedValueRef(inval, (EbxFieldType)ebxBoxedValue.Type, subType);
		}

		internal int HashString(string strToHash)
		{
			int num = 5381;
			for (int i = 0; i < strToHash.Length; i++)
			{
				byte b = (byte)strToHash[i];
				num = (num * 33) ^ b;
			}
			return num;
		}
	}
}