namespace FrostySdk.Resources
{
	public class DelayLoadBundle
	{
		private string name;

		private int unknown;

		private uint hash;

		public string Name
		{
			get
			{
				return name;
			}
			internal set
			{
				name = value;
			}
		}

		public uint Hash => hash;

		public DelayLoadBundle(int inUnk, uint inHash)
		{
			unknown = inUnk;
			hash = inHash;
		}
	}
}
