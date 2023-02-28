using FMT.FileTools;
using FrostySdk.Interfaces;

namespace FrostySdk.Deobfuscators
{
    public class DADeobfuscator : IDeobfuscator
    {
        private byte[] key;

        public long Initialize(NativeReader reader)
        {
            uint num = reader.ReadUInt();
            if (num != 30331136 && num != 63885568)
            {
                reader.Position = 0L;
                return -1L;
            }
            if (num == 30331136)
            {
                reader.Position = 296L;
                key = reader.ReadBytes(260);
                for (int i = 0; i < key.Length; i++)
                {
                    key[i] ^= 123;
                }
            }
            reader.Position = 556L;
            return reader.Length;
        }

        public bool AdjustPosition(long newPosition)
        {
            return false;
        }

        public void Deobfuscate(byte[] buffer, long position, int offset, int numBytes)
        {
            if (key != null)
            {
                long num = position - numBytes - 556;
                for (int i = 0; i < numBytes; i++)
                {
                    buffer[i] = (byte)(key[(num + i) % 257] ^ buffer[i]);
                }
            }
        }

        public bool AdjustPosition(NativeReader reader, long newPosition)
        {
            return false;
        }
    }
}
