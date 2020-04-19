// Decompiled with JetBrains decompiler
// Type: FifaLibrary.DbReader
// Assembly: FifaLibrary19, Version=14.3.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5E4EB9DC-F35A-4480-B02D-6E230B595138
// Assembly location: C:\Program Files (x86)\RDBM 20\RDBM20\FifaLibrary19.dll

using System;
using System.IO;

namespace v2k4FIFAModdingCL.CGFE
{
    public class DbReader : BinaryReader
    {
        private int m_CurrentByte;
        private int m_CurrentBitPosition;
        private FifaPlatform m_Platform;

        public DbReader(Stream stream, FifaPlatform platform)
          : base(stream)
        {
            this.m_Platform = platform;
        }

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

        public override short ReadInt16()
        {
            if (this.m_Platform != FifaPlatform.XBox)
                return base.ReadInt16();
            byte[] numArray = this.ReadBytes(2);
            Array.Reverse((Array)numArray);
            return BitConverter.ToInt16(numArray, 0);
        }

        public override ushort ReadUInt16()
        {
            if (this.m_Platform != FifaPlatform.XBox)
                return base.ReadUInt16();
            byte[] numArray = this.ReadBytes(2);
            Array.Reverse((Array)numArray);
            return BitConverter.ToUInt16(numArray, 0);
        }

        public override int ReadInt32()
        {
            if (this.m_Platform != FifaPlatform.XBox)
                return base.ReadInt32();
            byte[] numArray = this.ReadBytes(4);
            Array.Reverse((Array)numArray);
            return BitConverter.ToInt32(numArray, 0);
        }

        public override uint ReadUInt32()
        {
            if (this.m_Platform != FifaPlatform.XBox)
                return base.ReadUInt32();
            byte[] numArray = this.ReadBytes(4);
            Array.Reverse((Array)numArray);
            return BitConverter.ToUInt32(numArray, 0);
        }

        public override long ReadInt64()
        {
            if (this.m_Platform != FifaPlatform.XBox)
                return base.ReadInt64();
            byte[] numArray = this.ReadBytes(8);
            Array.Reverse((Array)numArray);
            return BitConverter.ToInt64(numArray, 0);
        }

        public override ulong ReadUInt64()
        {
            if (this.m_Platform != FifaPlatform.XBox)
                return base.ReadUInt64();
            byte[] numArray = this.ReadBytes(8);
            Array.Reverse((Array)numArray);
            return BitConverter.ToUInt64(numArray, 0);
        }

        public override float ReadSingle()
        {
            if (this.m_Platform != FifaPlatform.XBox)
                return base.ReadSingle();
            byte[] numArray = this.ReadBytes(4);
            Array.Reverse((Array)numArray);
            return BitConverter.ToSingle(numArray, 0);
        }

        public override double ReadDouble()
        {
            if (this.m_Platform != FifaPlatform.XBox)
                return base.ReadDouble();
            byte[] numArray = this.ReadBytes(8);
            Array.Reverse((Array)numArray);
            return BitConverter.ToDouble(numArray, 0);
        }

        public int PopInteger(FieldDescriptor fieldDescriptor)
        {
            return this.m_Platform == FifaPlatform.PC ? this.PopIntegerPc(fieldDescriptor) : this.PopIntegerXbox(fieldDescriptor);
        }

        private int PopIntegerXbox(FieldDescriptor fieldDescriptor)
        {
            int num1 = (int)this.ReadByte();
            int num2 = fieldDescriptor.BitOffset % 8;
            int num3 = 0;
            for (int index = fieldDescriptor.Depth - 1; index >= 0; --index)
            {
                int num4 = (num1 & 128 >> num2) == 0 ? 0 : 1;
                num3 += num4 << index;
                ++num2;
                if (num2 == 8)
                {
                    num1 = (int)this.ReadByte();
                    num2 = 0;
                }
            }
            return num3 + fieldDescriptor.RangeLow;
        }

        private int PopIntegerPc(FieldDescriptor fieldDescriptor)
        {
            int num1 = 0;
            int depth = fieldDescriptor.Depth;
            int num2 = 0;
            if (this.m_CurrentBitPosition != 0)
            {
                num2 = 8 - this.m_CurrentBitPosition;
                num1 = this.m_CurrentByte >> this.m_CurrentBitPosition;
            }
            for (; num2 < depth; num2 += 8)
            {
                this.m_CurrentByte = (int)this.ReadByte();
                num1 += this.m_CurrentByte << num2;
            }
            this.m_CurrentBitPosition = depth + 8 - num2 & 7;
            int num3 = (int)((1L << depth) - 1L);
            return (num1 & num3) + fieldDescriptor.RangeLow;
        }

        public void Align(long position)
        {
            this.BaseStream.Position = position;
            this.m_CurrentBitPosition = 0;
            this.m_CurrentByte = 0;
        }

        public void AlignToByte()
        {
            if (this.m_CurrentBitPosition == 0)
                return;
            this.m_CurrentBitPosition = 0;
            this.m_CurrentByte = 0;
        }

        public void AlignTo32Bit()
        {
            int num = (int)(this.BaseStream.Position & 3L);
            if (num != 0)
                this.BaseStream.Position += (long)(4 - num);
            this.m_CurrentBitPosition = 0;
        }
    }
}
