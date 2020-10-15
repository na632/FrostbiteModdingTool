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
        public AssetManager AssetManager { get; set; }

        public TOCFile TOCFile { get; set; }
        public SBFile SBFile { get; set; }

        public int SBIndex { get; set; }
        
        public List<DbObject> Read(string tocPath, int sbIndex, BinarySbDataHelper helper, string SBName, bool native_data = false)
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
                    var rObjs = ReadSB(sbPath, helper);
                    if (rObjs != null)
                        objs.AddRange(rObjs);

                    // Attempt to read from CAS
                    if (File.ReadAllBytes(sbPath).Length <= 64)
                    {
                        foreach (var b in TOCFile.Bundles) 
                        {
                            MemoryStream memoryStream = new MemoryStream();

                            using (NativeReader nativeReader3 = new NativeReader(
                                //new FileStream(AssetManager.fs.ResolvePath(AssetManager.fs.GetFilePath(b.CasIndex)), FileMode.Open, FileAccess.Read))
                                new FileStream(@"E:\Origin Games\FIFA 21\Data\Win32\superbundlelayout\fifa_installpackage_00\cas_01.cas", FileMode.Open, FileAccess.Read)
                                )
                                )
                            {
                                //nativeReader3.Position = b.Offset;
                            //    nativeReader3.Position = 0;
                            //    //memoryStream.Write(nativeReader3.ReadBytes((int)b.Size), 0, (int)b.Size);
                            //    memoryStream.Write(nativeReader3.ReadBytes((int)b.Size), 0, (int)b.Size);
                            //}
                            //if (memoryStream.Length > 0)
                            //{
                                SBFile sbFile = new SBFile(this, TOCFile, SBIndex);
                                DbObject obj = new DbObject();
                                //using (NativeReader nr_mem = new NativeReader(memoryStream))
                                //{
                                    sbFile.BinaryRead_FIFA21(b, ref obj, nativeReader3);
                                //}
                            }
                        }


                    }
                    return objs;
                    //}
                }
            }
            return null;
        }

        

        public List<DbObject> ReadSB(string sbPath, BinarySbDataHelper helper)
        {
            Debug.WriteLine($"[DEBUG] Loading SB File: {sbPath}");

            using (NativeReader nativeReader = new NativeReader(new FileStream(sbPath, FileMode.Open, FileAccess.Read)))
            {
                if (nativeReader.Length > 0)
                {
                    SBFile = new SBFile(this, TOCFile, SBIndex);
                    SBFile.FileLocation = sbPath;
                    return SBFile.Read(nativeReader);
                }
            }
            return null;
        }
    }
}
