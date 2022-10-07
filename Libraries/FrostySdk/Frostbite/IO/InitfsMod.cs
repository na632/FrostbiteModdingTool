using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.Frostbite.IO
{
	public class InitFSMod
	{
		public string Description { get; set; } = string.Empty;

		public Dictionary<string, byte[]> Data { get; } = new Dictionary<string, byte[]>();

		public void ModifyFile(string key, byte[] data)
		{
			this.Data[key] = data;
		}

		public void Clear(string key)
		{
			if (this.Data.Remove(key))
			{
			}
		}

	}
}
