using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.IO;

namespace FrostySdk.Resources
{
	public class Texture : IDisposable
	{
		private uint[] mipOffsets = new uint[2];

		private TextureType type;

		private int pixelFormat;

		private uint unknown1;

		private TextureFlags flags;

		private ushort width;

		private ushort height;

		private ushort depth;

		private ushort sliceCount;

		private byte mipCount;

		private byte firstMip;

		private Guid chunkId;

		private uint[] mipSizes = new uint[15];

		private uint chunkSize;

		private uint[] unknown3 = new uint[4];

		private uint assetNameHash;

		private string textureGroup;

		private Stream data;

		private uint logicalOffset;

		private uint logicalSize;

		private uint rangeStart;

		private uint rangeEnd;

		public uint FirstMipOffset
		{
			get
			{
				return mipOffsets[0];
			}
			set
			{
				mipOffsets[0] = value;
			}
		}

		public uint SecondMipOffset
		{
			get
			{
				return mipOffsets[1];
			}
			set
			{
				mipOffsets[1] = value;
			}
		}

		public string PixelFormat
		{
			get
			{
				string text = "RenderFormat";
				return Enum.Parse(TypeLibrary.GetType(text), pixelFormat.ToString()).ToString().Replace(text + "_", "");
			}
		}

		public TextureType Type => type;

		public TextureFlags Flags
		{
			get
			{
				return flags;
			}
			set
			{
				flags = value;
			}
		}

		public ushort Width => width;

		public ushort Height => height;

		public ushort SliceCount
		{
			get
			{
				return sliceCount;
			}
			set
			{
				sliceCount = value;
				if (type == TextureType.TT_2dArray || type == TextureType.TT_3d)
				{
					depth = sliceCount;
				}
			}
		}

		public ushort Depth => depth;

		public byte MipCount => mipCount;

		public byte FirstMip
		{
			get
			{
				return firstMip;
			}
			set
			{
				firstMip = value;
			}
		}

		public uint[] MipSizes => mipSizes;

		public string TextureGroup
		{
			get
			{
				return textureGroup;
			}
			set
			{
				textureGroup = value;
			}
		}

		public uint AssetNameHash
		{
			get
			{
				return assetNameHash;
			}
			set
			{
				assetNameHash = value;
			}
		}

		public Stream Data => data;

		public uint LogicalOffset
		{
			get
			{
				return logicalOffset;
			}
			set
			{
				logicalOffset = value;
			}
		}

		public uint LogicalSize
		{
			get
			{
				return logicalSize;
			}
			set
			{
				logicalSize = value;
			}
		}

		public uint RangeStart
		{
			get
			{
				return rangeStart;
			}
			set
			{
				rangeStart = value;
			}
		}

		public uint RangeEnd
		{
			get
			{
				return rangeEnd;
			}
			set
			{
				rangeEnd = value;
			}
		}

		public uint[] Unknown3 => unknown3;

		public Guid ChunkId
		{
			get
			{
				return chunkId;
			}
			set
			{
				chunkId = value;
			}
		}

		public uint ChunkSize => chunkSize;

		public Texture(Stream stream, AssetManager am)
		{
			using (NativeReader nativeReader = new NativeReader(stream))
			{
				if (ProfilesLibrary.DataVersion == 20131115)
				{
					unknown3[0] = nativeReader.ReadUInt();
					type = (TextureType)nativeReader.ReadUInt();
					pixelFormat = nativeReader.ReadInt();
					unknown3[1] = nativeReader.ReadUInt();
				}
				else
				{
					mipOffsets[0] = nativeReader.ReadUInt();
					mipOffsets[1] = nativeReader.ReadUInt();
					type = (TextureType)nativeReader.ReadUInt();
					pixelFormat = nativeReader.ReadInt();
					if (ProfilesLibrary.DataVersion == 20170321 || ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20171110 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20180628 || ProfilesLibrary.DataVersion == 20190729 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905)
					{
						unknown1 = nativeReader.ReadUInt();
					}
					flags = (TextureFlags)nativeReader.ReadUShort();
				}
				width = nativeReader.ReadUShort();
				height = nativeReader.ReadUShort();
				depth = nativeReader.ReadUShort();
				sliceCount = nativeReader.ReadUShort();
				if (ProfilesLibrary.DataVersion == 20131115)
				{
					flags = (TextureFlags)nativeReader.ReadUShort();
				}
				mipCount = nativeReader.ReadByte();
				firstMip = nativeReader.ReadByte();
				chunkId = nativeReader.ReadGuid();
				for (int i = 0; i < 15; i++)
				{
					mipSizes[i] = nativeReader.ReadUInt();
				}
				chunkSize = nativeReader.ReadUInt();
				if (ProfilesLibrary.DataVersion == 20181207)
				{
					for (int j = 0; j < 3; j++)
					{
						unknown3[j] = nativeReader.ReadUInt();
					}
				}
				else if (ProfilesLibrary.DataVersion == 20170321)
				{
					for (int k = 0; k < 4; k++)
					{
						unknown3[k] = nativeReader.ReadUInt();
					}
				}
				else if (ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807)
				{
					unknown3[0] = nativeReader.ReadUInt();
				}
				assetNameHash = nativeReader.ReadUInt();
				if (ProfilesLibrary.DataVersion == 20150223)
				{
					unknown3[0] = nativeReader.ReadUInt();
				}
				textureGroup = nativeReader.ReadSizedString(16);
				if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
				{
					unknown3[0] = nativeReader.ReadUInt();
				}
				data = am.GetChunk(am.GetChunkEntry(chunkId));
			}
		}

