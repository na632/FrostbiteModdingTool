using FrostySdk;

namespace Frosty.ModSupport
{
	public class FrostyAppliedMod
	{
		private bool isEnabled;

		private FrostbiteMod mod;

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

		public FrostbiteMod Mod => mod;

		public FrostyAppliedMod(FrostbiteMod inMod, bool inIsEnabled = true)
		{
			mod = inMod;
			isEnabled = inIsEnabled;
		}
	}
}
