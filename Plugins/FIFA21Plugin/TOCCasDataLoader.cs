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

		//public void Load(NativeReader nativeReader)
		//{
		//	_ = nativeReader.Position;
		//	if (nativeReader.Position < nativeReader.Length)
		//	{

		//		//for (int sha1Index = 0; sha1Index < MetaData.ResCount; sha1Index++)
		//		//{
		//		//	var sha1 = nativeReader.ReadGuid();
		//		//	//var unk1 = nativeReader.ReadInt();
		//		//	//var unk2 = nativeReader.ReadInt();
		//		//}
		//		if (

		//			TOCFile.FileLocation.Contains(@"data\win32/contentlaunchsb")
		//			|| TOCFile.FileLocation.Contains(@"data\win32/contentsb")
		//			|| TOCFile.FileLocation.Contains(@"data\win32/globalsfull")

		//			// not neccessary
		//			//|| TOCFile.FileLocation.Contains(@"data\win32/careersba")
		//			|| TOCFile.FileLocation.Contains(@"data\win32/ui")
		//			// adboards and stadiums
		//			|| TOCFile.FileLocation.Contains(@"data\win32/worldssb")

		//			|| TOCFile.FileLocation.Contains(@"data\win32/globals")
		//			)
		//		{
		//			//if(FileLocation.Contains(@"data\win32/"))
		//			//{ 
		//			AssetManager.Instance.logger.Log("Searching for CAS Data from " + TOCFile.FileLocation);
		//			Dictionary<string, List<CASBundle>> CASToBundles = new Dictionary<string, List<CASBundle>>();

		//			BoyerMoore casBinarySearcher = new BoyerMoore(new byte[] { 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00 });
		//			casBinarySearcher.SetPattern(new byte[] { 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20 });
		//			var readerBeforeSearch = 0;
		//			nativeReader.Position = 0;
		//			var dataToEnd = nativeReader.ReadToEnd();
		//			var positionInTOC = casBinarySearcher.SearchAll(dataToEnd);

		//			foreach (var p in positionInTOC)
		//			{
		//				nativeReader.Position = p + readerBeforeSearch - 4;
		//				var totalityOffsetCount = nativeReader.ReadInt(Endian.Big);
		//				// 0x20 x 3
		//				var unk1 = nativeReader.ReadInt(Endian.Big);
		//				var unk2 = nativeReader.ReadInt(Endian.Big);
		//				var unk3 = nativeReader.ReadInt(Endian.Big);
		//				var unk4 = nativeReader.ReadInt(Endian.Big);

		//				var unk5 = nativeReader.ReadByte();
		//				var unk6 = nativeReader.ReadByte();
		//				var catalog = (int)nativeReader.ReadByte();
		//				var cas = (int)nativeReader.ReadByte();

		//				CASBundle bundle = new CASBundle();
		//				bundle.Catalog = catalog;
		//				bundle.Cas = cas;
		//				//bundle.TOCOffsets.Add(nativeReader.Position);
		//				bundle.BundleOffset = nativeReader.ReadInt(Endian.Big);
		//				//bundle.TOCSizes.Add(nativeReader.Position);
		//				bundle.BundleSize = nativeReader.ReadInt(Endian.Big);
		//				for (var i = 0; i < totalityOffsetCount - 1; i++)
		//				{
		//					bundle.TOCOffsets.Add(nativeReader.Position);
		//					bundle.Offsets.Add(nativeReader.ReadInt(Endian.Big));
		//					bundle.TOCSizes.Add(nativeReader.Position);
		//					bundle.Sizes.Add(nativeReader.ReadInt(Endian.Big));
		//				}
		//				if (catalog > 0 && AssetManager.Instance.fs.Catalogs.Count() > catalog)
		//				{
		//					//var path = AssetManager.Instance.fs.ResolvePath(AssetManager.Instance.fs.GetFilePath(catalog, cas, false));
		//					var path = AssetManager.Instance.fs.GetFilePath(catalog, cas, false);
		//					if (!string.IsNullOrEmpty(path))
		//					{
		//						var lstBundles = new List<CASBundle>();
		//						if (CASToBundles.ContainsKey(path))
		//						{
		//							lstBundles = CASToBundles[path];
		//						}
		//						else
		//						{
		//							CASToBundles.Add(path, lstBundles);
		//						}

		//						lstBundles.Add(bundle);
		//						CASToBundles[path] = lstBundles;
		//					}
		//				}
		//			}

		//			if (CASToBundles.Count > 0)
		//			{
		//				AssetManager.Instance.logger.Log($"Found {CASToBundles.Count} CAS to Bundles");

		//				foreach (var cas2bundle in CASToBundles)
		//				{
		//					AssetManager.Instance.logger.Log($"Found {cas2bundle.Value.Count} Bundles in {cas2bundle.Key} loading...");

		//					CASDataLoader casDataLoader = new CASDataLoader(TOCFile);
		//					casDataLoader.Load(AssetManager.Instance, cas2bundle.Key, cas2bundle.Value);
		//				}
		//			}
		//		}


		//	}
		//}

		public void Load2(NativeReader nativeReader)
		{
			_ = nativeReader.Position;
			if (nativeReader.Position < nativeReader.Length)
			{

			
				//if (
    //                //true
    //                TOCFile.NativeFileLocation.Contains(@"win32/contentlaunchsb")
    //                || TOCFile.NativeFileLocation.Contains(@"win32/contentsb")
    //                || TOCFile.NativeFileLocation.Contains(@"win32/globalsfull")

    //                // not neccessary
    //                //|| TOCFile.NativeFileLocation.Contains(@"win32/careersba")
    //                || TOCFile.NativeFileLocation.Contains(@"win32/ui")
    //                // adboards and stadiums
    //                || TOCFile.NativeFileLocation.Contains(@"win32/worldssb")

    //                || TOCFile.NativeFileLocation.Contains(@"win32/globals")
    //                )
				{
					List<CASBundle> bundles = new List<CASBundle>();

					AssetManager.Instance.logger.Log("Searching for CAS Data from " + TOCFile.FileLocation);
					for (int i = 0; i < TOCFile.MetaData.BundleCount; i++)
					{
						CASBundle bundle = new CASBundle();

						long startPosition = nativeReader.Position;
						int a = nativeReader.ReadInt32BigEndian();
						int b = nativeReader.ReadInt32BigEndian();
						int flagsOffset = nativeReader.ReadInt32BigEndian();
						int entriesCount = nativeReader.ReadInt32BigEndian();
						int entriesOffset = nativeReader.ReadInt32BigEndian();
						int num = nativeReader.ReadInt32BigEndian();
						int e = nativeReader.ReadInt32BigEndian();
						int f = nativeReader.ReadInt32BigEndian();
						bool isInPatch = false;
						byte catalog = 0;
						byte cas = 0;
						nativeReader.Position = startPosition + flagsOffset;
						bundle.Flags = nativeReader.ReadBytes(entriesCount);
						nativeReader.Position = startPosition + entriesOffset;
						for (int j2 = 0; j2 < entriesCount; j2++)
						{
							bool hasCasIdentifier = bundle.Flags[j2] == 1;
							if (hasCasIdentifier)
							{
								nativeReader.ReadByte();
								isInPatch = nativeReader.ReadBoolean();
								catalog = nativeReader.ReadByte();
								cas = nativeReader.ReadByte();
							}
							long locationOfOffset = nativeReader.Position;
							int bundleOffsetInCas = nativeReader.ReadInt32BigEndian();
							long locationOfSize = nativeReader.Position;
							int bundleSizeInCas = nativeReader.ReadInt32BigEndian();
							if (j2 == 0)
							{
								bundle.BundleOffset = bundleOffsetInCas;
								bundle.BundleSize = bundleSizeInCas;
								bundle.Cas = cas;
								bundle.Catalog = catalog;
								bundle.Patch = isInPatch;
							}
							else
							{
								if(cas != bundle.Cas)
								{ 
								}
								if (catalog != bundle.Catalog)
								{
								}
								if(isInPatch != bundle.Patch)
                                {
                                }
								bundle.TOCOffsets.Add(locationOfOffset);
								bundle.Offsets.Add(bundleOffsetInCas);
								//if (!bundle.TOCOffsetsToCAS.ContainsKey(bundleOffsetInCas))
								//{
								//	bundle.TOCOffsetsToCAS.Add(bundleOffsetInCas, cas);
								//}
								//if (!bundle.TOCOffsetsToCatalog.ContainsKey(bundleOffsetInCas))
								//{
								//	bundle.TOCOffsetsToCatalog.Add(bundleOffsetInCas, catalog);
								//}
								//if (!bundle.TOCOffsetsToPatch.ContainsKey(bundleOffsetInCas))
								//{
								//	bundle.TOCOffsetsToPatch.Add(bundleOffsetInCas, IsInPatch);
								//}
								bundle.TOCSizes.Add(locationOfSize);
								bundle.Sizes.Add(bundleSizeInCas);

                                //bundle.TOCCas.Add(cas);
                                //bundle.TOCCatalog.Add(catalog);
                                //bundle.TOCPatch.Add(isInPatch);
                            }
							//bundle.TOCCas.Add(cas);
							//bundle.TOCCatalog.Add(catalog);
							//bundle.TOCPatch.Add(isInPatch);
						}
						bundles.Add(bundle);
						nativeReader.Position = startPosition + flagsOffset + entriesCount;

					}

					Dictionary<string, List<CASBundle>> CASToBundles = new Dictionary<string, List<CASBundle>>();

					

					if (bundles.Count > 0)
					{
						AssetManager.Instance.logger.Log($"Found {bundles.Count} bundles for CasFiles");

						foreach (var bundle in bundles)
						{
							var path = AssetManager.Instance.fs.GetFilePath(bundle.Catalog, bundle.Cas, bundle.Patch);
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

						foreach (var ctb in CASToBundles)
						{
							CASDataLoader casDataLoader = new CASDataLoader(TOCFile);
							casDataLoader.Load(ctb.Key, ctb.Value);
						}
					}
				}
			}
		}
	}
}
