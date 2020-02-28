using FrostySdk;

namespace Frosty.ModSupport
{
	public class FrostyAppliedMod
	{
		private bool isEnabled;

		private FrostyMod mod;

		public bool IsEnabled
		{
			get
			{
				return isEnabled;
			}
			set
			{
				isEnabled = value;
			}
		}

		public FrostyMod Mod => mod;

		public FrostyAppliedMod(FrostyMod inMod, bool inIsEnabled = true)
		{
			mod = inMod;
			isEnabled = inIsEnabled;
		}
	}
}
