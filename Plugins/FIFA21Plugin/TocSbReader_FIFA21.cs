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
        
        public List<DbObject> Read(string tocPath, int sbIndex, BinarySbDataHelper helper, string SBName)
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
                    using (NativeReader nativeReader = new NativeReader(new FileStream(tocPath, FileMode.Open, FileAccess.Read), AssetManager.fs.CreateDeobfuscator()))
                    {
                        TOCFile = new TOCFile(this);
                        TOCFile.SuperBundleName = tocPath;
                        TOCFile.Read(nativeReader);
                        return ReadSB(sbPath, helper);
                    }
                //}
            }
            return null;
        }

        

        public List<DbObject> ReadSB(string sbPath, BinarySbDataHelper helper)
        {
            Debug.WriteLine($"[DEBUG] Loading SB File: {sbPath}");

            using (NativeReader nativeReader = new NativeReader(new FileStream(sbPath, FileMode.Open, FileAccess.Read))) //, AssetManager.fs.CreateDeobfuscator()))
            {
                if (nativeReader.Length > 0)
                {
                    SBFile = new SBFile(this, TOCFile, SBIndex);
                    return SBFile.Read(nativeReader);
                }
            }
            return null;
        }
    }
}
