using System;
using System.IO;
using System.Text;

namespace FrostySdk.IO
{
	public class NativeWriter : BinaryWriter
	{
		public NativeWriter(Stream inStream, bool leaveOpen = false, bool wide = false)
			: base(inStream, wide ? Encoding.Unicode : Encoding.Default, leaveOpen)
		{
		}

		public void Write(Guid value, Endian endian)
		{
			if (endian == Endian.Big)
			{
				byte[] array = value.ToByteArray();
				Write(array[3]);
				Write(array[2]);
				Write(array[1]);
				Write(array[0]);
				Write(array[5]);
				Write(array[4]);
				Write(array[7]);
				Write(array[6]);
				for (int i = 0; i < 8; i++)
				{
					Write(array[8 + i]);
				}
			}
			else
			{
				Write(value);
			}
		}

		public void Write(short value, Endian endian)
		{
			if (endian == Endian.Big)
			{
				Write((short)(ushort)(((value & 0xFF) << 8) | ((value & 0xFF00) >> 8)));
			}
			else
			{
				Write(value);
			}
		}

		public void Write(ushort value, Endian endian)
		{
			if (endian == Endian.Big)
			{
				Write((ushort)(((value & 0xFF) << 8) | ((value & 0xFF00) >> 8)));
			}
			else
			{
				Write(value);
			}
		}

		public void Write(int value, Endian endian)
		{
			if (endian == Endian.Big)
			{
				Write(((value & 0xFF) << 24) | ((value & 0xFF00) << 8) | ((value >> 8) & 0xFF00) | ((value >> 24) & 0xFF));
			}
			else
			{
				Write(value);
			}
		}

		public void Write(uint value, Endian endian)
		{
			if (endian == Endian.Big)
			{
				Write(((value & 0xFF) << 24) | ((value & 0xFF00) << 8) | ((value >> 8) & 0xFF00) | ((value >> 24) & 0xFF));
			}
			else
			{
				Write(value);
			}
		}

		public void Write(long value, Endian endian)
		{
			if (endian == Endian.Big)
			{
				Write(((value & 0xFF) << 56) | ((value & 0xFF00) << 40) | ((value & 0xFF0000) << 24) | ((value & 4278190080u) << 8) | (((value >> 8) & 4278190080u) | ((value >> 24) & 0xFF0000) | ((value >> 40) & 0xFF00) | ((value >> 56) & 0xFF)));
			}
			else
			{
				Write(value);
			}
		}

		private void WriteString(string str)
		{
			for (int i = 0; i < str.Length; i++)
			{
				Write(str[i]);
			}
		}

		public void WriteNullTerminatedString(string str)
		{
			WriteString(str);
			Write('\0');
		}

		public void WriteSizedString(string str)
		{
			Write7BitEncodedInt(str.Length);
			WriteString(str);
		}

		public void WriteFixedSizedString(string str, int size)
		{
			WriteString(str);
			for (int i = 0; i < size - str.Length; i++)
			{
				Write('\0');
			}
		}

		public new void Write7BitEncodedInt(int value)
		{
			uint num;
			for (num = (uint)value; num >= 128; num >>= 7)
			{
				Write((byte)(num | 0x80));
			}
			Write((byte)num);
		}

		public void Write7BitEncodedLong(long value)
		{
			ulong num;
			for (num = (ulong)value; num >= 128; num >>= 7)
			{
				Write((byte)(num | 0x80));
			}
			Write((byte)num);
		}

		public void Write(Guid value)
		{
			Write(value.ToByteArray(), 0, 16);
		}

		public void Write(Sha1 value)
		{
			Write(value.ToByteArray(), 0, 20);
		}

		public void WriteLine(string str)
		{
			WriteString(str);
			Write('\r');
			Write('\n');
		}

		public void WritePadding(byte alignment)
		{
			while (BaseStream.Position % (long)alignment != 0L)
			{
				Write((byte)0);
			}
		}
	}
}
