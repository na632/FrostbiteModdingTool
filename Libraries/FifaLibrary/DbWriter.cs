using System;
using System.IO;

namespace FifaLibrary
{
	public class DbWriter : BinaryWriter
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

		public DbWriter(Stream stream, FifaPlatform platform)
			: base(stream)
		{
			m_Platform = platform;
		}

		public override void Write(short value)
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				base.Write(bytes);
			}
			else
			{
				base.Write(value);
			}
		}

		public override void Write(ushort value)
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				base.Write(bytes);
			}
			else
			{
				base.Write(value);
			}
		}

		public override void Write(int value)
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				base.Write(bytes);
			}
			else
			{
				base.Write(value);
			}
		}

		public override void Write(uint value)
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				base.Write(bytes);
			}
			else
			{
				base.Write(value);
			}
		}

		public override void Write(long value)
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				base.Write(bytes);
			}
			else
			{
				base.Write(value);
			}
		}

		public override void Write(ulong value)
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				base.Write(bytes);
			}
			else
			{
				base.Write(value);
			}
		}

		public override void Write(float value)
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				base.Write(bytes);
			}
			else
			{
				base.Write(value);
			}
		}

		public override void Write(double value)
		{
			if (m_Platform == FifaPlatform.XBox)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				base.Write(bytes);
			}
			else
			{
				base.Write(value);
			}
		}

		public void PushInteger(int value, FieldDescriptor fieldDescriptor)
		{
			if (m_Platform == FifaPlatform.PC)
			{
				PushIntegerPc(value, fieldDescriptor);
			}
			else
			{
				PushIntegerXbox(value, fieldDescriptor);
			}
		}

		private void PushIntegerXbox(int value, FieldDescriptor fieldDescriptor)
		{
			BinaryReader binaryReader = new BinaryReader(BaseStream);
			int num = 0;
			if (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
			{
				num = binaryReader.ReadByte();
				binaryReader.BaseStream.Position--;
			}
			int num2 = fieldDescriptor.BitOffset % 8;
			int num3 = value - fieldDescriptor.RangeLow;
			for (int num4 = fieldDescriptor.Depth - 1; num4 >= 0; num4--)
			{
				int num5 = num3 & (1 << num4);
				int num6 = 128 >> num2;
				num = ((num5 != 0) ? (num | num6) : (num & ~num6));
				num2++;
				if (num2 == 8)
				{
					byte value2 = (byte)num;
					Write(value2);
					num2 = 0;
					num = 0;
					if (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
					{
						num = binaryReader.ReadByte();
						binaryReader.BaseStream.Position--;
					}
				}
			}
			if (num2 != 0)
			{
				byte value3 = (byte)num;
				Write(value3);
			}
		}

		private void PushIntegerPc(int value, FieldDescriptor fieldDescriptor)
		{
			int num = fieldDescriptor.Depth;
			bool flag = false;
			int num2 = value - fieldDescriptor.RangeLow;
			do
			{
				if (m_CurrentBitPosition + num > 8)
				{
					int num3 = 8 - m_CurrentBitPosition;
					int num4 = (1 << num3) - 1;
					int num5 = num2 & num4;
					m_CurrentByte += num5 << m_CurrentBitPosition;
					Write((byte)m_CurrentByte);
					num2 >>= num3;
					m_CurrentByte = 0;
					m_CurrentBitPosition = 0;
					num -= num3;
				}
				else if (m_CurrentBitPosition + num < 8)
				{
					m_CurrentByte += num2 << m_CurrentBitPosition;
					m_CurrentBitPosition += num;
					flag = true;
				}
				else
				{
					m_CurrentByte += num2 << m_CurrentBitPosition;
					Write((byte)m_CurrentByte);
					m_CurrentByte = 0;
					m_CurrentBitPosition = 0;
					flag = true;
				}
			}
			while (!flag);
		}

		public void WritePendingByte()
		{
			if (m_CurrentBitPosition != 0)
			{
				Write((byte)m_CurrentByte);
				m_CurrentBitPosition = 0;
				m_CurrentByte = 0;
			}
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
				Write(m_CurrentByte);
			}
		}

		public void AlignTo32Bit()
		{
			if (m_CurrentBitPosition != 0)
			{
				Write(m_CurrentByte);
			}
			int i = (int)(BaseStream.Position & 3);
			if (i != 0)
			{
				for (; i < 4; i++)
				{
					Write((byte)0);
				}
			}
		}
	}
}
