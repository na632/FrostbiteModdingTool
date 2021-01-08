using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace DDSReader
{
	public class DDSImage
	{
		public readonly Pfim.IImage _image;

		public byte[] Data
		{
			get
			{
				if (_image != null)
					return _image.Data;
				else
					return new byte[0];
			}
		}

		public DDSImage(string file)
		{
			_image = Pfim.Pfim.FromFile(file);
			Process();
		}

		public DDSImage(Stream stream)
		{
			if (stream == null)
				throw new Exception("DDSImage ctor: Stream is null");

			_image = Pfim.Dds.Create(stream, new Pfim.PfimConfig(targetFormat: Pfim.TargetFormat.Native));
			Process();
		}

		public DDSImage(byte[] data)
		{
			if (data == null || data.Length <= 0)
				throw new Exception("DDSImage ctor: no data");

			_image = Pfim.Dds.Create(data, new Pfim.PfimConfig(targetFormat: Pfim.TargetFormat.Native));
			Process();
		}

		public void Save(string file)
		{
			if (_image.Format == Pfim.ImageFormat.Rgba32)
			{
				Image<Bgra32> image = Image.LoadPixelData<Bgra32>(
				_image.Data, _image.Width, _image.Height);
				image.Save(file);
			}
			else if (_image.Format == Pfim.ImageFormat.Rgb24)
			{
				Image<Rgb24> image = Image.LoadPixelData<Rgb24>(
				_image.Data, _image.Width, _image.Height);
				image.Save(file);
			}
			else
				throw new Exception("Unsupported pixel format (" + _image.Format + ")");
		}

		public Stream SaveToStream()
        {
			var s = new MemoryStream();
			if (_image.Format == Pfim.ImageFormat.Rgba32)
			{
				Image<Bgra32> image = Image.LoadPixelData<Bgra32>(
				_image.Data, _image.Width, _image.Height);
				image.SaveAsBmp(s);
			}
			else if (_image.Format == Pfim.ImageFormat.Rgb24)
			{
				Image<Rgb24> image = Image.LoadPixelData<Rgb24>(
				_image.Data, _image.Width, _image.Height);
				image.SaveAsBmp(s);
			}
			s.Position = 0;
			return s;
		}

		private void Process()
		{
			if (_image == null)
				throw new Exception("DDSImage image creation failed");

			if (_image.Compressed)
				_image.Decompress();
		}

	}
}