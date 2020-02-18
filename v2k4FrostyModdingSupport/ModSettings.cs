namespace Frosty.ModSupport
{
	public class ModSettings
	{
		private string title = "";

		private string author = "";

		private string category = "";

		private string version = "";

		private string description = "";

		private byte[] iconData;

		private byte[][] screenshotData;

		private bool isDirty;

		public bool IsDirty => isDirty;

		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				if (!title.Equals(value))
				{
					title = value;
					isDirty = true;
				}
			}
		}

		public string Author
		{
			get
			{
				return author;
			}
			set
			{
				if (!author.Equals(value))
				{
					author = value;
					isDirty = true;
				}
			}
		}

		public string Category
		{
			get
			{
				return category;
			}
			set
			{
				if (!category.Equals(value))
				{
					category = value;
					isDirty = true;
				}
			}
		}

		public string Version
		{
			get
			{
				return version;
			}
			set
			{
				if (!version.Equals(value))
				{
					version = value;
					isDirty = true;
				}
			}
		}

		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				if (!description.Equals(value))
				{
					description = value;
					isDirty = true;
				}
			}
		}

		public byte[] Icon
		{
			get
			{
				return iconData;
			}
			set
			{
				iconData = value;
				isDirty = true;
			}
		}

		public ModSettings()
		{
			screenshotData = new byte[4][];
		}

		public void SetScreenshot(int index, byte[] buffer)
		{
			screenshotData[index] = buffer;
			isDirty = true;
		}

		public byte[] GetScreenshot(int index)
		{
			return screenshotData[index];
		}

		public void ClearDirtyFlag()
		{
			isDirty = false;
		}
	}
}
