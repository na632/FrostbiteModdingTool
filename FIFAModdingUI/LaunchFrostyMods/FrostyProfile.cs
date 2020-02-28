using Frosty.ModSupport;
using FrostySdk;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace FrostyModManager
{
	public class FrostyProfile
	{
		private string name;

		private List<FrostyAppliedMod> appliedMods = new List<FrostyAppliedMod>();

		public string Name => name;

		public List<FrostyAppliedMod> AppliedMods => appliedMods;


		public FrostyProfile(string inName)
		{
			name = inName;
		}

		public void Refresh()
		{
			Config.Add("Profiles", name, ToConfigString());
		}

		public void AddMod(FrostyMod mod, bool isEnabled = true)
		{
			if (appliedMods.FindIndex((FrostyAppliedMod a) => a.Mod == mod) == -1)
			{
				appliedMods.Add(new FrostyAppliedMod(mod, isEnabled));
				Config.Add("Profiles", name, ToConfigString());
			}
		}

		public void RemoveMod(FrostyAppliedMod mod)
		{
			appliedMods.Remove(mod);
			Config.Add("Profiles", name, ToConfigString());
		}

		public void MoveModUp(FrostyAppliedMod mod)
		{
			int num = appliedMods.FindIndex((FrostyAppliedMod a) => a == mod);
			num--;
			if (num >= 0)
			{
				appliedMods.RemoveAt(num + 1);
				appliedMods.Insert(num, mod);
				Config.Add("Profiles", name, ToConfigString());
			}
		}

		public void MoveModDown(FrostyAppliedMod mod)
		{
			int num = appliedMods.FindIndex((FrostyAppliedMod a) => a == mod);
			num++;
			if (num < appliedMods.Count)
			{
				appliedMods.RemoveAt(num - 1);
				appliedMods.Insert(num, mod);
				Config.Add("Profiles", name, ToConfigString());
			}
		}

		private string ToConfigString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (appliedMods.Count > 0)
			{
				foreach (FrostyAppliedMod appliedMod in appliedMods)
				{
					stringBuilder.Append(appliedMod.Mod.Filename + ":" + appliedMod.IsEnabled.ToString() + "|");
				}
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}
	}
}
