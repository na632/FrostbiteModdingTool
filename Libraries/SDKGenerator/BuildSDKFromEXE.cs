using FrostbiteSdk;
using FrostySdk;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdkGenerator
{
    public class BuildSDKFromEXE
    {
        public bool Build(string exeLocation)
        {
            byte[] exeData = File.ReadAllBytes(exeLocation);
            if (exeData == null || exeData.Length == 0)
                return false;

            var pattern = new byte[] { 0x48, 0x89 };
            //BoyerMoore moore = new BoyerMoore(exeData);
            //var list1 = moore.SearchAll(pattern, 0);
            //var list2 = IndexesOf(exeData, pattern);
            //var firstOff = list2.FirstOrDefault();

            // 128400896 // there is 48 89s above!!

            // 129677992 // AttribSchema string
            using (var nr = new NativeReader(new MemoryStream(exeData)))
            {
                var offsetList1 = nr.ScanAOB("48 39 ?? ?? ?? ?? ?? ?? ?? 48");
                //var offsetList2 = nr.ScanAOB2(pattern);
                //var offsetList = nr.ScanAOB("48 89");
                var firstOff = offsetList1.FirstOrDefault();
                nr.Position = firstOff + 3;
                int num = nr.ReadInt();
                nr.Position = firstOff + 3 + num + 4;
                //var ty = nr.ReadLong();
            }
            return true;
        }

        
    }
}
