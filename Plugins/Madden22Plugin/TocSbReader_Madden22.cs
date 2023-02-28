using FMT.FileTools;
using FrostySdk;
using FrostySdk.Managers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Madden22Plugin
{
    public class TocSbReader_Madden22
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
                    TOCFile.DoLogging = DoLogging;
                    TOCFile.ProcessData = ProcessData;
                    TOCFile.Read(nativeReader);

                }
            }

            return null;
        }

    }
}
