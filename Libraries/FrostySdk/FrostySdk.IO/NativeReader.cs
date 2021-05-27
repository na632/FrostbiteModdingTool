using FrostySdk.Interfaces;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;

namespace FrostySdk.IO
{
	public class NativeReader : IDisposable
	{
		protected Stream stream;

		protected IDeobfuscator deobfuscator;

		protected byte[] buffer;

		protected char[] charBuffer;

		protected long streamLength;

		protected Encoding wideDecoder;

		public Stream BaseStream => stream;

		public virtual long Position
		{
			get
			{
				if (stream == null)
				{
					return 0L;
				}
				return stream.Position;
			}
			set
			{
				if (deobfuscator == null || !deobfuscator.AdjustPosition(this, value))
				{
					stream.Position = value;
				}
			}
		}

		public virtual long Length => streamLength;

		public NativeReader(Stream inStream)
		{
			stream = inStream;
			if (stream != null)
			{
				streamLength = stream.Length;
			}
			wideDecoder = new UnicodeEncoding();
			buffer = new byte[20];
			charBuffer = new char[2];
		}

		public NativeReader(Stream inStream, IDeobfuscator inDeobfuscator)
			: this(inStream)
		{
			deobfuscator = inDeobfuscator;
			if (deobfuscator != null && stream != null)
			{
				long num = deobfuscator.Initialize(this);
				if (num != -1)
				{
					streamLength = num;
				}
			}
		}
		public NativeReader(Stream inStream, bool skipObfuscation)
			: this(inStream)
		{
			if (stream != null)
			{
				if (skipObfuscation)
				{
					uint num = ReadUInt32LittleEndian();
					if (num != 30331136 && num != 63885568)
					{
						Position = 0L;
					}
					else
					{
						Position = 556L;
					}
				}
			}
		}

		public NativeReader(Stream inStream, bool skipObfuscation, bool wide)
			: this(inStream)
		{
			if (stream != null)
			{
				wideDecoder = (wide ? Encoding.UTF8 : Encoding.ASCII);
				if (skipObfuscation)
				{
					uint num = ReadUInt32LittleEndian();
					if (num != 30331136 && num != 63885568)
					{
						Position = 0L;
					}
					else
					{
						Position = 556L;
					}
				}
			}
		}

		public NativeReader(byte[] data)
        {
			stream = new MemoryStream(data);
        }

		public NativeReader Skip(int numberOfBytesToSkip)
        {
			BaseStream.Position += numberOfBytesToSkip;
			return this;
        }

		public static byte[] ReadInStream(Stream inStream)
		{
			using (NativeReader nativeReader = new NativeReader(inStream))
			{
				return nativeReader.ReadToEnd();
			}
		}

		public char ReadWideChar()
		{
			FillBuffer(2);
			wideDecoder.GetChars(buffer, 0, 2, charBuffer, 0);
			return charBuffer[0];
		}

		public bool ReadBoolean()
		{
			return ReadByte() == 1;
		}

		public byte ReadByte()
		{
			FillBuffer(1);
			return buffer[0];
		}

		public sbyte ReadSByte()
		{
			FillBuffer(1);
			return (sbyte)buffer[0];
		}

		public short ReadShort(Endian inEndian = Endian.Little)
		{
			FillBuffer(2);
			if (inEndian == Endian.Little)
			{
				return (short)(buffer[0] | (buffer[1] << 8));
			}
			return (short)(buffer[1] | (buffer[0] << 8));
		}

		public int ReadInt16LittleEndian()
		{
			return ReadShort(Endian.Little);
		}

		public int ReadInt16BigEndian()
		{
			return ReadShort(Endian.Big);
		}

		public ushort ReadUShort(Endian inEndian = Endian.Little)
		{
			FillBuffer(2);
			if (inEndian == Endian.Little)
			{
				return (ushort)(buffer[0] | (buffer[1] << 8));
			}
			return (ushort)(buffer[1] | (buffer[0] << 8));
		}

		public ushort ReadUInt16LittleEndian()
		{
			return ReadUShort(Endian.Little);
		}

		public ushort ReadUInt16BigEndian()
		{
			return ReadUShort(Endian.Big);
		}


