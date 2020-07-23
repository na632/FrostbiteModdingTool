using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml.Serialization;
using v2k4FIFAModdingCL.Career.ObjectiveHelpers;
using v2k4FIFAModdingCL.Career.StoryAssetHelpers;
using System.Text;
using System.Web;
using v2k4FIFAModdingCL.CGFE;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using v2k4FIFAModding.Career;
using System.Windows;
using System.Threading;
using FrostySdk;
using FrostySdk.Managers;
using FIFAModdingUI;
using System.Diagnostics;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;
using FrostySdk.IO;
using System;
using FrostySdk.Ebx;
//using FifaLibrary;

namespace FIFAModdingTests
{
    [TestClass]
    public class FrostbiteDuplicationTests
    {
        [TestMethod]
        public void TestDuplicationOfHead()
        {
            ProjectManagement projectManagement = new ProjectManagement();
            FIFAInstanceSingleton.FIFARootPath = @"E:\Origin Games\FIFA 20";
            FIFAInstanceSingleton.FIFAVERSION = "FIFA20";
            var FrostyProject = projectManagement.StartNewProject();
            var ebxItems = FrostyProject.AssetManager.EnumerateEbx();
			//FrostyProject.FileSystem.AddManifestChunk(new ManifestChunkInfo() {  })

			foreach (var i in ebxItems.Where(x => x.Path.Contains("olivier_giroud")).OrderBy(x => x.Path).ThenBy(x => x.Filename))
            {
                Debug.WriteLine(i.Path);
                var ebx = FrostyProject.AssetManager.GetEbx(i);
				string newName = i.Path + "/" + i.Filename.Trim('/');
				newName = newName.Replace("178509", "223988");
				Debug.WriteLine(newName);
				if (ebx.RootObjects != null && ebx.Objects != null && ebx.RootObjects.Any() && ebx.Objects.Any())
				{
					var dup = DuplicateAsset(FrostyProject.AssetManager, i, ebx, newName, true, i.GetType());
					if(dup!=null)
                    {

                    }
				}
				 //223988 -- Ward from Exeter
				 //178509

				//FrostyProject.AssetManager.

			}
        }

		private EbxAssetEntry DuplicateAsset(AssetManager assetManager, EbxAssetEntry entry, EbxAsset ebx, string newName, bool createNew, Type newType)
		{
			EbxAsset dupAsset = null;
            if (createNew)
            {
                dupAsset = new EbxAsset(TypeLibrary.CreateObject(newType.Name));
            }
            else
            {
                using (EbxWriter ebxWriter = new EbxWriter(new MemoryStream(), EbxWriteFlags.IncludeTransient | EbxWriteFlags.DoNotSort))
				{
					ebxWriter.WriteAsset(ebx);
					using (EbxReader ebxReader = new EbxReader(new MemoryStream(((MemoryStream)ebxWriter.BaseStream).ToArray())))
					{
						dupAsset = ebxReader.ReadAsset();
					}
				}
			}
			dupAsset.SetFileGuid(Guid.NewGuid());
			dynamic rootObject = dupAsset.RootObject;
			if (rootObject != null)
			{
				rootObject.Name = newName;
				AssetClassGuid assetClassGuid = new AssetClassGuid(Utils.GenerateDeterministicGuid(ebx.Objects, (Type)rootObject.GetType(), ebx.FileGuid), -1);
				rootObject.SetInstanceGuid(assetClassGuid);
				EbxAssetEntry ebxAssetEntry = assetManager.AddEbx(rootObject.Name, dupAsset);
				ebxAssetEntry.AddBundles.AddRange(entry.EnumerateBundles());
				ebxAssetEntry.ModifiedEntry.DependentAssets.AddRange(dupAsset.Dependencies);
				return ebxAssetEntry;
			}
			return null;
		}
	}
}