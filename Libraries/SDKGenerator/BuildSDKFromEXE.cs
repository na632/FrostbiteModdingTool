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

            var pattern = new byte[] { 0x48, 0x89, 0x41, 0x08 };
            //BoyerMoore moore = new BoyerMoore(exeData);
            //var list1 = moore.SearchAll(pattern, 0);
            var list2 = IndexesOf(exeData, pattern);


            return true;
        }

        public static IEnumerable<int> IndexesOf(byte[] haystack, byte[] needle,
    int startIndex = 0, bool includeOverlapping = false)
        {
            int matchIndex = haystack.AsSpan(startIndex).IndexOf(needle);
            while (matchIndex >= 0)
            {
                yield return startIndex + matchIndex;
                startIndex += matchIndex + (includeOverlapping ? 1 : needle.Length);
                matchIndex = haystack.AsSpan(startIndex).IndexOf(needle);
            }
        }
    }
}
