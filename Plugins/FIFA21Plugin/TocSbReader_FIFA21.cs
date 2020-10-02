using FrostySdk.IO;
using FrostySdk.Managers;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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
        
        public void Read(string tocPath, int sbIndex, BinarySbDataHelper helper)
        {
            if (AssetManager == null)
                AssetManager = AssetManager.Instance;

            byte[] key = KeyManager.Instance.GetKey("Key2");
            if (tocPath != "")
            {
                var sbPath = tocPath.Replace(".toc", ".sb");

                List<BaseBundleInfo> listOfBundles = new List<BaseBundleInfo>();
                using (NativeReader nativeReader = new NativeReader(new FileStream(tocPath, FileMode.Open, FileAccess.Read), AssetManager.fs.CreateDeobfuscator()))
                {
                    TOCFile = new TOCFile();
                    TOCFile.Read(nativeReader);

                    ReadSB(sbPath, helper);
                }
            }
        }

        

        public void ReadSB(string sbPath, BinarySbDataHelper helper, int offset = 0)
        {
            using (NativeReader nativeReader = new NativeReader(new FileStream(sbPath, FileMode.Open, FileAccess.Read), AssetManager.fs.CreateDeobfuscator()))
            {
                nativeReader.Position = offset;
                SBFile = new SBFile();

                // Initial Header data
                for (var i = 0; i < 8; i++)
                {
                    SBFile.ArrayOfInitialHeaderData[i] = nativeReader.ReadInt(Endian.Big);
                }

                // Header Information (Counts offsets etc)
                SBFile.SBHeaderInformation = new SBHeaderInformation(nativeReader);



            }
        }
    }
}
