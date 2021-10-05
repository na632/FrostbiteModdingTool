using Frostbite.FileManagers;
using FrostbiteSdk.FrostbiteSdk.Managers;
using Frosty.Hash;
using FrostySdk.Frosty.FET;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FrostySdk
{

    public class FrostbiteProject
	{
		private const uint FormatVersion = 12u;

		private const ulong Magic = 98218709832262uL;

		private string filename;

		private DateTime creationDate;

		private DateTime modifiedDate;

		private uint gameVersion;

		private ModSettings modSettings;

		public string DisplayName
		{
			get
			{
				if (filename == "")
				{
					return "New Project.fbproject";
				}
				return new FileInfo(filename).Name;
			}
		}

		public string Filename
		{
			get
			{
				return filename;
			}
			set
			{
				filename = value;
			}
		}

		public bool IsDirty
		{
			get
			{
				if (AssetManager.GetDirtyCount() == 0)
				{
					return modSettings.IsDirty;
				}
				return true;
			}
		}

		public ModSettings ModSettings => modSettings;

		public AssetManager AssetManager { get { return AssetManager.Instance; } }

		public FileSystem FileSystem { get { return AssetManager.Instance.fs; } }

		public FrostbiteProject()
        {
			filename = "";
			creationDate = DateTime.Now;
			modifiedDate = DateTime.Now;
			gameVersion = 0u;
			modSettings = new ModSettings();
			modSettings.Author = "";
			modSettings.ClearDirtyFlag();
		}

		public FrostbiteProject(AssetManager assetManager, FileSystem fileSystem)
		{
			filename = "";
			creationDate = DateTime.Now;
			modifiedDate = DateTime.Now;
			gameVersion = 0u;
			modSettings = new ModSettings();
			modSettings.Author = "";
			modSettings.ClearDirtyFlag();
		}

		public bool Load(string inFilename)
		{
			ModifiedAssetEntries = null;

			filename = inFilename;
			using (NativeReader nativeReader = new NativeReader(new FileStream(inFilename, FileMode.Open, FileAccess.Read)))
			{
				if (nativeReader.ReadULong() == 98218709832262L)
				{
					return InternalLoad(nativeReader);
				}
			}
			//return LegacyLoad(inFilename);

			return false;
		}

		public async Task<bool> LoadAsync(string inFilename)
        {
			return await new TaskFactory().StartNew(() =>
			{
				return Load(inFilename);
			}, TaskCreationOptions.LongRunning);
        }

		public static string LastFilePath;

		public IEnumerable<AssetEntry> ModifiedAssetEntries
		{
			get
			{
				List<AssetEntry> entries = new List<AssetEntry>();
				entries.AddRange(AssetManager.EnumerateEbx(modifiedOnly: true));
				entries.AddRange(AssetManager.EnumerateRes(modifiedOnly: true));
				entries.AddRange(AssetManager.EnumerateChunks(modifiedOnly: true));
				entries.AddRange(AssetManager.EnumerateCustomAssets("legacy", modifiedOnly: true));
				return entries;
			}
			set { }
		}

		public async Task<bool> SaveAsync(string overrideFilename = "", bool updateDirtyState = true)
		{
			return await Task.Run(() => { return Save(overrideFilename, updateDirtyState); });
		}

		public bool Save(string overrideFilename = "", bool updateDirtyState = true)
		{
			string fileName = filename;
			if (!string.IsNullOrEmpty(overrideFilename))
			{
				fileName = overrideFilename;
			}
			
			if(string.IsNullOrEmpty(fileName))
            {
				return false;
            }
			LastFilePath = fileName;

			modifiedDate = DateTime.Now;
			gameVersion = FileSystem.Head;
			FileInfo fileInfo = new FileInfo(fileName);
			if (!fileInfo.Directory.Exists)
			{
				Directory.CreateDirectory(fileInfo.DirectoryName);
			}
			string text = fileInfo.FullName + ".tmp";
			using (NativeWriter nativeWriter = new NativeWriter(new FileStream(text, FileMode.Create)))
			{
				nativeWriter.Write(98218709832262uL);
				nativeWriter.Write(12u);
				nativeWriter.WriteNullTerminatedString(ProfilesLibrary.ProfileName);
				nativeWriter.Write(creationDate.Ticks);
				nativeWriter.Write(modifiedDate.Ticks);
				nativeWriter.Write(gameVersion);
				nativeWriter.WriteNullTerminatedString(modSettings.Title);
				nativeWriter.WriteNullTerminatedString(modSettings.Author);
				nativeWriter.WriteNullTerminatedString(modSettings.Category);
				nativeWriter.WriteNullTerminatedString(modSettings.Version);
				nativeWriter.WriteNullTerminatedString(modSettings.Description);
				if (modSettings.Icon != null && modSettings.Icon.Length != 0)
				{
					nativeWriter.Write(modSettings.Icon.Length);
					nativeWriter.Write(modSettings.Icon);
				}
				else
				{
					nativeWriter.Write(0);
				}
				for (int i = 0; i < 4; i++)
				{
					byte[] screenshot = modSettings.GetScreenshot(i);
					if (screenshot != null && screenshot.Length != 0)
					{
						nativeWriter.Write(screenshot.Length);
						nativeWriter.Write(screenshot);
					}
					else
					{
						nativeWriter.Write(0);
					}
				}
				nativeWriter.Write(0);
				long position = nativeWriter.BaseStream.Position;
				nativeWriter.Write(3735928559u);
				int num = 0;
				foreach (BundleEntry item in AssetManager.EnumerateBundles(BundleType.None, modifiedOnly: true))
				{
					if (item.Added)
					{
						nativeWriter.WriteNullTerminatedString(item.Name);
						nativeWriter.WriteNullTerminatedString(AssetManager.GetSuperBundle(item.SuperBundleId).Name);
						nativeWriter.Write((int)item.Type);
						num++;
					}
				}
				nativeWriter.BaseStream.Position = position;
				nativeWriter.Write(num);
				nativeWriter.BaseStream.Position = nativeWriter.BaseStream.Length;
				position = nativeWriter.BaseStream.Position;
				nativeWriter.Write(3735928559u);
				num = 0;
				foreach (EbxAssetEntry item2 in AssetManager.EnumerateEbx("", modifiedOnly: true))
				{
					if (item2.IsAdded)
					{
						nativeWriter.WriteNullTerminatedString(item2.Name);
						nativeWriter.Write(item2.Guid);
						num++;
					}
				}
				nativeWriter.BaseStream.Position = position;
				nativeWriter.Write(num);
				nativeWriter.BaseStream.Position = nativeWriter.BaseStream.Length;
				position = nativeWriter.BaseStream.Position;
				nativeWriter.Write(3735928559u);
				num = 0;
				foreach (ResAssetEntry item3 in AssetManager.EnumerateRes(0u, modifiedOnly: true))
				{
					if (item3.IsAdded)
					{
						nativeWriter.WriteNullTerminatedString(item3.Name);
						nativeWriter.Write(item3.ResRid);
						nativeWriter.Write(item3.ResType);
						nativeWriter.Write(item3.ResMeta);
						num++;
					}
				}
				nativeWriter.BaseStream.Position = position;
				nativeWriter.Write(num);
				nativeWriter.BaseStream.Position = nativeWriter.BaseStream.Length;
				position = nativeWriter.BaseStream.Position;
				nativeWriter.Write(3735928559u);
				num = 0;
				foreach (ChunkAssetEntry item4 in AssetManager.EnumerateChunks(modifiedOnly: true))
				{
					if (item4.IsAdded)
					{
						nativeWriter.Write(item4.Id);
						nativeWriter.Write(item4.H32);
						num++;
					}
				}
				nativeWriter.BaseStream.Position = position;
				nativeWriter.Write(num);
				nativeWriter.BaseStream.Position = nativeWriter.BaseStream.Length;
				position = nativeWriter.BaseStream.Position;
				nativeWriter.Write(3735928559u);
				num = 0;
				foreach (EbxAssetEntry ebxItem in AssetManager.EnumerateEbx("", modifiedOnly: true, includeLinked: true))
				{
					nativeWriter.WriteNullTerminatedString(ebxItem.Name);
					SaveLinkedAssets(ebxItem, nativeWriter);
					nativeWriter.Write(ebxItem.IsDirectlyModified);
					if (ebxItem.IsDirectlyModified)
					{
						nativeWriter.Write(ebxItem.ModifiedEntry.IsTransientModified);
						nativeWriter.WriteNullTerminatedString(ebxItem.ModifiedEntry.UserData);
						nativeWriter.Write(ebxItem.AddBundles.Count);
						foreach (int addBundle in ebxItem.AddBundles)
						{
							nativeWriter.WriteNullTerminatedString(AssetManager.GetBundleEntry(addBundle).Name);
						}
						EbxAsset asset = ebxItem.ModifiedEntry.DataObject as EbxAsset;

						EbxBaseWriter ebxWriter = null;
						if(!string.IsNullOrEmpty(ProfilesLibrary.LoadedProfile.ProjectEbxWriter))
                        {
							ebxWriter = (EbxBaseWriter)AssetManager.Instance.LoadTypeByName(
								ProfilesLibrary.LoadedProfile.ProjectEbxWriter
								, new MemoryStream(), EbxWriteFlags.IncludeTransient, false);
						}

						if (ebxWriter == null)
							ebxWriter = new EbxWriter(new MemoryStream(), EbxWriteFlags.IncludeTransient, false);

                        //using (EbxWriterV2 ebxWriter = new EbxWriterV2(new MemoryStream(), EbxWriteFlags.IncludeTransient))
                        {
							asset.ParentEntry = ebxItem;
							ebxWriter.WriteAsset(asset);
							byte[] array = ((MemoryStream)ebxWriter.BaseStream).ToArray();
							nativeWriter.Write(array.Length);
							nativeWriter.Write(array);
						}
						if (updateDirtyState)
						{
							ebxItem.IsDirty = false;
						}
					}
					num++;
				}
				nativeWriter.BaseStream.Position = position;
				nativeWriter.Write(num);
				nativeWriter.BaseStream.Position = nativeWriter.BaseStream.Length;
				position = nativeWriter.BaseStream.Position;
				nativeWriter.Write(3735928559u);
				num = 0;
				foreach (ResAssetEntry item6 in AssetManager.EnumerateRes(0u, modifiedOnly: true))
				{
					nativeWriter.WriteNullTerminatedString(item6.Name);
					SaveLinkedAssets(item6, nativeWriter);
					nativeWriter.Write(item6.IsDirectlyModified);
					if (item6.IsDirectlyModified)
					{
						nativeWriter.Write(item6.ModifiedEntry.Sha1);
						nativeWriter.Write(item6.ModifiedEntry.OriginalSize);
						if (item6.ModifiedEntry.ResMeta != null)
						{
							nativeWriter.Write(item6.ModifiedEntry.ResMeta.Length);
							nativeWriter.Write(item6.ModifiedEntry.ResMeta);
						}
						else
						{
							nativeWriter.Write(0);
						}
						nativeWriter.WriteNullTerminatedString(item6.ModifiedEntry.UserData);
						nativeWriter.Write(item6.AddBundles.Count);
						foreach (int addBundle2 in item6.AddBundles)
						{
							nativeWriter.WriteNullTerminatedString(AssetManager.GetBundleEntry(addBundle2).Name);
						}
						byte[] array2 = item6.ModifiedEntry.Data;
						if (item6.ModifiedEntry.DataObject != null)
						{
							array2 = (item6.ModifiedEntry.DataObject as ModifiedResource).Save();
						}
						nativeWriter.Write(array2.Length);
						nativeWriter.Write(array2);
						if (updateDirtyState)
						{
							item6.IsDirty = false;
						}
					}
					num++;
				}
				nativeWriter.BaseStream.Position = position;
				nativeWriter.Write(num);
				nativeWriter.BaseStream.Position = nativeWriter.BaseStream.Length;
				position = nativeWriter.BaseStream.Position;
				nativeWriter.Write(3735928559u);
				num = 0;
				foreach (ChunkAssetEntry item7 in AssetManager.EnumerateChunks(modifiedOnly: true))
				{
					nativeWriter.Write(item7.Id);
					nativeWriter.Write(item7.ModifiedEntry.Sha1);
					nativeWriter.Write(item7.ModifiedEntry.LogicalOffset);
					nativeWriter.Write(item7.ModifiedEntry.LogicalSize);
					nativeWriter.Write(item7.ModifiedEntry.RangeStart);
					nativeWriter.Write(item7.ModifiedEntry.RangeEnd);
					nativeWriter.Write(item7.ModifiedEntry.FirstMip);
					nativeWriter.Write(item7.ModifiedEntry.H32);
					nativeWriter.Write(item7.ModifiedEntry.AddToChunkBundle);
					nativeWriter.WriteNullTerminatedString(item7.ModifiedEntry.UserData);
					nativeWriter.Write(item7.AddBundles.Count);
					foreach (int addBundle3 in item7.AddBundles)
					{
						nativeWriter.WriteNullTerminatedString(AssetManager.GetBundleEntry(addBundle3).Name);
					}
					nativeWriter.Write(item7.ModifiedEntry.Data.Length);
					nativeWriter.Write(item7.ModifiedEntry.Data);
					if (updateDirtyState)
					{
						item7.IsDirty = false;
					}
					num++;
				}
				nativeWriter.BaseStream.Position = position;
				nativeWriter.Write(num);
				nativeWriter.BaseStream.Position = nativeWriter.BaseStream.Length;
				position = nativeWriter.BaseStream.Position;

				// --------------------
				// Legacy Files
				nativeWriter.Write(AssetManager.EnumerateCustomAssets("legacy", modifiedOnly: true).Count());
				foreach (LegacyFileEntry lfe in AssetManager.EnumerateCustomAssets("legacy", modifiedOnly: true))
				{
					if (lfe.Name != null)
					{
						var serialisedLFE = JsonConvert.SerializeObject(lfe);
						nativeWriter.WriteLengthPrefixedString(serialisedLFE);
					}
				}

				// -----------------------
				// Added Legacy Files
				var hasAddedLegacyFiles = AssetManager.Instance.CustomAssetManagers["legacy"].AddedFileEntries.Count > 0;
				nativeWriter.Write(hasAddedLegacyFiles);
				if (hasAddedLegacyFiles)
				{
					nativeWriter.Write(AssetManager.Instance.CustomAssetManagers["legacy"].AddedFileEntries.Count);
					foreach (var lfe in AssetManager.Instance.CustomAssetManagers["legacy"].AddedFileEntries)
					{
						nativeWriter.WriteLengthPrefixedString(JsonConvert.SerializeObject(lfe));
					}
				}

				// -----------------------
				// Embedded files
				nativeWriter.Write(AssetManager.EmbeddedFileEntries.Count > 0);
				nativeWriter.Write(AssetManager.EmbeddedFileEntries.Count);
				foreach (EmbeddedFileEntry efe in AssetManager.EmbeddedFileEntries)
				{
					var serialisedEFE = JsonConvert.SerializeObject(efe);
					nativeWriter.WriteLengthPrefixedString(serialisedEFE);
				}

				if (updateDirtyState)
				{
					modSettings.ClearDirtyFlag();
				}
			}
			if (File.Exists(text))
			{
				bool flag = false;
				using (FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read))
				{
					if (fileStream.Length > 0)
					{
						flag = true;
					}
				}
				if (flag)
				{
					File.Delete(fileInfo.FullName);
					File.Move(text, fileInfo.FullName);
				}
			}

			return true;
		}

		public ModSettings GetModSettings()
		{
			return modSettings;
		}

		public void WriteToMod(string filename, ModSettings overrideSettings)
		{
			byte[] projectbytes;

			var memoryStream = new MemoryStream();
			FrostbiteModWriter frostyModWriter = new FrostbiteModWriter(memoryStream, overrideSettings);
			frostyModWriter.WriteProject(this);

			memoryStream.Position = 0;
			projectbytes = new NativeReader(memoryStream).ReadToEnd();

			MemoryStream zipStream = new MemoryStream(Utils.CompressFile(projectbytes, compressionOverride: CompressionType.ZStd));
            //ZipFile pZip = new ZipFile();
            //pZip.AddEntry("mE", projectbytes);
            //pZip.Save(zipStream);

			if (File.Exists(filename))
				File.Delete(filename);

			zipStream.Position = 0;
            NativeWriter nwFinal = new NativeWriter(new FileStream(filename, FileMode.CreateNew));
            nwFinal.Write((ushort)2);
            nwFinal.Write(new NativeReader(zipStream).ReadToEnd());
            nwFinal.Close();
            nwFinal.Dispose();
        }

		public void WriteToFIFAMod(string filename, ModSettings overrideSettings)
		{
			if (File.Exists(filename))
				File.Delete(filename);
			using (var fs = new FileStream(filename, FileMode.Create)) {
				FIFAModWriter frostyModWriter = new FIFAModWriter(string.Empty, AssetManager, FileSystem
					, fs
					, overrideSettings);
				frostyModWriter.WriteProject(this);
			}
		}

		public static void SaveLinkedAssets(AssetEntry entry, NativeWriter writer)
		{
			writer.Write(entry.LinkedAssets.Count);
			foreach (AssetEntry linkedAsset in entry.LinkedAssets)
			{
				writer.WriteNullTerminatedString(linkedAsset.AssetType);
				if (linkedAsset is ChunkAssetEntry)
				{
					writer.Write(((ChunkAssetEntry)linkedAsset).Id);
				}
				else
				{
					writer.WriteNullTerminatedString(linkedAsset.Name);
				}
			}
		}

		public List<AssetEntry> LoadLinkedAssets(NativeReader reader)
		{
			int num = reader.ReadInt();
			List<AssetEntry> list = new List<AssetEntry>();
			for (int i = 0; i < num; i++)
			{
				string text = reader.ReadNullTerminatedString();
				if (text == "ebx")
				{
					string name = reader.ReadNullTerminatedString();
					EbxAssetEntry ebxEntry = AssetManager.GetEbxEntry(name);
					if (ebxEntry != null)
					{
						list.Add(ebxEntry);
					}
				}
				else if (text == "res")
				{
					string name2 = reader.ReadNullTerminatedString();
					ResAssetEntry resEntry = AssetManager.GetResEntry(name2);
					if (resEntry != null)
					{
						list.Add(resEntry);
					}
				}
				else if (text == "chunk")
				{
					Guid id = reader.ReadGuid();
					ChunkAssetEntry chunkEntry = AssetManager.GetChunkEntry(id);
					if (chunkEntry != null)
					{
						list.Add(chunkEntry);
					}
				}
				else
				{
					string key = reader.ReadNullTerminatedString();
					AssetEntry customAssetEntry = AssetManager.GetCustomAssetEntry(text, key);
					if (customAssetEntry != null)
					{
						list.Add(customAssetEntry);
					}
				}
			}
			return list;
		}

		public void LoadLinkedAssets(DbObject asset, AssetEntry entry, uint version)
		{
			if (version == 2)
			{
				string value = asset.GetValue<string>("linkedAssetType");
				if (value == "res")
				{
					string value2 = asset.GetValue<string>("linkedAssetId");
					entry.LinkedAssets.Add(AssetManager.GetResEntry(value2));
				}
				else if (value == "chunk")
				{
					Guid value3 = asset.GetValue<Guid>("linkedAssetId");
					entry.LinkedAssets.Add(AssetManager.GetChunkEntry(value3));
				}
			}
			else
			{
				foreach (DbObject item in asset.GetValue<DbObject>("linkedAssets"))
				{
					string value4 = item.GetValue<string>("type");
					if (value4 == "ebx")
					{
						string value5 = item.GetValue<string>("id");
						EbxAssetEntry ebxEntry = AssetManager.GetEbxEntry(value5);
						if (ebxEntry != null)
						{
							entry.LinkedAssets.Add(ebxEntry);
						}
					}
					else if (value4 == "res")
					{
						string value6 = item.GetValue<string>("id");
						ResAssetEntry resEntry = AssetManager.GetResEntry(value6);
						if (resEntry != null)
						{
							entry.LinkedAssets.Add(resEntry);
						}
					}
					else if (value4 == "chunk")
					{
						Guid value7 = item.GetValue<Guid>("id");
						ChunkAssetEntry chunkEntry = AssetManager.GetChunkEntry(value7);
						if (chunkEntry != null)
						{
							entry.LinkedAssets.Add(chunkEntry);
						}
					}
					else
					{
						string value8 = item.GetValue<string>("id");
						AssetEntry customAssetEntry = AssetManager.GetCustomAssetEntry(value4, value8);
						if (customAssetEntry != null)
						{
							entry.LinkedAssets.Add(customAssetEntry);
						}
					}
				}
			}
		}

		public int EBXCount;
		public int RESCount;
		public int ChunkCount;
		public int LegacyCount;

		private bool InternalLoad(NativeReader reader)
		{
			uint num = reader.ReadUInt();
			switch (num)
			{
				default:
					return false;
				case 0u:
				case 1u:
				case 2u:
				case 3u:
				case 4u:
				case 5u:
				case 6u:
				case 7u:
				case 8u:
					return false;
				case 9u:
				case 10u:
				case 11u:
				case 12u:
					{
						if (reader.ReadNullTerminatedString() != ProfilesLibrary.ProfileName)
						{
							return false;
						}
						Dictionary<int, AssetEntry> dictionary = new Dictionary<int, AssetEntry>();
						creationDate = new DateTime(reader.ReadLong());
						modifiedDate = new DateTime(reader.ReadLong());
						gameVersion = reader.ReadUInt();
						modSettings.Title = reader.ReadNullTerminatedString();
						modSettings.Author = reader.ReadNullTerminatedString();
						modSettings.Category = reader.ReadNullTerminatedString();
						modSettings.Version = reader.ReadNullTerminatedString();
						modSettings.Description = reader.ReadNullTerminatedString();
						int num2 = reader.ReadInt();
						if (num2 > 0)
						{
							modSettings.Icon = reader.ReadBytes(num2);
						}
						for (int i = 0; i < 4; i++)
						{
							num2 = reader.ReadInt();
							if (num2 > 0)
							{
								modSettings.SetScreenshot(i, reader.ReadBytes(num2));
							}
						}
						modSettings.ClearDirtyFlag();
						int count = reader.ReadInt();
						count = reader.ReadInt();
						for (int j = 0; j < count; j++)
						{
							string name = reader.ReadNullTerminatedString();
							string sbname = reader.ReadNullTerminatedString();
							BundleType type = (BundleType)reader.ReadInt();
							AssetManager.AddBundle(name, type, AssetManager.GetSuperBundleId(sbname));
						}
						count = reader.ReadInt();
						for (int k = 0; k < count; k++)
						{
							EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
							ebxAssetEntry.Name = reader.ReadNullTerminatedString();
							ebxAssetEntry.Guid = reader.ReadGuid();
							AssetManager.AddEbx(ebxAssetEntry);
						}
						count = reader.ReadInt();
						for (int l = 0; l < count; l++)
						{
							ResAssetEntry resAssetEntry = new ResAssetEntry();
							resAssetEntry.Name = reader.ReadNullTerminatedString();
							resAssetEntry.ResRid = reader.ReadULong();
							resAssetEntry.ResType = reader.ReadUInt();
							resAssetEntry.ResMeta = reader.ReadBytes(16);
							AssetManager.AddRes(resAssetEntry);
						}
						count = reader.ReadInt();
						for (int m = 0; m < count; m++)
						{
							ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
							chunkAssetEntry.Id = reader.ReadGuid();
							chunkAssetEntry.H32 = reader.ReadInt();
							//AssetManager.ForceChunkRemoval = true;
							//AssetManager.AddChunk(chunkAssetEntry);
							//AssetManager.ForceChunkRemoval = false;
						}
						count = reader.ReadInt();
						EBXCount = count;
						for (int n = 0; n < count; n++)
						{
							string name2 = reader.ReadNullTerminatedString();
							
							List<AssetEntry> collection = LoadLinkedAssets(reader);
							bool flag = reader.ReadBoolean();
							bool isTransientModified = false;
							string userData = "";
							List<int> list = new List<int>();
							byte[] buffer = null;
							if (flag)
							{
								isTransientModified = reader.ReadBoolean();
								if (num >= 12)
								{
									userData = reader.ReadNullTerminatedString();
								}
								int num4 = reader.ReadInt();
								for (int num5 = 0; num5 < num4; num5++)
								{
									string name3 = reader.ReadNullTerminatedString();
									int bundleId = AssetManager.GetBundleId(name3);
									if (bundleId != -1)
									{
										list.Add(bundleId);
									}
								}
								buffer = reader.ReadBytes(reader.ReadInt());
							}
							EbxAssetEntry ebxEntry = AssetManager.GetEbxEntry(name2);
							if (ebxEntry != null)
							{
								ebxEntry.LinkedAssets.AddRange(collection);
								ebxEntry.AddBundles.AddRange(list);
								if (flag)
								{
									ebxEntry.ModifiedEntry = new ModifiedAssetEntry();
									ebxEntry.ModifiedEntry.IsTransientModified = isTransientModified;
									ebxEntry.ModifiedEntry.UserData = userData;
									EbxReader ebxReader = null;
									
									if (!string.IsNullOrEmpty(ProfilesLibrary.LoadedProfile.ProjectEbxWriter))
									{
										ebxReader = (EbxReader)AssetManager.Instance.LoadTypeByName(
											ProfilesLibrary.LoadedProfile.ProjectEbxReader,
											new MemoryStream(buffer), false);
									}

									if(ebxReader == null)
                                    {
                                        if (ProfilesLibrary.IsFIFA20DataVersion() && num == 9)
                                            ebxReader = new EbxReaderV2(new MemoryStream(buffer), inPatched: false);
                                        else
                                            ebxReader = new EbxReader(new MemoryStream(buffer));
                                    }


									using (
										ebxReader
												)
									{
										EbxAsset ebxAsset = ebxReader.ReadAsset();
										ebxEntry.ModifiedEntry.DataObject = ebxAsset;
										if (ebxEntry.IsAdded)
										{
											ebxEntry.Type = ebxAsset.RootObject.GetType().Name;
										}
										ebxEntry.ModifiedEntry.DependentAssets.AddRange(ebxAsset.Dependencies);
									}
								}
								int key = Fnv1.HashString(ebxEntry.Name);
								if (!dictionary.ContainsKey(key))
								{
									dictionary.Add(key, ebxEntry);
								}
							}
						}
						count = reader.ReadInt();
						RESCount = count;

						for (int num6 = 0; num6 < count; num6++)
						{
							string text = reader.ReadNullTerminatedString();
							List<AssetEntry> collection2 = LoadLinkedAssets(reader);
							bool flag2 = reader.ReadBoolean();
							Sha1 sha = Sha1.Zero;
							long originalSize = 0L;
							List<int> list2 = new List<int>();
							byte[] array = null;
							byte[] array2 = null;
							string userData2 = "";
							if (flag2)
							{
								sha = reader.ReadSha1();
								originalSize = reader.ReadLong();
								int num7 = reader.ReadInt();
								if (num7 > 0)
								{
									array = reader.ReadBytes(num7);
								}
								if (num >= 12)
								{
									userData2 = reader.ReadNullTerminatedString();
								}
								num7 = reader.ReadInt();
								for (int num8 = 0; num8 < num7; num8++)
								{
									string name4 = reader.ReadNullTerminatedString();
									int bundleId2 = AssetManager.GetBundleId(name4);
									if (bundleId2 != -1)
									{
										list2.Add(bundleId2);
									}
								}
								array2 = reader.ReadBytes(reader.ReadInt());
							}
							ResAssetEntry resEntry = AssetManager.GetResEntry(text);
							if (num < 11)
							{
								if (resEntry == null)
								{
									string name5 = text;
									int num9 = text.LastIndexOf("shaderblocks");
									if (num9 != -1)
									{
										name5 = text.Remove(num9);
										name5 += "shaderblocks_variation/blocks";
									}
									else
									{
										num9 = text.LastIndexOf("_mesh_");
										if (num9 != -1)
										{
											name5 = text.Remove(num9 + 5);
											name5 += "_mesh/blocks";
										}
									}
									bool flag3 = text.Contains("persistentblock");
									ResAssetEntry resEntry2 = AssetManager.GetResEntry(name5);
									//if (resEntry2 != null)
									//{
									//	ShaderBlockDepot resAs = AssetManager.GetResAs<ShaderBlockDepot>(resEntry2);
									//	Resources.Old.ShaderBlockResource shaderBlockResource = null;
									//	using (CasReader casReader = new CasReader(new MemoryStream(array2)))
									//	{
									//		using (NativeReader reader2 = new NativeReader(new MemoryStream(casReader.Read())))
									//		{
									//			shaderBlockResource = ((!flag3) ? ((Resources.Old.ShaderBlockResource)new Resources.Old.MeshParamDbBlock(reader2)) : ((Resources.Old.ShaderBlockResource)new Resources.Old.ShaderPersistentParamDbBlock(reader2)));
									//		}
									//	}
									//	Resources.ShaderBlockResource newResource = shaderBlockResource.Convert();
									//	if (!resAs.ReplaceResource(newResource))
									//	{
									//		Console.WriteLine(text);
									//	}
									//	AssetManager.ModifyRes(resEntry2.Name, resAs);
									//	resEntry2.IsDirty = false;
									//}
								}
								else if (resEntry != null && resEntry.ResType == 3639990959u)
								{
									ShaderBlockDepot shaderBlockDepot = new ShaderBlockDepot();
									using (CasReader casReader2 = new CasReader(new MemoryStream(array2)))
									{
										using (NativeReader reader3 = new NativeReader(new MemoryStream(casReader2.Read())))
										{
											shaderBlockDepot.Read(reader3, AssetManager, resEntry, null);
										}
									}
									for (int num10 = 0; num10 < shaderBlockDepot.ResourceCount; num10++)
									{
										Resources.ShaderBlockResource resource = shaderBlockDepot.GetResource(num10);
										if (resource is Resources.ShaderPersistentParamDbBlock || resource is Resources.MeshParamDbBlock)
										{
											resource.IsModified = true;
										}
									}
									AssetManager.ModifyRes(resEntry.Name, shaderBlockDepot, array);
									resEntry.IsDirty = false;
									flag2 = false;
								}
							}
							if (resEntry == null)
							{
								continue;
							}
							resEntry.LinkedAssets.AddRange(collection2);
							resEntry.AddBundles.AddRange(list2);
							if (flag2)
							{
								resEntry.ModifiedEntry = new ModifiedAssetEntry();
								resEntry.ModifiedEntry.Sha1 = sha;
								resEntry.ModifiedEntry.OriginalSize = originalSize;
								resEntry.ModifiedEntry.ResMeta = array;
								resEntry.ModifiedEntry.UserData = userData2;
								if (sha == Sha1.Zero)
								{
									resEntry.ModifiedEntry.DataObject = ModifiedResource.Read(array2);
								}
								else
								{
									resEntry.ModifiedEntry.Data = array2;
								}
							}
							int key2 = Fnv1.HashString(resEntry.Name);
							if (!dictionary.ContainsKey(key2))
							{
								dictionary.Add(key2, resEntry);
							}
						}
						count = reader.ReadInt();
						ChunkCount = count;

						for (int iModifiedChunk = 0; iModifiedChunk < count; iModifiedChunk++)
						{
							Guid id = reader.ReadGuid();
							Sha1 sha2 = reader.ReadSha1();
							uint logicalOffset = reader.ReadUInt();
							uint logicalSize = reader.ReadUInt();
							uint rangeStart = reader.ReadUInt();
							uint rangeEnd = reader.ReadUInt();
							int firstMip = reader.ReadInt();
							int h = reader.ReadInt();
							bool addToChunkBundle = reader.ReadBoolean();
							string userData3 = "";
							if (num >= 12)
							{
								userData3 = reader.ReadNullTerminatedString();
							}
							List<int> list3 = new List<int>();
							int num12 = reader.ReadInt();
							for (int num13 = 0; num13 < num12; num13++)
							{
								string name6 = reader.ReadNullTerminatedString();
								int bundleId3 = AssetManager.GetBundleId(name6);
								if (bundleId3 != -1)
								{
									list3.Add(bundleId3);
								}
							}
							byte[] data = reader.ReadBytes(reader.ReadInt());
							ChunkAssetEntry chunkAssetEntry2 = AssetManager.GetChunkEntry(id);
							if (chunkAssetEntry2 == null)
							{
								ChunkAssetEntry chunkAssetEntry3 = new ChunkAssetEntry();
								chunkAssetEntry3.Id = id;
								chunkAssetEntry3.H32 = h;
								AssetManager.AddChunk(chunkAssetEntry3);
								if (dictionary.ContainsKey(chunkAssetEntry3.H32))
								{
									foreach (int bundle in dictionary[chunkAssetEntry3.H32].Bundles)
									{
										chunkAssetEntry3.AddToBundle(bundle);
									}
								}
								chunkAssetEntry2 = chunkAssetEntry3;
							}
							chunkAssetEntry2.AddBundles.AddRange(list3);
							chunkAssetEntry2.ModifiedEntry = new ModifiedAssetEntry();
							chunkAssetEntry2.ModifiedEntry.Sha1 = sha2;
							chunkAssetEntry2.ModifiedEntry.LogicalOffset = logicalOffset;
							chunkAssetEntry2.ModifiedEntry.LogicalSize = logicalSize;
							chunkAssetEntry2.ModifiedEntry.RangeStart = rangeStart;
							chunkAssetEntry2.ModifiedEntry.RangeEnd = rangeEnd;
							chunkAssetEntry2.ModifiedEntry.FirstMip = firstMip;
							chunkAssetEntry2.ModifiedEntry.H32 = h;
							chunkAssetEntry2.ModifiedEntry.AddToChunkBundle = addToChunkBundle;
							chunkAssetEntry2.ModifiedEntry.UserData = userData3;
							chunkAssetEntry2.ModifiedEntry.Data = data;
						}

						// ----------------------------------------------------------------------------------------------------
						// LEGACY FILE HANDLING
						//

						// Count of Modified Legacy Files
						var legacyFileManager = AssetManager.Instance.CustomAssetManagers["legacy"] as LegacyFileManager_FMTV2;
						if (reader.Length > reader.Position)
						{
							count = reader.ReadInt();
							LegacyCount = count;

							if (legacyFileManager != null)
							{
								List<LegacyFileEntry> legacyFileEntries = new List<LegacyFileEntry>(count);
								for (int iItem = 0; iItem < count; iItem++)
								{
									var rawFile = reader.ReadLengthPrefixedString();
									if (legacyFileManager != null)
									{
										LegacyFileEntry lfe = JsonConvert.DeserializeObject<LegacyFileEntry>(rawFile);
										legacyFileEntries.Add(lfe);
									}
								}
								legacyFileManager.LoadEntriesModifiedFromProject(legacyFileEntries);
							}
						}

						// Count of Added Legacy Files
						if (reader.Length > reader.Position)
						{
							bool addedLegacyFiles = reader.ReadBoolean();
							if (addedLegacyFiles)
							{
								count = reader.ReadInt();
								LegacyCount += count;

								if (legacyFileManager != null)
								{
									List<LegacyFileEntry> legacyFileEntries = new List<LegacyFileEntry>(count);
									for (int iItem = 0; iItem < count; iItem++)
									{
										var rawFile = reader.ReadLengthPrefixedString();
										if (legacyFileManager != null)
										{
											LegacyFileEntry lfe = JsonConvert.DeserializeObject<LegacyFileEntry>(rawFile);
											legacyFileEntries.Add(lfe);
										}
									}
									legacyFileManager.LoadEntriesAddedFromProject(legacyFileEntries);
								}
							}
						}

						if (reader.Length > reader.Position)
						{
							bool hasEmbeddedFiles = reader.ReadBoolean();
							if (hasEmbeddedFiles)
							{
								AssetManager.Instance.EmbeddedFileEntries = new List<EmbeddedFileEntry>();
								int embeddedFileCount = reader.ReadInt();
								for (int iItem = 0; iItem < embeddedFileCount; iItem++)
								{
									var rawFile = reader.ReadLengthPrefixedString();
									EmbeddedFileEntry efe = JsonConvert.DeserializeObject<EmbeddedFileEntry>(rawFile);
									AssetManager.Instance.EmbeddedFileEntries.Add(efe);
								}
							}
						}

						//
						//
						// ----------------------------------------------------------------------------------------------------


						return true;
				}
			}
		}

	}

	public class ModSettings
	{
		private string title = "";

		private string author = "";

		private string category = "";

		private string version = "";

		private string description = "";

		private byte[] iconData;

		private byte[][] screenshotData;

		private bool isDirty;

		public bool IsDirty => isDirty;

		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				if (!title.Equals(value))
				{
					title = value;
					isDirty = true;
				}
			}
		}

		public string Author
		{
			get
			{
				return author;
			}
			set
			{
				if (!author.Equals(value))
				{
					author = value;
					isDirty = true;
				}
			}
		}

		public string Category
		{
			get
			{
				return category;
			}
			set
			{
				if (!category.Equals(value))
				{
					category = value;
					isDirty = true;
				}
			}
		}

		public string Version
		{
			get
			{
				return version;
			}
			set
			{
				if (!version.Equals(value))
				{
					version = value;
					isDirty = true;
				}
			}
		}

		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				if (!description.Equals(value))
				{
					description = value;
					isDirty = true;
				}
			}
		}

		public byte[] Icon
		{
			get
			{
				return iconData;
			}
			set
			{
				iconData = value;
				isDirty = true;
			}
		}

		public ModSettings()
		{
			screenshotData = new byte[4][];
		}

		public void SetScreenshot(int index, byte[] buffer)
		{
			screenshotData[index] = buffer;
			isDirty = true;
		}

		public byte[] GetScreenshot(int index)
		{
			return screenshotData[index];
		}

		public void ClearDirtyFlag()
		{
			isDirty = false;
		}
	}


}