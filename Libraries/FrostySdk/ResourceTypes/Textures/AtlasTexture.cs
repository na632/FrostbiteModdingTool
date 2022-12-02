using FMT.FileTools;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.IO;
using System.Text;

namespace FrostySdk.Resources
{
	public class AtlasTexture
	{
		private ushort atlasType;

		private ushort width;

		private ushort height;

		private ushort unknown2;

		private float unknown3;

		private float unknown4;

		private Guid chunkId;

		private Stream data;

		private uint[] mipSizes = new uint[15];

		public ushort Width => width;

		public ushort Height => height;

		public Stream Data => data;

		public Guid ChunkId => chunkId;

		public int MipCount
		{
			get
			{
				if (ProfileManager.DataVersion != 20171117)
				{
					return 1;
				}
				int num = 0;
				for (int i = 0; i < mipSizes.Length; i++)
				{
					if (mipSizes[i] != 0)
					{
						num++;
					}
				}
				return num;
			}
		}

		public AtlasTexture(Stream stream, AssetManager am)
		{
			using (NativeReader nativeReader = new NativeReader(stream))
			{
				atlasType = nativeReader.ReadUShort();
				width = nativeReader.ReadUShort();
				height = nativeReader.ReadUShort();
				unknown2 = nativeReader.ReadUShort();
				unknown3 = nativeReader.ReadFloat();
				unknown4 = nativeReader.ReadFloat();
				chunkId = nativeReader.ReadGuid();
				if (ProfileManager.DataVersion == 20171117)
				{
					for (int i = 0; i < 15; i++)
					{
						mipSizes[i] = nativeReader.ReadUInt();
					}
				}
				data = am.GetChunk(am.GetChunkEntry(chunkId));
			}
		}

		public AtlasTexture(AtlasTexture other)
		{
			atlasType = other.atlasType;
			unknown2 = other.unknown2;
			unknown3 = other.unknown3;
			unknown4 = other.unknown4;
			chunkId = other.chunkId;
			data = other.data;
		}

		public void SetData(int w, int h, Guid newChunkId, AssetManager am)
		{
			width = (ushort)w;
			height = (ushort)h;
			chunkId = newChunkId;
			data = am.GetChunk(am.GetChunkEntry(chunkId));
			if (ProfileManager.DataVersion != 20171117)
			{
				return;
			}
			uint num = (uint)data.Length;
			int num2 = 4;
			for (int i = 0; i < 15; i++)
			{
				if (num != 0)
				{
					w = Math.Max(1, w);
					h = Math.Max(1, h);
					uint num3 = (uint)(Math.Max(1, (w + 3) / 4) * num2 * h);
					mipSizes[i] = num3;
					num -= num3;
					w >>= 1;
					h >>= 1;
				}
			}
		}

		public byte[] ToBytes()
		{
			using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
			{
				nativeWriter.Write(atlasType);
				nativeWriter.Write(width);
				nativeWriter.Write(height);
				nativeWriter.Write(unknown2);
				nativeWriter.Write(unknown3);
				nativeWriter.Write(unknown4);
				nativeWriter.Write(chunkId);
				if (ProfileManager.DataVersion == 20171117)
				{
					for (int i = 0; i < 15; i++)
					{
						nativeWriter.Write(mipSizes[i]);
					}
				}
				return ((MemoryStream)nativeWriter.BaseStream).ToArray();
			}
		}

		public string ToDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Type: " + atlasType);
			stringBuilder.AppendLine("Width: " + width);
			stringBuilder.AppendLine("Height: " + height);
			stringBuilder.AppendLine("Unknown: " + unknown2);
			stringBuilder.AppendLine("Unknown: " + unknown3);
			stringBuilder.AppendLine("Unknown: " + unknown4);
			stringBuilder.AppendLine("ChunkId: " + chunkId);
			return stringBuilder.ToString();
		}
	}
}
