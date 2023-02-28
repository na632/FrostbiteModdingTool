using System.Collections.Generic;
using System.Linq;

namespace Madden22Plugin
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

        public int BundleOffset { get; set; }
        public int BundleSize { get; internal set; }

        public List<int> Sizes = new List<int>();

        public List<int> Offsets = new List<int>();

        public List<dynamic> Entries = new List<dynamic>();

        public long TotalSize
        {
            get
            {
                if (Sizes.Count == 0)
                {
                    return 0;
                }

                //return Sizes.Where(x => x < 10095655).Sum() + Offsets.Where(x => x < int.MaxValue).Sum() + DataOffset;
                return Sizes.Sum() + BundleSize;
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
}