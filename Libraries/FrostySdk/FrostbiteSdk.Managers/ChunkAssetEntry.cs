using System;

namespace FrostySdk.Managers
{
	public sealed class ChunkAssetEntry : AssetEntry
	{
		public Guid Id { get; set; }

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

		public override string Name => Id.ToString();

		public override string Type => "Chunk";

		public override string AssetType => "chunk";
	}
}
