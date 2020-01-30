using Microsoft.Win32;

namespace FifaLibrary
{
	public class RegistryInfo
	{
		private static string GetString(string year, string key)
		{
			string text = null;
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(year);
			if (registryKey != null)
			{
				text = (string)registryKey.GetValue(key);
				if (text.EndsWith("\\"))
				{
					text = text.Substring(0, text.Length - 1);
				}
				registryKey.Close();
			}
			return text;
		}

		public static string GetLatestFifaInstalled()
		{
			for (int num = 20; num >= 10; num--)
			{
				string text = "SOFTWARE\\EA SPORTS\\FIFA " + num.ToString();
				if (GetString(text, "Install Dir") != null)
				{
					return text;
				}
				text = "SOFTWARE\\Wow6432Node\\EA SPORTS\\FIFA " + num.ToString();
				if (GetString(text, "Install Dir") != null)
				{
					return text;
				}
			}
			return null;
		}

		public static string[] GetAllFifaInstalled()
		{
			int num = 0;
			string[] subKeyNames = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EA SPORTS").GetSubKeyNames();
			for (int i = 0; i < subKeyNames.Length; i++)
			{
				if (!subKeyNames[i].StartsWith("FIFA"))
				{
					subKeyNames[i] = null;
				}
				else
				{
					num++;
				}
			}
			if (num == 0)
			{
				return null;
			}
			string[] array = new string[num];
			num = 0;
			for (int j = 0; j < subKeyNames.Length; j++)
			{
				if (subKeyNames[j] != null)
				{
					array[num++] = subKeyNames[j];
				}
			}
			return array;
		}

		public static string GetFifaKey(int year)
		{
			return "SOFTWARE\\EA SPORTS\\FIFA " + year.ToString();
		}

		public static bool IsFifaInstalled(string game)
		{
			return GetString(game, "Install Dir") != null;
		}

		public static string GetInstallDir(string game)
		{
			string text = null;
			text = GetString(game, "Install Dir");
			if (text != null && text.EndsWith("\\"))
			{
				text = text.Substring(0, text.Length - 1);
			}
			return text;
		}

		public static string GetFolder(string game)
		{
			return GetString(game, "Folder");
		}

		public static string GetLocale(string game)
		{
			return GetString(game, "Locale");
		}
	}
}
