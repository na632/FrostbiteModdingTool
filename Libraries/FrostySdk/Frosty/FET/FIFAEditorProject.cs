using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrostySdk;
using Modding;
using Modding.Categories;
using Frosty.Hash;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using FrostbiteSdk.Frostbite.FileManagers;

namespace FrostySdk.Frosty.FET
{


	public class FIFAEditorProject
	{
		public class HeaderData
		{
			public uint ProjectVersion { get; set; }

			public uint HeaderVersion { get; set; }

			public ModSettings ModSettings { get; } = new ModSettings();


			//public LocaleIniSettings LocaleIniSettings { get; } = new LocaleIniSettings();


			public DateTimeOffset CreationDate { get; set; }

			public DateTimeOffset ModifiedDate { get; set; }

			public string GameName { get; set; }

			public uint GameVersion { get; set; }
		}

		public const string FileExtension = "fifaproject";

		private readonly AssetManager assetManager;

		//private readonly ILegacyFileManager legacyFileManager;

		private readonly FileSystem fileSystem;

		private const uint FormatVersion = 19u;

		private const uint HeaderVersion = 1u;

		private const ulong Magic = 5498700893333637446uL;

		public string DisplayName
		{
			get
			{
				if (Filename == "")
				{
					return "New Project.fifaproject";
				}
				return Path.GetFileName(Filename);
			}
		}

		public string Filename { get; set; }

		//public bool IsDirty
		//{
		//	get
		//	{
		//		if (!assetManager.IsDirty())
		//		{
		//			return ModSettings.IsDirty;
		//		}
		//		return true;
		//	}
		//}

		public string GameName { get; }

		public HeaderData Header { get; } = new HeaderData();


		public ModSettings ModSettings => Header.ModSettings;

		//public LocaleIniSettings LocaleIniSettings => Header.LocaleIniSettings;

		public FIFAEditorProject(string gameName, AssetManager assetManager, FileSystem fileSystem)
		{
			GameName = gameName ?? throw new ArgumentNullException("gameName");
			this.assetManager = assetManager ?? throw new ArgumentNullException("assetManager");
			//this.legacyFileManager = legacyFileManager ?? throw new ArgumentNullException("legacyFileManager");
			this.fileSystem = fileSystem ?? throw new ArgumentNullException("fileSystem");
			Filename = "";
			Header.CreationDate = DateTimeOffset.Now;
			Header.ModifiedDate = DateTimeOffset.Now;
			Header.GameVersion = 0u;
		}

