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
                return this.m_Platform;
            }
            set
            {
                this.m_Platform = value;
            }
        }

        public DbWriter(Stream stream, FifaPlatform platform)
            : base(stream)
        {
            this.m_Platform = platform;
        }

        public override void Write(short value)
        {
            if (this.m_Platform == FifaPlatform.XBox)
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
            if (this.m_Platform == FifaPlatform.XBox)
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
            if (this.m_Platform == FifaPlatform.XBox)
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
            if (this.m_Platform == FifaPlatform.XBox)
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
            if (this.m_Platform == FifaPlatform.XBox)
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
            if (this.m_Platform == FifaPlatform.XBox)
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
            if (this.m_Platform == FifaPlatform.XBox)
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
            if (this.m_Platform == FifaPlatform.XBox)
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
            if (this.m_Platform == FifaPlatform.PC)
            {
                this.PushIntegerPc(value, fieldDescriptor);
            }
            else
            {
                this.PushIntegerXbox(value, fieldDescriptor);
            }
        }

        private void PushIntegerXbox(int value, FieldDescriptor fieldDescriptor)
        {
            BinaryReader binaryReader = new BinaryReader(this.BaseStream);
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
                    this.Write(value2);
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
                this.Write(value3);
            }
        }

        private void PushIntegerPc(int value, FieldDescriptor fieldDescriptor)
        {
            int num = fieldDescriptor.Depth;
            bool flag = false;
            int num2 = value - fieldDescriptor.RangeLow;
            do
            {
                if (this.m_CurrentBitPosition + num > 8)
                {
                    int num3 = 8 - this.m_CurrentBitPosition;
                    int num4 = (1 << num3) - 1;
                    int num5 = num2 & num4;
                    this.m_CurrentByte += num5 << this.m_CurrentBitPosition;
                    this.Write((byte)this.m_CurrentByte);
                    num2 >>= num3;
                    this.m_CurrentByte = 0;
                    this.m_CurrentBitPosition = 0;
                    num -= num3;
                }
                else if (this.m_CurrentBitPosition + num < 8)
                {
                    this.m_CurrentByte += num2 << this.m_CurrentBitPosition;
                    this.m_CurrentBitPosition += num;
                    flag = true;
                }
                else
                {
                    this.m_CurrentByte += num2 << this.m_CurrentBitPosition;
                    this.Write((byte)this.m_CurrentByte);
                    this.m_CurrentByte = 0;
                    this.m_CurrentBitPosition = 0;
                    flag = true;
                }
            }
            while (!flag);
        }

        public void WritePendingByte()
        {
            if (this.m_CurrentBitPosition != 0)
            {
                this.Write((byte)this.m_CurrentByte);
                this.m_CurrentBitPosition = 0;
                this.m_CurrentByte = 0;
            }
        }

        public void Align(long position)
        {
            this.BaseStream.Position = position;
            this.m_CurrentBitPosition = 0;
            this.m_CurrentByte = 0;
        }

        public void AlignToByte()
        {
            if (this.m_CurrentBitPosition != 0)
            {
                this.Write(this.m_CurrentByte);
            }
        }

        public void AlignTo32Bit()
        {
            if (this.m_CurrentBitPosition != 0)
            {
                this.Write(this.m_CurrentByte);
            }
            int i = (int)(this.BaseStream.Position & 3);
            if (i != 0)
            {
                for (; i < 4; i++)
                {
                    this.Write((byte)0);
                }
            }
        }
    }
}
