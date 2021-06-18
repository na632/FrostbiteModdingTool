namespace FrostySdk.Managers
{
	public class AssetExtraData
	{
		public Sha1 BaseSha1;

		public Sha1 DeltaSha1;

		//public long DataOffset;
		public uint DataOffset;

		public int SuperBundleId;

		public bool IsPatch;

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

		
    }
}
