using System.Drawing;
using System.IO;

namespace FifaLibrary
{
	public class Rx3Image
	{
		private Rx3ImageHeader m_Header;

		private MipMap[] m_MipMapImage;

		private bool m_SwapEndian;

		public Rx3ImageHeader Header
		{
			get
			{
				return m_Header;
			}
			set
			{
				m_Header = value;
			}
		}

		public Bitmap GetBitmap()
		{
			return m_MipMapImage[0].Bitmap;
		}

		public Bitmap GetBitmap(int mipMapLevel)
		{
			if (mipMapLevel >= m_MipMapImage.Length)
			{
				return null;
			}
			return m_MipMapImage[mipMapLevel].Bitmap;
		}

		public bool SetBitmap(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			if (bitmap.Width != m_Header.Width || bitmap.Width != m_Header.Width)
			{
				return false;
			}
			Bitmap bitmap2 = bitmap;
			m_MipMapImage[0].Bitmap = bitmap2;
			for (int i = 1; i < m_Header.NMipMaps; i++)
			{
				bitmap2 = GraphicUtil.ReduceBitmap(bitmap2);
				m_MipMapImage[i].Bitmap = bitmap2;
			}
			return true;
		}

		public Rx3Image(BinaryReader r, bool swapEndian)
		{
			m_SwapEndian = swapEndian;
			m_Header = new Rx3ImageHeader(r, m_SwapEndian);
			Load(r);
		}

		public bool Load(BinaryReader r)
		{
			m_MipMapImage = new MipMap[m_Header.NMipMaps];
			int num = m_Header.Width;
			int num2 = m_Header.Height;
			for (int i = 0; i < m_Header.NMipMaps; i++)
			{
				m_MipMapImage[i] = new MipMap(num, num2, m_Header.ImageType, m_SwapEndian);
				m_MipMapImage[i].Load(r);
				num /= 2;
				num2 /= 2;
			}
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			m_Header.Save(w);
			for (int i = 0; i < m_Header.NMipMaps; i++)
			{
				m_MipMapImage[i].Save(w);
			}
			return true;
		}
	}
}
