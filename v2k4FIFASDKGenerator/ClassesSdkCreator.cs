using FrostyEditor;
using FrostyEditor.IO;
using FrostyEditor.Windows;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using v2k4FIFASDKGenerator.BaseInfo;
using static Frosty.OpenFrostyFiles;
using FieldInfo = v2k4FIFASDKGenerator.BaseInfo.FieldInfo;

namespace v2k4FIFASDKGenerator
{
    public class ClassesSdkCreator
    {
        public static AssetManager AssetManager;

        public static ResourceManager ResourceManager;

        public static FileSystem FileSystem;

        public static EbxAssetEntry SelectedAsset;

        public static string configFilename = "FrostyEditor.ini";


        

        public static long offset;

        private List<ClassInfo> classInfos = new List<ClassInfo>();

        private List<string> alreadyProcessedClasses = new List<string>();

        private Dictionary<long, ClassInfo> offsetClassInfoMapping = new Dictionary<long, ClassInfo>();

        private List<EbxClass> processed = new List<EbxClass>();

        private Dictionary<string, List<EbxField>> fieldMapping;

        private Dictionary<string, Tuple<EbxClass, DbObject>> mapping;

        private List<Tuple<EbxClass, DbObject>> values;

        private DbObject classList;

        private DbObject classMetaList;

        private SdkUpdateState state;

        public ClassesSdkCreator(SdkUpdateState inState)
        {
            state = inState;
        }

        public bool GatherTypeInfos(SdkUpdateTask task)
        {
            Trace.WriteLine("GatherTypeInfos");
            Debug.WriteLine("GatherTypeInfos");
            Console.WriteLine("GatherTypeInfos");

            var executingAssembly = Assembly.GetExecutingAssembly();
            var names = executingAssembly.GetManifestResourceNames();
            //using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FrostyEditor.Classes.txt"))
            //using (Stream stream = executingAssembly.GetManifestResourceStream("v2k4FIFASDKGenerator.Classes.txt"))
            using (FileStream stream = new FileStream("FIFA20.Classes.txt", FileMode.Open))
            {
                if (stream != null)
                {
                    classMetaList = TypeLibrary.LoadClassesSDK(stream);
                }
            }
            
            classList = DumpClasses(task);
            if (classList != null)
            {
                Trace.WriteLine("Classes Dumped");
                Debug.WriteLine("Classes Dumped");
                Console.WriteLine("Classes Dumped");

                return classList.Count > 0;
            }
            return false;
        }

        

