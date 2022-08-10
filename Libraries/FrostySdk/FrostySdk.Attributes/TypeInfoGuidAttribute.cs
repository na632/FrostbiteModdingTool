using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, AllowMultiple = true, Inherited = false)]
	public class TypeInfoGuidAttribute : Attribute
	{
		public Guid Guid
		{
			get;
			set;
		}

		public TypeInfoGuidAttribute(string inGuid)
		{
			Guid = Guid.Parse(inGuid);
		}
	}
}
