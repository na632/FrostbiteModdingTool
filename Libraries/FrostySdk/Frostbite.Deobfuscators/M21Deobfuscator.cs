using FMT.FileTools;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostySdk.Deobfuscators
{
	public class M21Deobfuscator : IDeobfuscator
	{
		public long Initialize(NativeReader reader)
		{
			//uint num = reader.ReadUInt();
			//if (num != 30331136 && num != 63885568)
			//{
			//	reader.Position = 0L;
			//	return -1L;
			//}
			reader.Position = 556L;
			return reader.Length;
		}

		public bool AdjustPosition(NativeReader reader, long newPosition)
		{
			return false;
		}

		public void Deobfuscate(byte[] buffer, long position, int offset, int numBytes)
		{
		}
	}
}
