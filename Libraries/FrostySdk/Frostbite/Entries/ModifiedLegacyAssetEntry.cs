using FrostySdk.Managers;

namespace FrostySdk.FrostbiteSdk.Managers
{
    public class ModifiedLegacyAssetEntry : ModifiedAssetEntry, IModifiedAssetEntry
    {
        public override object DataObject { get; set; }
    }
}
