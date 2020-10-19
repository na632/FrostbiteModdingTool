using Frosty.Hash;
using FrostySdk;
using FrostySdk.Frostbite.IO;
using FrostySdk.IO;
using FrostySdk.Managers;
using paulv2k4ModdingExecuter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using static paulv2k4ModdingExecuter.FrostyModExecutor;

namespace FIFA21Plugin
{

    public class FIFA21BundleAction
    {
        public class BundleFileEntry
        {
            public int CasIndex;

            public int Offset;

            public int Size;

            public string Name;

            public BundleFileEntry(int inCasIndex, int inOffset, int inSize, string inName = null)
            {
                CasIndex = inCasIndex;
                Offset = inOffset;
                Size = inSize;
                Name = inName;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return $"CASIdx({CasIndex}), Offset({Offset}), Size({Size}), Name({Name})";

            }
        }

        private static readonly object locker = new object();

        public static int CasFileCount = 0;

        private Exception errorException;

        private ManualResetEvent doneEvent;

        private FrostyModExecutor parent;

        private CatalogInfo catalogInfo;

        private Dictionary<int, string> casFiles = new Dictionary<int, string>();

        public CatalogInfo CatalogInfo => catalogInfo;

        public Dictionary<int, string> CasFiles => casFiles;

        public bool HasErrored => errorException != null;

        public Exception Exception => errorException;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inCatalogInfo"></param>
        /// <param name="inDoneEvent"></param>
        /// <param name="inParent"></param>
        public FIFA21BundleAction(CatalogInfo inCatalogInfo, FrostyModExecutor inParent)
        {
            catalogInfo = inCatalogInfo;
            parent = inParent;
        }

