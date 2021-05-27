using FrostySdk;
using FrostySdk.Frosty;
using FrostySdk.IO;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrostySdk
{
	public class FrostbiteMod : IFrostbiteMod
	{
		public static ulong Magic = 72155812747760198uL;

		//public static uint Version = 3u;
		public static uint Version = 4u;

		/// <summary>
		/// Upgraded to 8u to support FIFA Editor Tool Versioning
		/// </summary>
		//public static uint Version = 8u;


		private string path;


		private int gameVersion;

		private List<string> warnings = new List<string>();

		private bool bNewFormat;


		public FrostbiteModDetails ModDetails { get; set; }

		public string Path => path;


		public int GameVersion => gameVersion;

		public IEnumerable<string> Warnings => warnings;

		public bool HasWarnings => warnings.Count != 0;

		public bool NewFormat => bNewFormat;


        public string Filename { get; set; }
        public IEnumerable<BaseModResource> Resources { get; set; }

		public FrostbiteMod(string inFilename)
		{
			FileInfo fileInfo = new FileInfo(inFilename);
			Filename = fileInfo.Name;
			path = inFilename;
			using (var fs = new FileStream(path, FileMode.Open))
			{
				ReadFromStream(fs);
			}
		}

		public FrostbiteMod(Stream stream)
		{
			ReadFromStream(stream);
		}

		public bool IsEncrypted { get; set; }

		public byte[] ModBytes { get; set; }

		private void ReadFromStream(Stream stream)
		{
			// Read initial bytes
			stream.Position = 0;
			ModBytes = new NativeReader(stream).ReadToEnd();
			stream.Position = 0;
			
			// Check for Zip Encryption
			bool IsEncrypted = new NativeReader(stream).ReadShort() == 1;
			if (IsEncrypted)
			{ 
				var m = new MemoryStream(ModBytes, 2, ModBytes.Length-2);
				using (ZipFile zipFileReader = ZipFile.Read(m))
				{
					var entry = zipFileReader.Entries.First();
					var entryStream = new MemoryStream();
					entry.Extract(entryStream);
					entryStream.Position = 0;
					ModBytes = new NativeReader(entryStream).ReadToEnd();
				}
			}

			// Read internal Bytes
			var innerStream = new MemoryStream(ModBytes);
			using (FrostbiteModReader frostyModReader = new FrostbiteModReader(innerStream))
			{
				if (frostyModReader.IsValid)
				{
					bNewFormat = true;
					gameVersion = frostyModReader.GameVersion;
					ModDetails = frostyModReader.ReadModDetails();
					Resources = frostyModReader.ReadResources();
					ModDetails.SetIcon(frostyModReader.GetResourceData(Resources.First()));
					for (int i = 0; i < 4; i++)
					{
						byte[] resourceData = frostyModReader.GetResourceData(Resources.ElementAt(i + 1));
						if (resourceData != null)
						{
							ModDetails.AddScreenshot(resourceData);
						}
					}
				}
			}
		}

		public byte[] GetResourceData(BaseModResource resource)
		{
			if(ModBytes != null && ModBytes.Length > 0)
            {
				return GetResourceData(resource, new MemoryStream(ModBytes));
			}
			using (FrostbiteModReader frostyModReader = new FrostbiteModReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
			{
				return frostyModReader.GetResourceData(resource);
			}
		}

		public byte[] GetResourceData(BaseModResource resource, Stream stream)
		{
			stream.Position = 0;
			FrostbiteModReader frostyModReader = new FrostbiteModReader(stream);
			{
				return frostyModReader.GetResourceData(resource);
			}
		}

		public void AddWarning(string warning)
		{
			warnings.Add(warning);
		}

        public override string ToString()
        {
			if(ModDetails != null)
            {
				return ModDetails.Title;
            }
            return base.ToString();
        }

        public void Dispose()
        {
			ModDetails = null;
			ModBytes = null;
		}
    }
}