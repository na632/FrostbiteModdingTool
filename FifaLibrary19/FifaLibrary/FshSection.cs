using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace FifaLibrary
{
	public class FshSection
	{
		private Fsh m_Parent;

		private int m_Type;

		public bool m_IsCompressed;

		public bool m_IsBitmap;

		public Bitmap m_Bitmap;

		public bool m_IsPalette;

		public Color[] m_Palette;

		public int m_NextOffset;

		public int m_Size;

		private FshSection m_NextSection;

		public short m_Width;

		public short m_Height;

		public short[] m_Misc = new short[4];

		public int m_NScales;

		public byte[] m_RawData;

		public byte[] PadData;

		public int Type => m_Type;

		public FshSection NextSection => m_NextSection;

		public FshSection(Fsh parent, BinaryReader r, int maxSize)
		{
			m_Parent = parent;
			int num = r.ReadInt32();
			m_NextOffset = ((num >> 8) & 0xFFFFFF);
			maxSize -= m_NextOffset;
			m_Type = (num & 0x7F);
			m_IsCompressed = ((num & 0x80) != 0);
			m_IsPalette = false;
			if (m_NextOffset != 0)
			{
				m_Size = m_NextOffset;
			}
			else
			{
				m_Size = maxSize;
			}
			switch (m_Type)
			{
			case 96:
			case 97:
			case 98:
			case 109:
			case 120:
			case 123:
			case 125:
			case 126:
			case 127:
				m_Width = r.ReadInt16();
				m_Height = r.ReadInt16();
				m_Misc[0] = r.ReadInt16();
				m_Misc[1] = r.ReadInt16();
				m_Misc[2] = r.ReadInt16();
				m_Misc[3] = r.ReadInt16();
				m_NScales = ((m_Misc[3] >> 12) & 0xF);
				num = m_Size - 16;
				m_RawData = r.ReadBytes(num);
				break;
			case 105:
				m_Width = r.ReadInt16();
				m_Height = r.ReadInt16();
				m_Misc[0] = r.ReadInt16();
				m_Misc[1] = r.ReadInt16();
				m_Misc[2] = r.ReadInt16();
				m_Misc[3] = r.ReadInt16();
				num = m_Size - 16;
				m_RawData = r.ReadBytes(num);
				break;
			case 111:
				m_Width = r.ReadInt16();
				m_Height = r.ReadInt16();
				num = m_Size - 8;
				m_RawData = r.ReadBytes(num);
				break;
			case 112:
				num = m_Size - 4;
				m_RawData = r.ReadBytes(num);
				break;
			case 124:
				m_Width = r.ReadInt16();
				m_Height = r.ReadInt16();
				num = m_Size - 8;
				m_RawData = r.ReadBytes(num);
				break;
			case 36:
			case 42:
				m_Width = r.ReadInt16();
				m_Height = r.ReadInt16();
				m_Misc[0] = r.ReadInt16();
				m_Misc[1] = r.ReadInt16();
				m_Misc[2] = r.ReadInt16();
				m_Misc[3] = r.ReadInt16();
				m_IsPalette = true;
				num = m_Size - 16;
				m_RawData = r.ReadBytes(num);
				break;
			}
			if (m_NextOffset != 0)
			{
				m_NextSection = new FshSection(m_Parent, r, maxSize);
			}
		}

		public FshSection(BinaryReader r)
		{
			int count = (int)(r.BaseStream.Length - r.BaseStream.Position);
			m_RawData = r.ReadBytes(count);
		}

		public void Save(BinaryWriter w)
		{
			int num = 0;
			num |= (m_NextOffset & 0xFFFFFF) << 8;
			num |= (m_IsCompressed ? 128 : 0);
			num |= (m_Type & 0x7F);
			w.Write(num);
			switch (m_Type)
			{
			case 36:
			case 42:
			case 96:
			case 97:
			case 98:
			case 105:
			case 109:
			case 120:
			case 123:
			case 125:
			case 126:
			case 127:
				w.Write(m_Width);
				w.Write(m_Height);
				w.Write(m_Misc[0]);
				w.Write(m_Misc[1]);
				w.Write(m_Misc[2]);
				m_Misc[3] = (short)((m_Misc[3] & 0xFFF) | (m_NScales << 12));
				w.Write(m_Misc[3]);
				w.Write(m_RawData);
				break;
			case 111:
			case 124:
				w.Write(m_Width);
				w.Write(m_Height);
				w.Write(m_RawData);
				break;
			case 112:
				w.Write(m_RawData);
				break;
			}
			if (m_NextOffset != 0)
			{
				m_NextSection.Save(w);
			}
		}

		public Color[] RawDataToPalette()
		{
			if (m_IsPalette)
			{
				Color[] array = new Color[m_Width];
				switch (m_Type)
				{
				case 36:
				{
					for (int j = 0; j < m_Width; j++)
					{
						array[j] = Color.FromArgb(m_RawData[j * 3], m_RawData[j * 3 + 1], m_RawData[j * 3 + 2]);
					}
					break;
				}
				case 42:
				{
					for (int i = 0; i < m_Width; i++)
					{
						array[i] = Color.FromArgb(m_RawData[i * 4], m_RawData[i * 4 + 1], m_RawData[i * 4 + 2], m_RawData[i * 4 + 3]);
					}
					break;
				}
				default:
					return null;
				}
				return array;
			}
			return null;
		}

		public int ComputeRawDataLength(int width, int height)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i <= m_NScales; i++)
			{
				switch (m_Type)
				{
				case 96:
					num = (width + 3) / 4 * ((height + 3) / 4) * 8;
					break;
				case 97:
				case 98:
					num = (width + 3) / 4 * ((height + 3) / 4) * 16;
					break;
				case 109:
				case 120:
				case 126:
					num = width * height * 2;
					break;
				case 123:
					num = width * height;
					break;
				case 125:
					num = width * height * 4;
					break;
				case 127:
					num = width * height * 3;
					break;
				}
				num2 += num;
				width /= 2;
				height /= 2;
			}
			int num3 = num2 & 0xF;
			if (num3 != 0)
			{
				num2 += 16 - num3;
			}
			return num2;
		}

		public Bitmap RawDataToBmp()
		{
			_ = m_IsCompressed;
			m_Bitmap = new Bitmap(m_Width, m_Height, PixelFormat.Format32bppArgb);
			MemoryStream memoryStream = new MemoryStream(m_RawData);
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			new BinaryWriter(memoryStream);
			switch (m_Type)
			{
			case 123:
			{
				for (FshSection nextSection = m_NextSection; nextSection != null; nextSection = nextSection.m_NextSection)
				{
					m_Palette = nextSection.RawDataToPalette();
					if (m_Palette != null)
					{
						break;
					}
				}
				if (m_Palette == null)
				{
					break;
				}
				for (int num3 = 0; num3 < m_Bitmap.Height; num3++)
				{
					for (int num4 = 0; num4 < m_Bitmap.Width; num4++)
					{
						int num5 = binaryReader.ReadByte();
						m_Bitmap.SetPixel(num4, num3, m_Palette[num5]);
					}
				}
				break;
			}
			case 125:
			{
				Rectangle rect = new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height);
				BitmapData bitmapData = m_Bitmap.LockBits(rect, ImageLockMode.WriteOnly, m_Bitmap.PixelFormat);
				IntPtr scan = bitmapData.Scan0;
				int num9 = m_Bitmap.Width * m_Bitmap.Height;
				Marshal.Copy(m_RawData, 0, scan, num9 * 4);
				m_Bitmap.UnlockBits(bitmapData);
				break;
			}
			case 127:
			{
				for (int k = 0; k < m_Bitmap.Height; k++)
				{
					for (int l = 0; l < m_Bitmap.Width; l++)
					{
						int blue2 = binaryReader.ReadByte();
						int green2 = binaryReader.ReadByte();
						int red2 = binaryReader.ReadByte();
						m_Bitmap.SetPixel(l, k, Color.FromArgb(red2, green2, blue2));
					}
				}
				break;
			}
			case 109:
			{
				for (int num6 = 0; num6 < m_Bitmap.Height; num6++)
				{
					for (int num7 = 0; num7 < m_Bitmap.Width; num7++)
					{
						ushort num8 = binaryReader.ReadUInt16();
						int blue4 = (num8 & 0xF) * 16;
						int green4 = ((num8 >> 4) & 0xF) * 16;
						int red4 = ((num8 >> 8) & 0xF) * 16;
						int alpha2 = ((num8 >> 12) & 0xF) * 16;
						m_Bitmap.SetPixel(num7, num6, Color.FromArgb(alpha2, red4, green4, blue4));
					}
				}
				break;
			}
			case 98:
				ReadDxtToBitmap(5, binaryReader);
				break;
			case 97:
				ReadDxtToBitmap(3, binaryReader);
				break;
			case 96:
				ReadDxtToBitmap(1, binaryReader);
				break;
			case 120:
			{
				for (int m = 0; m < m_Bitmap.Height; m++)
				{
					for (int n = 0; n < m_Bitmap.Width; n++)
					{
						ushort num2 = binaryReader.ReadUInt16();
						int blue3 = (num2 & 0x1F) * 8;
						int green3 = ((num2 >> 5) & 0x3F) * 4;
						int red3 = ((num2 >> 11) & 0x1F) * 8;
						m_Bitmap.SetPixel(n, m, Color.FromArgb(red3, green3, blue3));
					}
				}
				break;
			}
			case 126:
			{
				for (int i = 0; i < m_Bitmap.Height; i++)
				{
					for (int j = 0; j < m_Bitmap.Width; j++)
					{
						ushort num = binaryReader.ReadUInt16();
						int blue = (num & 0x1F) * 8;
						int green = ((num >> 5) & 0x1F) * 8;
						int red = ((num >> 10) & 0x1F) * 8;
						int alpha = ((num & 0x8000) != 0) ? 255 : 0;
						m_Bitmap.SetPixel(j, i, Color.FromArgb(alpha, red, green, blue));
					}
				}
				break;
			}
			}
			return m_Bitmap;
		}

		public bool WriteBitmap(Bitmap bitmap, BinaryWriter bw)
		{
			switch (m_Type)
			{
			case 123:
			{
				if (m_Palette == null)
				{
					return false;
				}
				for (int num3 = 0; num3 < bitmap.Height; num3++)
				{
					for (int num4 = 0; num4 < bitmap.Width; num4++)
					{
						Color pixel4 = bitmap.GetPixel(num4, num3);
						byte value = 0;
						for (int num5 = 0; num5 < m_Palette.Length; num5++)
						{
							if (pixel4 == m_Palette[num5])
							{
								value = (byte)num5;
								break;
							}
						}
						bw.Write(value);
					}
				}
				break;
			}
			case 125:
			{
				for (int num9 = 0; num9 < bitmap.Height; num9++)
				{
					for (int num10 = 0; num10 < bitmap.Width; num10++)
					{
						Color pixel6 = bitmap.GetPixel(num10, num9);
						byte b5 = pixel6.B;
						byte g5 = pixel6.G;
						byte r5 = pixel6.R;
						byte a3 = pixel6.A;
						bw.Write(b5);
						bw.Write(g5);
						bw.Write(r5);
						bw.Write(a3);
					}
				}
				break;
			}
			case 127:
			{
				for (int k = 0; k < bitmap.Height; k++)
				{
					for (int l = 0; l < bitmap.Width; l++)
					{
						Color pixel2 = bitmap.GetPixel(l, k);
						byte b2 = pixel2.B;
						byte g2 = pixel2.G;
						byte r2 = pixel2.R;
						bw.Write(b2);
						bw.Write(g2);
						bw.Write(r2);
					}
				}
				break;
			}
			case 109:
			{
				for (int num6 = 0; num6 < bitmap.Height; num6++)
				{
					for (int num7 = 0; num7 < bitmap.Width; num7++)
					{
						Color pixel5 = bitmap.GetPixel(num7, num6);
						byte b4 = pixel5.B;
						byte g4 = pixel5.G;
						byte r4 = pixel5.R;
						byte a2 = pixel5.A;
						ushort num8 = 0;
						num8 = (ushort)(num8 | (ushort)((b4 & 0xF0) >> 4));
						num8 = (ushort)(num8 | (ushort)(g4 & 0xF0));
						num8 = (ushort)(num8 | (ushort)((r4 & 0xF0) << 4));
						num8 = (ushort)(num8 | (ushort)((a2 & 0xF0) << 8));
						bw.Write(num8);
					}
				}
				break;
			}
			case 98:
				return WriteBitmapToDxt(bitmap, 5, bw);
			case 97:
				return WriteBitmapToDxt(bitmap, 3, bw);
			case 96:
				return WriteBitmapToDxt(bitmap, 1, bw);
			case 120:
			{
				for (int m = 0; m < bitmap.Height; m++)
				{
					for (int n = 0; n < bitmap.Width; n++)
					{
						Color pixel3 = bitmap.GetPixel(n, m);
						byte b3 = pixel3.B;
						byte g3 = pixel3.G;
						byte r3 = pixel3.R;
						ushort num2 = 0;
						num2 = (ushort)(num2 | (ushort)((b3 & 0xF8) >> 3));
						num2 = (ushort)(num2 | (ushort)((g3 & 0xFC) << 3));
						num2 = (ushort)(num2 | (ushort)((r3 & 0xF8) << 8));
						bw.Write(num2);
					}
				}
				break;
			}
			case 126:
			{
				for (int i = 0; i < bitmap.Height; i++)
				{
					for (int j = 0; j < bitmap.Width; j++)
					{
						Color pixel = bitmap.GetPixel(j, i);
						byte b = pixel.B;
						byte g = pixel.G;
						byte r = pixel.R;
						byte a = pixel.A;
						ushort num = 0;
						num = (ushort)(num | (ushort)((b & 0xF8) >> 3));
						num = (ushort)(num | (ushort)((g & 0xF8) << 2));
						num = (ushort)(num | (ushort)((r & 0xF8) << 7));
						if (a != 0)
						{
							num = (ushort)(num | 0x8000);
						}
						bw.Write(num);
					}
				}
				break;
			}
			}
			return true;
		}

		public bool BitmapToRawData()
		{
			int num = ComputeRawDataLength(m_Bitmap.Width, m_Bitmap.Height);
			if (num != m_RawData.Length)
			{
				m_RawData = new byte[num];
				m_Size = num + 16;
				if (m_NextSection != null)
				{
					m_NextOffset = m_Size;
				}
				else
				{
					m_NextOffset = 0;
				}
				m_Height = (short)m_Bitmap.Height;
				m_Width = (short)m_Bitmap.Width;
				m_Parent.ComputeImageDir();
			}
			BinaryWriter bw = new BinaryWriter(new MemoryStream(m_RawData));
			Bitmap bitmap = m_Bitmap;
			int num2 = (1 << m_NScales) - 1;
			if (((m_Bitmap.Width & num2) | (m_Bitmap.Height & num2)) != 0)
			{
				m_NScales = 0;
			}
			for (int i = 0; i <= m_NScales; i++)
			{
				if (!WriteBitmap(bitmap, bw))
				{
					return false;
				}
				if (i < m_NScales)
				{
					bitmap = GraphicUtil.ReduceBitmap(bitmap);
					if (bitmap == null)
					{
						m_NScales = i - 1;
						break;
					}
				}
			}
			return true;
		}

		public bool ReplaceRawData(byte[] rawData, int width, int height)
		{
			int num = rawData.Length;
			if (num != m_RawData.Length)
			{
				m_RawData = new byte[num];
				m_Size = num + 16;
				if (m_NextSection != null)
				{
					m_NextOffset = m_Size;
				}
				else
				{
					m_NextOffset = 0;
				}
				m_Height = (short)width;
				m_Width = (short)height;
				m_Parent.ComputeImageDir();
			}
			for (int i = 0; i < m_RawData.Length; i++)
			{
				m_RawData[i] = rawData[i];
			}
			return true;
		}

		private void ReadDxtToBitmap(int dxtType, BinaryReader br)
		{
			DxtBlock dxtBlock = new DxtBlock(dxtType);
			Rectangle rect = new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height);
			BitmapData bitmapData = m_Bitmap.LockBits(rect, ImageLockMode.WriteOnly, m_Bitmap.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			int num = m_Bitmap.Width * m_Bitmap.Height;
			int[] array = new int[num];
			for (int i = 0; i < m_Bitmap.Height / 4; i++)
			{
				for (int j = 0; j < m_Bitmap.Width / 4; j++)
				{
					dxtBlock.Load(br);
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
		}

		private bool WriteBitmapToDxt(Bitmap bitmap, int dxtType, BinaryWriter bw)
		{
			if (((bitmap.Height & 3) | (bitmap.Width & 3)) != 0)
			{
				return false;
			}
			DxtBlock dxtBlock = new DxtBlock(dxtType);
			int num = (bitmap.Height + 3) / 4;
			int num2 = (bitmap.Width + 3) / 4;
			for (int i = 0; i < num; i++)
			{
				int num3 = i * 4;
				for (int j = 0; j < num2; j++)
				{
					int num4 = j * 4;
					dxtBlock.Colors[0, 0] = bitmap.GetPixel(num4, num3);
					dxtBlock.Colors[0, 1] = bitmap.GetPixel(num4, num3 + 1);
					dxtBlock.Colors[0, 2] = bitmap.GetPixel(num4, num3 + 2);
					dxtBlock.Colors[0, 3] = bitmap.GetPixel(num4, num3 + 3);
					dxtBlock.Colors[1, 0] = bitmap.GetPixel(num4 + 1, num3);
					dxtBlock.Colors[1, 1] = bitmap.GetPixel(num4 + 1, num3 + 1);
					dxtBlock.Colors[1, 2] = bitmap.GetPixel(num4 + 1, num3 + 2);
					dxtBlock.Colors[1, 3] = bitmap.GetPixel(num4 + 1, num3 + 3);
					dxtBlock.Colors[2, 0] = bitmap.GetPixel(num4 + 2, num3);
					dxtBlock.Colors[2, 1] = bitmap.GetPixel(num4 + 2, num3 + 1);
					dxtBlock.Colors[2, 2] = bitmap.GetPixel(num4 + 2, num3 + 2);
					dxtBlock.Colors[2, 3] = bitmap.GetPixel(num4 + 2, num3 + 3);
					dxtBlock.Colors[3, 0] = bitmap.GetPixel(num4 + 3, num3);
					dxtBlock.Colors[3, 1] = bitmap.GetPixel(num4 + 3, num3 + 1);
					dxtBlock.Colors[3, 2] = bitmap.GetPixel(num4 + 3, num3 + 2);
					dxtBlock.Colors[3, 3] = bitmap.GetPixel(num4 + 3, num3 + 3);
					dxtBlock.Save(bw);
				}
			}
			return true;
		}

		public bool ReplaceBitmap(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			m_Bitmap = bitmap;
			BitmapToRawData();
			return true;
		}

		public void Hash(string fileName)
		{
			if (m_Type != 111)
			{
				return;
			}
			char[] array = FifaUtil.ComputeHash(fileName).ToString("x8").ToCharArray();
			int num = 0;
			while (true)
			{
				if (num < m_RawData.Length - 11)
				{
					if (m_RawData[num] == 44 && m_RawData[num + 1] == 48 && m_RawData[num + 2] == 120)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			num += 3;
			for (int i = 0; i < 8; i++)
			{
				m_RawData[num + i] = (byte)array[i];
			}
		}
	}
}
