using System.Drawing;
using System.IO;

namespace FifaLibrary
{
	public class DdsFile
	{
		private string m_Signature;

		private uint m_HeaderSize;

		private uint m_HeaderFlags;

		private int m_Width;

		private int m_Height;

		private int m_PitchOrLinearSize;

		private int m_Depth;

		private int m_MipMapCount;

		private int m_PixelFormatSize;

		private int m_PixelFormatFlag;

		private int m_FourCC;

		private EImageType m_ImageType;

		private int m_RGBBitCount;

		private int m_RBitMask;

		private int m_GBitMask;

		private int m_BBitMask;

		private int m_ABitMask;

		private int m_SurfaceFlags;

		private int m_CubemapFlags;

		private RawImage[] m_RawImages;

		public DdsFile()
		{
		}

		public DdsFile(string fileName)
		{
			Load(fileName);
		}

		public DdsFile(FifaFile fifaFile)
		{
			Load(fifaFile);
		}

		public DdsFile(BinaryReader r)
		{
			Load(r);
		}

		public bool Load(FifaFile fifaFile)
		{
			if (fifaFile.IsCompressed)
			{
				fifaFile.Decompress();
			}
			BinaryReader reader = fifaFile.GetReader();
			bool result = Load(reader);
			fifaFile.ReleaseReader(reader);
			return result;
		}

		public bool Load(string fileName)
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			bool result = Load(binaryReader);
			fileStream.Close();
			binaryReader.Close();
			return result;
		}

		public bool Load(BinaryReader r)
		{
			m_Signature = new string(r.ReadChars(4));
			if (m_Signature != "DDS ")
			{
				return false;
			}
			m_HeaderSize = r.ReadUInt32();
			m_HeaderFlags = r.ReadUInt32();
			m_Height = r.ReadInt32();
			m_Width = r.ReadInt32();
			m_PitchOrLinearSize = r.ReadInt32();
			m_Depth = r.ReadInt32();
			m_MipMapCount = r.ReadInt32();
			for (int i = 0; i < 11; i++)
			{
				r.ReadInt32();
			}
			m_PixelFormatSize = r.ReadInt32();
			m_PixelFormatFlag = r.ReadInt32();
			m_FourCC = r.ReadInt32();
			switch (m_FourCC)
			{
			case 827611204:
				m_ImageType = EImageType.DXT1;
				break;
			case 861165636:
				m_ImageType = EImageType.DXT3;
				break;
			case 894720068:
				m_ImageType = EImageType.DXT5;
				break;
			case 0:
				m_ImageType = EImageType.A8R8G8B8;
				break;
			}
			m_RGBBitCount = r.ReadInt32();
			m_RBitMask = r.ReadInt32();
			m_GBitMask = r.ReadInt32();
			m_BBitMask = r.ReadInt32();
			m_ABitMask = r.ReadInt32();
			m_SurfaceFlags = r.ReadInt32();
			m_CubemapFlags = r.ReadInt32();
			for (int j = 0; j < 3; j++)
			{
				r.ReadInt32();
			}
			if (m_MipMapCount > 0)
			{
				int num = m_Width / 4 * m_Height / 4 * 16;
				int num2 = m_Width;
				int num3 = m_Height;
				m_RawImages = new RawImage[m_MipMapCount + 1];
				for (int k = 0; k <= m_MipMapCount; k++)
				{
					m_RawImages[k] = new RawImage(num2, num3, m_ImageType, num);
					m_RawImages[k].Load(r);
					num /= 4;
					num2 /= 2;
					num3 /= 2;
				}
			}
			else
			{
				int size = (int)(r.BaseStream.Length - r.BaseStream.Position);
				m_RawImages = new RawImage[1];
				m_RawImages[0] = new RawImage(m_Width, m_Height, m_ImageType, size);
				m_RawImages[0].Load(r);
			}
			return true;
		}

		public bool Save(string fileName)
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			bool result = Save(binaryWriter);
			fileStream.Close();
			binaryWriter.Close();
			return result;
		}

		public bool Save(BinaryWriter w)
		{
			w.Write('D');
			w.Write('D');
			w.Write('S');
			w.Write(' ');
			w.Write(m_HeaderSize);
			w.Write(m_HeaderFlags);
			w.Write(m_Height);
			w.Write(m_Width);
			w.Write(m_PitchOrLinearSize);
			w.Write(m_Depth);
			w.Write(m_MipMapCount);
			for (int i = 0; i < 11; i++)
			{
				w.Write(0);
			}
			w.Write(m_PixelFormatSize);
			w.Write(m_PixelFormatFlag);
			w.Write(m_FourCC);
			w.Write(m_RGBBitCount);
			w.Write(m_RBitMask);
			w.Write(m_GBitMask);
			w.Write(m_BBitMask);
			w.Write(m_ABitMask);
			w.Write(m_SurfaceFlags);
			w.Write(m_CubemapFlags);
			for (int j = 0; j < 3; j++)
			{
				w.Write(0);
			}
			if (m_MipMapCount > 0)
			{
				for (int k = 0; k < m_MipMapCount + 1; k++)
				{
					m_RawImages[k].Save(w);
				}
			}
			else
			{
				m_RawImages[0].Save(w);
			}
			return true;
		}

		public Bitmap GetBitmap()
		{
			if (m_RawImages != null && m_RawImages.Length >= 1)
			{
				return m_RawImages[0].Bitmap;
			}
			return null;
		}

		public void ReplaceBitmap(Bitmap bitmap)
		{
			if (m_MipMapCount > 0)
			{
				Bitmap bitmap2 = bitmap;
				m_RawImages[0].Bitmap = bitmap;
				for (int i = 1; i < m_MipMapCount + 1; i++)
				{
					bitmap2 = GraphicUtil.ReduceBitmap(bitmap2);
					m_RawImages[i].Bitmap = bitmap2;
				}
			}
			else
			{
				m_RawImages[0].Bitmap = bitmap;
			}
		}
	}
}
