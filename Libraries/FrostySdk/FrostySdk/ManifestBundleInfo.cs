using System.Collections.Generic;

namespace FrostySdk
{
    public class ManifestBundleInfo
    {
        public int hash;

        public List<ManifestFileInfo> files = new List<ManifestFileInfo>();
    }
}
