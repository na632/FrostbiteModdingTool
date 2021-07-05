using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging; 


namespace UsefulThings
{
    /// <summary>
    /// General C# helpers.
    /// </summary>
    public static class General
    {
        #region DPI
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        /// <summary>
        /// Gets mouse pointer location relative to top left of monitor, scaling for DPI as required.
        /// </summary>
        /// <param name="relative">Window on monitor.</param>
        /// <returns>Mouse location scaled for DPI.</returns>
        //public static Point GetDPIAwareMouseLocation(Window relative)
        //{
        //    Win32Point w32Mouse = new Win32Point();
        //    GetCursorPos(ref w32Mouse);

        //    var scale = UsefulThings.General.GetDPIScalingFactorFOR_CURRENT_MONITOR(relative);
        //    Point location = new Point(w32Mouse.X / scale, w32Mouse.Y / scale);
        //    return location;
        //}

        /// <summary>
        /// Gets DPI scaling factor for main monitor from registry keys. 
        /// Returns 1 if key is unavailable.
        /// </summary>
        /// <returns>Returns scale or 1 if not found.</returns>
        //public static double GetDPIScalingFactorFROM_REGISTRY()
        //{
        //    var currentDPI = (int)(Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop\\WindowMetrics", "AppliedDPI", 96) ?? 96);
        //    return currentDPI / 96.0;
        //}


        /// <summary>
        /// Gets DPI Scaling factor for monitor app is currently on. 
        /// NOT actual DPI, the scaling factor relative to standard 96 DPI.
        /// </summary>
        /// <param name="current">Main window to get DPI for.</param>
        /// <returns>DPI scaling factor.</returns>
        //public static double GetDPIScalingFactorFOR_CURRENT_MONITOR(Window current)
        //{
        //    PresentationSource source = PresentationSource.FromVisual(current);
        //    Matrix m = source.CompositionTarget.TransformToDevice;
        //    return m.M11;
        //}

        /// <summary>
        /// Returns actual DPI of given visual object. Application DPI is constant across it's visuals.
        /// </summary>
        /// <param name="anyVisual">Any visual from the Application UI to test.</param>
        /// <returns>DPI of Application.</returns>
        //public static int GetAbsoluteDPI(Visual anyVisual)
        //{
        //    PresentationSource source = PresentationSource.FromVisual(anyVisual);
        //    if (source != null)
        //        return (int)(96.0 * source.CompositionTarget.TransformToDevice.M11);

        //    return 96;
        //}
        #endregion DPI


        /// <summary>
        /// Gets version of assembly calling this function.
        /// </summary>
        /// <returns>String of assembly version.</returns>
        public static string GetCallingVersion()
        {
            return Assembly.GetCallingAssembly().GetName().Version.ToString();
        }


        /// <summary>
        /// Gets version of main assembly that started this process.
        /// </summary>
        /// <returns></returns>
        public static string GetStartingVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }


        /// <summary>
        /// Gets location of assembly calling this function.
        /// </summary>
        /// <returns>Path to location.</returns>
        public static string GetExecutingLoc()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        /// <summary>
        /// Determines if number is a power of 2. 
        /// </summary>
        /// <param name="number">Number to check.</param>
        /// <returns>True if number is a power of 2.</returns>
        public static bool IsPowerOfTwo(int number)
        {
            return (number & (number - 1)) == 0;
        }


        /// <summary>
        /// Determines if number is a power of 2. 
        /// </summary>
        /// <param name="number">Number to check.</param>
        /// <returns>True if number is a power of 2.</returns>
        public static bool IsPowerOfTwo(long number)
        {
            return (number & (number - 1)) == 0;
        }

        /// <summary>
        /// Rounds number to the nearest power of 2. Doesn't use Math. Uses bitshifting (not my method).
        /// </summary>
        /// <param name="number">Number to round.</param>
        /// <returns>Nearest power of 2.</returns>
        public static int RoundToNearestPowerOfTwo(int number)
        {
            // KFreon: Gets next Highest power
            int next = number - 1;
            next |= next >> 1;
            next |= next >> 2;
            next |= next >> 4;
            next |= next >> 8;
            next |= next >> 16;
            next++;

            // KFreon: Compare previous and next for the closest
            int prev = next >> 1;
            return number - prev > next - number ? next : prev;
        }

        public static string FindValidNewFileName(string baseName)
        {

            if (!File.Exists(baseName))
                return baseName;

            int count = 1;
            string ext = Path.GetExtension(baseName);
            string pathWithoutExtension = GetFullPathWithoutExtension(baseName);

            // Detect if a similar path was provided i.e. <path>_#.ext - Remove the _# and start incrementation at #.
            char last = pathWithoutExtension.Last();
            if (pathWithoutExtension[pathWithoutExtension.Length - 2] == '_' && last.IsDigit())
            {
                count = int.Parse(last + "");
                pathWithoutExtension = pathWithoutExtension.Substring(0, pathWithoutExtension.Length - 2);
            }

            string tempName = pathWithoutExtension;
            while (File.Exists(tempName + ext))
            {
                tempName = pathWithoutExtension;
                tempName += "_" + count;
                count++;
            }

            return tempName + ext;
        }