		public byte[] ToBytes()
		{
			MemoryStream memoryStream = new MemoryStream();
			using (NativeWriter nativeWriter = new NativeWriter(memoryStream))
			{
				nativeWriter.Write(mipOffsets[0]);
				nativeWriter.Write(mipOffsets[1]);
				nativeWriter.Write((uint)type);
				nativeWriter.Write(pixelFormat);
				if (ProfilesLibrary.DataVersion == 20170321 || ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20171110 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20180628 || ProfilesLibrary.DataVersion == 20190729 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905)
				{
					nativeWriter.Write(unknown1);
				}
				nativeWriter.Write((ushort)flags);
				nativeWriter.Write(width);
				nativeWriter.Write(height);
				nativeWriter.Write(depth);
				nativeWriter.Write(sliceCount);
				nativeWriter.Write(mipCount);
				nativeWriter.Write(firstMip);
				nativeWriter.Write(chunkId);
				for (int i = 0; i < 15; i++)
				{
					nativeWriter.Write(mipSizes[i]);
				}
				nativeWriter.Write(chunkSize);
				if (ProfilesLibrary.DataVersion == 20170321)
				{
					for (int j = 0; j < 4; j++)
					{
						nativeWriter.Write(unknown3[j]);
					}
				}
				if (ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807)
				{
					nativeWriter.Write(unknown3[0]);
				}
				nativeWriter.Write(assetNameHash);
				if (ProfilesLibrary.DataVersion == 20150223)
				{
					nativeWriter.Write(unknown3[0]);
				}
				nativeWriter.WriteFixedSizedString(textureGroup, 16);
				if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
				{
					nativeWriter.Write(unknown3[0]);
				}
			}
			return memoryStream.ToArray();
		}

		public Texture(TextureType inType, string inFormat, ushort inWidth, ushort inHeight, ushort inDepth = 1)
		{
			type = inType;
			pixelFormat = getTextureFormat(inFormat);
			width = inWidth;
			height = inHeight;
			depth = inDepth;
			sliceCount = inDepth;
			unknown1 = 0u;
			flags = (TextureFlags)0;
			unknown3[0] = uint.MaxValue;
			unknown3[1] = uint.MaxValue;
			unknown3[2] = uint.MaxValue;
			unknown3[3] = uint.MaxValue;
		}

		~Texture()
		{
			Dispose(disposing: false);
		}

		public void SetData(Guid newChunkId, AssetManager am)
		{
			data = am.GetChunk(am.GetChunkEntry(newChunkId));
			chunkId = newChunkId;
			chunkSize = (uint)data.Length;
		}

		public void SetData(byte[] inData)
		{
			data = new MemoryStream(inData);
			chunkId = Guid.Empty;
			chunkSize = (uint)data.Length;
		}

		public void CalculateMipData(byte inMipCount, int blockSize, bool isCompressed, uint dataSize)
		{
			if (isCompressed)
			{
				blockSize /= 4;
			}
			mipCount = inMipCount;
			int num = width;
			int num2 = height;
			int num3 = depth;
			int num4 = (!isCompressed) ? 1 : 4;
			for (int i = 0; i < mipCount; i++)
			{
				int num5 = isCompressed ? (Math.Max(1, (num + 3) / 4) * blockSize) : ((num * blockSize + 7) / 8);
				mipSizes[i] = (uint)(num5 * num2);
				if (type == TextureType.TT_3d)
				{
					mipSizes[i] *= (uint)num3;
				}
				num >>= 1;
				num2 >>= 1;
				num2 = ((num2 < num4) ? num4 : num2);
				num = ((num < num4) ? num4 : num);
			}
			if (mipCount == 1)
			{
				logicalOffset = 0u;
				logicalSize = dataSize;
				flags = (TextureFlags)0;
				return;
			}
			logicalOffset = 0u;
			for (int j = 0; j < mipCount - firstMip; j++)
			{
				logicalSize |= (uint)(3 << j * 2);
			}
			logicalOffset = (dataSize & ~logicalSize);
			logicalSize = (dataSize & logicalSize);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				data.Dispose();
			}
		}

		private int getTextureFormat(string format)
		{
			string text = "RenderFormat";
			return (int)Enum.Parse(TypeLibrary.GetType(text), text + "_" + format);
		}
	}
}
