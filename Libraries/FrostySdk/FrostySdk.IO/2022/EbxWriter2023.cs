// Sdk.IO.EbxWriterRiff
using FrostySdk.IO;
using System.IO;


namespace FrostySdk.FrostySdk.IO
{
    //public class EbxWriter2023 : EbxWriterRiff// EbxWriter2022
    public class EbxWriter2023 : EbxWriter2022
    {
        public EbxWriter2023(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.None, bool leaveOpen = false)
            : base(inStream, inFlags, true)
        {
        }

        //        protected override void WriteClass(object obj, Type objType, NativeWriter writer)
        //        {
        //            if (obj == null)
        //            {
        //                throw new ArgumentNullException("obj");
        //            }
        //            if (objType == null)
        //            {
        //                throw new ArgumentNullException("objType");
        //            }
        //            if (writer == null)
        //            {
        //                throw new ArgumentNullException("writer");
        //            }
        //            if (objType.BaseType!.Namespace == "FrostySdk.Ebx")
        //            {
        //                WriteClass(obj, objType.BaseType, writer);
        //            }

        //            PropertyInfo[] properties = objType
        //                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        //#if DEBUG
        //            var writtenProperties = new List<PropertyInfo>();
        //#endif

        //            var classMeta = objType.GetCustomAttribute<EbxClassMetaAttribute>();

        //            foreach(var propertyInfo in properties.OrderBy(x=>x.GetCustomAttribute<EbxFieldMetaAttribute>().Offset))
        //            {
        //                IsTransientAttribute isTransientAttribute = propertyInfo.GetCustomAttribute<IsTransientAttribute>();
        //                if (isTransientAttribute != null)
        //                    continue;

        //                EbxFieldMetaAttribute ebxFieldMetaAttribute = propertyInfo.GetCustomAttribute<EbxFieldMetaAttribute>();
        //                if (ebxFieldMetaAttribute == null || ebxFieldMetaAttribute.Type == EbxFieldType.Inherited)
        //                    continue;

        //                if (propertyInfo == null)
        //                {
        //#if DEBUG
        //                    Debug.WriteLine("There is a dodgy Property in here. How can there be a null property info in a list of property infos?");
        //#endif
        //                    continue;
        //                }
        //                else
        //                {
        //                    writtenProperties.Add(propertyInfo);
        //                    bool isReference = propertyInfo.GetCustomAttribute<IsReferenceAttribute>() != null;
        //                    if (ebxFieldMetaAttribute.IsArray)
        //                    {
        //                        uint fieldNameHash = propertyInfo.GetCustomAttribute<HashAttribute>()!.Hash;
        //                        WriteArray(propertyInfo.GetValue(obj), ebxFieldMetaAttribute.ArrayType, fieldNameHash, classMeta.Alignment, writer, isReference);
        //                    }
        //                    else
        //                    {
        //                        WriteField(propertyInfo.GetValue(obj), ebxFieldMetaAttribute.Type, classMeta.Alignment, writer, isReference);
        //                    }
        //                }
        //            }
        //#if DEBUG
        //            var unwrittenProperties = properties.Where(x => !writtenProperties.Any(y => y.Name == x.Name));
        //            if (unwrittenProperties.Any() && obj == objsToProcess[0])
        //            {

        //            }
        //#endif
        //            writer.WritePadding(classMeta.Alignment);
        //        }

    }
}