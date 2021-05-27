using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class FieldIndexAttribute : Attribute
	{
		public int Index
		{
			get;
			set;
		}

		public FieldIndexAttribute(int inIndex)
		{
			Index = inIndex;
		}
	}
}
