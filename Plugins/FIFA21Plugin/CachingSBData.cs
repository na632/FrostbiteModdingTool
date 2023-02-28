using FrostySdk;
using FrostySdk.Frostbite.PluginInterfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static FIFA21Plugin.SBFile;

namespace FIFA21Plugin
{
    public static class CachingSB
    {
        public static List<CachingSBData> CachingSBs = new List<CachingSBData>();

        public static void Save()
        {
            var json = JsonConvert.SerializeObject(CachingSBs);
            if (File.Exists(ProfileManager.ProfileName + ".CachingSBData.cache"))
                File.Delete(ProfileManager.ProfileName + ".CachingSBData.cache");

            File.WriteAllText(ProfileManager.ProfileName + ".CachingSBData.cache", json);
        }

        public static List<CachingSBData> Load()
        {
            if (File.Exists(ProfileManager.ProfileName + ".CachingSBData.cache"))
            {
                CachingSBs = JsonConvert.DeserializeObject<List<CachingSBData>>(File.ReadAllText(ProfileManager.ProfileName + ".CachingSBData.cache"));

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
            public long BinaryDataOffsetEnd { get; set; }
            public long CatalogCasGroupOffsetEnd { get; set; }
            public BaseBundleInfo BaseBundleItem { get; internal set; }

            private Dictionary<string, List<string>> listOfItems;

            public Dictionary<string, List<string>> ListOfItems
            {
                get
                {
                    if (listOfItems == null)
                    {
                        listOfItems = new Dictionary<string, List<string>>();
                        listOfItems.Add("ebx", new List<string>());
                        listOfItems.Add("res", new List<string>());
                        listOfItems.Add("chunk", new List<string>());
                    }

                    return listOfItems;
                }
                set
                {
                    listOfItems = value;
                }
            }

        }



        public List<Bundle> Bundles = new List<Bundle>();

        public Bundle GetLastBundle()
        {
            if (Bundles.Count > 0)
            {
                return Bundles.OrderByDescending(x => x.StartOffset).FirstOrDefault();
            }
            return null;
        }




        public override string ToString()
        {
            if (SBFile != null)
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
