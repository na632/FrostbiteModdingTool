using FrostySdk;
using FrostySdk.IO;
using System.Collections.Generic;
using System.IO;

namespace Frosty.ModSupport
{
	public class FrostyMod
	{
		public static ulong Magic = 72155812747760198uL;

		public static uint Version = 3u;

		private string filename;

		private string path;

		private FrostyModDetails modDetails;

		private int gameVersion;

		private List<string> warnings = new List<string>();

		private bool bNewFormat;

		private BaseModResource[] resources;

		public FrostyModDetails ModDetails => modDetails;

		public string Path => path;

		public string Filename => filename;

		public int GameVersion => gameVersion;

		public IEnumerable<string> Warnings => warnings;

		public bool HasWarnings => warnings.Count != 0;

		public bool NewFormat => bNewFormat;

		public IEnumerable<BaseModResource> Resources => resources;

		public FrostyMod(string inFilename, DbObject modObj)
		{
			FileInfo fileInfo = new FileInfo(inFilename);
			filename = fileInfo.Name;
			path = inFilename;
			modDetails = new FrostyModDetails(modObj.GetValue<string>("title"), modObj.GetValue<string>("author"), modObj.GetValue<string>("category"), modObj.GetValue<string>("version"), modObj.GetValue<string>("description"));
			gameVersion = modObj.GetValue("gameVersion", 0);
			DbObject value = modObj.GetValue<DbObject>("resources");
			//int value2 = modObj.GetValue("icon", -1);
			//if (value2 != -1)
			//{
			//	modDetails.SetIcon(GetResource(value, value2));
			//}
			//foreach (int item in modObj.GetValue<DbObject>("screenshots"))
			//{
			//	modDetails.AddScreenshot(GetResource(value, item));
			//}
		}

		public FrostyMod(string inFilename)
		{
			FileInfo fileInfo = new FileInfo(inFilename);
			filename = fileInfo.Name;
			path = inFilename;
            if (File.Exists(inFilename))
            {
                using (var fs = new FileStream(inFilename, FileMode.Open, FileAccess.Read))
                {
                    using (FrostyModReader frostyModReader = new FrostyModReader(fs))
                    {
                        if (frostyModReader.IsValid)
                        {
                            bNewFormat = true;
                            gameVersion = frostyModReader.GameVersion;
                            modDetails = frostyModReader.ReadModDetails();
                            resources = frostyModReader.ReadResources();
                            //modDetails.SetIcon(frostyModReader.GetResourceData(resources[0]));
                            //for (int i = 0; i < 4; i++)
                            //{
                            //    byte[] resourceData = frostyModReader.GetResourceData(resources[i + 1]);
                            //    if (resourceData != null)
                            //    {
                            //        //modDetails.AddScreenshot(resourceData);
                            //    }
                            //}
                        }
                    }
                }
            }
		}

		public byte[] GetResourceData(BaseModResource resource)
		{
			using (FrostyModReader frostyModReader = new FrostyModReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
			{
				return frostyModReader.GetResourceData(resource);
			}
		}

		public void AddWarning(string warning)
		{
			warnings.Add(warning);
		}

		private byte[] GetResource(DbObject resourceList, int resourceId)
		{
			byte[] result = null;
			DbObject dbObject = resourceList[resourceId] as DbObject;
			int value = dbObject.GetValue("archiveIndex", 0);
			FileInfo fileInfo = new FileInfo(new FileInfo(path).FullName.Replace(".fbmod", "_" + value.ToString("D2") + ".archive"));
			if (fileInfo.Exists)
			{
				using (NativeReader nativeReader = new NativeReader(new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read)))
				{
					nativeReader.Position = dbObject.GetValue("archiveOffset", 0L);
					return nativeReader.ReadBytes(dbObject.GetValue("uncompressedSize", 0));
				}
			}
			return result;
		}
	}
}
