using System;
using System.IO;

namespace FrostySdk.Resources
{
	internal class BitReader : IDisposable
	{
		private Stream stream;

		private byte[] buffer = new byte[4];

		private int value;

		private int shift;

		private bool atEnd;

		public bool EndOfStream => atEnd;

		public BitReader(Stream inStream)
		{
			stream = inStream;
		}

		~BitReader()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		public bool GetBit()
		{
			if (shift >= 32)
			{
				if (stream.Position >= stream.Length)
				{
					atEnd = true;
					return false;
				}
				shift = 0;
				FillBuffer();
			}
			return ((value >> shift++) & 1) == 1;
		}

		public void SetPosition(int pos)
		{
			stream.Position = (pos >> 5) * 4;
			shift = (pos & 0x1F);
			FillBuffer();
		}

		private void FillBuffer()
		{
			stream.Read(buffer, 0, 4);
			value = BitConverter.ToInt32(buffer, 0);
		}

		private void Dispose(bool disposing = false)
		{
			if (disposing)
			{
				stream.Dispose();
				stream = null;
				GC.SuppressFinalize(this);
			}
		}
	}
}
