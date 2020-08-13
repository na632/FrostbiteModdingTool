namespace FrostySdk.Managers
{
	public class ResAssetEntry : AssetEntry
	{
		public ulong ResRid;

		public uint ResType;

		public byte[] ResMeta;

		public override string Type
		{
			get
			{
				ResourceType resType = (ResourceType)ResType;
				return resType.ToString();
			}
		}

		public override string AssetType => "res";
	}
}
