using System;
using System.Collections.Generic;
using System.IO;

namespace FrostySdk
{
	public class FrostbiteModDetails
	{
		private string title;

		private string author;

		private string version;

		private string description;

		private string category;

		public string Title => title;

		public string Author => author;

		public string Version => version;

		public string Description => description;

		public int ScreenshotsCount { get; set; }

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

		public int EmbeddedFileCount { get; set; }

		public FrostbiteModDetails(string inTitle, string inAuthor, string inCategory, string inVersion, string inDescription)
		{
			title = inTitle;
			author = inAuthor;
			version = inVersion;
			description = inDescription;
			category = inCategory;
		}

		public FrostbiteModDetails(string inTitle, string inAuthor, string inCategory, string inVersion, string inDescription, int embeddedFileCount)
		{
			title = inTitle;
			author = inAuthor;
			version = inVersion;
			description = inDescription;
			category = inCategory;
			EmbeddedFileCount = embeddedFileCount;
		}

		public void SetIcon(byte[] buffer)
		{
		}

		public void AddScreenshot(byte[] buffer)
		{
		}

		public override int GetHashCode()
		{
            return Convert.ToInt32((((((-212883103L * 16777619L) ^ title.GetHashCode()) * 16777619) ^ author.GetHashCode()) * 16777619) ^ version.GetHashCode());
		}

	}
}
