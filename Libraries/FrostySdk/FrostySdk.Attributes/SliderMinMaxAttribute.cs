using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SliderMinMaxAttribute : EditorMetaDataAttribute
	{
		public float MinValue
		{
			get;
			set;
		}

		public float MaxValue
		{
			get;
			set;
		}

		public float SmallChange
		{
			get;
			set;
		}

		public float LargeChange
		{
			get;
			set;
		}

		public SliderMinMaxAttribute(float min, float max, float small, float large)
		{
			MinValue = min;
			MaxValue = max;
			SmallChange = small;
			LargeChange = large;
		}
	}
}
