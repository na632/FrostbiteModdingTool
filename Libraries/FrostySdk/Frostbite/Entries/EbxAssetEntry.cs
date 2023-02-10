using FMT.FileTools;
using System;
using System.Collections.Generic;

namespace FrostySdk.Managers
{
	public sealed class EbxAssetEntry : AssetEntry, IAssetEntry
	{
        public EbxAssetEntry(ModifiedAssetEntry modifiedAssetEntry = null) : base(modifiedAssetEntry)
        {
        }

		public Guid? Guid => Id;

		public override string AssetType => EAssetType.ebx.ToString();

        public bool IsBinary { get; internal set; }

        public override AssetEntry Clone()
        {
			return this.MemberwiseClone() as EbxAssetEntry;
		}
    }
}
