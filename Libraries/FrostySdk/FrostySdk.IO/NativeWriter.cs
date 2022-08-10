using FrostySdk.Resources;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace FrostySdk.IO
{
	public class NativeWriter : BinaryWriter
	{
		public long Length { get { return BaseStream.Length; } }
		public long Position { get { return BaseStream.Position; } set { BaseStream.Position = value; } }

		private Encoding encoding;
 			
		public NativeWriter(Stream inStream, bool leaveOpen = false, bool wide = false)
			//: base(inStream, wide ? Encoding.Unicode : Encoding.Default, leaveOpen)
			: base(inStream, wide ? Encoding.UTF8 : Encoding.Default, leaveOpen)
		{
			encoding = wide ? Encoding.UTF8 : Encoding.Default;
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

		public void Write(ulong value, Endian endian)
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

		public void WriteInt16BigEndian(short value)
		{
			Write(value, Endian.Big);
		}
		public void WriteInt16LittleEndian(short value)
		{
			Write(value, Endian.Little);
		}

		public void WriteUInt16(ushort value, Endian endian)
		{
			switch (endian)
			{
				case Endian.Little:
					WriteUInt16LittleEndian(value);
					break;
				case Endian.Big:
					WriteUInt16BigEndian(value);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public void WriteUInt16BigEndian(ushort value)
		{
			//Span<byte> span = stackalloc byte[2];
			//BinaryPrimitives.WriteUInt16BigEndian(span, value);
			//Write(span);

			Write(value, Endian.Big);

		}

		public void WriteUInt16LittleEndian(ushort value)
		{
			//Span<byte> span = stackalloc byte[2];
			//BinaryPrimitives.WriteUInt16LittleEndian(span, value);
			//Write(span);
			Write(value, Endian.Little);
		}

		public void WriteInt32BigEndian(int value)
		{
			Write(value, Endian.Big);
		}

		public void WriteInt32LittleEndian(int value)
		{
			//Span<byte> span = stackalloc byte[4];
			//BinaryPrimitives.WriteInt32LittleEndian(span, value);
			//Write(span);
			Write((int)value);
		}

		public void WriteUInt32BigEndian(uint value)
		{
			Write(value, Endian.Big);
		}

		public void WriteUInt32LittleEndian(uint value)
		{
			//Span<byte> span = stackalloc byte[4];
			//BinaryPrimitives.WriteUInt32LittleEndian(span, value);
			//Write(span);
			Write((uint)value);

		}

		public void WriteInt64LittleEndian(long value)
		{
			//Span<byte> span = stackalloc byte[8];
			//BinaryPrimitives.WriteInt64LittleEndian(span, value);
			//Write(span);
			Write((long)value);

		}

		public void WriteInt64BigEndian(long value)
		{
			//Span<byte> span = stackalloc byte[8];
			//BinaryPrimitives.WriteInt64BigEndian(span, value);
			//Write(span);

			Write((long)value, Endian.Big);

		}

		public void WriteUInt64LittleEndian(ulong value)
		{
			//Span<byte> span = stackalloc byte[8];
			//BinaryPrimitives.WriteUInt64LittleEndian(span, value);
			//Write(span);
			Write((ulong)value, Endian.Little);

		}

		public void WriteUInt64BigEndian(ulong value)
		{
            //Span<byte> span = stackalloc byte[8];
            //BinaryPrimitives.WriteUInt64BigEndian(span, value);
            //Write(span);

            Write((ulong)value, Endian.Big);

        }

		


		public void WriteSingleLittleEndian(float value)
		{
			Write(value);
		}
		public void WriteDoubleLittleEndian(double value)
		{
			Write(value);
		}

		public void WriteGuid(Guid value)
		{
			Write(value);
		}

		public void WriteBytes(byte[] value)
        {
			Write(value);
        }

		//public void WriteBytes(byte[] data, int offset, int length)
		//{
		//	BaseStream.Write(data, offset, length);
		//}


		private void WriteString(string str)
		{
            //for (int i = 0; i < str.Length; i++)
            //{
            //    Write(str[i]);
            //}
            //int byteCount = encoding.GetByteCount(str);
            var bytes = encoding.GetBytes(str);
            Write(bytes);
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
		public void WriteLengthPrefixedString(string str)
		{
			if (string.IsNullOrEmpty(str) || str.Length == 0)
			{
				Write7BitEncodedInt(0);
			}
			else
			{
				Write7BitEncodedInt(str.Length);
				WriteString(str);
			}
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

		public void WriteEmpty(int numberOfBytes)
        {
			WriteEmptyBytes(numberOfBytes);
		}

		public void WriteEmptyBytes(int numberOfBytes)
		{
			Write(new byte[numberOfBytes]);
		}

		public void WriteVector3(Vec3 vec)
		{
			Write(vec.x);
			Write(vec.y);
			Write(vec.z);
			Write(vec.pad);
		}

		public void WriteAxisAlignedBox(AxisAlignedBox aab)
		{
			WriteVector3(aab.min);
			WriteVector3(aab.max);
		}

		public void WriteLinearTransform(LinearTransform lt)
		{
			WriteVector3(lt.right);
			WriteVector3(lt.up);
			WriteVector3(lt.forward);
			WriteVector3(lt.trans);
		}
	}

	public class FileWriter : NativeWriter
    {
		public FileWriter(Stream inStream, bool leaveOpen = false, bool wide = false)
			: base(inStream, leaveOpen, wide)
		{
		}
	}
}
