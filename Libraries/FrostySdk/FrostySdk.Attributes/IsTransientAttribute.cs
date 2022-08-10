using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class IsTransientAttribute : Attribute
	{
	}
}
