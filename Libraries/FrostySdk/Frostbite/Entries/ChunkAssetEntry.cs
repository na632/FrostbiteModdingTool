using System;
using System.Linq;

namespace FrostySdk.Managers
{
    public sealed class ChunkAssetEntry : AssetEntry
    {

        //public Guid Id { get; set; }

        public uint BundledSize { get; set; }

        public uint LogicalOffset { get; set; }

        public uint SB_LogicalOffset_Position { get; set; }

        public uint LogicalSize { get; set; }

        public uint SB_LogicalSize_Position { get; set; }


        public uint RangeStart { get; set; }

        public uint RangeEnd { get; set; }

        public int H32 { get; set; }

        public int FirstMip { get; set; }

        public bool IsTocChunk { get; set; }

        public Guid? DuplicatedFromId
        {
            get
            {
                if (!string.IsNullOrEmpty(DuplicatedFromName))
                {
                    return Guid.Parse(DuplicatedFromName);
                }

                return null;
            }
        }

        public override string Name => Id.ToString();

        public override string Type => "Chunk";

        public override string AssetType => EAssetType.chunk.ToString();

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                if (LinkedAssets.Any())
                {
                    var firstLinkedAsst = LinkedAssets.FirstOrDefault();
                    if (firstLinkedAsst != null)
                    {
                        return Name + "[" + firstLinkedAsst.Name + "]";
                    }
                }

                if (AssetManager.Instance != null)
                {
                    if (AssetManager.Instance.EnumerateCustomAssets("legacy", false).Any())
                    {
                        var legAsset = AssetManager.Instance.GetCustomAssetEntry("legacy", Name);
                        if (legAsset != null)
                        {
                            return Name + "[" + legAsset.Name + "]";
                        }

                    }
                }

                return Name;
            }
            return base.ToString();
        }
    }
}
