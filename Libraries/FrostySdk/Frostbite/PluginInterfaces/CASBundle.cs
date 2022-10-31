using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.Frostbite.PluginInterfaces
{
    public class CASBundle
    {
        public int unk1 { get; set; }
        public int unk2 { get; set; }
        public int unk3 { get; set; }
        public int unk4 { get; set; }
        public int unk5 { get; set; }

        // ---------------------------------------
        // Cas Identifiers

        public byte Unk { get; set; }
        public byte Catalog { get; set; }
        public byte Cas { get; set; }
        public bool Patch { get; internal set; }

        //
        // ---------------------------------------


        public int FlagsOffset { get; set; }

        public uint BundleOffset { get; set; }
        public uint BundleSize { get; internal set; }

        public List<uint> Sizes = new List<uint>();

        public List<uint> Offsets = new List<uint>();

        public List<CASBundleEntry> Entries = new List<CASBundleEntry>();

        public long TotalSize
        {
            get
            {
                if (Sizes.Count == 0)
                {
                    return 0;
                }

                return Sizes.Sum(x => (int)x) + BundleSize;
            }
        }

        public int EntriesCount { get; internal set; }
        public int EntriesOffset { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public List<long> TOCOffsets = new List<long>();
        /// <summary>
        /// 
        /// </summary>
        public List<long> TOCSizes = new List<long>();
        /// <summary>
        /// 
        /// </summary>
        public List<int> TOCCas = new List<int>();
        /// <summary>
        /// 
        /// </summary>
        public List<int> TOCCatalog = new List<int>();
        /// <summary>
        /// 
        /// </summary>
        public List<bool> TOCPatch = new List<bool>();
        /// <summary>
        /// 
        /// </summary>
        public byte[] Flags;
        //public List<bool> Flags = new List<bool>();

        public override string ToString()
        {
            return $"Cas:{Cas}-Catalog:{Catalog}-BundleOffset:{BundleOffset}-Size:{TotalSize}";
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

    public class CASBundleEntry
    {
        public int unk { get; set; }
        public bool isInPatch { get; set; }
        public int catalog { get; set; }
        public int cas { get; set; }
        public uint bundleSizeInCas { get; set; }
        public long locationOfSize { get; set; }
        public uint bundleOffsetInCas { get; set; }
        public long locationOfOffset { get; set; }
    }
}
