using FrostbiteSdk;
using FrostbiteSdk.SdkGenerator;
using FrostyEditor;
using FrostyEditor.IO;
using FrostyEditor.Windows;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using SdkGenerator.BaseInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SdkGenerator
{
    public class ClassesSdkCreator
    {
        public readonly static AssetManager AssetManager = AssetManager.Instance;

        public static ResourceManager ResourceManager;

        public static FileSystem FileSystem;

        public static EbxAssetEntry SelectedAsset;

        //public static string configFilename = "FrostyEditor.ini";


        

        public static long offset;

        private List<IClassInfo> classInfos = new List<IClassInfo>();

        private List<string> alreadyProcessedClasses = new List<string>();

        private Dictionary<long, IClassInfo> offsetClassInfoMapping = new Dictionary<long, IClassInfo>();

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

            if (!string.IsNullOrEmpty(ProfilesLibrary.SDKClassesFile))
            {
                var classesTxtFilePath = AppContext.BaseDirectory + "/SdkGen/" + ProfilesLibrary.SDKClassesFile;
                using (FileStream stream = new FileStream(classesTxtFilePath, FileMode.Open))
                {
                    if (stream != null)
                    {
                        classMetaList = TypeLibrary.LoadClassesSDK(stream);
                    }
                }
            }
            else 
            { 
                if (ProfilesLibrary.IsMadden21DataVersion())
                {
                    using (FileStream stream = new FileStream(AppContext.BaseDirectory + "/SdkGen/M21.Classes.txt", FileMode.Open))
                    {
                        if (stream != null)
                        {
                            classMetaList = TypeLibrary.LoadClassesSDK(stream);
                        }
                    }
                }
             
                //else
                //{
                if (ProfilesLibrary.IsFIFA20DataVersion())
                {
                    using (FileStream stream = new FileStream(AppContext.BaseDirectory + "/SdkGen/FIFA20.Classes.txt", FileMode.Open))
                    {
                        if (stream != null)
                        {
                            classMetaList = TypeLibrary.LoadClassesSDK(stream);
                        }
                    }
                }
                //}
                if (ProfilesLibrary.IsFIFA21DataVersion())
                {
                    using (FileStream stream = new FileStream(AppContext.BaseDirectory + "/SdkGen/FIFA21.Classes.txt", FileMode.Open))
                    {
                        if (stream != null)
                        {
                            classMetaList = TypeLibrary.LoadClassesSDK(stream);
                        }
                    }
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
            if(FileSystem == null)
            {
                FileSystem = AssetManager.Instance.fs;
            }
            // Data must be loaded and cached before SDK is built

            mapping = new Dictionary<string, Tuple<EbxClass, DbObject>>();
            fieldMapping = new Dictionary<string, List<EbxField>>();

            List<Guid> existingClasses = new List<Guid>();
            foreach (var f in FileSystem.memoryFs.Keys
                .Where(x=>x.Contains("SharedTypeDescriptor", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Length)
                //.OrderBy(x=>x.Contains("Patch", StringComparison.OrdinalIgnoreCase))
                )
            {
                LoadSharedTypeDescriptors(f, mapping, ref existingClasses);
            }
            //if (FileSystem.HasFileInMemoryFs("SharedTypeDescriptors.ebx"))
            //{
            //    List<Guid> existingClasses = new List<Guid>();
            //    LoadSharedTypeDescriptors("SharedTypeDescriptors.ebx", mapping, existingClasses);
            //    LoadSharedTypeDescriptors("SharedTypeDescriptors_patch.ebx", mapping, existingClasses);
            //    LoadSharedTypeDescriptors("SharedTypeDescriptors_Patch.ebx", mapping, existingClasses);
            //}
            //else
            //{
            //    throw new Exception("Havent found Shared Type Descriptors .EBX!");
             
            //}
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
                var className = @class.GetValue<string>("name");
                if(className.Equals("RenderFormat", StringComparison.OrdinalIgnoreCase))
                {

                }
                if (className.Contains("actor_movement", StringComparison.OrdinalIgnoreCase))
                {

                }
                if (fieldMapping.ContainsKey(className) 
                    && fieldMapping[className].Count < @class.GetValue<DbObject>("fields").List.Count)
                {
                    fieldMapping[className].Clear();
                    List<EbxField> fieldList = new List<EbxField>();
                    foreach (DbObject fieldInList in @class.GetValue<DbObject>("fields"))
                    {
                        EbxField ebxField = default(EbxField);
                        ebxField.Name = fieldInList.GetValue<string>("name");
                        ebxField.Type = (ushort)fieldInList.GetValue("flags", 0);
                        ebxField.NameHash = fieldInList.GetValue<int>("nameHash", 0);
                        fieldList.Add(ebxField);
                    }
                    fieldMapping[className] = fieldList;
                }
                if (!fieldMapping.ContainsKey(className))
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
                    EbxClass classItem = ebxClass;
                    List<EbxField> fieldList = new List<EbxField>();
                    foreach (DbObject fieldInList in @class.GetValue<DbObject>("fields"))
                    {
                        EbxField ebxField = default(EbxField);
                        ebxField.Name = fieldInList.GetValue<string>("name");
                        ebxField.Type = (ushort)fieldInList.GetValue("flags", 0);
                        ebxField.NameHash = fieldInList.GetValue<int>("nameHash", 0);
                        fieldList.Add(ebxField);
                    }
                    if(ebxClass.Name.ToLower().Contains("blocking"))
                    {

                    }
                    values.Add(new Tuple<EbxClass, DbObject>(classItem, @class));
                    fieldMapping.Add(classItem.Name, fieldList);
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

        private void LoadSharedTypeDescriptors2(string name, Dictionary<string, Tuple<EbxClass, DbObject>> mapping, ref List<Guid> existingClasses)
        {
            Dictionary<int, DbObject> classDictionaryToHash = new Dictionary<int, DbObject>();
            Dictionary<int, string> fieldDictionaryToHash = new Dictionary<int, string>();
            foreach (DbObject @class in classList)
            {
                if (@class.GetValue<string>("name").Equals("RenderFormat", StringComparison.OrdinalIgnoreCase))
                {

                }

                if (@class.GetValue<string>("name").Contains("goalkeeper", StringComparison.OrdinalIgnoreCase))
                {

                }

                if (@class.GetValue<string>("name").Contains("blocking"))
                {

                }
                if (!@class.HasValue("basic"))
                {
                    classDictionaryToHash.Add(@class.GetValue("nameHash", 0), @class);
                    foreach (DbObject item4 in @class.GetValue<DbObject>("fields"))
                    {
                        //if (!fieldDictionaryToHash.ContainsKey((ulong)item4.GetValue<ulong>("nameHash")))
                        if (!fieldDictionaryToHash.ContainsKey(item4.GetValue<int>("nameHash")))
                        {
                            fieldDictionaryToHash.Add(item4.GetValue<int>("nameHash"), item4.GetValue("name", ""));
                        }
                        else
                        {

                        }
                    }
                }
            }

            EbxSharedTypeDescriptorV2 std;
            //if (!string.IsNullOrEmpty(ProfilesLibrary.EBXTypeDescriptor))
            //{
            //    std = (IEbxSharedTypeDescriptor)AssetManager.Instance.LoadTypeByName(ProfilesLibrary.EBXTypeDescriptor
            //            , FileSystem.Instance, name, name.Contains("patch", StringComparison.OrdinalIgnoreCase));
            //}
            //else
            std = new EbxSharedTypeDescriptorV2(
                name
                , name.Contains("patch", StringComparison.OrdinalIgnoreCase)
                , false
                );
            std.ReflectionTypeDescripter = true;
            var isPatch = name.Contains("patch", StringComparison.OrdinalIgnoreCase);
            std.Read(FileSystem.Instance.GetFileFromMemoryFs(name), isPatch);

            foreach (var g in std.Guids)
            {
                if (!existingClasses.Contains(g))
                {
                    existingClasses.Add(g);
                }
            }
            for (int i = 0; i < std.Fields.Count; i++)
            {
                EbxField field = std.Fields[i];
                if (fieldDictionaryToHash.TryGetValue(field.NameHash, out var fieldName))
                {
                    field.Name = fieldName;
                }
                else
                {
                    field.Name = string.Empty;
                }
                std.Fields[i] = field;
            }

            for (int k = 0; k < std.Classes.Count; k++)
            {
                if (std.Classes[k].HasValue)
                {
                    EbxClass @class = std.Classes[k].Value;
                    Guid guid = std.Guids[k];
                    if (classDictionaryToHash.ContainsKey((int)@class.NameHash))
                    {
                        DbObject dboClass = classDictionaryToHash[(int)@class.NameHash];
                        if (mapping.ContainsKey(dboClass.GetValue("name", "")))
                        {
                            //mapping.Remove(dboClass.GetValue("name", ""));
                            //fieldMapping.Remove(dboClass.GetValue("name", ""));
                        }
                        if (!dboClass.HasValue("typeInfoGuid"))
                        {
                            dboClass.SetValue("typeInfoGuid", DbObject.CreateList());
                        }
                        if (dboClass.GetValue<DbObject>("typeInfoGuid").FindIndex((object a) => (Guid)a == guid) == -1)
                        {
                            dboClass.GetValue<DbObject>("typeInfoGuid").Add(guid);
                        }

                        EbxClass ebxClassItem = default(EbxClass);
                        ebxClassItem.Name = dboClass.GetValue("name", "");
                        if (ebxClassItem.Name.Contains("blocking", StringComparison.OrdinalIgnoreCase))
                        {

                        }
                        if (ebxClassItem.Name.Contains("RenderFormat", StringComparison.OrdinalIgnoreCase))
                        {

                        }
                        if (ebxClassItem.Name.Contains("goalkeeper", StringComparison.OrdinalIgnoreCase))
                        {

                        }
                        if (ebxClassItem.Name.Contains("movement", StringComparison.OrdinalIgnoreCase))
                        {

                        }
                        ebxClassItem.NameHash = @class.NameHash;
                        ebxClassItem.FieldCount = @class.FieldCount;
                        ebxClassItem.Alignment = @class.Alignment;
                        ebxClassItem.Size = @class.Size;
                        ebxClassItem.Type = (ushort)(@class.Type >> 1);
                        ebxClassItem.SecondSize = (ushort)dboClass.GetValue("size", 0);
                        if(!mapping.ContainsKey(ebxClassItem.Name))
                            mapping.Add(ebxClassItem.Name, new Tuple<EbxClass, DbObject>(ebxClassItem, dboClass));
                        if(!fieldMapping.ContainsKey(ebxClassItem.Name))
                            fieldMapping.Add(ebxClassItem.Name, new List<EbxField>());
                        DbObject sdkFields = dboClass.GetValue<DbObject>("fields");
                        DbObject dbObject4 = DbObject.CreateList();
                        if (@class.FieldCount > 0)
                        {
                            List<string> usedFields = new List<string>();
                            for (int l = 0; l < @class.FieldCount; l++)
                            {
                                EbxField field = std.Fields[@class.FieldIndex + l];
                                if (fieldDictionaryToHash.ContainsKey(field.NameHash))
                                {
                                    DbObject dboField = sdkFields.List.Single(x => (uint)((DbObject)x).GetValue<int>("nameHash") == (uint)field.NameHash) as DbObject;
                                    if (dboField != null)
                                    {
                                        dboField.SetValue("type", field.Type);
                                        dboField.SetValue("offset", field.DataOffset);
                                        dboField.SetValue("value", (int)field.DataOffset);
                                        //if (field.DebugType == EbxFieldType.Array)
                                        {
                                            var fCRIndex = @class.Index + (short)field.ClassRef;
                                            if (fCRIndex != -1 && std.Guids.Count > fCRIndex)
                                            {
                                                Guid guid3 = std.Guids[fCRIndex];
                                                if (dboField != null)
                                                {
                                                    dboField.SetValue("guid", guid3);
                                                }
                                            }
                                        }
                                        usedFields.Add(field.NameHash.ToString());
                                    }
                                }

                                fieldMapping[ebxClassItem.Name].Add(field);
                            }
                            if(usedFields.Count != sdkFields.List.Count)
                            {

                            }
                        }
                    }
                    else
                    {
                    }
                }
            }
        }

        private void LoadSharedTypeDescriptors(string name, Dictionary<string, Tuple<EbxClass, DbObject>> mapping, ref List<Guid> existingClasses)
        {
            if (!string.IsNullOrEmpty(ProfilesLibrary.EBXTypeDescriptor))
            {
                LoadSharedTypeDescriptors2(name, mapping, ref existingClasses);
                return;
            }
        }

        private int ProcessClass(EbxClass pclass, DbObject pobj, List<EbxField> fields, DbObject outList, ref int offset, ref int fieldIndex)
        {

            string parent = pobj.GetValue<string>("parent");
            if (parent != "")
            {
                Tuple<EbxClass, DbObject> tuple2 = values.Find((Tuple<EbxClass, DbObject> a) => a.Item1.Name == parent);
                offset = ProcessClass(tuple2.Item1, tuple2.Item2, fieldMapping[tuple2.Item1.Name], outList, ref offset, ref fieldIndex);
                if (tuple2.Item1.Name == "DataContainer" && pclass.Name != "Asset")
                {
                    pobj.SetValue("isData", true);
                }
            }
            if (processed.Contains(pclass))
            {
                foreach (DbObject dbObject in outList.List)
                {
                    if (dbObject.GetValue<string>("name") == pclass.Name)
                    {
                        fieldIndex += dbObject.GetValue<DbObject>("fields").Count;
                        return dbObject.GetValue<int>("size");
                    }
                }
                return 0;
            }
            processed.Add(pclass);
            DbObject dboClassMetaList = classMetaList != null ? classMetaList.List.FirstOrDefault((object o) => ((DbObject)o).GetValue<string>("name") == pclass.Name) as DbObject : null;
            DbObject dbObject3 = pobj.GetValue<DbObject>("fields");
            DbObject dbObject4 = DbObject.CreateList();
            if (pclass.DebugType == EbxFieldType.Enum)
            {
                foreach (DbObject dbObject8 in dbObject3.List)
                {
                    DbObject dbObject12 = DbObject.CreateObject();
                    dbObject12.AddValue("name", dbObject8.GetValue<string>("name"));
                    dbObject12.AddValue("value", dbObject8.GetValue<int>("value"));
                    dbObject4.List.Add(dbObject12);
                }
            }
            else
            {
                if(pclass.Name.Contains("FloatCurvePoint", StringComparison.OrdinalIgnoreCase))
                {

                }

                if (pclass.Name.Contains("RenderFormat", StringComparison.OrdinalIgnoreCase))
                {

                }

                List<EbxField> ebxFieldList = new List<EbxField>();
                foreach (DbObject dbObject7 in dbObject3.List.Distinct())
                {
                    ebxFieldList.Add(new EbxField
                    {
                        Name = dbObject7.GetValue<string>("name"),
                        Type = (ushort)dbObject7.GetValue<int>("flags"),
                        DataOffset = (uint)dbObject7.GetValue<uint>("offset"),
                        NameHash = dbObject7.GetValue<int>("nameHash")
                    });
                }
                ebxFieldList = ebxFieldList.OrderBy(x => x.DataOffset).ToList();
                foreach (EbxField ebxField in ebxFieldList)
                {
                    EbxField field = ebxField;
                    if (field.DebugType == EbxFieldType.Inherited)
                    {
                        continue;
                    }
                    DbObject dbObject6 = null;
                    foreach (DbObject dbObject11 in dbObject3.List)
                    {
                        if (dbObject11.GetValue<string>("name") == field.Name)
                        {
                            dbObject6 = dbObject11;
                            break;
                        }
                    }
                    if (dbObject6 == null)
                    {
                        Console.WriteLine(pclass.Name + "." + field.Name + " missing from executable definition");
                        continue;
                    }
                    DbObject fieldObj = DbObject.CreateObject();
                    if (dboClassMetaList != null)
                    {
                        DbObject dbObject10 = dboClassMetaList.GetValue<DbObject>("fields");
            DbObject dbObject13 = classMetaList.List.FirstOrDefault((object o) => ((DbObject)o).GetValue<string>("name") == pclass.Name) as DbObject;
                        if (dbObject13 !=  null)
                        {
                            fieldObj.AddValue("meta", dbObject13);
                        }
                    }
                    fieldObj.AddValue("name", field.Name);
                    fieldObj.AddValue("type", (int)field.DebugType);
                    fieldObj.AddValue("flags", (int)field.Type);
                    fieldObj.AddValue("offset", (int)field.DataOffset);
                    fieldObj.AddValue("nameHash", (int)field.NameHash);
                    if (field.DebugType == EbxFieldType.Pointer || field.DebugType == EbxFieldType.Struct || field.DebugType == EbxFieldType.Enum)
                    {
                        string baseTypeName2 = dbObject6.GetValue<string>("baseType");
                        int index2 = values.FindIndex((Tuple<EbxClass, DbObject> a) => a.Item1.Name == baseTypeName2 && !a.Item2.HasValue("basic"));
                        if (values.Any((Tuple<EbxClass, DbObject> a) => a.Item1.Name == baseTypeName2 && !a.Item2.HasValue("basic")))
                        {
                            fieldObj.AddValue("baseType", values[index2].Item1.Name);
                        }
                        else if (field.DebugType == EbxFieldType.Enum)
                        {
                            throw new InvalidDataException();
                        }
                        if (index2 != -1 && field.DebugType == EbxFieldType.Struct)
                        {
                            foreach (EbxField field2 in fields)
                            {
                                if (!string.IsNullOrEmpty(field2.Name) && field2.Name.Equals(field.Name))
                                {
                                    if (field.Type != field2.Type)
                                    {
                                        fieldObj.SetValue("flags", (int)field2.Type);
                                    }
                                    break;
                                }
                            }
                            while (offset % (int)values[index2].Item1.Alignment != 0)
                            {
                                offset++;
                            }
                        }
                    }
                    else if (field.DebugType == EbxFieldType.Array)
                    {
                        string baseTypeName = dbObject6.GetValue<string>("baseType");
                        int index3 = values.FindIndex((Tuple<EbxClass, DbObject> a) => a.Item1.Name == baseTypeName && !a.Item2.HasValue("basic"));
                        if (index3 != -1)
                        {
                            fieldObj.AddValue("baseType", values[index3].Item1.Name);
                            fieldObj.AddValue("arrayFlags", (int)values[index3].Item1.Type);
                        }
                        else
                        {
                            EbxFieldType ebxFieldType = (EbxFieldType)((uint)(dbObject6.GetValue<int>("arrayFlags") >> 4) & 0x1Fu);
                            if (ebxFieldType - 2 <= EbxFieldType.DbObject || ebxFieldType == EbxFieldType.Enum)
                            {
                                fieldObj.AddValue("baseType", baseTypeName);
                            }
                            fieldObj.AddValue("arrayFlags", dbObject6.GetValue<int>("arrayFlags"));
                        }
                        if (dbObject6.HasValue("guid"))
                        {
                            fieldObj.SetValue("guid", dbObject6.GetValue<Guid>("guid"));
                        }
                    }
                    if (field.DebugType == EbxFieldType.ResourceRef || field.DebugType == EbxFieldType.TypeRef || field.DebugType == EbxFieldType.FileRef || field.DebugType == EbxFieldType.BoxedValueRef)
                    {
                        //while (offset % 8 != 0)
                        //{
                        //    offset++;
                        //}
                    }
                    else if (field.DebugType == EbxFieldType.Array || field.DebugType == EbxFieldType.Pointer)
                    {
                        while (offset % 4 != 0)
                        {
                            offset++;
                        }
                    }
                    fieldObj.AddValue("offset", offset);
                    fieldObj.SetValue("index", dbObject6.GetValue<int>("index") + fieldIndex);
                    dbObject4.List.Add(fieldObj);
                    switch (field.DebugType)
                    {
                        case EbxFieldType.Struct:
                            {
                                Tuple<EbxClass, DbObject> tuple = values.Find((Tuple<EbxClass, DbObject> a) => a.Item1.Name == fieldObj.GetValue<string>("baseType"));
                                int offset2 = 0;
                                int fieldIndex2 = 0;
                                offset += ProcessClass(tuple.Item1, tuple.Item2, fieldMapping[tuple.Item1.Name], outList, ref offset2, ref fieldIndex2);
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
                        case EbxFieldType.UInt64:
                            offset += 8;
                            break;
                        case EbxFieldType.Int64:
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
            if (!ProfilesLibrary.IsFIFA22DataVersion())
            {
                while (offset % (int)pclass.Alignment != 0)
                {
                    offset++;
                }
            }
            pobj.SetValue("flags", (int)pclass.Type);
            pobj.SetValue("size", offset);
            if (pclass.DebugType == EbxFieldType.Enum)
            {
                pobj.SetValue("size", 4);
            }
            pobj.SetValue("alignment", (int)pclass.Alignment);
            pobj.SetValue("fields", dbObject4);
            fieldIndex += dbObject4.Count;
            if (dboClassMetaList != null)
            {
                pobj.AddValue("meta", dboClassMetaList);
                foreach (DbObject dbObject5 in dboClassMetaList.GetValue<DbObject>("fields").List)
                {
                    if (dbObject5.GetValue<bool>("added"))
                    {
                        DbObject dbObject9 = DbObject.CreateObject();
                        dbObject9.AddValue("name", dbObject5.GetValue<string>("name"));
                        dbObject9.AddValue("type", 15);
                        dbObject9.AddValue("meta", dbObject5);
                        pobj.GetValue<DbObject>("fields").List.Add(dbObject9);
                    }
                }
            }
            outList.List.Add(pobj);
            return offset;
        }

        public static Random RandomEmpty = new Random();

        private DbObject DumpClasses(SdkUpdateTask task)
        {
            MemoryReader memoryReader = null;
            //string typeStr = "v2k4FIFASDKGenerator.ClassesSdkCreator+ClassInfo";
            //string typeStr = "v2k4FIFASDKGenerator.Madden21.ClassInfo";
            //string typeStr = "SdkGenerator.Madden20.ClassInfo";
            string typeStr = "SdkGenerator.BaseInfo.ClassInfo";

            if (!string.IsNullOrEmpty(ProfilesLibrary.SDKGeneratorClassInfoType))
            {
                typeStr = ProfilesLibrary.SDKGeneratorClassInfoType;
            }
            else
            {

                //if (ProfilesLibrary.DataVersion == 20181207)
                //{
                //    str = "FrostyEditor.Anthem.";
                //}
                //else if (ProfilesLibrary.IsMadden20DataVersion())
                //{
                //    str = "FrostyEditor.Madden20.";
                //}
                //else if (ProfilesLibrary.DataVersion == 20190905)
                //{
                //    str = "FrostyEditor.Madden20.";
                //}
                //else 
                switch (ProfilesLibrary.DataVersion)
                {

                    case (int)ProfilesLibrary.DataVersions.FIFA19:
                        typeStr = "SdkGenerator.BaseInfo.ClassInfo";
                        break;
                    case (int)ProfilesLibrary.DataVersions.FIFA20:
                        typeStr = "SdkGenerator.Madden20.ClassInfo";
                        break;
                    case (int)ProfilesLibrary.DataVersions.MADDEN20:
                        typeStr = "SdkGenerator.Madden20.ClassInfo";
                        break;
                    case (int)ProfilesLibrary.DataVersions.FIFA21:
                        typeStr = "SdkGenerator.FIFA21.ClassInfo";
                        break;
                    //case (int)ProfilesLibrary.DataVersions.MADDEN21:
                    //    typeStr = "SdkGenerator.Madden21.ClassInfo";
                    //    break;
                    default:
                        typeStr = "SdkGenerator.BaseInfo.ClassInfo";
                        break;

                }
            }
          

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

            //var t = AssetManager.LoadTypeByName(typeStr);
            //IClassInfo classInfo = (IClassInfo)t;
            while (offset != 0L)
            {
                task.StatusMessage = $"Found {++num} type(s)";
                memoryReader.Position = offset;
                //var t = Type.GetType(typeStr);
                //IClassInfo classInfo = (IClassInfo)Activator.CreateInstance(t);
                var t = AssetManager.Instance.LoadTypeByName(typeStr);
                IClassInfo classInfo = (IClassInfo)t;
                classInfo.Read(memoryReader);
                classInfos.Add(classInfo);
                if (!offsetClassInfoMapping.ContainsKey(typeInfoOffset))
                    offsetClassInfoMapping.Add(typeInfoOffset, classInfo);
                else
                    break;
                //if (offset != 0L)
                //{
                //    typeInfoOffset = offset;
                //}
                if (classInfo.nextOffset != 0L)
                {
                    offset = classInfo.nextOffset;
                    typeInfoOffset = offset;
                }
                else if(offset != 0)
                {
                    typeInfoOffset = offset;
                }
            }
            Debug.WriteLine(task.StatusMessage);
            
            memoryReader.Dispose();
            memoryReader = null;

            DbObject result = new DbObject(bObject: false);
            classInfos.Sort((IClassInfo a, IClassInfo b) => a.typeInfo.name.CompareTo(b.typeInfo.name));

            var findSomeStuffTest = classInfos.Where(x => x.typeInfo.name.ToLower().Contains("lua")).ToList();

            foreach (IClassInfo classInfo2 in classInfos)
            {
                if(classInfo2.typeInfo.name.Equals("RenderFormat", StringComparison.OrdinalIgnoreCase))
                {

                }

                if (classInfo2.typeInfo.Type == 2
                    || classInfo2.typeInfo.Type == 3
                    || classInfo2.typeInfo.Type == 8
                    || classInfo2.typeInfo.Type == 24
                    || classInfo2.typeInfo.Type == 27)
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

        private void CreateBasicClassObject(IClassInfo classInfo, ref DbObject classList)
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

        private void CreateClassObject(IClassInfo classInfo, ref DbObject classList)
        {
            if (!alreadyProcessedClasses.Contains(classInfo.typeInfo.name))
            {
                IClassInfo classInfo2 = offsetClassInfoMapping.ContainsKey(classInfo.parentClass) ? offsetClassInfoMapping[classInfo.parentClass] : null;
                if (classInfo2 != null)
                {
                    CreateClassObject(classInfo2, ref classList);
                }
                int alignment = classInfo.typeInfo.alignment;
                int size = (int)classInfo.typeInfo.size;
                DbObject dbObject = new DbObject();
                dbObject.AddValue("name", classInfo.typeInfo.name);
                dbObject.AddValue("nameHash", classInfo.typeInfo.nameHash);
                if (classInfo.typeInfo.name.Contains("actor_movement"))
                {

                }
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
                //classInfo.typeInfo.Modify(dbObject);
                DbObject dbObject2 = new DbObject(bObject: false);
                foreach (IFieldInfo field in classInfo.typeInfo.fields)
                {
                    DbObject dbObject3 = new DbObject();
                    if (classInfo.typeInfo.Type == 8)
                    {
                        dbObject3.AddValue("name", field.name);
                        dbObject3.AddValue("value", (int)field.typeOffset);
                    }
                    else if(offsetClassInfoMapping.ContainsKey(field.typeOffset))
                    {
                        IClassInfo classInfo3 = offsetClassInfoMapping[field.typeOffset];
                        dbObject3.AddValue("name", field.name);
                        dbObject3.AddValue("nameHash", field.nameHash);
                        dbObject3.AddValue("type", classInfo3.typeInfo.Type);
                        dbObject3.AddValue("flags", (int)classInfo3.typeInfo.flags);
                        dbObject3.AddValue("offset", (int)field.offset);
                        dbObject3.AddValue("index", field.index);
                        if (classInfo3.typeInfo.Type == 3
                            || classInfo3.typeInfo.Type == 2 
                            || classInfo3.typeInfo.Type == 8)
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
    

	public static class NeededExtension
    {
		public static int FindIndex<T>(this IList<T> source, int startIndex,
							   Predicate<T> match)
		{
			// TODO: Validation
			for (int i = startIndex; i < source.Count; i++)
			{
				if (match(source[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static int FindIndex<T>(this IList<T> source,
							   Predicate<T> match)
		{
			// TODO: Validation
			for (int i = 0; i < source.Count; i++)
			{
				if (match(source[i]))
				{
					return i;
				}
			}
			return -1;
		}
	}
}
