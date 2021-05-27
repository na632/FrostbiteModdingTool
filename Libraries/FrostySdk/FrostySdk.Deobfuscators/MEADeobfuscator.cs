using FrostySdk.Interfaces;
using FrostySdk.IO;
using System;
using System.IO;

namespace FrostySdk.Deobfuscators
{
	public class MEADeobfuscator : IDeobfuscator
	{
		private uint magic;

		private byte obfuscationType;

		private byte initialValue;

		private byte currentValue;

		public long Initialize(NativeReader reader)
		{
			if (reader.Length < 36)
			{
				return -1L;
			}
			reader.Position = reader.Length - 36;
			int num = reader.ReadInt();
			string a = reader.ReadSizedString(32);
			long num2 = reader.Length;
			if (a == "@e!adnXd$^!rfOsrDyIrI!xVgHeA!6Vc")
			{
				reader.Position = reader.Length - num;
				byte[] array = reader.ReadBytes(num);
				uint num3 = 0u;
				uint num4 = 0u;
				uint num5 = 0u;
				uint num6 = 0u;
				uint num7 = 0u;
				short num8 = BitConverter.ToInt16(array, 392);
				if (num8 != 0)
				{
					for (int i = 0; i < num8; i++)
					{
						byte b = array[410 + i];
						num3 = (b ^ (2 * (b + num3)));
					}
				}
				num4 = (uint)((int)num4 + (array[405] ^ (2 * (array[405] + (array[404] ^ (2 * (array[404] + (array[403] ^ (2 * (array[403] + (array[402] ^ (2 * array[402]))))))))))));
				num4 = (uint)((int)num4 + (array[3] ^ (2 * (array[3] + (array[2] ^ (2 * (array[2] + (array[1] ^ (2 * (array[1] + (array[0] ^ (2 * array[0]))))))))))));
				num4 = (uint)((int)num4 + (array[391] ^ (2 * array[391])));
				num4 += num3;
				num4 = (uint)((int)num4 + (array[397] ^ (2 * (array[397] + (array[396] ^ (2 * (array[396] + (array[395] ^ (2 * (array[395] + (array[394] ^ (2 * array[394]))))))))))));
				num5 = (uint)((int)num5 + (array[409] ^ (2 * (array[409] + (array[408] ^ (2 * (array[408] + (array[407] ^ (2 * (array[407] + (array[406] ^ (2 * array[406]))))))))))));
				num5 += num4;
				for (int j = 0; j < 129; j++)
				{
					byte b2 = array[j * 3 + 5 - 1];
					byte b3 = array[j * 3 + 5];
					byte b4 = array[j * 3 + 5 + 1];
					num6 = (b4 ^ (2 * (b4 + (b3 ^ (2 * (b3 + (b2 ^ (2 * (b2 + num6)))))))));
				}
				num7 = num6 + num5;
				if (num8 != 0)
				{
					DeobfuscateBlock(array, 410, num8);
				}
				DeobfuscateBlock(array, 394, 4);
				DeobfuscateBlock(array, 0, 4);
				DeobfuscateBlock(array, 402, 4);
				DeobfuscateBlock(array, 406, 4);
				DeobfuscateBlock(array, 4, 387);
				magic = BitConverter.ToUInt32(array, 0);
				obfuscationType = array[4];
				initialValue = (byte)(array[5] ^ num7);
				currentValue = initialValue;
				num2 -= num;
			}
			reader.Position = 0L;
			uint num9 = reader.ReadUInt();
			if (num9 == 30331136 || num9 == 63885568)
			{
				reader.Position = 556L;
			}
			else
			{
				reader.Position = 0L;
			}
			return num2;
		}

		public bool AdjustPosition(NativeReader reader, long newPosition)
		{
			if (magic == 0)
			{
				return false;
			}
			if (newPosition == 0L)
			{
				currentValue = initialValue;
				return false;
			}
			if (reader.Position > newPosition)
			{
				throw new InvalidOperationException("Cannot move backwards in obfuscated stream");
			}
			while (reader.Position != newPosition)
			{
				reader.ReadByte();
			}
			return true;
		}

		public void Deobfuscate(byte[] buffer, long position, int offset, int numBytes)
		{
			if (magic == 4)
			{
				long num = position - numBytes;
				if (obfuscationType != 2)
				{
					throw new InvalidDataException("Unimplemented obfuscation method used");
				}
				for (int i = 0; i < numBytes; i++)
				{
					byte b = buffer[i];
					buffer[i] = (byte)(currentValue ^ b);
					currentValue = (byte)((b ^ initialValue) - (num + i));
				}
			}
		}

		private void DeobfuscateBlock(byte[] buffer, int offset, int count)
		{
			int num = 1172968056;
			int num2 = 0;
			for (int i = 0; i < count; i++)
			{
				int num3 = (byte)(buffer[i + offset] ^ (num + ((num >> 8) & 0xFF) + (num >> 16) + ((num >> 24) & 0xFF)));
				buffer[i + offset] = (byte)num3;
				int num4 = RollOver(num, num3 & 0x1F);
				num = RollOver((num3 | ((num3 | ((num3 | (num3 << 8)) << 8)) << 8)) + num4, 1);
				if (num2 > 16)
				{
					num *= 2;
					num2 = 0;
				}
				num2++;
			}
		}

		private int RollOver(int value, int count)
		{
			int num = 32;
			count %= num;
			int num2 = value >> num - count;
			num2 &= ~(-1 << count);
			value <<= count;
			value |= num2;
			return value;
		}
	}
}
