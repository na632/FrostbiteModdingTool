// Sdk.IO.EbxWriterRiff
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.IO._2022.Readers;

namespace Frostbite.IO
{

    public class EbxWriter2023 : EbxBaseWriter
    {
        private class PropertyComparer : IComparer<PropertyInfo>
        {
            public int Compare(PropertyInfo x, PropertyInfo y)
            {
                if (x == null)
                {
                    throw new ArgumentNullException("x");
                }
                if (y == null)
                {
                    throw new ArgumentNullException("y");
                }
                int xFieldIndex = x.GetCustomAttribute<FieldIndexAttribute>()?.Index ?? (-1);
                int yFieldIndex = y.GetCustomAttribute<FieldIndexAttribute>()?.Index ?? (-1);
                return xFieldIndex.CompareTo(yFieldIndex);
            }
        }

        private class EbxImportReferenceComparer : IComparer<EbxImportReference>
        {
            public int Compare(EbxImportReference x, EbxImportReference y)
            {
                int fileGuidComparison = FirstFourBytesGuidComparer.Instance.Compare(x.FileGuid, y.FileGuid);
                if (fileGuidComparison != 0)
                {
                    return fileGuidComparison;
                }
                return FirstFourBytesGuidComparer.Instance.Compare(x.ClassGuid, y.ClassGuid);
            }
        }

        private class FirstFourBytesGuidComparer : IComparer<Guid>
        {
            public static FirstFourBytesGuidComparer Instance { get; } = new FirstFourBytesGuidComparer();


            public int Compare(Guid x, Guid y)
            {
                Span<byte> guidBytes = stackalloc byte[16];
                x.TryWriteBytes(guidBytes);
                uint xVal = (uint)((guidBytes[0] << 24) | (guidBytes[1] << 16) | (guidBytes[2] << 8) | guidBytes[3]);
                y.TryWriteBytes(guidBytes);
                uint yVal = (uint)((guidBytes[0] << 24) | (guidBytes[1] << 16) | (guidBytes[2] << 8) | guidBytes[3]);
                return xVal.CompareTo(yVal);
            }
        }

        private readonly List<object> objsToProcess = new List<object>();

        private readonly List<Type> typesToProcess = new List<Type>();

        private readonly List<EbxFieldMetaAttribute> arrayTypes = new List<EbxFieldMetaAttribute>();

        private readonly List<object> objs = new List<object>();

        private readonly List<object> sortedObjs = new List<object>();

        private readonly List<Guid> dependencies = new List<Guid>();

        private readonly List<EbxClass> classTypes = new List<EbxClass>();

        private readonly List<Guid> classGuids = new List<Guid>();

        private readonly List<uint> typeInfoSignatures = new List<uint>();

        private readonly List<EbxField> fieldTypes = new List<EbxField>();

        private readonly List<string> typeNames = new List<string>();

        private readonly List<EbxImportReference> imports = new List<EbxImportReference>();

        private readonly List<EbxInstance> instances = new List<EbxInstance>();

        private readonly List<EbxArray> arrays = new List<EbxArray>();

        private readonly List<byte[]> arrayData = new List<byte[]>();

        private readonly List<int> dataContainerOffsets = new List<int>();

        private readonly List<(int pointerOffset, int arrayIndex)> pointerOffsets = new List<(int, int)>();

        private readonly List<int> patchedPointerOffsets = new List<int>();

        private readonly List<(int resourceRefOffset, int arrayIndex)> resourceRefOffsets = new List<(int, int)>();

        private readonly List<int> patchedResourceRefOffsets = new List<int>();

        private readonly List<(int importOffset, int arrayIndex)> importOffsets = new List<(int, int)>();

        private readonly List<int> patchedImportOffsets = new List<int>();

        private readonly List<(int typeImportOffset, int arrayIndex)> typeInfoOffsets = new List<(int, int)>();

        private readonly List<int> patchedTypeInfoOffsets = new List<int>();

        private readonly Dictionary<string, List<(int offset, int containingArrayIndex)>> stringsToCStringOffsets = new Dictionary<string, List<(int, int)>>();

        private readonly Dictionary<(int offset, int containingArrayIndex), int> pointerRefPositionToDataContainerIndex = new Dictionary<(int, int), int>();

        private readonly List<(int arrayOffset, int arrayIndex, int containingArrayIndex, int dataContainerIndex, int propertyIndex, string propertyName)> unpatchedArrayInfo = new List<(int, int, int, int, int, string)>();

        private readonly List<int> arrayIndicesMap = new List<int>();

        private List<EbxArray> originalAssetArrays;

        private long payloadPosition;

        private int arraysPosition;

        private int boxedValuesPosition;

        private int stringTablePosition;

        private int currentArrayIndex = -1;

        private int currentArrayDepth;

        private byte[] mainData;

        private ushort uniqueClassCount;

        private int exportedCount;

        public EbxWriter2023(Stream ebxOutputStream, EbxWriteFlags writeFlags = EbxWriteFlags.None, bool wideEncoding = false)
            : base(ebxOutputStream, writeFlags, wideEncoding)
        {
        }

        public override void WriteAsset(EbxAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException("asset");
            }
            originalAssetArrays = asset.arrays;
            WriteEbxObjects(asset.RootObjects, asset.FileGuid);
        }

