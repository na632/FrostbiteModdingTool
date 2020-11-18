using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static FIFA21Plugin.FIFA21AssetLoader;

namespace FIFA21Plugin
{

	public class BundleEntryInfo
    {
		/// <summary>
		/// Is it a EBX, RES or Chunk
		/// </summary>
		public string Type { get; set; }
		public Guid? ChunkGuid { get; set; }
		public Sha1? Sha { get; set; }
		public string Name { get; set; }
		public long Offset { get; set; }
		public long Size { get; set; }
		public int Flag { get; set; }
		public long StringOffset { get; set; }
		public int Index { get; set; }
        public int CasIndex { get; internal set; }
        public int Offset2 { get; internal set; }
        public int OriginalSize { get; internal set; }

        public override string ToString()
        {
			var builtString = string.Empty;

			if (!string.IsNullOrEmpty(Type))
			{
				builtString += Type;
			}

			if (!string.IsNullOrEmpty(Name))
            {
				builtString += " " + Name;
            }
			
			if (Sha.HasValue)
			{
				builtString += " " + Sha.Value.ToString();
			}


			if (!string.IsNullOrEmpty(builtString))
			{
				builtString = base.ToString();
			}

			return builtString;


		}
    }

    public class TOCFile
    {
        public SBFile AssociatedSBFile { get; set; }
        public string FileLocation { get; internal set; }
        public string NativeFileLocation { get; internal set; }

        //public int[] ArrayOfInitialHeaderData = new int[12];

        public ContainerMetaData MetaData = new ContainerMetaData();
		public List<BaseBundleInfo> Bundles = new List<BaseBundleInfo>();

		public string SuperBundleName;

		private TocSbReader_FIFA21 ParentReader;
		public TOCFile(TocSbReader_FIFA21 parent)
        {
			ParentReader = parent;
        }

		public class ContainerMetaData
        {
			public int Magic { get; set; }
			public int CasIndexingLength { get; set; }
			public int BundleCount { get; set; }
			public int Offset1 { get; set; }
			public int Offset2 { get; set; }
			public int ResCount { get; set; }
			public int Offset4 { get; set; }
			public int Offset5 { get; set; }
			public int Offset6 { get; set; }
			public int Offset7 { get; set; }
			public int CountOfSomething { get; set; }
			public int CountOfSomething2 { get; set; }
			public int Offset9 { get; set; }
			public int Offset10 { get; set; }
			public int Offset11 { get; set; }

			public void Read(NativeReader nativeReader)
			{
				Magic = nativeReader.ReadInt(Endian.Big);
				CasIndexingLength = nativeReader.ReadInt(Endian.Big);
				BundleCount = nativeReader.ReadInt(Endian.Big);
				Offset1 = nativeReader.ReadInt(Endian.Big);
				Offset2 = nativeReader.ReadInt(Endian.Big);
				ResCount = nativeReader.ReadInt(Endian.Big);
				Offset4 = nativeReader.ReadInt(Endian.Big);
				Offset5 = nativeReader.ReadInt(Endian.Big);
				Offset6 = nativeReader.ReadInt(Endian.Big);
				Offset7 = nativeReader.ReadInt(Endian.Big);
				CountOfSomething = nativeReader.ReadInt(Endian.Big);
				
				
				// Testing
				CountOfSomething2 = nativeReader.ReadInt(Endian.Big);
				Offset9 = nativeReader.ReadInt(Endian.Big);
				Offset10 = nativeReader.ReadInt(Endian.Big);
				Offset11 = nativeReader.ReadInt(Endian.Big);
				nativeReader.Position -= 16;

                if (ListOfPositionChecks.Contains(Offset1))
                {

                }
				if (ListOfPositionChecks.Contains(Offset2))
				{

				}
				if (ListOfPositionChecks.Contains(Offset4))
				{

				}
				if (ListOfPositionChecks.Contains(Offset5))
				{

				}
				if (ListOfPositionChecks.Contains(Offset6))
				{

				}
				if (ListOfPositionChecks.Contains(Offset7))
				{

				}
				if (ListOfPositionChecks.Contains(CountOfSomething2))
				{

				}
				if (ListOfPositionChecks.Contains(Offset9))
				{

				}
				if (ListOfPositionChecks.Contains(Offset10))
				{

				}
				if (ListOfPositionChecks.Contains(Offset11))
				{

				}

				//BoyerMoore boyerMoore = new BoyerMoore(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x3C });
				//var findInternalPatterns = boyerMoore.SearchAll(nativeReader.ReadToEnd());

			}

