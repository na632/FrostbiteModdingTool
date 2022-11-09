using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(x => x.EndsWith("the.key"));
			if (string.IsNullOrEmpty(resourceName))
				return false;
			byte[] array = null;
			var memStream = new MemoryStream();
			var manStream = assembly.GetManifestResourceStream(resourceName);
			if (manStream == null)
				return false;

			manStream.CopyTo(memStream);
			array = memStream.ToArray();
			memStream.Close();
            manStream.Close();
            memStream.Dispose();
            manStream.Dispose();

            if (array == null)
                return false;

            if (array.Length == 0)
                return false;

            byte[] array2 = new byte[16];
			Array.Copy(array, array2, 16);
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
	}
}
