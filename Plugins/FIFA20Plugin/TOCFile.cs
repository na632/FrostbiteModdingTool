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

namespace FIFA20Plugin
{
	public class BundleFileInfo
	{
		public int Index;

		public int Offset;

		public int Size;

		public long OffsetPosition;

		public long SizePosition;

		public BundleFileInfo(int index, int offset, int size, long offset_pos, long size_pos)
		{
			Index = index;
			Offset = offset;
			Size = size;
			OffsetPosition = offset_pos;
			SizePosition = size_pos;
			ItemsInBundle = new List<(string, string)>();
		}

		public override string ToString()
		{
			return Index.ToString() + "-offset:(" + Offset + ")-size:(" + Size.ToString() + ")";
		}

		/// <summary>
		/// A list of all the items included in the bundle by name / guid
		/// </summary>
		public List<(string, string)> ItemsInBundle;
	}

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
		public List<ChunkAssetEntry> TocChunks = new List<ChunkAssetEntry>();


		public void Read(string tocPath, AssetManager parent, BinarySbDataHelper helper, int sbIndex, bool readCasData = true, bool processCasData = true)
		{
			NativePath = tocPath.Contains(".toc", StringComparison.OrdinalIgnoreCase) ? tocPath : tocPath + ".toc";
			tocPath = parent.fs.ResolvePath(NativePath);

			//if (File.Exists(tocPath + ".premod"))
			//         {
			//	File.Copy(tocPath + ".premod", tocPath, true);
			//	File.Delete(tocPath + ".premod");
			//         }

			byte[] key = KeyManager.Instance.GetKey("Key2");
			if (tocPath != "")
			{
				int num2 = 0;
				int toc_chunk_offset = 0;
				byte[] toc_array = null;
				const int array_offset = 556 + 12;
				using (NativeReader nativeReader = new NativeReader(new FileStream(tocPath, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
				{
					uint num4 = nativeReader.ReadUInt();
					num2 = nativeReader.ReadInt() - 12;
					toc_chunk_offset = nativeReader.ReadInt() - 12;
					toc_array = nativeReader.ReadToEnd();
					if (num4 == 3286619587u)
					{
						using (Aes aes = Aes.Create())
						{
							aes.Key = key;
							aes.IV = key;
							aes.Padding = PaddingMode.None;
							ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
							using (MemoryStream stream = new MemoryStream(toc_array))
							{
								using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
								{
									cryptoStream.Read(toc_array, 0, toc_array.Length);
								}
							}
						}
					}
				}
				if (toc_array.Length != 0)
				{
					using (NativeReader toc_reader = new NativeReader(new MemoryStream(toc_array)))
					{
						List<int> lstBundles = new List<int>();
						if (num2 <= 0)
							return;
						toc_reader.Position = num2;
						int numberOfBundles = toc_reader.ReadInt();
						for (int i = 0; i < numberOfBundles; i++)
						{
							lstBundles.Add(toc_reader.ReadInt());
						}

						DateTime lastLogTime = DateTime.Now;

						string bundleName = string.Empty;
						for (int j = 0; j < numberOfBundles; j++)
						{
							if (lastLogTime.AddSeconds(15) < DateTime.Now)
							{
								parent.logger.Log($"{NativePath} Progress: {Math.Round((double)j / (double)numberOfBundles * 100.0)}");
								lastLogTime = DateTime.Now;
							}

							int gotoposition1 = toc_reader.ReadInt() - 12;
							long position = toc_reader.Position;
							toc_reader.Position = gotoposition1;
							int num7 = toc_reader.ReadInt() - 1;
							FrostbiteSdk.Extras.MemoryUtils MemoryUtil = new FrostbiteSdk.Extras.MemoryUtils();
							int casIndex;
							string fp = string.Empty;
							FileStream casFileStream = null;

							var bundleEntryBundles = new List<BundleFileInfo>();

							do
							{
								// write your bundle information here, then write bundle data to end of file
								casIndex = toc_reader.ReadInt(Endian.Little);
								var offset_position = toc_reader.Position + array_offset;
								int offset = toc_reader.ReadInt(Endian.Little);
								var size_position = toc_reader.Position + array_offset;
								int size = toc_reader.ReadInt(Endian.Little);
								var filePath = parent.fs.GetFilePath(casIndex & int.MaxValue);
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
								}
								NativeReader nativeReader3 = new NativeReader(casFileStream);
								nativeReader3.Position = offset;
								MemoryUtil.Write(nativeReader3.ReadBytes(size));
								bundleEntryBundles.Add(new BundleFileInfo(casIndex & int.MaxValue, offset, size, offset_position, size_position));
							}
							while ((casIndex & 2147483648u) != 0L);
							if (casFileStream != null)
							{
								casFileStream.Close();
								casFileStream.Dispose();
							}
							toc_reader.Position = num7 - 12;
							int num11 = 0;
							bundleName = "";
							do
							{
								string str = toc_reader.ReadNullTerminatedString();
								num11 = toc_reader.ReadInt() - 1;
								bundleName += str;
								if (num11 != -1)
								{
									toc_reader.Position = num11 - 12;
								}
							}
							while (num11 != -1);
							bundleName = Utils.ReverseString(bundleName);
							toc_reader.Position = position;


							BundleEntry item = new BundleEntry
							{
								Name = bundleName,
								SuperBundleId = sbIndex
							};
							var sbName = item.Name.Substring(item.Name.LastIndexOf('/') + 1, item.Name.Length - item.Name.LastIndexOf('/') - 1);
							//System.Diagnostics.Debug.WriteLine("[DEBUG] BundleEntry " + n);

							parent.bundles.Add(item);
							TocBundles.Add(item, bundleEntryBundles);

							//foreach(var b in TocBundles)
							//                     {
							//	using (BinarySbReader_M21 binarySbReader
							//	= new BinarySbReader_M21(
							//		MemoryUtil.GetMemoryStream()
							//		, 0
							//		, parent.fs.CreateDeobfuscator()))
							//	{
							//		binarySbReader.bundleName = n;

							//		DbObject dbObject = binarySbReader.ReadDbObject();
							//	}
							//}
							if (readCasData)
							{
								ReadBundleCasData(parent, helper, bundleEntryBundles, MemoryUtil, bundleName, processCasData);
							}
						}

						if (toc_chunk_offset <= 0)
							return;

						toc_reader.Position = toc_chunk_offset;
						int chunkCount = toc_reader.ReadInt();
						lstBundles = new List<int>();
						for (int k = 0; k < chunkCount; k++)
						{
							lstBundles.Add(toc_reader.ReadInt());
						}
						for (int l = 0; l < chunkCount; l++)
						{
							ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
							int num16 = toc_reader.ReadInt();
							long position2 = toc_reader.Position;
							toc_reader.Position = num16 - 12;
							Guid guid = toc_reader.ReadGuid();
							int index = toc_reader.ReadInt();

							chunkAssetEntry.SB_CAS_Offset_Position = (int)toc_reader.Position;
							int chunkOffet = toc_reader.ReadInt();
							chunkAssetEntry.SB_CAS_Size_Position = (int)toc_reader.Position;
							int chunkSize = toc_reader.ReadInt();

							chunkAssetEntry.Id = guid;
							chunkAssetEntry.Sha1 = Sha1.Create(Encoding.ASCII.GetBytes(chunkAssetEntry.Id.ToString()));

							chunkAssetEntry.OriginalSize = chunkSize;
							chunkAssetEntry.Size = chunkSize;
							chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
							chunkAssetEntry.ExtraData = new AssetExtraData();
							chunkAssetEntry.ExtraData.CasPath = parent.fs.GetFilePath(index);
							chunkAssetEntry.ExtraData.DataOffset = (uint)chunkOffet;
							chunkAssetEntry.TOCFileLocation = NativePath;
							chunkAssetEntry.IsTocChunk = true;

							if (TocChunks == null)
								TocChunks = new List<ChunkAssetEntry>();

							TocChunks.Add(chunkAssetEntry);

							toc_reader.Position = position2;

							if (!parent.chunkList.ContainsKey(guid))
							{
								parent.chunkList.TryAdd(guid, chunkAssetEntry);
							}
							else
							{
								parent.chunkList[guid] = chunkAssetEntry;
							}
						}
					}
				}
			}
		}

		public Dictionary<string, DbObject> BundleTOCSB = new Dictionary<string, DbObject>();

		private void ReadBundleCasData(AssetManager parent, BinarySbDataHelper helper, List<BundleFileInfo> TocBundles, MemoryUtils MemoryUtil, string bundleName, bool process = true)
		{
			/*
			using (BinarySbReader_M21 binarySbReader
												= new BinarySbReader_M21(
													MemoryUtil.GetMemoryStream()
													, 0
													, parent.fs.CreateDeobfuscator()))
			{
				binarySbReader.bundleName = bundleName;

				var dbObject = binarySbReader.ReadDbObject();
				if (dbObject != null && dbObject.Dictionary != null && dbObject.Dictionary.Count > 0)
					BundleTOCSB.Add(bundleName, dbObject);

				BundleFileInfo bundleFileInfo = TocBundles.First();
				long finishingItemOffset = bundleFileInfo.Offset + (dbObject.GetValue("dataOffset", 0L));
				long currentLength = bundleFileInfo.Size - (dbObject.GetValue("dataOffset", 0L));
				int tocBundleIndex = 0;
				foreach (DbObject finishingEbxItem in dbObject.GetValue<DbObject>("ebx"))
				{
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
					finishingEbxItem.SetValue("SB_OriginalSize_Position", bundleFileInfo.Offset + finishingEbxItem.GetValue<int>("SB_OriginalSize_Position"));
					finishingEbxItem.SetValue("ParentBundleOffset_Position", bundleFileInfo.OffsetPosition);
					finishingEbxItem.SetValue("ParentBundleOffset", bundleFileInfo.Offset);
					finishingEbxItem.SetValue("ParentBundleSize_Position", bundleFileInfo.SizePosition);
					finishingEbxItem.SetValue("ParentBundleSize", bundleFileInfo.Size);
					finishingEbxItem.SetValue("Bundle", bundleName);
					bundleFileInfo.ItemsInBundle.Add(("ebx", finishingEbxItem.GetValue<string>("name")));
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
					finishingRESItem.SetValue("SB_OriginalSize_Position", bundleFileInfo.Offset + finishingRESItem.GetValue<int>("SB_OriginalSize_Position"));
					finishingRESItem.SetValue("ParentBundleOffset_Position", bundleFileInfo.OffsetPosition);
					finishingRESItem.SetValue("ParentBundleOffset", bundleFileInfo.Offset);
					finishingRESItem.SetValue("ParentBundleSize_Position", bundleFileInfo.SizePosition);
					finishingRESItem.SetValue("ParentBundleSize", bundleFileInfo.Size);
					finishingRESItem.SetValue("Bundle", bundleName);

					bundleFileInfo.ItemsInBundle.Add(("res", finishingRESItem.GetValue<string>("name")));

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
					finishingChunkItem.SetValue("SB_OriginalSize_Position", bundleFileInfo.Offset + finishingChunkItem.GetValue<int>("SB_OriginalSize_Position"));
					finishingChunkItem.SetValue("ParentBundleOffset_Position", bundleFileInfo.OffsetPosition);
					finishingChunkItem.SetValue("ParentBundleOffset", bundleFileInfo.Offset);
					finishingChunkItem.SetValue("ParentBundleSize_Position", bundleFileInfo.SizePosition);
					finishingChunkItem.SetValue("ParentBundleSize", bundleFileInfo.Size);
					finishingChunkItem.SetValue("Bundle", bundleName);

					bundleFileInfo.ItemsInBundle.Add(("chunk", finishingChunkItem.GetValue<Guid>("id").ToString()));
				}

				if (process)
				{
					parent.ProcessBundleEbx(dbObject, parent.bundles.Count - 1, helper);
					parent.ProcessBundleRes(dbObject, parent.bundles.Count - 1, helper);
					parent.ProcessBundleChunks(dbObject, parent.bundles.Count - 1, helper);
				}


			}
			*/
		}
	}
}
