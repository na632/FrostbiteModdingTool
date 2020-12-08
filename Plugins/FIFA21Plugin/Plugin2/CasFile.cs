using FrostySdk.IO;
using System;
using System.IO;

namespace FIFA21Plugin.Plugin2
{
	public class CasFile
	{
		public static long MaxCasSize
		{
			get;
		} = 1073741824L;


		public static int CreateCasIdentifier(byte unk, bool isPatch, byte packageIndex, byte casIndex)
		{
			return (unk << 24) | ((isPatch ? 1 : 0) << 16) | (packageIndex << 8) | casIndex;
		}

		public static ushort GetDataAtOffset(string casFile, int offset)
		{
			if (casFile == null)
			{
				throw new ArgumentNullException("casFile");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "Offset must be a non-negative value.");
			}
			using (FileStream fs = new FileStream(casFile, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				if (fs.Length < offset + 4 + 2)
				{
					return ushort.MaxValue;
				}
				fs.Position = offset + 4;
				return new NativeReader(fs).ReadUShort();
			}
		}

		public static bool HasFileAtOffset(string casFile, long offset)
		{
			if (casFile == null)
			{
				throw new ArgumentNullException("casFile");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "Offset must be a non-negative value.");
			}
			using (FileStream fs = new FileStream(casFile, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				if (fs.Length < offset + 4 + 2)
				{
					return false;
				}
				fs.Position = offset + 4;
				NativeReader fileReader = new NativeReader(fs);
				byte compressionType = fileReader.ReadByte();
				byte num = fileReader.ReadByte();
				compressionType = (byte)(compressionType & 0x7Fu);
				return (num & 0x70) == 112 && (compressionType == 0 || compressionType == 2 || compressionType == 15 || compressionType == 9 || compressionType == 17 || compressionType == 21 || compressionType == 25);
			}
		}
	}
}