			private List<int> ListOfPositionChecks
				= new List<int>() { 4118, 4733, 5361, 5659, 2268974, 445678803 };

        }

		public int[] tocMetaData = new int[15];

		public void Read(NativeReader nativeReader)
		{
			var startPosition = nativeReader.Position;
			if (File.Exists("debugToc.dat"))
				File.Delete("debugToc.dat");

			nativeReader.Position = 0;
			using (NativeWriter writer = new NativeWriter(new FileStream("debugToc.dat", FileMode.OpenOrCreate)))
			{
				writer.Write(nativeReader.ReadToEnd());
			}
			nativeReader.Position = 0;

            if (FileLocation.Contains("contentlaunchsb"))
            {
				// Manchester City CAS location is 1aa28887 (1A A2 88 87 in Endian.BIG)
				// Found this in Data / ContentLaunchSb TOC at Offset 2605292 / 27 c0 ec 00  ( 27c0ec00 in Endian.BIG | 00 EC C0 27 Endian.Little)
			}

			ParentReader.AssetManager.logger.Log("Seaching for Internal TOC Bundles");
			//var findInternalPatterns = FIFA21AssetLoader.SearchBytePattern(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x3C }, nativeReader.ReadToEnd());
			BoyerMoore boyerMoore = new BoyerMoore(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x3C });
			var findInternalPatterns = boyerMoore.SearchAll(nativeReader.ReadToEnd());
			nativeReader.Position = startPosition;
			ParentReader.AssetManager.logger.Log($"{findInternalPatterns.Count} Internal TOC Bundles found");