		private static void WriteHeader(NativeWriter writer, string gameName, HeaderData header, bool updateHeader)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (gameName == null)
			{
				throw new ArgumentNullException("gameName");
			}
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}
			WriteHeader(writer, gameName, header, header.ModSettings, updateHeader);
		}

		private static void WriteHeader(NativeWriter writer, string gameName, HeaderData header, ModSettings modSettings, bool updateHeader)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (gameName == null)
			{
				throw new ArgumentNullException("gameName");
			}
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}
			if (updateHeader)
			{
				header.ModifiedDate = DateTimeOffset.Now;
			}
			writer.WriteUInt64LittleEndian(5498700893333637446uL);
			writer.WriteUInt32LittleEndian(header.ProjectVersion);
			writer.WriteUInt32LittleEndian(header.HeaderVersion);
			writer.WriteLengthPrefixedString(gameName);
			writer.WriteInt64LittleEndian(header.CreationDate.Ticks);
			writer.WriteInt64LittleEndian(header.CreationDate.Offset.Ticks);
			writer.WriteInt64LittleEndian(header.ModifiedDate.Ticks);
			writer.WriteInt64LittleEndian(header.ModifiedDate.Offset.Ticks);
			writer.WriteUInt32LittleEndian(header.GameVersion);
			writer.WriteLengthPrefixedString(modSettings.Title);
			writer.WriteLengthPrefixedString(modSettings.Author);
			writer.Write((byte)7);
			writer.Write(Convert.ToByte(0));
			writer.WriteLengthPrefixedString(string.Empty);
			writer.WriteLengthPrefixedString(string.Empty);
			writer.WriteLengthPrefixedString(modSettings.Version);
			writer.WriteLengthPrefixedString(modSettings.Description);
			writer.WriteLengthPrefixedString(string.Empty);
			writer.WriteLengthPrefixedString(string.Empty);
			writer.WriteLengthPrefixedString(string.Empty);
			writer.WriteLengthPrefixedString(string.Empty);
			writer.WriteLengthPrefixedString(string.Empty);
			writer.WriteLengthPrefixedString(string.Empty);
			writer.WriteLengthPrefixedString(string.Empty);
			writer.WriteLengthPrefixedString(string.Empty);
			writer.WriteInt32LittleEndian(0);
			writer.WriteInt32LittleEndian(0);
			writer.Write7BitEncodedInt(0);
		}

		public static FIFAEditorProject ConvertFromFbProject(FrostbiteProject project, string newFilename)
        {
			FIFAEditorProject FETProject = new FIFAEditorProject("FIFA21", AssetManager.Instance, AssetManager.Instance.fs);
			FETProject.ModSettings.Author = project.ModSettings.Author;

			//StringBuilder sb = new StringBuilder(">> THIS MOD HAS BEEN CONVERTED FROM Frostbite Modding Tool AND MAY NOT WORK AS INTENDED! <<");
			//sb.AppendLine(project.ModSettings.Description);
			//FETProject.ModSettings.Description = Environment.NewLine + project.ModSettings.Description;
			FETProject.ModSettings.Title = project.ModSettings.Title;
			FETProject.ModSettings.Description = project.ModSettings.Description;
			FETProject.ModSettings.Version = project.ModSettings.Version;
			//FETProject.ModSettings.Category = project.ModSettings.Category;

			FETProject.Save(newFilename);
			return FETProject;
		}

		public void Save(string overrideFilename = "", bool updateDirtyState = true)
		{
			string fileName = ((!string.IsNullOrEmpty(overrideFilename)) ? overrideFilename : Filename);
			Header.GameVersion = fileSystem.Head;
			FileInfo fileInfo = new FileInfo(fileName);
			if (!fileInfo.Directory!.Exists)
			{
				Directory.CreateDirectory(fileInfo.DirectoryName);
			}
			string tempFilePath = fileInfo.FullName + ".tmp";
			using (NativeWriter writer = new NativeWriter(new FileStream(tempFilePath, FileMode.Create), wide: true))
			{
				Header.ProjectVersion = 19u;
				Header.HeaderVersion = 1u;
				WriteHeader(writer, GameName, Header, updateHeader: true);
				long position = writer.Position;
				writer.WriteUInt32LittleEndian(3735928559u);
				uint addedBundlesCount = 0u;
				foreach (BundleEntry item in assetManager.EnumerateBundles(BundleType.None, modifiedOnly: true))
				{
					if (item.Added)
					{
						writer.WriteLengthPrefixedString(item.Name);
						writer.WriteLengthPrefixedString(assetManager.GetSuperBundle(item.SuperBundleId).Name);
						writer.WriteInt32LittleEndian((int)item.Type);
						addedBundlesCount++;
					}
				}
				writer.Position = position;
				writer.WriteUInt32LittleEndian(addedBundlesCount);
				writer.Position = writer.Length;
				position = writer.Position;
				writer.WriteUInt32LittleEndian(3735928559u);
				uint addedEbxCount = 0u;
				foreach (EbxAssetEntry item2 in assetManager.EnumerateEbx("", modifiedOnly: true))
				{
					if (item2.IsAdded)
					{
						writer.WriteLengthPrefixedString(item2.Name);
						writer.WriteGuid(item2.Guid);
						addedEbxCount++;
					}
				}
				writer.Position = position;
				writer.WriteUInt32LittleEndian(addedEbxCount);
				writer.Position = writer.Length;
				position = writer.Position;
				writer.WriteUInt32LittleEndian(3735928559u);
				uint addedResCount = 0u;
				foreach (ResAssetEntry item3 in assetManager.EnumerateRes(0u, modifiedOnly: true))
				{
					if (item3.IsAdded)
					{
						writer.WriteLengthPrefixedString(item3.Name);
						writer.Write((ulong)item3.ResRid);
						writer.WriteUInt32LittleEndian(item3.ResType);
						writer.WriteBytes(item3.ResMeta);
						addedResCount++;
					}
				}
				writer.Position = position;
				writer.WriteUInt32LittleEndian(addedResCount);
				writer.Position = writer.Length;
				position = writer.Position;
				writer.WriteUInt32LittleEndian(3735928559u);
				uint addedChunkCount = 0u;
				foreach (ChunkAssetEntry item4 in assetManager.EnumerateChunks(modifiedOnly: true))
				{
					if (item4.IsAdded)
					{
						writer.WriteGuid(item4.Id);
						writer.WriteInt32LittleEndian(item4.H32);
						addedChunkCount++;
					}
				}
				writer.Position = position;
				writer.WriteUInt32LittleEndian(addedChunkCount);
				writer.Position = writer.Length;
				position = writer.Position;
				writer.WriteUInt32LittleEndian(3735928559u);
				uint num = 0u;
				foreach (EbxAssetEntry modifiedEbxAssetEntry in assetManager.EnumerateEbx("", modifiedOnly: true, includeLinked: true))
				{
					writer.WriteLengthPrefixedString(modifiedEbxAssetEntry.Name);
					SaveLinkedAssets(modifiedEbxAssetEntry, writer);
					writer.Write((bool)modifiedEbxAssetEntry.IsDirectlyModified);
					if (modifiedEbxAssetEntry.IsDirectlyModified)
					{
						writer.Write((bool)modifiedEbxAssetEntry.ModifiedEntry.IsTransientModified);
						writer.WriteLengthPrefixedString(modifiedEbxAssetEntry.ModifiedEntry.UserData);
						writer.WriteUInt32LittleEndian(AssetManager.Instance.fs.Head);
						writer.WriteInt64LittleEndian(DateTime.Now.Ticks);
						writer.Write(0ul);
						writer.Write(modifiedEbxAssetEntry.ModifiedEntry.Sha1);
						writer.WriteInt32LittleEndian(modifiedEbxAssetEntry.AddBundles.Count);
						foreach (int addBundle in modifiedEbxAssetEntry.AddBundles)
						{
							writer.WriteLengthPrefixedString(assetManager.GetBundleEntry(addBundle).Name);
						}
						EbxAsset asset = modifiedEbxAssetEntry.ModifiedEntry.DataObject as EbxAsset;
						using (EbxWriter ebxWriter = new EbxWriter(new MemoryStream(), EbxWriteFlags.IncludeTransient))
						{
							ebxWriter.WriteAsset(asset);
							writer.WriteInt32LittleEndian((int)ebxWriter.BaseStream.Length);
							ebxWriter.BaseStream.Position = 0L;
							ebxWriter.BaseStream.CopyTo(writer.BaseStream);
						}
						if (updateDirtyState)
						{
							modifiedEbxAssetEntry.IsDirty = false;
						}
					}
					num++;
				}
				writer.Position = position;
				writer.WriteUInt32LittleEndian(num);
				writer.Position = writer.Length;
				position = writer.Position;
				writer.WriteUInt32LittleEndian(3735928559u);
				num = 0u;
				foreach (ResAssetEntry modifiedResAssetEntry in assetManager.EnumerateRes(0u, modifiedOnly: true))
				{
					writer.WriteLengthPrefixedString(modifiedResAssetEntry.Name);
					SaveLinkedAssets(modifiedResAssetEntry, writer);
					writer.Write((bool)modifiedResAssetEntry.IsDirectlyModified);
					if (modifiedResAssetEntry.IsDirectlyModified)
					{
						writer.Write(modifiedResAssetEntry.ModifiedEntry.Sha1);
						writer.WriteInt64LittleEndian(modifiedResAssetEntry.ModifiedEntry.OriginalSize);
						if (modifiedResAssetEntry.ModifiedEntry.ResMeta != null)
						{
							writer.WriteInt32LittleEndian(modifiedResAssetEntry.ModifiedEntry.ResMeta.Length);
							writer.WriteBytes(modifiedResAssetEntry.ModifiedEntry.ResMeta);
						}
						else
						{
							writer.WriteInt32LittleEndian(0);
						}
						writer.WriteLengthPrefixedString(modifiedResAssetEntry.ModifiedEntry.UserData);
						writer.WriteUInt32LittleEndian(AssetManager.Instance.fs.Head);
						writer.WriteInt64LittleEndian(DateTime.Now.Ticks);
						writer.Write(0ul);
						writer.Write(modifiedResAssetEntry.ModifiedEntry.Sha1);
						writer.WriteInt32LittleEndian(modifiedResAssetEntry.AddBundles.Count);
						foreach (int addBundle2 in modifiedResAssetEntry.AddBundles)
						{
							writer.WriteLengthPrefixedString(assetManager.GetBundleEntry(addBundle2).Name);
						}
						byte[] array2 = modifiedResAssetEntry.ModifiedEntry.Data;
						if (modifiedResAssetEntry.ModifiedEntry.DataObject != null)
						{
							array2 = ((ModifiedResource)modifiedResAssetEntry.ModifiedEntry.DataObject).Save();
						}
						writer.WriteInt32LittleEndian(array2.Length);
						writer.WriteBytes(array2);
						if (updateDirtyState)
						{
							modifiedResAssetEntry.IsDirty = false;
						}
					}
					num++;
				}
				writer.Position = position;
				writer.WriteUInt32LittleEndian(num);
				writer.Position = writer.Length;
				position = writer.Position;
				writer.WriteUInt32LittleEndian(3735928559u);
				num = 0u;
				foreach (ChunkAssetEntry modifiedChunkAssetEntry in assetManager.EnumerateChunks(modifiedOnly: true))
				{
					writer.WriteGuid(modifiedChunkAssetEntry.Id);
					writer.Write(modifiedChunkAssetEntry.ModifiedEntry.Sha1);
					writer.WriteUInt32LittleEndian(modifiedChunkAssetEntry.ModifiedEntry.LogicalOffset);
					writer.WriteUInt32LittleEndian(modifiedChunkAssetEntry.ModifiedEntry.LogicalSize);
					writer.WriteUInt32LittleEndian(modifiedChunkAssetEntry.ModifiedEntry.RangeStart);
					writer.WriteUInt32LittleEndian(modifiedChunkAssetEntry.ModifiedEntry.RangeEnd);
					writer.WriteInt32LittleEndian(modifiedChunkAssetEntry.ModifiedEntry.FirstMip);
					writer.WriteInt32LittleEndian(modifiedChunkAssetEntry.ModifiedEntry.H32);
					writer.Write((bool)modifiedChunkAssetEntry.ModifiedEntry.AddToChunkBundle);
					writer.WriteLengthPrefixedString(modifiedChunkAssetEntry.ModifiedEntry.UserData);
					writer.WriteUInt32LittleEndian(AssetManager.Instance.fs.Head);
					writer.WriteInt64LittleEndian(DateTime.Now.Ticks);
					writer.Write(0ul);
					writer.Write(modifiedChunkAssetEntry.ModifiedEntry.Sha1);
					writer.WriteInt32LittleEndian(modifiedChunkAssetEntry.AddBundles.Count);
					foreach (int addBundle3 in modifiedChunkAssetEntry.AddBundles)
					{
						writer.WriteLengthPrefixedString(assetManager.GetBundleEntry(addBundle3).Name);
					}
					writer.WriteInt32LittleEndian(modifiedChunkAssetEntry.ModifiedEntry.Data.Length);
					writer.WriteBytes(modifiedChunkAssetEntry.ModifiedEntry.Data);
					if (updateDirtyState)
					{
						modifiedChunkAssetEntry.IsDirty = false;
					}
					num++;
				}
				writer.Position = position;
				writer.WriteUInt32LittleEndian(num);
				writer.Position = writer.Length;
				position = writer.BaseStream.Position;
				writer.WriteUInt32LittleEndian(3735928559u);
				num = 0u;
				//if (new Fifa_Tool.Project.LegacyCustomActionHandler().SaveToProject(legacyFileManager, writer))
				//{
				//	num++;
				//}
				writer.Position = position;
				writer.WriteUInt32LittleEndian(num);
				writer.Position = writer.Length;
				if (updateDirtyState)
				{
					ModSettings.ClearDirtyFlag();
				}
			}
			if (!File.Exists(tempFilePath))
			{
				return;
			}
			bool flag = false;
			using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
			{
				if (fileStream.Length > 0)
				{
					flag = true;
				}
			}
			string backup = fileInfo.FullName + ".bak";
			if (flag)
			{
				File.Delete(backup);
				if (File.Exists(fileInfo.FullName))
				{
					File.Replace(tempFilePath, fileInfo.FullName, backup, ignoreMetadataErrors: true);
				}
				else
				{
					File.Move(tempFilePath, fileInfo.FullName);
				}
			}
		}

		public void WriteToMod(string filename, ModSettings overrideSettings, bool generateChecksums, CancellationToken cancellationToken)
		{
			if (filename == null)
			{
				throw new ArgumentNullException("filename");
			}
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filename));
				using FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 65536);
				FIFAModWriter modWriter = new FIFAModWriter(GameName, assetManager, fileSystem, fileStream, overrideSettings);
				//modWriter.WriteProject(this);
			}
			catch
			{
				try
				{
					File.Delete(filename);
				}
				catch
				{
				}
				throw;
			}
		}

		public static void SaveLinkedAssets(AssetEntry entry, NativeWriter writer)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			writer.WriteInt32LittleEndian(entry.LinkedAssets.Count);
			foreach (AssetEntry linkedAsset in entry.LinkedAssets)
			{
				writer.WriteLengthPrefixedString(linkedAsset.AssetType);
				ChunkAssetEntry chunkAssetEntry = linkedAsset as ChunkAssetEntry;
				if (chunkAssetEntry != null)
				{
					writer.WriteGuid(chunkAssetEntry.Id);
				}
				else
				{
					writer.WriteLengthPrefixedString(linkedAsset.Name);
				}
			}
		}

		public static List<AssetEntry> LoadLinkedAssets(AssetManager assetManager, NativeReader reader, uint version)
		{
			int num = reader.ReadInt32LittleEndian();
			List<AssetEntry> list = new List<AssetEntry>();
			for (int i = 0; i < num; i++)
			{
				string text = ((version < 16) ? reader.ReadNullTerminatedString() : reader.ReadLengthPrefixedString());
				switch (text)
				{
					case "ebx":
						{
							string name2 = ((version < 16) ? reader.ReadNullTerminatedString() : reader.ReadLengthPrefixedString());
							EbxAssetEntry ebxEntry = assetManager.GetEbxEntry(name2);
							if (ebxEntry != null)
							{
								list.Add(ebxEntry);
							}
							continue;
						}
					case "res":
						{
							string name = ((version < 16) ? reader.ReadNullTerminatedString() : reader.ReadLengthPrefixedString());
							ResAssetEntry resEntry = assetManager.GetResEntry(name);
							if (resEntry != null)
							{
								list.Add(resEntry);
							}
							continue;
						}
					case "chunk":
						{
							Guid id = reader.ReadGuid();
							ChunkAssetEntry chunkEntry = assetManager.GetChunkEntry(id);
							if (chunkEntry != null)
							{
								list.Add(chunkEntry);
							}
							continue;
						}
				}
				string key = ((version < 16) ? reader.ReadNullTerminatedString() : reader.ReadLengthPrefixedString());
				if (text == "legacy")
				{
					
					LegacyFileEntry entry = LegacyFileManager.Instance.GetLFEntry(key);
					if (entry != null)
					{
						list.Add(entry);
					}
					continue;
				}
				throw new NotSupportedException("Asset type is not supported (" + text + ").");
			}
			return list;
		}

		public static void LoadLinkedAssets(AssetManager assetManager, DbObject asset, AssetEntry entry, uint version)
		{
			foreach (DbObject item in asset.GetValue<DbObject>("linkedAssets").List)
			{
				string value4 = item.GetValue<string>("type");
				switch (value4)
				{
					case "ebx":
						{
							string value8 = item.GetValue<string>("id");
							EbxAssetEntry ebxEntry = assetManager.GetEbxEntry(value8);
							if (ebxEntry != null)
							{
								entry.LinkAsset(ebxEntry);
							}
							continue;
						}
					case "res":
						{
							string value6 = item.GetValue<string>("id");
							ResAssetEntry resEntry = assetManager.GetResEntry(value6);
							if (resEntry != null)
							{
								entry.LinkAsset(resEntry);
							}
							continue;
						}
					case "chunk":
						{
							Guid value7 = item.GetValue<Guid>("id");
							ChunkAssetEntry chunkEntry = assetManager.GetChunkEntry(value7);
							if (chunkEntry != null)
							{
								entry.LinkAsset(chunkEntry);
							}
							continue;
						}
				}
				string value5 = item.GetValue<string>("id");
				if (value4 == "legacy")
				{
					LegacyFileEntry legacyEntry = (LegacyFileEntry)LegacyFileManager.Instance.GetAssetEntry(value5);
					if (legacyEntry != null)
					{
						entry.LinkAsset(legacyEntry);
					}
					continue;
				}
				throw new NotSupportedException("Asset type is not supported (" + value4 + ").");
			}
		}


		public static HeaderData LoadHeader(string filename, out NativeReader fileReader, bool keepReader = false, HeaderData header = null)
		{
			if (filename == null)
			{
				throw new ArgumentNullException("filename");
			}
			if (header == null)
			{
				header = new HeaderData();
			}
			FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
			NativeReader reader = new NativeReader(fileStream);
			try
			{
				if (reader.ReadULong() != 5498700893333637446L)
				{
					throw new InvalidDataException("Project file is in an unsupported format.");
				}
				uint projectVersion = (header.ProjectVersion = reader.ReadUInt32LittleEndian());
				switch (projectVersion)
				{
					case 0u:
					case 1u:
					case 2u:
					case 3u:
					case 4u:
					case 5u:
					case 6u:
					case 7u:
					case 8u:
						throw new InvalidDataException($"Project file is too old (version {projectVersion}) and no longer supported.");
					default:
						{
							uint headerVersion = (header.HeaderVersion = reader.ReadUInt32LittleEndian());
							break;
						}
					case 9u:
					case 10u:
					case 11u:
					case 12u:
					case 13u:
					case 14u:
					case 15u:
					case 16u:
					case 17u:
					case 18u:
						break;
				}
				if (projectVersion <= 15)
				{
					header.GameName = reader.ReadNullTerminatedString();
					header.CreationDate = new DateTimeOffset(reader.ReadInt64LittleEndian(), TimeSpan.FromTicks(reader.ReadInt64LittleEndian()));
					header.ModifiedDate = new DateTimeOffset(reader.ReadInt64LittleEndian(), TimeSpan.FromTicks(reader.ReadInt64LittleEndian()));
					header.GameVersion = reader.ReadUInt32LittleEndian();
				}
				else
				{
					reader = new NativeReader(fileStream, skipObfuscation: false, wide: true)
					{
						Position = fileStream.Position
					};
					header.GameName = reader.ReadLengthPrefixedString();
					var cdTicks1 = reader.ReadInt64LittleEndian();
					var cdTicks2 = reader.ReadInt64LittleEndian();
					var mdTicks1 = reader.ReadInt64LittleEndian();
					var mdTicks2 = reader.ReadInt64LittleEndian();
					header.CreationDate = new DateTimeOffset(cdTicks1, TimeSpan.FromTicks(cdTicks2));
					header.ModifiedDate = new DateTimeOffset(mdTicks1, TimeSpan.FromTicks(mdTicks2));
					header.GameVersion = reader.ReadUInt32LittleEndian();
				}
				if (projectVersion >= 17)
				{
				}
				fileReader = (keepReader ? reader : null);
				return header;
			}
			finally
			{
				if (!keepReader)
				{
					fileStream.Dispose();
					reader.Dispose();
				}
			}
		}


		public bool Load(string filename)
        {
			Filename = filename ?? throw new ArgumentNullException("filename");
			NativeReader reader;
			HeaderData header = LoadHeader(filename, out reader, keepReader: true, Header);
			try
			{
				header.ModSettings.ClearDirtyFlag();
				if (header.GameName != GameName)
				{
					return false;
				}
				if (header.ProjectVersion <= 15)
				{
					
				}
				else if (!InternalLoad(reader, header.ProjectVersion))
				{
					return false;
				}
				return true;
			}
			finally
			{
				reader?.Dispose();
			}
		}

		private bool InternalLoad(NativeReader reader, uint version)
		{
			Dictionary<int, AssetEntry> dictionary = new Dictionary<int, AssetEntry>();
			int addedBundleCount = reader.ReadInt32LittleEndian();
			for (int i = 0; i < addedBundleCount; i++)
			{
				string name = reader.ReadLengthPrefixedString();
				string sbname = reader.ReadLengthPrefixedString();
				BundleType type = (BundleType)reader.ReadInt32LittleEndian();
				assetManager.AddBundle(name, type, assetManager.GetSuperBundleId(sbname));
			}
			int addedEbxCount = reader.ReadInt32LittleEndian();
			for (int j = 0; j < addedEbxCount; j++)
			{
				EbxAssetEntry ebxAssetEntry = new EbxAssetEntry
				{
					Name = reader.ReadLengthPrefixedString(),
					Guid = reader.ReadGuid()
				};
				assetManager.AddEbx(ebxAssetEntry);
			}
			int addedResCount = reader.ReadInt32LittleEndian();
			for (int k = 0; k < addedResCount; k++)
			{
				ResAssetEntry resAssetEntry = new ResAssetEntry
				{
					Name = reader.ReadLengthPrefixedString(),
					ResRid = reader.ReadULong(),
					ResType = reader.ReadUInt32LittleEndian(),
					ResMeta = reader.ReadBytes(16)
				};
				assetManager.AddRes(resAssetEntry);
			}
			int addedChunkCount = reader.ReadInt32LittleEndian();
			for (int l = 0; l < addedChunkCount; l++)
			{
				ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry
				{
					Id = reader.ReadGuid(),
					H32 = reader.ReadInt32LittleEndian()
				};
				assetManager.AddChunk(chunkAssetEntry);
			}
			int modifiedEbxCount = reader.ReadInt32LittleEndian();
			for (int m = 0; m < modifiedEbxCount; m++)
			{
				string name2 = reader.ReadLengthPrefixedString();
				List<AssetEntry> collection = LoadLinkedAssets(assetManager, reader, version);
				bool flag = reader.ReadBoolean();
				bool isTransientModified = false;
				string userData = "";
				uint gamePatchVersionAtImport = 0u;
				DateTimeOffset modificationDateTime = DateTimeOffset.MinValue;
				Sha1 originalSha1AtImport = Sha1.Zero;
				List<int> list = new List<int>();
				byte[] buffer = null;
				if (flag)
				{
					isTransientModified = reader.ReadBoolean();
					userData = reader.ReadLengthPrefixedString();
					if (version >= 18)
					{
						gamePatchVersionAtImport = reader.ReadUInt32LittleEndian();
						long dateTimeTicks = reader.ReadInt64LittleEndian();
						long offsetTicks = reader.ReadInt64LittleEndian();
						modificationDateTime = new DateTimeOffset(dateTimeTicks, TimeSpan.FromTicks(offsetTicks));
						originalSha1AtImport = reader.ReadSha1();
					}
					int num15 = reader.ReadInt32LittleEndian();
					for (int num16 = 0; num16 < num15; num16++)
					{
						string name3 = reader.ReadLengthPrefixedString();
						int bundleId = assetManager.GetBundleId(name3);
						if (bundleId != -1)
						{
							list.Add(bundleId);
						}
					}
					buffer = reader.ReadBytes(reader.ReadInt32LittleEndian());
				}
				EbxAssetEntry ebxEntry = assetManager.GetEbxEntry(name2);
				if (ebxEntry == null)
				{
					continue;
				}
				if (flag)
				{
					using EbxReader ebxReader = new EbxReader(new MemoryStream(buffer));
					EbxAsset ebxAsset = ebxReader.ReadAsset();
					ebxEntry.ModifiedEntry = new ModifiedAssetEntry
					{
						IsTransientModified = isTransientModified,
						UserData = userData,
						DataObject = ebxAsset,
					};
					if (ebxEntry.IsAdded)
					{
						ebxEntry.Type = ebxAsset.RootObject.GetType().Name;
					}
				}
				int key = Fnv1.HashString(ebxEntry.Name);
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, ebxEntry);
				}
			}
			int modifiedResCount = reader.ReadInt32LittleEndian();
			for (int num17 = 0; num17 < modifiedResCount; num17++)
			{
				string text = reader.ReadLengthPrefixedString();
				List<AssetEntry> collection2 = LoadLinkedAssets(assetManager,  reader, version);
				bool flag2 = reader.ReadBoolean();
				Sha1 sha = Sha1.Zero;
				long originalSize = 0L;
				List<int> list2 = new List<int>();
				byte[] array = null;
				byte[] array2 = null;
				string userData2 = "";
				uint gamePatchVersionAtImport2 = 0u;
				DateTimeOffset modificationDateTime2 = DateTimeOffset.MinValue;
				Sha1 originalSha1AtImport2 = Sha1.Zero;
				if (flag2)
				{
					sha = reader.ReadSha1();
					originalSize = reader.ReadInt64LittleEndian();
					int num18 = reader.ReadInt32LittleEndian();
					if (num18 > 0)
					{
						array = reader.ReadBytes(num18);
					}
					userData2 = reader.ReadLengthPrefixedString();
					if (version >= 18)
					{
						gamePatchVersionAtImport2 = reader.ReadUInt32LittleEndian();
						long dateTimeTicks2 = reader.ReadInt64LittleEndian();
						long offsetTicks2 = reader.ReadInt64LittleEndian();
						modificationDateTime2 = new DateTimeOffset(dateTimeTicks2, TimeSpan.FromTicks(offsetTicks2));
						originalSha1AtImport2 = reader.ReadSha1();
					}
					num18 = reader.ReadInt32LittleEndian();
					for (int num19 = 0; num19 < num18; num19++)
					{
						string name4 = reader.ReadLengthPrefixedString();
						int bundleId2 = assetManager.GetBundleId(name4);
						if (bundleId2 != -1)
						{
							list2.Add(bundleId2);
						}
					}
					array2 = reader.ReadBytes(reader.ReadInt32LittleEndian());
				}
				ResAssetEntry resEntry = assetManager.GetResEntry(text);
				if (resEntry == null)
				{
					continue;
				}
				//resEntry.AddLinkedAssets(collection2);
				//resEntry.AddAddedBundles(list2);
				if (flag2)
				{
					resEntry.ModifiedEntry = new ModifiedAssetEntry
					{
						Sha1 = sha,
						OriginalSize = originalSize,
						ResMeta = array,
						UserData = userData2,
					};
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
			int modifiedChunkCount = reader.ReadInt32LittleEndian();
			for (int num11 = 0; num11 < modifiedChunkCount; num11++)
			{
				Guid id = reader.ReadGuid();
				Sha1 sha2 = reader.ReadSha1();
				uint logicalOffset = reader.ReadUInt32LittleEndian();
				uint logicalSize = reader.ReadUInt32LittleEndian();
				uint rangeStart = reader.ReadUInt32LittleEndian();
				uint rangeEnd = reader.ReadUInt32LittleEndian();
				int firstMip = reader.ReadInt32LittleEndian();
				int h = reader.ReadInt32LittleEndian();
				bool addToChunkBundle = reader.ReadBoolean();
				string userData3 = reader.ReadLengthPrefixedString();
				uint gamePatchVersionAtImport3 = 0u;
				DateTimeOffset modificationDateTime3 = DateTimeOffset.MinValue;
				Sha1 originalSha1AtImport3 = Sha1.Zero;
				if (version >= 18)
				{
					gamePatchVersionAtImport3 = reader.ReadUInt32LittleEndian();
					long dateTimeTicks3 = reader.ReadInt64LittleEndian();
					long offsetTicks3 = reader.ReadInt64LittleEndian();
					modificationDateTime3 = new DateTimeOffset(dateTimeTicks3, TimeSpan.FromTicks(offsetTicks3));
					originalSha1AtImport3 = reader.ReadSha1();
				}
				List<int> list3 = new List<int>();
				int num12 = reader.ReadInt32LittleEndian();
				for (int num13 = 0; num13 < num12; num13++)
				{
					string name5 = reader.ReadLengthPrefixedString();
					int bundleId3 = assetManager.GetBundleId(name5);
					if (bundleId3 != -1)
					{
						list3.Add(bundleId3);
					}
				}
				byte[] data = reader.ReadBytes(reader.ReadInt32LittleEndian());
				ChunkAssetEntry chunkAssetEntry2 = assetManager.GetChunkEntry(id);
				if (chunkAssetEntry2 == null)
				{
					ChunkAssetEntry chunkAssetEntry3 = new ChunkAssetEntry
					{
						Id = id,
						H32 = h
					};
					assetManager.AddChunk(chunkAssetEntry3);
					if (dictionary.ContainsKey(chunkAssetEntry3.H32))
					{
						foreach (int bundle in dictionary[chunkAssetEntry3.H32].Bundles)
						{
							chunkAssetEntry3.AddToBundle(bundle);
						}
					}
					chunkAssetEntry2 = chunkAssetEntry3;
				}
				chunkAssetEntry2.ModifiedEntry = new ModifiedAssetEntry
				{
					Sha1 = sha2,
					LogicalOffset = logicalOffset,
					LogicalSize = logicalSize,
					RangeStart = rangeStart,
					RangeEnd = rangeEnd,
					FirstMip = firstMip,
					H32 = h,
					AddToChunkBundle = addToChunkBundle,
					UserData = userData3,
					Data = data,
				};
			}
			int legacyFileCount = reader.ReadInt32LittleEndian();
			for (int num14 = 0; num14 < legacyFileCount; num14++)
			{
				string type2 = reader.ReadLengthPrefixedString();
				if (type2 == "legacy")
				{
					//new Fifa_Tool.Project.LegacyCustomActionHandler().LoadFromProject(assetManager, legacyFileManager, version, reader, type2);
					continue;
				}
				throw new NotSupportedException("Type of custom handler isn't recognised: " + type2);
			}
			return true;
		}


	}

}