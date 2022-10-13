namespace FrostyModManager
{
	internal class ConfigElement
	{
		protected string id;

		protected string displayName;

		protected object currentValue;

		public object CurrentValue
		{
			get
			{
				return currentValue;
			}
			internal set
			{
				currentValue = value;
			}
		}

		public ConfigElement(string inId, string inDisplayName)
		{
			id = inId;
			displayName = inDisplayName;
		}

		public override string ToString()
		{
			return displayName;
		}
	}
	internal class ConfigElement<T> : ConfigElement
	{
		private T defValue;

		private T minValue;

		private T maxValue;

		public ConfigElement(string inId, string inDisplayName, T inDefValue, T inMinValue, T inMaxValue)
			: base(inId, inDisplayName)
		{
			defValue = inDefValue;
			minValue = inMinValue;
			maxValue = inMaxValue;
			currentValue = defValue;
		}
	}
}
