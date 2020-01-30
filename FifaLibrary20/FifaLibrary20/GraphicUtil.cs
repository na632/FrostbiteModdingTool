using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class GraphicUtil
	{
		public static Bitmap ReduceBitmap(Bitmap srcBitmap)
		{
			int width = srcBitmap.Width;
			int height = srcBitmap.Height;
			if (width * height == 0)
			{
				return null;
			}
			width /= 2;
			height /= 2;
			if (width == 0)
			{
				width = 1;
			}
			if (height == 0)
			{
				height = 1;
			}
			return ResizeBitmap(srcBitmap, width, height, InterpolationMode.HighQualityBicubic);
		}

		public static Bitmap RemapBitmap(Bitmap srcBitmap, int destWidth, int destHeight)
		{
			Bitmap bitmap = new Bitmap(destWidth, destHeight, PixelFormat.Format32bppArgb);
			int width = srcBitmap.Width;
			int height = srcBitmap.Height;
			float num = (float)width / (float)destWidth;
			float num2 = (float)height / (float)destHeight;
			for (int i = 0; i < destWidth; i++)
			{
				for (int j = 0; j < destHeight; j++)
				{
					bitmap.SetPixel(i, j, RemapPixel(srcBitmap, (float)i * num, (float)j * num2));
				}
			}
			return bitmap;
		}

		private static Color RemapPixel(Bitmap srcBitmap, float x, float y)
		{
			int num = (int)Math.Floor(x);
			int num2 = (int)Math.Floor(y);
			int x2 = (num < srcBitmap.Width) ? num : (srcBitmap.Width - 1);
			int x3 = (num + 1 < srcBitmap.Width) ? (num + 1) : (srcBitmap.Width - 1);
			int y2 = (num2 < srcBitmap.Height) ? num2 : (srcBitmap.Height - 1);
			int y3 = (num2 + 1 < srcBitmap.Height) ? (num2 + 1) : (srcBitmap.Height - 1);
			Color pixel = srcBitmap.GetPixel(x2, y2);
			Color pixel2 = srcBitmap.GetPixel(x3, y2);
			Color pixel3 = srcBitmap.GetPixel(x2, y3);
			Color pixel4 = srcBitmap.GetPixel(x3, y3);
			float num3 = x - (float)num;
			float num4 = y - (float)num2;
			float num5 = (1f - num3) * (1f - num4);
			float num6 = num3 * (1f - num4);
			float num7 = (1f - num3) * num4;
			float num8 = num3 * num4;
			int red = (int)((float)(int)pixel.R * num5 + (float)(int)pixel2.R * num6 + (float)(int)pixel3.R * num7 + (float)(int)pixel4.R * num8);
			int green = (int)((float)(int)pixel.G * num5 + (float)(int)pixel2.G * num6 + (float)(int)pixel3.G * num7 + (float)(int)pixel4.G * num8);
			int blue = (int)((float)(int)pixel.B * num5 + (float)(int)pixel2.B * num6 + (float)(int)pixel3.B * num7 + (float)(int)pixel4.B * num8);
			return Color.FromArgb((int)((float)(int)pixel.A * num5 + (float)(int)pixel2.A * num6 + (float)(int)pixel3.A * num7 + (float)(int)pixel4.A * num8), red, green, blue);
		}

		public static void LoadPictureImage(PictureBox picture, Bitmap bitmap)
		{
			if (bitmap == null)
			{
				picture.Image = bitmap;
			}
			else if (picture.Width == bitmap.Width && picture.Height == bitmap.Height)
			{
				picture.Image = bitmap;
			}
			else
			{
				picture.Image = RemapBitmap(bitmap, picture.Width, picture.Height);
			}
		}

		public static Bitmap ResizeBitmap(Bitmap sourceBitmap, int width, int height, InterpolationMode interpolationMode)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			if (width < 0)
			{
				width = -width;
			}
			if (height < 0)
			{
				height = -height;
			}
			if (width == 0 || height == 0)
			{
				return null;
			}
			if (sourceBitmap.Width == width && sourceBitmap.Height == height)
			{
				return sourceBitmap;
			}
			Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.InterpolationMode = interpolationMode;
			graphics.DrawImage(sourceBitmap, new Rectangle(0, 0, width, height), 0, 0, sourceBitmap.Width, sourceBitmap.Height, GraphicsUnit.Pixel);
			graphics.Dispose();
			return bitmap;
		}

		public static Bitmap CanvasSizeBitmap(Bitmap sourceBitmap, int width, int height)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bitmap);
			int x = (width - sourceBitmap.Width) / 2;
			int y = (height - sourceBitmap.Height) / 2;
			graphics.DrawImage(sourceBitmap, new Rectangle(x, y, sourceBitmap.Width, sourceBitmap.Height), 0, 0, sourceBitmap.Width, sourceBitmap.Height, GraphicsUnit.Pixel);
			graphics.Dispose();
			return bitmap;
		}

		public static Bitmap CanvasSizeBitmapCentered(Bitmap sourceBitmap, int width, int height)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			if (width > sourceBitmap.Width || height > sourceBitmap.Height)
			{
				return null;
			}
			int num = (sourceBitmap.Width - width) / 2;
			int num2 = (sourceBitmap.Height - height) / 2;
			Bitmap bitmap = new Bitmap(width, height, sourceBitmap.PixelFormat);
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadOnly, sourceBitmap.PixelFormat);
			rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			IntPtr scan2 = bitmapData2.Scan0;
			int num3 = sourceBitmap.Width * sourceBitmap.Height;
			int num4 = bitmap.Width * bitmap.Height;
			int[] array = new int[num3];
			int[] array2 = new int[num4];
			Marshal.Copy(scan, array, 0, num3);
			Marshal.Copy(scan2, array2, 0, num4);
			int num5 = 0;
			int num6 = num2 * sourceBitmap.Width + num;
			for (int i = 0; i < bitmap.Height; i++)
			{
				for (int j = 0; j < bitmap.Width; j++)
				{
					array2[num5] = array[num6];
					num5++;
					num6++;
				}
				num6 += num * 2;
			}
			Marshal.Copy(array2, 0, scan2, num4);
			sourceBitmap.UnlockBits(bitmapData);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		public static Bitmap Get32bitBitmap(Bitmap sourceBitmap)
		{
			Bitmap bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.DrawImage(sourceBitmap, 0, 0, sourceBitmap.Width, sourceBitmap.Height);
			graphics.Dispose();
			return bitmap;
		}

		public static bool GetAlfaFromChannel(Bitmap sourceBitmap, Bitmap alfaBitmap, int channel)
		{
			if (sourceBitmap.Width != alfaBitmap.Width || sourceBitmap.Height != alfaBitmap.Height)
			{
				return false;
			}
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
			rect = new Rectangle(0, 0, alfaBitmap.Width, alfaBitmap.Height);
			BitmapData bitmapData2 = alfaBitmap.LockBits(rect, ImageLockMode.ReadOnly, alfaBitmap.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			IntPtr scan2 = bitmapData2.Scan0;
			int num = sourceBitmap.Width * sourceBitmap.Height;
			byte[] array = new byte[num * 4];
			byte[] array2 = new byte[num * 4];
			Marshal.Copy(scan, array, 0, num * 4);
			Marshal.Copy(scan2, array2, 0, num * 4);
			for (int i = 3; i < num * 4; i += 4)
			{
				array[i] = array2[i - channel];
			}
			Marshal.Copy(array, 0, scan, num * 4);
			sourceBitmap.UnlockBits(bitmapData);
			alfaBitmap.UnlockBits(bitmapData2);
			return true;
		}

		public static bool RemoveAlfaChannel(Bitmap sourceBitmap)
		{
			if (sourceBitmap == null)
			{
				return false;
			}
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			int num = sourceBitmap.Width * sourceBitmap.Height;
			byte[] array = new byte[num * 4];
			Marshal.Copy(scan, array, 0, num * 4);
			for (int i = 3; i < num * 4; i += 4)
			{
				array[i] = byte.MaxValue;
			}
			Marshal.Copy(array, 0, scan, num * 4);
			sourceBitmap.UnlockBits(bitmapData);
			return true;
		}

		public static Bitmap SubSampleBitmap(Bitmap sourceBitmap, int xStep, int yStep)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			if (xStep <= 0 || yStep <= 0)
			{
				return null;
			}
			if (xStep == 1 && yStep == 1)
			{
				return (Bitmap)sourceBitmap.Clone();
			}
			Bitmap bitmap = new Bitmap(sourceBitmap.Width / xStep, sourceBitmap.Height / yStep, sourceBitmap.PixelFormat);
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadOnly, sourceBitmap.PixelFormat);
			rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			IntPtr scan2 = bitmapData2.Scan0;
			int num = sourceBitmap.Width * sourceBitmap.Height;
			int num2 = bitmap.Width * bitmap.Height;
			int[] array = new int[num];
			int[] array2 = new int[num2];
			Marshal.Copy(scan, array, 0, num);
			Marshal.Copy(scan2, array2, 0, num2);
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < bitmap.Height; i++)
			{
				for (int j = 0; j < bitmap.Width; j++)
				{
					array2[num3] = array[num4];
					num3++;
					num4 += xStep;
				}
				num4 += (yStep - 1) * sourceBitmap.Width;
			}
			Marshal.Copy(array2, 0, scan2, num2);
			sourceBitmap.UnlockBits(bitmapData);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		public static bool ColorizeRGB(Bitmap sourceBitmap, Color color1, Color color2, Color color3, bool preserveArmBand)
		{
			if (sourceBitmap == null)
			{
				return false;
			}
			Color[,] array = new Color[48, 256];
			preserveArmBand = (preserveArmBand && sourceBitmap.Width == 1024 && sourceBitmap.Height == 1024);
			if (preserveArmBand)
			{
				int num = 0;
				int num2 = 0;
				for (int i = 975; i <= 1022; i++)
				{
					num2 = 0;
					for (int j = 384; j <= 639; j++)
					{
						array[num, num2++] = sourceBitmap.GetPixel(j, i);
					}
					num++;
				}
			}
			bool flag = ColorizeRGB(sourceBitmap, color1, color2, color3, 0, sourceBitmap.Height);
			if (flag && preserveArmBand)
			{
				int num3 = 0;
				int num4 = 0;
				for (int k = 975; k <= 1022; k++)
				{
					num4 = 0;
					for (int l = 384; l <= 639; l++)
					{
						sourceBitmap.SetPixel(l, k, array[num3, num4++]);
					}
					num3++;
				}
			}
			return flag;
		}

		public static bool ColorizeRGB(Bitmap sourceBitmap, Color color1, Color color2, Color color3, int firstRow, int lastRow)
		{
			if (sourceBitmap == null)
			{
				return false;
			}
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			int num = sourceBitmap.Width * sourceBitmap.Height;
			byte[] array = new byte[num * 4];
			Marshal.Copy(scan, array, 0, num * 4);
			for (int i = firstRow * sourceBitmap.Width; i < lastRow * sourceBitmap.Width; i++)
			{
				int num2 = array[i * 4];
				int num3 = array[i * 4 + 1];
				int num4 = array[i * 4 + 2];
				int num5 = (color1.R * num4 + color2.R * num3 + color3.R * num2) / 226;
				int num6 = (color1.G * num4 + color2.G * num3 + color3.G * num2) / 226;
				int num7 = (color1.B * num4 + color2.B * num3 + color3.B * num2) / 226;
				if (num5 > 255)
				{
					num5 = 255;
				}
				if (num6 > 255)
				{
					num6 = 255;
				}
				if (num7 > 255)
				{
					num7 = 255;
				}
				array[i * 4] = (byte)num7;
				array[i * 4 + 1] = (byte)num6;
				array[i * 4 + 2] = (byte)num5;
			}
			Marshal.Copy(array, 0, scan, num * 4);
			sourceBitmap.UnlockBits(bitmapData);
			return true;
		}

		public static bool PrepareToColorize(Bitmap sourceBitmap, int firstRow, int lastRow)
		{
			if (sourceBitmap == null)
			{
				return false;
			}
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			int num = sourceBitmap.Width * sourceBitmap.Height;
			byte[] array = new byte[num * 4];
			Marshal.Copy(scan, array, 0, num * 4);
			for (int i = firstRow * sourceBitmap.Width; i < lastRow * sourceBitmap.Width; i++)
			{
				int num2 = array[i * 4];
				int num3 = array[i * 4 + 1];
				int num4 = array[i * 4 + 2];
				while (num4 + num3 + num2 > 226)
				{
					if (num4 > 0)
					{
						num4--;
					}
					if (num3 > 0)
					{
						num3--;
					}
					if (num2 > 0)
					{
						num2--;
					}
				}
				array[i * 4] = (byte)num2;
				array[i * 4 + 1] = (byte)num3;
				array[i * 4 + 2] = (byte)num4;
			}
			Marshal.Copy(array, 0, scan, num * 4);
			sourceBitmap.UnlockBits(bitmapData);
			return true;
		}

		public static Bitmap MultiplyBitmap(Bitmap sourceBitmap, Bitmap multBitmap)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			if (multBitmap == null)
			{
				return (Bitmap)sourceBitmap.Clone();
			}
			int num = sourceBitmap.Width * sourceBitmap.Height;
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			int[] array = new int[num];
			int[] array2 = new int[num];
			if (multBitmap == null)
			{
				return (Bitmap)sourceBitmap.Clone();
			}
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
			Marshal.Copy(bitmapData.Scan0, array, 0, num);
			sourceBitmap.UnlockBits(bitmapData);
			if (multBitmap.Width != sourceBitmap.Width || multBitmap.Height != sourceBitmap.Height)
			{
				multBitmap = ResizeBitmap(multBitmap, sourceBitmap.Width, sourceBitmap.Height, InterpolationMode.Bilinear);
			}
			Marshal.Copy(multBitmap.LockBits(rect, ImageLockMode.ReadWrite, multBitmap.PixelFormat).Scan0, array2, 0, num);
			multBitmap.UnlockBits(bitmapData);
			for (int i = 0; i < num; i++)
			{
				Color color = Color.FromArgb(array[i]);
				int num2 = array2[i] & 0xFF;
				int num3 = (int)((array2[i] & 4278190080u) >> 24);
				int num4 = color.R * num2 / 255;
				int num5 = color.G * num2 / 255;
				int num6 = color.B * num2 / 255;
				int num7 = color.A * num3 / 255;
				array[i] = ((((((num7 << 8) | num4) << 8) | num5) << 8) | num6);
			}
			Bitmap bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);
			rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
			IntPtr scan = bitmapData2.Scan0;
			Marshal.Copy(array, 0, scan, num);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		public static Bitmap MultiplyColorToBitmap(Bitmap sourceBitmap, Color color, int divisor, bool preserveAlfa)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			int num = sourceBitmap.Width * sourceBitmap.Height;
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			int[] array = new int[num];
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
			Marshal.Copy(bitmapData.Scan0, array, 0, num);
			sourceBitmap.UnlockBits(bitmapData);
			for (int i = 0; i < num; i++)
			{
				Color color2 = Color.FromArgb(array[i]);
				int num2 = color2.A;
				int num3 = color.R * color2.R / divisor;
				int num4 = color.G * color2.G / divisor;
				int num5 = color.B * color2.B / divisor;
				if (num3 > 255)
				{
					num3 = 255;
				}
				if (num4 > 255)
				{
					num4 = 255;
				}
				if (num5 > 255)
				{
					num5 = 255;
				}
				if (!preserveAlfa)
				{
					num2 = 255;
				}
				array[i] = ((((((num2 << 8) | num3) << 8) | num4) << 8) | num5);
			}
			Bitmap bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);
			rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
			IntPtr scan = bitmapData2.Scan0;
			Marshal.Copy(array, 0, scan, num);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		public static Bitmap AddWrinklesBitmap(Bitmap sourceBitmap, Bitmap wrinkleBitmap)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			if (wrinkleBitmap == null)
			{
				return (Bitmap)sourceBitmap.Clone();
			}
			int num = sourceBitmap.Width * sourceBitmap.Height;
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			int[] array = new int[num];
			int[] array2 = new int[num];
			if (wrinkleBitmap == null)
			{
				return (Bitmap)sourceBitmap.Clone();
			}
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
			Marshal.Copy(bitmapData.Scan0, array, 0, num);
			sourceBitmap.UnlockBits(bitmapData);
			if (wrinkleBitmap.Width != sourceBitmap.Width || wrinkleBitmap.Height != sourceBitmap.Height)
			{
				wrinkleBitmap = ResizeBitmap(wrinkleBitmap, sourceBitmap.Width, sourceBitmap.Height, InterpolationMode.Bilinear);
			}
			Marshal.Copy(wrinkleBitmap.LockBits(rect, ImageLockMode.ReadWrite, wrinkleBitmap.PixelFormat).Scan0, array2, 0, num);
			wrinkleBitmap.UnlockBits(bitmapData);
			for (int i = 0; i < num; i++)
			{
				Color color = Color.FromArgb(array[i]);
				int num2 = array2[i] & 0xFF;
				int num3 = (int)((array2[i] & 4278190080u) >> 24);
				int num4 = color.R * num2 / 255;
				int num5 = color.G * num2 / 255;
				int num6 = color.B * num2 / 255;
				int num7 = color.A * num3 / 255;
				array[i] = ((((((num7 << 8) | num4) << 8) | num5) << 8) | num6);
			}
			Bitmap bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);
			rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
			IntPtr scan = bitmapData2.Scan0;
			Marshal.Copy(array, 0, scan, num);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		public static Bitmap EmbossBitmap(Bitmap sourceBitmap, Bitmap embossingBitmap)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			if (embossingBitmap == null)
			{
				return (Bitmap)sourceBitmap.Clone();
			}
			int num = sourceBitmap.Width * sourceBitmap.Height;
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			int[] array = new int[num];
			int[] array2 = new int[num];
			if (embossingBitmap == null)
			{
				return (Bitmap)sourceBitmap.Clone();
			}
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
			Marshal.Copy(bitmapData.Scan0, array, 0, num);
			sourceBitmap.UnlockBits(bitmapData);
			if (embossingBitmap.Width != sourceBitmap.Width || embossingBitmap.Height != sourceBitmap.Height)
			{
				embossingBitmap = ResizeBitmap(embossingBitmap, sourceBitmap.Width, sourceBitmap.Height, InterpolationMode.Bilinear);
			}
			Marshal.Copy(embossingBitmap.LockBits(rect, ImageLockMode.ReadWrite, embossingBitmap.PixelFormat).Scan0, array2, 0, num);
			embossingBitmap.UnlockBits(bitmapData);
			for (int i = 0; i < num; i++)
			{
				Color color = Color.FromArgb(array[i]);
				int num2 = (127 - ((array2[i] & 0xFF00) >> 8)) / 2;
				int num3 = color.R + num2;
				if (num3 > 255)
				{
					num3 = 255;
				}
				if (num3 < 0)
				{
					num3 = 0;
				}
				int num4 = color.G + num2;
				if (num4 > 255)
				{
					num4 = 255;
				}
				if (num4 < 0)
				{
					num4 = 0;
				}
				int num5 = color.B + num2;
				if (num5 > 255)
				{
					num5 = 255;
				}
				if (num5 < 0)
				{
					num5 = 0;
				}
				int a = color.A;
				array[i] = ((((((a << 8) | num3) << 8) | num4) << 8) | num5);
			}
			Bitmap bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);
			rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
			IntPtr scan = bitmapData2.Scan0;
			Marshal.Copy(array, 0, scan, num);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		public static Bitmap Overlap(Bitmap lowerBitmap, Bitmap upperBitmap, Rectangle destRectangle)
		{
			if (lowerBitmap == null && upperBitmap == null)
			{
				return null;
			}
			if (lowerBitmap == null)
			{
				return (Bitmap)upperBitmap.Clone();
			}
			if (upperBitmap == null)
			{
				return (Bitmap)lowerBitmap.Clone();
			}
			Bitmap bitmap = ResizeBitmap(upperBitmap, destRectangle.Width, destRectangle.Height, InterpolationMode.Bicubic);
			if (bitmap != null)
			{
				Bitmap obj = (Bitmap)lowerBitmap.Clone();
				Graphics graphics = Graphics.FromImage(obj);
				graphics.DrawImage(bitmap, destRectangle.Left, destRectangle.Top);
				graphics.Dispose();
				return obj;
			}
			return lowerBitmap;
		}

		public static Bitmap ColorizeWhite(Bitmap srcBitmap, Color color)
		{
			if (srcBitmap == null)
			{
				return null;
			}
			int r = color.R;
			int g = color.G;
			int b = color.B;
			for (int i = 0; i < srcBitmap.Width; i++)
			{
				for (int j = 0; j < srcBitmap.Height; j++)
				{
					Color pixel = srcBitmap.GetPixel(i, j);
					if (pixel != Color.FromArgb(0, 0, 0, 0))
					{
						srcBitmap.SetPixel(i, j, Color.FromArgb(pixel.A, r, g, b));
					}
				}
			}
			return srcBitmap;
		}

		public static Bitmap AddColorOffsetPreservingAlfa(Bitmap sourceBitmap, int dR, int dG, int dB, bool preserveAlfa)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			if (sourceBitmap.PixelFormat != PixelFormat.Format32bppArgb)
			{
				return null;
			}
			int num = sourceBitmap.Width * sourceBitmap.Height;
			int[] array = new int[num];
			Bitmap bitmap = (Bitmap)sourceBitmap.Clone();
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			Marshal.Copy(scan, array, 0, num);
			for (int i = 0; i < num; i++)
			{
				Color color = Color.FromArgb(array[i]);
				int r = color.R;
				int g = color.G;
				int b = color.B;
				int a = color.A;
				r += dR;
				g += dG;
				b += dB;
				if (r > 255)
				{
					r = 255;
				}
				if (g > 255)
				{
					g = 255;
				}
				if (b > 255)
				{
					b = 255;
				}
				if (r < 0)
				{
					r = 0;
				}
				if (g < 0)
				{
					g = 0;
				}
				if (b < 0)
				{
					b = 0;
				}
				if (preserveAlfa)
				{
					array[i] = Color.FromArgb(a, r, g, b).ToArgb();
				}
				else
				{
					array[i] = Color.FromArgb(255, r, g, b).ToArgb();
				}
			}
			Marshal.Copy(array, 0, scan, num);
			bitmap.UnlockBits(bitmapData);
			return bitmap;
		}

		public static void RemapRectangle(Bitmap srcBitmap, Rectangle srcRect, Bitmap destBitmap, Rectangle destRect)
		{
			Graphics graphics = Graphics.FromImage(destBitmap);
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.DrawImage(srcBitmap, destRect, srcRect, GraphicsUnit.Pixel);
			graphics.Dispose();
		}

		public static void DrawOver(Bitmap belowBitmap, Bitmap overBitmap)
		{
			Graphics graphics = Graphics.FromImage(belowBitmap);
			graphics.DrawImage(overBitmap, 0, 0, overBitmap.Width, overBitmap.Height);
			graphics.Dispose();
		}

		public static Bitmap MakeAutoTransparent(Bitmap bitmap)
		{
			Color pixel = bitmap.GetPixel(0, 0);
			bitmap.MakeTransparent(pixel);
			return bitmap;
		}

		public static Color GetDominantColor(Bitmap bitmap, Rectangle rectangle)
		{
			int[] array = new int[256];
			for (int i = rectangle.X; i < rectangle.X + rectangle.Width; i++)
			{
				for (int j = rectangle.Y; j < rectangle.Y + rectangle.Height; j++)
				{
					array[bitmap.GetPixel(i, j).R]++;
				}
			}
			int num = -1;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int k = 0; k < 256; k++)
			{
				if (array[k] > num)
				{
					num = array[k];
					num2 = k;
				}
			}
			int num5 = 0;
			int num6 = 0;
			for (int l = rectangle.X; l < rectangle.X + rectangle.Width; l++)
			{
				for (int m = rectangle.Y; m < rectangle.Y + rectangle.Height; m++)
				{
					Color pixel = bitmap.GetPixel(l, m);
					if (pixel.R == num2)
					{
						num5 += pixel.G;
						num6 += pixel.B;
					}
				}
			}
			num3 = num5 / num;
			num4 = num6 / num;
			return Color.FromArgb(255, num2, num3, num4);
		}

		public static Bitmap ColorTuning(Bitmap variableBitmap, Bitmap referenceBitmap, Rectangle rect)
		{
			Color dominantColor = GetDominantColor(variableBitmap, rect);
			Color dominantColor2 = GetDominantColor(referenceBitmap, rect);
			int deltaR = dominantColor2.R - dominantColor.R;
			int deltaG = dominantColor2.G - dominantColor.G;
			int deltaB = dominantColor2.B - dominantColor.B;
			Bitmap obj = (Bitmap)variableBitmap.Clone();
			AddColorOffset(obj, deltaR, deltaG, deltaB);
			return obj;
		}

		public static Bitmap ColorTuning(Bitmap variableBitmap, Rectangle variableRect, Bitmap referenceBitmap, Rectangle referenceRect)
		{
			Color dominantColor = GetDominantColor(variableBitmap, variableRect);
			Color dominantColor2 = GetDominantColor(referenceBitmap, referenceRect);
			int deltaR = dominantColor2.R - dominantColor.R;
			int deltaG = dominantColor2.G - dominantColor.G;
			int deltaB = dominantColor2.B - dominantColor.B;
			Bitmap obj = (Bitmap)variableBitmap.Clone();
			AddColorOffset(obj, deltaR, deltaG, deltaB);
			return obj;
		}

		public static void AddColorOffset(Bitmap bitmap, int deltaR, int deltaG, int deltaB)
		{
			if (bitmap == null)
			{
				return;
			}
			for (int i = 0; i < bitmap.Width; i++)
			{
				for (int j = 0; j < bitmap.Height; j++)
				{
					Color pixel = bitmap.GetPixel(i, j);
					int num = pixel.R + deltaR;
					int num2 = pixel.G + deltaG;
					int num3 = pixel.B + deltaB;
					if (num > 255)
					{
						num = 255;
					}
					if (num2 > 255)
					{
						num2 = 255;
					}
					if (num3 > 255)
					{
						num3 = 255;
					}
					if (num < 0)
					{
						num = 0;
					}
					if (num2 < 0)
					{
						num2 = 0;
					}
					if (num3 < 0)
					{
						num3 = 0;
					}
					bitmap.SetPixel(i, j, Color.FromArgb(pixel.A, num, num2, num3));
				}
			}
		}

		public static Bitmap CreateReferenceBitmap(Bitmap sourceBitmap, Color c1, Color c2, Color c3)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			int num = sourceBitmap.Width * sourceBitmap.Height;
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			int[] array = new int[num];
			int[] array2 = new int[3];
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
			Marshal.Copy(bitmapData.Scan0, array, 0, num);
			sourceBitmap.UnlockBits(bitmapData);
			Color[] refColors = new Color[3]
			{
				c1,
				c2,
				c3
			};
			int[] rgb = new int[3];
			for (int i = 0; i < num; i++)
			{
				Color color = Color.FromArgb(array[i]);
				int num4;
				int num3;
				int num2;
				int num5 = num4 = (num3 = (num2 = 0));
				if (color == c1)
				{
					num4 = 226;
					num3 = 0;
					num2 = 0;
					num5 = 255;
				}
				else if (color == c2)
				{
					num4 = 0;
					num3 = 226;
					num2 = 0;
					num5 = 255;
				}
				else if (color == c3)
				{
					num4 = 0;
					num3 = 0;
					num2 = 226;
					num5 = 255;
				}
				else if (UseColorCombination(refColors, color, ref rgb, useOneColor: true, array2))
				{
					num4 = rgb[0];
					num3 = rgb[1];
					num2 = rgb[2];
					num5 = 255;
				}
				if (num5 == 255)
				{
					array[i] = ((((((num5 << 8) | num4) << 8) | num3) << 8) | num2);
					continue;
				}
				num4 = color.R;
				num3 = color.G;
				num2 = color.B;
				num5 = 0;
				array[i] = ((((((num5 << 8) | num4) << 8) | num3) << 8) | num2);
			}
			for (int j = 0; j < num; j++)
			{
				Color tC = Color.FromArgb(array[j]);
				if (tC.A != 0)
				{
					continue;
				}
				int num6 = j / sourceBitmap.Width;
				int num7 = j - num6 * sourceBitmap.Width;
				array2[0] = (array2[1] = (array2[2] = 0));
				for (int k = -2; k <= 2; k++)
				{
					for (int l = -2; l <= 2; l++)
					{
						int num8 = num6 + k;
						int num9 = num7 + l;
						if (num8 < 0 || num8 >= sourceBitmap.Height || num9 < 0 || num9 >= sourceBitmap.Width)
						{
							continue;
						}
						Color color2 = Color.FromArgb(array[num8 * sourceBitmap.Width + num9]);
						if (color2.A != 0)
						{
							if (color2.R != 0)
							{
								array2[0]++;
							}
							else if (color2.G != 0)
							{
								array2[1]++;
							}
							else if (color2.B != 0)
							{
								array2[2]++;
							}
						}
					}
				}
				UseColorCombination(refColors, tC, ref rgb, useOneColor: false, array2);
				int num4 = rgb[0];
				int num3 = rgb[1];
				int num2 = rgb[2];
				int num5 = 255;
				array[j] = ((((((num5 << 8) | num4) << 8) | num3) << 8) | num2);
			}
			Bitmap bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);
			rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
			IntPtr scan = bitmapData2.Scan0;
			Marshal.Copy(array, 0, scan, num);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		public static Bitmap CreateReferenceBitmapPreservingAlpha(Bitmap sourceBitmap, Color c1, Color c2, Color c3)
		{
			if (sourceBitmap == null)
			{
				return null;
			}
			int num = sourceBitmap.Width * sourceBitmap.Height;
			Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			int[] array = new int[num];
			int[] array2 = new int[3];
			BitmapData bitmapData = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
			Marshal.Copy(bitmapData.Scan0, array, 0, num);
			sourceBitmap.UnlockBits(bitmapData);
			Color[] refColors = new Color[3]
			{
				c1,
				c2,
				c3
			};
			int[] rgb = new int[3];
			byte[] array3 = new byte[num];
			for (int i = 0; i < num; i++)
			{
				Color color = Color.FromArgb(array[i]);
				array3[i] = (byte)((array[i] & 4278190080u) >> 24);
				int num4;
				int num3;
				int num2;
				int num5 = num4 = (num3 = (num2 = 0));
				if (color == c1)
				{
					num4 = 226;
					num3 = 0;
					num2 = 0;
					num5 = 255;
				}
				else if (color == c2)
				{
					num4 = 0;
					num3 = 226;
					num2 = 0;
					num5 = 255;
				}
				else if (color == c3)
				{
					num4 = 0;
					num3 = 0;
					num2 = 226;
					num5 = 255;
				}
				else if (UseColorCombination(refColors, color, ref rgb, useOneColor: true, array2))
				{
					num4 = rgb[0];
					num3 = rgb[1];
					num2 = rgb[2];
					num5 = 255;
				}
				if (num5 == 255)
				{
					array[i] = ((((((array3[i] << 8) | num4) << 8) | num3) << 8) | num2);
					continue;
				}
				num4 = color.R;
				num3 = color.G;
				num2 = color.B;
				num5 = 0;
				array[i] = ((((((num5 << 8) | num4) << 8) | num3) << 8) | num2);
			}
			for (int j = 0; j < num; j++)
			{
				Color tC = Color.FromArgb(array[j]);
				if (tC.A != 0)
				{
					continue;
				}
				int num6 = j / sourceBitmap.Width;
				int num7 = j - num6 * sourceBitmap.Width;
				array2[0] = (array2[1] = (array2[2] = 0));
				for (int k = -2; k <= 2; k++)
				{
					for (int l = -2; l <= 2; l++)
					{
						int num8 = num6 + k;
						int num9 = num7 + l;
						if (num8 < 0 || num8 >= sourceBitmap.Height || num9 < 0 || num9 >= sourceBitmap.Width)
						{
							continue;
						}
						Color color2 = Color.FromArgb(array[num8 * sourceBitmap.Width + num9]);
						if (color2.A != 0)
						{
							if (color2.R != 0)
							{
								array2[0]++;
							}
							else if (color2.G != 0)
							{
								array2[1]++;
							}
							else if (color2.B != 0)
							{
								array2[2]++;
							}
						}
					}
				}
				UseColorCombination(refColors, tC, ref rgb, useOneColor: false, array2);
				int num4 = rgb[0];
				int num3 = rgb[1];
				int num2 = rgb[2];
				int num5 = 255;
				array[j] = ((((((array3[j] << 8) | num4) << 8) | num3) << 8) | num2);
			}
			Bitmap bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);
			rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
			IntPtr scan = bitmapData2.Scan0;
			Marshal.Copy(array, 0, scan, num);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		public static Bitmap CreateReferenceBitmapPreservingAlpha(Bitmap sourceBitmap, Color c1, Color c2, Color c3, bool preserveArmBand)
		{
			Color[,] array = new Color[48, 256];
			preserveArmBand = (preserveArmBand && sourceBitmap.Width == 1024 && sourceBitmap.Height == 1024);
			if (preserveArmBand)
			{
				int num = 0;
				int num2 = 0;
				for (int i = 975; i <= 1022; i++)
				{
					num2 = 0;
					for (int j = 384; j <= 639; j++)
					{
						array[num, num2++] = sourceBitmap.GetPixel(j, i);
					}
					num++;
				}
			}
			Bitmap bitmap = CreateReferenceBitmapPreservingAlpha(sourceBitmap, c1, c2, c3);
			if (bitmap != null && preserveArmBand)
			{
				int num3 = 0;
				int num4 = 0;
				for (int k = 975; k <= 1022; k++)
				{
					num4 = 0;
					for (int l = 384; l <= 639; l++)
					{
						bitmap.SetPixel(l, k, array[num3, num4++]);
					}
					num3++;
				}
			}
			return bitmap;
		}

		public static Bitmap CreateReferenceBitmap(Bitmap sourceBitmap, Color c1, Color c2, Color c3, bool preserveArmBand)
		{
			Color[,] array = new Color[48, 256];
			preserveArmBand = (preserveArmBand && sourceBitmap.Width == 1024 && sourceBitmap.Height == 1024);
			if (preserveArmBand)
			{
				int num = 0;
				int num2 = 0;
				for (int i = 975; i <= 1022; i++)
				{
					num2 = 0;
					for (int j = 384; j <= 639; j++)
					{
						array[num, num2++] = sourceBitmap.GetPixel(j, i);
					}
					num++;
				}
			}
			Bitmap bitmap = CreateReferenceBitmap(sourceBitmap, c1, c2, c3);
			if (bitmap != null && preserveArmBand)
			{
				int num3 = 0;
				int num4 = 0;
				for (int k = 975; k <= 1022; k++)
				{
					num4 = 0;
					for (int l = 384; l <= 639; l++)
					{
						bitmap.SetPixel(l, k, array[num3, num4++]);
					}
					num3++;
				}
			}
			return bitmap;
		}

		private static bool UseColorCombination(Color[] refColors, Color tC, ref int[] rgb, bool useOneColor, int[] hist)
		{
			int[] array = new int[3];
			rgb[0] = (rgb[1] = (rgb[2] = 0));
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			if (hist[0] + hist[1] + hist[2] != 0)
			{
				if (hist[0] > 0 && hist[1] == 0 && hist[2] == 0)
				{
					rgb[0] = UseOneColor(refColors[0], tC);
					return true;
				}
				if (hist[1] > 0 && hist[0] == 0 && hist[2] == 0)
				{
					rgb[1] = UseOneColor(refColors[1], tC);
					return true;
				}
				if (hist[2] > 0 && hist[0] == 0 && hist[1] == 0)
				{
					rgb[2] = UseOneColor(refColors[2], tC);
					return true;
				}
				if (!useOneColor)
				{
					if (hist[0] >= hist[2] && hist[1] >= hist[2])
					{
						if (UseTwoColors(refColors[0], refColors[1], tC, out rgb[0], out rgb[1]))
						{
							return true;
						}
					}
					else if (hist[0] >= hist[1] && hist[2] >= hist[1])
					{
						if (UseTwoColors(refColors[0], refColors[2], tC, out rgb[0], out rgb[2]))
						{
							return true;
						}
					}
					else if (hist[1] >= hist[0] && hist[2] >= hist[0] && UseTwoColors(refColors[1], refColors[2], tC, out rgb[1], out rgb[2]))
					{
						return true;
					}
				}
			}
			for (int i = 0; i < 3; i++)
			{
				rgb[i] = 0;
				array[i] = (tC.R - refColors[i].R) * (tC.R - refColors[i].R) + (tC.G - refColors[i].G) * (tC.G - refColors[i].G) + (tC.B - refColors[i].B) * (tC.B - refColors[i].B);
				if (refColors[i].A == 0)
				{
					array[i] = int.MaxValue;
				}
			}
			if (array[0] <= array[1] && array[0] <= array[2])
			{
				num = 0;
				if (array[1] < array[2])
				{
					num2 = 1;
					num3 = 2;
				}
				else
				{
					num2 = 2;
					num3 = 1;
				}
			}
			else if (array[1] <= array[0] && array[1] <= array[2])
			{
				num = 1;
				if (array[0] < array[2])
				{
					num2 = 0;
					num3 = 2;
				}
				else
				{
					num2 = 2;
					num3 = 0;
				}
			}
			else
			{
				num = 2;
				if (array[0] < array[1])
				{
					num2 = 0;
					num3 = 1;
				}
				else
				{
					num2 = 1;
					num3 = 0;
				}
			}
			if (array[num] * 8 < array[num2])
			{
				rgb[num] = UseOneColor(refColors[num], tC);
				return true;
			}
			if (useOneColor)
			{
				return false;
			}
			if (array[num] * 8 > array[num2] && UseTwoColors(refColors[num], refColors[num2], tC, out rgb[num], out rgb[num2]))
			{
				return true;
			}
			if (array[num] * 8 > array[num3] && UseTwoColors(refColors[num], refColors[num3], tC, out rgb[num], out rgb[num3]))
			{
				return true;
			}
			rgb[num] = UseOneColor(refColors[num], tC);
			return true;
		}

		private static int UseOneColor(Color refColor, Color targetColor)
		{
			int num = (refColor.R + refColor.G + refColor.B) / 3;
			int num2 = (targetColor.R + targetColor.G + targetColor.B) / 3;
			int num3 = (num2 <= num) ? (226 * num2 / num) : (226 + 30 * (num2 - num) / (255 - num));
			if (num3 < 0)
			{
				num3 = 0;
			}
			if (num3 > 255)
			{
				num3 = 255;
			}
			return num3;
		}

		private static bool UseTwoColors(Color c1, Color c2, Color tc, out int w1, out int w2)
		{
			int num = Math.Abs(c1.R - c2.R);
			int num2 = Math.Abs(c1.G - c2.G);
			int num3 = Math.Abs(c1.B - c2.B);
			if (num >= num2 && num >= num3)
			{
				if (c1.R < c2.R)
				{
					w1 = (c2.R - tc.R) * 226 / num;
					w2 = (tc.R - c1.R) * 226 / num;
				}
				else
				{
					w1 = (tc.R - c2.R) * 226 / num;
					w2 = (c1.R - tc.R) * 226 / num;
				}
			}
			else if (num2 >= num && num2 >= num3)
			{
				if (c1.G < c2.G)
				{
					w1 = (c2.G - tc.G) * 226 / num2;
					w2 = (tc.G - c1.G) * 226 / num2;
				}
				else
				{
					w1 = (tc.G - c2.G) * 226 / num2;
					w2 = (c1.G - tc.G) * 226 / num2;
				}
			}
			else if (c1.B < c2.B)
			{
				w1 = (c2.B - tc.B) * 226 / num3;
				w2 = (tc.B - c1.B) * 226 / num3;
			}
			else
			{
				w1 = (tc.B - c2.B) * 226 / num3;
				w2 = (c1.B - tc.B) * 226 / num3;
			}
			if (w1 < 0)
			{
				w1 = 0;
			}
			if (w1 > 255)
			{
				w1 = 255;
			}
			if (w2 < 0)
			{
				w2 = 0;
			}
			if (w2 > 255)
			{
				w2 = 255;
			}
			if (w1 >= 0)
			{
				return w2 >= 0;
			}
			return false;
		}
	}
}
