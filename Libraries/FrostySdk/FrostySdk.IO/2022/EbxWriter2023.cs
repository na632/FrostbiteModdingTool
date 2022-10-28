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

        protected override void WriteClass(object obj, Type objType, FileWriter writer, long startPosition = 0)
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
                WriteClass(obj, objType.BaseType, writer, writer.Position);
            }

            if (startPosition == 0L) startPosition = writer.Position;

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
                //.OrderBy(x => x.DataOffset)
                .ToList();

            //foreach (EbxField field in fields)
            foreach (EbxField field in fields2)
            {

                if (field.DebugType == EbxFieldType.Inherited)
                {
                    continue;
                }
                PropertyInfo propertyInfo = Array.Find(properties, (PropertyInfo p) => (uint?)p.GetCustomAttribute<HashAttribute>()?.Hash == (uint?)field.NameHash);

                //long currentOffset = writer.Position - startPosition;
                //if (currentOffset < 0)
                //{
                //    writer.Position = startPosition;
                //}
                //else if (currentOffset > field.DataOffset)
                //{
                //    writer.Position = startPosition + field.DataOffset;
                //}
                //else if (currentOffset < field.DataOffset)
                //{
                //    int adjustment = (int)(field.DataOffset - currentOffset);
                //    int adjustmentByPaddingTo8 = (int)(8 - currentOffset % 8);
                //    if (adjustment != adjustmentByPaddingTo8)
                //    {
                //        //continue;
                //    }
                //    writer.WriteEmpty(adjustment);
                //}

                if (propertyInfo == null)
                {
                    AssetManager.Instance.LogError($"EBX Writing: Unable to write field {field.NameHash} of class {obj.GetType().Name}");
                    continue;
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