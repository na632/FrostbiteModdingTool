namespace FrostySdk
{
	public struct ManifestFileRef
	{
		private int value;

		public int CatalogIndex => (value >> 12) - 1;

		public bool IsInPatch => (value & 0x100) != 0;

		public int CasIndex => (value & 0xFF) + 1;

		public ManifestFileRef(int inIndex, bool inPatch, int inCasIndex)
		{
			value = ((inIndex + 1 << 12) | (inPatch ? 256 : 0) | ((inCasIndex - 1) & 0xFF));
		}

		public static implicit operator ManifestFileRef(int inValue)
		{
			ManifestFileRef result = default(ManifestFileRef);
			result.value = inValue;
			return result;
		}

		public static implicit operator int(ManifestFileRef inRef)
		{
			return inRef.value;
		}
	}
}
