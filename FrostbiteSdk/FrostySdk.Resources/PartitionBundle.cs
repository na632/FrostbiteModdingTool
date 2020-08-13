using System;

namespace FrostySdk.Resources
{
	public class PartitionBundle
	{
		private DelayLoadBundle bundle;

		private Guid guid;

		private uint hash;

		public DelayLoadBundle Bundle
		{
			get
			{
				return bundle;
			}
			internal set
			{
				bundle = value;
			}
		}

		public Guid Guid => guid;

		public uint Hash => hash;

		public PartitionBundle(Guid inGuid, uint inHash)
		{
			guid = inGuid;
			hash = inHash;
		}
	}
}