        public bool CrossReferenceAssets(SdkUpdateTask task)
        {
            // Data must be loaded and cached before SDK is built

            mapping = new Dictionary<string, Tuple<EbxClass, DbObject>>();
            fieldMapping = new Dictionary<string, List<EbxField>>();
            if (FileSystem.HasFileInMemoryFs("SharedTypeDescriptors.ebx"))
            {
                List<Guid> existingClasses = new List<Guid>();
                LoadSharedTypeDescriptors("SharedTypeDescriptors.ebx", mapping, existingClasses);
                LoadSharedTypeDescriptors("SharedTypeDescriptors_patch.ebx", mapping, existingClasses);
            }
            else
            {
                uint ebxCount = AssetManager.GetEbxCount();
                uint num = 0u;
                foreach (EbxAssetEntry item3 in AssetManager.EnumerateEbx())
                {
                    Stream ebxStream = AssetManager.GetEbxStream(item3);
                    if (ebxStream != null)
                    {
                        task.StatusMessage = $"{(float)(double)(++num) / (float)(double)ebxCount * 100f:0}%";
                        using (EbxReader ebxReader = new EbxReader(ebxStream))
                        {
                            List<EbxClass> classTypes = ebxReader.ClassTypes;
                            List<EbxField> fieldTypes = ebxReader.FieldTypes;
                            foreach (EbxClass item4 in classTypes)
                            {
                                if (item4.Name != "array" && !mapping.ContainsKey(item4.Name))
                                {
                                    DbObject item = null;
                                    int num2 = 0;
                                    foreach (DbObject @class in classList)
                                    {
                                        if (@class.GetValue<string>("name") == item4.Name)
                                        {
                                            item = @class;
                                            classList.RemoveAt(num2);
                                            break;
                                        }
                                        num2++;
                                    }
                                    mapping.Add(item4.Name, new Tuple<EbxClass, DbObject>(item4, item));
                                    fieldMapping.Add(item4.Name, new List<EbxField>());
                                    for (int i = 0; i < item4.FieldCount; i++)
                                    {
                                        EbxField item2 = fieldTypes[item4.FieldIndex + i];
                                        fieldMapping[item4.Name].Add(item2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public bool CreateSDK()
        {
            DbObject dbObject = new DbObject(bObject: false);
            values = mapping.Values.ToList();
            values.Sort((Tuple<EbxClass, DbObject> a, Tuple<EbxClass, DbObject> b) => a.Item1.Name.CompareTo(b.Item1.Name));

            Debug.WriteLine("Creating SDK");

            for (int i = 0; i < values.Count; i++)
            {
                Tuple<EbxClass, DbObject> tuple = values[i];
                EbxClass item = tuple.Item1;
                DbObject item2 = tuple.Item2;
                if (item2 != null)
                {
                    int num = (item.DebugType == EbxFieldType.Pointer) ? 8 : 0;
                    int fieldIndex = 0;
                    ProcessClass(item, item2, fieldMapping[item.Name], dbObject, ref num, ref fieldIndex);
                }
            }
            List<DbObject> list = new List<DbObject>();
            foreach (DbObject @class in classList)
            {
                if (!fieldMapping.ContainsKey(@class.GetValue<string>("name")))
                {
                    if ((byte)((@class.GetValue("flags", 0) >> 4) & 0x1F) == 3 && @class.GetValue("alignment", 0) == 0)
                    {
                        @class.SetValue("alignment", 4);
                    }
                    EbxClass ebxClass = default(EbxClass);
                    ebxClass.Name = @class.GetValue<string>("name");
                    ebxClass.Type = (ushort)@class.GetValue("flags", 0);
                    ebxClass.Alignment = (byte)@class.GetValue("alignment", 0);
                    ebxClass.FieldCount = (byte)@class.GetValue<DbObject>("fields").Count;
                    EbxClass item3 = ebxClass;
                    List<EbxField> list2 = new List<EbxField>();
                    foreach (DbObject item5 in @class.GetValue<DbObject>("fields"))
                    {
                        EbxField ebxField = default(EbxField);
                        ebxField.Name = item5.GetValue<string>("name");
                        ebxField.Type = (ushort)item5.GetValue("flags", 0);
                        EbxField item4 = ebxField;
                        list2.Add(item4);
                    }
                    values.Add(new Tuple<EbxClass, DbObject>(item3, @class));
                    fieldMapping.Add(item3.Name, list2);
                    list.Add(@class);
                }
            }
            foreach (DbObject classObj in list)
            {
                if (classObj.HasValue("basic"))
                {
                    dbObject.Add(classObj);
                }
                else
                {
                    Tuple<EbxClass, DbObject> tuple2 = values.Find((Tuple<EbxClass, DbObject> a) => a.Item2 == classObj);
                    int num2 = 0;
                    int fieldIndex2 = 0;
                    ProcessClass(tuple2.Item1, tuple2.Item2, fieldMapping[tuple2.Item1.Name], dbObject, ref num2, ref fieldIndex2);
                }
            }
            using (ModuleWriter moduleWriter = new ModuleWriter("EbxClasses.dll", dbObject))
            {
                moduleWriter.Write(FileSystem.Head);
            }
            if (File.Exists("EbxClasses.dll"))
            {
                FileInfo fileInfo = new FileInfo(".\\TmpProfiles\\" + ProfilesLibrary.SDKFilename + ".dll");
                if (!fileInfo.Directory.Exists)
                {
                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                }
                if (fileInfo.Exists)
                {
                    File.Delete(fileInfo.FullName);
                }
                File.Move("EbxClasses.dll", fileInfo.FullName);
                return true;
            }
            Console.WriteLine("Failed to produce SDK");
            return false;
        }

        private void LoadSharedTypeDescriptors(string name, Dictionary<string, Tuple<EbxClass, DbObject>> mapping, List<Guid> existingClasses)
        {
            byte[] fileFromMemoryFs = FileSystem.GetFileFromMemoryFs(name);
            if (fileFromMemoryFs != null)
            {
                Dictionary<uint, DbObject> dictionary = new Dictionary<uint, DbObject>();
                Dictionary<uint, string> dictionary2 = new Dictionary<uint, string>();
                foreach (DbObject @class in classList)
                {
                    if (!@class.HasValue("basic"))
                    {
                        dictionary.Add((uint)@class.GetValue("nameHash", 0), @class);
                        foreach (DbObject item4 in @class.GetValue<DbObject>("fields"))
                        {
                            if (!dictionary2.ContainsKey((uint)item4.GetValue("nameHash", 0)))
                            {
                                dictionary2.Add((uint)item4.GetValue("nameHash", 0), item4.GetValue("name", ""));
                            }
                        }
                    }
                }
                using (NativeReader nativeReader = new NativeReader(new MemoryStream(fileFromMemoryFs)))
                {
                    nativeReader.ReadUInt();
                    ushort num = nativeReader.ReadUShort();
                    ushort num2 = nativeReader.ReadUShort();
                    List<EbxField> list = new List<EbxField>();
                    for (int i = 0; i < num2; i++)
                    {
                        uint num3 = nativeReader.ReadUInt();
                        EbxField item = default(EbxField);
                        item.Name = (dictionary2.ContainsKey(num3) ? dictionary2[num3] : "");
                        item.NameHash = num3;
                        item.Type = (ushort)(nativeReader.ReadUShort() >> 1);
                        item.ClassRef = nativeReader.ReadUShort();
                        item.DataOffset = nativeReader.ReadUInt();
                        item.SecondOffset = nativeReader.ReadUInt();
                        list.Add(item);
                    }
                    int num4 = 0;
                    List<EbxClass?> list2 = new List<EbxClass?>();
                    List<Guid> list3 = new List<Guid>();
                    for (int j = 0; j < num; j++)
                    {
                        long position = nativeReader.Position;
                        Guid guid2 = nativeReader.ReadGuid();
                        Guid b = nativeReader.ReadGuid();
                        if (existingClasses.Contains(guid2) && guid2 == b)
                        {
                            list3.Add(guid2);
                            list2.Add(null);
                        }
                        else
                        {
                            existingClasses.Add(guid2);
                            nativeReader.Position -= 16L;
                            uint nameHash = nativeReader.ReadUInt();
                            uint num5 = nativeReader.ReadUInt();
                            int num6 = nativeReader.ReadByte();
                            byte b2 = nativeReader.ReadByte();
                            ushort type = nativeReader.ReadUShort();
                            uint num7 = nativeReader.ReadUInt();
                            if ((b2 & 0x80) != 0)
                            {
                                num6 += 256;
                                b2 = (byte)(b2 & 0x7F);
                            }
                            EbxClass value = default(EbxClass);
                            value.NameHash = nameHash;
                            value.FieldCount = (byte)num6;
                            value.FieldIndex = (int)((position - (num5 - 8)) / 16);
                            value.Alignment = b2;
                            value.Type = type;
                            value.Size = (ushort)num7;
                            value.Index = j;
                            list2.Add(value);
                            list3.Add(guid2);
                        }
                    }
                    for (int k = 0; k < list2.Count; k++)
                    {
                        if (list2[k].HasValue)
                        {
                            EbxClass value2 = list2[k].Value;
                            Guid guid = list3[k];
                            if (dictionary.ContainsKey(value2.NameHash))
                            {
                                DbObject dbObject3 = dictionary[value2.NameHash];
                                if (mapping.ContainsKey(dbObject3.GetValue("name", "")))
                                {
                                    mapping.Remove(dbObject3.GetValue("name", ""));
                                    fieldMapping.Remove(dbObject3.GetValue("name", ""));
                                }
                                if (!dbObject3.HasValue("typeInfoGuid"))
                                {
                                    dbObject3.SetValue("typeInfoGuid", DbObject.CreateList());
                                }
                                if (dbObject3.GetValue<DbObject>("typeInfoGuid").FindIndex((object a) => (Guid)a == guid) == -1)
                                {
                                    dbObject3.GetValue<DbObject>("typeInfoGuid").Add(guid);
                                }
                                EbxClass item2 = default(EbxClass);
                                item2.Name = dbObject3.GetValue("name", "");
                                item2.FieldCount = value2.FieldCount;
                                item2.Alignment = value2.Alignment;
                                item2.Size = value2.Size;
                                item2.Type = (ushort)(value2.Type >> 1);
                                item2.SecondSize = (ushort)dbObject3.GetValue("size", 0);
                                mapping.Add(item2.Name, new Tuple<EbxClass, DbObject>(item2, dbObject3));
                                fieldMapping.Add(item2.Name, new List<EbxField>());
                                DbObject value3 = dbObject3.GetValue<DbObject>("fields");
                                DbObject dbObject4 = DbObject.CreateList();
                                dbObject3.RemoveValue("fields");
                                for (int l = 0; l < value2.FieldCount; l++)
                                {
                                    EbxField item3 = list[value2.FieldIndex + l];
                                    bool flag = false;
                                    foreach (DbObject item5 in value3)
                                    {
                                        if (item5.GetValue("nameHash", 0) == (int)item3.NameHash)
                                        {
                                            item5.SetValue("type", item3.Type);
                                            item5.SetValue("offset", item3.DataOffset);
                                            item5.SetValue("value", (int)item3.DataOffset);
                                            if (item3.DebugType == EbxFieldType.Array)
                                            {
                                                Guid guid3 = list3[value2.Index + (short)item3.ClassRef];
                                                item5.SetValue("guid", guid3);
                                            }
                                            dbObject4.Add(item5);
                                            flag = true;
                                            break;
                                        }
                                    }
                                    if (!flag)
                                    {
                                        uint num8 = (ProfilesLibrary.DataVersion == 20190905) ? 3301947476u : 3109710567u;
                                        if (item3.NameHash != num8)
                                        {
                                            item3.Name = ((item3.Name != "") ? item3.Name : ("Unknown_" + item3.NameHash.ToString("x8")));
                                            DbObject dbObject6 = DbObject.CreateObject();
                                            dbObject6.SetValue("name", item3.Name);
                                            dbObject6.SetValue("nameHash", (int)item3.NameHash);
                                            dbObject6.SetValue("type", item3.Type);
                                            dbObject6.SetValue("flags", (ushort)0);
                                            dbObject6.SetValue("offset", item3.DataOffset);
                                            dbObject6.SetValue("value", (int)item3.DataOffset);
                                            dbObject4.Add(dbObject6);
                                        }
                                    }
                                    fieldMapping[item2.Name].Add(item3);
                                    num4++;
                                }
                                dbObject3.SetValue("fields", dbObject4);
                            }
                            else
                            {
                                num4 += value2.FieldCount;
                            }
                        }
                    }
                }
            }
        }

        private int ProcessClass(EbxClass pclass, DbObject pobj, List<EbxField> fields, DbObject outList, ref int offset, ref int fieldIndex)
        {
            string parent = pobj.GetValue<string>("parent");
            if (parent != "")
            {
                Tuple<EbxClass, DbObject> tuple = values.Find((Tuple<EbxClass, DbObject> a) => a.Item1.Name == parent);
                offset = ProcessClass(tuple.Item1, tuple.Item2, fieldMapping[tuple.Item1.Name], outList, ref offset, ref fieldIndex);
                if (tuple.Item1.Name == "DataContainer" && pclass.Name != "Asset")
                {
                    pobj.SetValue("isData", true);
                }
            }
            if (processed.Contains(pclass))
            {
                foreach (DbObject @out in outList)
                {
                    if (@out.GetValue<string>("name") == pclass.Name)
                    {
                        fieldIndex += @out.GetValue<DbObject>("fields").Count;
                        return @out.GetValue("size", 0);
                    }
                }
                return 0;
            }
            processed.Add(pclass);
            int num = classMetaList.FindIndex((object o) => ((DbObject)o).GetValue<string>("name") == pclass.Name);
            DbObject dbObject2 = null;
            if (num != -1)
            {
                dbObject2 = (classMetaList[num] as DbObject);
            }
            DbObject value = pobj.GetValue<DbObject>("fields");
            DbObject dbObject3 = new DbObject(bObject: false);
            if (pclass.DebugType == EbxFieldType.Enum)
            {
                foreach (DbObject item2 in value)
                {
                    DbObject dbObject5 = new DbObject();
                    dbObject5.AddValue("name", item2.GetValue<string>("name"));
                    dbObject5.AddValue("value", item2.GetValue("value", 0));
                    dbObject3.Add(dbObject5);
                }
            }
            else
            {
                List<EbxField> list = new List<EbxField>();
                foreach (DbObject item3 in value)
                {
                    EbxField item = default(EbxField);
                    item.Name = item3.GetValue<string>("name");
                    item.Type = (ushort)item3.GetValue("flags", 0);
                    item.DataOffset = (uint)item3.GetValue("offset", 0);
                    item.NameHash = (uint)item3.GetValue("nameHash", 0);
                    list.Add(item);
                }
                list.Sort((EbxField a, EbxField b) => a.DataOffset.CompareTo(b.DataOffset));
                foreach (EbxField field in list)
                {
                    if (field.Name == string.Empty)
                        continue;

                    if (field.DebugType != 0)
                    {
                        DbObject dbObject7 = null;
                        foreach (DbObject item4 in value)
                        {
                            if (item4.GetValue<string>("name") == field.Name)
                            {
                                dbObject7 = item4;
                                break;
                            }
                        }
                        if (dbObject7 == null)
                        {
                            Console.WriteLine(pclass.Name + "." + field.Name + " missing from executable definition");
                        }
                        else
                        {
                            DbObject fieldObj = new DbObject();
                            if (dbObject2 != null)
                            {
                                DbObject value2 = dbObject2.GetValue<DbObject>("fields");
                                num = value2.FindIndex((object o) => ((DbObject)o).GetValue<string>("name") == field.Name);
                                if (num != -1)
                                {
                                    DbObject value3 = value2[num] as DbObject;
                                    fieldObj.AddValue("meta", value3);
                                }
                            }
                            fieldObj.AddValue("name", field.Name);
                            fieldObj.AddValue("type", (int)field.DebugType);
                            fieldObj.AddValue("flags", (int)field.Type);
                            if (ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190905 || ProfilesLibrary.DataVersion == 20190911)
                            {
                                fieldObj.AddValue("offset", (int)field.DataOffset);
                                fieldObj.AddValue("nameHash", field.NameHash);
                            }
                            if (field.DebugType == EbxFieldType.Pointer || field.DebugType == EbxFieldType.Struct || field.DebugType == EbxFieldType.Enum)
                            {
                                string baseTypeName2 = dbObject7.GetValue<string>("baseType");
                                int num2 = values.FindIndex((Tuple<EbxClass, DbObject> a) => a.Item1.Name == baseTypeName2 && !a.Item2.HasValue("basic"));
                                if (num2 != -1)
                                {
                                    fieldObj.AddValue("baseType", values[num2].Item1.Name);
                                }
                                else if (field.DebugType == EbxFieldType.Enum)
                                {
                                    throw new InvalidDataException();
                                }
                                if (field.DebugType == EbxFieldType.Struct)
                                {
                                    foreach (EbxField field2 in fields)
                                    {
                                        if (field2.Name.Equals(field.Name))
                                        {
                                            if (field.Type != field2.Type)
                                            {
                                                fieldObj.SetValue("flags", (int)field2.Type);
                                            }
                                            break;
                                        }
                                    }
                                    while (offset % (int)values[num2].Item1.Alignment != 0)
                                    {
                                        offset++;
                                    }
                                }
                            }
                            else if (field.DebugType == EbxFieldType.Array)
                            {
                                string baseTypeName = dbObject7.GetValue<string>("baseType");
                                int num3 = values.FindIndex((Tuple<EbxClass, DbObject> a) => a.Item1.Name == baseTypeName && !a.Item2.HasValue("basic"));
                                if (num3 != -1)
                                {
                                    fieldObj.AddValue("baseType", values[num3].Item1.Name);
                                    fieldObj.AddValue("arrayFlags", (int)values[num3].Item1.Type);
                                }
                                else
                                {
                                    EbxFieldType ebxFieldType = (EbxFieldType)((dbObject7.GetValue("arrayFlags", 0) >> 4) & 0x1F);
                                    if (ebxFieldType == EbxFieldType.Pointer || ebxFieldType == EbxFieldType.Struct || ebxFieldType == EbxFieldType.Enum)
                                    {
                                        fieldObj.AddValue("baseType", baseTypeName);
                                    }
                                    fieldObj.AddValue("arrayFlags", dbObject7.GetValue("arrayFlags", 0));
                                }
                                if (dbObject7.HasValue("guid"))
                                {
                                    fieldObj.SetValue("guid", dbObject7.GetValue<Guid>("guid"));
                                }
                            }
                            if (field.DebugType == EbxFieldType.ResourceRef || field.DebugType == EbxFieldType.TypeRef || field.DebugType == EbxFieldType.FileRef || field.DebugType == EbxFieldType.BoxedValueRef)
                            {
                                while (offset % 8 != 0)
                                {
                                    offset++;
                                }
                            }
                            else if (field.DebugType == EbxFieldType.Array || field.DebugType == EbxFieldType.Pointer)
                            {
                                while (offset % 4 != 0)
                                {
                                    offset++;
                                }
                            }
                            if (ProfilesLibrary.DataVersion != 20181207)
                            {
                                fieldObj.AddValue("offset", offset);
                            }
                            fieldObj.SetValue("index", dbObject7.GetValue("index", 0) + fieldIndex);
                            dbObject3.Add(fieldObj);
                            switch (field.DebugType)
                            {
                                case EbxFieldType.Struct:
                                    {
                                        Tuple<EbxClass, DbObject> tuple2 = values.Find((Tuple<EbxClass, DbObject> a) => a.Item1.Name == fieldObj.GetValue<string>("baseType"));
                                        int num4 = 0;
                                        int fieldIndex2 = 0;
                                        offset += ProcessClass(tuple2.Item1, tuple2.Item2, fieldMapping[tuple2.Item1.Name], outList, ref num4, ref fieldIndex2);
                                        break;
                                    }
                                case EbxFieldType.Pointer:
                                    offset += 4;
                                    break;
                                case EbxFieldType.Array:
                                    offset += 4;
                                    break;
                                case EbxFieldType.String:
                                    offset += 32;
                                    break;
                                case EbxFieldType.CString:
                                    offset += 4;
                                    break;
                                case EbxFieldType.Enum:
                                    offset += 4;
                                    break;
                                case EbxFieldType.FileRef:
                                    offset += 8;
                                    break;
                                case EbxFieldType.Boolean:
                                    offset++;
                                    break;
                                case EbxFieldType.Int8:
                                    offset++;
                                    break;
                                case EbxFieldType.UInt8:
                                    offset++;
                                    break;
                                case EbxFieldType.Int16:
                                    offset += 2;
                                    break;
                                case EbxFieldType.UInt16:
                                    offset += 2;
                                    break;
                                case EbxFieldType.Int32:
                                    offset += 4;
                                    break;
                                case EbxFieldType.UInt32:
                                    offset += 4;
                                    break;
                                case EbxFieldType.Int64:
                                    offset += 8;
                                    break;
                                case EbxFieldType.UInt64:
                                    offset += 8;
                                    break;
                                case EbxFieldType.Float32:
                                    offset += 4;
                                    break;
                                case EbxFieldType.Float64:
                                    offset += 8;
                                    break;
                                case EbxFieldType.Guid:
                                    offset += 16;
                                    break;
                                case EbxFieldType.Sha1:
                                    offset += 20;
                                    break;
                                case EbxFieldType.ResourceRef:
                                    offset += 8;
                                    break;
                                case EbxFieldType.TypeRef:
                                    offset += 8;
                                    break;
                                case EbxFieldType.BoxedValueRef:
                                    offset += 16;
                                    break;
                            }
                        }
                    }
                }
            }
            while (offset % (int)pclass.Alignment != 0)
            {
                offset++;
            }
            pobj.SetValue("flags", (int)pclass.Type);
            pobj.SetValue("size", offset);
            if (ProfilesLibrary.DataVersion == 20181207)
            {
                pobj.SetValue("size", pclass.Size);
            }
            if (pclass.DebugType == EbxFieldType.Enum)
            {
                pobj.SetValue("size", 4);
            }
            pobj.SetValue("alignment", (int)pclass.Alignment);
            pobj.SetValue("fields", dbObject3);
            fieldIndex += dbObject3.Count;
            if (dbObject2 != null)
            {
                pobj.AddValue("meta", dbObject2);
                foreach (DbObject item5 in dbObject2.GetValue<DbObject>("fields"))
                {
                    if (item5.GetValue("added", defaultValue: false))
                    {
                        DbObject dbObject10 = new DbObject();
                        dbObject10.AddValue("name", item5.GetValue<string>("name"));
                        dbObject10.AddValue("type", 15);
                        dbObject10.AddValue("meta", item5);
                        pobj.GetValue<DbObject>("fields").Add(dbObject10);
                    }
                }
            }
            outList.Add(pobj);
            return offset;
        }

        private DbObject DumpClasses(SdkUpdateTask task)
        {
            MemoryReader memoryReader = null;
            //string typeStr = "v2k4FIFASDKGenerator.ClassesSdkCreator+ClassInfo";
            string typeStr = "v2k4FIFASDKGenerator.Madden20.ClassInfo";

            //if (ProfilesLibrary.DataVersion == 20181207)
            //{
            //    str = "FrostyEditor.Anthem.";
            //}
            //else if (ProfilesLibrary.DataVersion == 20190729)
            //{
            //    str = "FrostyEditor.Madden20.";
            //}
            //else if (ProfilesLibrary.DataVersion == 20190905)
            //{
            //    str = "FrostyEditor.Madden20.";
            //}
            //else 
            if (ProfilesLibrary.DataVersion == 20190911)
            {
                typeStr = "v2k4FIFASDKGenerator.Madden20.ClassInfo";
            }
            else if (ProfilesLibrary.DisplayName.Contains("18"))
            {
                typeStr = "v2k4FIFASDKGenerator.FIFA18.ClassInfo";
            }
            //else if (ProfilesLibrary.DataVersion == 20191101)
            //{
            //    str = "FrostyEditor.Madden20.";
            //}

            // Find types to find out all is good
            //Assembly thisLibClasses = typeof(v2k4FIFASDKGenerator.ClassesSdkCreator).Assembly;
            //var types = thisLibClasses.GetTypes();
            //foreach (Type type in types)
            //{
            //    Debug.WriteLine(type.FullName);
            //}


            long typeInfoOffset = state.TypeInfoOffset;
            memoryReader = new MemoryReader(state.Process, typeInfoOffset);
            offsetClassInfoMapping.Clear();
            classInfos.Clear();
            alreadyProcessedClasses.Clear();
            processed.Clear();
            if (fieldMapping != null)
            {
                fieldMapping.Clear();
            }
            offset = typeInfoOffset;
            int num = 0;
            while (offset != 0L)
            {
                task.StatusMessage = $"Found {++num} type(s)";
                memoryReader.Position = offset;
                var t = Type.GetType(typeStr);
                ClassInfo classInfo = (ClassInfo)Activator.CreateInstance(t);
                classInfo.Read(memoryReader);
                classInfos.Add(classInfo);
                offsetClassInfoMapping.Add(typeInfoOffset, classInfo);
                if (offset != 0L)
                {
                    typeInfoOffset = offset;
                }
            }
            memoryReader.Dispose();
            DbObject result = new DbObject(bObject: false);
            classInfos.Sort((ClassInfo a, ClassInfo b) => a.typeInfo.name.CompareTo(b.typeInfo.name));

            var findSomeStuffTest = classInfos.Where(x => x.typeInfo.name.ToLower().Contains("lua")).ToList();

            foreach (ClassInfo classInfo2 in classInfos)
            {
                if (classInfo2.typeInfo.Type == 2 || classInfo2.typeInfo.Type == 3 || classInfo2.typeInfo.Type == 8 || classInfo2.typeInfo.Type == 27)
                {
                    if (classInfo2.typeInfo.Type == 27)
                    {
                        classInfo2.typeInfo.flags = 48;
                    }
                    CreateClassObject(classInfo2, ref result);
                }
                else if (classInfo2.typeInfo.Type != 4)
                {
                    CreateBasicClassObject(classInfo2, ref result);
                }
            }
            return result;
        }

        private void CreateBasicClassObject(ClassInfo classInfo, ref DbObject classList)
        {
            int alignment = classInfo.typeInfo.alignment;
            int size = (int)classInfo.typeInfo.size;
            DbObject dbObject = DbObject.CreateObject();
            dbObject.SetValue("name", classInfo.typeInfo.name);
            dbObject.SetValue("type", classInfo.typeInfo.Type);
            dbObject.SetValue("flags", (int)classInfo.typeInfo.flags);
            dbObject.SetValue("alignment", alignment);
            dbObject.SetValue("size", size);
            dbObject.SetValue("runtimeSize", size);
            if (classInfo.typeInfo.guid != Guid.Empty)
            {
                dbObject.SetValue("guid", classInfo.typeInfo.guid);
            }
            dbObject.SetValue("namespace", classInfo.typeInfo.nameSpace);
            dbObject.SetValue("fields", DbObject.CreateList());
            dbObject.SetValue("parent", "");
            dbObject.SetValue("basic", true);
            classInfo.typeInfo.Modify(dbObject);
            classList.Add(dbObject);
        }

        private void CreateClassObject(ClassInfo classInfo, ref DbObject classList)
        {
            if (!alreadyProcessedClasses.Contains(classInfo.typeInfo.name))
            {
                ClassInfo classInfo2 = offsetClassInfoMapping.ContainsKey(classInfo.parentClass) ? offsetClassInfoMapping[classInfo.parentClass] : null;
                if (classInfo2 != null)
                {
                    CreateClassObject(classInfo2, ref classList);
                }
                int alignment = classInfo.typeInfo.alignment;
                int size = (int)classInfo.typeInfo.size;
                DbObject dbObject = new DbObject();
                dbObject.AddValue("name", classInfo.typeInfo.name);
                dbObject.AddValue("parent", (classInfo2 != null) ? classInfo2.typeInfo.name : "");
                dbObject.AddValue("type", classInfo.typeInfo.Type);
                dbObject.AddValue("flags", (int)classInfo.typeInfo.flags);
                dbObject.AddValue("alignment", alignment);
                dbObject.AddValue("size", size);
                dbObject.AddValue("runtimeSize", size);
                dbObject.AddValue("additional", (int)classInfo.isDataContainer);
                dbObject.AddValue("namespace", classInfo.typeInfo.nameSpace);
                if (classInfo.typeInfo.guid != Guid.Empty)
                {
                    dbObject.AddValue("guid", classInfo.typeInfo.guid);
                }
                classInfo.typeInfo.Modify(dbObject);
                DbObject dbObject2 = new DbObject(bObject: false);
                foreach (FieldInfo field in classInfo.typeInfo.fields)
                {
                    DbObject dbObject3 = new DbObject();
                    if (classInfo.typeInfo.Type == 8)
                    {
                        dbObject3.AddValue("name", field.name);
                        dbObject3.AddValue("value", (int)field.typeOffset);
                    }
                    else
                    {
                        ClassInfo classInfo3 = offsetClassInfoMapping[field.typeOffset];
                        dbObject3.AddValue("name", field.name);
                        dbObject3.AddValue("type", classInfo3.typeInfo.Type);
                        dbObject3.AddValue("flags", (int)classInfo3.typeInfo.flags);
                        dbObject3.AddValue("offset", (int)field.offset);
                        dbObject3.AddValue("index", field.index);
                        if (classInfo3.typeInfo.Type == 3 || classInfo3.typeInfo.Type == 2 || classInfo3.typeInfo.Type == 8)
                        {
                            dbObject3.AddValue("baseType", classInfo3.typeInfo.name);
                        }
                        else if (classInfo3.typeInfo.Type == 4)
                        {
                            classInfo3 = offsetClassInfoMapping[classInfo3.parentClass];
                            dbObject3.AddValue("baseType", classInfo3.typeInfo.name);
                            dbObject3.AddValue("arrayFlags", (int)classInfo3.typeInfo.flags);
                        }
                    }
                    field.Modify(dbObject3);
                    dbObject2.Add(dbObject3);
                }
                dbObject.AddValue("fields", dbObject2);
                classList.Add(dbObject);
                alreadyProcessedClasses.Add(classInfo.typeInfo.name);
            }
        }
    }
}