        public FIFA21BundleAction(FrostyModExecutor inParent)
        {
            parent = inParent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location_toc_file"></param>
        /// <param name="toc_reader_starting_position"></param>
        /// <param name="sbToc_reader_other_starting_position"></param>
        /// <param name="garbage_section_array"></param>
        /// <returns></returns>
        public byte[] ReadTocIntoByteArray(string location_toc_file
            , out int toc_reader_starting_position
            , out int sbToc_reader_other_starting_position
            , out byte[] garbage_section_array)
        {
            byte[] key_2_from_key_manager = KeyManager.Instance.GetKey("Key2");
            byte[] toc_array = null;
            uint initialHash = 0;
            using (NativeReader nativeReader = new NativeReader(new FileStream(location_toc_file, FileMode.Open, FileAccess.Read)))
            {
                garbage_section_array = nativeReader.ReadBytes(556);
                for (var i = 4; i < 556; i++)
                {
                    garbage_section_array[i] = 0;
                }


                // At position 22C / 556
                nativeReader.Position = 556;
                toc_array = nativeReader.ReadToEnd();
                nativeReader.Position = 556;
                initialHash = nativeReader.ReadUInt();
                toc_reader_starting_position = nativeReader.ReadInt();
                sbToc_reader_other_starting_position = nativeReader.ReadInt();
            }

            //using (NativeWriter nativeWriter = new NativeWriter(new FileStream("test_sb_toc.dat", FileMode.OpenOrCreate)))
            //{
            //    //nativeWriter.Write(garbage_section_array);
            //    //nativeWriter.Write(initialHash);
            //    //nativeWriter.Write(toc_reader_starting_position + 12);
            //    //nativeWriter.Write(sbToc_reader_other_starting_position + 12);
            //    nativeWriter.Write(toc_array);
            //}

            return toc_array;
        }


        /// <summary>
        /// Finds what bundles need to be modified
        /// </summary>
        /// <param name="original_toc_file_byte_array"></param>
        /// <param name="list_bundle_entries"></param>
        /// <param name="list_modified_bundles"></param>
        public void GetModifiedBundles(
            byte[] original_toc_file_byte_array
            , out List<BundleFileEntry> list_bundle_entries
            , out Dictionary<ModBundleInfo, List<BundleFileEntry>> list_modified_bundles
            , out List<int> IsEBXList
            )
        {
            list_bundle_entries = new List<BundleFileEntry>();
            list_modified_bundles = new Dictionary<ModBundleInfo, List<BundleFileEntry>>();
            using (NativeReader reader_of_original_toc_file_array = new NativeReader(new MemoryStream(original_toc_file_byte_array)))
            {
                const int repositioning_value = 4;
                const int header_length = 556;

                if(reader_of_original_toc_file_array.ReadUInt() != 3280507699u)
                {
                    throw new Exception("TOC file should start with 3280507699 at position 556 (0)");
                }
                // Position 4 is where the position for the ebx item count is
                reader_of_original_toc_file_array.Position = repositioning_value;
                // read the item count and reposition
                var itemCountPosition = reader_of_original_toc_file_array.ReadInt();
                reader_of_original_toc_file_array.Position = itemCountPosition;

                // read out the count and add the items to the list
                int parentBundlesCount = reader_of_original_toc_file_array.ReadInt();
                IsEBXList = new List<int>();
                for (int i = 0; i < parentBundlesCount; i++)
                {
                    IsEBXList.Add(reader_of_original_toc_file_array.ReadInt());

                }

                //
                List<int> list2 = new List<int>();
                var indexesToNameStartPoint = 0;
                for (int j = 0; j < parentBundlesCount; j++)
                {
                    // -------------------------------------------------
                    // Find the Bundle Entries

                    int positionofcasfiles = reader_of_original_toc_file_array.ReadInt();
                    long readablepositionofcasfiles = reader_of_original_toc_file_array.Position;
                    reader_of_original_toc_file_array.Position = positionofcasfiles;
                    int positionofstring = reader_of_original_toc_file_array.ReadInt() - 1;


                    int casIndex = 0;
                    var casIndexChecker = casIndex & 2147483648u;
                    do
                    {
                        casIndex = reader_of_original_toc_file_array.ReadInt();
                        casIndexChecker = casIndex & 2147483648u;

                        int inOffset = reader_of_original_toc_file_array.ReadInt();
                        int inSize = reader_of_original_toc_file_array.ReadInt();
                        list_bundle_entries.Add(new BundleFileEntry((int)casIndex & int.MaxValue, inOffset, inSize));
                    }
                    while (casIndexChecker != 0L);

                    //
                    // --------------------------------------------------


                    // --------------------------------------------------
                    // Get out the objects that have been changed
                    reader_of_original_toc_file_array.Position = positionofstring;
                    int num10 = 0;
                    string name = "";
                    do
                    {
                        string str = reader_of_original_toc_file_array.ReadNullTerminatedString();
                        num10 = reader_of_original_toc_file_array.ReadInt() - 1;
                        name = Utils.ReverseString(str) + name;
                        if (num10 != -1)
                        {
                            reader_of_original_toc_file_array.Position = num10;
                        }

                    }
                    while (num10 != -1) ;
                    reader_of_original_toc_file_array.Position = readablepositionofcasfiles;

                    for(var i = indexesToNameStartPoint; i < list_bundle_entries.Count; i++)
                    {
                        list_bundle_entries[i].Name = name;
                    }

                    indexesToNameStartPoint = list_bundle_entries.Count - 1;

                    // --------------------------------------------------
                    // discover the object in the modified bundles
                    int hash_key = Fnv1.HashString(name.ToLower());
                    if (parent.modifiedBundles.ContainsKey(hash_key))
                    {
                        ModBundleInfo modBundleInfo = parent.modifiedBundles[hash_key];
                        list_modified_bundles.Add(modBundleInfo, list_bundle_entries);
                    }
                }
            }
        }

        private void WriteBundleToCASFile(ref Dictionary<ModBundleInfo, List<BundleFileEntry>> list_of_modified_bundles)
        {
            int casFileIndex = 0;
            NativeWriter writer_new_cas_file = GetNextCas(out casFileIndex);

            List<BundleFileEntry> bundle_file_list = null;
            foreach (var kvp in list_of_modified_bundles)
            {
                bundle_file_list = kvp.Value;
                var modBundleInfo = kvp.Key;

                MemoryStream memoryStream = new MemoryStream();
                foreach (BundleFileEntry item in bundle_file_list)
                {
                    using (NativeReader nativeReader3 = new NativeReader(new FileStream(parent.fs.ResolvePath(parent.fs.GetFilePath(item.CasIndex)), FileMode.Open, FileAccess.Read)))
                    {
                        nativeReader3.Position = item.Offset;
                        memoryStream.Write(nativeReader3.ReadBytes(item.Size), 0, item.Size);
                    }
                }

                if (memoryStream.Length == 0)
                    throw new Exception("Memory is empty, must have read from the wrong CAS file");

                DbObject dbObject = null;

                // get the data out of the original CAS
                using (BinarySbReader_M21 binarySbReader = new BinarySbReader_M21(memoryStream, 0L, parent.fs.CreateDeobfuscator()))
                {
                    dbObject = binarySbReader.ReadDbObject();
                    foreach (DbObject ebxItem in dbObject.GetValue<DbObject>("ebx"))
                    {
                        ebxItem.GetValue("size", 0);
                        long offsetValue = ebxItem.GetValue("offset", 0L);
                        long currentOffsetLookup = 0L;
                        foreach (BundleFileEntry bundle in bundle_file_list)
                        {
                            if (offsetValue < currentOffsetLookup + bundle.Size)
                            {
                                offsetValue -= currentOffsetLookup;
                                offsetValue += bundle.Offset;
                                ebxItem.SetValue("offset", offsetValue);
                                ebxItem.SetValue("cas", bundle.CasIndex);
                                break;
                            }
                            currentOffsetLookup += bundle.Size;
                        }
                    }
                    foreach (DbObject item4 in dbObject.GetValue<DbObject>("res"))
                    {
                        item4.GetValue("size", 0);
                        long value2 = item4.GetValue("offset", 0L);
                        long num12 = 0L;
                        foreach (BundleFileEntry item5 in bundle_file_list)
                        {
                            if (value2 < num12 + item5.Size)
                            {
                                value2 -= num12;
                                value2 += item5.Offset;
                                item4.SetValue("offset", value2);
                                item4.SetValue("cas", item5.CasIndex);
                                break;
                            }
                            num12 += item5.Size;
                        }
                    }
                    foreach (DbObject chunk in dbObject.GetValue<DbObject>("chunks"))
                    {
                        chunk.GetValue("size", 0);
                        long value3 = chunk.GetValue("offset", 0L);
                        long num13 = 0L;
                        foreach (BundleFileEntry item7 in bundle_file_list)
                        {
                            if (value3 < num13 + item7.Size)
                            {
                                value3 -= num13;
                                value3 += item7.Offset;
                                chunk.SetValue("offset", value3);
                                chunk.SetValue("cas", item7.CasIndex);
                                break;
                            }
                            num13 += item7.Size;
                        }
                    }
                }

                NativeWriter WriterCASFileBody_ToMemory = new NativeWriter(new MemoryStream());

                    foreach (DbObject ebx in dbObject.GetValue<DbObject>("ebx"))
                    {
                        int num14 = modBundleInfo.Modify.Ebx.FindIndex((string a) => a.Equals(ebx.GetValue<string>("name")));
                        if (num14 != -1)
                        {
                            EbxAssetEntry ebxAssetEntry = parent.modifiedEbx[modBundleInfo.Modify.Ebx[num14]];
                            //if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + parent.archiveData[ebxAssetEntry.Sha1].Data.Length > 1073741824)
                            //{
                            //    writer_new_cas_file?.Close();
                            //    writer_new_cas_file = GetNextCas(out casFileIndex);
                            //}
                            ebx.SetValue("originalSize", ebxAssetEntry.OriginalSize);
                            ebx.SetValue("size", ebxAssetEntry.Size);
                            ebx.SetValue("cas", casFileIndex);
                            ebx.SetValue("offset", (int)WriterCASFileBody_ToMemory.BaseStream.Position);
                            var ebxData = parent.archiveData[ebxAssetEntry.Sha1].Data;
                            //using (FileStream fileStream = new FileStream("test.xmx", FileMode.OpenOrCreate))
                            //{
                            //    fileStream.Write(ebxData, 0, ebxData.Length);
                            //}

                            WriterCASFileBody_ToMemory.Write(ebxData);
                        }
                    }
                    foreach (DbObject res in dbObject.GetValue<DbObject>("res"))
                    {
                        int num15 = modBundleInfo.Modify.Res.FindIndex((string a) => a.Equals(res.GetValue<string>("name")));
                        if (num15 != -1)
                        {
                            ResAssetEntry resAssetEntry = parent.modifiedRes[modBundleInfo.Modify.Res[num15]];
                            //if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + parent.archiveData[resAssetEntry.Sha1].Data.Length > 1073741824)
                            //{
                            //    writer_new_cas_file?.Close();
                            //    writer_new_cas_file = GetNextCas(out casFileIndex);
                            //}
                            res.SetValue("originalSize", resAssetEntry.OriginalSize);
                            res.SetValue("size", resAssetEntry.Size);
                            res.SetValue("cas", casFileIndex);
                            res.SetValue("offset", (int)WriterCASFileBody_ToMemory.BaseStream.Position);
                            res.SetValue("resRid", (long)resAssetEntry.ResRid);
                            res.SetValue("resMeta", resAssetEntry.ResMeta);
                            res.SetValue("resType", resAssetEntry.ResType);
                            WriterCASFileBody_ToMemory.Write(parent.archiveData[resAssetEntry.Sha1].Data);
                        }
                    }
                    foreach (DbObject chunk in dbObject.GetValue<DbObject>("chunks"))
                    {
                        int num16 = modBundleInfo.Modify.Chunks.FindIndex((Guid a) => a == chunk.GetValue<Guid>("id"));
                        if (num16 != -1)
                        {
                            ChunkAssetEntry chunkAssetEntry = parent.modifiedChunks[modBundleInfo.Modify.Chunks[num16]];
                            //if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + parent.archiveData[chunkAssetEntry.Sha1].Data.Length > 1073741824)
                            //{
                            //    writer_new_cas_file?.Close();
                            //    writer_new_cas_file = GetNextCas(out casFileIndex);
                            //}
                            chunk.SetValue("originalSize", chunkAssetEntry.OriginalSize);
                            chunk.SetValue("size", chunkAssetEntry.Size);
                            chunk.SetValue("cas", casFileIndex);
                            chunk.SetValue("offset", (int)WriterCASFileBody_ToMemory.BaseStream.Position);
                            chunk.SetValue("logicalOffset", chunkAssetEntry.LogicalOffset);
                            chunk.SetValue("logicalSize", chunkAssetEntry.LogicalSize);
                            WriterCASFileBody_ToMemory.Write(parent.archiveData[chunkAssetEntry.Sha1].Data);
                        }
                    }
                    var originalBundle = bundle_file_list.OrderBy(x=>x.CasIndex).ToList()[0];
                bundle_file_list.Clear();
                bundle_file_list.Add(originalBundle);
                // grab out objects
                foreach (DbObject ebxObject in dbObject.GetValue<DbObject>("ebx"))
                {
                    bundle_file_list.Add(new BundleFileEntry(ebxObject.GetValue("cas", 0), ebxObject.GetValue("offset", 0), ebxObject.GetValue("size", 0)));
                }
                foreach (DbObject resObject in dbObject.GetValue<DbObject>("res"))
                {
                    bundle_file_list.Add(new BundleFileEntry(resObject.GetValue("cas", 0), resObject.GetValue("offset", 0), resObject.GetValue("size", 0)));
                }
                foreach (DbObject chunkObject in dbObject.GetValue<DbObject>("chunks"))
                {
                    bundle_file_list.Add(new BundleFileEntry(chunkObject.GetValue("cas", 0), chunkObject.GetValue("offset", 0), chunkObject.GetValue("size", 0)));
                }


                int ebxCount = dbObject.GetValue<DbObject>("ebx").Count;
                int resCount = dbObject.GetValue<DbObject>("res").Count;
                int chunkCount = dbObject.GetValue<DbObject>("chunks").Count;

                // Reverse of BinarySbReader_M21
                using (NativeWriter WriterCASFileHeader_ToMemory = new NativeWriter(new MemoryStream()))
                {
                    WriterCASFileHeader_ToMemory.Write(0xFFFFFFFF, Endian.Big); //Header size
                    WriterCASFileHeader_ToMemory.Write(3018715229u, Endian.Little); // Hash
                    WriterCASFileHeader_ToMemory.Write(ebxCount + resCount + chunkCount, Endian.Little); // Total files
                    WriterCASFileHeader_ToMemory.Write(ebxCount, Endian.Little); // ebx count
                    WriterCASFileHeader_ToMemory.Write(resCount, Endian.Little); // res count
                    WriterCASFileHeader_ToMemory.Write(chunkCount, Endian.Little); // chunk count
                    WriterCASFileHeader_ToMemory.Write(0xFFFFFFFF, Endian.Little); // string offset
                    WriterCASFileHeader_ToMemory.Write(0xFFFFFFFF, Endian.Little); // meta offset
                    WriterCASFileHeader_ToMemory.Write((int)0); // meta size
                    long stringOffset = 0L;
                    new Dictionary<uint, long>();
                    List<string> listOfStringValues = new List<string>();
                    foreach (DbObject ebxItem in dbObject.GetValue<DbObject>("ebx"))
                    {
                        WriterCASFileHeader_ToMemory.Write((uint)stringOffset, Endian.Little);
                        listOfStringValues.Add(ebxItem.GetValue<string>("name"));
                        stringOffset += ebxItem.GetValue<string>("name").Length + 1;
                        WriterCASFileHeader_ToMemory.Write(ebxItem.GetValue("originalSize", 0), Endian.Little);
                    }
                    foreach (DbObject resItem in dbObject.GetValue<DbObject>("res"))
                    {
                        WriterCASFileHeader_ToMemory.Write((uint)stringOffset, Endian.Little);
                        listOfStringValues.Add(resItem.GetValue<string>("name"));
                        stringOffset += resItem.GetValue<string>("name").Length + 1;
                        WriterCASFileHeader_ToMemory.Write(resItem.GetValue("originalSize", 0), Endian.Little);
                    }
                    foreach (DbObject resItem in dbObject.GetValue<DbObject>("res"))
                    {
                        WriterCASFileHeader_ToMemory.Write((uint)resItem.GetValue("resType", 0L), Endian.Little);
                    }
                    foreach (DbObject resItem in dbObject.GetValue<DbObject>("res"))
                    {
                        WriterCASFileHeader_ToMemory.Write(resItem.GetValue<byte[]>("resMeta"));
                    }
                    foreach (DbObject resItem in dbObject.GetValue<DbObject>("res"))
                    {
                        WriterCASFileHeader_ToMemory.Write(resItem.GetValue("resRid", 0L), Endian.Little);
                    }
                    foreach (DbObject chunkItem in dbObject.GetValue<DbObject>("chunks"))
                    {
                        WriterCASFileHeader_ToMemory.Write(chunkItem.GetValue<Guid>("id"), Endian.Little);
                        WriterCASFileHeader_ToMemory.Write(chunkItem.GetValue("logicalOffset", 0), Endian.Little);
                        WriterCASFileHeader_ToMemory.Write(chunkItem.GetValue("logicalSize", 0), Endian.Little);
                    }
                    long stringOffsetStart = WriterCASFileHeader_ToMemory.BaseStream.Position;
                    foreach (string item18 in listOfStringValues)
                    {
                        WriterCASFileHeader_ToMemory.WriteNullTerminatedString(item18);
                    }
                    long metaOffsetStart = 0L;
                    long num19 = 0L;
                    if (dbObject.GetValue<DbObject>("chunks").Count > 0)
                    {
                        DbObject value4 = dbObject.GetValue<DbObject>("chunkMeta");
                        metaOffsetStart = WriterCASFileHeader_ToMemory.BaseStream.Position;
                        using (DbWriter dbWriter = new DbWriter(new MemoryStream()))
                        {
                            WriterCASFileHeader_ToMemory.Write(dbWriter.WriteDbObject("chunkMeta", value4));
                        }
                        num19 = WriterCASFileHeader_ToMemory.BaseStream.Position - metaOffsetStart;
                    }



                    long headersize = WriterCASFileHeader_ToMemory.BaseStream.Position - 4;
                    WriterCASFileHeader_ToMemory.BaseStream.Position = 24L;

                    if(stringOffsetStart > 4)
                        WriterCASFileHeader_ToMemory.Write((uint)(stringOffsetStart - 4), Endian.Little); // String Offset
                    else
                        WriterCASFileHeader_ToMemory.Write((int)0, Endian.Little); // String Offset

                    if (metaOffsetStart > 4)
                        WriterCASFileHeader_ToMemory.Write((uint)(metaOffsetStart - 4), Endian.Little); // Meta Offset
                    else
                        WriterCASFileHeader_ToMemory.Write((uint)(0), Endian.Little); // Meta Offset

                    WriterCASFileHeader_ToMemory.Write((uint)(0), Endian.Little); // Meta Size 
                    WriterCASFileHeader_ToMemory.Flush();

                    WriterCASFileHeader_ToMemory.BaseStream.Position = 0L;
                    WriterCASFileHeader_ToMemory.Write((uint)headersize, Endian.Big); // Header Size at the top
                    //if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + WriterCASFileHeader_ToMemory.BaseStream.Length > 1073741824)
                    //{
                    //    writer_new_cas_file?.Close();
                    //    writer_new_cas_file = GetNextCas(out casFileIndex);
                    //}
                    // TODO: Get this done so we can tell the toc/sb where stuff is
                    //bundle_file_list.Add(new BundleFileEntry(casFileIndex, (int)writer_new_cas_file.BaseStream.Position, (int)(headersize + 4)));

                    WriterCASFileHeader_ToMemory.Flush();
                    WriterCASFileBody_ToMemory.Flush();
                    var headerBytes = ((MemoryStream)WriterCASFileHeader_ToMemory.BaseStream).ToArray();
                    var casBodyBytes = ((MemoryStream)WriterCASFileBody_ToMemory.BaseStream).ToArray();
                    // Dispose of this now. Free memory
                    WriterCASFileBody_ToMemory.Close();
                    WriterCASFileBody_ToMemory.Dispose();

                    writer_new_cas_file.Write(headerBytes);
                    writer_new_cas_file.Write(casBodyBytes);

                   


                    writer_new_cas_file?.Close();

                    // return list of new CAS files
                    //bundle_file_list = bundle_file_list.Where(x => x.CasIndex > CasFileCount-1).ToList();
                }
            }
        }

        private bool WriteNewTocFile(
            string location_toc_file_mod_data
            , byte[] tocSbHeaderBytes
            , byte[] original_toc_file_byte_array
            )
        {
            // Do a copy of the original toc sb
            using (NativeWriter writer_new_toc_file_mod_data = new NativeWriter(new FileStream(location_toc_file_mod_data, FileMode.Create)))
            {
                writer_new_toc_file_mod_data.Write(tocSbHeaderBytes);
                writer_new_toc_file_mod_data.Write(original_toc_file_byte_array);
            }

            // -----------------------------------------------------------------------
            // Find out what has been modified

            GetModifiedBundles(original_toc_file_byte_array
                , out List<BundleFileEntry> list_of_bundle_entries
                , out Dictionary<ModBundleInfo, List<BundleFileEntry>> list_of_modified_bundles
                , out List<int> IsEBXList);


            // Create the new CAS Files
            if(list_of_modified_bundles.Count > 0)
            {
                var new_list_of_modified_bundles = new Dictionary<ModBundleInfo, List<BundleFileEntry>>();
                foreach (var kvp in list_of_modified_bundles) { new_list_of_modified_bundles.Add(kvp.Key, new List<BundleFileEntry>(kvp.Value)); }
                WriteBundleToCASFile(ref new_list_of_modified_bundles);

                var dictParentBundleToChild = new Dictionary<string, List<BundleFileEntry>>();
                foreach(var kvp in new_list_of_modified_bundles.OrderBy(x=>x.Value.OrderBy(y=>y.Name)))
                {
                    var lastName = "";
                    foreach(var i in kvp.Value)
                    {
                        if (!string.IsNullOrEmpty(i.Name) && i.Name != lastName)
                        {
                            lastName = i.Name;
                        }

                        if(!dictParentBundleToChild.ContainsKey(lastName))
                            dictParentBundleToChild.Add(lastName, kvp.Value);

                    }
                }

                if (dictParentBundleToChild.Values.Count() > 0)
                {
                    using (NativeWriter writer_new_toc_file_mod_data = new NativeWriter(new FileStream(location_toc_file_mod_data, FileMode.Create)))
                    {
                        writer_new_toc_file_mod_data.Write(tocSbHeaderBytes);
                        writer_new_toc_file_mod_data.Write(3280507699u);
                        writer_new_toc_file_mod_data.Write(0xFFFFFFFF); // Position of reader - 556 + 4
                        writer_new_toc_file_mod_data.Write(0xFFFFFFFF); // Position of reader - 556 + 8
                        //writer_new_toc_file_mod_data.Write(0xFFFFFFFF); // Position of reader - 556 + 12
                        //writer_new_toc_file_mod_data.Write(0xFFFFFFFF); // Position of reader - 556 + 16

                        var bundlePositions = new List<long>();
                        foreach (var kvp in dictParentBundleToChild)
                        {
                            var bundleEntries = kvp.Value.OrderByDescending(x=>x.CasIndex).ToList();

                            long positionofstring = 0;
                            var modulebundleinfo = kvp.Key;
                            var positionofstring_position = writer_new_toc_file_mod_data.BaseStream.Position;
                            var starterOfBundlePosition = writer_new_toc_file_mod_data.BaseStream.Position;
                            
                            bundlePositions.Add(starterOfBundlePosition);
                            writer_new_toc_file_mod_data.Write(0xFFFFFFFF);
                            writer_new_toc_file_mod_data.Flush();
                            for (var indexBundle = 0; indexBundle < bundleEntries.Count; indexBundle++)
                            {
                                var bundle = bundleEntries[indexBundle];
                                uint newCasIndex = (uint)bundle.CasIndex;
                                if (indexBundle != bundleEntries.Count - 1)
                                {
                                    newCasIndex = (uint)((int)newCasIndex | int.MinValue);
                                }
                                writer_new_toc_file_mod_data.Write(newCasIndex);
                                var theReverseOfCasIndex = newCasIndex & 2147483648u;

                                writer_new_toc_file_mod_data.Write(bundle.Offset);
                                writer_new_toc_file_mod_data.Write(bundle.Size);
                                writer_new_toc_file_mod_data.Flush();
                                if(indexBundle == bundleEntries.Count-1)
                                {
                                    positionofstring = writer_new_toc_file_mod_data.BaseStream.Position;
                                    writer_new_toc_file_mod_data.WriteNullTerminatedString(Utils.ReverseString(kvp.Key));
                                    writer_new_toc_file_mod_data.Write(0);
                                    writer_new_toc_file_mod_data.Flush();
                                }
                            }

                            writer_new_toc_file_mod_data.BaseStream.Position = positionofstring_position;
                            writer_new_toc_file_mod_data.Write((int)((positionofstring + 1) - 556));

                        }

                        writer_new_toc_file_mod_data.Flush();
                        writer_new_toc_file_mod_data.BaseStream.Position = writer_new_toc_file_mod_data.BaseStream.Length;
                        var positionofadditionalukndata = writer_new_toc_file_mod_data.BaseStream.Position;

                        writer_new_toc_file_mod_data.Write(dictParentBundleToChild.Count);
                        writer_new_toc_file_mod_data.Flush();

                        foreach (var kvp in dictParentBundleToChild)
                        {
                            writer_new_toc_file_mod_data.Write(dictParentBundleToChild.Values.FirstOrDefault()[0].CasIndex * -1);
                        }
                        writer_new_toc_file_mod_data.Flush();

                        foreach (var pos in bundlePositions)
                        {
                            var rPos = (int)(pos - 556);
                            writer_new_toc_file_mod_data.Write(rPos);
                        }
                        writer_new_toc_file_mod_data.Flush();

                        writer_new_toc_file_mod_data.BaseStream.Position = 560;
                        writer_new_toc_file_mod_data.Write((int)(positionofadditionalukndata - 556));
                        writer_new_toc_file_mod_data.Flush();


                    }
                }
                
            }
            return list_of_modified_bundles.Count > 0;
        }

        private bool CheckTocCasReadCorrectly(string tocPath)
        {
            TocCasReader_M21 tocCasReader_M21 = new TocCasReader_M21();
            tocCasReader_M21.Read(tocPath, 0, new BinarySbDataHelper(AssetManager.Instance));
            return true;
        }

        public void Run()
        {
            try
            {
                parent.Logger.Log("Loading files to know what to change.");

                FIFA21AssetLoader assetLoader = new FIFA21AssetLoader();
                assetLoader.Load(AssetManager.Instance, new BinarySbDataHelper(AssetManager.Instance));

                parent.Logger.Log("Finished loading files. Enumerating modified bundles.");

                foreach (var modEBX in parent.modifiedEbx)
                {
                    var inEbx = AssetManager.Instance.GetEbx(modEBX.Value);

                    var originalEntry = AssetManager.Instance.GetEbxEntry(modEBX.Value.Name);
                    if(originalEntry != null)
                    {
                        var ebxData = parent.archiveData[modEBX.Value.Sha1].Data;
                        var casPath = originalEntry.ExtraData.CasPath.Replace("native_data"
                            , AssetManager.Instance.fs.BasePath + "ModData\\Data");
                        casPath = casPath.Replace("native_patch"
                            , AssetManager.Instance.fs.BasePath + "ModData\\Patch");

                        byte[] originalCASArray = null;
                        using (NativeReader readerOfCas = new NativeReader(new FileStream(casPath, FileMode.Open)))
                        {
                            originalCASArray = readerOfCas.ReadToEnd();
                        }

                        var positionOfNewData = 0;
                        using (NativeWriter nativeWriter = new NativeWriter(new FileStream(casPath, FileMode.Open)))
                        {
                            nativeWriter.Write(originalCASArray);
                            // write the new data to end of the file (this should be fine)
                            positionOfNewData = (int)nativeWriter.BaseStream.Position;
                            nativeWriter.Write(ebxData);
                        }

                        foreach (var @object in assetLoader.AllDbObjects)
                        {
                            foreach (DbObject ebxObject in @object.GetValue<DbObject>("ebx"))
                            {
                                if (ebxObject.GetValue<string>("name") == modEBX.Value.Name)
                                {
                                    parent.Logger.Log("Writing new asset entry for EBX (" + modEBX + ")");

                                    var sb_cas_offset_position = ebxObject.GetValue<int>("SB_CAS_Offset_Position");
                                    var sb_sha1_position = ebxObject.GetValue<int>("SB_Sha1_Position");
                                    var sb_original_size_position = ebxObject.GetValue<int>("SB_OriginalSize_Position");

                                    var sbpath = ebxObject.GetValue<string>("SBFileLocation");
                                    sbpath = sbpath.Replace("\\patch", "\\ModData\\Patch");
                                    byte[] arrayOfSB = null;
                                    using (NativeReader nativeReader = new NativeReader(new FileStream(sbpath, FileMode.Open)))
                                    {
                                        arrayOfSB = nativeReader.ReadToEnd();
                                    }
                                    File.Delete(sbpath);
                                    using (NativeWriter nativeWriter = new NativeWriter(new FileStream(sbpath, FileMode.OpenOrCreate)))
                                    {
                                        //nativeWriter.Write(arrayOfSB);
                                        //nativeWriter.BaseStream.Position = sb_cas_offset_position;
                                        //nativeWriter.Write((uint)positionOfNewData, Endian.Big);
                                        //nativeWriter.Flush();

                                        //nativeWriter.Write((uint)ebxData.Length, Endian.Big);
                                        //nativeWriter.Flush();

                                        //nativeWriter.BaseStream.Position = sb_sha1_position;
                                        //nativeWriter.Write(modEBX.Value.Sha1);
                                        //nativeWriter.Flush();

                                        //nativeWriter.BaseStream.Position = sb_original_size_position;
                                        //nativeWriter.Write(Convert.ToUInt32(modEBX.Value.OriginalSize), Endian.Little);
                                        //nativeWriter.Flush();


                                    }

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private NativeWriter GetNextCas(out int casFileIndex)
        {
            int num = 1;
            string text = parent.fs.BasePath + "ModData\\patch\\" + catalogInfo.Name + "\\cas_" + num.ToString("D2") + ".cas";
            while (File.Exists(text))
            {
                num++;
                text = parent.fs.BasePath + "ModData\\patch\\" + catalogInfo.Name + "\\cas_" + num.ToString("D2") + ".cas";
            }
            lock (locker)
            {
                casFiles.Add(++CasFileCount, "/native_data/Patch/" + catalogInfo.Name + "/cas_" + num.ToString("D2") + ".cas");
                AssetManager.Instance.ModCASFiles.Add(CasFileCount, "/native_data/Patch/" + catalogInfo.Name + "/cas_" + num.ToString("D2") + ".cas");
                casFileIndex = CasFileCount;
            }
            FileInfo fileInfo = new FileInfo(text);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }
            return new NativeWriter(new FileStream(text, FileMode.Create));
        }

        private uint HashString(string strToHash, uint initial = 0u)
        {
            uint num = 2166136261u;
            if (initial != 0)
            {
                num = initial;
            }
            for (int i = 0; i < strToHash.Length; i++)
            {
                num = (strToHash[i] ^ (16777619 * num));
            }
            return num;
        }

        private static uint HashData(byte[] b, uint initial = 0u)
        {
            uint num = (uint)((sbyte)b[0] ^ 0x50C5D1F);
            int num2 = 1;
            if (initial != 0)
            {
                num = initial;
                num2 = 0;
            }
            for (int i = num2; i < b.Length; i++)
            {
                num = (uint)((int)(sbyte)b[i] ^ (int)(16777619 * num));
            }
            return num;
        }

        public void ThreadPoolCallback(object threadContext)
        {
            Run();
            if (Interlocked.Decrement(ref parent.numTasks) == 0)
            {
                doneEvent.Set();
            }
        }
    }

}
