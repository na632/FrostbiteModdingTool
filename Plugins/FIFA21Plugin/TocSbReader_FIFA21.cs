using FrostySdk;
using FrostySdk.Frostbite.PluginInterfaces;
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
    public class TocSbReader_FIFA21 : IDisposable
    {
        private const uint ReadableSectionMagic = 3599661469;

        public AssetManager AssetManager { get; set; }

        public TOCFile TOCFile { get; set; }
        public SBFile SBFile { get; set; }

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


        public TocSbReader_FIFA21()
        {

        }

        public TocSbReader_FIFA21(in bool processData, in bool doLogging)
        {
            ProcessData = processData;
            DoLogging = doLogging;
        }

        public void Dispose()
        {
            //if (TOCFile != null)
            //    TOCFile.Dispose();

            TOCFile = null;
        }

        public List<DbObject> Read(string tocPath, int sbIndex, string SBName, bool native_data = false, string nativePath = null)
        {
            SBIndex = sbIndex;

            if (AssetManager == null)
                AssetManager = AssetManager.Instance;

            if (tocPath != "")
            {
                SbPath = tocPath.Replace(".toc", ".sb");

                Debug.WriteLine($"[DEBUG] Loading TOC File: {tocPath}");
                List<DbObject> objs = new List<DbObject>();

                // TOC File 
                TOCFile = new TOCFile(nativePath.Replace(".sb", ".toc"), DoLogging, ProcessData, false);
                
                if (TOCFile.TOCObjects != null && TOCFile.TOCObjects.Count > 0 && !ProcessData)
                    objs.AddRange(TOCFile.TOCObjects.List.Select(x => ((DbObject)x)));

                // SB File
                var sbObjs = ReadSB(SbPath, nativePath != null ? nativePath.Replace(".toc", ".sb") : null);
                if (sbObjs != null)
                {
                    //objs.Clear();
                    objs.AddRange(sbObjs);
                }

#if DEBUG
                var firstEntry = AssetManager.Instance.EBX.First();
                //AssetManager.Instance.AddEbx();
                var e = AssetManager.Instance.GetEbx(firstEntry.Value);
#endif

                return objs;
            }




            return null;
        }

        

        public List<DbObject> ReadSB(string sbPath, string nativeSBPath = null)
        {
            //Debug.WriteLine($"[DEBUG] Loading SB File: {sbPath}");
            //if(sbPath.Contains("contentlaunchsb", StringComparison.OrdinalIgnoreCase))
            //{

            //}

            using (NativeReader nativeReader = new NativeReader(new FileStream(sbPath, FileMode.Open, FileAccess.Read)))
            {
                if (nativeReader.Length > 1)
                {
                    SBFile = new SBFile(this, TOCFile, SBIndex);
                    SBFile.NativeFileLocation = nativeSBPath;
                    SBFile.FileLocation = sbPath;
                    SBFile.DoLogging = DoLogging;
                    //SBFile.ProcessData = ProcessData;
                    return SBFile.Read(nativeReader);
                }
            }
            return null;
        }
    }
}
