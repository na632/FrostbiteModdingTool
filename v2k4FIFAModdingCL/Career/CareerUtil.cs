using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using v2k4FIFAModdingCL;
using v2k4FIFAModdingCL.CGFE;

namespace v2k4FIFAModding.Career
{
    public class CareerUtil
    {
        public static Dictionary<string, string> GetCareerSaves(string directory)
        {
            var myDocs = directory;

            var r = Directory.GetFiles(myDocs, "Career*", SearchOption.AllDirectories);

            /// SWITCH THIS FOR 
            /*
             * 
             * DbReader dbReader = new DbReader(fileStream, FifaPlatform.PC);
            dbReader.BaseStream.Position = 18L;
            m_InGameName = FifaUtil.ReadNullTerminatedString(dbReader);
             */
            Dictionary<string, string> results = new Dictionary<string, string>();
            foreach (var i in r)
            {
                byte[] test = new byte[30];
                using (var fileStream = new FileStream(i, FileMode.Open))
                using (v2k4FIFAModdingCL.CGFE.DbReader dbReader = new v2k4FIFAModdingCL.CGFE.DbReader(fileStream, FifaPlatform.PC))
                {
                    dbReader.BaseStream.Position = 18L;
                    results.Add(i, FifaUtil.ReadNullTerminatedString(dbReader));
                }
            }

            return results;
        }
    }
}
