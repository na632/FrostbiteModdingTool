using Frosty.Hash;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FrostySdk.Managers
{
	public class AssetManager : ILoggable
	{
        public static AssetManager Instance;

		internal class BinarySbDataHelper
		{
			private Dictionary<string, byte[]> ebxDataFiles = new Dictionary<string, byte[]>();

			private Dictionary<string, byte[]> resDataFiles = new Dictionary<string, byte[]>();

			private Dictionary<string, byte[]> chunkDataFiles = new Dictionary<string, byte[]>();

			private AssetManager am;

			public BinarySbDataHelper(AssetManager inParent)
			{
				am = inParent;
			}

			public void FilterAndAddBundleData(DbObject baseList, DbObject deltaList)
			{
				FilterBinaryBundleData(baseList, deltaList, "ebx", ebxDataFiles);
				FilterBinaryBundleData(baseList, deltaList, "res", resDataFiles);
				FilterBinaryBundleData(baseList, deltaList, "chunks", chunkDataFiles);
			}

			public void RemoveEbxData(string name)
			{
				ebxDataFiles.Remove(name);
			}

			public void RemoveResData(string name)
			{
				resDataFiles.Remove(name);
			}

			public void RemoveChunkData(string name)
			{
				chunkDataFiles.Remove(name);
			}

			public void WriteToCache(AssetManager am)
			{
				if (ebxDataFiles.Count + resDataFiles.Count + chunkDataFiles.Count != 0)
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(am.fs.CacheName + "_sbdata.cas", FileMode.Create)))
					{
						foreach (KeyValuePair<string, byte[]> ebxDataFile in ebxDataFiles)
						{
							am.ebxList[ebxDataFile.Key].ExtraData.DataOffset = binaryWriter.BaseStream.Position;
							binaryWriter.Write(ebxDataFile.Value);
						}
						foreach (KeyValuePair<string, byte[]> resDataFile in resDataFiles)
						{
							am.resList[resDataFile.Key].ExtraData.DataOffset = binaryWriter.BaseStream.Position;
							binaryWriter.Write(resDataFile.Value);
						}
						foreach (KeyValuePair<string, byte[]> chunkDataFile in chunkDataFiles)
						{
							Guid key = new Guid(chunkDataFile.Key);
							am.chunkList[key].ExtraData.DataOffset = binaryWriter.BaseStream.Position;
							binaryWriter.Write(chunkDataFile.Value);
						}
					}
					ebxDataFiles.Clear();
					resDataFiles.Clear();
					chunkDataFiles.Clear();
				}
			}

			private void FilterBinaryBundleData(DbObject baseList, DbObject deltaList, string listName, Dictionary<string, byte[]> dataFiles)
			{
				foreach (DbObject item in deltaList.GetValue<DbObject>(listName))
				{
					Sha1 value = item.GetValue<Sha1>("sha1");
					string text = item.GetValue<string>("name");
					if (text == null)
					{
						text = item.GetValue<Guid>("id").ToString();
					}
					if (!dataFiles.ContainsKey(text))
					{
						bool flag = false;
						if (baseList != null)
						{
							foreach (DbObject item2 in baseList.GetValue<DbObject>(listName))
							{
								if (item2.GetValue<Sha1>("sha1") == value)
								{
									item.SetValue("size", item2.GetValue("size", 0L));
									item.SetValue("originalSize", item2.GetValue("originalSize", 0L));
									item.SetValue("offset", item2.GetValue("offset", 0L));
									item.RemoveValue("data");
									flag = true;
									break;
								}
							}
						}
						if (!flag)
						{
							byte[] array = Utils.CompressFile(item.GetValue<byte[]>("data"));
							dataFiles.Add(text, array);
							item.SetValue("size", array.Length);
							item.AddValue("cache", true);
							item.RemoveValue("sb");
						}
					}
				}
			}
		}

		internal class BaseBundleInfo
		{
			public string Name;

			public long Offset;

			public long Size;
		}

		internal interface IAssetLoader
		{
			void Load(AssetManager parent, BinarySbDataHelper helper);
		}

		internal class EdgeAssetLoader : IAssetLoader
		{
			public void Load(AssetManager parent, BinarySbDataHelper helper)
			{
				foreach (string superBundle in parent.fs.SuperBundles)
				{
					DbObject dbObject = parent.ProcessTocChunks($"{superBundle}.toc", helper);
					if (dbObject != null)
					{
						parent.WriteToLog("Loading data ({0})", superBundle);
						using (NativeReader nativeReader = new NativeReader(new FileStream(parent.fs.ResolvePath($"{superBundle}.sb"), FileMode.Open, FileAccess.Read)))
						{
							DbObject value = dbObject.GetValue<DbObject>("bundles");
							for (int i = 0; i < value.GetValue<DbObject>("names").Count; i++)
							{
								string name = value.GetValue<DbObject>("names")[i] as string;
								int num = (int)value.GetValue<DbObject>("offsets")[i];
								int num2 = (int)value.GetValue<DbObject>("sizes")[i];
								parent.bundles.Add(new BundleEntry
								{
									Name = name,
									SuperBundleId = parent.superBundles.Count - 1
								});
								int bundleId = parent.bundles.Count - 1;
								DbObject sb = null;
								using (DbReader dbReader = new DbReader(nativeReader.CreateViewStream(num, num2), parent.fs.CreateDeobfuscator()))
								{
									sb = dbReader.ReadDbObject();
								}
								parent.ProcessBundleEbx(sb, bundleId, helper);
								parent.ProcessBundleRes(sb, bundleId, helper);
								parent.ProcessBundleChunks(sb, bundleId, helper);
							}
						}
					}
				}
			}
		}

		internal class AnthemAssetLoader : IAssetLoader
		{
			public void Load(AssetManager parent, BinarySbDataHelper helper)
			{
				foreach (CatalogInfo item5 in parent.fs.EnumerateCatalogInfos())
				{
					foreach (string sbName in item5.SuperBundles.Keys)
					{
						SuperBundleEntry superBundleEntry = parent.superBundles.Find((SuperBundleEntry a) => a.Name == sbName);
						int num = -1;
						if (superBundleEntry != null)
						{
							num = parent.superBundles.IndexOf(superBundleEntry);
						}
						else
						{
							parent.superBundles.Add(new SuperBundleEntry
							{
								Name = sbName
							});
							num = parent.superBundles.Count - 1;
						}
						parent.WriteToLog("Loading data ({0})", sbName);
						string text = sbName.Replace("win32", item5.Name);
						if (parent.fs.ResolvePath("native_data/" + text + ".toc") == "")
						{
							text = sbName;
						}
						List<BaseBundleInfo> list = new List<BaseBundleInfo>();
						List<BaseBundleInfo> list2 = new List<BaseBundleInfo>();
						string text2 = parent.fs.ResolvePath($"native_data/{text}.toc");
						if (text2 != "")
						{
							int[] array = new int[12];
							byte[] array2 = null;
							using (NativeReader nativeReader = new NativeReader(new FileStream(text2, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
							{
								for (int i = 0; i < 12; i++)
								{
									array[i] = nativeReader.ReadInt(Endian.Big);
								}
								array2 = nativeReader.ReadToEnd();
							}
							if (array2.Length != 0)
							{
								using (NativeReader nativeReader2 = new NativeReader(new MemoryStream(array2)))
								{
									List<int> list3 = new List<int>();
									if (array[0] != 0)
									{
										for (int j = 0; j < array[2]; j++)
										{
											list3.Add(nativeReader2.ReadInt(Endian.Big));
										}
										nativeReader2.Position = array[1] - 48;
										for (int k = 0; k < array[2]; k++)
										{
											int num2 = nativeReader2.ReadInt(Endian.Big);
											uint num3 = nativeReader2.ReadUInt(Endian.Big);
											long offset = nativeReader2.ReadLong(Endian.Big);
											long position = nativeReader2.Position;
											nativeReader2.Position = array[8] - 48 + num2;
											string name2 = nativeReader2.ReadNullTerminatedString();
											nativeReader2.Position = position;
											BaseBundleInfo item = new BaseBundleInfo
											{
												Name = name2,
												Offset = offset,
												Size = num3
											};
											list.Add(item);
										}
									}
									if (array[3] != 0)
									{
										nativeReader2.Position = array[3] - 48;
										List<int> list4 = new List<int>();
										for (int l = 0; l < array[5]; l++)
										{
											list4.Add(nativeReader2.ReadInt(Endian.Big));
										}
										nativeReader2.Position = array[4] - 48;
										List<Guid> list5 = new List<Guid>();
										for (int m = 0; m < array[5]; m++)
										{
											byte[] array3 = nativeReader2.ReadBytes(16);
											Guid value = new Guid(new byte[16]
											{
												array3[15],
												array3[14],
												array3[13],
												array3[12],
												array3[11],
												array3[10],
												array3[9],
												array3[8],
												array3[7],
												array3[6],
												array3[5],
												array3[4],
												array3[3],
												array3[2],
												array3[1],
												array3[0]
											});
											int num4 = nativeReader2.ReadInt(Endian.Big) & 0xFFFFFF;
											while (list5.Count <= num4)
											{
												list5.Add(Guid.Empty);
											}
											list5[num4 / 3] = value;
										}
										nativeReader2.Position = array[6] - 48;
										for (int n = 0; n < array[5]; n++)
										{
											nativeReader2.ReadByte();
											bool patch = nativeReader2.ReadBoolean();
											byte catalog = nativeReader2.ReadByte();
											byte cas = nativeReader2.ReadByte();
											uint num5 = nativeReader2.ReadUInt(Endian.Big);
											uint num6 = nativeReader2.ReadUInt(Endian.Big);
											ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
											chunkAssetEntry.Id = list5[n];
											chunkAssetEntry.Size = num6;
											chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
											chunkAssetEntry.ExtraData = new AssetExtraData();
											chunkAssetEntry.ExtraData.CasPath = parent.fs.GetFilePath(catalog, cas, patch);
											chunkAssetEntry.ExtraData.DataOffset = num5;
											if (parent.chunkList.ContainsKey(chunkAssetEntry.Id))
											{
												parent.chunkList.Remove(chunkAssetEntry.Id);
											}
											parent.chunkList.Add(chunkAssetEntry.Id, chunkAssetEntry);
										}
									}
								}
							}
						}
						text2 = parent.fs.ResolvePath($"native_patch/{text}.toc");
						if (text2 != "")
						{
							int[] array4 = new int[12];
							byte[] array5 = null;
							using (NativeReader nativeReader3 = new NativeReader(new FileStream(text2, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
							{
								for (int num7 = 0; num7 < 12; num7++)
								{
									array4[num7] = nativeReader3.ReadInt(Endian.Big);
								}
								array5 = nativeReader3.ReadToEnd();
							}
							if (array5.Length != 0)
							{
								new NativeReader(new FileStream(parent.fs.ResolvePath($"{text}.sb"), FileMode.Open, FileAccess.Read));
								using (NativeReader nativeReader4 = new NativeReader(new MemoryStream(array5)))
								{
									List<int> list6 = new List<int>();
									if (array4[0] != 0)
									{
										for (int num8 = 0; num8 < array4[2]; num8++)
										{
											list6.Add(nativeReader4.ReadInt(Endian.Big));
										}
										nativeReader4.Position = array4[1] - 48;
										for (int num9 = 0; num9 < array4[2]; num9++)
										{
											int num10 = nativeReader4.ReadInt(Endian.Big);
											uint num11 = nativeReader4.ReadUInt(Endian.Big);
											long offset2 = nativeReader4.ReadLong(Endian.Big);
											long position2 = nativeReader4.Position;
											nativeReader4.Position = array4[8] - 48 + num10;
											string name = nativeReader4.ReadNullTerminatedString();
											nativeReader4.Position = position2;
											int num12 = list.FindIndex((BaseBundleInfo bbi) => bbi.Name.Equals(name));
											if (num12 != -1)
											{
												list.RemoveAt(num12);
											}
											BaseBundleInfo item2 = new BaseBundleInfo
											{
												Name = name,
												Offset = offset2,
												Size = num11
											};
											list2.Add(item2);
										}
									}
									if (array4[3] != 0)
									{
										nativeReader4.Position = array4[3] - 48;
										List<int> list7 = new List<int>();
										for (int num13 = 0; num13 < array4[5]; num13++)
										{
											list7.Add(nativeReader4.ReadInt(Endian.Big));
										}
										nativeReader4.Position = array4[4] - 48;
										List<Guid> list8 = new List<Guid>();
										for (int num14 = 0; num14 < array4[5]; num14++)
										{
											byte[] array6 = nativeReader4.ReadBytes(16);
											Guid value2 = new Guid(new byte[16]
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
											int num15 = nativeReader4.ReadInt(Endian.Big) & 0xFFFFFF;
											while (list8.Count <= num15)
											{
												list8.Add(Guid.Empty);
											}
											list8[num15 / 3] = value2;
										}
										nativeReader4.Position = array4[6] - 48;
										for (int num16 = 0; num16 < array4[5]; num16++)
										{
											nativeReader4.ReadByte();
											bool patch2 = nativeReader4.ReadBoolean();
											byte catalog2 = nativeReader4.ReadByte();
											byte cas2 = nativeReader4.ReadByte();
											uint num17 = nativeReader4.ReadUInt(Endian.Big);
											uint num18 = nativeReader4.ReadUInt(Endian.Big);
											ChunkAssetEntry chunkAssetEntry2 = new ChunkAssetEntry();
											chunkAssetEntry2.Id = list8[num16];
											chunkAssetEntry2.Size = num18;
											chunkAssetEntry2.Location = AssetDataLocation.CasNonIndexed;
											chunkAssetEntry2.ExtraData = new AssetExtraData();
											chunkAssetEntry2.ExtraData.CasPath = parent.fs.GetFilePath(catalog2, cas2, patch2);
											chunkAssetEntry2.ExtraData.DataOffset = num17;
											if (parent.chunkList.ContainsKey(chunkAssetEntry2.Id))
											{
												parent.chunkList.Remove(chunkAssetEntry2.Id);
											}
											parent.chunkList.Add(chunkAssetEntry2.Id, chunkAssetEntry2);
										}
									}
								}
							}
						}
						if (list.Count > 0)
						{
							using (NativeReader nativeReader5 = new NativeReader(new FileStream(parent.fs.ResolvePath($"native_data/{text}.sb"), FileMode.Open, FileAccess.Read)))
							{
								foreach (BaseBundleInfo item6 in list)
								{
									BundleEntry item3 = new BundleEntry
									{
										Name = item6.Name,
										SuperBundleId = num
									};
									parent.bundles.Add(item3);
									Stream inStream = nativeReader5.CreateViewStream(item6.Offset, item6.Size);
									DbObject dbObject = null;
									using (BinarySbReader binarySbReader = new BinarySbReader(inStream, 0L, parent.fs.CreateDeobfuscator()))
									{
										uint num19 = binarySbReader.ReadUInt(Endian.Big);
										uint num20 = binarySbReader.ReadUInt(Endian.Big) + num19;
										uint num21 = binarySbReader.ReadUInt(Endian.Big);
										binarySbReader.Position += 20L;
										dbObject = binarySbReader.ReadDbObject();
										binarySbReader.Position = num21;
										bool[] array7 = new bool[binarySbReader.TotalCount];
										for (uint num22 = 0u; num22 < binarySbReader.TotalCount; num22++)
										{
											array7[num22] = binarySbReader.ReadBoolean();
										}
										binarySbReader.Position = num20;
										bool flag = false;
										int num23 = 0;
										int num24 = 0;
										int num25 = 0;
										for (int num26 = 0; num26 < dbObject.GetValue<DbObject>("ebx").Count; num26++)
										{
											if (array7[num25++])
											{
												binarySbReader.ReadByte();
												flag = binarySbReader.ReadBoolean();
												num23 = binarySbReader.ReadByte();
												num24 = binarySbReader.ReadByte();
											}
											DbObject dbObject2 = dbObject.GetValue<DbObject>("ebx")[num26] as DbObject;
											int num27 = binarySbReader.ReadInt(Endian.Big);
											int num28 = binarySbReader.ReadInt(Endian.Big);
											dbObject2.SetValue("catalog", num23);
											dbObject2.SetValue("cas", num24);
											dbObject2.SetValue("offset", num27);
											dbObject2.SetValue("size", num28);
											if (flag)
											{
												dbObject2.SetValue("patch", true);
											}
										}
										for (int num29 = 0; num29 < dbObject.GetValue<DbObject>("res").Count; num29++)
										{
											if (array7[num25++])
											{
												binarySbReader.ReadByte();
												flag = binarySbReader.ReadBoolean();
												num23 = binarySbReader.ReadByte();
												num24 = binarySbReader.ReadByte();
											}
											DbObject dbObject3 = dbObject.GetValue<DbObject>("res")[num29] as DbObject;
											int num30 = binarySbReader.ReadInt(Endian.Big);
											int num31 = binarySbReader.ReadInt(Endian.Big);
											dbObject3.SetValue("catalog", num23);
											dbObject3.SetValue("cas", num24);
											dbObject3.SetValue("offset", num30);
											dbObject3.SetValue("size", num31);
											if (flag)
											{
												dbObject3.SetValue("patch", true);
											}
										}
										for (int num32 = 0; num32 < dbObject.GetValue<DbObject>("chunks").Count; num32++)
										{
											if (array7[num25++])
											{
												binarySbReader.ReadByte();
												flag = binarySbReader.ReadBoolean();
												num23 = binarySbReader.ReadByte();
												num24 = binarySbReader.ReadByte();
											}
											DbObject dbObject4 = dbObject.GetValue<DbObject>("chunks")[num32] as DbObject;
											int num33 = binarySbReader.ReadInt(Endian.Big);
											int num34 = binarySbReader.ReadInt(Endian.Big);
											dbObject4.SetValue("catalog", num23);
											dbObject4.SetValue("cas", num24);
											dbObject4.SetValue("offset", num33);
											dbObject4.SetValue("size", num34);
											if (flag)
											{
												dbObject4.SetValue("patch", true);
											}
										}
									}
									parent.ProcessBundleEbx(dbObject, parent.bundles.Count - 1, helper);
									parent.ProcessBundleRes(dbObject, parent.bundles.Count - 1, helper);
									parent.ProcessBundleChunks(dbObject, parent.bundles.Count - 1, helper);
								}
							}
						}
						if (list2.Count > 0)
						{
							using (NativeReader nativeReader6 = new NativeReader(new FileStream(parent.fs.ResolvePath($"native_patch/{text}.sb"), FileMode.Open, FileAccess.Read)))
							{
								foreach (BaseBundleInfo item7 in list2)
								{
									BundleEntry item4 = new BundleEntry
									{
										Name = item7.Name,
										SuperBundleId = num
									};
									parent.bundles.Add(item4);
									Stream inStream2 = nativeReader6.CreateViewStream(item7.Offset, item7.Size);
									DbObject dbObject5 = null;
									using (BinarySbReader binarySbReader2 = new BinarySbReader(inStream2, 0L, parent.fs.CreateDeobfuscator()))
									{
										uint num35 = binarySbReader2.ReadUInt(Endian.Big);
										uint num36 = binarySbReader2.ReadUInt(Endian.Big) + num35;
										uint num37 = binarySbReader2.ReadUInt(Endian.Big);
										binarySbReader2.Position += 20L;
										dbObject5 = binarySbReader2.ReadDbObject();
										binarySbReader2.Position = num37;
										bool[] array8 = new bool[binarySbReader2.TotalCount];
										for (uint num38 = 0u; num38 < binarySbReader2.TotalCount; num38++)
										{
											array8[num38] = binarySbReader2.ReadBoolean();
										}
										binarySbReader2.Position = num36;
										bool flag2 = false;
										int num39 = 0;
										int num40 = 0;
										int num41 = 0;
										for (int num42 = 0; num42 < dbObject5.GetValue<DbObject>("ebx").Count; num42++)
										{
											if (array8[num41++])
											{
												binarySbReader2.ReadByte();
												flag2 = binarySbReader2.ReadBoolean();
												num39 = binarySbReader2.ReadByte();
												num40 = binarySbReader2.ReadByte();
											}
											DbObject dbObject6 = dbObject5.GetValue<DbObject>("ebx")[num42] as DbObject;
											int num43 = binarySbReader2.ReadInt(Endian.Big);
											int num44 = binarySbReader2.ReadInt(Endian.Big);
											dbObject6.SetValue("catalog", num39);
											dbObject6.SetValue("cas", num40);
											dbObject6.SetValue("offset", num43);
											dbObject6.SetValue("size", num44);
											if (flag2)
											{
												dbObject6.SetValue("patch", true);
											}
										}
										for (int num45 = 0; num45 < dbObject5.GetValue<DbObject>("res").Count; num45++)
										{
											if (array8[num41++])
											{
												binarySbReader2.ReadByte();
												flag2 = binarySbReader2.ReadBoolean();
												num39 = binarySbReader2.ReadByte();
												num40 = binarySbReader2.ReadByte();
											}
											DbObject dbObject7 = dbObject5.GetValue<DbObject>("res")[num45] as DbObject;
											int num46 = binarySbReader2.ReadInt(Endian.Big);
											int num47 = binarySbReader2.ReadInt(Endian.Big);
											dbObject7.SetValue("catalog", num39);
											dbObject7.SetValue("cas", num40);
											dbObject7.SetValue("offset", num46);
											dbObject7.SetValue("size", num47);
											if (flag2)
											{
												dbObject7.SetValue("patch", true);
											}
										}
										for (int num48 = 0; num48 < dbObject5.GetValue<DbObject>("chunks").Count; num48++)
										{
											if (array8[num41++])
											{
												binarySbReader2.ReadByte();
												flag2 = binarySbReader2.ReadBoolean();
												num39 = binarySbReader2.ReadByte();
												num40 = binarySbReader2.ReadByte();
											}
											DbObject dbObject8 = dbObject5.GetValue<DbObject>("chunks")[num48] as DbObject;
											int num49 = binarySbReader2.ReadInt(Endian.Big);
											int num50 = binarySbReader2.ReadInt(Endian.Big);
											dbObject8.SetValue("catalog", num39);
											dbObject8.SetValue("cas", num40);
											dbObject8.SetValue("offset", num49);
											dbObject8.SetValue("size", num50);
											if (flag2)
											{
												dbObject8.SetValue("patch", true);
											}
										}
									}
									parent.ProcessBundleEbx(dbObject5, parent.bundles.Count - 1, helper);
									parent.ProcessBundleRes(dbObject5, parent.bundles.Count - 1, helper);
									parent.ProcessBundleChunks(dbObject5, parent.bundles.Count - 1, helper);
								}
							}
						}
					}
				}
			}
		}

		internal class PVZAssetLoader : IAssetLoader
		{
			public void Load(AssetManager parent, BinarySbDataHelper helper)
			{
				foreach (string superBundle in parent.fs.SuperBundles)
				{
					int num = -1;
					parent.superBundles.Add(new SuperBundleEntry
					{
						Name = superBundle
					});
					num = parent.superBundles.Count - 1;
					parent.WriteToLog("Loading data ({0})", superBundle);
					string text = "";
					text = superBundle;
					List<BaseBundleInfo> list = new List<BaseBundleInfo>();
					List<BaseBundleInfo> list2 = new List<BaseBundleInfo>();
					string text2 = parent.fs.ResolvePath($"native_data/{text}.toc");
					if (text2 != "")
					{
						int[] array = new int[12];
						byte[] array2 = null;
						using (NativeReader nativeReader = new NativeReader(new FileStream(text2, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
						{
							for (int i = 0; i < 12; i++)
							{
								array[i] = nativeReader.ReadInt(Endian.Big);
							}
							array2 = nativeReader.ReadToEnd();
						}
						if (array2.Length != 0)
						{
							using (NativeReader nativeReader2 = new NativeReader(new MemoryStream(array2)))
							{
								List<int> list3 = new List<int>();
								if (array[0] != 0)
								{
									for (int j = 0; j < array[2]; j++)
									{
										list3.Add(nativeReader2.ReadInt(Endian.Big));
									}
									nativeReader2.Position = array[1] - 48;
									for (int k = 0; k < array[2]; k++)
									{
										int num2 = nativeReader2.ReadInt(Endian.Big);
										uint num3 = nativeReader2.ReadUInt(Endian.Big);
										long offset = nativeReader2.ReadLong(Endian.Big);
										long position = nativeReader2.Position;
										nativeReader2.Position = array[8] - 48 + num2;
										string name2 = nativeReader2.ReadNullTerminatedString();
										nativeReader2.Position = position;
										BaseBundleInfo item = new BaseBundleInfo
										{
											Name = name2,
											Offset = offset,
											Size = num3
										};
										list.Add(item);
									}
								}
								if (array[3] != 0)
								{
									nativeReader2.Position = array[3] - 48;
									List<int> list4 = new List<int>();
									for (int l = 0; l < array[5]; l++)
									{
										list4.Add(nativeReader2.ReadInt(Endian.Big));
									}
									nativeReader2.Position = array[4] - 48;
									List<Guid> list5 = new List<Guid>();
									for (int m = 0; m < array[5]; m++)
									{
										byte[] array3 = nativeReader2.ReadBytes(16);
										Guid value = new Guid(new byte[16]
										{
											array3[15],
											array3[14],
											array3[13],
											array3[12],
											array3[11],
											array3[10],
											array3[9],
											array3[8],
											array3[7],
											array3[6],
											array3[5],
											array3[4],
											array3[3],
											array3[2],
											array3[1],
											array3[0]
										});
										int num4 = nativeReader2.ReadInt(Endian.Big) & 0xFFFFFF;
										while (list5.Count <= num4)
										{
											list5.Add(Guid.Empty);
										}
										list5[num4 / 3] = value;
									}
									nativeReader2.Position = array[6] - 48;
									for (int n = 0; n < array[5]; n++)
									{
										nativeReader2.ReadByte();
										bool patch = nativeReader2.ReadBoolean();
										byte catalog = nativeReader2.ReadByte();
										byte cas = nativeReader2.ReadByte();
										uint num5 = nativeReader2.ReadUInt(Endian.Big);
										uint num6 = nativeReader2.ReadUInt(Endian.Big);
										ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
										chunkAssetEntry.Id = list5[n];
										chunkAssetEntry.Size = num6;
										chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
										chunkAssetEntry.ExtraData = new AssetExtraData();
										chunkAssetEntry.ExtraData.CasPath = parent.fs.GetFilePath(catalog, cas, patch);
										chunkAssetEntry.ExtraData.DataOffset = num5;
										if (parent.chunkList.ContainsKey(chunkAssetEntry.Id))
										{
											parent.chunkList.Remove(chunkAssetEntry.Id);
										}
										parent.chunkList.Add(chunkAssetEntry.Id, chunkAssetEntry);
									}
								}
							}
						}
					}
					text2 = parent.fs.ResolvePath($"native_patch/{text}.toc");
					if (text2 != "")
					{
						int[] array4 = new int[12];
						byte[] array5 = null;
						using (NativeReader nativeReader3 = new NativeReader(new FileStream(text2, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
						{
							for (int num7 = 0; num7 < 12; num7++)
							{
								array4[num7] = nativeReader3.ReadInt(Endian.Big);
							}
							array5 = nativeReader3.ReadToEnd();
						}
						if (array5.Length != 0)
						{
							new NativeReader(new FileStream(parent.fs.ResolvePath($"{text}.sb"), FileMode.Open, FileAccess.Read));
							using (NativeReader nativeReader4 = new NativeReader(new MemoryStream(array5)))
							{
								List<int> list6 = new List<int>();
								if (array4[0] != 0)
								{
									for (int num8 = 0; num8 < array4[2]; num8++)
									{
										list6.Add(nativeReader4.ReadInt(Endian.Big));
									}
									nativeReader4.Position = array4[1] - 48;
									for (int num9 = 0; num9 < array4[2]; num9++)
									{
										int num10 = nativeReader4.ReadInt(Endian.Big);
										uint num11 = nativeReader4.ReadUInt(Endian.Big);
										long offset2 = nativeReader4.ReadLong(Endian.Big);
										long position2 = nativeReader4.Position;
										nativeReader4.Position = array4[8] - 48 + num10;
										string name = nativeReader4.ReadNullTerminatedString();
										nativeReader4.Position = position2;
										int num12 = list.FindIndex((BaseBundleInfo bbi) => bbi.Name.Equals(name));
										if (num12 != -1)
										{
											list.RemoveAt(num12);
										}
										BaseBundleInfo item2 = new BaseBundleInfo
										{
											Name = name,
											Offset = offset2,
											Size = num11
										};
										list2.Add(item2);
									}
								}
								if (array4[3] != 0)
								{
									nativeReader4.Position = array4[3] - 48;
									List<int> list7 = new List<int>();
									for (int num13 = 0; num13 < array4[5]; num13++)
									{
										list7.Add(nativeReader4.ReadInt(Endian.Big));
									}
									nativeReader4.Position = array4[4] - 48;
									List<Guid> list8 = new List<Guid>();
									for (int num14 = 0; num14 < array4[5]; num14++)
									{
										byte[] array6 = nativeReader4.ReadBytes(16);
										Guid value2 = new Guid(new byte[16]
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
										int num15 = nativeReader4.ReadInt(Endian.Big) & 0xFFFFFF;
										while (list8.Count <= num15)
										{
											list8.Add(Guid.Empty);
										}
										list8[num15 / 3] = value2;
									}
									nativeReader4.Position = array4[6] - 48;
									for (int num16 = 0; num16 < array4[5]; num16++)
									{
										nativeReader4.ReadByte();
										bool patch2 = nativeReader4.ReadBoolean();
										byte catalog2 = nativeReader4.ReadByte();
										byte cas2 = nativeReader4.ReadByte();
										uint num17 = nativeReader4.ReadUInt(Endian.Big);
										uint num18 = nativeReader4.ReadUInt(Endian.Big);
										ChunkAssetEntry chunkAssetEntry2 = new ChunkAssetEntry();
										chunkAssetEntry2.Id = list8[num16];
										chunkAssetEntry2.Size = num18;
										chunkAssetEntry2.Location = AssetDataLocation.CasNonIndexed;
										chunkAssetEntry2.ExtraData = new AssetExtraData();
										chunkAssetEntry2.ExtraData.CasPath = parent.fs.GetFilePath(catalog2, cas2, patch2);
										chunkAssetEntry2.ExtraData.DataOffset = num17;
										if (parent.chunkList.ContainsKey(chunkAssetEntry2.Id))
										{
											parent.chunkList.Remove(chunkAssetEntry2.Id);
										}
										parent.chunkList.Add(chunkAssetEntry2.Id, chunkAssetEntry2);
									}
								}
							}
						}
					}
					if (list.Count > 0)
					{
						using (NativeReader nativeReader5 = new NativeReader(new FileStream(parent.fs.ResolvePath($"native_data/{text}.sb"), FileMode.Open, FileAccess.Read)))
						{
							foreach (BaseBundleInfo item5 in list)
							{
								BundleEntry item3 = new BundleEntry
								{
									Name = item5.Name,
									SuperBundleId = num
								};
								parent.bundles.Add(item3);
								Stream inStream = nativeReader5.CreateViewStream(item5.Offset, item5.Size);
								DbObject dbObject = null;
								using (BinarySbReader binarySbReader = new BinarySbReader(inStream, 0L, parent.fs.CreateDeobfuscator()))
								{
									uint num19 = binarySbReader.ReadUInt(Endian.Big);
									uint num20 = binarySbReader.ReadUInt(Endian.Big) + num19;
									uint num21 = binarySbReader.ReadUInt(Endian.Big);
									binarySbReader.Position += 20L;
									dbObject = binarySbReader.ReadDbObject();
									binarySbReader.Position = num21;
									bool[] array7 = new bool[binarySbReader.TotalCount];
									for (uint num22 = 0u; num22 < binarySbReader.TotalCount; num22++)
									{
										array7[num22] = binarySbReader.ReadBoolean();
									}
									binarySbReader.Position = num20;
									bool flag = false;
									int num23 = 0;
									int num24 = 0;
									int num25 = 0;
									for (int num26 = 0; num26 < dbObject.GetValue<DbObject>("ebx").Count; num26++)
									{
										if (array7[num25++])
										{
											binarySbReader.ReadByte();
											flag = binarySbReader.ReadBoolean();
											num23 = binarySbReader.ReadByte();
											num24 = binarySbReader.ReadByte();
										}
										DbObject dbObject2 = dbObject.GetValue<DbObject>("ebx")[num26] as DbObject;
										int num27 = binarySbReader.ReadInt(Endian.Big);
										int num28 = binarySbReader.ReadInt(Endian.Big);
										dbObject2.SetValue("catalog", num23);
										dbObject2.SetValue("cas", num24);
										dbObject2.SetValue("offset", num27);
										dbObject2.SetValue("size", num28);
										if (flag)
										{
											dbObject2.SetValue("patch", true);
										}
									}
									for (int num29 = 0; num29 < dbObject.GetValue<DbObject>("res").Count; num29++)
									{
										if (array7[num25++])
										{
											binarySbReader.ReadByte();
											flag = binarySbReader.ReadBoolean();
											num23 = binarySbReader.ReadByte();
											num24 = binarySbReader.ReadByte();
										}
										DbObject dbObject3 = dbObject.GetValue<DbObject>("res")[num29] as DbObject;
										int num30 = binarySbReader.ReadInt(Endian.Big);
										int num31 = binarySbReader.ReadInt(Endian.Big);
										dbObject3.SetValue("catalog", num23);
										dbObject3.SetValue("cas", num24);
										dbObject3.SetValue("offset", num30);
										dbObject3.SetValue("size", num31);
										if (flag)
										{
											dbObject3.SetValue("patch", true);
										}
									}
									for (int num32 = 0; num32 < dbObject.GetValue<DbObject>("chunks").Count; num32++)
									{
										if (array7[num25++])
										{
											binarySbReader.ReadByte();
											flag = binarySbReader.ReadBoolean();
											num23 = binarySbReader.ReadByte();
											num24 = binarySbReader.ReadByte();
										}
										DbObject dbObject4 = dbObject.GetValue<DbObject>("chunks")[num32] as DbObject;
										int num33 = binarySbReader.ReadInt(Endian.Big);
										int num34 = binarySbReader.ReadInt(Endian.Big);
										dbObject4.SetValue("catalog", num23);
										dbObject4.SetValue("cas", num24);
										dbObject4.SetValue("offset", num33);
										dbObject4.SetValue("size", num34);
										if (flag)
										{
											dbObject4.SetValue("patch", true);
										}
									}
								}
								parent.ProcessBundleEbx(dbObject, parent.bundles.Count - 1, helper);
								parent.ProcessBundleRes(dbObject, parent.bundles.Count - 1, helper);
								parent.ProcessBundleChunks(dbObject, parent.bundles.Count - 1, helper);
							}
						}
					}
					if (list2.Count > 0)
					{
						using (NativeReader nativeReader6 = new NativeReader(new FileStream(parent.fs.ResolvePath($"native_patch/{text}.sb"), FileMode.Open, FileAccess.Read)))
						{
							foreach (BaseBundleInfo item6 in list2)
							{
								BundleEntry item4 = new BundleEntry
								{
									Name = item6.Name,
									SuperBundleId = num
								};
								parent.bundles.Add(item4);
								Stream inStream2 = nativeReader6.CreateViewStream(item6.Offset, item6.Size);
								DbObject dbObject5 = null;
								using (BinarySbReader binarySbReader2 = new BinarySbReader(inStream2, 0L, parent.fs.CreateDeobfuscator()))
								{
									uint num35 = binarySbReader2.ReadUInt(Endian.Big);
									uint num36 = binarySbReader2.ReadUInt(Endian.Big) + num35;
									uint num37 = binarySbReader2.ReadUInt(Endian.Big);
									binarySbReader2.Position += 20L;
									dbObject5 = binarySbReader2.ReadDbObject();
									binarySbReader2.Position = num37;
									bool[] array8 = new bool[binarySbReader2.TotalCount];
									for (uint num38 = 0u; num38 < binarySbReader2.TotalCount; num38++)
									{
										array8[num38] = binarySbReader2.ReadBoolean();
									}
									binarySbReader2.Position = num36;
									bool flag2 = false;
									int num39 = 0;
									int num40 = 0;
									int num41 = 0;
									for (int num42 = 0; num42 < dbObject5.GetValue<DbObject>("ebx").Count; num42++)
									{
										if (array8[num41++])
										{
											binarySbReader2.ReadByte();
											flag2 = binarySbReader2.ReadBoolean();
											num39 = binarySbReader2.ReadByte();
											num40 = binarySbReader2.ReadByte();
										}
										DbObject dbObject6 = dbObject5.GetValue<DbObject>("ebx")[num42] as DbObject;
										int num43 = binarySbReader2.ReadInt(Endian.Big);
										int num44 = binarySbReader2.ReadInt(Endian.Big);
										dbObject6.SetValue("catalog", num39);
										dbObject6.SetValue("cas", num40);
										dbObject6.SetValue("offset", num43);
										dbObject6.SetValue("size", num44);
										if (flag2)
										{
											dbObject6.SetValue("patch", true);
										}
									}
									for (int num45 = 0; num45 < dbObject5.GetValue<DbObject>("res").Count; num45++)
									{
										if (array8[num41++])
										{
											binarySbReader2.ReadByte();
											flag2 = binarySbReader2.ReadBoolean();
											num39 = binarySbReader2.ReadByte();
											num40 = binarySbReader2.ReadByte();
										}
										DbObject dbObject7 = dbObject5.GetValue<DbObject>("res")[num45] as DbObject;
										int num46 = binarySbReader2.ReadInt(Endian.Big);
										int num47 = binarySbReader2.ReadInt(Endian.Big);
										dbObject7.SetValue("catalog", num39);
										dbObject7.SetValue("cas", num40);
										dbObject7.SetValue("offset", num46);
										dbObject7.SetValue("size", num47);
										if (flag2)
										{
											dbObject7.SetValue("patch", true);
										}
									}
									for (int num48 = 0; num48 < dbObject5.GetValue<DbObject>("chunks").Count; num48++)
									{
										if (array8[num41++])
										{
											binarySbReader2.ReadByte();
											flag2 = binarySbReader2.ReadBoolean();
											num39 = binarySbReader2.ReadByte();
											num40 = binarySbReader2.ReadByte();
										}
										DbObject dbObject8 = dbObject5.GetValue<DbObject>("chunks")[num48] as DbObject;
										int num49 = binarySbReader2.ReadInt(Endian.Big);
										int num50 = binarySbReader2.ReadInt(Endian.Big);
										dbObject8.SetValue("catalog", num39);
										dbObject8.SetValue("cas", num40);
										dbObject8.SetValue("offset", num49);
										dbObject8.SetValue("size", num50);
										if (flag2)
										{
											dbObject8.SetValue("patch", true);
										}
									}
								}
								parent.ProcessBundleEbx(dbObject5, parent.bundles.Count - 1, helper);
								parent.ProcessBundleRes(dbObject5, parent.bundles.Count - 1, helper);
								parent.ProcessBundleChunks(dbObject5, parent.bundles.Count - 1, helper);
							}
						}
					}
				}
			}
		}

		internal class FifaAssetLoader : IAssetLoader
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
						int num = -1;
						if (superBundleEntry != null)
						{
							num = parent.superBundles.IndexOf(superBundleEntry);
						}
						else
						{
							parent.superBundles.Add(new SuperBundleEntry
							{
								Name = sbName
							});
							num = parent.superBundles.Count - 1;
						}
						parent.WriteToLog($"Loading data ({sbName})");
						parent.superBundles.Add(new SuperBundleEntry
						{
							Name = sbName
						});
						string arg = sbName;
						if (item2.SuperBundles[sbName])
						{
							arg = sbName.Replace("win32", item2.Name);
						}
						string text = parent.fs.ResolvePath($"{arg}.toc");
						if (text != "")
						{
							int num2 = 0;
							int num3 = 0;
							byte[] array = null;
							using (NativeReader nativeReader = new NativeReader(new FileStream(text, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
							{
								uint num4 = nativeReader.ReadUInt();
								num2 = nativeReader.ReadInt() - 12;
								num3 = nativeReader.ReadInt() - 12;
								array = nativeReader.ReadToEnd();
								if (num4 == 3286619587u)
								{
									using (Aes aes = Aes.Create())
									{
										aes.Key = key;
										aes.IV = key;
										aes.Padding = PaddingMode.None;
										ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
										using (MemoryStream stream = new MemoryStream(array))
										{
											using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
											{
												cryptoStream.Read(array, 0, array.Length);
											}
										}
									}
								}
							}
							if (array.Length != 0)
							{
								using (NativeReader nativeReader2 = new NativeReader(new MemoryStream(array)))
								{
									List<int> list = new List<int>();
									if (num2 > 0)
									{
										nativeReader2.Position = num2;
										int num5 = nativeReader2.ReadInt();
										for (int i = 0; i < num5; i++)
										{
											list.Add(nativeReader2.ReadInt());
										}
										for (int j = 0; j < num5; j++)
										{
											parent.logger.Log($"progress:{Math.Round( (double)j / (double)num5 * 100.0 , 2)}");
											int num6 = nativeReader2.ReadInt() - 12;
											long position = nativeReader2.Position;
											nativeReader2.Position = num6;
											int num7 = nativeReader2.ReadInt() - 1;
											List<BundleFileInfo> list2 = new List<BundleFileInfo>();
											MemoryStream memoryStream = new MemoryStream();
											int num8;
											do
											{
												num8 = nativeReader2.ReadInt();
												int num9 = nativeReader2.ReadInt();
												int num10 = nativeReader2.ReadInt();
												using (NativeReader nativeReader3 = new NativeReader(new FileStream(parent.fs.ResolvePath(parent.fs.GetFilePath(num8 & int.MaxValue)), FileMode.Open, FileAccess.Read)))
												{
													nativeReader3.Position = num9;
													memoryStream.Write(nativeReader3.ReadBytes(num10), 0, num10);
												}
												list2.Add(new BundleFileInfo(num8 & int.MaxValue, num9, num10));
											}
											while ((num8 & 2147483648u) != 0L);
											nativeReader2.Position = num7 - 12;
											int num11 = 0;
											string text2 = "";
											do
											{
												string str = nativeReader2.ReadNullTerminatedString();
												num11 = nativeReader2.ReadInt() - 1;
												text2 += str;
												if (num11 != -1)
												{
													nativeReader2.Position = num11 - 12;
												}
											}
											while (num11 != -1);
											text2 = Utils.ReverseString(text2);
											nativeReader2.Position = position;
											BundleEntry item = new BundleEntry
											{
												Name = text2,
												SuperBundleId = num
											};
											parent.bundles.Add(item);
											using (BinarySbReader binarySbReader = new BinarySbReader(memoryStream, 0L, parent.fs.CreateDeobfuscator()))
											{
												DbObject dbObject = binarySbReader.ReadDbObject();
												BundleFileInfo bundleFileInfo = list2[0];
												long num12 = bundleFileInfo.Offset + (dbObject.GetValue("dataOffset", 0L) + 4);
												long num13 = bundleFileInfo.Size - (dbObject.GetValue("dataOffset", 0L) + 4);
												int num14 = 0;
												foreach (DbObject item3 in dbObject.GetValue<DbObject>("ebx"))
												{
													if (num13 == 0L)
													{
														bundleFileInfo = list2[++num14];
														num13 = bundleFileInfo.Size;
														num12 = bundleFileInfo.Offset;
													}
													int value = item3.GetValue("size", 0);
													item3.SetValue("offset", num12);
													item3.SetValue("cas", bundleFileInfo.Index);
													num12 += value;
													num13 -= value;
												}
												foreach (DbObject item4 in dbObject.GetValue<DbObject>("res"))
												{
													if (num13 == 0L)
													{
														bundleFileInfo = list2[++num14];
														num13 = bundleFileInfo.Size;
														num12 = bundleFileInfo.Offset;
													}
													int value2 = item4.GetValue("size", 0);
													item4.SetValue("offset", num12);
													item4.SetValue("cas", bundleFileInfo.Index);
													num12 += value2;
													num13 -= value2;
												}
												foreach (DbObject item5 in dbObject.GetValue<DbObject>("chunks"))
												{
													if (num13 == 0L)
													{
														bundleFileInfo = list2[++num14];
														num13 = bundleFileInfo.Size;
														num12 = bundleFileInfo.Offset;
													}
													int value3 = item5.GetValue("size", 0);
													item5.SetValue("offset", num12);
													item5.SetValue("cas", bundleFileInfo.Index);
													num12 += value3;
													num13 -= value3;
												}
												parent.ProcessBundleEbx(dbObject, parent.bundles.Count - 1, helper);
												parent.ProcessBundleRes(dbObject, parent.bundles.Count - 1, helper);
												parent.ProcessBundleChunks(dbObject, parent.bundles.Count - 1, helper);
											}
										}
									}
									if (num3 > 0)
									{
										nativeReader2.Position = num3;
										int num15 = nativeReader2.ReadInt();
										list = new List<int>();
										for (int k = 0; k < num15; k++)
										{
											list.Add(nativeReader2.ReadInt());
										}
										for (int l = 0; l < num15; l++)
										{
											int num16 = nativeReader2.ReadInt();
											long position2 = nativeReader2.Position;
											nativeReader2.Position = num16 - 12;
											Guid guid = nativeReader2.ReadGuid();
											int index = nativeReader2.ReadInt();
											int num17 = nativeReader2.ReadInt();
											int num18 = nativeReader2.ReadInt();
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
											nativeReader2.Position = position2;
										}
									}
								}
							}
						}
						num++;
					}
				}
			}
		}

		internal class LegacyAssetLoader : IAssetLoader
		{
			public void Load(AssetManager parent, BinarySbDataHelper helper)
			{
				foreach (string superBundle in parent.fs.SuperBundles)
				{
					parent.superBundles.Add(new SuperBundleEntry
					{
						Name = superBundle
					});
					bool flag = false;
					DbObject dbObject = parent.ProcessTocChunks($"native_data/{superBundle}.toc", helper, isBase: true);
					if (dbObject != null)
					{
						bool flag2 = dbObject.GetValue("alwaysEmitSuperBundle", defaultValue: false);
						if (ProfilesLibrary.DataVersion == 20140225 || ProfilesLibrary.DataVersion == 20150223)
						{
							flag2 = true;
						}
						parent.WriteToLog("Loading data ({0})", superBundle);
						DbObject dbObject2 = parent.ProcessTocChunks($"native_patch/{superBundle}.toc", helper);
						DbObject value = dbObject.GetValue<DbObject>("bundles");
						DbObject dbObject3 = value;
						if (value.Count != 0)
						{
							Dictionary<string, BaseBundleInfo> dictionary = new Dictionary<string, BaseBundleInfo>();
							if (flag2)
							{
								foreach (DbObject item in value)
								{
									BaseBundleInfo baseBundleInfo = new BaseBundleInfo();
									baseBundleInfo.Name = item.GetValue<string>("id");
									baseBundleInfo.Offset = item.GetValue("offset", 0L);
									baseBundleInfo.Size = item.GetValue("size", 0L);
									dictionary.Add(baseBundleInfo.Name.ToLower(), baseBundleInfo);
								}
							}
							if (dbObject2 != null)
							{
								dbObject3 = dbObject2.GetValue<DbObject>("bundles");
								flag = true;
							}
							NativeReader nativeReader = new NativeReader(new FileStream(parent.fs.ResolvePath($"native_data/{superBundle}.sb"), FileMode.Open, FileAccess.Read));
							NativeReader nativeReader2 = nativeReader;
							if (flag)
							{
								nativeReader2 = new NativeReader(new FileStream(parent.fs.ResolvePath($"native_patch/{superBundle}.sb"), FileMode.Open, FileAccess.Read));
							}
							foreach (DbObject item2 in dbObject3)
							{
								string text = item2.GetValue<string>("id").ToLower();
								long value2 = item2.GetValue("offset", 0L);
								long value3 = item2.GetValue("size", 0L);
								bool value4 = item2.GetValue("delta", defaultValue: false);
								bool value5 = item2.GetValue("base", defaultValue: false);
								parent.bundles.Add(new BundleEntry
								{
									Name = text,
									SuperBundleId = parent.superBundles.Count - 1
								});
								int bundleId = parent.bundles.Count - 1;
								Stream stream = value5 ? nativeReader.CreateViewStream(value2, value3) : nativeReader2.CreateViewStream(value2, value3);
								DbObject dbObject5 = null;
								if (flag2)
								{
									if (value4)
									{
										BaseBundleInfo baseBundleInfo2 = dictionary.ContainsKey(text) ? dictionary[text] : null;
										using (BinarySbReader binarySbReader = new BinarySbReader((baseBundleInfo2 != null) ? nativeReader.CreateViewStream(baseBundleInfo2.Offset, baseBundleInfo2.Size) : null, stream, parent.fs.CreateDeobfuscator()))
										{
											dbObject5 = binarySbReader.ReadDbObject();
										}
										DbObject baseList = null;
										if (baseBundleInfo2 != null)
										{
											using (BinarySbReader binarySbReader2 = new BinarySbReader(nativeReader.CreateViewStream(baseBundleInfo2.Offset, baseBundleInfo2.Size), baseBundleInfo2.Offset, parent.fs.CreateDeobfuscator()))
											{
												baseList = binarySbReader2.ReadDbObject();
											}
										}
										helper.FilterAndAddBundleData(baseList, dbObject5);
									}
									else
									{
										using (BinarySbReader binarySbReader3 = new BinarySbReader(stream, value2, parent.fs.CreateDeobfuscator()))
										{
											dbObject5 = binarySbReader3.ReadDbObject();
										}
									}
								}
								else
								{
									using (DbReader dbReader = new DbReader(stream, parent.fs.CreateDeobfuscator()))
									{
										dbObject5 = dbReader.ReadDbObject();
									}
								}
								parent.ProcessBundleEbx(dbObject5, bundleId, helper);
								parent.ProcessBundleRes(dbObject5, bundleId, helper);
								parent.ProcessBundleChunks(dbObject5, bundleId, helper);
							}
							nativeReader.Dispose();
							nativeReader2.Dispose();
							if (flag2)
							{
								GC.Collect();
							}
						}
					}
				}
			}
		}

		internal class ManifestAssetLoader : IAssetLoader
		{
			public void Load(AssetManager parent, BinarySbDataHelper helper)
			{
				parent.WriteToLog("Loading data from manifest");
				parent.superBundles.Add(new SuperBundleEntry
				{
					Name = "<none>"
				});
				foreach (DbObject item2 in parent.fs.EnumerateBundles())
				{
					BundleEntry item = new BundleEntry
					{
						Name = item2.GetValue<string>("name"),
						SuperBundleId = 0
					};
					parent.bundles.Add(item);
					if (item2 != null)
					{
						parent.ProcessBundleEbx(item2, parent.bundles.Count - 1, helper);
						parent.ProcessBundleRes(item2, parent.bundles.Count - 1, helper);
						parent.ProcessBundleChunks(item2, parent.bundles.Count - 1, helper);
					}
				}
				foreach (ChunkAssetEntry item3 in parent.fs.ProcessManifestChunks())
				{
					if (!parent.chunkList.ContainsKey(item3.Id))
					{
						parent.chunkList.Add(item3.Id, item3);
					}
				}
			}
		}

		internal class StandardAssetLoader : IAssetLoader
		{
			public void Load(AssetManager parent, BinarySbDataHelper helper)
			{
				foreach (string superBundle in parent.fs.SuperBundles)
				{
					DbObject dbObject = parent.ProcessTocChunks($"{superBundle}.toc", helper);
					if (dbObject != null)
					{
						parent.WriteToLog("Loading data ({0})", superBundle);
						parent.superBundles.Add(new SuperBundleEntry
						{
							Name = superBundle
						});
						using (NativeReader nativeReader = new NativeReader(new FileStream(parent.fs.ResolvePath($"{superBundle}.sb"), FileMode.Open, FileAccess.Read)))
						{
							foreach (DbObject item in dbObject.GetValue<DbObject>("bundles"))
							{
								string name = item.GetValue<string>("id").ToLower();
								long value = item.GetValue("offset", 0L);
								long value2 = item.GetValue("size", 0L);
								item.GetValue("delta", defaultValue: false);
								item.GetValue("base", defaultValue: false);
								parent.bundles.Add(new BundleEntry
								{
									Name = name,
									SuperBundleId = parent.superBundles.Count - 1
								});
								int bundleId = parent.bundles.Count - 1;
								DbObject sb = null;
								using (DbReader dbReader = new DbReader(nativeReader.CreateViewStream(value, value2), parent.fs.CreateDeobfuscator()))
								{
									sb = dbReader.ReadDbObject();
								}
								parent.ProcessBundleEbx(sb, bundleId, helper);
								parent.ProcessBundleRes(sb, bundleId, helper);
								parent.ProcessBundleChunks(sb, bundleId, helper);
							}
						}
					}
				}
			}
		}

		private const ulong CacheMagic = 144213406785688134uL;

		private const uint CacheVersion = 2u;

		private FileSystem fs;

		private ResourceManager rm;

		private ILogger logger;

		private List<SuperBundleEntry> superBundles = new List<SuperBundleEntry>();

		private List<BundleEntry> bundles = new List<BundleEntry>();

		private Dictionary<string, EbxAssetEntry> ebxList = new Dictionary<string, EbxAssetEntry>(StringComparer.OrdinalIgnoreCase);

		private Dictionary<string, ResAssetEntry> resList = new Dictionary<string, ResAssetEntry>();

		private Dictionary<Guid, ChunkAssetEntry> chunkList = new Dictionary<Guid, ChunkAssetEntry>();

		private Dictionary<Guid, EbxAssetEntry> ebxGuidList = new Dictionary<Guid, EbxAssetEntry>();

		private Dictionary<ulong, ResAssetEntry> resRidList = new Dictionary<ulong, ResAssetEntry>();

		private Dictionary<string, ICustomAssetManager> customAssetManagers = new Dictionary<string, ICustomAssetManager>();

		public AssetManager(FileSystem inFs, ResourceManager inRm)
		{
			fs = inFs;
			rm = inRm;

            Instance = this;
		}

		public void RegisterCustomAssetManager(string type, Type managerType)
		{
			customAssetManagers.Add(type, (ICustomAssetManager)Activator.CreateInstance(managerType));
		}

        public void RegisterLegacyAssetManager()
        {
            customAssetManagers.Add("legacy", new LegacyFileManager());
        }

        public void Initialize(bool additionalStartup = true, AssetManagerImportResult result = null)
		{
			DateTime now = DateTime.Now;
			List<EbxAssetEntry> prePatchCache = new List<EbxAssetEntry>();
			if (!ReadFromCache(out prePatchCache))
			{
				BinarySbDataHelper binarySbDataHelper = new BinarySbDataHelper(this);
				((IAssetLoader)Activator.CreateInstance(ProfilesLibrary.AssetLoader)).Load(this, binarySbDataHelper);
				binarySbDataHelper.WriteToCache(this);
				GC.Collect();
				WriteToCache();
			}
			TimeSpan timeSpan = DateTime.Now - now;
			WriteToLog("Loading complete", timeSpan.ToString());
			DoEbxIndexing();
			if (!additionalStartup)
			{
				return;
			}
			foreach (BundleEntry bundle in bundles)
			{
				bundle.Type = BundleType.SharedBundle;
				bundle.Blueprint = GetEbxEntry(bundle.Name.Remove(0, 6));
				if (bundle.Blueprint == null)
				{
					bundle.Blueprint = GetEbxEntry(bundle.Name);
				}
				if (bundle.Blueprint != null)
				{
					bundle.Type = BundleType.SubLevel;
					if (TypeLibrary.IsSubClassOf(bundle.Blueprint.Type, "BlueprintBundle"))
					{
						bundle.Type = BundleType.BlueprintBundle;
					}
				}
			}
			foreach (ICustomAssetManager value in customAssetManagers.Values)
			{
				value.Initialize(logger);
			}
			if (result != null && ProfilesLibrary.DataVersion != 20180914 && ProfilesLibrary.DataVersion != 20190729 && ProfilesLibrary.DataVersion != 20190911)
			{
				result.InvalidatedDueToPatch = (prePatchCache != null);
				if (prePatchCache != null)
				{
					WriteToLog("Processing patch results");
					List<Guid> list = new List<Guid>();
					List<EbxAssetEntry> list2 = new List<EbxAssetEntry>();
					List<EbxAssetEntry> list3 = new List<EbxAssetEntry>();
					List<EbxAssetEntry> list4 = new List<EbxAssetEntry>();
					foreach (EbxAssetEntry item in prePatchCache)
					{
						EbxAssetEntry ebxEntry = GetEbxEntry(item.Guid);
						if (ebxEntry != null)
						{
							list.Add(item.Guid);
							if (ebxEntry.Sha1 != item.Sha1)
							{
								list2.Add(ebxEntry);
							}
						}
						else
						{
							list4.Add(new EbxAssetEntry
							{
								Name = item.Name,
								Type = item.Type,
								Guid = item.Guid
							});
						}
					}
					foreach (EbxAssetEntry value2 in ebxList.Values)
					{
						if (!list.Contains(value2.Guid))
						{
							list3.Add(value2);
						}
					}
					result.AddedAssets = list3;
					result.ModifiedAssets = list2;
					result.RemovedAssets = list4;
				}
			}
			if (ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190729 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905)
			{
				WriteToLog("Loading type info");
				TypeLibrary.Reflection.LoadClassInfoAssets(this);
			}
		}

		public void SetLogger(ILogger inLogger)
		{
			logger = inLogger;
		}

		public void ClearLogger()
		{
			logger = null;
		}

		public void DoEbxIndexing()
		{
			if (ebxGuidList.Count <= 0)
			{
				List<EbxAssetEntry> list = new List<EbxAssetEntry>();
				int count = ebxList.Count;
				int num = 0;
				_ = DateTime.Now;
				foreach (EbxAssetEntry value in ebxList.Values)
				{
					bool inPatched = false;
					if (ProfilesLibrary.DataVersion == 20181207 && value.ExtraData.CasPath.StartsWith("native_patch"))
					{
						inPatched = true;
					}
					Stream ebxStream = GetEbxStream(value);
					int num2 = Fnv1.HashString(value.Name.ToLower());
					if (ebxStream != null)
					{
						using (EbxReader ebxReader = (ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905) ? new EbxReaderV2(ebxStream, fs, inPatched) : new EbxReader(ebxStream))
						{
							value.Type = ebxReader.RootType;
							value.Guid = ebxReader.FileGuid;
							ebxReader.Position = ebxReader.stringsOffset;
							string text = ebxReader.ReadNullTerminatedString();
							if (Fnv1.HashString(text.ToLower()) == num2)
							{
								value.Name = text;
							}
							foreach (EbxImportReference import in ebxReader.imports)
							{
								if (!value.ContainsDependency(import.FileGuid))
								{
									value.DependentAssets.Add(import.FileGuid);
								}
							}
							if (!ebxGuidList.ContainsKey(value.Guid))
							{
								ebxGuidList.Add(value.Guid, value);
								goto IL_01df;
							}
						}
						continue;
					}
					if (rm.IsEncrypted(value.Sha1))
					{
						value.Type = "EncryptedAsset";
					}
					else
					{
						list.Add(value);
					}
					goto IL_01df;
					IL_01df:
					if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
					{
						if (TypeLibrary.IsSubClassOf(value.Type, "BlueprintBundle") || TypeLibrary.IsSubClassOf(value.Type, "SubWorldData"))
						{
							BundleEntry bundleEntry = bundles[value.Bundles[0]];
							bundleEntry.Name = value.Name;
							if (!bundleEntry.Name.StartsWith("win32/"))
							{
								bundleEntry.Name = "win32/" + bundleEntry.Name;
							}
							bundleEntry.Blueprint = value;
						}
						else if ((ProfilesLibrary.DataVersion == 20180628 && TypeLibrary.IsSubClassOf(value.Type, "UIItemDescriptionAsset")) || (ProfilesLibrary.DataVersion == 20171117 && TypeLibrary.IsSubClassOf(value.Type, "UIMetaDataAsset")))
						{
							string text2 = "win32/" + value.Name.ToLower() + "_bundle";
							int h = Fnv1.HashString(text2);
							BundleEntry bundleEntry2 = bundles.Find((BundleEntry a) => a.Name.Equals(h.ToString("x8")));
							if (bundleEntry2 != null)
							{
								bundleEntry2.Name = text2;
							}
						}
					}
					num++;
					WriteToLog($"Initial load - Indexing data ({(int)((double)num / (double)count * 100.0)}%)");
					//WriteToLog("progress:{0}", (double)num / (double)count * 100.0);
				}
				foreach (EbxAssetEntry item in list)
				{
					ebxList.Remove(item.Name);
				}
				list.Clear();
				WriteToCache();
				WriteToLog("Initial load - Indexing complete");
			}
		}

		public uint GetModifiedCount()
		{
			uint num = (uint)ebxList.Values.Count((EbxAssetEntry entry) => entry.IsModified);
			uint num2 = (uint)resList.Values.Count((ResAssetEntry entry) => entry.IsModified);
			uint num3 = (uint)chunkList.Values.Count((ChunkAssetEntry entry) => entry.IsModified);
			uint num4 = 0u;
			foreach (ICustomAssetManager value in customAssetManagers.Values)
			{
				num4 = (uint)((int)num4 + value.EnumerateAssets(modifiedOnly: true).Count());
			}
			return num + num2 + num3 + num4;
		}

		public uint GetDirtyCount()
		{
			uint num = (uint)ebxList.Values.Count((EbxAssetEntry entry) => entry.IsDirty);
			uint num2 = (uint)resList.Values.Count((ResAssetEntry entry) => entry.IsDirty);
			uint num3 = (uint)chunkList.Values.Count((ChunkAssetEntry entry) => entry.IsDirty);
			uint num4 = 0u;
			foreach (ICustomAssetManager value in customAssetManagers.Values)
			{
				num4 = (uint)((int)num4 + value.EnumerateAssets(modifiedOnly: true).Count((AssetEntry a) => a.IsDirty));
			}
			return num + num2 + num3 + num4;
		}

		public uint GetEbxCount(string ebxType)
		{
			return (uint)ebxList.Values.Count((EbxAssetEntry entry) => entry.Type != null && entry.Type.Equals(ebxType));
		}

		public uint GetEbxCount()
		{
			return (uint)ebxList.Count;
		}

		public uint GetResCount(uint resType)
		{
			return (uint)resList.Values.Count((ResAssetEntry entry) => entry.ResType == resType);
		}

		public void Reset()
		{
			List<EbxAssetEntry> list = ebxList.Values.ToList();
			List<ResAssetEntry> list2 = resList.Values.ToList();
			List<ChunkAssetEntry> list3 = chunkList.Values.ToList();
			foreach (EbxAssetEntry item in list)
			{
				RevertAsset(item, dataOnly: false, suppressOnModify: false);
			}
			foreach (ResAssetEntry item2 in list2)
			{
				RevertAsset(item2, dataOnly: false, suppressOnModify: false);
			}
			foreach (ChunkAssetEntry item3 in list3)
			{
				RevertAsset(item3, dataOnly: false, suppressOnModify: false);
			}
			foreach (ICustomAssetManager value in customAssetManagers.Values)
			{
				foreach (AssetEntry item4 in value.EnumerateAssets(modifiedOnly: true))
				{
					RevertAsset(item4, dataOnly: false, suppressOnModify: false);
				}
			}
		}

		public void RevertAsset(AssetEntry entry, bool dataOnly = false, bool suppressOnModify = true)
		{
			if (!entry.IsModified)
			{
				return;
			}
			foreach (AssetEntry linkedAsset in entry.LinkedAssets)
			{
				RevertAsset(linkedAsset, dataOnly, suppressOnModify);
			}
			entry.ClearModifications();
			if (dataOnly)
			{
				return;
			}
			entry.LinkedAssets.Clear();
			entry.AddBundles.Clear();
			entry.RemBundles.Clear();
			if (entry.IsAdded)
			{
				if (entry is EbxAssetEntry)
				{
					EbxAssetEntry ebxAssetEntry = entry as EbxAssetEntry;
					ebxGuidList.Remove(ebxAssetEntry.Guid);
					ebxList.Remove(ebxAssetEntry.Name);
				}
				else if (entry is ResAssetEntry)
				{
					ResAssetEntry resAssetEntry = entry as ResAssetEntry;
					resRidList.Remove(resAssetEntry.ResRid);
					resList.Remove(resAssetEntry.Name);
				}
				else if (entry is ChunkAssetEntry)
				{
					ChunkAssetEntry chunkAssetEntry = entry as ChunkAssetEntry;
					chunkList.Remove(chunkAssetEntry.Id);
				}
			}
			entry.IsDirty = false;
			if (!entry.IsAdded && !suppressOnModify)
			{
				entry.OnModified();
			}
		}

		public void AddChunk(ChunkAssetEntry entry)
		{
			entry.IsAdded = true;
			chunkList.Add(entry.Id, entry);
		}

		public void AddRes(ResAssetEntry entry)
		{
			entry.IsAdded = true;
			resList.Add(entry.Name.ToLower(), entry);
			resRidList.Add(entry.ResRid, entry);
		}

		public void AddEbx(EbxAssetEntry entry)
		{
			entry.IsAdded = true;
			ebxList.Add(entry.Name.ToLower(), entry);
			ebxGuidList.Add(entry.Guid, entry);
		}

		public BundleEntry AddBundle(string name, BundleType type, int sbIndex)
		{
			int num = bundles.FindIndex((BundleEntry be) => be.Name == name);
			if (num != -1)
			{
				return bundles[num];
			}
			BundleEntry bundleEntry = new BundleEntry();
			bundleEntry.Name = name;
			bundleEntry.SuperBundleId = sbIndex;
			bundleEntry.Type = type;
			bundleEntry.Added = true;
			bundles.Add(bundleEntry);
			return bundleEntry;
		}

		public SuperBundleEntry AddSuperBundle(string name)
		{
			int num = superBundles.FindIndex((SuperBundleEntry sbe) => sbe.Name.Equals(name));
			if (num != -1)
			{
				return superBundles[num];
			}
			SuperBundleEntry superBundleEntry = new SuperBundleEntry();
			superBundleEntry.Name = name;
			superBundleEntry.Added = true;
			superBundles.Add(superBundleEntry);
			return superBundleEntry;
		}

		public EbxAssetEntry AddEbx(string name, EbxAsset asset, params int[] bundles)
		{
			string key = name.ToLower();
			if (ebxList.ContainsKey(key))
			{
				return ebxList[key];
			}
			EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
			ebxAssetEntry.Name = name;
			ebxAssetEntry.Guid = asset.FileGuid;
			ebxAssetEntry.Type = asset.RootObject.GetType().Name;
			ebxAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
			using (EbxWriter ebxWriter = new EbxWriter(new MemoryStream()))
			{
				ebxWriter.WriteAsset(asset);
				((MemoryStream)ebxWriter.BaseStream).ToArray();
			}
			ebxAssetEntry.ModifiedEntry.DataObject = asset;
			ebxAssetEntry.ModifiedEntry.OriginalSize = 0L;
			ebxAssetEntry.ModifiedEntry.Sha1 = Sha1.Zero;
			ebxAssetEntry.ModifiedEntry.IsInline = false;
			ebxAssetEntry.IsDirty = true;
			ebxAssetEntry.IsAdded = true;
			ebxList.Add(key, ebxAssetEntry);
			ebxGuidList.Add(ebxAssetEntry.Guid, ebxAssetEntry);
			return ebxAssetEntry;
		}

		public ResAssetEntry AddRes(string name, ResourceType resType, byte[] resMeta, byte[] buffer, params int[] bundles)
		{
			name = name.ToLower();
			if (resList.ContainsKey(name))
			{
				return resList[name];
			}
			ResAssetEntry resAssetEntry = new ResAssetEntry();
			resAssetEntry.Name = name;
			resAssetEntry.ResRid = Utils.GenerateResourceId();
			resAssetEntry.ResType = (uint)resType;
			resAssetEntry.ResMeta = resMeta;
			resAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
			resAssetEntry.ModifiedEntry.Data = Utils.CompressFile(buffer);
			resAssetEntry.ModifiedEntry.OriginalSize = buffer.Length;
			resAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(resAssetEntry.ModifiedEntry.Data);
			resAssetEntry.ModifiedEntry.IsInline = false;
			resAssetEntry.ModifiedEntry.ResMeta = resAssetEntry.ResMeta;
			resAssetEntry.IsAdded = true;
			resAssetEntry.IsDirty = true;
			resList.Add(resAssetEntry.Name, resAssetEntry);
			resRidList.Add(resAssetEntry.ResRid, resAssetEntry);
			return resAssetEntry;
		}

		public Guid AddChunk(byte[] buffer, Guid? overrideGuid = null, Texture texture = null, params int[] bundles)
		{
			ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
			CompressionType compressionOverride = (ProfilesLibrary.DataVersion == 20170929) ? CompressionType.Oodle : CompressionType.Default;
			chunkAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
			chunkAssetEntry.ModifiedEntry.Data = ((texture != null) ? Utils.CompressTexture(buffer, texture, compressionOverride) : Utils.CompressFile(buffer, null, ResourceType.Invalid, compressionOverride));
			chunkAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(chunkAssetEntry.ModifiedEntry.Data);
			chunkAssetEntry.ModifiedEntry.LogicalSize = (uint)buffer.Length;
			chunkAssetEntry.ModifiedEntry.FirstMip = -1;
			chunkAssetEntry.AddBundles.AddRange(bundles);
			if (texture != null)
			{
				chunkAssetEntry.ModifiedEntry.LogicalOffset = texture.LogicalOffset;
				chunkAssetEntry.ModifiedEntry.LogicalSize = texture.LogicalSize;
				chunkAssetEntry.ModifiedEntry.RangeStart = texture.RangeStart;
				chunkAssetEntry.ModifiedEntry.RangeEnd = texture.RangeEnd;
				chunkAssetEntry.ModifiedEntry.FirstMip = texture.FirstMip;
			}
			chunkAssetEntry.IsAdded = true;
			chunkAssetEntry.IsDirty = true;
			if (overrideGuid.HasValue)
			{
				chunkAssetEntry.Id = overrideGuid.Value;
			}
			else
			{
				byte[] array = Guid.NewGuid().ToByteArray();
				array[15] |= 1;
				chunkAssetEntry.Id = new Guid(array);
			}
			chunkList.Add(chunkAssetEntry.Id, chunkAssetEntry);
			return chunkAssetEntry.Id;
		}

		public bool ModifyChunk(Guid chunkId, byte[] buffer, Texture texture = null)
		{
			if (!chunkList.ContainsKey(chunkId))
			{
				return false;
			}
			ChunkAssetEntry chunkAssetEntry = chunkList[chunkId];
			CompressionType compressionOverride = (ProfilesLibrary.DataVersion == 20170929) ? CompressionType.Oodle : CompressionType.Default;
			if ((ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20190911) && texture != null)
			{
				compressionOverride = CompressionType.Oodle;
			}
			if (chunkAssetEntry.ModifiedEntry == null)
			{
				chunkAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
			}
			chunkAssetEntry.ModifiedEntry.Data = ((texture != null) ? Utils.CompressTexture(buffer, texture, compressionOverride) : Utils.CompressFile(buffer, null, ResourceType.Invalid, compressionOverride));
			chunkAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(chunkAssetEntry.ModifiedEntry.Data);
			chunkAssetEntry.ModifiedEntry.LogicalSize = (uint)buffer.Length;
			if (texture != null)
			{
				chunkAssetEntry.ModifiedEntry.LogicalOffset = texture.LogicalOffset;
				chunkAssetEntry.ModifiedEntry.LogicalSize = texture.LogicalSize;
				chunkAssetEntry.ModifiedEntry.RangeStart = texture.RangeStart;
				chunkAssetEntry.ModifiedEntry.RangeEnd = (uint)chunkAssetEntry.ModifiedEntry.Data.Length;
				chunkAssetEntry.ModifiedEntry.FirstMip = texture.FirstMip;
			}
			chunkAssetEntry.IsDirty = true;
			return true;
		}

		public void ModifyRes(ulong resRid, byte[] buffer, byte[] meta = null)
		{
			if (resRidList.ContainsKey(resRid))
			{
				ResAssetEntry resAssetEntry = resRidList[resRid];
				CompressionType compressionOverride = (ProfilesLibrary.DataVersion == 20170929) ? CompressionType.Oodle : CompressionType.Default;
				if (resAssetEntry.ModifiedEntry == null)
				{
					resAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
				}
				resAssetEntry.ModifiedEntry.Data = Utils.CompressFile(buffer, null, (ResourceType)resAssetEntry.ResType, compressionOverride);
				resAssetEntry.ModifiedEntry.OriginalSize = buffer.Length;
				resAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(resAssetEntry.ModifiedEntry.Data);
				if (meta != null)
				{
					resAssetEntry.ModifiedEntry.ResMeta = meta;
				}
				resAssetEntry.IsDirty = true;
			}
		}

		public void ModifyRes(ulong resRid, Resource resource)
		{
			if (resRidList.ContainsKey(resRid))
			{
				ResAssetEntry resAssetEntry = resRidList[resRid];
				if (resAssetEntry.ModifiedEntry == null)
				{
					resAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
				}
				resAssetEntry.ModifiedEntry.DataObject = resource.Save();
				resAssetEntry.IsDirty = true;
			}
		}

		public void ModifyRes(string resName, byte[] buffer, byte[] meta = null)
		{
			if (resList.ContainsKey(resName))
			{
				ResAssetEntry resAssetEntry = resList[resName];
				CompressionType compressionOverride = (ProfilesLibrary.DataVersion == 20170929) ? CompressionType.Oodle : CompressionType.Default;
				if (resAssetEntry.ModifiedEntry == null)
				{
					resAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
				}
				resAssetEntry.ModifiedEntry.Data = Utils.CompressFile(buffer, null, (ResourceType)resAssetEntry.ResType, compressionOverride);
				resAssetEntry.ModifiedEntry.OriginalSize = buffer.Length;
				resAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(resAssetEntry.ModifiedEntry.Data);
				if (meta != null)
				{
					resAssetEntry.ModifiedEntry.ResMeta = meta;
				}
				resAssetEntry.IsDirty = true;
			}
		}

		public void ModifyRes(string resName, Resource resource, byte[] meta = null)
		{
			if (resList.ContainsKey(resName))
			{
				ResAssetEntry resAssetEntry = resList[resName];
				if (resAssetEntry.ModifiedEntry == null)
				{
					resAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
				}
				resAssetEntry.ModifiedEntry.DataObject = resource.Save();
				if (meta != null)
				{
					resAssetEntry.ModifiedEntry.ResMeta = meta;
				}
				resAssetEntry.IsDirty = true;
			}
		}

		public void ModifyEbx(string name, EbxAsset asset)
		{
			name = name.ToLower();
			if (ebxList.ContainsKey(name))
			{
				EbxAssetEntry ebxAssetEntry = ebxList[name];
				if (ebxAssetEntry.ModifiedEntry == null)
				{
					ebxAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
				}
				ebxAssetEntry.ModifiedEntry.DataObject = asset;
				ebxAssetEntry.ModifiedEntry.OriginalSize = 0L;
				ebxAssetEntry.ModifiedEntry.Sha1 = Sha1.Zero;
				ebxAssetEntry.ModifiedEntry.IsTransientModified = asset.TransientEdit;
				ebxAssetEntry.ModifiedEntry.DependentAssets.Clear();
				ebxAssetEntry.ModifiedEntry.DependentAssets.AddRange(asset.Dependencies);
				ebxAssetEntry.IsDirty = true;
			}
		}

		public void ModifyCustomAsset(string type, string name, byte[] data)
		{
			if (customAssetManagers.ContainsKey(type))
			{
				customAssetManagers[type].ModifyAsset(name, data);
			}
		}

		public IEnumerable<SuperBundleEntry> EnumerateSuperBundles(bool modifiedOnly = false)
		{
			foreach (SuperBundleEntry superBundle in superBundles)
			{
				if (!modifiedOnly || superBundle.Added)
				{
					yield return superBundle;
				}
			}
		}

		public IEnumerable<BundleEntry> EnumerateBundles(BundleType type = BundleType.None, bool modifiedOnly = false)
		{
			foreach (BundleEntry bundle in bundles)
			{
				if ((type == BundleType.None || bundle.Type == type) && (!modifiedOnly || bundle.Added))
				{
					yield return bundle;
				}
			}
		}

		public IEnumerable<EbxAssetEntry> EnumerateEbx(BundleEntry bentry)
		{
			int num = bundles.IndexOf(bentry);
			return EnumerateEbx("", false, false, true, num);
		}

		public IEnumerable<EbxAssetEntry> EnumerateEbx(string type = "", bool modifiedOnly = false, bool includeLinked = false, bool includeHidden = true, string bundleSubPath = "")
		{
			List<int> list = new List<int>();
			if (bundleSubPath != "")
			{
				bundleSubPath = bundleSubPath.ToLower();
				for (int i = 0; i < bundles.Count; i++)
				{
					if (bundles[i].Name.Equals(bundleSubPath) || bundles[i].Name.StartsWith(bundleSubPath + "/"))
					{
						list.Add(i);
					}
				}
			}
			return EnumerateEbx(type, modifiedOnly, includeLinked, includeHidden, list.ToArray());
		}

		protected IEnumerable<EbxAssetEntry> EnumerateEbx(string type, bool modifiedOnly, bool includeLinked, bool includeHidden, params int[] bundles)
		{
			foreach (EbxAssetEntry value in ebxList.Values)
			{
				if ((!modifiedOnly || (value.IsModified && (!value.IsIndirectlyModified || includeLinked || value.IsDirectlyModified))) && (!(type != "") || (value.Type != null && TypeLibrary.IsSubClassOf(value.Type, type))))
				{
					if (bundles.Length != 0)
					{
						bool flag = false;
						foreach (int item in bundles)
						{
							if (value.Bundles.Contains(item))
							{
								flag = true;
								break;
							}
							if (value.AddBundles.Contains(item))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}
					}
					yield return value;
				}
			}
		}

		public IEnumerable<ResAssetEntry> EnumerateRes(BundleEntry bentry)
		{
			int num = bundles.IndexOf(bentry);
			if (num != -1)
			{
				foreach (ResAssetEntry item in EnumerateRes(0u, false, num))
				{
					yield return item;
				}
			}
		}

		public IEnumerable<ResAssetEntry> EnumerateRes(uint resType = 0u, bool modifiedOnly = false, string bundleSubPath = "")
		{
			List<int> list = new List<int>();
			if (bundleSubPath != "")
			{
				bundleSubPath = bundleSubPath.ToLower();
				for (int i = 0; i < bundles.Count; i++)
				{
					if (bundles[i].Name.Equals(bundleSubPath) || bundles[i].Name.StartsWith(bundleSubPath + "/"))
					{
						list.Add(i);
					}
				}
				if (list.Count == 0)
				{
					yield break;
				}
			}
			foreach (ResAssetEntry item in EnumerateRes(resType, modifiedOnly, list.ToArray()))
			{
				yield return item;
			}
		}

		protected IEnumerable<ResAssetEntry> EnumerateRes(uint resType, bool modifiedOnly, params int[] bundles)
		{
			foreach (ResAssetEntry value in resList.Values)
			{
				if ((!modifiedOnly || value.IsDirectlyModified) && (resType == 0 || value.ResType == resType))
				{
					if (bundles.Length != 0)
					{
						bool flag = false;
						foreach (int item in bundles)
						{
							if (value.Bundles.Contains(item))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}
					}
					yield return value;
				}
			}
		}

		public IEnumerable<ChunkAssetEntry> EnumerateChunks(BundleEntry bentry)
		{
			int bindex = bundles.IndexOf(bentry);
			if (bindex != -1)
			{
				foreach (ChunkAssetEntry value in chunkList.Values)
				{
					if (value.Bundles.Contains(bindex))
					{
						yield return value;
					}
				}
			}
		}

		public IEnumerable<ChunkAssetEntry> EnumerateChunks(bool modifiedOnly = false)
		{
			foreach (ChunkAssetEntry value in chunkList.Values)
			{
				if (!modifiedOnly || value.IsDirectlyModified)
				{
					yield return value;
				}
			}
		}

		public IEnumerable<AssetEntry> EnumerateCustomAssets(string type, bool modifiedOnly = false)
		{
			if (customAssetManagers.ContainsKey(type))
			{
				foreach (AssetEntry item in customAssetManagers[type].EnumerateAssets(modifiedOnly))
				{
					yield return item;
				}
			}
		}

		public int GetSuperBundleId(SuperBundleEntry sbentry)
		{
			return superBundles.FindIndex((SuperBundleEntry sbe) => sbe.Name.Equals(sbentry.Name));
		}

		public int GetSuperBundleId(string sbname)
		{
			return superBundles.FindIndex((SuperBundleEntry sbe) => sbe.Name.Equals(sbname, StringComparison.OrdinalIgnoreCase));
		}

		public SuperBundleEntry GetSuperBundle(int id)
		{
			if (id >= superBundles.Count)
			{
				return null;
			}
			return superBundles[id];
		}

		public int GetBundleId(BundleEntry bentry)
		{
			return bundles.FindIndex((BundleEntry be) => be.Name.Equals(bentry.Name));
		}

		public int GetBundleId(string name)
		{
			return bundles.FindIndex((BundleEntry be) => be.Name.Equals(name));
		}

		public BundleEntry GetBundleEntry(int bundleId)
		{
			if (bundleId >= bundles.Count)
			{
				return null;
			}
			return bundles[bundleId];
		}

		public AssetEntry GetCustomAssetEntry(string type, string key)
		{
			if (!customAssetManagers.ContainsKey(type))
			{
				return null;
			}
			return customAssetManagers[type].GetAssetEntry(key);
		}

		public T GetCustomAssetEntry<T>(string type, string key) where T : AssetEntry
		{
			return (T)GetCustomAssetEntry(type, key);
		}

		public EbxAssetEntry GetEbxEntry(Guid ebxGuid)
		{
			if (!ebxGuidList.ContainsKey(ebxGuid))
			{
				return null;
			}
			return ebxGuidList[ebxGuid];
		}

		public EbxAssetEntry GetEbxEntry(string name)
		{
			name = name.ToLower();
			if (!ebxList.ContainsKey(name))
			{
				return null;
			}
			return ebxList[name];
		}

		public ResAssetEntry GetResEntry(ulong resRid)
		{
			if (!resRidList.ContainsKey(resRid))
			{
				return null;
			}
			return resRidList[resRid];
		}

		public ResAssetEntry GetResEntry(string name)
		{
			name = name.ToLower();
			if (!resList.ContainsKey(name))
			{
				return null;
			}
			return resList[name];
		}

		public ChunkAssetEntry GetChunkEntry(Guid id)
		{
			if (!chunkList.ContainsKey(id))
			{
				return null;
			}
			return chunkList[id];
		}

		public Stream GetCustomAsset(string type, AssetEntry entry)
		{
			if (!customAssetManagers.ContainsKey(type))
			{
				return null;
			}
			return customAssetManagers[type].GetAsset(entry);
		}

		public EbxAsset GetEbx(EbxAssetEntry entry)
		{
			if (entry.ModifiedEntry != null && entry.ModifiedEntry.DataObject != null)
			{
				return entry.ModifiedEntry.DataObject as EbxAsset;
			}
			if (GetAsset(entry) == null)
			{
				return null;
			}
			bool inPatched = false;
			if ((ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905) && entry.ExtraData.CasPath.StartsWith("native_patch"))
			{
				inPatched = true;
			}
			using (EbxReader ebxReader = (ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905) ? new EbxReaderV2(GetAsset(entry), fs, inPatched) : new EbxReader(GetAsset(entry)))
			{
				return ebxReader.ReadAsset();
			}
		}

		public Stream GetEbxStream(EbxAssetEntry entry)
		{
			if (entry.IsModified)
			{
				using (EbxWriter ebxWriter = new EbxWriter(new MemoryStream(), EbxWriteFlags.None, leaveOpen: true))
				{
					ebxWriter.WriteAsset(entry.ModifiedEntry.DataObject as EbxAsset);
					ebxWriter.BaseStream.Position = 0L;
					return ebxWriter.BaseStream;
				}
			}
			return GetAsset(entry);
		}

		public Stream GetRes(ResAssetEntry entry)
		{
			return GetAsset(entry);
		}

		public T GetResAs<T>(ResAssetEntry entry) where T : Resource, new()
		{
			using (NativeReader reader = new NativeReader(GetAsset(entry)))
			{
				ModifiedResource modifiedData = null;
				if (entry.ModifiedEntry != null && entry.ModifiedEntry.DataObject != null)
				{
					modifiedData = (entry.ModifiedEntry.DataObject as ModifiedResource);
				}
				T val = new T();
				val.Read(reader, this, entry, modifiedData);
				return val;
			}
		}

		public Stream GetChunk(ChunkAssetEntry entry)
		{
			return GetAsset(entry);
		}

		private Stream GetAsset(AssetEntry entry)
		{
			if (entry.ModifiedEntry != null && entry.ModifiedEntry.Data != null)
			{
				return rm.GetResourceData(entry.ModifiedEntry.Data);
			}
			switch (entry.Location)
			{
			case AssetDataLocation.Cas:
				if (entry.ExtraData == null)
				{
					return rm.GetResourceData(entry.Sha1);
				}
				return rm.GetResourceData(entry.ExtraData.BaseSha1, entry.ExtraData.DeltaSha1);
			case AssetDataLocation.SuperBundle:
				return rm.GetResourceData((entry.ExtraData.IsPatch ? "native_patch/" : "native_data/") + superBundles[entry.ExtraData.SuperBundleId].Name + ".sb", entry.ExtraData.DataOffset, entry.Size);
			case AssetDataLocation.Cache:
				return rm.GetResourceData(entry.ExtraData.DataOffset, entry.Size);
			case AssetDataLocation.CasNonIndexed:
				return rm.GetResourceData(entry.ExtraData.CasPath, entry.ExtraData.DataOffset, entry.Size);
			default:
				return null;
			}
		}

		private void ProcessBundleEbx(DbObject sb, int bundleId, BinarySbDataHelper helper)
		{
			if (sb.GetValue<DbObject>("ebx") != null)
			{
				foreach (DbObject item in sb.GetValue<DbObject>("ebx"))
				{
					EbxAssetEntry ebxAssetEntry = AddEbx(item);
					if (ebxAssetEntry.Sha1 != item.GetValue<Sha1>("sha1") && item.GetValue("casPatchType", 0) != 0)
					{
						ebxAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
						ebxAssetEntry.Size = item.GetValue("size", 0L);
						ebxAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
						ebxAssetEntry.IsInline = item.HasValue("idata");
					}
					if (item.GetValue("cache", defaultValue: false) && ebxAssetEntry.Location != AssetDataLocation.Cache)
					{
						helper.RemoveEbxData(ebxAssetEntry.Name);
					}
					ebxAssetEntry.Bundles.Add(bundleId);
				}
			}
		}

		private void ProcessBundleRes(DbObject sb, int bundleId, BinarySbDataHelper helper)
		{
			if (sb.GetValue<DbObject>("res") != null)
			{
				foreach (DbObject item in sb.GetValue<DbObject>("res"))
				{
					if (!ProfilesLibrary.IsResTypeIgnored((ResourceType)item.GetValue("resType", 0L)))
					{
						ResAssetEntry resAssetEntry = AddRes(item);
						if (resAssetEntry.Sha1 != item.GetValue<Sha1>("sha1") && item.GetValue("casPatchType", 0) != 0)
						{
							resRidList.Remove(resAssetEntry.ResRid);
							resAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
							resAssetEntry.Size = item.GetValue("size", 0L);
							resAssetEntry.ResRid = (ulong)item.GetValue("resRid", 0L);
							resAssetEntry.ResMeta = item.GetValue<byte[]>("resMeta");
							resAssetEntry.IsInline = item.HasValue("idata");
							resAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
							resRidList.Add(resAssetEntry.ResRid, resAssetEntry);
						}
						if (item.GetValue("cache", defaultValue: false) && resAssetEntry.Location != AssetDataLocation.Cache)
						{
							helper.RemoveResData(resAssetEntry.Name);
						}
						resAssetEntry.Bundles.Add(bundleId);
					}
				}
			}
		}

		private void ProcessBundleChunks(DbObject sb, int bundleId, BinarySbDataHelper helper)
		{
			if (sb.GetValue<DbObject>("chunks") != null)
			{
				foreach (DbObject item in sb.GetValue<DbObject>("chunks"))
				{
					ChunkAssetEntry chunkAssetEntry = AddChunk(item);
					if (item.GetValue("cache", defaultValue: false) && chunkAssetEntry.Location != AssetDataLocation.Cache)
					{
						helper.RemoveChunkData(chunkAssetEntry.Id.ToString());
					}
					if (chunkAssetEntry.Size == 0L)
					{
						chunkAssetEntry.Size = item.GetValue("size", 0L);
						chunkAssetEntry.LogicalOffset = item.GetValue("logicalOffset", 0u);
						chunkAssetEntry.LogicalSize = item.GetValue("logicalSize", 0u);
						chunkAssetEntry.RangeStart = item.GetValue("rangeStart", 0u);
						chunkAssetEntry.RangeEnd = item.GetValue("rangeEnd", 0u);
						chunkAssetEntry.BundledSize = item.GetValue("bundledSize", 0u);
						chunkAssetEntry.IsInline = item.HasValue("idata");
					}
					chunkAssetEntry.Bundles.Add(bundleId);
				}
			}
		}

		private DbObject ProcessTocChunks(string superBundleName, BinarySbDataHelper helper, bool isBase = false)
		{
			string text = fs.ResolvePath(superBundleName);
			if (text == "")
			{
				return null;
			}
			DbObject dbObject = null;
			using (DbReader dbReader = new DbReader(new FileStream(text, FileMode.Open, FileAccess.Read), fs.CreateDeobfuscator()))
			{
				dbObject = dbReader.ReadDbObject();
			}
			if (isBase && ProfilesLibrary.DataVersion != 20141118 && ProfilesLibrary.DataVersion != 20141117 && ProfilesLibrary.DataVersion != 20151103 && ProfilesLibrary.DataVersion != 20150223 && ProfilesLibrary.DataVersion != 20131115 && ProfilesLibrary.DataVersion != 20140225)
			{
				return dbObject;
			}
			if (dbObject.GetValue<DbObject>("chunks") != null)
			{
				foreach (DbObject item in dbObject.GetValue<DbObject>("chunks"))
				{
					Guid value = item.GetValue<Guid>("id");
					ChunkAssetEntry chunkAssetEntry = null;
					if (chunkList.ContainsKey(value))
					{
						chunkAssetEntry = chunkList[value];
						chunkList.Remove(value);
						helper.RemoveChunkData(chunkAssetEntry.Id.ToString());
					}
					else
					{
						chunkAssetEntry = new ChunkAssetEntry();
					}
					chunkAssetEntry.Id = item.GetValue<Guid>("id");
					chunkAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
					if (item.GetValue("size", 0L) != 0L)
					{
						chunkAssetEntry.Location = AssetDataLocation.SuperBundle;
						chunkAssetEntry.Size = item.GetValue("size", 0L);
						chunkAssetEntry.ExtraData = new AssetExtraData();
						chunkAssetEntry.ExtraData.DataOffset = item.GetValue("offset", 0L);
						chunkAssetEntry.ExtraData.SuperBundleId = superBundles.Count - 1;
						chunkAssetEntry.ExtraData.IsPatch = superBundleName.StartsWith("native_patch");
					}
					chunkList.Add(chunkAssetEntry.Id, chunkAssetEntry);
				}
				return dbObject;
			}
			return dbObject;
		}

		private EbxAssetEntry AddEbx(DbObject ebx)
		{
			string text = ebx.GetValue<string>("name").ToLower();
			if (ebxList.ContainsKey(text))
			{
				return ebxList[text];
			}
			EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
			ebxAssetEntry.Name = text;
			ebxAssetEntry.Sha1 = ebx.GetValue<Sha1>("sha1");
			ebxAssetEntry.BaseSha1 = rm.GetBaseSha1(ebxAssetEntry.Sha1);
			ebxAssetEntry.Size = ebx.GetValue("size", 0L);
			ebxAssetEntry.OriginalSize = ebx.GetValue("originalSize", 0L);
			ebxAssetEntry.IsInline = ebx.HasValue("idata");
			ebxAssetEntry.Location = AssetDataLocation.Cas;
			if (ebx.HasValue("cas"))
			{
				ebxAssetEntry.Location = AssetDataLocation.CasNonIndexed;
				ebxAssetEntry.ExtraData = new AssetExtraData();
				ebxAssetEntry.ExtraData.DataOffset = ebx.GetValue("offset", 0L);
				ebxAssetEntry.ExtraData.CasPath = (ebx.HasValue("catalog") ? fs.GetFilePath(ebx.GetValue("catalog", 0), ebx.GetValue("cas", 0), ebx.HasValue("patch")) : fs.GetFilePath(ebx.GetValue("cas", 0)));
			}
			else if (ebx.GetValue("sb", defaultValue: false))
			{
				ebxAssetEntry.Location = AssetDataLocation.SuperBundle;
				ebxAssetEntry.ExtraData = new AssetExtraData();
				ebxAssetEntry.ExtraData.DataOffset = ebx.GetValue("offset", 0L);
				ebxAssetEntry.ExtraData.SuperBundleId = superBundles.Count - 1;
			}
			else if (ebx.GetValue("cache", defaultValue: false))
			{
				ebxAssetEntry.Location = AssetDataLocation.Cache;
				ebxAssetEntry.ExtraData = new AssetExtraData();
				ebxAssetEntry.ExtraData.DataOffset = 3735928559L;
			}
			else if (ebx.GetValue("casPatchType", 0) == 2)
			{
				ebxAssetEntry.ExtraData = new AssetExtraData();
				ebxAssetEntry.ExtraData.BaseSha1 = ebx.GetValue<Sha1>("baseSha1");
				ebxAssetEntry.ExtraData.DeltaSha1 = ebx.GetValue<Sha1>("deltaSha1");
			}
			ebxList.Add(text, ebxAssetEntry);
			return ebxAssetEntry;
		}

		private ResAssetEntry AddRes(DbObject res)
		{
			string value = res.GetValue<string>("name");
			if (resList.ContainsKey(value))
			{
				return resList[value];
			}
			ResAssetEntry resAssetEntry = new ResAssetEntry();
			resAssetEntry.Name = value;
			resAssetEntry.Sha1 = res.GetValue<Sha1>("sha1");
			resAssetEntry.BaseSha1 = rm.GetBaseSha1(resAssetEntry.Sha1);
			resAssetEntry.Size = res.GetValue("size", 0L);
			resAssetEntry.OriginalSize = res.GetValue("originalSize", 0L);
			resAssetEntry.ResRid = (ulong)res.GetValue("resRid", 0L);
			resAssetEntry.ResType = (uint)res.GetValue("resType", 0L);
			resAssetEntry.ResMeta = res.GetValue<byte[]>("resMeta");
			resAssetEntry.IsInline = res.HasValue("idata");
			resAssetEntry.Location = AssetDataLocation.Cas;
			if (res.HasValue("cas"))
			{
				resAssetEntry.Location = AssetDataLocation.CasNonIndexed;
				resAssetEntry.ExtraData = new AssetExtraData();
				resAssetEntry.ExtraData.DataOffset = res.GetValue("offset", 0L);
				resAssetEntry.ExtraData.CasPath = (res.HasValue("catalog") ? fs.GetFilePath(res.GetValue("catalog", 0), res.GetValue("cas", 0), res.HasValue("patch")) : fs.GetFilePath(res.GetValue("cas", 0)));
			}
			else if (res.GetValue("sb", defaultValue: false))
			{
				resAssetEntry.Location = AssetDataLocation.SuperBundle;
				resAssetEntry.ExtraData = new AssetExtraData();
				resAssetEntry.ExtraData.DataOffset = res.GetValue("offset", 0L);
				resAssetEntry.ExtraData.SuperBundleId = superBundles.Count - 1;
			}
			else if (res.GetValue("cache", defaultValue: false))
			{
				resAssetEntry.Location = AssetDataLocation.Cache;
				resAssetEntry.ExtraData = new AssetExtraData();
				resAssetEntry.ExtraData.DataOffset = 3735928559L;
			}
			else if (res.GetValue("casPatchType", 0) == 2)
			{
				resAssetEntry.ExtraData = new AssetExtraData();
				resAssetEntry.ExtraData.BaseSha1 = res.GetValue<Sha1>("baseSha1");
				resAssetEntry.ExtraData.DeltaSha1 = res.GetValue<Sha1>("deltaSha1");
			}
			resList.Add(value, resAssetEntry);
			if (resAssetEntry.ResRid != 0L)
			{
				resRidList.Add(resAssetEntry.ResRid, resAssetEntry);
			}
			return resAssetEntry;
		}

		private ChunkAssetEntry AddChunk(DbObject chunk)
		{
			Guid value = chunk.GetValue<Guid>("id");
			if (chunkList.ContainsKey(value))
			{
				return chunkList[value];
			}
			ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
			chunkAssetEntry.Id = value;
			chunkAssetEntry.Sha1 = chunk.GetValue<Sha1>("sha1");
			chunkAssetEntry.Size = chunk.GetValue("size", 0L);
			chunkAssetEntry.LogicalOffset = chunk.GetValue("logicalOffset", 0u);
			chunkAssetEntry.LogicalSize = chunk.GetValue("logicalSize", 0u);
			chunkAssetEntry.RangeStart = chunk.GetValue("rangeStart", 0u);
			chunkAssetEntry.RangeEnd = chunk.GetValue("rangeEnd", 0u);
			chunkAssetEntry.BundledSize = chunk.GetValue("bundledSize", 0u);
			chunkAssetEntry.IsInline = chunk.HasValue("idata");
			chunkAssetEntry.Location = AssetDataLocation.Cas;
			if (chunk.HasValue("cas"))
			{
				chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
				chunkAssetEntry.ExtraData = new AssetExtraData();
				chunkAssetEntry.ExtraData.DataOffset = chunk.GetValue("offset", 0L);
				chunkAssetEntry.ExtraData.CasPath = (chunk.HasValue("catalog") ? fs.GetFilePath(chunk.GetValue("catalog", 0), chunk.GetValue("cas", 0), chunk.HasValue("patch")) : fs.GetFilePath(chunk.GetValue("cas", 0)));
			}
			else if (chunk.GetValue("sb", defaultValue: false))
			{
				chunkAssetEntry.Location = AssetDataLocation.SuperBundle;
				chunkAssetEntry.ExtraData = new AssetExtraData();
				chunkAssetEntry.ExtraData.DataOffset = chunk.GetValue("offset", 0L);
				chunkAssetEntry.ExtraData.SuperBundleId = superBundles.Count - 1;
			}
			else if (chunk.GetValue("cache", defaultValue: false))
			{
				chunkAssetEntry.Location = AssetDataLocation.Cache;
				chunkAssetEntry.ExtraData = new AssetExtraData();
				chunkAssetEntry.ExtraData.DataOffset = 3735928559L;
			}
			chunkList.Add(value, chunkAssetEntry);
			return chunkAssetEntry;
		}

		public void SendManagerCommand(string type, string command, params object[] value)
		{
			if (customAssetManagers.ContainsKey(type))
			{
				customAssetManagers[type].OnCommand(command, value);
			}
		}

		private bool ReadFromCache(out List<EbxAssetEntry> prePatchCache)
		{
			prePatchCache = null;
			if (!File.Exists(fs.CacheName + ".cache"))
			{
				return false;
			}
			WriteToLog("Loading data (" + fs.CacheName + ".cache)");
			bool flag = false;
			using (NativeReader nativeReader = new NativeReader(new FileStream(fs.CacheName + ".cache", FileMode.Open, FileAccess.Read)))
			{
				if (nativeReader.ReadULong() != 144213406785688134L)
				{
					return false;
				}
				if (nativeReader.ReadUInt() != 2)
				{
					return false;
				}
				if (nativeReader.ReadInt() != Fnv1.HashString(ProfilesLibrary.ProfileName))
				{
					return false;
				}
				if (nativeReader.ReadUInt() != fs.Head)
				{
					flag = true;
					prePatchCache = new List<EbxAssetEntry>();
				}
				int num = nativeReader.ReadInt();
				if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
				{
					superBundles.Add(new SuperBundleEntry
					{
						Name = "<none>"
					});
				}
				else
				{
					for (int i = 0; i < num; i++)
					{
						SuperBundleEntry superBundleEntry = new SuperBundleEntry();
						superBundleEntry.Name = nativeReader.ReadNullTerminatedString();
						superBundles.Add(superBundleEntry);
					}
				}
				num = nativeReader.ReadInt();
				if (num == 0)
				{
					return false;
				}
				for (int j = 0; j < num; j++)
				{
					BundleEntry bundleEntry = new BundleEntry();
					bundleEntry.Name = nativeReader.ReadNullTerminatedString();
					bundleEntry.SuperBundleId = nativeReader.ReadInt();
					if (!flag)
					{
						bundles.Add(bundleEntry);
					}
				}
				num = nativeReader.ReadInt();
				for (int k = 0; k < num; k++)
				{
					EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
					ebxAssetEntry.Name = nativeReader.ReadNullTerminatedString();
					ebxAssetEntry.Sha1 = nativeReader.ReadSha1();
					ebxAssetEntry.BaseSha1 = rm.GetBaseSha1(ebxAssetEntry.Sha1);
					ebxAssetEntry.Size = nativeReader.ReadLong();
					ebxAssetEntry.OriginalSize = nativeReader.ReadLong();
					ebxAssetEntry.Location = (AssetDataLocation)nativeReader.ReadInt();
					ebxAssetEntry.IsInline = nativeReader.ReadBoolean();
					ebxAssetEntry.Type = nativeReader.ReadNullTerminatedString();
					Guid guid = nativeReader.ReadGuid();
					if (nativeReader.ReadBoolean())
					{
						ebxAssetEntry.ExtraData = new AssetExtraData();
						ebxAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
						ebxAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
						ebxAssetEntry.ExtraData.DataOffset = nativeReader.ReadLong();
						ebxAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
						ebxAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
						ebxAssetEntry.ExtraData.CasPath = nativeReader.ReadNullTerminatedString();
					}
					int num2 = nativeReader.ReadInt();
					for (int l = 0; l < num2; l++)
					{
						ebxAssetEntry.Bundles.Add(nativeReader.ReadInt());
					}
					num2 = nativeReader.ReadInt();
					for (int m = 0; m < num2; m++)
					{
						ebxAssetEntry.DependentAssets.Add(nativeReader.ReadGuid());
					}
					if (flag)
					{
						ebxAssetEntry.Guid = guid;
						prePatchCache.Add(ebxAssetEntry);
					}
					else
					{
						if (guid != Guid.Empty)
						{
							ebxAssetEntry.Guid = guid;
							if (ebxGuidList.ContainsKey(ebxAssetEntry.Guid))
							{
								continue;
							}
							ebxGuidList.Add(guid, ebxAssetEntry);
						}
						ebxList.Add(ebxAssetEntry.Name, ebxAssetEntry);
					}
				}
				num = nativeReader.ReadInt();
				for (int n = 0; n < num; n++)
				{
					ResAssetEntry resAssetEntry = new ResAssetEntry();
					resAssetEntry.Name = nativeReader.ReadNullTerminatedString();
					resAssetEntry.Sha1 = nativeReader.ReadSha1();
					resAssetEntry.BaseSha1 = rm.GetBaseSha1(resAssetEntry.Sha1);
					resAssetEntry.Size = nativeReader.ReadLong();
					resAssetEntry.OriginalSize = nativeReader.ReadLong();
					resAssetEntry.Location = (AssetDataLocation)nativeReader.ReadInt();
					resAssetEntry.IsInline = nativeReader.ReadBoolean();
					resAssetEntry.ResRid = nativeReader.ReadULong();
					resAssetEntry.ResType = nativeReader.ReadUInt();
					resAssetEntry.ResMeta = nativeReader.ReadBytes(nativeReader.ReadInt());
					if (nativeReader.ReadBoolean())
					{
						resAssetEntry.ExtraData = new AssetExtraData();
						resAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
						resAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
						resAssetEntry.ExtraData.DataOffset = nativeReader.ReadLong();
						resAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
						resAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
						resAssetEntry.ExtraData.CasPath = nativeReader.ReadNullTerminatedString();
					}
					int num3 = nativeReader.ReadInt();
					for (int num4 = 0; num4 < num3; num4++)
					{
						resAssetEntry.Bundles.Add(nativeReader.ReadInt());
					}
					if (!flag)
					{
						resList.Add(resAssetEntry.Name, resAssetEntry);
						if (resAssetEntry.ResRid != 0L)
						{
							resRidList.Add(resAssetEntry.ResRid, resAssetEntry);
						}
					}
				}
				num = nativeReader.ReadInt();
				for (int num5 = 0; num5 < num; num5++)
				{
					ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
					chunkAssetEntry.Id = nativeReader.ReadGuid();
					chunkAssetEntry.Sha1 = nativeReader.ReadSha1();
					chunkAssetEntry.BaseSha1 = rm.GetBaseSha1(chunkAssetEntry.Sha1);
					chunkAssetEntry.Size = nativeReader.ReadLong();
					chunkAssetEntry.Location = (AssetDataLocation)nativeReader.ReadInt();
					chunkAssetEntry.IsInline = nativeReader.ReadBoolean();
					chunkAssetEntry.BundledSize = nativeReader.ReadUInt();
					chunkAssetEntry.RangeStart = nativeReader.ReadUInt();
					chunkAssetEntry.RangeEnd = nativeReader.ReadUInt();
					chunkAssetEntry.LogicalOffset = nativeReader.ReadUInt();
					chunkAssetEntry.LogicalSize = nativeReader.ReadUInt();
					chunkAssetEntry.H32 = nativeReader.ReadInt();
					chunkAssetEntry.FirstMip = nativeReader.ReadInt();
					if (nativeReader.ReadBoolean())
					{
						chunkAssetEntry.ExtraData = new AssetExtraData();
						chunkAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
						chunkAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
						chunkAssetEntry.ExtraData.DataOffset = nativeReader.ReadLong();
						chunkAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
						chunkAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
						chunkAssetEntry.ExtraData.CasPath = nativeReader.ReadNullTerminatedString();
					}
					int num6 = nativeReader.ReadInt();
					for (int num7 = 0; num7 < num6; num7++)
					{
						chunkAssetEntry.Bundles.Add(nativeReader.ReadInt());
					}
					if (!flag)
					{
						chunkList.Add(chunkAssetEntry.Id, chunkAssetEntry);
					}
				}
			}
			return !flag;
		}

		private void WriteToCache()
		{
			using (NativeWriter nativeWriter = new NativeWriter(new FileStream(fs.CacheName + ".cache", FileMode.Create)))
			{
				nativeWriter.Write(144213406785688134uL);
				nativeWriter.Write(2u);
				nativeWriter.Write(Fnv1.HashString(ProfilesLibrary.ProfileName));
				nativeWriter.Write(fs.Head);
				if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
				{
					nativeWriter.Write(0);
				}
				else
				{
					nativeWriter.Write(superBundles.Count);
					foreach (SuperBundleEntry superBundle in superBundles)
					{
						nativeWriter.WriteNullTerminatedString(superBundle.Name);
					}
				}
				nativeWriter.Write(bundles.Count);
				foreach (BundleEntry bundle in bundles)
				{
					nativeWriter.WriteNullTerminatedString(bundle.Name);
					nativeWriter.Write(bundle.SuperBundleId);
				}
				nativeWriter.Write(ebxList.Values.Count);
				foreach (EbxAssetEntry value in ebxList.Values)
				{
					nativeWriter.WriteNullTerminatedString(value.Name);
					nativeWriter.Write(value.Sha1);
					nativeWriter.Write(value.Size);
					nativeWriter.Write(value.OriginalSize);
					nativeWriter.Write((int)value.Location);
					nativeWriter.Write(value.IsInline);
					nativeWriter.WriteNullTerminatedString((value.Type != null) ? value.Type : "");
					nativeWriter.Write(value.Guid);
					nativeWriter.Write(value.ExtraData != null);
					if (value.ExtraData != null)
					{
						nativeWriter.Write(value.ExtraData.BaseSha1);
						nativeWriter.Write(value.ExtraData.DeltaSha1);
						nativeWriter.Write(value.ExtraData.DataOffset);
						nativeWriter.Write(value.ExtraData.SuperBundleId);
						nativeWriter.Write(value.ExtraData.IsPatch);
						nativeWriter.WriteNullTerminatedString(value.ExtraData.CasPath);
					}
					nativeWriter.Write(value.Bundles.Count);
					foreach (int bundle2 in value.Bundles)
					{
						nativeWriter.Write(bundle2);
					}
					nativeWriter.Write(value.DependentAssets.Count);
					foreach (Guid item in value.EnumerateDependencies())
					{
						nativeWriter.Write(item);
					}
				}
				nativeWriter.Write(resList.Values.Count);
				foreach (ResAssetEntry value2 in resList.Values)
				{
					nativeWriter.WriteNullTerminatedString(value2.Name);
					nativeWriter.Write(value2.Sha1);
					nativeWriter.Write(value2.Size);
					nativeWriter.Write(value2.OriginalSize);
					nativeWriter.Write((int)value2.Location);
					nativeWriter.Write(value2.IsInline);
					nativeWriter.Write(value2.ResRid);
					nativeWriter.Write(value2.ResType);
					nativeWriter.Write(value2.ResMeta.Length);
					nativeWriter.Write(value2.ResMeta);
					nativeWriter.Write(value2.ExtraData != null);
					if (value2.ExtraData != null)
					{
						nativeWriter.Write(value2.ExtraData.BaseSha1);
						nativeWriter.Write(value2.ExtraData.DeltaSha1);
						nativeWriter.Write(value2.ExtraData.DataOffset);
						nativeWriter.Write(value2.ExtraData.SuperBundleId);
						nativeWriter.Write(value2.ExtraData.IsPatch);
						nativeWriter.WriteNullTerminatedString(value2.ExtraData.CasPath);
					}
					nativeWriter.Write(value2.Bundles.Count);
					foreach (int bundle3 in value2.Bundles)
					{
						nativeWriter.Write(bundle3);
					}
				}
				nativeWriter.Write(chunkList.Count);
				foreach (ChunkAssetEntry value3 in chunkList.Values)
				{
					nativeWriter.Write(value3.Id);
					nativeWriter.Write(value3.Sha1);
					nativeWriter.Write(value3.Size);
					nativeWriter.Write((int)value3.Location);
					nativeWriter.Write(value3.IsInline);
					nativeWriter.Write(value3.BundledSize);
					nativeWriter.Write(value3.RangeStart);
					nativeWriter.Write(value3.RangeEnd);
					nativeWriter.Write(value3.LogicalOffset);
					nativeWriter.Write(value3.LogicalSize);
					nativeWriter.Write(value3.H32);
					nativeWriter.Write(value3.FirstMip);
					nativeWriter.Write(value3.ExtraData != null);
					if (value3.ExtraData != null)
					{
						nativeWriter.Write(value3.ExtraData.BaseSha1);
						nativeWriter.Write(value3.ExtraData.DeltaSha1);
						nativeWriter.Write(value3.ExtraData.DataOffset);
						nativeWriter.Write(value3.ExtraData.SuperBundleId);
						nativeWriter.Write(value3.ExtraData.IsPatch);
						nativeWriter.WriteNullTerminatedString(value3.ExtraData.CasPath);
					}
					nativeWriter.Write(value3.Bundles.Count);
					foreach (int bundle4 in value3.Bundles)
					{
						nativeWriter.Write(bundle4);
					}
				}
			}
		}

		private void WriteToLog(string text, params object[] vars)
		{
			if (logger != null)
			{
				logger.Log(text, vars);
			}
		}

		private Sha1 GenerateSha1(byte[] buffer)
		{
			using (SHA1Managed sHA1Managed = new SHA1Managed())
			{
				return new Sha1(sHA1Managed.ComputeHash(buffer));
			}
		}
	}
}
