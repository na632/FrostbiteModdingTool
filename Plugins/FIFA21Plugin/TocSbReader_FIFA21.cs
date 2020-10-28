using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static FrostySdk.Managers.AssetManager;

namespace FIFA21Plugin
{
    public class TocSbReader_FIFA21
    {
        private const uint ReadableSectionMagic = 3599661469;

        public AssetManager AssetManager { get; set; }

        public TOCFile TOCFile { get; set; }
        public SBFile SBFile { get; set; }

        public int SBIndex { get; set; }
        
        public List<DbObject> Read(string tocPath, int sbIndex, BinarySbDataHelper helper, string SBName, bool native_data = false, string nativePath = null)
        {
            SBIndex = sbIndex;

            if (AssetManager == null)
                AssetManager = AssetManager.Instance;

            byte[] key = KeyManager.Instance.GetKey("Key2");
            if (tocPath != "")
            {
                var sbPath = tocPath.Replace(".toc", ".sb");

                Debug.WriteLine($"[DEBUG] Loading TOC File: {tocPath}");
                //if (!tocPath.Contains("globals.toc"))
                //{
                List<DbObject> objs = new List<DbObject>();
                using (NativeReader nativeReader = new NativeReader(new FileStream(tocPath, FileMode.Open, FileAccess.Read), AssetManager.fs.CreateDeobfuscator()))
                {
                    
                    // TOC File 
                    TOCFile = new TOCFile(this);
                    TOCFile.FileLocation = tocPath;
                    TOCFile.SuperBundleName = tocPath;
                    TOCFile.Read(nativeReader);

                    // SB File
                    var rObjs = ReadSB(sbPath, helper, nativePath.Replace(".toc", ".sb"));
                    if (rObjs != null)
                        objs.AddRange(rObjs);

                    // Attempt to read from CAS
                    if (File.ReadAllBytes(sbPath).Length <= 64)
                    {
                        foreach (var b in TOCFile.Bundles) 
                        {
                            
                        }


                        //MemoryStream memoryStream = new MemoryStream();

                        //using (NativeReader nr_cas_01 = new NativeReader(
                        //    new FileStream(AssetManager.fs.BasePath + @"\Data\Win32\superbundlelayout\fifa_installpackage_00\cas_01.cas", FileMode.Open, FileAccess.Read)
                        //    )
                        //    )
                        //{
                        //    List<long> PositionOfReadableItems = new List<long>();
                        //    while (nr_cas_01.Position < nr_cas_01.Length) 
                        //    {
                        //        if (nr_cas_01.ReadUInt(Endian.Big) == ReadableSectionMagic)
                        //        {
                        //            PositionOfReadableItems.Add(nr_cas_01.Position - 8);
                        //        }
                        //    }

                        //    nr_cas_01.Position = 0;
                        //    using (
                        //        NativeReader nr_splash = new NativeReader(
                        //        nr_cas_01.CreateViewStream(69045582, nr_cas_01.Length - 69045582)
                        //        ))
                        //    {
                        //        SBFile sbFile = new SBFile(this, TOCFile, SBIndex);
                        //        DbObject obj = new DbObject();
                        //        sbFile.BinaryRead_FIFA21(new FIFA21AssetLoader.BaseBundleInfo()
                        //            , ref obj, nr_splash, false);
                        //        foreach (DbObject ebx in obj.GetValue<DbObject>("ebx"))
                        //        {
                        //        }
                        //        objs.Add(obj);
                        //    }
                        //}

                        //var casFile1 = AssetManager.fs.BasePath + @"\Data\Win32\superbundlelayout\fifa_installpackage_00\cas_01.cas";
                        //using (NativeReader nativeReader3 = new NativeReader(
                        //    new FileStream(casFile1, FileMode.Open, FileAccess.Read)
                        //    )
                        //    )
                        //{

                        //    SBFile sbFile = new SBFile(this, TOCFile, SBIndex);
                        //    DbObject obj = new DbObject();
                        //    sbFile.BinaryRead_FIFA21(new FIFA21AssetLoader.BaseBundleInfo(), ref obj, nativeReader3);
                        //    objs.Add(obj);
                        //}

                        //var casFile2 = AssetManager.fs.BasePath + @"\Data\Win32\superbundlelayout\fifa_installpackage_03\cas_01.cas";
                        //using (NativeReader nativeReader3 = new NativeReader(
                        //    new FileStream(casFile2, FileMode.Open, FileAccess.Read)
                        //    )
                        //    )
                        //{

                        //    SBFile sbFile = new SBFile(this, TOCFile, SBIndex);
                        //    DbObject obj = new DbObject();
                        //    sbFile.BinaryRead_FIFA21(new FIFA21AssetLoader.BaseBundleInfo(), ref obj, nativeReader3);
                        //    objs.Add(obj);
                        //}

                        //var casFile8 = AssetManager.fs.BasePath + @"\Data\Win32\superbundlelayout\fifa_installpackage_08\cas_01.cas";
                        //using (NativeReader nativeReader3 = new NativeReader(
                        //    new FileStream(casFile8, FileMode.Open, FileAccess.Read)
                        //    )
                        //    )
                        //{

                        //    SBFile sbFile = new SBFile(this, TOCFile, SBIndex);
                        //    DbObject obj = new DbObject();
                        //    sbFile.BinaryRead_FIFA21(new FIFA21AssetLoader.BaseBundleInfo(), ref obj, nativeReader3);
                        //    objs.Add(obj);

                        //    SBFile sbFile2 = new SBFile(this, TOCFile, SBIndex);
                        //    var obj2 = new DbObject();

                        //    sbFile.BinaryRead_FIFA21(new FIFA21AssetLoader.BaseBundleInfo(), ref obj2
                        //        , new NativeReader(nativeReader3.CreateViewStream(770646876, nativeReader3.Length - 770646876)));
                        //    objs.Add(obj2);
                        //}

                    }
                    return objs;
                    //}
                }
            }




            return null;
        }

        

        public List<DbObject> ReadSB(string sbPath, BinarySbDataHelper helper, string nativeSBPath = null)
        {
            Debug.WriteLine($"[DEBUG] Loading SB File: {sbPath}");

            using (NativeReader nativeReader = new NativeReader(new FileStream(sbPath, FileMode.Open, FileAccess.Read)))
            {
                if (nativeReader.Length > 0)
                {
                    SBFile = new SBFile(this, TOCFile, SBIndex);
                    SBFile.NativeFileLocation = nativeSBPath;
                    SBFile.FileLocation = sbPath;
                    return SBFile.Read(nativeReader);
                }
            }
            return null;
        }
    }
}
