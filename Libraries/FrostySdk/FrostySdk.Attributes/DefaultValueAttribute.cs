using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class DefaultValueAttribute : Attribute
	{
		public object DefaultValue
		{
			get;
			set;
		}

		public DefaultValueAttribute(object value)
		{
			DefaultValue = value;
		}
	}
}
