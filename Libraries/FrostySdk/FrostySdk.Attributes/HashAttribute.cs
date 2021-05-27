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
        public ulong ActualHash
		{
			get;
			set;
		}

		public HashAttribute(int inHash)
		{
			ActualHash = Convert.ToUInt64(inHash);
		}

		public HashAttribute(uint inHash)
		{
			ActualHash = Convert.ToUInt64(inHash);
		}
	}
}
