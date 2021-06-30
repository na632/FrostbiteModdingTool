namespace FrostySdk.Managers
{
	public class AssetExtraData
	{
		public Sha1 BaseSha1 { get; set; }

		public Sha1 DeltaSha1 { get; set; }

		public uint DataOffset { get; set; }

		public int SuperBundleId { get; set; }

		public bool IsPatch { get; set; }

		private string casPath;

		public string CasPath
		{
			get
			{
				if (!string.IsNullOrEmpty(casPath))
					return casPath;

				if(Catalog.HasValue && Cas.HasValue)
                {
					return FileSystem.Instance.GetFilePath(Catalog.Value, Cas.Value, IsPatch);
                }

                if (CasIndex.HasValue)
                {
					return FileSystem.Instance.GetFilePath(CasIndex.Value);
				}

				return string.Empty;
			}
			set
			{
				casPath = value;
			}
		}

		public ushort? Catalog { get; set; }
		public ushort? Cas { get; set; }

        public int? CasIndex { get; set; }

		public byte Unk { get; set; }

		
    }
}
