using FrostbiteSdk;
using FrostbiteSdk.FrostbiteSdk.Managers;
using Frosty.Hash;
using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.FrostySdk.IO;
using FrostySdk.IO;
using FrostySdk.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FrostySdk
{
	public class FrostbiteModWriter : NativeWriter
	{
		public class Manifest
		{
			private Dictionary<string, int> nameEntries = new Dictionary<string, int>();

			private Dictionary<Sha1, int> sha1Entries = new Dictionary<Sha1, int>();

			private List<object> objects = new List<object>();

			public int Count => objects.Count;

			public int Add(byte[] data)
			{
				objects.Add(data);
				return objects.Count - 1;
			}

			public int Add(Sha1 sha1, byte[] data)
			{
				if (sha1Entries.ContainsKey(sha1))
				{
					return sha1Entries[sha1];
				}
				objects.Add(data);
				sha1Entries.Add(sha1, objects.Count - 1);
				return objects.Count - 1;
			}

			public int Add(string name, byte[] data)
			{
				if (nameEntries.ContainsKey(name))
				{
					return nameEntries[name];
				}
				objects.Add(data);
				nameEntries.Add(name, objects.Count - 1);
				return objects.Count - 1;
			}

			public void Write(NativeWriter writer)
			{
				long num = writer.BaseStream.Position + objects.Count * 16;
				long num2 = 0L;
				foreach (object @object in objects)
				{
					writer.Write(num2);
					long num3 = 0L;
					long position = writer.BaseStream.Position;
					writer.BaseStream.Position = num + num2;
					byte[] array = (byte[])@object;
					writer.Write(array);
					num3 = array.Length;
					writer.BaseStream.Position = position;
					writer.Write(num3);
					num2 += num3;
				}
			}
		}

		protected class EmbeddedResource : EditorModResource
		{
			public override ModResourceType Type => ModResourceType.Embedded;

			public EmbeddedResource(string inName, byte[] data, Manifest manifest)
			{
				name = inName;
				if (data != null)
				{
					resourceIndex = manifest.Add(data);
					size = data.Length;
				}
			}
		}

		protected class BundleResource : EditorModResource
		{
			private int superBundleName;

			public override ModResourceType Type => (ModResourceType)5;

			public BundleResource(BundleEntry entry, Manifest manifest)
			{
				name = entry.Name.ToLower();
				superBundleName = Fnv1a.HashString(AssetManager.Instance.GetSuperBundle(entry.SuperBundleId).Name.ToLower());
			}

			public override void Write(NativeWriter writer)
			{
				base.Write(writer);
				writer.WriteNullTerminatedString(name);
				writer.Write(superBundleName);
			}
		}

		public class EbxResource : EditorModResource
		{
			private static List<string> _listOfEBXRawFilesToUse = new List<string>();
			public static List<string> ListOfEBXRawFilesToUse
            {
				get
                {
                    if (File.Exists("EBXRawFilesToUse.dat") && _listOfEBXRawFilesToUse.Count == 0)
                    {
						_listOfEBXRawFilesToUse = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("EBXRawFilesToUse.dat"));
                    }

					return _listOfEBXRawFilesToUse;
                }
				set
                {
					_listOfEBXRawFilesToUse = value;

					if (File.Exists("EBXRawFilesToUse.dat"))
						File.Delete("EBXRawFilesToUse.dat");


					File.WriteAllText("EBXRawFilesToUse.dat", JsonConvert.SerializeObject(_listOfEBXRawFilesToUse));

				}
            }

			public override ModResourceType Type => ModResourceType.Ebx;

			public EbxResource(EbxAssetEntry entry, Manifest manifest)
			{
				CompressionType compressionOverride = ProfilesLibrary.GetCompressionType(ProfilesLibrary.CompTypeArea.EBX);

				name = entry.Name.ToLower();
				userData = entry.ModifiedEntry.UserData;

				EbxBaseWriter ebxBaseWriter = null;
				if (!string.IsNullOrEmpty(ProfilesLibrary.EBXWriter))
				{
					ebxBaseWriter = (EbxBaseWriter)AssetManager.Instance.LoadTypeByName(ProfilesLibrary.EBXWriter
						, new MemoryStream(), EbxWriteFlags.None, false);
				}
				else
				{
					throw new Exception("No EBX Writer provided for Game Profile");
					//ebxBaseWriter =
					//(
					//ProfilesLibrary.IsFIFA20DataVersion()
					//|| ProfilesLibrary.IsFIFA21DataVersion()
					//|| ProfilesLibrary.IsMadden21DataVersion()
					//)

					//? (EbxBaseWriter)new EbxWriterV2(new MemoryStream(), EbxWriteFlags.None, false)
					//: ((EbxBaseWriter)new EbxWriter(new MemoryStream(), EbxWriteFlags.None, false));
				}

                using (ebxBaseWriter)
				{

					var newAsset = entry.ModifiedEntry.DataObject as EbxAsset;
					newAsset.ParentEntry = entry;
					ebxBaseWriter.WriteAsset(newAsset);

					byte[] uncompArray = ((MemoryStream)ebxBaseWriter.BaseStream).ToArray();
					byte[] array = Utils.CompressFile(uncompArray, null, ResourceType.Invalid, compressionOverride);
					if(name.Contains("gp_") && (ebxBaseWriter is EbxWriterV2 || ebxBaseWriter is EbxWriter2021))
                    {
						File.WriteAllBytes($"ebx.{entry.Filename.Replace("\\", "_")}.write.dat", uncompArray); 
					}

					// this writes to Original Size not Size of the data ?!
					//size = array.Length;
					size = ebxBaseWriter.Length;
					resourceIndex = manifest.Add(array);
					sha1 = Utils.GenerateSha1(array);
				}
				foreach (int bundle in entry.Bundles)
				{
					BundleEntry bundleEntry = AssetManager.Instance.GetBundleEntry(bundle);
					AddBundle(bundleEntry.Name, modify: true);
				}
				foreach (int item in entry.EnumerateBundles(addedOnly: true))
				{
					BundleEntry bundleEntry2 = AssetManager.Instance.GetBundleEntry(item);
					AddBundle(bundleEntry2.Name, modify: false);
				}
			}
		}

		protected class ResResource : EditorModResource
		{
			private uint resType;

			private ulong resRid;

			private byte[] resMeta;

			public override ModResourceType Type => ModResourceType.Res;

			public ResResource(ResAssetEntry entry, Manifest manifest)
			{
				name = entry.Name.ToLower();
				sha1 = entry.ModifiedEntry.Sha1;
				resourceIndex = manifest.Add(entry.ModifiedEntry.Sha1, entry.ModifiedEntry.Data);
				size = entry.ModifiedEntry.OriginalSize;
				resType = entry.ResType;
				resRid = entry.ResRid;
				resMeta = ((entry.ModifiedEntry.ResMeta != null) ? entry.ModifiedEntry.ResMeta : entry.ResMeta);
				userData = entry.ModifiedEntry.UserData;
				flags = (byte)(entry.IsInline ? 1 : 0);
				foreach (int bundle in entry.Bundles)
				{
					BundleEntry bundleEntry = AssetManager.Instance.GetBundleEntry(bundle);
					AddBundle(bundleEntry.Name, modify: true);
				}
				foreach (int item in entry.EnumerateBundles(addedOnly: true))
				{
					BundleEntry bundleEntry2 = AssetManager.Instance.GetBundleEntry(item);
					AddBundle(bundleEntry2.Name, modify: false);
				}
			}

			public override void Write(NativeWriter writer)
			{
				base.Write(writer);
				writer.Write(resType);
				writer.Write(resRid);
				writer.Write(resMeta.Length);
				writer.Write(resMeta);
			}
		}

		protected class ChunkResource : EditorModResource
		{
			private uint rangeStart;

			private uint rangeEnd;

			private uint logicalOffset;

			private uint logicalSize;

			private int h32;

			private int firstMip;

			public override ModResourceType Type => ModResourceType.Chunk;

			public ChunkResource(ChunkAssetEntry entry, Manifest manifest)
			{
				name = entry.Id.ToString();
				sha1 = entry.ModifiedEntry.Sha1;
				resourceIndex = manifest.Add(entry.ModifiedEntry.Sha1, entry.ModifiedEntry.Data);
				size = entry.ModifiedEntry.OriginalSize;
				rangeStart = entry.ModifiedEntry.RangeStart;
				rangeEnd = entry.ModifiedEntry.RangeEnd;
				logicalOffset = entry.ModifiedEntry.LogicalOffset;
				logicalSize = entry.ModifiedEntry.LogicalSize;
				h32 = entry.ModifiedEntry.H32;
				firstMip = entry.ModifiedEntry.FirstMip;
				userData = entry.ModifiedEntry.UserData;
				flags = (byte)(entry.IsInline ? 1 : 0);
				//flags |= (byte)(entry.ModifiedEntry.AddToChunkBundle ? 2 : 0);
				if (entry.ModifiedEntry.AddToChunkBundle)
				{
					//if (ProfilesLibrary.MustAddChunks || entry.IsAdded)
					//{
					//	AddBundle("chunks", modify: false);
					//}
					//else
					//{
						AddBundle("chunks", modify: true);
					//}
				}
				foreach (int bundle in entry.Bundles)
				{
					BundleEntry bundleEntry = AssetManager.Instance.GetBundleEntry(bundle);
					if(bundleEntry != null)
						AddBundle(!string.IsNullOrEmpty(bundleEntry.Name) ? bundleEntry.Name : bundle.ToString(), modify: true);
				}
				foreach (int bundle in entry.EnumerateBundles(addedOnly: true))
				{
					BundleEntry bundleEntry = AssetManager.Instance.GetBundleEntry(bundle);
					if(bundleEntry != null)
						AddBundle(!string.IsNullOrEmpty(bundleEntry.Name) ? bundleEntry.Name : bundle.ToString(), modify: false);
				}
			}

			public override void Write(NativeWriter writer)
			{
				base.Write(writer);
				writer.Write(rangeStart);
				writer.Write(rangeEnd);
				writer.Write(logicalOffset);
				writer.Write(logicalSize);
				writer.Write(h32);
				writer.Write(firstMip);
			}
		}


		protected class LegacyFileResource : EditorModResource
		{
			public override ModResourceType Type => ModResourceType.Legacy;

			public LegacyFileResource(LegacyFileEntry entry, Manifest manifest)
			{
				name = entry.Name;
				size = entry.ModifiedEntry.Size;
				resourceIndex = manifest.Add(entry.Name, entry.ModifiedEntry.Data);
			}

			public override void Write(NativeWriter writer)
			{
				base.Write(writer);
				writer.Write(name);
			}
		}

		protected class EmbeddedFileResource : EditorModResource
		{
			public override ModResourceType Type => ModResourceType.EmbeddedFile;

			public EmbeddedFileResource(EmbeddedFileEntry entry, Manifest manifest)
			{
				name = entry.ExportedRelativePath;
				size = entry.Data.Length;
				resourceIndex = manifest.Add(entry.Name, entry.Data);
			}

			public override void Write(NativeWriter writer)
			{
				base.Write(writer);
				writer.Write(name);
			}
		}

		private ModSettings overrideSettings;

		protected Manifest manifest = new Manifest();

		protected List<BaseModResource> resources = new List<BaseModResource>();

		public Manifest ResourceManifest => manifest;

		public FrostbiteModWriter(Stream inStream, ModSettings inOverrideSettings = null)
			: base(inStream)
		{
			overrideSettings = inOverrideSettings;
		}

		public virtual void WriteProject(FrostbiteProject project)
		{
			Write(FrostbiteMod.Magic);
			Write(FrostbiteMod.Version);
			Write(16045690984833335023uL);
			Write(3735928559u);
			Write(ProfilesLibrary.ProfileName);
			Write(FileSystem.Instance.Head);
			ModSettings modSettings = overrideSettings;
			if (modSettings == null)
			{
				modSettings = project.ModSettings;
			}
			WriteNullTerminatedString(modSettings.Title);
			WriteNullTerminatedString(modSettings.Author);
			WriteNullTerminatedString(modSettings.Category);
			WriteNullTerminatedString(modSettings.Version);
			WriteNullTerminatedString(modSettings.Description);

			// -----------------------------------------------------
			// Embedded Files
			// --------------------------------------------------
			// Convert Locale.Ini mod to EmbeddedFileEntry
			if (AssetManager.Instance.LocaleINIMod.HasUserData)
			{
				project.AssetManager.EmbeddedFileEntries.RemoveAll(x 
					=> x.ImportedFileLocation.Contains("Locale.ini", StringComparison.OrdinalIgnoreCase)
					|| x.ExportedRelativePath.Contains("Locale.ini", StringComparison.OrdinalIgnoreCase)
					);
				project.AssetManager.EmbeddedFileEntries.Add(new EmbeddedFileEntry() {
					Name = "Locale.ini",
					ImportedFileLocation = "PROJECT",
					ExportedRelativePath = "Data\\Locale.ini",
					Data = AssetManager.Instance.LocaleINIMod.UserDataEncrypted
				});
			}
			// 5 = Icon and Screenshots
			// The count of embedded files is added
			Write(5 + project.AssetManager.EmbeddedFileEntries.Count);
			AddResource(new EmbeddedResource("Icon;", modSettings.Icon, manifest));
			for (int i = 0; i < 4; i++)
			{
				AddResource(new EmbeddedResource("Screenshot;" + i.ToString(), modSettings.GetScreenshot(i), manifest));
			}
			for (int i = 0; i < project.AssetManager.EmbeddedFileEntries.Count; i++)
			{
				var efe = project.AssetManager.EmbeddedFileEntries[i];
				AddResource(new EmbeddedResource("efe;" + efe.ExportedRelativePath, efe.Data, manifest));
			}
			// end of embedded
			// ----------------------------------------------------

			foreach (BundleEntry bundleEntry in AssetManager.Instance.EnumerateBundles(BundleType.None, modifiedOnly: true))
			{
				if (bundleEntry.Added)
				{
					AddResource(new BundleResource(bundleEntry, manifest));
				}
			}
			foreach (EbxAssetEntry ebxAsset in AssetManager.Instance.EnumerateEbx("", modifiedOnly: true))
			{
				if (!ebxAsset.ModifiedEntry.IsTransientModified && ebxAsset.HasModifiedData)
				{
					AddResource(new EbxResource(ebxAsset, manifest));
				}
			}
			foreach (ResAssetEntry resAsset in AssetManager.Instance.EnumerateRes(0u, modifiedOnly: true))
			{
				if (resAsset.HasModifiedData)
				{
                    bool flag = false;
					var a = Assembly.GetAssembly(this.GetType());

					if (a == null)
						a = Assembly.GetEntryAssembly();
					//if (a == null)
					//	a = Assembly.GetCallingAssembly();
					if (a == null)
						a = Assembly.GetExecutingAssembly();
					var allAttr = a.GetCustomAttributes(true);
					var onlyRes = a.GetCustomAttributes<ResCustomHandlerAttribute>();

					foreach (ResCustomHandlerAttribute customAttribute in onlyRes)
                    {
                        if (customAttribute.ResType == (ResourceType)resAsset.ResType)
                        {
                            ((ICustomActionHandler)Activator.CreateInstance(customAttribute.CustomHandler)).SaveToMod(this, resAsset);
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        AddResource(new ResResource(resAsset, manifest));
                    }
                }
			}
			foreach (ChunkAssetEntry chunkEntry in AssetManager.Instance.EnumerateChunks(modifiedOnly: true))
			{
				if (chunkEntry.HasModifiedData)
				{
					AddResource(new ChunkResource(chunkEntry, manifest));
				}
			}
			// Write Legacy stuff
			foreach (LegacyFileEntry lfe in AssetManager.Instance.EnumerateCustomAssets("legacy", true))
			{
				if(lfe.HasModifiedData)
                {
					AddResource(new LegacyFileResource(lfe, manifest));
                }
			}
			// Write Embedded stuff
			foreach (EmbeddedFileEntry efe in AssetManager.Instance.EmbeddedFileEntries)
			{
				AddResource(new EmbeddedFileResource(efe, manifest));
			}
			Write(resources.Count);
			foreach (EditorModResource resource in resources)
			{
				resource.Write(this);
			}


			long manifestDataPosition = BaseStream.Position;
			manifest.Write(this);
			long legacyFilePosition = BaseStream.Position;
			BaseStream.Position = 12L;
			Write(manifestDataPosition);
			Write(manifest.Count);

			

		}

		public void AddResource(BaseModResource resource)
		{
			resources.Add(resource);
		}
	}
}
