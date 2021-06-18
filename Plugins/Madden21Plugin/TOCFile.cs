using FrostbiteSdk.Extras;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static Madden21Plugin.Madden21AssetLoader;

namespace Madden21Plugin
{
	public class TOCFile
	{
		private string NativePath;

		public string ResolvedPath
		{
			get
			{
				return AssetManager.Instance.fs.ResolvePath(NativePath);
			}
		}

		public string ResolvedModPath
		{
			get
			{
				var originalResolvedPath = ResolvedPath;
				var newPath = originalResolvedPath.Replace("\\data\\", "\\ModData\\data\\", StringComparison.OrdinalIgnoreCase);
				newPath = newPath.Replace("\\patch\\", "\\ModData\\patch\\", StringComparison.OrdinalIgnoreCase);
				return newPath;
			}
		}

		public Dictionary<BundleEntry, List<BundleFileInfo>> TocBundles = new Dictionary<BundleEntry, List<BundleFileInfo>>();

		public Dictionary<BundleEntry, BundleFileInfo> TocBundlesInSingular
        {
			get
            {
				var singular = new Dictionary<BundleEntry, BundleFileInfo>();
				foreach(var bundle in TocBundles)
                {
					var bfi = bundle.Value.OrderByDescending(x => x.Index).First();
					var index = bfi.Index;
					var offset = bfi.Offset;
					var size = bfi.Size;
					singular.Add(bundle.Key, bfi);
                }
				return singular;
            }
        }


		public List<ChunkAssetEntry> TocChunks = new List<ChunkAssetEntry>();
		public List<int> TOCChunkBundleIndexes = new List<int>();
		public List<int> BundleIndexes = new List<int>();
		public Dictionary<int, List<BundleFileInfo>> BundleIndexToBundles = new Dictionary<int, List<BundleFileInfo>>();
		public uint TocMagic = 0;
		public int StartOfBundles = 0;
		public int TocChunkOffset = 0;
		public int FirstBundleNameOffset = 0;
		public int EndOfChunksOffset = 0;
		private int ArrayOffset = 556;

		private string tocFilePath = string.Empty;

		public TOCFile(string tocPath)
        {
			NativePath = tocPath.Contains(".toc", StringComparison.OrdinalIgnoreCase) ? tocPath : tocPath + ".toc";
			if (NativePath.Contains(FileSystem.Instance.BasePath, StringComparison.OrdinalIgnoreCase))
			{
				NativePath = NativePath.Replace(FileSystem.Instance.BasePath, "", StringComparison.OrdinalIgnoreCase);
				if (NativePath.Contains("patch", StringComparison.OrdinalIgnoreCase))
				{
					NativePath = NativePath.Replace("patch", "native_patch", StringComparison.OrdinalIgnoreCase);
				}
				if (NativePath.Contains("data", StringComparison.OrdinalIgnoreCase))
				{
					NativePath = NativePath.Replace("data", "native_data", StringComparison.OrdinalIgnoreCase);
				}
			}

			tocFilePath = FileSystem.Instance.ResolvePath(NativePath);
		}

