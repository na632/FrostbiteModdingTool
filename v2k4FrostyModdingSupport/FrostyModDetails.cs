using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace Frosty.ModSupport
{
	public class FrostyModDetails
	{
		private string title;

		private string author;

		private string version;

		private string description;

		private string category;

		private ImageSource icon;

		private List<ImageSource> screenshots = new List<ImageSource>();

		public string Title => title;

		public string Author => author;

		public string Version => version;

		public string Description => description;

		public string Category
		{
			get
			{
				if (!(category == ""))
				{
					return category;
				}
				return "Misc";
			}
		}

		public ImageSource Icon => icon;

		public List<ImageSource> Screenshots => screenshots;

		public FrostyModDetails(string inTitle, string inAuthor, string inCategory, string inVersion, string inDescription)
		{
			title = inTitle;
			author = inAuthor;
			version = inVersion;
			description = inDescription;
			category = inCategory;
		}

		public void SetIcon(byte[] buffer)
		{
			//icon = LoadImage(buffer);
		}

		public void AddScreenshot(byte[] buffer)
		{
			//ImageSource item = LoadImage(buffer);
			//screenshots.Add(item);
		}

		public override int GetHashCode()
		{
            return Convert.ToInt32((((((-212883103L * 16777619L) ^ title.GetHashCode()) * 16777619) ^ author.GetHashCode()) * 16777619) ^ version.GetHashCode());
		}

		//private BitmapImage LoadImage(byte[] buffer)
		//{
		//	if (buffer == null || buffer.Length == 0)
		//	{
		//		return null;
		//	}
		//	BitmapImage bitmapImage = new BitmapImage();
		//	using (MemoryStream streamSource = new MemoryStream(buffer))
		//	{
		//		bitmapImage.BeginInit();
		//		bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
		//		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		//		bitmapImage.UriSource = null;
		//		bitmapImage.StreamSource = streamSource;
		//		bitmapImage.EndInit();
		//	}
		//	bitmapImage.Freeze();
		//	return bitmapImage;
		//}
	}
}
