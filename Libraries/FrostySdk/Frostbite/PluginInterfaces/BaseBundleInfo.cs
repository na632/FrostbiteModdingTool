using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.Frostbite.PluginInterfaces
{
    public class BaseBundleInfo
    {
        public static int BundleItemIndex = 0;

        public string Name { get; set; }

        public long Offset { get; set; }

        public long TOCSizePosition { get; set; }

        public long Size { get; set; }

        public long TocOffset { get; set; }

        public int CasIndex { get; set; }
        public int Unk { get; set; }

        public int TocBundleIndex { get; set; }

        public override string ToString()
        {
            return $"Offset:{Offset}-Size:{Size}-Index:{TocBundleIndex}";
        }

    }
}
