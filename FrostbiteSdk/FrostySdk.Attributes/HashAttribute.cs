using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class HashAttribute : Attribute
	{
		public int Hash
		{
			get;
			set;
		}

		public HashAttribute(int inHash)
		{
			Hash = inHash;
		}
	}
}
