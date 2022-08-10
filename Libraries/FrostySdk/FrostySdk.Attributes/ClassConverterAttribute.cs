using System;
using System.Reflection;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class ClassConverterAttribute : Attribute
	{
		public Type Type
		{
			get;
			set;
		}

		public ClassConverterAttribute(string inType)
		{
			Type = Assembly.GetEntryAssembly().GetType("FrostyEditor.Controls.Editors." + inType);
		}
	}
}
