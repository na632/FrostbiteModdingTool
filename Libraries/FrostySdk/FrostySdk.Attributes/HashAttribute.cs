using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class HashAttribute : Attribute
	{
        public int Hash
        {
			get
			{
				return (int)(long)(ActualHash);
			}
        }
  //      public ulong ActualHash
		//{
		//	get;
		//	set;
		//}

		public long ActualHash
		{
			get;
			set;
		}

		public HashAttribute(int inHash)
		{
			if (long.TryParse(inHash.ToString(), out long r))
			{
				ActualHash = r;
			}
		}

		public HashAttribute(uint inHash)
		{
			ActualHash = Convert.ToInt64(inHash);
		}
	}
}
