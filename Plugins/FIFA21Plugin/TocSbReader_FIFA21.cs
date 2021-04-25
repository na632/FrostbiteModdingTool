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
                    TOCFile.SuperBundleName = SBName;
                    TOCFile.NativeFileLocation = nativePath;
                    TOCFile.FileLocation = tocPath;
                    TOCFile.SuperBundleName = Guid.NewGuid().ToString();
                    TOCFile.Read(nativeReader);

                    // SB File
                    var rObjs = ReadSB(sbPath, helper, nativePath.Replace(".toc", ".sb"));
                    if (rObjs != null)
                        objs.AddRange(rObjs);

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