			foreach (var internalPos in findInternalPatterns)
			{
				var actualInternalPos = internalPos + 4;

				nativeReader.Position = actualInternalPos;
				var magic = nativeReader.ReadInt(Endian.Big);
				if (magic != 0x3c)
					throw new Exception("Magic is not the expected value of 0x3c");

				nativeReader.Position -= 4;

				MetaData.Read(nativeReader);
				////nativeReader.Position -= 12 * 4;

				////for (int tmdIndex = 0; tmdIndex < 12; tmdIndex++)
				////{
				////	tocMetaData[tmdIndex] = nativeReader.ReadInt(Endian.Big);
				////}

				List<int> bundleReferences = new List<int>();

				// Obviously not usable in standard way
				if (MetaData.BundleCount == 32)
				{
					//nativeReader.Position = 556 + MetaData.Offset6;
					//var newOffset = nativeReader.ReadInt(Endian.Big) + 556;
					//nativeReader.Position = newOffset;
					//var nextOffset = nativeReader.ReadInt(Endian.Big) + 556;

				}
				else
				{


					if (MetaData.BundleCount > 0 && MetaData.BundleCount != MetaData.CasIndexingLength)
					{
						//for (int num8 = 0; num8 < tocMetaData[2]; num8++)
						for (int index = 0; index < MetaData.BundleCount; index++)
						{
							bundleReferences.Add((int)nativeReader.ReadUInt(Endian.Big));
						}
						nativeReader.Position = actualInternalPos + MetaData.CasIndexingLength;

						/*
						handle.read(4)

			while handle.tell() % 8 != 0:
				handle.read(1)
						*/


						//if (FileLocation.Contains("fifagame"))
						//{

						//}
						//if (FileLocation.Contains("chants_preview"))
						//{

						//}
						//nativeReader.Position = 556 + tocMetaData[3] - ((tocMetaData[2] + 2) * 4);
						//for (int indexOfBundleCount = 0; indexOfBundleCount < tocMetaData[2]; indexOfBundleCount++)
						for (int indexOfBundleCount = 0; indexOfBundleCount < MetaData.BundleCount; indexOfBundleCount++)
						{
							/*
								string_off = unpack(">I", handle.read(4))[0]
								size = unpack(">I", handle.read(4))[0]
								handle.read(4)  # unknown
								offset = unpack(">I", handle.read(4))[0]
								name = ReadUtil.read_string_rewind(handle, offset6 + string_off)

							 */
							int string_off = nativeReader.ReadInt(Endian.Big);


							if (FileLocation.Contains("contentsb"))
							{

							}
							//var anthemSearcher = 556 + tocMetaData[8] - 48 + casStringOffset;
							//var anthemSearcher = 556 + tocMetaData[8] + casStringOffset;

							//var t = TocOffset + off;

							//if (t >= 2600000 && t <= 2605400)
							//{
							//	t += 50;
							//}
							int size = nativeReader.ReadInt(Endian.Big);

							nativeReader.ReadInt(Endian.Big); // unknown

							int dataOffset = nativeReader.ReadInt(Endian.Big);

							BaseBundleInfo newBundleInfo = new BaseBundleInfo
							{
								TocOffset = string_off,
								Offset = dataOffset,
								Size = size
							};
							Bundles.Add(newBundleInfo);
						}

						/*
						List<DbObject> resObjects = new List<DbObject>();

						if (MetaData.Offset2 > MetaData.MetaDataLength && MetaData.Offset2 > MetaData.Offset1)
						{

							// need to fix this part and then build res objects
							nativeReader.Position = MetaData.Offset2 + 556;
							// starts at 314572 for content launch sb (kits and faces)
							// this sequence seems to end at 814930 for content launch sb
							for (int indexOfRes = 0; indexOfRes < MetaData.ResCount; indexOfRes++)
							{
								DbObject resObject = new DbObject();
								resObject.AddValue("id", nativeReader.ReadGuid());
								resObject.AddValue("bit1", nativeReader.ReadBoolean());
								resObject.AddValue("bit2", nativeReader.ReadBoolean());
								resObject.AddValue("unk1", nativeReader.ReadUShort());

								resObjects.Add(resObject);
							}

							nativeReader.Position = MetaData.Offset5 + 556;
							for (int indexOfRes = 0; indexOfRes < MetaData.ResCount; indexOfRes++)
							{
								DbObject resObject = resObjects[indexOfRes];
								resObject.AddValue("unk3", nativeReader.ReadByte());
								resObject.AddValue("patch", nativeReader.ReadBoolean());
								resObject.AddValue("catalog", nativeReader.ReadByte());
								resObject.AddValue("cas", nativeReader.ReadByte());
								resObject.AddValue("offset", nativeReader.ReadInt(Endian.Big));
								resObject.AddValue("size", nativeReader.ReadInt(Endian.Big));

								var casPath = AssetManager.Instance.fs.GetFilePath(
									resObject.GetValue<int>("catalog")
									, resObject.GetValue<int>("cas")
									, resObject.GetValue<bool>("patch"));
								resObject.AddValue("casPath", casPath);
								if (casPath.Contains("/fifa_installpackage_03/cas_03.cas"))
								{

								}

								resObjects.Add(resObject);
							}
						}

						if (MetaData.Offset11 + 552 < nativeReader.Length)
						{
							nativeReader.Position = MetaData.Offset11 + 556;

							List<int> collection = new List<int>();
							for (var i = 0; i < MetaData.Offset10; i++)
							{
								collection.Add(nativeReader.ReadInt(Endian.Big));
							}

							nativeReader.Position += 4;
							List<int> collection2 = new List<int>();


							for (var i = 0; i < MetaData.Offset8; i++)
							{
								collection2.Add(nativeReader.ReadInt(Endian.Big));
							}
							for (var i = 0; i < MetaData.Offset9; i++)
							{
								collection2.Add(nativeReader.ReadInt(Endian.Big));
							}
							for (var i = 0; i < MetaData.ResCount; i++)
							{
								collection2.Add(nativeReader.ReadInt(Endian.Big));
							}
							for (var i = 0; i < MetaData.Sec4Size; i++)
							{
								collection2.Add(nativeReader.ReadInt(Endian.Big));
							}


						}
						*/
						var chunks = new List<ChunkAssetEntry>();

						if (MetaData.Offset1 != 0 && MetaData.Offset1 != 32)
						{
							if (MetaData.ResCount > 0)
							{
								nativeReader.Position = actualInternalPos + MetaData.Offset1;
								List<int> list7 = new List<int>();
								for (int num13 = 0; num13 < MetaData.ResCount; num13++)
								{
									list7.Add(nativeReader.ReadInt(Endian.Big));
								}
								nativeReader.Position = actualInternalPos + MetaData.Offset2;


								List<Guid> tocChunkGuids = new List<Guid>();
								for (int num14 = 0; num14 < MetaData.ResCount; num14++)
								{
									byte[] array6 = nativeReader.ReadBytes(16);
									Guid tocChunkGuid = new Guid(new byte[16]
									{
										array6[15],
										array6[14],
										array6[13],
										array6[12],
										array6[11],
										array6[10],
										array6[9],
										array6[8],
										array6[7],
										array6[6],
										array6[5],
										array6[4],
										array6[3],
										array6[2],
										array6[1],
										array6[0]
									});
									//Guid value2 = nativeReader.ReadGuid(Endian.Little);
									//if (tocChunkGuid == Guid.Parse("cc9e36b9-9304-2832-01ff-e8820db10773"))
									//{

									//}
									int num15 = nativeReader.ReadInt(Endian.Big) & 0xFFFFFF;
									while (tocChunkGuids.Count <= num15)
									{
										tocChunkGuids.Add(Guid.Empty);
									}
									tocChunkGuids[num15 / 3] = tocChunkGuid;
								}
								nativeReader.Position = actualInternalPos + MetaData.Offset4;


								ParentReader.AssetManager.logger.Log($"Found {MetaData.ResCount} Chunks in TOC");

								for (int num16 = 0; num16 < MetaData.ResCount; num16++)
								{
									ChunkAssetEntry chunkAssetEntry2 = new ChunkAssetEntry();
									 
									var unk2 = nativeReader.ReadByte();
									bool patch2 = nativeReader.ReadBoolean();
									byte catalog2 = nativeReader.ReadByte();
									byte cas2 = nativeReader.ReadByte();

									chunkAssetEntry2.SB_CAS_Offset_Position = (int)nativeReader.Position;
									uint chunkOffset = nativeReader.ReadUInt(Endian.Big);
									chunkAssetEntry2.SB_CAS_Size_Position = (int)nativeReader.Position;
									uint chunkSize = nativeReader.ReadUInt(Endian.Big);
									chunkAssetEntry2.Id = tocChunkGuids[num16];
                                    if (chunkAssetEntry2.Id.ToString() == "966d0ca0-144a-c788-3678-3bc050252ff5") // Thiago Test
                                    {
                                    }
                                    chunkAssetEntry2.TOCFileLocation = NativeFileLocation;
									chunkAssetEntry2.SBFileLocation = null;

									chunkAssetEntry2.Size = chunkSize;
									chunkAssetEntry2.Location = AssetDataLocation.CasNonIndexed;
									chunkAssetEntry2.ExtraData = new AssetExtraData();
									chunkAssetEntry2.ExtraData.CasPath = AssetManager.Instance.fs.GetFilePath(catalog2, cas2, patch2);
									if (chunkAssetEntry2.ExtraData.CasPath.Contains("/fifa_installpackage_03/cas_03.cas"))
									{


                                        //BoyerMoore boyerMoore1 = new BoyerMoore(chunkAssetEntry2.Id.ToByteArray());
										//using (FileStream fsCas = new FileStream(AssetManager.Instance.fs.ResolvePath(chunkAssetEntry2.ExtraData.CasPath), FileMode.Open))
										//using (NativeReader reader = new NativeReader(fsCas))
										//{
										//    var positionInCas = boyerMoore1.Search(reader.ReadToEnd());
										//}
          //                              BoyerMoore boyerMoore1 = new BoyerMoore(new byte[] { 0x1A, 0x90, 0x84, 0xD3 });
										//var readerBeforeSearch = nativeReader.Position;
										//var positionInTOC = boyerMoore1.SearchAll(nativeReader.ReadToEnd());
										//nativeReader.Position = readerBeforeSearch;
										//boyerMoore1.SetPattern(new byte[] { 0xD3, 0x84, 0x90, 0x1A });
										//positionInTOC = boyerMoore1.SearchAll(nativeReader.ReadToEnd());
										//nativeReader.Position = readerBeforeSearch;

									}
									chunkAssetEntry2.ExtraData.DataOffset = chunkOffset;
                                    if (!AssetManager.Instance.chunkList.ContainsKey(chunkAssetEntry2.Id))
                                    {
										AssetManager.Instance.chunkList.Add(chunkAssetEntry2.Id, chunkAssetEntry2);
									}
									chunks.Add(chunkAssetEntry2);
								}
							}

							_ = nativeReader.Position;
							if(nativeReader.Position < nativeReader.Length)
                            {

								//for (int sha1Index = 0; sha1Index < MetaData.ResCount; sha1Index++)
								//{
								//	var sha1 = nativeReader.ReadGuid();
								//	//var unk1 = nativeReader.ReadInt();
								//	//var unk2 = nativeReader.ReadInt();
								//}
								if (FileLocation.Contains(@"data\win32/contentlaunchsb") 
									|| FileLocation.Contains(@"data\win32/contentsb")
									|| FileLocation.Contains(@"data\win32/globalsfull")
									)
								{
									AssetManager.Instance.logger.Log("Searching for CAS Data from " + FileLocation);
									Dictionary<string, List<CASBundle>> CASToBundles = new Dictionary<string, List<CASBundle>>();
									
									BoyerMoore casBinarySearcher = new BoyerMoore(new byte[] { 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00 });
									var readerBeforeSearch = nativeReader.Position;
									//var readerBeforeSearch = 0;
									//var positionInTOC = new List<int>();// casBinarySearcher.SearchAll(nativeReader.ReadToEnd());
									var positionInTOC = casBinarySearcher.SearchAll(nativeReader.ReadToEnd());
									//positionInTOC.Clear();
									foreach (var p in positionInTOC)
									{
										nativeReader.Position = p + readerBeforeSearch - 4;
										var totalityOffsetCount = nativeReader.ReadInt();
										// 0x20 x 3
										_ = nativeReader.ReadInt();
										_ = nativeReader.ReadInt();
										_ = nativeReader.ReadInt();

										var catalog = nativeReader.ReadInt(Endian.Big);
										var cas = (int)nativeReader.ReadByte();

										//_ = nativeReader.ReadInt();

										CASBundle bundle = new CASBundle();
										bundle.Catalog = catalog;
										bundle.Cas = cas;
										bundle.BundleOffset = nativeReader.ReadInt(Endian.Big);
										bundle.DataOffset = nativeReader.ReadInt(Endian.Big);
										for(var i = 1; i < totalityOffsetCount && totalityOffsetCount < 100; i++)
                                        {
											bundle.Offsets.Add(nativeReader.ReadInt(Endian.Big));
											bundle.Sizes.Add(nativeReader.ReadInt(Endian.Big));
										}
										if (AssetManager.Instance.fs.Catalogs.Count() > catalog)
										{
											var path = AssetManager.Instance.fs.ResolvePath(AssetManager.Instance.fs.GetFilePath(catalog, cas, false));

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

									if(CASToBundles.Count > 0)
                                    {
										AssetManager.Instance.logger.Log($"Found {CASToBundles.Count} CAS to Bundles");

										foreach (var cas2bundle in CASToBundles)
                                        {
											AssetManager.Instance.logger.Log($"Found {cas2bundle.Value.Count} Bundles in {cas2bundle.Key} loading...");

											CASDataLoader casDataLoader = new CASDataLoader();
											casDataLoader.Load(AssetManager.Instance, cas2bundle.Key, cas2bundle.Value);
										}
									}
								}


							}



						}
					}

					//if (MetaData.Offset5 > 128)
					//{
					//	SBFile sbFile = new SBFile(ParentReader, this, 0);
					//	BaseBundleInfo newBundleInfo = new BaseBundleInfo
					//	{
					//		TocOffset = 0,
					//		Offset = MetaData.Offset5,
					//		Size = MetaData.Offset6
					//	};
					//	//Bundles.Add(newBundleInfo);
					//}
				}


			}


		}

		public static IEnumerable<int> PatternAt(byte[] source, byte[] pattern)
		{
			for (int i = 0; i < source.Length; i++)
			{
				if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
				{
					yield return i;
				}
			}
		}
	}

	
}