        /// <summary>
        /// Gets a full file path (not just the name) without the file extension.
        /// </summary>
        /// <param name="fullPath">Full path to remove extension from.</param>
        /// <returns>File path without extension.</returns>
        public static string GetFullPathWithoutExtension(string fullPath)
        {
            return fullPath.Substring(0, fullPath.LastIndexOf('.'));
        }

        /// <summary>
        /// Determines if string is a number.
        /// </summary>
        /// <param name="str">String to check.</param>
        /// <returns>True if string is a number.</returns>
        public static bool IsDigit(this string str)
        {
            int res = -1;
            return Int32.TryParse(str, out res);
        }

        /// <summary>
        /// Determines if character is a number.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns>True if c is a number.</returns>
        public static bool IsDigit(this char c)
        {
            return ("" + c).IsDigit();
        }


        /// <summary>
        /// Determines if character is a letter.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsLetter(this char c)
        {
            return !c.IsDigit();
        }


        /// <summary>
        /// Determines if string is a letter.
        /// </summary>
        /// <param name="str">String to check.</param>
        /// <returns>True if str is a letter.</returns>
        public static bool IsLetter(this string str)
        {
            if (str.Length == 1)
                return !str.IsDigit();

            return false;
        }

        /// <summary>
        /// Creates a WriteableBitmap from an array of pixels.
        /// </summary>
        /// <param name="pixels">Pixel data</param>
        /// <param name="width">Width of image</param>
        /// <param name="height">Height of image</param>
        /// <param name="format">Defines how pixels are layed out.</param>
        /// <returns>WriteableBitmap containing pixels</returns>
        public static WriteableBitmap CreateWriteableBitmap(Array pixels, int width, int height, PixelFormat format)
        {
            WriteableBitmap wb = new WriteableBitmap(width, height, 96, 96, format, BitmapPalettes.Halftone256Transparent);
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, wb.BackBufferStride, 0);
            return wb;
        }

        /// <summary>
        /// Creates a WriteableBitmap from an array of pixels with the default BGRA32 pixel format.
        /// </summary>
        /// <param name="pixels">Pixel data</param>
        /// <param name="width">Width of image</param>
        /// <param name="height">Height of image</param>
        /// <returns>WriteableBitmap containing pixels</returns>
        public static WriteableBitmap CreateWriteableBitmap(Array pixels, int width, int height)
        {
            WriteableBitmap wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256Transparent);
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, wb.BackBufferStride, 0);
            return wb;
        }


        /// <summary>
        /// Creates a WPF style Bitmap (i.e. not using the System.Drawing.Bitmap)
        /// </summary>
        /// <param name="source">Stream containing bitmap data. NOTE fully formatted bitmap file, not just data.</param>
        /// <param name="cacheOption">Determines how/when image data is cached. Default is "Cache to memory on load."</param>
        /// <param name="decodeWidth">Specifies width to decode to. Aspect ratio preserved if only this set.</param>
        /// <param name="decodeHeight">Specifies height to decode to. Aspect ratio preserved if only this set.</param>
        /// <param name="DisposeStream">True = dispose of parent stream.</param>
        /// <returns>Bitmap from stream.</returns>
        public static BitmapImage CreateWPFBitmap(Stream source, int decodeWidth = 0, int decodeHeight = 0, BitmapCacheOption cacheOption = BitmapCacheOption.OnLoad, bool DisposeStream = false)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.DecodePixelWidth = decodeWidth;
            bmp.DecodePixelHeight = decodeHeight;

            // KFreon: Rewind stream to start
            source.Seek(0, SeekOrigin.Begin);
            bmp.StreamSource = source;
            bmp.CacheOption = cacheOption;
            bmp.EndInit();
            bmp.Freeze();  // Allows use across threads somehow (seals memory I'd guess)

            if (DisposeStream)
                source.Dispose();

            return bmp;
        }


        /// <summary>
        /// Creates WPF Bitmap from byte array.
        /// </summary>
        /// <param name="source">Fully formatted bitmap in byte[]</param>
        /// <param name="decodeWidth">Specifies width to decode to. Aspect ratio preserved if only this set.</param>
        /// <param name="decodeHeight">Specifies height to decode to. Aspect ratio preserved if only this set.</param>
        /// <returns>BitmapImage object.</returns>
        public static BitmapImage CreateWPFBitmap(byte[] source, int decodeWidth = 0, int decodeHeight = 0)
        {
            using (MemoryStream ms = new MemoryStream(source))
                return CreateWPFBitmap(ms, decodeWidth, decodeHeight);
        }


        /// <summary>
        /// Creates WPF Bitmap from List of bytes.
        /// </summary>
        /// <param name="source">Fully formatted bitmap in List of bytes.</param>
        /// <param name="decodeWidth">Specifies width to decode to. Aspect ratio preserved if only this set.</param>
        /// <param name="decodeHeight">Specifies height to decode to. Aspect ratio preserved if only this set.</param>
        /// <returns>BitmapImage of source data.</returns>
        public static BitmapImage CreateWPFBitmap(List<byte> source, int decodeWidth = 0, int decodeHeight = 0)
        {
            //byte[] newsource = source.ToArray(source.Count);
            byte[] newsource = source.ToArray();
            return CreateWPFBitmap(newsource, decodeWidth, decodeHeight);
        }


        /// <summary>
        /// Creates WPF Bitmap from a file.
        /// </summary>
        /// <param name="Filename">Path to file.</param>
        /// <param name="decodeWidth">Specifies width to decode to. Aspect ratio preserved if only this set.</param>
        /// <param name="decodeHeight">Specifies height to decode to. Aspect ratio preserved if only this set.</param>
        /// <returns>BitmapImage based on file.</returns>
        public static BitmapImage CreateWPFBitmap(string Filename, int decodeWidth = 0, int decodeHeight = 0)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.DecodePixelWidth = decodeWidth;
            bmp.DecodePixelHeight = decodeHeight;
            bmp.UriSource = new Uri(Filename);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }


        /// <summary>
        /// Creates WPF BitmapSource from a GDI Bitmap.
        /// Bitmap MUST STAY ALIVE for the life of this BitmapSource.
        /// </summary>
        /// <param name="GDIBitmap">Bitmap to convert.</param>
        /// <returns>BitmapSource of GDIBitmap</returns>
        public static BitmapSource CreateWPFBitmap(System.Drawing.Bitmap GDIBitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(GDIBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }


        /// <summary>
        /// Creates a WPF bitmap from another BitmapSource.
        /// </summary>
        /// <param name="source">Image source to create from.</param>
        /// <param name="decodeWidth">Width to decode to.</param>
        /// <param name="decodeHeight">Height to decode to.</param>
        /// <returns>BitmapImage of source</returns>
        public static BitmapImage CreateWPFBitmap(BitmapSource source, int decodeWidth = 0, int decodeHeight = 0)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            MemoryStream ms = new MemoryStream();

            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(ms);

            return CreateWPFBitmap(ms, decodeWidth, decodeHeight, DisposeStream: true);
        }

        /// <summary>
        /// Gets pixels of image as byte[] formatted as BGRA32.
        /// </summary>
        /// <param name="bmp">Bitmap to extract pixels from. Can be any supported pixel format.</param>
        /// <returns>Pixels as BGRA32.</returns>
        public static byte[] GetPixelsAsBGRA32(this BitmapSource bmp)
        {
            // KFreon: Read pixel data from image.
            int size = (int)(4 * bmp.PixelWidth * bmp.PixelHeight);
            byte[] pixels = new byte[size];
            BitmapSource source = bmp;

            // Convert if required.
            if (bmp.Format != PixelFormats.Bgra32)
            {
                Debug.WriteLine($"Getting pixels as BGRA32 required conversion from: {bmp.Format}.");
                bmp = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, BitmapPalettes.Halftone256Transparent, 0);
            }

            int stride = bmp.PixelWidth * (bmp.Format.BitsPerPixel / 8);
            bmp.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        public static string GetEnumDescription(Enum theEnum)
        {
            if (theEnum == null)
                return null;

            FieldInfo info = theEnum.GetType().GetField(theEnum.ToString());
            object[] attribs = info.GetCustomAttributes(false);
            if (attribs.Length == 0)
                return theEnum.ToString();
            else
                return (attribs[0] as DescriptionAttribute)?.Description;
        }

        public static byte[] ReadBytes(this Stream stream, int Length)
        {
            byte[] bytes = new byte[Length];
            stream.Read(bytes, 0, Length);
            return bytes;
        }
        public static string StringifyObject(object obj, int level = 0, string propName = null)
        {
            var propertyList = TypeDescriptor.GetProperties(obj);
            StringBuilder sb = new StringBuilder();
            var classname = TypeDescriptor.GetClassName(obj);
            string tags = new string(Enumerable.Repeat('-', level * 3).ToArray());
            string spacing = new string(Enumerable.Repeat(' ', level * 3).ToArray());

            if (propertyList.Count == 0)
                return spacing + $"{propName} = {obj}";

            sb.AppendLine($"{tags} {classname} {tags}");
            foreach (PropertyDescriptor descriptor in propertyList)
                sb.AppendLine(spacing + StringifyObject(descriptor.GetValue(obj), level + 1, descriptor.Name));

            sb.AppendLine($"{tags} END {classname} {tags}");


            return sb.ToString();
        }


    }


}