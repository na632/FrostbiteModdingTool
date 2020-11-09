using Frosty.Hash;
using FrostySdk;
using FrostySdk.Deobfuscators;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static FIFA21Plugin.FIFA21AssetLoader;

namespace FIFA21Plugin
{
    

    public class SBFile
    {
        public TOCFile AssociatedTOCFile { get; set; }
        public string NativeFileLocation { get; set; }
        public string FileLocation { get; set; }

        public int SBInitialHeaderLength = 32;
        public int SBInformationHeaderLength = 36;

        public int[] ArrayOfInitialHeaderData = new int[8];

        public SBHeaderInformation SBHeaderInformation { get; set; }

        List<BundleEntryInfo> Bundles = new List<BundleEntryInfo>();

        private int SuperBundleIndex = 0;

        private TocSbReader_FIFA21 ParentReader;

        public SBFile() { }
        public SBFile(TocSbReader_FIFA21 parent, TOCFile parentTOC, int sbIndex)
        {
            ParentReader = parent;
            AssociatedTOCFile = parentTOC;
            SuperBundleIndex = sbIndex;
        }

        public struct EBX
        {

        }

        public struct RES
        {

        }

        public struct CHUNK
        {

        }

        public List<DbObject> Read(NativeReader nativeReader)
        {
            if (NativeFileLocation.Contains("contentsb"))
            {

            }


            List<DbObject> dbObjects = new List<DbObject>();

            var startOffset = nativeReader.Position;
            if (File.Exists("debugSB.dat"))
                File.Delete("debugSB.dat");
            using (NativeWriter writer = new NativeWriter(new FileStream("debugSB.dat", FileMode.OpenOrCreate)))
            {
                writer.Write(nativeReader.ReadToEnd());
            }
            nativeReader.Position = startOffset;
            var index = 0;
            foreach (BaseBundleInfo BaseBundleItem in AssociatedTOCFile.Bundles)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());

