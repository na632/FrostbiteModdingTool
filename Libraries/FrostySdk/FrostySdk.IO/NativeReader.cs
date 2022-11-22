using FrostbiteSdk;
using FrostySdk.Interfaces;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FrostySdk.IO
{
    public class NativeReader : FMT.FileTools.NativeReader, IDisposable
	{
		protected IDeobfuscator deobfuscator;

        public NativeReader(string filePath) : base(filePath)
        {
        }

        public NativeReader(Stream inStream) : base(inStream)
        {
        }

        public NativeReader(byte[] data) : base(data)
        {
        }

		//public NativeReader(string filePath) : base(filePath)
		//{

		//}

		//public NativeReader(Stream inStream) : base(inStream)
		//{

		//}

		public NativeReader(Stream inStream, IDeobfuscator inDeobfuscator) : this(inStream)
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

		//public NativeReader(Stream inStream, bool skipObfuscation, bool wide)
		//	: this(inStream)
		//{
		//	if (stream != null)
		//	{
		//		wideDecoder = (wide ? Encoding.UTF8 : Encoding.ASCII);
		//		if (skipObfuscation)
		//		{
		//			uint num = ReadUInt32LittleEndian();
		//			if (num != 30331136 && num != 63885568)
		//			{
		//				Position = 0L;
		//			}
		//			else
		//			{
		//				Position = 556L;
		//			}
		//		}
		//	}
		//}

		//      public NativeReader(byte[] data) : base(data)
		//      {
		//      }

		public short ReadShort(Endian inEndian = Endian.Little)
		{
			FillBuffer(2);
			if (inEndian == Endian.Little)
			{
				return (short)(buffer[0] | (buffer[1] << 8));
			}
			return (short)(buffer[1] | (buffer[0] << 8));
		}

		//public short ReadInt16LittleEndian()
		//{
		//	return ReadShort(Endian.Little);
		//}

		//public int ReadInt16BigEndian()
		//{
		//	return ReadShort(Endian.Big);
		//}

		public ushort ReadUShort(Endian inEndian = Endian.Little)
		{
			FillBuffer(2);
			if (inEndian == Endian.Little)
			{
				return (ushort)(buffer[0] | (buffer[1] << 8));
			}
			return (ushort)(buffer[1] | (buffer[0] << 8));
		}

		//public ushort ReadUInt16LittleEndian()
		//{
		//	return ReadUShort(Endian.Little);
		//}

		//public ushort ReadUInt16BigEndian()
		//{
		//	return ReadUShort(Endian.Big);
		//}


		public int ReadInt(Endian inEndian = Endian.Little)
		{
            FillBuffer(4);
            if (inEndian == Endian.Little)
            {
				return BitConverter.ToInt32(buffer.Take(4).ToArray());
			}
			return BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(buffer.Take(4).ToArray()));

		}

		//public int ReadInt32LittleEndian()
  //      {
		//	return ReadInt(Endian.Little);
  //      }

		//public int ReadInt32BigEndian()
		//{
		//	return ReadInt(Endian.Big);
		//}

		public uint ReadUInt(Endian inEndian = Endian.Little)
		{
            FillBuffer(4);
            if (inEndian == Endian.Little)
            {
				return BitConverter.ToUInt32(buffer.Take(4).ToArray());
            }
			return BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(buffer.Take(4).ToArray()));
		}

		//public uint ReadUInt32LittleEndian()
		//{
		//	return ReadUInt(Endian.Little);
		//}

		//public uint ReadUInt32BigEndian()
		//{
		//	return ReadUInt(Endian.Big);
		//}


		public long ReadLong(Endian inEndian = Endian.Little)
		{
			FillBuffer(8);
			if(inEndian == Endian.Little)
				return BitConverter.ToInt64(buffer.Take(8).ToArray());
			return BinaryPrimitives.ReverseEndianness(BitConverter.ToInt64(buffer.Take(8).ToArray()));
		}

		//public long ReadInt64LittleEndian()
		//{
		//	return ReadLong(Endian.Little);
		//}

		//public long ReadInt64BigEndian()
		//{
		//	return ReadLong(Endian.Big);
		//}

		public ulong ReadULong(Endian inEndian = Endian.Little)
		{
			FillBuffer(8);
			if (inEndian == Endian.Little)
				return BitConverter.ToUInt64(buffer.Take(8).ToArray());
			return BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt64(buffer.Take(8).ToArray()));

		}

		//public ulong ReadUInt64LittleEndian()
		//{
		//	return ReadULong(Endian.Little);
		//}

		//public ulong ReadUInt64BigEndian()
		//{
		//	return ReadULong(Endian.Big);
		//}

		public unsafe float ReadFloat(Endian inEndian = Endian.Little)
		{
			FillBuffer(4);

			var int32bytes = BitConverter.ToInt32(buffer.Take(4).ToArray());
			var reInt32Bytes = BinaryPrimitives.ReverseEndianness(int32bytes);
			if (inEndian == Endian.Little)
			{
				return BitConverter.Int32BitsToSingle(int32bytes);
			}

			return BitConverter.Int32BitsToSingle(reInt32Bytes);

		}

		//public unsafe float ReadSingleLittleEndian()
		//{
		//	return ReadFloat();
		//}

		//public unsafe float ReadSingleBigEndian()
		//{
		//	return ReadFloat(Endian.Big);
		//}

		public unsafe double ReadDouble(Endian inEndian = Endian.Little)
		{
			FillBuffer(8);

			var int64bytes = BitConverter.ToInt64(buffer.Take(8).ToArray());
			var reInt64Bytes = BinaryPrimitives.ReverseEndianness(int64bytes);
			if (inEndian == Endian.Little)
			{
				return BitConverter.Int64BitsToDouble(int64bytes);
			}

			return BitConverter.Int64BitsToDouble(reInt64Bytes);
		}

		//public unsafe double ReadDoubleLittleEndian()
		//{
		//	return ReadDouble(Endian.Little);
		//}

		//public unsafe double ReadDoubleBigEndian()
		//{
		//	return ReadDouble(Endian.Big);
		//}

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


		//public Guid ReadGuidReverse()
  //      {
		//	byte[] array6 = ReadBytes(16);
		//	Guid guid = new Guid(new byte[16]
		//	{
		//		array6[15],
		//		array6[14],
		//		array6[13],
		//		array6[12],
		//		array6[11],
		//		array6[10],
		//		array6[9],
		//		array6[8],
		//		array6[7],
		//		array6[6],
		//		array6[5],
		//		array6[4],
		//		array6[3],
		//		array6[2],
		//		array6[1],
		//		array6[0]
		//	});
		//	return guid;
		//}
		public Sha1 ReadSha1()
		{
			FillBuffer(20);
			return new Sha1(buffer);
		}

		//public int Read7BitEncodedInt()
		//{
		//	int num = 0;
		//	int num2 = 0;
		//	int num3 = 0;
		//	while (true)
		//	{
		//		num3 = ReadByte();
		//		num |= (num3 & 0x7F) << num2;
		//		if (num3 >> 7 == 0)
		//		{
		//			break;
		//		}
		//		num2 += 7;
		//	}
		//	return num;
		//}

		//public long Read7BitEncodedLong()
		//{
		//	long num = 0L;
		//	int num2 = 0;
		//	int num3 = 0;
		//	while (true)
		//	{
		//		num3 = ReadByte();
		//		num |= (num3 & 0x7F) << num2;
		//		if (num3 >> 7 == 0)
		//		{
		//			break;
		//		}
		//		num2 += 7;
		//	}
		//	return num;
		//}

		//public string ReadNullTerminatedString(bool reverse = false)
		//{
		//	var startPosition = Position;
		//	var size = 0;
		//	StringBuilder stringBuilder = new StringBuilder();
		//	while (true)
		//	{
		//		char c = (char)ReadBytes(1).First();
		//		//char c = (char)ReadByte();
		//		if (c == '\0')
		//		{
		//			break;
		//		}
		//		stringBuilder.Append(c);

		//		if(stringBuilder.Length > 10000)
		//			throw new Exception("Can't find a text in this byte array");
		//	}

		//	return !reverse ? stringBuilder.ToString() : stringBuilder.ToString().Reverse().ToString();
		//}

		//public string ReadSizedString(int strLen)
		//{
		//	//byte[] bytes = ReadBytes(strLen);
		//	//return Encoding.UTF8.GetString(bytes);

		//	//if (wideDecoder == Encoding.UTF8)
		//	//{
		//	//    StringBuilder stringBuilder = new StringBuilder();
		//	//    for (int i = 0; i < strLen * 2; i++)
		//	//    {
		//	//        char c = (char)ReadByte();
		//	//        if (c != 0)
		//	//        {
		//	//            stringBuilder.Append(c);
		//	//        }
		//	//    }
		//	//    return stringBuilder.ToString();
		//	//}
		//	//else
		//	//{
		//	StringBuilder stringBuilder = new StringBuilder();
		//	for (int i = 0; i < strLen; i++)
		//	{
		//		char c = (char)ReadByte();
		//		if (c != 0)
		//		{
		//			stringBuilder.Append(c);
		//		}
		//	}
		//	return stringBuilder.ToString();
		//	//}

		//	//byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(strLen);
		//	//Span<byte> span = new Span<byte>(rentedBuffer, 0, strLen);
		//	//Span<byte> buffer = span;
		//	//try
		//	//{
		//	//	ReadIntoSpan(buffer);
		//	//	return wideDecoder.GetString(buffer);
		//	//}
		//	//finally
		//	//{
		//	//	if (rentedBuffer != null)
		//	//	{
		//	//		ArrayPool<byte>.Shared.Return(rentedBuffer);
		//	//	}
		//	//}

		//}

		//public string ReadLine()
		//{
		//	StringBuilder stringBuilder = new StringBuilder();
		//	byte b = 0;
		//	while (b != 13 && b != 10)
		//	{
		//		b = ReadByte();
		//		stringBuilder.Append((char)b);
		//		if (b == 10 || b == 13 || Position >= Length)
		//		{
		//			break;
		//		}
		//	}
		//	if (b == 13)
		//	{
		//		ReadByte();
		//	}
		//	return stringBuilder.ToString().Trim('\r', '\n');
		//}

		//public void Pad(int alignment)
		//{
		//	if (alignment == 0)
		//		return;

		//	while (Position % alignment != 0L)
		//	{
		//		Position++;
		//	}
		//}

		//public string ReadLengthPrefixedString()
		//{
		//	int byteCount = Read7BitEncodedInt();
		//	if (byteCount == 0)
		//	{
		//		//return string.Empty;
		//		return null;
		//	}
		//	return ReadSizedString(byteCount);
		//}

		//public byte[] ReadToEnd()
		//{
		//	long num = Length - Position;
		//	if (num < int.MaxValue)
		//	{
		//		return ReadBytes((int)num);
		//	}
		//	byte[] array = new byte[num];
		//	while (num > 0)
		//	{
		//		int num2 = (int)((num > int.MaxValue) ? int.MaxValue : num);
		//		byte[] array2 = new byte[num2];
		//		int dstOffset = Read(array2, 0, num2);
		//		num -= num2;
		//		Buffer.BlockCopy(array2, 0, array, dstOffset, num2);
		//	}
		//	return array;
		//}

		//public byte[] ReadAll()
  //      {
		//	Position = 0;
		//	return ReadToEnd();
  //      }

		//public byte[] ReadBytes(int count)
		//{
		//	byte[] array = new byte[count];
		//	int num = 0;
		//	do
		//	{
		//		int num2 = Read(array, num, count);
		//		if (num2 == 0)
		//		{
		//			break;
		//		}
		//		num += num2;
		//		count -= num2;
		//	}
		//	while (count > 0);
		//	return array;
		//}

		//public virtual int Read(byte[] inBuffer, int offset, int numBytes)
		//{
		//	int result = stream.Read(inBuffer, offset, numBytes);
		//	if (deobfuscator != null)
		//	{
		//		deobfuscator.Deobfuscate(inBuffer, Position, offset, numBytes);
		//	}
		//	return result;
		//}

		//private void ReadIntoSpan(Span<byte> buffer)
		//{
		//	if (buffer == null)
		//	{
		//		throw new ArgumentNullException("buffer");
		//	}
		//	if (buffer.Length == 0)
		//	{
		//		return;
		//	}
		//	int bytesRead = 0;
		//	do
		//	{
		//		Span<byte> span = buffer.Slice(bytesRead);
		//		int read = stream.Read(span);
		//		if (read == 0)
		//		{
		//			throw new EndOfStreamException();
		//		}
		//		bytesRead += read;
		//	}
		//	while (bytesRead < buffer.Length);
		//}

		/// <summary>
		/// Slow performance but supports Wildcards (i.e. ??)
		/// </summary>
		/// <param name="pattern"></param>
		/// <returns></returns>
        public unsafe IList<long> ScanAOB(string pattern)
        {
			if (pattern.StartsWith("?"))
				throw new ArgumentException("pattern cannot start with a wildcard!");

            List<long> offsets = new List<long>();
            pattern = pattern.Replace(" ", "");
            PatternType[] patternArray = new PatternType[pattern.Length / 2];
            for (int i = 0; i < patternArray.Length; i++)
            {
                string a = pattern.Substring(i * 2, 2);
                patternArray[i] = new PatternType
                {
                    isWildcard = (a == "??"),
                    value = (byte)((a != "??") ? byte.Parse(pattern.Substring(i * 2, 2), NumberStyles.HexNumber) : 0)
                };
            }
            bool flag = false;
            long startPosition = Position;
			do
			{
				var positionOfOffset = Position;
				byte[] readBytes = ReadBytes(patternArray.Length);
				bool match = false;
				for (var i = 0; i < readBytes.Length; i++)
				{
					if (!patternArray[i].isWildcard && readBytes[i] == patternArray[i].value)
					{
						match = true;
					}
					else if (!patternArray[i].isWildcard)
					{
						match = false;
						break;
					}

				}
				if (match)
					offsets.Add(positionOfOffset);

				Position -= patternArray.Length;
				Position += 1;
			} while (Position + patternArray.Length < Length);
            return offsets;
        }

		/// <summary>
		/// Fast performance but does not support strings or wildcards, bytes only
		/// </summary>
		/// <param name="haystack"></param>
		/// <param name="needle"></param>
		/// <param name="startIndex"></param>
		/// <param name="includeOverlapping"></param>
		/// <returns></returns>
        public IEnumerable<int> ScanAOB2(byte[] needle,
    int startIndex = 0, bool includeOverlapping = false)
        {
			Position = 0;
			var haystack = ReadBytes((int)Length);
			int matchIndex = haystack.AsSpan(startIndex).IndexOf(needle);
			while (matchIndex >= 0)
			{
				yield return startIndex + matchIndex;
				startIndex += matchIndex + (includeOverlapping ? 1 : needle.Length);
				matchIndex = haystack.AsSpan(startIndex).IndexOf(needle);
			}
        }

  //      public Stream CreateViewStream(long offset, long size)
		//{
		//	Position = offset;
		//	var bytes = ReadBytes((int)size);
		//	return new MemoryStream(bytes);
		//}

		//public void Dispose()
		//{
		//	Dispose(disposing: true);
		//}

		//protected virtual void FillBuffer(int numBytes)
		//{
		//	if (stream == null || buffer == null)
		//	{
		//		throw new Exception("Cannot fill Buffer!");
		//	}

		//	stream.Read(buffer, 0, numBytes);
		//	if (deobfuscator != null)
		//	{
		//		deobfuscator.Deobfuscate(buffer, Position, 0, numBytes);
		//	}
		//}

		//protected virtual void Dispose(bool disposing)
		//{
		//	if (disposing)
		//	{
		//		Stream stream = this.stream;
		//		this.stream = null;
		//		stream?.Close();
		//	}
		//	this.stream = null;
		//	buffer = null;
		//}
	}


    public class FileReader : NativeReader
    {
        public FileReader(Stream inStream) : base(inStream)
        {
        }
    }
}
