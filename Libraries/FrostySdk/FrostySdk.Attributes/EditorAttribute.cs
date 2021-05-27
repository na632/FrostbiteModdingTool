using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class EditorAttribute : Attribute
	{
		public string EditorType
		{
			get;
			set;
		}

		public EditorAttribute(string name)
		{
			EditorType = "FrostyEditor.Controls.Editors." + name;
		}
	}
}
