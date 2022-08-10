using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace FrostySdk.CPPConversions
{
    public class PerfectHash
    {
		public static uint getPerfectHashSize(uint entryCount, uint valueSize)
		{
			return sizeof(uint) + (sizeof(int) + valueSize) * entryCount;
		}

		public static uint hash(uint d, in string key)
		{
			var keySize = key.Length;
			if (d == 0)
				d = 0x811c9dc5;

			for (int i = 0; i < keySize; ++i)
				d = (d* 0x01000193) ^ key[i];
	
			// mod this by the FNV prime so that we can't get screwed by situations
			// where the last n bits happen to always be the same and we have 2^n
			// entries
			return d % 0x01000193;
		}

		public static string getPerfectHashValue(in byte[] buffer, in string key)
		{
			uint size = (uint)buffer.Length;

			uint G = size + 1;
	
			int slot = buffer[hash(0, key) % size];
			if (slot < 0)
				slot = -(slot + 1);
			else
				slot = Convert.ToInt32(hash(Convert.ToUInt32(slot), key) % size);

			return (G + size) + (slot * size).ToString();
		}
    }
}
