using System;
using System.IO;

namespace FifaLibrary
{
	public class DbReader : BinaryReader
	{
		private int m_CurrentByte;

		private int m_CurrentBitPosition;

		private FifaPlatform m_Platform;

		public FifaPlatform Platform
		{
			get
			{
				return m_Platform;
			}
			set
			{
				m_Platform = value;
			}
		}

		public DbReader(Stream stream, FifaPlatform platform)
			: base(stream)
		{
			m_Platform = platform;
		}

		public override short ReadInt16()
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] array = base.ReadBytes(2);
				Array.Reverse(array);
				return BitConverter.ToInt16(array, 0);
			}
			return base.ReadInt16();
		}

		public override ushort ReadUInt16()
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] array = base.ReadBytes(2);
				Array.Reverse(array);
				return BitConverter.ToUInt16(array, 0);
			}
			return base.ReadUInt16();
		}

		public override int ReadInt32()
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] array = base.ReadBytes(4);
				Array.Reverse(array);
				return BitConverter.ToInt32(array, 0);
			}
			return base.ReadInt32();
		}

		public override uint ReadUInt32()
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] array = base.ReadBytes(4);
				Array.Reverse(array);
				return BitConverter.ToUInt32(array, 0);
			}
			return base.ReadUInt32();
		}

		public override long ReadInt64()
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] array = base.ReadBytes(8);
				Array.Reverse(array);
				return BitConverter.ToInt64(array, 0);
			}
			return base.ReadInt64();
		}

		public override ulong ReadUInt64()
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] array = base.ReadBytes(8);
				Array.Reverse(array);
				return BitConverter.ToUInt64(array, 0);
			}
			return base.ReadUInt64();
		}

		public override float ReadSingle()
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] array = base.ReadBytes(4);
				Array.Reverse(array);
				return BitConverter.ToSingle(array, 0);
			}
			return base.ReadSingle();
		}

		public override double ReadDouble()
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] array = base.ReadBytes(8);
				Array.Reverse(array);
				return BitConverter.ToDouble(array, 0);
			}
			return base.ReadDouble();
		}

		public int PopInteger(FieldDescriptor fieldDescriptor)
		{
			if (m_Platform == FifaPlatform.PC)
			{
				return PopIntegerPc(fieldDescriptor);
			}
			return PopIntegerXbox(fieldDescriptor);
		}

		private int PopIntegerXbox(FieldDescriptor fieldDescriptor)
		{
			int num = ReadByte();
			int num2 = fieldDescriptor.BitOffset % 8;
			int num3 = 0;
			for (int num4 = fieldDescriptor.Depth - 1; num4 >= 0; num4--)
			{
				int num5 = (((num & (128 >> num2)) != 0) ? 1 : 0);
				num3 += num5 << num4;
				num2++;
				if (num2 == 8)
				{
					num = ReadByte();
					num2 = 0;
				}
			}
			return num3 + fieldDescriptor.RangeLow;
		}

		private int PopIntegerPc(FieldDescriptor fieldDescriptor)
		{
			int num = 0;
			int depth = fieldDescriptor.Depth;
			int i = 0;
			if (m_CurrentBitPosition != 0)
			{
				i = 8 - m_CurrentBitPosition;
				num = m_CurrentByte >> m_CurrentBitPosition;
			}
			for (; i < depth; i += 8)
			{
				m_CurrentByte = ReadByte();
				num += m_CurrentByte << i;
			}
			m_CurrentBitPosition = (depth + 8 - i) & 7;
			int num2 = (int)((1L << depth) - 1);
			num &= num2;
			return num + fieldDescriptor.RangeLow;
		}

		public void Align(long position)
		{
			BaseStream.Position = position;
			m_CurrentBitPosition = 0;
			m_CurrentByte = 0;
		}

		public void AlignToByte()
		{
			if (m_CurrentBitPosition != 0)
			{
				m_CurrentBitPosition = 0;
				m_CurrentByte = 0;
			}
		}

		public void AlignTo32Bit()
		{
			int num = (int)(BaseStream.Position & 3);
			if (num != 0)
			{
				BaseStream.Position += 4 - num;
			}
			m_CurrentBitPosition = 0;
		}
	}
}
