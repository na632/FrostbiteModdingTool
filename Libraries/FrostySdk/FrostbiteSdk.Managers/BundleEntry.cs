using Frosty.Hash;

namespace FrostySdk.Managers
{
	public class BundleEntry
	{
		public string Name;

		public int NameHash => Fnv1.HashString(Name);

		public int SuperBundleId;

		public EbxAssetEntry Blueprint;

		public BundleType Type;

		public bool Added;

		public string DisplayName => Name;

		public int PersistedIndex { get; set; }

		public static int PersistedIndexCount;

        public override string ToString()
        {
			if (!string.IsNullOrEmpty(Name)) return Name;

            return base.ToString();
        }
    }
}
