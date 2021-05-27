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

		public string CasPath = "";

        public int CasIndex { get; set; }
    }
}
