using Frosty.Hash;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrostySdk.Frosty.FET
{
	public class FIFAModWriter : FrostbiteModWriter
	{
		private readonly AssetManager assetManager;

		private readonly FileSystem fileSystem;

		private readonly string gameName;

		private ModSettings overrideSettings;

		public FIFAModWriter(string gameName, AssetManager assetManager, FileSystem fileSystem, Stream inStream, ModSettings inOverrideSettings = null)
			: base(inStream)
		{
			this.gameName = gameName;
			this.assetManager = assetManager ?? throw new ArgumentNullException("assetManager");
			this.fileSystem = fileSystem ?? throw new ArgumentNullException("fileSystem");
			overrideSettings = inOverrideSettings;
		}

		public override void WriteProject(FrostbiteProject project)
		{
			if (project == null)
			{
				throw new ArgumentNullException("project");
			}
			Write(5498700893333637446ul);
			WriteUInt32LittleEndian(8u);
			Write(16045690984833335023uL);
			WriteUInt32LittleEndian(3735928559u);
			WriteLengthPrefixedString("FIFA21");
			WriteUInt32LittleEndian(AssetManager.Instance.fs.Head);
			ModSettings modSettings = overrideSettings ?? project.ModSettings;
			WriteLengthPrefixedString(modSettings.Title);
			WriteLengthPrefixedString(modSettings.Author);
			Write((byte)byte.MaxValue);
			Write(Convert.ToByte(0));
			WriteLengthPrefixedString(string.Empty);
			WriteLengthPrefixedString(string.Empty);
			WriteLengthPrefixedString(modSettings.Version);
			WriteLengthPrefixedString(modSettings.Description);
			WriteLengthPrefixedString(string.Empty);
			WriteLengthPrefixedString(string.Empty);
			WriteLengthPrefixedString(string.Empty);
			WriteLengthPrefixedString(string.Empty);
			WriteLengthPrefixedString(string.Empty);
			WriteLengthPrefixedString(string.Empty);
			WriteLengthPrefixedString(string.Empty);
			WriteLengthPrefixedString(string.Empty);
			AddResource(new EmbeddedResource("Icon", modSettings.Icon, this.manifest));
			WriteUInt32LittleEndian((uint)4);
            for (int i = 0; i < 4; i++)
            {
                AddResource(new EmbeddedResource("Screenshot" + i.ToString(), modSettings.GetScreenshot(i), manifest));
            }
            foreach (EbxAssetEntry ebxAsset in assetManager.EnumerateEbx("", modifiedOnly: true))
			{
				if (!ebxAsset.ModifiedEntry.IsTransientModified && ebxAsset.HasModifiedData)
				{
					AddResource(new EbxResource(ebxAsset, this.manifest));
				}
			}
			foreach (ResAssetEntry resAsset in assetManager.EnumerateRes(0u, modifiedOnly: true))
			{
				if (resAsset.HasModifiedData)
				{
					AddResource(new ResResource(resAsset, this.manifest));
				}
			}
			foreach (ChunkAssetEntry chunkAsset in assetManager.EnumerateChunks(modifiedOnly: true))
			{
				if (chunkAsset.HasModifiedData)
				{
					AddResource(new ChunkResource(chunkAsset, this.manifest));
				}
			}
			WriteInt32LittleEndian(resources.Count);
			foreach (EditorModResource resource in resources)
			{
				resource.Write(this);
			}

			long manifestPosition = base.Position;
			this.manifest.Write(this);
			base.Position = 12L;
			WriteInt64LittleEndian(manifestPosition);
			WriteInt32LittleEndian(this.manifest.Count);
		}

	}
}
