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

namespace BF2042Plugin
{
    public class TocSbReader_BF2042
    {
        private const uint ReadableSectionMagic = 3599661469;

        public AssetManager AssetManager { get; set; }

        public TOCFile TOCFile { get; set; }
        //public SBFile SBFile { get; set; }

        public int SBIndex { get; set; }


        /// <summary>
        /// Does the logging to Windows or Debuggers
        /// </summary>
        public bool DoLogging
        {
            get; set;
        } = true;


        /// <summary>
        /// Processes the Internal Data into the AssetManager EBX,RES,Chunk Lists
        /// </summary>
        public bool ProcessData = true;

        public string SbPath = string.Empty;

        public List<DbObject> Read(string tocPath, int sbIndex, BinarySbDataHelper helper, string SBName, bool native_data = false, string nativePath = null)
        {
            SBIndex = sbIndex;

            if (AssetManager == null)
                AssetManager = AssetManager.Instance;

            //byte[] key = KeyManager.Instance.GetKey("Key2");
            if (tocPath != "")
            {
                SbPath = tocPath.Replace(".toc", ".sb");

                Debug.WriteLine($"[DEBUG] Loading TOC File: {tocPath}");
                //if (!tocPath.Contains("globals.toc"))
                //{
                List<DbObject> objs = new List<DbObject>();
                using (NativeReader nativeReader = new NativeReader(new FileStream(tocPath, FileMode.Open, FileAccess.Read)))
                {
                    
                    // TOC File 
                    TOCFile = new TOCFile(this);
                    TOCFile.SuperBundleName = SBName;
                    TOCFile.NativeFileLocation = nativePath;
                    TOCFile.FileLocation = tocPath;
                    //TOCFile.SuperBundleName = Guid.NewGuid().ToString();
                    TOCFile.DoLogging = DoLogging;
                    TOCFile.ProcessData = ProcessData;
                    TOCFile.Read(nativeReader);

                    // SB File
                    //var rObjs = ReadSB(SbPath, helper, nativePath != null ? nativePath.Replace(".toc", ".sb") : null);
                    //if (rObjs != null)
                    //    objs.AddRange(rObjs);

                    //return objs;
                    //}
                }
            }




            return null;
        }

        

        //public List<DbObject> ReadSB(string sbPath, BinarySbDataHelper helper, string nativeSBPath = null)
        //{
        //    //Debug.WriteLine($"[DEBUG] Loading SB File: {sbPath}");
        //    //if(sbPath.Contains("contentlaunchsb", StringComparison.OrdinalIgnoreCase))
        //    //{

        //    //}

        //    using (NativeReader nativeReader = new NativeReader(new FileStream(sbPath, FileMode.Open, FileAccess.Read)))
        //    {
        //        if (nativeReader.Length > 1)
        //        {
        //            SBFile = new SBFile(this, TOCFile, SBIndex);
        //            SBFile.NativeFileLocation = nativeSBPath;
        //            SBFile.FileLocation = sbPath;
        //            SBFile.DoLogging = DoLogging;
        //            //SBFile.ProcessData = ProcessData;
        //            return SBFile.Read(nativeReader);
        //        }
        //    }
        //    return null;
        //}
    }
}
