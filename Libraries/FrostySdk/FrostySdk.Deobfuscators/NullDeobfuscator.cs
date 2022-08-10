using FrostySdk.Interfaces;
using FrostySdk.IO;

namespace FrostySdk.Deobfuscators
{
	public class NullDeobfuscator : IDeobfuscator
	{
		public long Initialize(NativeReader reader)
		{
			uint num = reader.ReadUInt();
			if (num != 30331136 && num != 63885568)
			{
				reader.Position = 0L;
				return -1L;
			}
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
