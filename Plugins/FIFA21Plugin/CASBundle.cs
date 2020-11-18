using System.Collections.Generic;
using System.Linq;

namespace FIFA21Plugin
{
    public class CASBundle
    {
        public int Catalog { get; set; }
        public int Cas { get; set; }

        public int BundleOffset { get; set; }
        public int DataOffset { get; internal set; }

        public List<int> Sizes = new List<int>();

        public List<int> Offsets = new List<int>();

        public long TotalSize
        { 
            get
            {
                if(Sizes.Count == 0)
                {
                    return 0;
                }

                //return Sizes.Where(x => x < 10095655).Sum() + Offsets.Where(x => x < int.MaxValue).Sum() + DataOffset;
                return Sizes.Where(x => x < 10095655).Sum() + DataOffset;
            } 
        }
    }
}