        public void WriteEbxObjects(IEnumerable<object> objects, Guid fileGuid)
        {
            if (objects == null)
            {
                throw new ArgumentNullException("objects");
            }
            Queue<object> queue = new Queue<object>();
            foreach (object @object in objects)
            {
                queue.Enqueue(@object);
            }
            while (queue.Count > 0)
            {
                object obj = queue.Dequeue();
                foreach (object extractedObj in ExtractClass(obj.GetType(), obj))
                {
                    queue.Enqueue(extractedObj);
                }
            }
            imports.Sort(new EbxImportReferenceComparer());
            WriteEbx(fileGuid);
        }

        private IEnumerable<object> ExtractClass(Type type, object obj, bool add = true)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (add)
            {
                if (objsToProcess.Contains(obj))
                {
                    return Enumerable.Empty<object>();
                }
                objsToProcess.Add(obj);
                objs.Add(obj);
            }
            PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            List<object> dataContainers = new List<object>();
            PropertyInfo[] array = properties;
            foreach (PropertyInfo propertyInfo in array)
            {
                if (!flags.HasFlag(EbxWriteFlags.IncludeTransient) && propertyInfo.GetCustomAttribute<IsTransientAttribute>() != null)
                {
                    continue;
                }
                if (propertyInfo.PropertyType == typeof(PointerRef))
                {
                    PointerRef pointerRef2 = (PointerRef)propertyInfo.GetValue(obj);
                    if (pointerRef2.Type == PointerRefType.Internal)
                    {
                        dataContainers.Add(pointerRef2.Internal);
                    }
                    else if (pointerRef2.Type == PointerRefType.External && !imports.Contains(pointerRef2.External))
                    {
                        imports.Add(pointerRef2.External);
                    }
                }
                else if (propertyInfo.PropertyType.Namespace == "Sdk.Ebx" && propertyInfo.PropertyType.BaseType != typeof(Enum))
                {
                    object value = propertyInfo.GetValue(obj);
                    dataContainers.AddRange(ExtractClass(value.GetType(), value, add: false));
                }
                else
                {
                    if (propertyInfo.PropertyType.Name != "List`1")
                    {
                        continue;
                    }
                    Type propertyType = propertyInfo.PropertyType;
                    IList typedArrayObject = (IList)propertyInfo.GetValue(obj);
                    int arrayCount = typedArrayObject.Count;
                    if (arrayCount <= 0)
                    {
                        continue;
                    }
                    if (typedArrayObject is List<PointerRef> pointerRefArray)
                    {
                        for (int i = 0; i < arrayCount; i++)
                        {
                            PointerRef pointerRef = pointerRefArray[i];
                            if (pointerRef.Type == PointerRefType.Internal)
                            {
                                dataContainers.Add(pointerRef.Internal);
                            }
                            else if (pointerRef.Type == PointerRefType.External && !imports.Contains(pointerRef.External))
                            {
                                imports.Add(pointerRef.External);
                            }
                        }
                    }
                    else if (propertyType.GenericTypeArguments[0].Namespace == "Sdk.Ebx" && propertyType.GenericTypeArguments[0].BaseType != typeof(Enum))
                    {
                        for (int j = 0; j < arrayCount; j++)
                        {
                            object arrayElement = typedArrayObject[j];
                            dataContainers.AddRange(ExtractClass(arrayElement.GetType(), arrayElement, add: false));
                        }
                    }
                }
            }
            if (type.BaseType != typeof(object) && type.BaseType != typeof(ValueType))
            {
                dataContainers.AddRange(ExtractClass(type.BaseType, obj, add: false));
            }
            return dataContainers;
        }

        private void WriteEbx(Guid fileGuid)
        {
            foreach (object item in objsToProcess)
            {
                ProcessClass(item.GetType(), item);
            }
            for (int j = 0; j < typesToProcess.Count; j++)
            {
                ProcessType(j);
            }
            ProcessData();
            WriteInt32LittleEndian(1179011410);
            long riffChunkDataLengthOffset = base.Position;
            WriteUInt32LittleEndian(0u);
            WriteUInt32LittleEndian(5784133u);
            WriteUInt32LittleEndian(1146634821u);
            long ebxdChunkDataLengthOffset = base.Position;
            WriteUInt32LittleEndian(0u);
            WritePadding(16);
            payloadPosition = base.Position;
            WriteBytes(mainData);
            long ebxdChunkLength = base.Position - ebxdChunkDataLengthOffset - 4;
            WritePadding(2);
            WriteUInt32LittleEndian(1481197125u);
            long efixChunkDataLengthOffset = base.Position;
            WriteUInt32LittleEndian(0u);
            WriteGuid(fileGuid);
            WriteInt32LittleEndian(classGuids.Count);
            foreach (Guid classGuid in classGuids)
            {
                WriteGuid(classGuid);
            }
            WriteInt32LittleEndian(typeInfoSignatures.Count);
            foreach (uint signature in typeInfoSignatures)
            {
                WriteUInt32LittleEndian(signature);
            }
            WriteInt32LittleEndian(exportedCount);
            WriteInt32LittleEndian(instances.Count);
            for (int i = 0; i < instances.Count; i++)
            {
                WriteInt32LittleEndian(dataContainerOffsets[i]);
            }
            patchedPointerOffsets.Sort();
            WriteInt32LittleEndian(patchedPointerOffsets.Count);
            foreach (int pointerOffset in patchedPointerOffsets)
            {
                WriteInt32LittleEndian(pointerOffset);
            }
            WriteInt32LittleEndian(patchedResourceRefOffsets.Count);
            foreach (int resourceRefOffset in patchedResourceRefOffsets)
            {
                WriteInt32LittleEndian(resourceRefOffset);
            }
            WriteInt32LittleEndian(imports.Count);
            foreach (EbxImportReference import in imports)
            {
                WriteGuid(import.FileGuid);
                WriteGuid(import.ClassGuid);
            }
            WriteInt32LittleEndian(patchedImportOffsets.Count);
            foreach (int importOffset in patchedImportOffsets)
            {
                WriteInt32LittleEndian(importOffset);
            }
            WriteInt32LittleEndian(patchedTypeInfoOffsets.Count);
            foreach (int typeInfoOffset in patchedTypeInfoOffsets)
            {
                WriteInt32LittleEndian(typeInfoOffset);
            }
            WriteInt32LittleEndian(arraysPosition);
            WriteInt32LittleEndian(boxedValuesPosition);
            WriteInt32LittleEndian(stringTablePosition);
            long efixChunkLength = base.Position - efixChunkDataLengthOffset - 4;
            WriteUInt32LittleEndian(1482179141u);
            int ebxxSize = 8;
            WriteInt32LittleEndian(ebxxSize);
            WriteInt32LittleEndian(0);
            WriteInt32LittleEndian(0);
            long riffChunkLength = base.Position - riffChunkDataLengthOffset - 4;
            base.Position = riffChunkDataLengthOffset;
            WriteUInt32LittleEndian((uint)riffChunkLength);
            base.Position = ebxdChunkDataLengthOffset;
            WriteUInt32LittleEndian((uint)ebxdChunkLength);
            base.Position = efixChunkDataLengthOffset;
            WriteUInt32LittleEndian((uint)efixChunkLength);
        }

        private ushort ProcessClass(Type type, object obj, bool isBaseType = false)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (obj == null)
            {
                return 0;
            }
            if (type.BaseType.Namespace == "Sdk.Ebx")
            {
                ProcessClass(type.BaseType, obj, isBaseType: true);
            }
            int classIndex = FindExistingClass(type);
            if (classIndex != -1)
            {
                return (ushort)classIndex;
            }
            type.GetCustomAttribute<EbxClassMetaAttribute>();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            List<PropertyInfo> propertiesToInclude = new List<PropertyInfo>();
            PropertyInfo[] array = properties;
            foreach (PropertyInfo propertyInfo in array)
            {
                if (propertyInfo.GetCustomAttribute<IsTransientAttribute>() == null || flags.HasFlag(EbxWriteFlags.IncludeTransient))
                {
                    propertiesToInclude.Add(propertyInfo);
                }
            }
            if (!isBaseType)
            {
                classIndex = AddClass(type.Name, type);
            }
            if (!type.IsEnum)
            {
                foreach (PropertyInfo item in propertiesToInclude)
                {
                    EbxFieldMetaAttribute ebxFieldMetaAttribute = item.GetCustomAttribute<EbxFieldMetaAttribute>();
                    switch (ebxFieldMetaAttribute.Type)
                    {
                        case EbxFieldType.Struct:
                            ProcessClass(item.PropertyType, item.GetValue(obj), isBaseType);
                            break;
                        case EbxFieldType.Array:
                            {
                                Type elementType = item.PropertyType.GenericTypeArguments[0];
                                if (FindExistingClass(elementType) != -1)
                                {
                                    break;
                                }
                                IList typedArray = (IList)item.GetValue(obj);
                                if (typedArray.Count == 0)
                                {
                                    break;
                                }
                                arrayTypes.Add(ebxFieldMetaAttribute);
                                if (ebxFieldMetaAttribute.ArrayType != EbxFieldType.Struct)
                                {
                                    break;
                                }
                                foreach (object arrayItem in typedArray)
                                {
                                    ProcessClass(elementType, arrayItem, isBaseType);
                                }
                                break;
                            }
                    }
                }
            }
            return (ushort)classIndex;
        }

        private void ProcessType(int index)
        {
            Type type = typesToProcess[index];
            EbxClass ebxClass = classTypes[index];
            if (type.Name == "List`1" || ebxClass.DebugType == EbxFieldType.Array)
            {
                EbxFieldMetaAttribute ebxFieldMetaAttribute = arrayTypes[0];
                arrayTypes.RemoveAt(0);
                ushort classIndex = (ushort)FindExistingClass(type.GenericTypeArguments[0]);
                if (classIndex == ushort.MaxValue)
                {
                    classIndex = 0;
                }
                AddField("member", ebxFieldMetaAttribute.ArrayFlags, classIndex, 0u, 0u);
                return;
            }
            if (ebxClass.DebugType == EbxFieldType.Enum)
            {
                string[] enumNames = type.GetEnumNames();
                Array enumValues = type.GetEnumValues();
                for (int i = 0; i < enumNames.Length; i++)
                {
                    int enumValue = (int)enumValues.GetValue(i);
                    AddField(enumNames[i], 0, 0, (uint)enumValue, (uint)enumValue);
                }
                return;
            }
            if (type.BaseType != typeof(object) && type.BaseType != typeof(ValueType))
            {
                ushort classRef = (ushort)FindExistingClass(type.BaseType);
                AddField("$", 0, classRef, 8u, 0u);
            }
            foreach (PropertyInfo item in from property in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                                          where (flags.HasFlag(EbxWriteFlags.IncludeTransient) || property.GetCustomAttribute<IsTransientAttribute>() == null) && !property.Name.Equals("__InstanceGuid", StringComparison.Ordinal)
                                          select property)
            {
                ProcessField(item);
            }
        }

        private void ProcessField(PropertyInfo pi)
        {
            if (pi == null)
            {
                throw new ArgumentNullException("pi");
            }
            EbxFieldMetaAttribute ebxFieldMetaAttribute = pi.GetCustomAttribute<EbxFieldMetaAttribute>();
            Type propType = pi.PropertyType;
            ushort classIndex = (ushort)typesToProcess.FindIndex((Type value) => value == propType);
            if (classIndex == ushort.MaxValue)
            {
                classIndex = 0;
            }
            AddField(pi.Name, ebxFieldMetaAttribute.Flags, classIndex, ebxFieldMetaAttribute.Offset, 0u);
        }

        private void ProcessData()
        {
            HashSet<Type> uniqueTypes = new HashSet<Type>();
            List<object> exportedInstances = new List<object>();
            List<object> nonExportedInstances = new List<object>();
            foreach (dynamic item in objs)
            {
                if (((AssetClassGuid)item.GetInstanceGuid()).IsExported)
                {
                    exportedInstances.Add(item);
                }
                else
                {
                    nonExportedInstances.Add(item);
                }
            }
            object primaryInstance = exportedInstances[0];
            exportedInstances.RemoveAt(0);
            exportedInstances.Sort(delegate (dynamic a, dynamic b)
            {
                AssetClassGuid assetClassGuid2 = a.GetInstanceGuid();
                AssetClassGuid assetClassGuid3 = b.GetInstanceGuid();
                return FirstFourBytesGuidComparer.Instance.Compare(assetClassGuid2.ExportedGuid, assetClassGuid3.ExportedGuid);
            });
            nonExportedInstances.Sort((object a, object b) => string.CompareOrdinal(a.GetType().Name, b.GetType().Name));
            sortedObjs.Add(primaryInstance);
            sortedObjs.AddRange(exportedInstances);
            sortedObjs.AddRange(nonExportedInstances);
            MemoryStream memoryStream = new MemoryStream();
            FileWriter nativeWriter = new FileWriter(memoryStream);
            for (int i = 0; i < sortedObjs.Count; i++)
            {
                AssetClassGuid assetClassGuid = ((dynamic)sortedObjs[i]).GetInstanceGuid();
                Type type = sortedObjs[i].GetType();
                int classIndex = FindExistingClass(type);
                EbxClass ebxClass = classTypes[classIndex];
                if (!uniqueTypes.Contains(type))
                {
                    uniqueTypes.Add(type);
                }
                nativeWriter.WritePadding(ebxClass.Alignment);
                //DataContainerFlags flags = DataContainerFlags.ObjectFlag_ReadOnly | DataContainerFlags.ObjectFlag_Aggregated;
                if (assetClassGuid.IsExported)
                {
                    nativeWriter.WriteGuid(assetClassGuid.ExportedGuid);
                    //flags |= DataContainerFlags.ObjectFlag_Exported | DataContainerFlags.ObjectFlag_HasGuid;
                }
                dataContainerOffsets.Add((int)nativeWriter.Position);
                nativeWriter.WriteInt64LittleEndian(classIndex);
                nativeWriter.WriteInt64LittleEndian(0L);
                nativeWriter.WriteUInt32LittleEndian(2u);
                nativeWriter.WriteUInt16LittleEndian((ushort)flags);
                nativeWriter.WriteUInt16LittleEndian(0);
                object obj = sortedObjs[i];
                List<int> list = dataContainerOffsets;
                WriteClass(obj, type, nativeWriter, list[list.Count - 1], i);
                EbxInstance ebxInstance = default(EbxInstance);
                ebxInstance.ClassRef = (ushort)classIndex;
                ebxInstance.Count = 1;
                ebxInstance.IsExported = assetClassGuid.IsExported;
                instances.Add(ebxInstance);
                exportedCount += (ushort)(ebxInstance.IsExported ? 1 : 0);
            }
            nativeWriter.WritePadding(16);
            arraysPosition = (int)nativeWriter.Position;
            nativeWriter.WriteEmpty(32);
            foreach (var unpatchedArray2 in from t in unpatchedArrayInfo
                                            orderby t.dataContainerIndex, t.propertyIndex
                                            select t)
            {
                EbxArray arrayInfo = arrays[unpatchedArray2.arrayIndex];
                byte[] arrayData = this.arrayData[unpatchedArray2.arrayIndex];
                if (arrayInfo.Count == 0)
                {
                    uint offsetFromPayload = (arrayInfo.Offset = (uint)(arraysPosition + 16));
                }
                else
                {
                    long beforePaddingPosition = nativeWriter.Position;
                    nativeWriter.WritePadding(16);
                    if (nativeWriter.Position - beforePaddingPosition < 4)
                    {
                        nativeWriter.WriteEmpty(16);
                    }
                    nativeWriter.Position -= 4L;
                    nativeWriter.WriteUInt32LittleEndian(arrayInfo.Count);
                    long beforeArrayPosition = nativeWriter.Position;
                    nativeWriter.WriteBytes(arrayData);
                    arrayInfo.Offset = (uint)beforeArrayPosition;
                }
                arrays[unpatchedArray2.arrayIndex] = arrayInfo;
            }
            nativeWriter.WritePadding(16);
            boxedValuesPosition = (int)nativeWriter.Position;
            nativeWriter.WritePadding(16);
            stringTablePosition = (int)nativeWriter.Position;
            foreach (KeyValuePair<string, List<(int, int)>> stringsToCStringOffset in stringsToCStringOffsets)
            {
                stringsToCStringOffset.Deconstruct(out var key, out var value);
                string stringValue = key;
                List<(int, int)> list2 = value;
                long stringPosition = nativeWriter.Position;
                nativeWriter.WriteNullTerminatedString(stringValue);
                long afterStringPosition = nativeWriter.Position;
                foreach (var (cStringOffset, containingArrayIndex6) in list2)
                {
                    if (containingArrayIndex6 == -1)
                    {
                        nativeWriter.Position = cStringOffset;
                    }
                    else
                    {
                        int realArrayIndex7 = arrayIndicesMap[containingArrayIndex6];
                        nativeWriter.Position = cStringOffset + arrays[realArrayIndex7].Offset;
                    }
                    int offset = (int)(stringPosition - nativeWriter.Position);
                    nativeWriter.WriteInt32LittleEndian(offset);
                }
                nativeWriter.Position = afterStringPosition;
            }
            foreach (var unpatchedArray in unpatchedArrayInfo)
            {
                EbxArray ebxArray = arrays[unpatchedArray.arrayIndex];
                var (arrayPointerOffset, _, _, _, _, _) = unpatchedArray;
                if (unpatchedArray.containingArrayIndex > -1)
                {
                    int realArrayIndex6 = arrayIndicesMap[unpatchedArray.containingArrayIndex];
                    nativeWriter.Position = arrays[realArrayIndex6].Offset + arrayPointerOffset;
                }
                else
                {
                    nativeWriter.Position = arrayPointerOffset;
                }
                int relativeOffset = (int)(ebxArray.Offset - nativeWriter.Position);
                nativeWriter.WriteInt32LittleEndian(relativeOffset);
            }
            foreach (KeyValuePair<(int, int), int> item2 in pointerRefPositionToDataContainerIndex)
            {
                item2.Deconstruct(out var key2, out var value2);
                (int, int) tuple3 = key2;
                int pointerRefPosition = tuple3.Item1;
                int containingArrayIndex5 = tuple3.Item2;
                int dataContainerIndex = value2;
                if (containingArrayIndex5 == -1)
                {
                    nativeWriter.Position = pointerRefPosition;
                }
                else
                {
                    int realArrayIndex5 = arrayIndicesMap[containingArrayIndex5];
                    nativeWriter.Position = pointerRefPosition + arrays[realArrayIndex5].Offset;
                }
                nativeWriter.WriteInt32LittleEndian((int)(dataContainerOffsets[dataContainerIndex] - nativeWriter.Position));
            }
            foreach (var (pointerOffset, containingArrayIndex4) in pointerOffsets)
            {
                if (containingArrayIndex4 == -1)
                {
                    patchedPointerOffsets.Add(pointerOffset);
                    continue;
                }
                int realArrayIndex4 = arrayIndicesMap[containingArrayIndex4];
                patchedPointerOffsets.Add((int)(pointerOffset + arrays[realArrayIndex4].Offset));
            }
            foreach (var (resourceRefOffset, containingArrayIndex3) in resourceRefOffsets)
            {
                if (containingArrayIndex3 == -1)
                {
                    patchedResourceRefOffsets.Add(resourceRefOffset);
                    continue;
                }
                int realArrayIndex3 = arrayIndicesMap[containingArrayIndex3];
                patchedResourceRefOffsets.Add((int)(resourceRefOffset + arrays[realArrayIndex3].Offset));
            }
            foreach (var (importOffset, containingArrayIndex2) in importOffsets)
            {
                if (containingArrayIndex2 == -1)
                {
                    patchedImportOffsets.Add(importOffset);
                    continue;
                }
                int realArrayIndex2 = arrayIndicesMap[containingArrayIndex2];
                patchedImportOffsets.Add((int)(importOffset + arrays[realArrayIndex2].Offset));
            }
            foreach (var (typeRefOffset, containingArrayIndex) in typeInfoOffsets)
            {
                if (containingArrayIndex == -1)
                {
                    patchedTypeInfoOffsets.Add(typeRefOffset);
                    continue;
                }
                int realArrayIndex = arrayIndicesMap[containingArrayIndex];
                patchedTypeInfoOffsets.Add((int)(typeRefOffset + arrays[realArrayIndex].Offset));
            }
            mainData = memoryStream.ToArray();
            uniqueClassCount = (ushort)uniqueTypes.Count;
        }

        private void WriteClass(object obj, Type objType, FileWriter writer, int startOfDataContainer, int dataContainerIndex)
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
            if (objType.BaseType.Namespace == "Sdk.Ebx")
            {
                WriteClass(obj, objType.BaseType, writer, startOfDataContainer, dataContainerIndex);
            }
            PropertyInfo[] properties = objType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            Array.Sort(properties, new PropertyComparer());
            EbxClass classType = GetClass(objType);
            foreach (var item in from index in Enumerable.Range(0, classType.FieldCount)
                                 select (index, GetField(classType, classType.FieldIndex + index)) into f
                                 orderby f.Item2.DataOffset
                                 select f)
            {
                EbxField field = item.Item2;
                if (field.DebugType == EbxFieldType.Inherited)
                {
                    continue;
                }
                int propertyInfoIndex = Array.FindIndex(properties, (PropertyInfo p) => p.GetCustomAttribute<HashAttribute>()?.Hash == (int?)field.NameHash);
                PropertyInfo propertyInfo = ((propertyInfoIndex == -1) ? null : properties[propertyInfoIndex]);
                long currentOffset = writer.Position - startOfDataContainer;
                if (currentOffset < 0)
                {
                }
                else if (currentOffset > field.DataOffset)
                {
                }
                else if (currentOffset < field.DataOffset)
                {
                    int adjustment = (int)(field.DataOffset - currentOffset);
                    int adjustmentByPaddingTo8 = (int)(8 - currentOffset % 8);
                    if (adjustment != adjustmentByPaddingTo8)
                    {
                    }
                    writer.WriteEmpty(adjustment);
                }
                if (propertyInfo == null)
                {
                    EbxClass unpatchedClassType = GetClassUnpatched(objType);
                    for (int i = 0; i < unpatchedClassType.FieldCount; i++)
                    {
                        EbxField unpatchedField = GetField(unpatchedClassType, unpatchedClassType.FieldIndex + i);
                        if (unpatchedField.DataOffset == field.DataOffset)
                        {
                            propertyInfoIndex = Array.FindIndex(properties, (PropertyInfo p) => p.GetCustomAttribute<HashAttribute>()?.Hash == (int?)unpatchedField.NameHash);
                            propertyInfo = ((propertyInfoIndex == -1) ? null : properties[propertyInfoIndex]);
                            break;
                        }
                    }
                }
                if (propertyInfo == null)
                {
                    switch (field.DebugType)
                    {
                        case EbxFieldType.TypeRef:
                            writer.WriteUInt64LittleEndian(0uL);
                            break;
                        case EbxFieldType.FileRef:
                            writer.WriteUInt64LittleEndian(0uL);
                            break;
                        case EbxFieldType.CString:
                            writer.WriteInt32LittleEndian(0);
                            break;
                        case EbxFieldType.Pointer:
                            writer.WriteInt32LittleEndian(0);
                            break;
                        case EbxFieldType.Struct:
                            {
                                EbxClass value = GetClass(classType, field);
                                writer.WritePadding(value.Alignment);
                                writer.WriteEmpty(value.Size);
                                break;
                            }
                        case EbxFieldType.Array:
                            writer.WriteInt32LittleEndian(0);
                            break;
                        case EbxFieldType.Enum:
                            writer.WriteInt32LittleEndian(0);
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
                            writer.WriteUInt16LittleEndian(0);
                            break;
                        case EbxFieldType.Int32:
                            writer.WriteInt32LittleEndian(0);
                            break;
                        case EbxFieldType.UInt32:
                            writer.WriteUInt32LittleEndian(0u);
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
                            writer.WriteFixedSizedString(string.Empty, 32);
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
                    EbxFieldMetaAttribute ebxFieldMetaAttribute = propertyInfo.GetCustomAttribute<EbxFieldMetaAttribute>();
                    bool isReference = propertyInfo.GetCustomAttribute<IsReferenceAttribute>() != null;
                    if (ebxFieldMetaAttribute.IsArray)
                    {
                        int fieldNameHash = propertyInfo.GetCustomAttribute<HashAttribute>().Hash;
                        WriteArray(propertyInfo.Name, propertyInfo.GetValue(obj), ebxFieldMetaAttribute.ArrayType, fieldNameHash, classType, classType.Alignment, writer, isReference, dataContainerIndex, propertyInfoIndex);
                    }
                    else
                    {
                        WriteField(propertyInfo.GetValue(obj), ebxFieldMetaAttribute.Type, classType.Alignment, writer, isReference, dataContainerIndex);
                    }
                }
            }
            writer.WritePadding(classType.Alignment);
        }

        private void WriteField(object obj, EbxFieldType ebxType, byte classAlignment, FileWriter writer, bool isReference, int dataContainerIndex)
        {
            switch (ebxType)
            {
                case EbxFieldType.TypeRef:
                    writer.WriteInt64LittleEndian(uint.Parse((TypeRef)obj));
                    break;
                case EbxFieldType.FileRef:
                    throw new NotSupportedException("The asset contains a FileRef field, which is not yet supported.");
                case EbxFieldType.CString:
                    pointerOffsets.Add(((int)writer.Position, currentArrayIndex));
                    AddString((CString)obj, (int)writer.Position);
                    writer.WriteUInt32LittleEndian(0u);
                    writer.WritePadding(8);
                    break;
                case EbxFieldType.Pointer:
                    {
                        PointerRef pointer = (PointerRef)obj;
                        uint pointerRefValue = 0u;
                        if (pointer.Type == PointerRefType.External)
                        {
                            int importIndex = imports.FindIndex((EbxImportReference value) => value == pointer.External);
                            pointerRefValue = (uint)(importIndex << 1) | 1u;
                            if (isReference && !dependencies.Contains(imports[importIndex].FileGuid))
                            {
                                dependencies.Add(imports[importIndex].FileGuid);
                            }
                            importOffsets.Add(((int)writer.Position, currentArrayIndex));
                        }
                        else if (pointer.Type == PointerRefType.Internal)
                        {
                            pointerRefValue = (uint)sortedObjs.FindIndex((object value) => value == pointer.Internal);
                            pointerRefPositionToDataContainerIndex[((int)writer.Position, currentArrayIndex)] = (int)pointerRefValue;
                            pointerOffsets.Add(((int)writer.Position, currentArrayIndex));
                        }
                        writer.WriteUInt32LittleEndian(pointerRefValue);
                        writer.WritePadding(8);
                        break;
                    }
                case EbxFieldType.Struct:
                    {
                        Type structObjType = obj.GetType();
                        int existingClassIndex = FindExistingClass(structObjType);
                        byte alignment = ((existingClassIndex == -1) ? structObjType.GetCustomAttribute<EbxClassMetaAttribute>().Alignment : classTypes[existingClassIndex].Alignment);
                        writer.WritePadding(alignment);
                        WriteClass(obj, structObjType, writer, (int)writer.Position, dataContainerIndex);
                        break;
                    }
                case EbxFieldType.Enum:
                    writer.WriteInt32LittleEndian((int)obj);
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
                    writer.WriteUInt16LittleEndian((ushort)obj);
                    break;
                case EbxFieldType.Int32:
                    writer.WriteInt32LittleEndian((int)obj);
                    break;
                case EbxFieldType.UInt32:
                    writer.WriteUInt32LittleEndian((uint)obj);
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
                    resourceRefOffsets.Add(((int)writer.Position, currentArrayIndex));
                    writer.WriteUInt64LittleEndian((ResourceRef)obj);
                    break;
                default:
                    {
                        DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
                        defaultInterpolatedStringHandler.AppendLiteral("Unknown field type ");
                        defaultInterpolatedStringHandler.AppendFormatted(ebxType);
                        throw new InvalidDataException(defaultInterpolatedStringHandler.ToStringAndClear());
                    }
            }
        }

        private void WriteArray(string propertyName, object obj, EbxFieldType elementFieldType, int fieldNameHash, EbxClass classType, byte classAlignment, FileWriter writer, bool isReference, int dataContainerIndex, int propertyIndex)
        {
            int classIndex = typesToProcess.FindIndex((Type item) => item == obj.GetType().GetGenericArguments()[0]);
            if (classIndex == -1)
            {
                classIndex = 65535;
            }
            IList typedObj = (IList)obj;
            int arrayCount = typedObj.Count;
            MemoryStream arrayMemoryStream = new MemoryStream();
            FileWriter arrayWriter = new FileWriter(arrayMemoryStream);
            int previousArrayIndex = currentArrayIndex;
            int localArrayIndex = (currentArrayIndex = arrayIndicesMap.Count);
            arrayIndicesMap.Add(-1);
            currentArrayDepth++;
            long pointerPosition = writer.Position;
            writer.WriteInt32LittleEndian(0);
            writer.WritePadding(8);
            for (int i = 0; i < arrayCount; i++)
            {
                object arrayElementObj = typedObj[i];
                WriteField(arrayElementObj, elementFieldType, classAlignment, arrayWriter, isReference, dataContainerIndex);
            }
            int arrayIndex = arrays.Count;
            arrays.Add(new EbxArray
            {
                Count = (uint)arrayCount,
                ClassRef = classIndex,
                PathDepth = 0u,
                TypeFlags = 0,
                Offset = 0u
            });
            arrayIndicesMap[localArrayIndex] = arrayIndex;
            unpatchedArrayInfo.Add(((int)pointerPosition, arrayIndex, previousArrayIndex, dataContainerIndex, propertyIndex, propertyName));
            pointerOffsets.Add(((int)pointerPosition, previousArrayIndex));
            arrayData.Add(arrayMemoryStream.ToArray());
            currentArrayDepth--;
            if (currentArrayDepth == 0)
            {
                currentArrayIndex = -1;
            }
            else
            {
                currentArrayIndex = previousArrayIndex;
            }
        }

        protected void AddString(string stringToAdd, int offset)
        {
            if (!stringsToCStringOffsets.TryGetValue(stringToAdd ?? string.Empty, out var offsets))
            {
                offsets = (stringsToCStringOffsets[stringToAdd ?? string.Empty] = new List<(int, int)>());
            }
            offsets.Add((offset, currentArrayIndex));
        }

        private int AddClass(PropertyInfo pi, Type classType)
        {
            GuidAttribute guidAttribute = pi.GetCustomAttribute<GuidAttribute>();
            Guid classGuid;
            EbxClass @class;
            if (guidAttribute != null)
            {
                classGuid = guidAttribute.Guid;
                @class = GetClass(classGuid);
            }
            else
            {
                EbxFieldMetaAttribute fieldMetaAttribute = pi.GetCustomAttribute<EbxFieldMetaAttribute>();
                @class = GetClass(fieldMetaAttribute.BaseType);
                classGuid = fieldMetaAttribute.BaseType.GetCustomAttribute<GuidAttribute>().Guid;
            }
            classTypes.Add(@class);
            typesToProcess.Add(classType);
            classGuids.Add(classGuid);
            return classTypes.Count - 1;
        }

        private int AddClass(string name, Type classType)
        {
            Guid classGuid = classType.GetCustomAttribute<GuidAttribute>().Guid;
            classGuids.Add(classGuid);
            EbxClass @class = GetClass(classType);
            classTypes.Add(@class);
            Span<byte> typeInfoGuidBytes = stackalloc byte[16];
            GetTypeInfoGuid(@class).TryWriteBytes(typeInfoGuidBytes);
            List<uint> list = typeInfoSignatures;
            Span<byte> span = typeInfoGuidBytes;
            list.Add(BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(12, span.Length - 12)));
            AddTypeName(name);
            typesToProcess.Add(classType);
            return classTypes.Count - 1;
        }

        private void AddField(string name, ushort type, ushort classRef, uint dataOffset, uint secondOffset)
        {
            AddTypeName(name);
        }

        private void AddTypeName(string inName)
        {
            if (!typeNames.Contains(inName))
            {
                typeNames.Add(inName);
            }
        }

        private int FindExistingClass(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return typesToProcess.FindIndex((Type value) => value == type);
        }

        internal EbxClass GetClass(Type objType)
        {
            if (objType == null)
            {
                throw new ArgumentNullException("objType");
            }
            EbxClass? ebxClass = null;
            IEnumerable<TypeInfoGuidAttribute> typeInfoGuidAttributes = objType.GetCustomAttributes<TypeInfoGuidAttribute>();
            if (EbxReader22B.patchStd != null)
            {
                foreach (TypeInfoGuidAttribute typeInfoGuidAttribute2 in typeInfoGuidAttributes)
                {
                    ebxClass = EbxReader22B.patchStd.GetClass(typeInfoGuidAttribute2.Guid);
                    if (ebxClass.HasValue)
                    {
                        return ebxClass.Value;
                    }
                }
            }
            foreach (TypeInfoGuidAttribute typeInfoGuidAttribute in typeInfoGuidAttributes)
            {
                ebxClass = EbxReader22B.std.GetClass(typeInfoGuidAttribute.Guid);
                if (ebxClass.HasValue)
                {
                    return ebxClass.Value;
                }
            }
            return ebxClass.Value;
        }

        internal EbxClass GetClassUnpatched(Type objType)
        {
            if (objType == null)
            {
                throw new ArgumentNullException("objType");
            }
            foreach (TypeInfoGuidAttribute typeInfoGuidAttribute in objType.GetCustomAttributes<TypeInfoGuidAttribute>())
            {
                EbxClass? ebxClass = EbxReader22B.std.GetClass(typeInfoGuidAttribute.Guid);
                if (ebxClass.HasValue)
                {
                    return ebxClass.Value;
                }
            }
            throw new ArgumentException("Class not found.");
        }

        internal Guid GetTypeInfoGuid(EbxClass classType)
        {
            if (classType.SecondSize == 0)
            {
                return EbxReader22B.std.GetGuid(classType).Value;
            }
            return EbxReader22B.patchStd.GetGuid(classType).Value;
        }

        internal EbxClass GetClass(Guid guid)
        {
            EbxClass? c = EbxReader22B.std.GetClass(guid);
            if (!c.HasValue)
            {
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
                defaultInterpolatedStringHandler.AppendLiteral("No class with this GUID: ");
                defaultInterpolatedStringHandler.AppendFormatted(guid);
                throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
            }
            return c.Value;
        }

        internal EbxField GetField(EbxClass classType, int index)
        {
            if (classType.SecondSize == 0)
            {
                return EbxReader22B.std.GetField(index).Value;
            }
            return EbxReader22B.patchStd.GetField(index).Value;
        }

        private EbxClass GetClass(EbxClass parentClassType, EbxField field)
        {
            int index = parentClassType.Index + (short)field.ClassRef;
            EbxClass? c = ((parentClassType.SecondSize == 0) ? EbxReader22B.std.GetClass(index) : EbxReader22B.patchStd.GetClass(index));
            if (!c.HasValue)
            {
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
                defaultInterpolatedStringHandler.AppendLiteral("No class with this index: ");
                defaultInterpolatedStringHandler.AppendFormatted(index);
                throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
            }
            return c.Value;
        }
    }
}
