using Frosty.Hash;
using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FrostySdk
{
	public class FrostyModWriter : NativeWriter
	{
		public class Manifest
		{
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

		private class EmbeddedResource : EditorModResource
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

		private class BundleResource : EditorModResource
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

		private class EbxResource : EditorModResource
		{
			public override ModResourceType Type => ModResourceType.Ebx;

			public EbxResource(EbxAssetEntry entry, Manifest manifest)
			{
				CompressionType compressionOverride = (ProfilesLibrary.DataVersion == 20170929) ? CompressionType.ZStd : CompressionType.Default;
				byte[] array = null;
				name = entry.Name.ToLower();
				userData = entry.ModifiedEntry.UserData;
				using (EbxBaseWriter ebxBaseWriter = ((ProfilesLibrary.DataVersion == 20190911) | (ProfilesLibrary.DataVersion == 20190905)) ? ((EbxBaseWriter)new EbxWriterV2(new MemoryStream())) : ((EbxBaseWriter)new EbxWriter(new MemoryStream())))
				{
					ebxBaseWriter.WriteAsset(entry.ModifiedEntry.DataObject as EbxAsset);
					size = ebxBaseWriter.BaseStream.Length;
					array = Utils.CompressFile(((MemoryStream)ebxBaseWriter.BaseStream).ToArray(), null, ResourceType.Invalid, compressionOverride);
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

		private class ResResource : EditorModResource
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

		private class ChunkResource : EditorModResource
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
				flags |= (byte)(entry.ModifiedEntry.AddToChunkBundle ? 2 : 0);
				if (entry.ModifiedEntry.AddToChunkBundle)
				{
					if (ProfilesLibrary.MustAddChunks || entry.IsAdded)
					{
						AddBundle("chunks", modify: false);
					}
					else
					{
						AddBundle("chunks", modify: true);
					}
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

		private ModSettings overrideSettings;

		private Manifest manifest = new Manifest();

		private List<BaseModResource> resources = new List<BaseModResource>();

		public Manifest ResourceManifest => manifest;

		public FrostyModWriter(Stream inStream, ModSettings inOverrideSettings = null)
			: base(inStream)
		{
			overrideSettings = inOverrideSettings;
		}

		public void WriteProject(FrostyProject project)
		{
			Write(FrostyMod.Magic);
			Write(FrostyMod.Version);
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
			AddResource(new EmbeddedResource("Icon", modSettings.Icon, manifest));
			for (int i = 0; i < 4; i++)
			{
				AddResource(new EmbeddedResource("Screenshot" + i.ToString(), modSettings.GetScreenshot(i), manifest));
			}
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
					//bool flag = false;
					//foreach (ResCustomHandlerAttribute customAttribute in Assembly.GetEntryAssembly().GetCustomAttributes<ResCustomHandlerAttribute>())
					//{
					//	if (customAttribute.ResType == (ResourceType)resAsset.ResType)
					//	{
					//		((ICustomActionHandler)Activator.CreateInstance(customAttribute.CustomHandler)).SaveToMod(this, resAsset);
					//		flag = true;
					//		break;
					//	}
					//}
					//if (!flag)
					//{
						AddResource(new ResResource(resAsset, manifest));
					//}
				}
			}
			foreach (ChunkAssetEntry chunkEntry in AssetManager.Instance.EnumerateChunks(modifiedOnly: true))
			{
				if (chunkEntry.HasModifiedData)
				{
					AddResource(new ChunkResource(chunkEntry, manifest));
				}
			}
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (Type type in types)
			{
				if (type.GetInterface(typeof(ICustomActionHandler).Name) != null)
				{
					((ICustomActionHandler)Activator.CreateInstance(type))?.SaveToMod(this);
				}
			}
			Write(resources.Count);
			foreach (EditorModResource resource in resources)
			{
				resource.Write(this);
			}
			long position = BaseStream.Position;
			manifest.Write(this);
			BaseStream.Position = 12L;
			Write(position);
			Write(manifest.Count);
		}

		public void AddResource(BaseModResource resource)
		{
			resources.Add(resource);
		}
	}
}