		public void Read(string tocPath, AssetManager parent, BinarySbDataHelper helper, int sbIndex, bool readCasData = true, bool processCasData = true)
		{
			NativePath = tocPath.Contains(".toc", StringComparison.OrdinalIgnoreCase) ? tocPath : tocPath + ".toc";
			//if(NativePath.Contains(parent.fs.BasePath, StringComparison.OrdinalIgnoreCase))
			if(NativePath.Contains(FileSystem.Instance.BasePath, StringComparison.OrdinalIgnoreCase))
            {
				NativePath = NativePath.Replace(FileSystem.Instance.BasePath, "", StringComparison.OrdinalIgnoreCase);
				if(NativePath.Contains("patch", StringComparison.OrdinalIgnoreCase))
                {
					NativePath = NativePath.Replace("patch", "native_patch", StringComparison.OrdinalIgnoreCase);
                }
				if (NativePath.Contains("data", StringComparison.OrdinalIgnoreCase))
				{
					NativePath = NativePath.Replace("data", "native_data", StringComparison.OrdinalIgnoreCase);
				}
			}

			tocFilePath = FileSystem.Instance.ResolvePath(NativePath);


			if (!string.IsNullOrEmpty(tocFilePath))
			{
				using (NativeReader nativeReader = new NativeReader(new FileStream(tocFilePath, FileMode.Open, FileAccess.Read)))
                {
                    ReadHeader(nativeReader);

                    if (StartOfBundles <= 0)
                        return;
                    nativeReader.Position = StartOfBundles;
                    int numberOfBundles = nativeReader.ReadInt();
                    for (int i = 0; i < numberOfBundles; i++)
                    {
                        BundleIndexes.Add(nativeReader.ReadInt());
                    }

                    DateTime lastLogTime = DateTime.Now;

                    string bundleName = string.Empty;
                    for (int j = 0; j < numberOfBundles; j++)
                    {

                        if (lastLogTime.AddSeconds(15) < DateTime.Now)
                        {
                            var percentDone = Math.Round((double)j / (double)numberOfBundles * 100.0);
                            parent.logger.Log($"{NativePath} Progress: {percentDone}");
                            lastLogTime = DateTime.Now;
                        }

                        //if(percentDone % 10 == 0)
                        //                  {
                        //	GC.Collect();
                        //	GC.WaitForPendingFinalizers();
                        //                  }

                        int gotoposition1 = nativeReader.ReadInt() + ArrayOffset;
                        long position = nativeReader.Position;
                        nativeReader.Position = gotoposition1;
                        int bundleNamePosition = nativeReader.ReadInt() - 1 + ArrayOffset;
                        using (var MemoryUtil = new FrostbiteSdk.Extras.MemoryUtils())
                        {
                            int bundleCasIndex;
                            string fp = string.Empty;
                            FileStream casFileStream = null;
                            NativeReader tocCasReader = null;
                            var bundleEntryBundles = new List<BundleFileInfo>(99999);

                            BundleFileInfo parentBFI = null;
                            do
                            {
                                // write your bundle information here, then write bundle data to end of file
                                var casIndex_Position = nativeReader.Position;
                                bundleCasIndex = nativeReader.ReadInt(Endian.Little);
                                var realCasIndex = bundleCasIndex & int.MaxValue;

                                var offset_position = nativeReader.Position;
                                int offset = nativeReader.ReadInt(Endian.Little);
                                var size_position = nativeReader.Position;
                                int size = nativeReader.ReadInt(Endian.Little);
                                var filePath = FileSystem.Instance.GetFilePath(realCasIndex);

                                if (readCasData)
                                {
                                    if (fp != filePath)
                                    {
                                        fp = filePath;
                                        var finalFilePath = parent.fs.ResolvePath(filePath);
                                        if (casFileStream != null)
                                        {
                                            casFileStream.Close();
                                            casFileStream.Dispose();
                                        }
                                        casFileStream = new FileStream(finalFilePath, FileMode.Open, FileAccess.Read);
                                        tocCasReader = new NativeReader(casFileStream);
                                    }
                                    tocCasReader.Position = offset;
                                    MemoryUtil.Write(tocCasReader.ReadBytes(size));

                                    var bfi = new BundleFileInfo(realCasIndex, offset, size, offset_position, size_position, casIndex_Position, parentBFI == null, parentBFI);
                                    bundleEntryBundles.Add(bfi);
                                    if (parentBFI == null) parentBFI = bfi;
                                }
                            }
                            while ((bundleCasIndex & 2147483648u) != 0L);
                            if (casFileStream != null)
                            {
                                casFileStream.Close();
                                casFileStream.Dispose();
                            }
                            nativeReader.Position = bundleNamePosition;
                            int nextStringPosition = 0;
                            bundleName = "";
                            do
                            {
                                string str = nativeReader.ReadNullTerminatedString();
                                nextStringPosition = nativeReader.ReadInt() - 1;
                                bundleName += str;
                                if (nextStringPosition != -1)
                                {
                                    nativeReader.Position = nextStringPosition + ArrayOffset;
                                }
                            }
                            while (nextStringPosition != -1);
                            bundleName = Utils.ReverseString(bundleName);
                            nativeReader.Position = position;


                            BundleEntry bundle = new BundleEntry
                            {
                                Name = bundleName,
                                SuperBundleId = sbIndex
                            };

                            if (parent != null)
                                parent.bundles.Add(bundle);
                            //TocBundles.Add(bundle, bundleEntryBundles);

                            //BundleIndexToBundles.Add(j, bundleEntryBundles);

                            if (readCasData)
                            {

                                //foreach(var beb in bundleEntryBundles.OrderBy(x=>x.Index).ThenBy(x=>x.Offset))
                                //                     {

                                //}
                                ReadBundleCasData(parent, helper, bundleEntryBundles, MemoryUtil, bundle, processCasData);
                            }
                        }
                    }

					ReadTOCChunks(nativeReader, processCasData);
                    
                }
            }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nativeReader">Pass in NULL for method to create its own reader</param>
        public void ReadHeader(NativeReader nativeReader = null)
        {
			if (string.IsNullOrEmpty(tocFilePath) && nativeReader == null)
				throw new FileNotFoundException("No TOC File Path provided. Cannot create a reader.");

			bool createdNR = nativeReader == null;
			if (nativeReader == null)
				nativeReader = new NativeReader(tocFilePath);

            nativeReader.Position = ArrayOffset;
            TocMagic = nativeReader.ReadUInt();
            //StartOfBundles = nativeReader.ReadInt() - 12;
            //TocChunkOffset = nativeReader.ReadInt() - 12;
            StartOfBundles = nativeReader.ReadInt() + ArrayOffset;
            TocChunkOffset = nativeReader.ReadInt() + ArrayOffset;
            FirstBundleNameOffset = nativeReader.ReadInt();

			if (createdNR) 
			{
				nativeReader.Dispose();
			}
        }

		public void ReadTOCChunks(NativeReader nativeReader = null, bool processData = true)
        {
			if (string.IsNullOrEmpty(tocFilePath) && nativeReader == null)
				throw new FileNotFoundException("No TOC File Path provided. Cannot create a reader.");

			bool createdNR = nativeReader == null;
			if (nativeReader == null)
				nativeReader = new NativeReader(tocFilePath);

			if (TocChunkOffset <= 0)
				return;

			nativeReader.Position = TocChunkOffset;
			int chunkCount = nativeReader.ReadInt();
			TOCChunkBundleIndexes = new List<int>();
			for (int k = 0; k < chunkCount; k++)
			{
				TOCChunkBundleIndexes.Add(nativeReader.ReadInt());
			}
			List<int> TocChunkPositions = new List<int>();
			for (int k = 0; k < chunkCount; k++)
			{
				TocChunkPositions.Add(nativeReader.ReadInt() + ArrayOffset);
			}
			EndOfChunksOffset = (int)nativeReader.Position;
			for (int l = 0; l < chunkCount; l++)
			{
				ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
				int chunkPosition = TocChunkPositions[l];
				nativeReader.Position = chunkPosition;
				Guid guid = nativeReader.ReadGuid();
				if (guid.ToString() == "e35237c0-ebbe-bc31-1504-aaf3eb947c9b")
				{

				}

				int chunkCasIndex = nativeReader.ReadInt();

				chunkAssetEntry.SB_CAS_Offset_Position = (int)nativeReader.Position;
				int chunkOffset = nativeReader.ReadInt();
				chunkAssetEntry.SB_CAS_Size_Position = (int)nativeReader.Position;
				int chunkSize = nativeReader.ReadInt();

				chunkAssetEntry.Id = guid;
				chunkAssetEntry.Sha1 = Sha1.Create(Encoding.ASCII.GetBytes(chunkAssetEntry.Id.ToString()));

				chunkAssetEntry.OriginalSize = chunkSize;
				chunkAssetEntry.Size = chunkSize;
				chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
				chunkAssetEntry.ExtraData = new AssetExtraData();
				chunkAssetEntry.ExtraData.CasIndex = chunkCasIndex;
				chunkAssetEntry.ExtraData.CasPath = FileSystem.Instance.GetFilePath(chunkCasIndex);
				chunkAssetEntry.ExtraData.DataOffset = (uint)chunkOffset;
				chunkAssetEntry.TOCFileLocation = NativePath;
				chunkAssetEntry.IsTocChunk = true;

				if (TocChunks == null)
					TocChunks = new List<ChunkAssetEntry>();

				TocChunks.Add(chunkAssetEntry);

				if (processData)
				{
					if (!AssetManager.Instance.chunkList.ContainsKey(guid))
					{
						AssetManager.Instance.chunkList.TryAdd(guid, chunkAssetEntry);
					}
				}
			}

			if (createdNR)
			{
				nativeReader.Dispose();
			}
		}

        public Dictionary<string, DbObject> BundleTOCSB = new Dictionary<string, DbObject>();

		private void ReadBundleCasData(AssetManager parent, BinarySbDataHelper helper, List<BundleFileInfo> TocBundles, MemoryUtils MemoryUtil, BundleEntry bundle, bool process = true)
		{
			using (BinarySbReader_M21 binarySbReader
												= new BinarySbReader_M21(
													MemoryUtil.GetMemoryStream()
													, 0
													, parent.fs.CreateDeobfuscator()))
			{

				var bundleName = bundle.Name;
				binarySbReader.bundleName = bundle.Name;

				var dbObject = binarySbReader.ReadDbObject();
				//if (dbObject != null && dbObject.Dictionary != null && dbObject.Dictionary.Count > 0)
				//	BundleTOCSB.Add(bundleName, dbObject);

				BundleFileInfo bundleFileInfo = TocBundles.First();
				long finishingItemOffset = bundleFileInfo.Offset + (dbObject.GetValue("dataOffset", 0L));
				long currentLength = bundleFileInfo.Size - (dbObject.GetValue("dataOffset", 0L));
				int tocBundleIndex = 0;
				foreach (DbObject finishingEbxItem in dbObject.GetValue<DbObject>("ebx"))
				{
					if(finishingEbxItem.HasValue("name") && finishingEbxItem.GetValue<string>("name").Contains("col_jersey_2020_whi_jersey_col"))
					{

                    }

                    if (currentLength == 0L)
                    {
                        bundleFileInfo = TocBundles[++tocBundleIndex];
                        currentLength = bundleFileInfo.Size;
                        finishingItemOffset = bundleFileInfo.Offset;
                    }

                    int itemSize = finishingEbxItem.GetValue("size", 0);
					finishingEbxItem.SetValue("offset", finishingItemOffset);
					finishingEbxItem.SetValue("cas", bundleFileInfo.Index);
					finishingItemOffset += itemSize;
					currentLength -= itemSize;

					finishingEbxItem.SetValue("TOCFileLocation", NativePath);
					//finishingEbxItem.SetValue("SB_OriginalSize_Position", bundleFileInfo.Offset + finishingEbxItem.GetValue<int>("SB_OriginalSize_Position"));
					//finishingEbxItem.SetValue("ParentBundleOffset_Position", bundleFileInfo.OffsetPosition);
					//finishingEbxItem.SetValue("ParentBundleOffset", bundleFileInfo.Offset);
					//finishingEbxItem.SetValue("ParentBundleSize_Position", bundleFileInfo.SizePosition);
					//finishingEbxItem.SetValue("ParentBundleSize", bundleFileInfo.Size);
					//finishingEbxItem.SetValue("Bundle", bundleName);
					//finishingEbxItem.SetValue("BundleIndex", parent.bundles.Count - 1);
					//bundleFileInfo.ItemsInBundle.Add(new BundleFileInfo.BundleItem("ebx", finishingEbxItem.GetValue<string>("name")));

					//if (!bundleFileInfo.BundleObject.HasValue("ebx"))
					//	bundleFileInfo.BundleObject.AddValue("ebx", new DbObject());

					//bundleFileInfo.BundleObject.GetValue<DbObject>("ebx").Add(finishingEbxItem);

				}
				foreach (DbObject finishingRESItem in dbObject.GetValue<DbObject>("res"))
				{
					if (currentLength == 0L)
					{
						bundleFileInfo = TocBundles[++tocBundleIndex];
						currentLength = bundleFileInfo.Size;
						finishingItemOffset = bundleFileInfo.Offset;
					}
					int itemSize = finishingRESItem.GetValue("size", 0);
					finishingRESItem.SetValue("offset", finishingItemOffset);
					finishingRESItem.SetValue("cas", bundleFileInfo.Index);
					finishingItemOffset += itemSize;
					currentLength -= itemSize;

					finishingRESItem.SetValue("TOCFileLocation", NativePath);
					//finishingRESItem.SetValue("SB_OriginalSize_Position", bundleFileInfo.Offset + finishingRESItem.GetValue<int>("SB_OriginalSize_Position"));
					//finishingRESItem.SetValue("ParentBundleOffset_Position", bundleFileInfo.OffsetPosition);
					//finishingRESItem.SetValue("ParentBundleOffset", bundleFileInfo.Offset);
					//finishingRESItem.SetValue("ParentBundleSize_Position", bundleFileInfo.SizePosition);
					//finishingRESItem.SetValue("ParentBundleSize", bundleFileInfo.Size);
					//finishingRESItem.SetValue("Bundle", bundleName);
					////finishingRESItem.SetValue("BundleIndex", parent.bundles.Count - 1);


					//bundleFileInfo.ItemsInBundle.Add(new BundleFileInfo.BundleItem("res", finishingRESItem.GetValue<string>("name")));

					//if (!bundleFileInfo.BundleObject.HasValue("res"))
					//	bundleFileInfo.BundleObject.AddValue("res", new DbObject());

					//bundleFileInfo.BundleObject.GetValue<DbObject>("res").Add(finishingRESItem);


				}
				foreach (DbObject finishingChunkItem in dbObject.GetValue<DbObject>("chunks"))
				{
					if (currentLength == 0L)
					{
						bundleFileInfo = TocBundles[++tocBundleIndex];
						currentLength = bundleFileInfo.Size;
						finishingItemOffset = bundleFileInfo.Offset;
					}
					int chunkItemSize = finishingChunkItem.GetValue("size", 0);
					finishingChunkItem.SetValue("offset", finishingItemOffset);
					finishingChunkItem.SetValue("cas", bundleFileInfo.Index);
					finishingItemOffset += chunkItemSize;
					currentLength -= chunkItemSize;

					finishingChunkItem.SetValue("TOCFileLocation", NativePath);
					//finishingChunkItem.SetValue("SB_OriginalSize_Position", bundleFileInfo.Offset + finishingChunkItem.GetValue<int>("SB_OriginalSize_Position"));
					//finishingChunkItem.SetValue("ParentBundleOffset_Position", bundleFileInfo.OffsetPosition);
					//finishingChunkItem.SetValue("ParentBundleOffset", bundleFileInfo.Offset);
					//finishingChunkItem.SetValue("ParentBundleSize_Position", bundleFileInfo.SizePosition);
					//finishingChunkItem.SetValue("ParentBundleSize", bundleFileInfo.Size);
					//finishingChunkItem.SetValue("Bundle", bundleName);
					//finishingChunkItem.SetValue("BundleIndex", parent.bundles.Count - 1);

					//bundleFileInfo.ItemsInBundle.Add(new BundleFileInfo.BundleItem("chunk", finishingChunkItem.GetValue<Guid>("id").ToString()));

					//if (!bundleFileInfo.BundleObject.HasValue("chunks"))
					//	bundleFileInfo.BundleObject.AddValue("chunks", new DbObject());

					// colts uniform
					//if (finishingChunkItem.GetValue<Guid>("name").ToString() == "e35237c0-ebbe-bc31-1504-aaf3eb947c9b"
					//	&& !bundleName.Contains("playeruniformtextures")
					//	)
					//{
					//	finishingChunkItem.SetValue("DNP", true);
					//}
				}

				if (process)
				{
					//parent.ProcessBundleEbx(dbObject, parent.bundles.Count - 1, helper);
					//parent.ProcessBundleRes(dbObject, parent.bundles.Count - 1, helper);
					//parent.ProcessBundleChunks(dbObject, parent.bundles.Count - 1, helper);
					parent.ProcessBundleEbx(dbObject, parent.bundles.Count - 1, helper);
					parent.ProcessBundleRes(dbObject, parent.bundles.Count - 1, helper);
					parent.ProcessBundleChunks(dbObject, parent.bundles.Count - 1, helper);
				}


			}
		}


		public byte[] WriteBytes()
		{
			var ms_newToc = new MemoryStream();
			using (NativeWriter nw = new NativeWriter(ms_newToc, true))
			{
				// TOC Start Magic
				nw.Write(30331136ul);
				nw.Write(new byte[548]);
				var posStartOfData = nw.Position;
				// Magic
				nw.Write(3280507699u);

				// Bundle OFFSET
				nw.Write(0);
				// Chunks OFFSET
				nw.Write((int)-1);
                // OFFSET of First bundle name + 1
                nw.Write((int)-1);
                //nw.Write((int)-1);

                List<int> BundlePositions = new List<int>();

				bool isFirstBundle = true;
				int FirstBundleNameOffset = -1;
				foreach(var bundle in TocBundles)
                {
					BundlePositions.Add((int)nw.Position - (int)posStartOfData);
					// --------------------------------------------
					// Write Bundles
					// 
					var bundleItemIndex = 0;
					bundle.Value.ForEach(bundleItem =>
					{
						uint indexer = (uint)bundleItem.Index;
						if (bundleItemIndex != bundle.Value.Count - 1)
						{
							indexer = (uint)((int)indexer | int.MinValue);
						}
						nw.Write(indexer);
						nw.Write(bundleItem.Offset);
						nw.Write(bundleItem.Size);

						bundleItemIndex++;
					});
                    // 
                    // --------------------------------------------

                    // --------------------------------------------
                    // Write the Bundle Name - Reversed
                    //
                    if (isFirstBundle)
                    {
						isFirstBundle = false;
						FirstBundleNameOffset = (int)nw.Position + 1;
					}
					nw.WriteNullTerminatedString(new string(bundle.Key.Name.Reverse().ToArray()));
                    //nw.Write(0);
                    nw.WritePadding(8);


                    // --------------------------------------------
                }

				var posStartOfBundles = nw.BaseStream.Position - posStartOfData;
				nw.Write(TocBundles.Count);
                for (var i = 0; i < TocBundles.Count; i++)
                {
                    nw.Write(-i - 1);
                }
                foreach (var bPos in BundlePositions)
                {
					nw.Write(bPos - 4);
                }

				List<int> TocChunkPositions = new List<int>();
				for (var i = 0; i < TocChunks.Count; i++)
                {
					
					TocChunkPositions.Add((int)nw.Position - (int)posStartOfData);
					var chunk = TocChunks[i];
					nw.Write(chunk.Id);
					nw.Write((int)chunk.ExtraData.CasIndex);
					nw.Write((int)chunk.ExtraData.DataOffset);
					nw.Write((int)chunk.Size);
                }
				var startOfTocChunks = nw.BaseStream.Position - (int)posStartOfData;
				nw.Write((int)TocChunks.Count);
				for(var k = 0; k < TOCChunkBundleIndexes.Count; k++)
                {
					nw.Write(TOCChunkBundleIndexes[k]);
				}
				for (var i = 0; i < TocChunks.Count; i++)
				{
					nw.Write((int)TocChunkPositions[i]);
				}


				nw.BaseStream.Position = posStartOfData + 4;
				// Position of Count Of Bundles
				//nw.Write((int)posStartOfBundles);
				if (TocBundles.Count == 0)
					nw.Write((int)-1);
				else
					nw.Write((int)posStartOfBundles);

				if (TocChunks.Count == 0)
					nw.Write((int)-1); // position of TOC Chunk Items
				else
					nw.Write((int)startOfTocChunks);

				// Unknown Offset???
				FirstBundleNameOffset -= (int)posStartOfData;
				nw.Write((int)FirstBundleNameOffset);
                //nw.Write((int)77);


            }

            var data = ms_newToc.ToArray();
			ms_newToc.Close();
			ms_newToc.Dispose();
			ms_newToc = null;
			return data;
		}
	}
}