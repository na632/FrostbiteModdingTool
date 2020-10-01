using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static FrostySdk.Managers.AssetManager;

namespace Madden21Plugin
{

	public class FIFA21AssetLoader : IAssetLoader
	{
		internal struct BundleFileInfo
		{
			public int Index;

			public int Offset;

			public int Size;

			public BundleFileInfo(int index, int offset, int size)
			{
				Index = index;
				Offset = offset;
				Size = size;
			}
		}

		public void Load(AssetManager parent, BinarySbDataHelper helper)
		{
			byte[] key = KeyManager.Instance.GetKey("Key2");
			foreach (CatalogInfo item2 in parent.fs.EnumerateCatalogInfos())
			{
				foreach (string sbName in item2.SuperBundles.Keys)
				{
					SuperBundleEntry superBundleEntry = parent.superBundles.Find((SuperBundleEntry a) => a.Name == sbName);
					int sbIndex = -1;
					if (superBundleEntry != null)
					{
						sbIndex = parent.superBundles.IndexOf(superBundleEntry);
					}
					else
					{
						parent.superBundles.Add(new SuperBundleEntry
						{
							Name = sbName
						});
						sbIndex = parent.superBundles.Count - 1;
					}
					parent.logger.Log($"Loading data ({sbName})");

					parent.superBundles.Add(new SuperBundleEntry
					{
						Name = sbName
					});
					string arg = sbName;
					if (item2.SuperBundles[sbName])
					{
						arg = sbName.Replace("win32", item2.Name);
					}
					string tocPath = parent.fs.ResolvePath($"{arg}.toc");
					if (tocPath != "")
					{
						int num2 = 0;
						int num3 = 0;
						byte[] toc_array = null;
						using (NativeReader nativeReader = new NativeReader(new FileStream(tocPath, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
						{
							uint num4 = nativeReader.ReadUInt();
							num2 = nativeReader.ReadInt() - 12;
							num3 = nativeReader.ReadInt() - 12;
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
								List<int> list = new List<int>();
								if (num2 > 0)
								{
									toc_reader.Position = num2;
									int num5 = toc_reader.ReadInt();
									for (int i = 0; i < num5; i++)
									{
										list.Add(toc_reader.ReadInt());
									}

									DateTime lastLogTime = DateTime.Now;

									for (int j = 0; j < num5; j++)
									{
										if (lastLogTime.AddSeconds(10) < DateTime.Now)
										{
											parent.logger.Log($"progress:{Math.Round((double)j / (double)num5 * 100.0)}");
											lastLogTime = DateTime.Now;
										}

										int num6 = toc_reader.ReadInt() - 12;
										long position = toc_reader.Position;
										toc_reader.Position = num6;
										int num7 = toc_reader.ReadInt() - 1;
										List<BundleFileInfo> list2 = new List<BundleFileInfo>();
										MemoryStream memoryStream = new MemoryStream();
										int casIndex;
										do
										{
											casIndex = toc_reader.ReadInt();
											int offset = toc_reader.ReadInt();
											int size = toc_reader.ReadInt();
											using (NativeReader nativeReader3 = new NativeReader(new FileStream(parent.fs.ResolvePath(parent.fs.GetFilePath(casIndex & int.MaxValue)), FileMode.Open, FileAccess.Read)))
											{
												nativeReader3.Position = offset;
												memoryStream.Write(nativeReader3.ReadBytes(size), 0, size);
											}
											list2.Add(new BundleFileInfo(casIndex & int.MaxValue, offset, size));
										}
										while ((casIndex & 2147483648u) != 0L);
										toc_reader.Position = num7 - 12;
										int num11 = 0;
										string text2 = "";
										do
										{
											string str = toc_reader.ReadNullTerminatedString();
											num11 = toc_reader.ReadInt() - 1;
											text2 += str;
											if (num11 != -1)
											{
												toc_reader.Position = num11 - 12;
											}
										}
										while (num11 != -1);
										text2 = Utils.ReverseString(text2);
										toc_reader.Position = position;


										BundleEntry item = new BundleEntry
										{
											Name = text2,
											SuperBundleId = sbIndex
										};
										var n = item.Name.Substring(item.Name.LastIndexOf('/') + 1, item.Name.Length - item.Name.LastIndexOf('/') - 1);
										//System.Diagnostics.Debug.WriteLine("[DEBUG] BundleEntry " + n);



										parent.bundles.Add(item);
										//using (BinarySbReader binarySbReader = new BinarySbReader(memoryStream, 0L, parent.fs.CreateDeobfuscator()))
										using (BinarySbReader_M21 binarySbReader = new BinarySbReader_M21(memoryStream, 0L, parent.fs.CreateDeobfuscator()))
										{
											binarySbReader.bundleName = n;

											DbObject dbObject = binarySbReader.ReadDbObject();
											BundleFileInfo bundleFileInfo = list2[0];
											//long finishingItemOffset = bundleFileInfo.Offset + (dbObject.GetValue("dataOffset", 0L) + 4);
											long finishingItemOffset = bundleFileInfo.Offset + (dbObject.GetValue("dataOffset", 0L));
											long num13 = bundleFileInfo.Size - (dbObject.GetValue("dataOffset", 0L));// + 4);
											int num14 = 0;
											foreach (DbObject finishingEbxItem in dbObject.GetValue<DbObject>("ebx"))
											{
												if (num13 == 0L)
												{
													bundleFileInfo = list2[++num14];
													num13 = bundleFileInfo.Size;
													finishingItemOffset = bundleFileInfo.Offset;
												}
												int value = finishingEbxItem.GetValue("size", 0);
												finishingEbxItem.SetValue("offset", finishingItemOffset);
												finishingEbxItem.SetValue("cas", bundleFileInfo.Index);
												finishingItemOffset += value;
												num13 -= value;
											}
											foreach (DbObject finishingRESItem in dbObject.GetValue<DbObject>("res"))
											{
												if (num13 == 0L)
												{
													bundleFileInfo = list2[++num14];
													num13 = bundleFileInfo.Size;
													finishingItemOffset = bundleFileInfo.Offset;
												}
												int value2 = finishingRESItem.GetValue("size", 0);
												finishingRESItem.SetValue("offset", finishingItemOffset);
												finishingRESItem.SetValue("cas", bundleFileInfo.Index);
												finishingItemOffset += value2;
												num13 -= value2;
											}
											foreach (DbObject finishingChunkItem in dbObject.GetValue<DbObject>("chunks"))
											{
												if (num13 == 0L)
												{
													bundleFileInfo = list2[++num14];
													num13 = bundleFileInfo.Size;
													finishingItemOffset = bundleFileInfo.Offset;
												}
												int chunkItemSize = finishingChunkItem.GetValue("size", 0);
												finishingChunkItem.SetValue("offset", finishingItemOffset);
												finishingChunkItem.SetValue("cas", bundleFileInfo.Index);
												finishingItemOffset += chunkItemSize;
												num13 -= chunkItemSize;
											}
											parent.ProcessBundleEbx(dbObject, parent.bundles.Count - 1, helper);
											parent.ProcessBundleRes(dbObject, parent.bundles.Count - 1, helper);
											parent.ProcessBundleChunks(dbObject, parent.bundles.Count - 1, helper);
										}

									}
								}
								if (num3 > 0)
								{
									toc_reader.Position = num3;
									int num15 = toc_reader.ReadInt();
									list = new List<int>();
									for (int k = 0; k < num15; k++)
									{
										list.Add(toc_reader.ReadInt());
									}
									for (int l = 0; l < num15; l++)
									{
										int num16 = toc_reader.ReadInt();
										long position2 = toc_reader.Position;
										toc_reader.Position = num16 - 12;
										Guid guid = toc_reader.ReadGuid();
										int index = toc_reader.ReadInt();
										int num17 = toc_reader.ReadInt();
										int num18 = toc_reader.ReadInt();
										if (!parent.chunkList.ContainsKey(guid))
										{
											parent.chunkList.Add(guid, new ChunkAssetEntry());
										}
										ChunkAssetEntry chunkAssetEntry = parent.chunkList[guid];
										chunkAssetEntry.Id = guid;
										chunkAssetEntry.Size = num18;
										chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
										chunkAssetEntry.ExtraData = new AssetExtraData();
										chunkAssetEntry.ExtraData.CasPath = parent.fs.GetFilePath(index);
										chunkAssetEntry.ExtraData.DataOffset = num17;
										parent.chunkList[guid].IsTocChunk = true;
										toc_reader.Position = position2;
									}
								}
							}
						}
					}
					sbIndex++;
				}
			}
		}
	}


}
