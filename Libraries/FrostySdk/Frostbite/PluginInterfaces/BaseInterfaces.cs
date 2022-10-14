﻿using FrostySdk.Interfaces;
using FrostySdk.IO;
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
		void WriteEbxEntry(NativeWriter nativeWriter, EbxAssetEntry ebxEntry);
		void WriteResEntry(NativeWriter nativeWriter, ResAssetEntry resEntry);
		void WriteChunkEntry(NativeWriter nativeWriter, ChunkAssetEntry chunkEntry);
	}

	public interface ICacheReader
	{
		public bool Read();
	}

    public interface IAssetLoader
    {
        void Load(AssetManager parent, BinarySbDataHelper helper);
    }

    public interface IAssetCompiler
    {
        bool Compile(FileSystem fs, ILogger logger, object frostyModExecuter);
    }
}
