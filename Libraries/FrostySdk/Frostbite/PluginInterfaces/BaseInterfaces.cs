using FrostySdk.Managers;
using FrostySdk.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.Frostbite.PluginInterfaces
{
	public interface Importer
	{
		public void DoImport(string path, AssetEntry assetEntry);

	}

	public interface ITextureImporter : Importer
	{
		public void DoImport(string path, EbxAssetEntry assetEntry, ref Texture textureAsset);

	}

	public interface ICacheWriter
    {
		public void Write();
    }

	public interface ICacheReader
	{
		public bool Read();
	}
}
