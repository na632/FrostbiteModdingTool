using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostbiteSdk.Managers
{
    public class ModifiedLegacyAssetEntry : ModifiedAssetEntry, IModifiedAssetEntry
    {
        public override object DataObject { get; set; }
    }
}
