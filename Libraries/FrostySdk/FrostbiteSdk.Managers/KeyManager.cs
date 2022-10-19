using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace FrostySdk.Managers
{
	public class KeyManager
	{
		private Dictionary<string, byte[]> keys { get; } = new Dictionary<string, byte[]>();

		public static KeyManager Instance { get; } = new KeyManager();

		public KeyManager()
		{
        }

		public void AddKey(string id, byte[] data)
		{
			if (!keys.ContainsKey(id))
			{
				keys.Add(id, null);
			}
			keys[id] = data;
		}

		public byte[] GetKey(string id)
		{
			if (!keys.ContainsKey(id))
			{
				return null;
			}
			return keys[id];
		}

		public bool HasKey(string id)
		{
			return keys.ContainsKey(id);
		}

		public static bool ReadInKeys()
		{
			if (Instance.keys.Count > 0)
				return true;

			var pathToKey = System.IO.Path.Combine(AppContext.BaseDirectory, "FrostbiteKeys", "fifa20.key");

            if (File.Exists(pathToKey))
			{
                byte[] array = NativeReader.ReadInStream(new FileStream(pathToKey, FileMode.Open, FileAccess.Read));
                byte[] array2 = new byte[16];
				Array.Copy(array, array2, 16);
				string s = System.Text.Encoding.UTF8.GetString(array2, 0, array2.Length);
				KeyManager.Instance.AddKey("Key1", array2);
				if (array.Length > 16)
				{
					array2 = new byte[16];
					Array.Copy(array, 16, array2, 0, 16);
					KeyManager.Instance.AddKey("Key2", array2);
					array2 = new byte[16384];
					Array.Copy(array, 32, array2, 0, 16384);
					KeyManager.Instance.AddKey("Key3", array2);
				}
				return true;
			}
			return false;
		}
	}
}
