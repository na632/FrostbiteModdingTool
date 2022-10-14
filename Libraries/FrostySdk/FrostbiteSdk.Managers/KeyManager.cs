using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace FrostySdk.Managers
{
	public class KeyManager
	{
		private Dictionary<string, byte[]> keys = new Dictionary<string, byte[]>();

		public static KeyManager Instance { get; } = new KeyManager();

		private KeyManager()
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

		public bool ReadInKeys()
		{
			if (Instance.keys.Count > 0)
				return true;

			byte[] array;

			if (File.Exists(ProfilesLibrary.CacheName + ".key"))
			{

				array = NativeReader.ReadInStream(new FileStream(ProfilesLibrary.CacheName + ".key", FileMode.Open, FileAccess.Read));
				byte[] array2 = new byte[16];
				Array.Copy(array, array2, 16);
				// From byte array to string
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
