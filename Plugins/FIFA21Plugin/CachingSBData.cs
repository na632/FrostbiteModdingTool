using FrostySdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static FIFA21Plugin.SBFile;

namespace FIFA21Plugin
{
    public static class CachingSB
    {
        public static List<CachingSBData> CachingSBs = new List<CachingSBData>();

        public static void Save()
        {
            var json = JsonConvert.SerializeObject(CachingSBs);
            if (File.Exists(ProfilesLibrary.ProfileName + ".CachingSBData.cache"))
                File.Delete(ProfilesLibrary.ProfileName + ".CachingSBData.cache");

            File.WriteAllText(ProfilesLibrary.ProfileName + ".CachingSBData.cache", json);
        }

        public static List<CachingSBData> Load()
        {
            if (File.Exists(ProfilesLibrary.ProfileName + ".CachingSBData.cache"))
            {
                CachingSBs = JsonConvert.DeserializeObject<List<CachingSBData>>(File.ReadAllText(ProfilesLibrary.ProfileName + ".CachingSBData.cache"));

                return CachingSBs;
            }

            return CachingSBs;
        }
    }
    public class CachingSBData
    {
        public string SBFile { get; set; }
        public class Bundle
        {
            public BundleHeader BundleHeader { get; set; }

            public long StartOffset { get; set; }

            public long BooleanOfCasGroupOffset { get; set; }
            public long BooleanOfCasGroupOffsetEnd { get; set; }

            public byte[] BooleanOfCasGroupData { get; set; }

            public long CatalogCasGroupOffset { get; set; }

            public byte[] CatalogCasGroupData { get; set; }


            public long BinaryDataOffset { get; set; }

            public byte[] BinaryDataData { get; set; }

            public int LastCatalogId { get; set; }
            public int LastCAS { get; set; }
            public long BinaryDataOffsetEnd { get; internal set; }
            public long CatalogCasGroupOffsetEnd { get; internal set; }
        }

        public List<Bundle> Bundles = new List<Bundle>();




        public override string ToString()
        {
            if(SBFile != null)
            {
                return SBFile + $" - Count: {Bundles.Count}";
            }
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


    }

}
