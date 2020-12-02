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

        /// <summary>
        /// Reads the entire SBFile from the Associated TOC Bundles
        /// </summary>
        /// <param name="nativeReader"></param>
        /// <returns></returns>
        public List<DbObject> Read(NativeReader nativeReader)
        {
            if (NativeFileLocation.Contains("contentsb"))
            {

            }

            CachingSBData cachingSBData = new CachingSBData();
            cachingSBData.SBFile = NativeFileLocation;


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
                    Name = Guid.NewGuid().ToString(),
                    SuperBundleId = SuperBundleIndex
                };
                using (NativeReader binarySbReader2 = new NativeReader(nativeReader.CreateViewStream(BaseBundleItem.Offset, nativeReader.Length - BaseBundleItem.Offset)))
                {
                    CachingSBData.Bundle cachingSBDataBundle = ReadInternalBundle((int)BaseBundleItem.Offset, ref dbObject, binarySbReader2);

                    cachingSBData.Bundles.Add(cachingSBDataBundle);

                }

                AssetManager.Instance.bundles.Add(bundleEntry);
                dbObjects.Add(dbObject);
                index++;
            }

            CachingSB.CachingSBs.Add(cachingSBData);
            CachingSB.Save();
            return dbObjects;
        }

        public class BundleHeader
        {
            public uint CatalogOffset { get; set; }
            public uint unk1 { get; set; }
            public uint casFileForGroupOffset { get; set; }
            public uint unk2 { get; set; }
            public uint CatalogAndCASOffset { get; set; }

        }

        /// <summary>
        /// Reads the reader from a viewstream of the internal bundle
        /// </summary>
        /// <param name="BaseBundleItem"></param>
        /// <param name="dbObject"></param>
        /// <param name="binarySbReader2"></param>
        /// <returns></returns>
        public CachingSBData.Bundle ReadInternalBundle(int bundleOffset, ref DbObject dbObject, NativeReader binarySbReader2)
        {
            if (File.Exists("debugSBViewStream.dat"))
                File.Delete("debugSBViewStream.dat");
            using (NativeWriter writer = new NativeWriter(new FileStream("debugSBViewStream.dat", FileMode.OpenOrCreate)))
            {
                writer.Write(binarySbReader2.ReadToEnd());
            }
            binarySbReader2.Position = 0;

            var cachingSBDataBundle = new CachingSBData.Bundle();
            cachingSBDataBundle.StartOffset = (int)bundleOffset;

            uint CatalogOffset = binarySbReader2.ReadUInt(Endian.Big);
            uint EndOfMeta = binarySbReader2.ReadUInt(Endian.Big) + CatalogOffset - 4; // end of META
            uint casFileForGroupOffset = binarySbReader2.ReadUInt(Endian.Big);
            var unk2 = binarySbReader2.ReadUInt(Endian.Big);
            uint CatalogAndCASOffset = binarySbReader2.ReadUInt(Endian.Big);

            cachingSBDataBundle.BundleHeader = new BundleHeader()
            {
                CatalogOffset = CatalogOffset
                 ,
                unk1 = EndOfMeta
                 ,
                casFileForGroupOffset = casFileForGroupOffset
                 ,
                unk2 = unk2
                 ,
                CatalogAndCASOffset = CatalogAndCASOffset
            };

            // read 3 unknowns 
            //binarySbReader2.Position += 12;
            _ = binarySbReader2.ReadInt(Endian.Big);
            _ = binarySbReader2.ReadInt(Endian.Big);
            _ = binarySbReader2.ReadInt(Endian.Big);

            // ---------------------------------------------------------------------------------------------------------------------
            // This is where it hits the Binary SB Reader. FIFA 21 is more like MADDEN 21 in this section

            cachingSBDataBundle.BinaryDataOffset = (int)binarySbReader2.Position;
            //SBHeaderInformation SBHeaderInformation = BinaryRead_FIFA21(nativeReader, BaseBundleItem, dbObject, binarySbReader2);
            SBHeaderInformation SBHeaderInformation = new BinaryReader_FIFA21().BinaryRead_FIFA21(bundleOffset, ref dbObject, binarySbReader2, true);
            cachingSBDataBundle.BinaryDataOffsetEnd = (int)binarySbReader2.Position;

            binarySbReader2.Position = cachingSBDataBundle.BinaryDataOffset;
            //cachingSBDataBundle.BinaryDataData = binarySbReader2.ReadBytes((int)cachingSBDataBundle.BinaryDataOffsetEnd - (int)cachingSBDataBundle.BinaryDataOffset);

            // END OF BINARY READER
            // ---------------------------------------------------------------------------------------------------------------------

            binarySbReader2.Position = casFileForGroupOffset;
            cachingSBDataBundle.BooleanOfCasGroupOffset = (int)binarySbReader2.Position;
            byte[] boolChangeOfCasData = new byte[SBHeaderInformation.totalCount];

            bool[] booleanChangeOfCas = new bool[SBHeaderInformation.totalCount];
            for (uint booleanIndex = 0u; booleanIndex < SBHeaderInformation.totalCount; booleanIndex++)
            {
                booleanChangeOfCas[booleanIndex] = binarySbReader2.ReadBoolean();
                boolChangeOfCasData[booleanIndex] = Convert.ToByte(booleanChangeOfCas[booleanIndex]);
            }
            //cachingSBDataBundle.BooleanOfCasGroupData = boolChangeOfCasData;
            cachingSBDataBundle.BooleanOfCasGroupOffsetEnd = binarySbReader2.Position;

            binarySbReader2.Position = CatalogAndCASOffset;
            cachingSBDataBundle.CatalogCasGroupOffset = binarySbReader2.Position;

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
                ebxObject.SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);

                ebxObject.SetValue("SB_CAS_Offset_Position", binarySbReader2.Position + bundleOffset);
                int offset = binarySbReader2.ReadInt(Endian.Big);

                ebxObject.SetValue("SB_CAS_Size_Position", binarySbReader2.Position + bundleOffset);
                int size = binarySbReader2.ReadInt(Endian.Big);

                ebxObject.SetValue("catalog", catalog);
                ebxObject.SetValue("cas", cas);
                ebxObject.SetValue("offset", offset);
                ebxObject.SetValue("size", size);
                if (patchFlag)
                {
                    ebxObject.SetValue("patch", true);

                    cachingSBDataBundle.LastCAS = cas;
                    cachingSBDataBundle.LastCatalogId = catalog;
                }

            }

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
                resObject.SetValue("SB_CAS_Offset_Position", binarySbReader2.Position + bundleOffset);
                int offset = binarySbReader2.ReadInt(Endian.Big);
                resObject.SetValue("SB_CAS_Size_Position", binarySbReader2.Position + bundleOffset);
                int size = binarySbReader2.ReadInt(Endian.Big);

                resObject.SetValue("SBFileLocation", NativeFileLocation);
                resObject.SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);

                //if (catalog >= 0 && catalog < AssetManager.Instance.fs.CatalogCount - 1)
                resObject.SetValue("catalog", catalog);
                //if(cas > 0)
                resObject.SetValue("cas", cas);
                resObject.SetValue("offset", offset);

                resObject.SetValue("size", size);
                if (patchFlag)
                {
                    resObject.SetValue("patch", true);

                    cachingSBDataBundle.LastCAS = cas;
                    cachingSBDataBundle.LastCatalogId = catalog;
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

                chnkObj.SetValue("SB_CAS_Offset_Position", binarySbReader2.Position + bundleOffset);
                int offset = binarySbReader2.ReadInt(Endian.Big);
                chnkObj.SetValue("SB_CAS_Size_Position", binarySbReader2.Position + bundleOffset);
                int size = binarySbReader2.ReadInt(Endian.Big);

                //if (catalog < 0 || catalog > AssetManager.Instance.fs.CatalogCount - 1)
                //{

                //}
                chnkObj.SetValue("SBFileLocation", NativeFileLocation);
                chnkObj.SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);
                //if (catalog >= 0 && catalog < AssetManager.Instance.fs.CatalogCount - 1)
                chnkObj.SetValue("catalog", catalog);
                //if(cas > 0)
                chnkObj.SetValue("cas", cas);

                chnkObj.SetValue("offset", offset);

                chnkObj.SetValue("size", size);
                if (patchFlag)
                {
                    chnkObj.SetValue("patch", true);

                    cachingSBDataBundle.LastCAS = cas;
                    cachingSBDataBundle.LastCatalogId = catalog;
                }
            }

            // 
            //cachingSBDataBundle.LastCAS = cas;
            //cachingSBDataBundle.LastCatalogId = catalog;

            cachingSBDataBundle.CatalogCasGroupOffsetEnd = (int)binarySbReader2.Position;
            if(cachingSBDataBundle.CatalogCasGroupOffsetEnd == 0)
            {

            }
            binarySbReader2.Position = cachingSBDataBundle.CatalogCasGroupOffset;
            //cachingSBDataBundle.CatalogCasGroupData = binarySbReader2.ReadBytes((int)cachingSBDataBundle.CatalogCasGroupOffsetEnd - (int)cachingSBDataBundle.CatalogCasGroupOffset);


            return cachingSBDataBundle;
        }

    }

   

}