		public int ReadInt(Endian inEndian = Endian.Little)
		{
            FillBuffer(4);
            if (inEndian == Endian.Little)
            {
				return BitConverter.ToInt32(buffer.Take(4).ToArray());
                //return buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
			}
			return BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(buffer.Take(4).ToArray()));
			//return buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24);

			//Span<byte> span = stackalloc byte[4];
			//ReadIntoSpan(span);
			//return inEndian == Endian.Little ? BinaryPrimitives.ReadInt32LittleEndian(span) : BinaryPrimitives.ReadInt32BigEndian(span);
		}

		public int ReadInt32LittleEndian()
        {
			return ReadInt(Endian.Little);
        }

		public int ReadInt32BigEndian()
		{
			return ReadInt(Endian.Big);
		}

		public uint ReadUInt(Endian inEndian = Endian.Little)
		{
            FillBuffer(4);
            if (inEndian == Endian.Little)
            {
				return BitConverter.ToUInt32(buffer.Take(4).ToArray());
                //return (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
            }
			//return (uint)(buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24));
			return BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(buffer.Take(4).ToArray()));

			//Span<byte> span = stackalloc byte[4];
			//ReadIntoSpan(span);
			//return inEndian == Endian.Little ? BinaryPrimitives.ReadUInt32LittleEndian(span) : BinaryPrimitives.ReadUInt32BigEndian(span);
		}

		public uint ReadUInt32LittleEndian()
		{
			return ReadUInt(Endian.Little);
		}

		public uint ReadUInt32BigEndian()
		{
			return ReadUInt(Endian.Big);
		}


		public long ReadLong(Endian inEndian = Endian.Little)
		{
			FillBuffer(8);
			if(inEndian == Endian.Little)
				return BitConverter.ToInt64(buffer.Take(8).ToArray());
			return BinaryPrimitives.ReverseEndianness(BitConverter.ToInt64(buffer.Take(8).ToArray()));

			//if (inEndian == Endian.Little)
			//{
			//	return (long)(((ulong)(uint)(buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24)) << 32) | (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24)));
			//}
			//return (long)(((ulong)(uint)(buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24)) << 32) | (uint)(buffer[7] | (buffer[6] << 8) | (buffer[5] << 16) | (buffer[4] << 24)));

			//Span<byte> span = stackalloc byte[8];
			//ReadIntoSpan(span);
			//return inEndian == Endian.Little ? BinaryPrimitives.ReadInt64LittleEndian(span) : BinaryPrimitives.ReadInt64BigEndian(span);
		}

		public long ReadInt64LittleEndian()
		{
			return ReadLong(Endian.Little);
		}

		public long ReadInt64BigEndian()
		{
			return ReadLong(Endian.Big);
		}

		public ulong ReadULong(Endian inEndian = Endian.Little)
		{
			//Span<byte> span = stackalloc byte[8];
			//ReadIntoSpan(span);
			//return inEndian == Endian.Little ? BinaryPrimitives.ReadUInt64LittleEndian(span) : BinaryPrimitives.ReadUInt64BigEndian(span);

			//FillBuffer(8);
			//if (inEndian == Endian.Little)
			//{
			//    return ((ulong)(uint)(buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24)) << 32) | (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
			//}
			//return ((ulong)(uint)(buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24)) << 32) | (uint)(buffer[7] | (buffer[6] << 8) | (buffer[5] << 16) | (buffer[4] << 24));

			FillBuffer(8);
			if (inEndian == Endian.Little)
				return BitConverter.ToUInt64(buffer.Take(8).ToArray());
			return BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt64(buffer.Take(8).ToArray()));

		}

		public ulong ReadUInt64LittleEndian()
		{
			return ReadULong(Endian.Little);
		}

		public ulong ReadUInt64BigEndian()
		{
			return ReadULong(Endian.Big);
		}

		public unsafe float ReadFloat(Endian inEndian = Endian.Little)
		{
			FillBuffer(4);
			//if (inEndian == Endian.Big)
			//{
			//	uint num = 0u;
			//	num = (uint)((inEndian != 0) ? (buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24)) : (buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24)));
			//	return *(float*)(&num);
			//}
			//else
			//         {
			//	float myFloat = System.BitConverter.ToSingle(buffer.Take(4).ToArray(), 0);
			//	return myFloat;
			//}

			var int32bytes = BitConverter.ToInt32(buffer.Take(4).ToArray());
			var reInt32Bytes = BinaryPrimitives.ReverseEndianness(int32bytes);
			if (inEndian == Endian.Little)
			{
				return BitConverter.Int32BitsToSingle(int32bytes);
			}

			return BitConverter.Int32BitsToSingle(reInt32Bytes);

		}

		public unsafe float ReadSingleLittleEndian()
		{
			//Span<byte> span = stackalloc byte[4];
			//ReadIntoSpan(span);
			//return BitConverter.ToSingle(span);

			return ReadFloat();
		}

		public unsafe float ReadSingleBigEndian()
		{
			return ReadFloat(Endian.Big);
		}

		public unsafe double ReadDouble(Endian inEndian = Endian.Little)
		{
			FillBuffer(8);
			//uint num = 0u;
			//uint num2 = 0u;
			//if (inEndian == Endian.Little)
			//{
			//	num = (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
			//	num2 = (uint)(buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24));
			//}
			//else
			//{
			//	num = (uint)(buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24));
			//	num2 = (uint)(buffer[7] | (buffer[6] << 8) | (buffer[5] << 16) | (buffer[4] << 24));
			//}
			//ulong num3 = ((ulong)num2 << 32) | num;
			//return *(double*)(&num3);

			var int64bytes = BitConverter.ToInt64(buffer.Take(8).ToArray());
			var reInt64Bytes = BinaryPrimitives.ReverseEndianness(int64bytes);
			if (inEndian == Endian.Little)
			{
				return BitConverter.Int64BitsToDouble(int64bytes);
			}

			return BitConverter.Int64BitsToDouble(reInt64Bytes);
		}

		public unsafe double ReadDoubleLittleEndian()
		{
			return ReadDouble(Endian.Little);
		}

		public unsafe double ReadDoubleBigEndian()
		{
			return ReadDouble(Endian.Big);
		}

		public Guid ReadGuid(Endian endian = Endian.Little)
		{
			FillBuffer(16);

			if (endian == Endian.Little)
			{
				return new Guid(new byte[16]
				{
					buffer[0],
					buffer[1],
					buffer[2],
					buffer[3],
					buffer[4],
					buffer[5],
					buffer[6],
					buffer[7],
					buffer[8],
					buffer[9],
					buffer[10],
					buffer[11],
					buffer[12],
					buffer[13],
					buffer[14],
					buffer[15]
				});
			}
			return new Guid(new byte[16]
			{
				buffer[3],
				buffer[2],
				buffer[1],
				buffer[0],
				buffer[5],
				buffer[4],
				buffer[7],
				buffer[6],
				buffer[8],
				buffer[9],
				buffer[10],
				buffer[11],
				buffer[12],
				buffer[13],
				buffer[14],
				buffer[15]
			});
		}

		public Sha1 ReadSha1()
		{
			FillBuffer(20);
			return new Sha1(buffer);
		}

		public int Read7BitEncodedInt()
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			while (true)
			{
				num3 = ReadByte();
				num |= (num3 & 0x7F) << num2;
				if (num3 >> 7 == 0)
				{
					break;
				}
				num2 += 7;
			}
			return num;
		}

		public long Read7BitEncodedLong()
		{
			long num = 0L;
			int num2 = 0;
			int num3 = 0;
			while (true)
			{
				num3 = ReadByte();
				num |= (num3 & 0x7F) << num2;
				if (num3 >> 7 == 0)
				{
					break;
				}
				num2 += 7;
			}
			return num;
		}

		public string ReadNullTerminatedString(bool reverse = false)
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (true)
			{
				char c = (char)ReadBytes(1).First();
				//char c = (char)ReadByte();
				if (c == '\0')
				{
					break;
				}
				stringBuilder.Append(c);

				if(stringBuilder.Length > 10000)
					throw new Exception("Can't find a text in this byte array");
			}

			return !reverse ? stringBuilder.ToString() : stringBuilder.ToString().Reverse().ToString();
		}

		public string ReadSizedString(int strLen)
		{
            //if (wideDecoder == Encoding.UTF8)
            //{
            //    StringBuilder stringBuilder = new StringBuilder();
            //    for (int i = 0; i < strLen * 2; i++)
            //    {
            //        char c = (char)ReadByte();
            //        if (c != 0)
            //        {
            //            stringBuilder.Append(c);
            //        }
            //    }
            //    return stringBuilder.ToString();
            //}
            //else
            //{
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < strLen; i++)
                {
                    char c = (char)ReadByte();
                    if (c != 0)
                    {
                        stringBuilder.Append(c);
                    }
                }
                return stringBuilder.ToString();
            //}

            //byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(strLen);
            //Span<byte> span = new Span<byte>(rentedBuffer, 0, strLen);
            //Span<byte> buffer = span;
            //try
            //{
            //	ReadIntoSpan(buffer);
            //	return wideDecoder.GetString(buffer);
            //}
            //finally
            //{
            //	if (rentedBuffer != null)
            //	{
            //		ArrayPool<byte>.Shared.Return(rentedBuffer);
            //	}
            //}

        }

		public string ReadLine()
		{
			StringBuilder stringBuilder = new StringBuilder();
			byte b = 0;
			while (b != 13 && b != 10)
			{
				b = ReadByte();
				stringBuilder.Append((char)b);
				if (b == 10 || b == 13 || Position >= Length)
				{
					break;
				}
			}
			if (b == 13)
			{
				ReadByte();
			}
			return stringBuilder.ToString().Trim('\r', '\n');
		}

		public void Pad(int alignment)
		{
			while (Position % alignment != 0L)
			{
				Position++;
			}
		}

		public string ReadLengthPrefixedString()
		{
			int byteCount = Read7BitEncodedInt();
			if (byteCount == 0)
			{
				return string.Empty;
			}
			return ReadSizedString(byteCount);
		}

		public byte[] ReadToEnd()
		{
			long num = Length - Position;
			if (num < int.MaxValue)
			{
				return ReadBytes((int)num);
			}
			byte[] array = new byte[num];
			while (num > 0)
			{
				int num2 = (int)((num > int.MaxValue) ? int.MaxValue : num);
				byte[] array2 = new byte[num2];
				int dstOffset = Read(array2, 0, num2);
				num -= num2;
				Buffer.BlockCopy(array2, 0, array, dstOffset, num2);
			}
			return array;
		}

		public byte[] ReadBytes(int count)
		{
			byte[] array = new byte[count];
			int num = 0;
			do
			{
				int num2 = Read(array, num, count);
				if (num2 == 0)
				{
					break;
				}
				num += num2;
				count -= num2;
			}
			while (count > 0);
			return array;
		}

		public virtual int Read(byte[] inBuffer, int offset, int numBytes)
		{
			int result = stream.Read(inBuffer, offset, numBytes);
			if (deobfuscator != null)
			{
				deobfuscator.Deobfuscate(inBuffer, Position, offset, numBytes);
			}
			return result;
		}

		private void ReadIntoSpan(Span<byte> buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (buffer.Length == 0)
			{
				return;
			}
			int bytesRead = 0;
			do
			{
				Span<byte> span = buffer.Slice(bytesRead);
				int read = stream.Read(span);
				if (read == 0)
				{
					throw new EndOfStreamException();
				}
				bytesRead += read;
			}
			while (bytesRead < buffer.Length);
		}

		public Stream CreateViewStream(long offset, long size)
		{
			Position = offset;
			var bytes = ReadBytes((int)size);
			return new MemoryStream(bytes);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		protected virtual void FillBuffer(int numBytes)
		{
			stream.Read(buffer, 0, numBytes);
			if (deobfuscator != null)
			{
				deobfuscator.Deobfuscate(buffer, Position, 0, numBytes);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stream stream = this.stream;
				this.stream = null;
				stream?.Close();
			}
			this.stream = null;
			buffer = null;
		}
	}
}
