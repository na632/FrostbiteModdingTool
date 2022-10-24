// Sdk.IO.EbxWriterRiff
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.IO._2022.Readers;
using FrostySdk.Managers;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace FrostySdk.FrostySdk.IO
{
    public class EbxWriter2023 : EbxWriter2022
    {
        public EbxWriter2023(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.None, bool leaveOpen = false)
            : base(inStream, inFlags, true)
        {
        }

        protected override void WriteClass(object obj, Type objType, FileWriter writer)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (objType == null)
            {
                throw new ArgumentNullException("objType");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (objType.BaseType!.Namespace == "FrostySdk.Ebx")
            {
                WriteClass(obj, objType.BaseType, writer);
            }

            var startPosition = writer.Position;

            PropertyInfo[] properties = objType
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

            var writtenProperties = new List<PropertyInfo>();

            EbxClass classType = GetClass(objType);
            var fields = (from index in Enumerable.Range(0, classType.FieldCount)
                          select GetField(classType, classType.FieldIndex + index) into field
                          where field.DebugType != EbxFieldType.Inherited
                          orderby field.DataOffset
                          select field).ToList();

            var fields2 = properties
                .Where(x => x.GetCustomAttribute<HashAttribute>() != null && x.GetCustomAttribute<FieldIndexAttribute>() != null)
                .OrderBy(x => x.GetCustomAttribute<EbxFieldMetaAttribute>().Offset)
                .Select(x => EbxReader22A.GetEbxFieldByProperty(classType, x))
                .ToList();

            //foreach (EbxField field in fields)
            foreach (EbxField field in fields2)
            {

                if (field.DebugType == EbxFieldType.Inherited)
                {
                    continue;
                }
                PropertyInfo propertyInfo = Array.Find(properties, (PropertyInfo p) => (uint?)p.GetCustomAttribute<HashAttribute>()?.Hash == (uint?)field.NameHash);
                if (propertyInfo == null)
                {
                    //EbxFieldType debugType = field2.DebugType;
                    //if (debugType == EbxFieldType.ResourceRef || debugType == EbxFieldType.TypeRef || debugType == EbxFieldType.FileRef || debugType == EbxFieldType.BoxedValueRef || debugType == EbxFieldType.UInt64 || debugType == EbxFieldType.Int64 || debugType == EbxFieldType.Float64)
                    //{
                    //    writer.WritePadding(8);
                    //}
                    //else
                    //{
                    //    debugType = field2.DebugType;
                    //    if (debugType == EbxFieldType.Array || debugType == EbxFieldType.Pointer)
                    //    {
                    //        writer.WritePadding(4);
                    //    }
                    //}
                    //switch (field2.DebugType)
                    //{
                    //    case EbxFieldType.TypeRef:
                    //        writer.WriteUInt64LittleEndian(0uL);
                    //        break;
                    //    case EbxFieldType.FileRef:
                    //        writer.WriteUInt64LittleEndian(0uL);
                    //        break;
                    //    case EbxFieldType.CString:
                    //        writer.WriteInt64LittleEndian(0L);
                    //        break;
                    //    case EbxFieldType.Pointer:
                    //        writer.WriteInt64LittleEndian(0L);
                    //        break;
                    //    case EbxFieldType.Struct:
                    //        {
                    //            EbxClass value = GetClass(classType, field2);
                    //            writer.WritePadding(value.Alignment);
                    //            writer.WriteEmpty(value.Size);
                    //            break;
                    //        }
                    //    case EbxFieldType.Array:
                    //        writer.WriteInt64LittleEndian(0L);
                    //        break;
                    //    case EbxFieldType.Enum:
                    //        writer.WriteInt32LittleEndian(0);
                    //        break;
                    //    case EbxFieldType.Float32:
                    //        writer.WriteSingleLittleEndian(0f);
                    //        break;
                    //    case EbxFieldType.Float64:
                    //        writer.WriteDoubleLittleEndian(0.0);
                    //        break;
                    //    case EbxFieldType.Boolean:
                    //        writer.Write((byte)0);
                    //        break;
                    //    case EbxFieldType.Int8:
                    //        writer.Write(0);
                    //        break;
                    //    case EbxFieldType.UInt8:
                    //        writer.Write((byte)0);
                    //        break;
                    //    case EbxFieldType.Int16:
                    //        writer.WriteInt16LittleEndian(0);
                    //        break;
                    //    case EbxFieldType.UInt16:
                    //        writer.WriteUInt16LittleEndian(0);
                    //        break;
                    //    case EbxFieldType.Int32:
                    //        writer.WriteInt32LittleEndian(0);
                    //        break;
                    //    case EbxFieldType.UInt32:
                    //        writer.WriteUInt32LittleEndian(0u);
                    //        break;
                    //    case EbxFieldType.Int64:
                    //        writer.WriteInt64LittleEndian(0L);
                    //        break;
                    //    case EbxFieldType.UInt64:
                    //        writer.WriteUInt64LittleEndian(0uL);
                    //        break;
                    //    case EbxFieldType.Guid:
                    //        writer.WriteGuid(Guid.Empty);
                    //        break;
                    //    case EbxFieldType.Sha1:
                    //        writer.Write(Sha1.Zero);
                    //        break;
                    //    case EbxFieldType.String:
                    //        writer.WriteFixedSizedString(string.Empty, 32);
                    //        break;
                    //    case EbxFieldType.ResourceRef:
                    //        writer.WriteUInt64LittleEndian(0uL);
                    //        break;
                    //    case EbxFieldType.BoxedValueRef:
                    //        writer.WriteGuid(Guid.Empty);
                    //        break;
                    //}
                }
                else
                {
                    writtenProperties.Add(propertyInfo);
                    EbxFieldMetaAttribute ebxFieldMetaAttribute = propertyInfo.GetCustomAttribute<EbxFieldMetaAttribute>();
                    bool isReference = propertyInfo.GetCustomAttribute<IsReferenceAttribute>() != null;
                    if (ebxFieldMetaAttribute.IsArray)
                    {
                        uint fieldNameHash = propertyInfo.GetCustomAttribute<HashAttribute>()!.Hash;
                        WriteArray(propertyInfo.GetValue(obj), ebxFieldMetaAttribute.ArrayType, fieldNameHash, classType, classType.Alignment, writer, isReference);
                    }
                    else
                    {
                        WriteField(propertyInfo.GetValue(obj), ebxFieldMetaAttribute.Type, classType.Alignment, writer, isReference);
                    }
                }
            }

            var unwrittenProperties = properties.Where(x => !writtenProperties.Any(y => y.Name == x.Name));
            if (unwrittenProperties.Any() && obj == objsToProcess[0])
            {

            }
            writer.WritePadding(classType.Alignment);
            //while(writer.Position < startPosition + classType.Size)
            //{
            //    writer.Write((byte)0);
            //}
        }

    }
}