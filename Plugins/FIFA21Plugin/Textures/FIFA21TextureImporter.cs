using Frostbite.Textures;
using Frosty.Hash;
using FrostySdk;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using System;
using System.Collections.Generic;
using System.IO;

namespace FIFA21Plugin.Textures
{
	public class FIFA21TextureImporter : ITextureImporter
	{
		public void DoImport(string path, EbxAssetEntry assetEntry, ref Texture textureAsset)
		{
			var extension = "DDS";
			var spl = path.Split('.');
			extension = spl[spl.Length - 1].ToUpper();

			var compatible_extensions = new List<string>() { "DDS", "PNG" };
			if (!compatible_extensions.Contains(extension))
			{
				return;
			}

			TextureUtils.ImageFormat imageFormat = TextureUtils.ImageFormat.DDS;
			Enum.Parse(imageFormat.GetType(), extension);

			MemoryStream memoryStream = null;
			TextureUtils.BlobData pOutData = default(TextureUtils.BlobData);
			if (imageFormat == TextureUtils.ImageFormat.DDS)
			{
				memoryStream = new MemoryStream(NativeReader.ReadInStream(new FileStream(path, FileMode.Open, FileAccess.Read)));
			}

			if (!Directory.Exists("Debugging"))
				Directory.CreateDirectory("Debugging");

			if (!Directory.Exists("Debugging\\Other\\"))
				Directory.CreateDirectory("Debugging\\Other\\");

			using (FileStream fileStream = new FileStream("Debugging\\Other\\_TextureImport.dat", FileMode.OpenOrCreate))
			{
				memoryStream.CopyTo(fileStream);
				fileStream.Flush();
			}
			memoryStream.Position = 0;

			

			using (NativeReader nativeReader = new NativeReader(memoryStream))
			{
				TextureUtils.DDSHeader dDSHeader = new TextureUtils.DDSHeader();
				if (dDSHeader.Read(nativeReader))
				{

				}

				var ebxAsset = AssetManager.Instance.GetEbx(assetEntry);
				ulong resRid = ((dynamic)ebxAsset.RootObject).Resource;
				ResAssetEntry resEntry = AssetManager.Instance.GetResEntry(resRid);
				ChunkAssetEntry chunkEntry = AssetManager.Instance.GetChunkEntry(textureAsset.ChunkId);
				byte[] textureArray = new byte[nativeReader.Length - nativeReader.Position];
				nativeReader.Read(textureArray, 0, (int)(nativeReader.Length - nativeReader.Position));
				AssetManager.Instance.ModifyChunk(textureAsset.ChunkId, textureArray, textureAsset);
				AssetManager.Instance.ModifyRes(resRid, textureAsset.ToBytes());
				//AssetManager.Instance.ModifyEbx(assetEntry.Name, ebxAsset);
				resEntry.LinkAsset(chunkEntry);
				assetEntry.LinkAsset(resEntry);
			}

		}

		public void DoImport(string path, AssetEntry assetEntry)
		{
			throw new NotImplementedException();
		}
	}
}