                BundleEntry bundleEntry = new BundleEntry
                {
                    //Name = AssociatedTOCFile.SuperBundleName + "_" + index.ToString(), 
                    Name = Guid.NewGuid().ToString(),
                    SuperBundleId = SuperBundleIndex
                };
                using (NativeReader binarySbReader2 = new NativeReader(nativeReader.CreateViewStream(BaseBundleItem.Offset, nativeReader.Length - BaseBundleItem.Offset)))
                {
                    if (File.Exists("debugSBViewStream.dat"))
                        File.Delete("debugSBViewStream.dat");
                    using (NativeWriter writer = new NativeWriter(new FileStream("debugSBViewStream.dat", FileMode.OpenOrCreate)))
                    {
                        writer.Write(binarySbReader2.ReadToEnd());
                    }
                    binarySbReader2.Position = 0;


                    uint CatalogOffset = binarySbReader2.ReadUInt(Endian.Big);
                    uint unk1 = binarySbReader2.ReadUInt(Endian.Big) + CatalogOffset;
                    uint casFileForGroupOffset = binarySbReader2.ReadUInt(Endian.Big);
                    var unk2 = binarySbReader2.ReadUInt(Endian.Big);
                    uint CatalogAndCASOffset = binarySbReader2.ReadUInt(Endian.Big);

                    binarySbReader2.Position += 12;

                    // ---------------------------------------------------------------------------------------------------------------------
                    // This is where it hits the Binary SB Reader. FIFA 21 is more like MADDEN 21 in this section

                    //SBHeaderInformation SBHeaderInformation = BinaryRead_FIFA21(nativeReader, BaseBundleItem, dbObject, binarySbReader2);
                    SBHeaderInformation SBHeaderInformation = new BinaryReader_FIFA21().BinaryRead_FIFA21(BaseBundleItem, ref dbObject, binarySbReader2, true);

                    // END OF BINARY READER
                    // ---------------------------------------------------------------------------------------------------------------------


                    binarySbReader2.Position = casFileForGroupOffset;
                    bool[] booleanChangeOfCas = new bool[SBHeaderInformation.totalCount];
                    for (uint booleanIndex = 0u; booleanIndex < SBHeaderInformation.totalCount; booleanIndex++)
                    {
                        booleanChangeOfCas[booleanIndex] = binarySbReader2.ReadBoolean();
                    }
                    binarySbReader2.Position = CatalogAndCASOffset;

                    bool patchFlag = false;
                    int unkInBatch1 = 0;
                    int catalog = 0;
                    int cas = 0;
                    int flagIndex = 0;

                    var ebxCount = dbObject.GetValue<DbObject>("ebx").Count;
                    for (int ebxIndex = 0; ebxIndex < ebxCount; ebxIndex++)
                    {
                        if (booleanChangeOfCas[flagIndex++])
                        {
                            unkInBatch1 = binarySbReader2.ReadByte();
                            patchFlag = binarySbReader2.ReadBoolean();
                            catalog = binarySbReader2.ReadByte();
                            cas = binarySbReader2.ReadByte();
                        }
                        DbObject ebxObject = dbObject.GetValue<DbObject>("ebx")[ebxIndex] as DbObject;

                        ebxObject.SetValue("SBFileLocation", NativeFileLocation);
                        ebxObject.SetValue("TOCFileLocation", AssociatedTOCFile.FileLocation);

                        if (ebxObject.GetValue<string>("name", "").Contains("movement"))
                        {
                        }
                        ebxObject.SetValue("SB_CAS_Offset_Position", binarySbReader2.Position + BaseBundleItem.Offset);
                        int offset = binarySbReader2.ReadInt(Endian.Big);

                        ebxObject.SetValue("SB_CAS_Size_Position", binarySbReader2.Position + BaseBundleItem.Offset);
                        int size = binarySbReader2.ReadInt(Endian.Big);

                        ebxObject.SetValue("catalog", catalog);
                        ebxObject.SetValue("cas", cas);
                        ebxObject.SetValue("offset", offset);
                        ebxObject.SetValue("size", size);
                        if (patchFlag)
                        {
                            ebxObject.SetValue("patch", true);
                        }

                    }

                    var positionBeforeRes = binarySbReader2.Position;



                    for (int indexRes = 0; indexRes < dbObject.GetValue<DbObject>("res").Count; indexRes++)
                    {
                        if (booleanChangeOfCas[flagIndex++])
                        {
                            var b = binarySbReader2.ReadByte();
                            patchFlag = binarySbReader2.ReadBoolean();
                            catalog = binarySbReader2.ReadByte();
                            cas = binarySbReader2.ReadByte();
                        }
                        DbObject resObject = dbObject.GetValue<DbObject>("res")[indexRes] as DbObject;
                        resObject.SetValue("SB_CAS_Offset_Position", binarySbReader2.Position + BaseBundleItem.Offset);
                        int offset = binarySbReader2.ReadInt(Endian.Big);
                        resObject.SetValue("SB_CAS_Size_Position", binarySbReader2.Position + BaseBundleItem.Offset);
                        int size = binarySbReader2.ReadInt(Endian.Big);

                        resObject.SetValue("SBFileLocation", NativeFileLocation);
                        resObject.SetValue("TOCFileLocation", AssociatedTOCFile.FileLocation);

                        //if (catalog >= 0 && catalog < AssetManager.Instance.fs.CatalogCount - 1)
                        resObject.SetValue("catalog", catalog);
                        //if(cas > 0)
                        resObject.SetValue("cas", cas);


                        resObject.SetValue("offset", offset);

                        resObject.SetValue("size", size);
                        if (patchFlag)
                        {
                            resObject.SetValue("patch", true);
                        }
                    }
                    for (int indexChunk = 0; indexChunk < dbObject.GetValue<DbObject>("chunks").Count; indexChunk++)
                    {
                        if (booleanChangeOfCas[flagIndex++])
                        {
                            var b = binarySbReader2.ReadByte();
                            patchFlag = binarySbReader2.ReadBoolean();
                            catalog = binarySbReader2.ReadByte();
                            cas = binarySbReader2.ReadByte();
                        }
                        DbObject chnkObj = dbObject.GetValue<DbObject>("chunks")[indexChunk] as DbObject;

                        chnkObj.SetValue("SB_CAS_Offset_Position", binarySbReader2.Position + BaseBundleItem.Offset);
                        int offset = binarySbReader2.ReadInt(Endian.Big);
                        chnkObj.SetValue("SB_CAS_Size_Position", binarySbReader2.Position + BaseBundleItem.Offset);
                        int size = binarySbReader2.ReadInt(Endian.Big);

                        //if (catalog < 0 || catalog > AssetManager.Instance.fs.CatalogCount - 1)
                        //{

                        //}
                        chnkObj.SetValue("SBFileLocation", NativeFileLocation);
                        chnkObj.SetValue("TOCFileLocation", AssociatedTOCFile.FileLocation);
                        //if (catalog >= 0 && catalog < AssetManager.Instance.fs.CatalogCount - 1)
                        chnkObj.SetValue("catalog", catalog);
                        //if(cas > 0)
                        chnkObj.SetValue("cas", cas);

                        chnkObj.SetValue("offset", offset);

                        chnkObj.SetValue("size", size);
                        if (patchFlag)
                        {
                            chnkObj.SetValue("patch", true);
                        }
                    }




                }

                AssetManager.Instance.bundles.Add(bundleEntry);
                dbObjects.Add(dbObject);
                index++;
            }

            return dbObjects;
        }

       
    }

   

}
