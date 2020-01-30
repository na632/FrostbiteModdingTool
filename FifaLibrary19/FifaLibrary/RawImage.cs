using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace FifaLibrary
{
	public class RawImage
	{
		private int m_Width;

		private int m_Height;

		private EImageType m_ImageType;

		protected bool m_SwapEndian;

		private Bitmap m_Bitmap;

		private bool m_NeedToSaveRawData;

		private byte[] m_RawData;

		protected int m_Size;

		public Bitmap Bitmap
		{
			get
			{
				if (m_Bitmap == null)
				{
					CreateBitmap();
				}
				return m_Bitmap;
			}
			set
			{
				m_Bitmap = value;
				m_NeedToSaveRawData = true;
			}
		}

		public RawImage(int width, int height, EImageType dxtType, int size)
		{
			m_Width = width;
			m_Height = height;
			m_ImageType = dxtType;
			m_Size = size;
			m_Bitmap = null;
		}

		public bool Load(BinaryReader r)
		{
			int count = m_Size;
			if (m_ImageType == EImageType.A8R8G8B8)
			{
				count = m_Width * m_Height * 4;
			}
			m_RawData = r.ReadBytes(count);
			m_NeedToSaveRawData = false;
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			if (m_NeedToSaveRawData)
			{
				CreateRawData();
				m_NeedToSaveRawData = false;
			}
			w.Write(m_RawData);
			return true;
		}

		private void CreateBitmap()
		{
			if (m_Width < 1)
			{
				m_Width = 1;
			}
			if (m_Height < 1)
			{
				m_Height = 1;
			}
			switch (m_ImageType)
			{
			case (EImageType)6:
				break;
			case EImageType.DXT1:
			case EImageType.DXT3:
			case EImageType.DXT5:
				m_Bitmap = new Bitmap(m_Width, m_Height, PixelFormat.Format32bppArgb);
				ReadDxtToBitmap();
				break;
			case EImageType.A8R8G8B8:
			{
				m_Bitmap = new Bitmap(m_Width, m_Height, PixelFormat.Format32bppArgb);
				Rectangle rect = new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height);
				BitmapData bitmapData = m_Bitmap.LockBits(rect, ImageLockMode.WriteOnly, m_Bitmap.PixelFormat);
				IntPtr scan = bitmapData.Scan0;
				int num3 = m_Bitmap.Width * m_Bitmap.Height;
				Marshal.Copy(m_RawData, 0, scan, num3 * 4);
				m_Bitmap.UnlockBits(bitmapData);
				break;
			}
			case EImageType.GREY8:
			{
				m_Bitmap = new Bitmap(m_Width, m_Height, PixelFormat.Format32bppArgb);
				int num = 0;
				for (int k = 0; k < m_Bitmap.Height; k++)
				{
					for (int l = 0; l < m_Bitmap.Width; l++)
					{
						byte num4 = m_RawData[num++];
						int alpha2 = 255;
						int red2 = num4;
						int green2 = num4;
						int blue2 = num4;
						m_Bitmap.SetPixel(l, k, Color.FromArgb(alpha2, red2, green2, blue2));
					}
				}
				break;
			}
			case EImageType.GREY8ALFA8:
			{
				m_Bitmap = new Bitmap(m_Width, m_Height, PixelFormat.Format32bppArgb);
				int num = 0;
				for (int i = 0; i < m_Bitmap.Height; i++)
				{
					for (int j = 0; j < m_Bitmap.Width; j++)
					{
						byte num2 = m_RawData[num++];
						int alpha = m_RawData[num++];
						int red = num2;
						int green = num2;
						int blue = num2;
						m_Bitmap.SetPixel(j, i, Color.FromArgb(alpha, red, green, blue));
					}
				}
				break;
			}
			case EImageType.DC_XY_NORMAL_MAP:
				m_Bitmap = new Bitmap(m_Width, m_Height, PixelFormat.Format32bppArgb);
				ReadDxtToBitmap();
				break;
			}
		}

		private void CreateRawData()
		{
			if (m_Width < 1)
			{
				m_Width = 1;
			}
			if (m_Height < 1)
			{
				m_Height = 1;
			}
			switch (m_ImageType)
			{
			case (EImageType)6:
				break;
			case EImageType.DXT1:
			case EImageType.DXT3:
			case EImageType.DXT5:
			case EImageType.DC_XY_NORMAL_MAP:
				WriteBitmapToDxt();
				break;
			case EImageType.A8R8G8B8:
				WriteBitmapToA8R8G8B8();
				break;
			case EImageType.GREY8:
				WriteBitmapToGrey8();
				break;
			case EImageType.GREY8ALFA8:
				WriteBitmapToGrey8Alfa8();
				break;
			}
		}

		private void ReadDxtToBitmap()
		{
			DxtBlock dxtBlock = new DxtBlock((int)m_ImageType);
			MemoryStream memoryStream = new MemoryStream(m_RawData);
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			Rectangle rect = new Rectangle(0, 0, m_Width, m_Height);
			BitmapData bitmapData = m_Bitmap.LockBits(rect, ImageLockMode.WriteOnly, m_Bitmap.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			int num = m_Bitmap.Width * m_Bitmap.Height;
			int[] array = new int[num];
			for (int i = 0; i < m_Bitmap.Height / 4; i++)
			{
				for (int j = 0; j < m_Bitmap.Width / 4; j++)
				{
					dxtBlock.Load(binaryReader);
					int num2 = i * 4 * m_Bitmap.Width;
					num2 += j * 4;
					array[num2] = dxtBlock.Colors[0, 0].ToArgb();
					array[num2 + 1] = dxtBlock.Colors[1, 0].ToArgb();
					array[num2 + 2] = dxtBlock.Colors[2, 0].ToArgb();
					array[num2 + 3] = dxtBlock.Colors[3, 0].ToArgb();
					num2 += m_Bitmap.Width;
					array[num2] = dxtBlock.Colors[0, 1].ToArgb();
					array[num2 + 1] = dxtBlock.Colors[1, 1].ToArgb();
					array[num2 + 2] = dxtBlock.Colors[2, 1].ToArgb();
					array[num2 + 3] = dxtBlock.Colors[3, 1].ToArgb();
					num2 += m_Bitmap.Width;
					array[num2] = dxtBlock.Colors[0, 2].ToArgb();
					array[num2 + 1] = dxtBlock.Colors[1, 2].ToArgb();
					array[num2 + 2] = dxtBlock.Colors[2, 2].ToArgb();
					array[num2 + 3] = dxtBlock.Colors[3, 2].ToArgb();
					num2 += m_Bitmap.Width;
					array[num2] = dxtBlock.Colors[0, 3].ToArgb();
					array[num2 + 1] = dxtBlock.Colors[1, 3].ToArgb();
					array[num2 + 2] = dxtBlock.Colors[2, 3].ToArgb();
					array[num2 + 3] = dxtBlock.Colors[3, 3].ToArgb();
				}
			}
			Marshal.Copy(array, 0, scan, num);
			m_Bitmap.UnlockBits(bitmapData);
			array = null;
			binaryReader.Close();
			memoryStream.Close();
		}

		private void WriteBitmapToA8R8G8B8()
		{
			if (m_Bitmap.Height * m_Bitmap.Width * 4 > m_RawData.Length)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < m_Bitmap.Height; i++)
			{
				for (int j = 0; j < m_Bitmap.Width; j++)
				{
					Color pixel = m_Bitmap.GetPixel(j, i);
					byte b = pixel.B;
					byte g = pixel.G;
					byte r = pixel.R;
					byte a = pixel.A;
					m_RawData[num++] = b;
					m_RawData[num++] = g;
					m_RawData[num++] = r;
					m_RawData[num++] = a;
				}
			}
		}

		private void WriteBitmapToDxt()
		{
			MemoryStream memoryStream = new MemoryStream(m_RawData);
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			DxtBlock dxtBlock = new DxtBlock((int)m_ImageType);
			int num = (m_Bitmap.Height + 3) / 4;
			int num2 = (m_Bitmap.Width + 3) / 4;
			if (m_Bitmap.Height < 4 || m_Bitmap.Width < 4)
			{
				dxtBlock.Colors[0, 0] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[0, 1] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[0, 2] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[0, 3] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[1, 0] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[1, 1] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[1, 2] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[1, 3] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[2, 0] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[2, 1] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[2, 2] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[2, 3] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[3, 0] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[3, 1] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[3, 2] = Color.FromArgb(0, 128, 128, 128);
				dxtBlock.Colors[3, 3] = Color.FromArgb(0, 128, 128, 128);
				for (int i = 0; i < m_Bitmap.Width; i++)
				{
					for (int j = 0; j < m_Bitmap.Height; j++)
					{
						if (i >= 0 && j >= 0 && i < 4 && j < 4)
						{
							dxtBlock.Colors[i, j] = m_Bitmap.GetPixel(i, j);
						}
					}
				}
				dxtBlock.Save(binaryWriter);
			}
			else
			{
				for (int k = 0; k < num; k++)
				{
					int num3 = k * 4;
					for (int l = 0; l < num2; l++)
					{
						int num4 = l * 4;
						dxtBlock.Colors[0, 0] = m_Bitmap.GetPixel(num4, num3);
						dxtBlock.Colors[0, 1] = m_Bitmap.GetPixel(num4, num3 + 1);
						dxtBlock.Colors[0, 2] = m_Bitmap.GetPixel(num4, num3 + 2);
						dxtBlock.Colors[0, 3] = m_Bitmap.GetPixel(num4, num3 + 3);
						dxtBlock.Colors[1, 0] = m_Bitmap.GetPixel(num4 + 1, num3);
						dxtBlock.Colors[1, 1] = m_Bitmap.GetPixel(num4 + 1, num3 + 1);
						dxtBlock.Colors[1, 2] = m_Bitmap.GetPixel(num4 + 1, num3 + 2);
						dxtBlock.Colors[1, 3] = m_Bitmap.GetPixel(num4 + 1, num3 + 3);
						dxtBlock.Colors[2, 0] = m_Bitmap.GetPixel(num4 + 2, num3);
						dxtBlock.Colors[2, 1] = m_Bitmap.GetPixel(num4 + 2, num3 + 1);
						dxtBlock.Colors[2, 2] = m_Bitmap.GetPixel(num4 + 2, num3 + 2);
						dxtBlock.Colors[2, 3] = m_Bitmap.GetPixel(num4 + 2, num3 + 3);
						dxtBlock.Colors[3, 0] = m_Bitmap.GetPixel(num4 + 3, num3);
						dxtBlock.Colors[3, 1] = m_Bitmap.GetPixel(num4 + 3, num3 + 1);
						dxtBlock.Colors[3, 2] = m_Bitmap.GetPixel(num4 + 3, num3 + 2);
						dxtBlock.Colors[3, 3] = m_Bitmap.GetPixel(num4 + 3, num3 + 3);
						dxtBlock.Save(binaryWriter);
					}
				}
			}
			binaryWriter.Close();
			memoryStream.Close();
		}

		private void WriteBitmapToGrey8()
		{
			if (m_Bitmap.Height * m_Bitmap.Width > m_RawData.Length)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < m_Bitmap.Height; i++)
			{
				for (int j = 0; j < m_Bitmap.Width; j++)
				{
					byte b = m_Bitmap.GetPixel(j, i).B;
					m_RawData[num++] = b;
				}
			}
		}

		private void WriteBitmapToGrey8Alfa8()
		{
			if (m_Bitmap.Height * m_Bitmap.Width * 4 > m_RawData.Length)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < m_Bitmap.Height; i++)
			{
				for (int j = 0; j < m_Bitmap.Width; j++)
				{
					byte b = m_Bitmap.GetPixel(j, i).B;
					m_RawData[num++] = b;
				}
			}
		}
	}
}
