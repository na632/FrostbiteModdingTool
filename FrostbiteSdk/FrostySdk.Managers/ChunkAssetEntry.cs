using System;

namespace FrostySdk.Managers
{
	public class ChunkAssetEntry : AssetEntry
	{
		public Guid Id;

		public uint BundledSize;

		public uint LogicalOffset;

		public uint LogicalSize;

		public uint RangeStart;

		public uint RangeEnd;

		public int H32;

		public int FirstMip;

		public bool IsTocChunk;

		public override string Name => Id.ToString();

		public override string Type => "Chunk";

		public override string AssetType => "chunk";
	}
}
