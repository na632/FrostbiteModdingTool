using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIFA21Plugin
{
    public class TOCCasDataLoader
    {
		public TOCFile TOCFile;

		public TOCCasDataLoader(TOCFile tf)
        {
			TOCFile = tf;
        }

        public void Load(NativeReader nativeReader)
        {
			_ = nativeReader.Position;
			if (nativeReader.Position < nativeReader.Length)
			{

				//for (int sha1Index = 0; sha1Index < MetaData.ResCount; sha1Index++)
				//{
				//	var sha1 = nativeReader.ReadGuid();
				//	//var unk1 = nativeReader.ReadInt();
				//	//var unk2 = nativeReader.ReadInt();
				//}
				if (

					TOCFile.FileLocation.Contains(@"data\win32/contentlaunchsb")
					|| TOCFile.FileLocation.Contains(@"data\win32/contentsb")
					|| TOCFile.FileLocation.Contains(@"data\win32/globalsfull")

					// not neccessary
					//|| TOCFile.FileLocation.Contains(@"data\win32/careersba")
					//|| TOCFile.FileLocation.Contains(@"data\win32/ui")
					// adboards and stadiums
					//|| TOCFile.FileLocation.Contains(@"data\win32/worldssb")

					// globals wont load properly
					//|| FileLocation.Contains(@"data\win32/globals")
					)
				{
					//if(FileLocation.Contains(@"data\win32/"))
					//{ 
					AssetManager.Instance.logger.Log("Searching for CAS Data from " + TOCFile.FileLocation);
					Dictionary<string, List<CASBundle>> CASToBundles = new Dictionary<string, List<CASBundle>>();

					BoyerMoore casBinarySearcher = new BoyerMoore(new byte[] { 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00 });
					casBinarySearcher.SetPattern(new byte[] { 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20 });
					//var readerBeforeSearch = nativeReader.Position;

					var readerBeforeSearch = 0;
					nativeReader.Position = 0;
					//var positionInTOC = new List<int>();// casBinarySearcher.SearchAll(nativeReader.ReadToEnd());
					var dataToEnd = nativeReader.ReadToEnd();
					var positionInTOC = casBinarySearcher.SearchAll(dataToEnd);
					//casBinarySearcher.SetPattern(new byte[] { 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20 });
					//positionInTOC.AddRange(casBinarySearcher.SearchAll(dataToEnd));
					//positionInTOC.Clear();
					//if (FileLocation.Contains(@"globals"))
					//{
					//	positionInTOC.Clear();
					//	positionInTOC.Add(271748 - (int)readerBeforeSearch);
					//}
					foreach (var p in positionInTOC)
					{
						nativeReader.Position = p + readerBeforeSearch - 4;
						var totalityOffsetCount = nativeReader.ReadInt(Endian.Big);
						// 0x20 x 3
						_ = nativeReader.ReadInt();
						_ = nativeReader.ReadInt();
						_ = nativeReader.ReadInt();
						_ = nativeReader.ReadInt();

						_ = nativeReader.ReadByte();
						_ = nativeReader.ReadByte();
						var catalog = (int)nativeReader.ReadByte();
						var cas = (int)nativeReader.ReadByte();

						//_ = nativeReader.ReadInt();

						CASBundle bundle = new CASBundle();
						bundle.Catalog = catalog;
						bundle.Cas = cas;
						bundle.BundleOffset = nativeReader.ReadInt(Endian.Big);
						bundle.DataOffset = nativeReader.ReadInt(Endian.Big);
						for (var i = 1; i < totalityOffsetCount; i++)
						{
							bundle.TOCOffsets.Add(nativeReader.Position);
							bundle.Offsets.Add(nativeReader.ReadInt(Endian.Big));
							bundle.TOCSizes.Add(nativeReader.Position);
							bundle.Sizes.Add(nativeReader.ReadInt(Endian.Big));
						}
						if (catalog > 0 && AssetManager.Instance.fs.Catalogs.Count() > catalog)
						{
							var path = AssetManager.Instance.fs.ResolvePath(AssetManager.Instance.fs.GetFilePath(catalog, cas, false));
							if (!string.IsNullOrEmpty(path))
							{
								var lstBundles = new List<CASBundle>();
								if (CASToBundles.ContainsKey(path))
								{
									lstBundles = CASToBundles[path];
								}
								else
								{
									CASToBundles.Add(path, lstBundles);
								}

								lstBundles.Add(bundle);
								CASToBundles[path] = lstBundles;
							}
						}
					}

					if (CASToBundles.Count > 0)
					{
						AssetManager.Instance.logger.Log($"Found {CASToBundles.Count} CAS to Bundles");

						foreach (var cas2bundle in CASToBundles)
						{
							AssetManager.Instance.logger.Log($"Found {cas2bundle.Value.Count} Bundles in {cas2bundle.Key} loading...");

                            CASDataLoader casDataLoader = new CASDataLoader(TOCFile);
                            casDataLoader.Load(AssetManager.Instance, cas2bundle.Key, cas2bundle.Value);
                        }
                    }
				}


			}
		}
    }
}
