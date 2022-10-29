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
    //public class EbxWriter2023 : EbxWriterRiff// EbxWriter2022
    public class EbxWriter2023 : EbxWriter2022
    {
        public EbxWriter2023(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.None, bool leaveOpen = false)
            : base(inStream, inFlags, true)
        {
        }

        //public override void WriteClass(object obj, Type objType, FileWriter writer, int startOfDataContainer, int dataContainerIndex)
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
                //WriteClass(obj, objType.BaseType, writer, (int)writer.Position, dataContainerIndex);
            }

            //if (startPosition == 0) startPosition = writer.Position;

            PropertyInfo[] properties = objType
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

            var writtenProperties = new List<PropertyInfo>();

            var classMeta = objType.GetCustomAttribute<EbxClassMetaAttribute>();

            //EbxClass classType = GetClass(objType);
            //var fields = (from index in Enumerable.Range(0, classType.FieldCount)
            //              select GetField(classType, classType.FieldIndex + index) into field
            //              where field.DebugType != EbxFieldType.Inherited
            //              orderby field.DataOffset
            //              select field).ToList();

            //var fields2 = properties
            //    .Where(x => x.GetCustomAttribute<HashAttribute>() != null && x.GetCustomAttribute<FieldIndexAttribute>() != null)
            //    .OrderBy(x => x.GetCustomAttribute<EbxFieldMetaAttribute>().Offset)
            //    .Select(x => EbxReader22A.GetEbxFieldByProperty(classType, x))
            //    //.OrderBy(x => x.DataOffset)
            //    .ToList();

            var propertyIndex = -1;
            //foreach (EbxField field in fields)
            //foreach (EbxField field in fields2)
            foreach(var propertyInfo in properties.OrderBy(x=>x.GetCustomAttribute<EbxFieldMetaAttribute>().Offset))
            {
                IsTransientAttribute isTransientAttribute = propertyInfo.GetCustomAttribute<IsTransientAttribute>();
                if (isTransientAttribute != null)
                    continue;

                EbxFieldMetaAttribute ebxFieldMetaAttribute = propertyInfo.GetCustomAttribute<EbxFieldMetaAttribute>();
                if (ebxFieldMetaAttribute == null || ebxFieldMetaAttribute.Type == EbxFieldType.Inherited)
                    continue;
            //{
            //    propertyIndex++;
            //    if (field.DebugType == EbxFieldType.Inherited)
            //    {
            //        continue;
            //    }
            //    PropertyInfo propertyInfo = Array.Find(properties, (PropertyInfo p) => (uint?)p.GetCustomAttribute<HashAttribute>()?.Hash == (uint?)field.NameHash);

                if (propertyInfo == null)
                {
                    //AssetManager.Instance.LogError($"EBX Writing: Unable to write field {field.NameHash} of class {obj.GetType().Name}");
                    continue;
                }
                else
                {
                    writtenProperties.Add(propertyInfo);
                    //EbxFieldMetaAttribute ebxFieldMetaAttribute = propertyInfo.GetCustomAttribute<EbxFieldMetaAttribute>();
                    bool isReference = propertyInfo.GetCustomAttribute<IsReferenceAttribute>() != null;
                    if (ebxFieldMetaAttribute.IsArray)
                    {
                        uint fieldNameHash = propertyInfo.GetCustomAttribute<HashAttribute>()!.Hash;
                        WriteArray(propertyInfo.GetValue(obj), ebxFieldMetaAttribute.ArrayType, fieldNameHash, classMeta.Alignment, writer, isReference);
                        //WriteArray(propertyInfo.Name, propertyInfo.GetValue(obj), ebxFieldMetaAttribute.ArrayType, (int)fieldNameHash, classType, classType.Alignment, writer, isReference, dataContainerIndex, propertyIndex);
                    }
                    else
                    {
                        WriteField(propertyInfo.GetValue(obj), ebxFieldMetaAttribute.Type, classMeta.Alignment, writer, isReference);
                        //WriteField(propertyInfo.GetValue(obj), ebxFieldMetaAttribute.Type, classType.Alignment, writer, isReference, dataContainerIndex);
                    }
                }
            }

            var unwrittenProperties = properties.Where(x => !writtenProperties.Any(y => y.Name == x.Name));
            if (unwrittenProperties.Any() && obj == objsToProcess[0])
            {

            }
            writer.WritePadding(classMeta.Alignment);
            //while(writer.Position < startPosition + classType.Size)
            //{
            //    writer.Write((byte)0);
            //}
        }

    }
}