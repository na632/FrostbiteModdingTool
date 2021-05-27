namespace FrostySdk.Managers
{
	public class BundleEntry
	{
		public string Name;

		public int SuperBundleId;

		public EbxAssetEntry Blueprint;

		public BundleType Type;

		public bool Added;

		public string DisplayName => Name;
	}
}
