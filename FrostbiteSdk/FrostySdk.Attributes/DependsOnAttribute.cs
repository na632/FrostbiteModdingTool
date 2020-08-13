using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DependsOnAttribute : Attribute
	{
		public string Name
		{
			get;
			set;
		}

		public DependsOnAttribute(string name)
		{
			Name = name;
		}
	}
}
