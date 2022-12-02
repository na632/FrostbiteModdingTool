using Frostbite.Textures;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModding.Frosty;

namespace FrostySdk.Frostbite.IO.Input
{
    public class AssetEntryImporter
    {
        private string imageFilter = "Image files (*.dds, *.png)|*.dds;*.png";
        private string jsonFilter = "JSON files (*.json)|*.json";
        private string fbxFilter = "FBX files (*.fbx)|*.fbx";

        public AssetEntry AssetEntry { get; }
        public AssetEntry SelectedEntry => AssetEntry;
        
        public AssetEntryImporter(AssetEntry assetEntry) { AssetEntry = assetEntry; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Import(string path) 
        { 
            if(AssetEntry is LegacyFileEntry)
            {
                return ImportChunkFileAsset(path);
            }

            ReadOnlySpan<char> entryType = SelectedEntry.Type.ToCharArray();
            if (entryType.StartsWith("TextureAsset", StringComparison.OrdinalIgnoreCase) || entryType.StartsWith("Texture", StringComparison.OrdinalIgnoreCase))
                return ImportEbxTexture(path);

            if (entryType.StartsWith("SkinnedMeshAsset", StringComparison.OrdinalIgnoreCase))
                return ImportEbxSkinnedMesh(path);


            return false;
        }
        
        public async ValueTask<bool> ImportAsync(string path)
        {
            return await Task.FromResult(Import(path));
        }

        public OpenFileDialog GetOpenDialogWithFilter()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            ReadOnlySpan<char> entryType = SelectedEntry.Type.ToCharArray();
            if(entryType.Contains("DDS", StringComparison.OrdinalIgnoreCase))
                openFileDialog.Filter = imageFilter;
            else if(SelectedEntry is LegacyFileEntry SelectedLegacyEntry)
                openFileDialog.Filter = $"Files (*.{SelectedLegacyEntry.Type})|*.{SelectedLegacyEntry.Type}";
            else if (entryType.StartsWith("TextureAsset", StringComparison.OrdinalIgnoreCase) || entryType.StartsWith("Texture", StringComparison.OrdinalIgnoreCase))
                openFileDialog.Filter = imageFilter;
            else if (entryType.StartsWith("SkinnedMeshAsset", StringComparison.OrdinalIgnoreCase))
                openFileDialog.Filter = fbxFilter;
            else
                openFileDialog.Filter = $"Files (*.json)|*.json";

            return openFileDialog;
        }


        public bool ImportChunkFileAsset(string path)
        {
            var chunkFileEntry = (LegacyFileEntry)AssetEntry;
            byte[] bytes = File.ReadAllBytes(path);

            if (chunkFileEntry.Type.ToUpper() == "DDS")
            {
                return (AssetManager.Instance.DoLegacyImageImport(path, chunkFileEntry));
            }
            else
            {
                AssetManager.Instance.ModifyLegacyAsset(
                    AssetEntry.Name
                    , bytes
                    , false);
                return true;
            }
        }

     

        public bool ImportEbxTexture(string path) 
        {
            bool result = false;
            ReadOnlySpan<char> chars = SelectedEntry.Type.ToCharArray();
            if (chars.StartsWith("TextureAsset", StringComparison.OrdinalIgnoreCase) || SelectedEntry.Type == "Texture")
            {
                var resEntry = ProjectManagement.Instance.Project.AssetManager.GetResEntry(SelectedEntry.Name);
                if (resEntry == null)
                    return false;

                Texture texture = new Texture((EbxAssetEntry)SelectedEntry);
                TextureImporter textureImporter = new TextureImporter();
                result = (!textureImporter.Import(path, (EbxAssetEntry)SelectedEntry, ref texture));
                textureImporter = null;
                texture = null;
            }

            return result;
        }

        /// <summary>
        /// Imports an Ebx Skinned Mesh. This however assumes it wants to import a "player"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool ImportEbxSkinnedMesh(string path) 
        {
            return ImportEbxSkinnedMesh(path, "content/character/rig/skeleton/player/skeleton_player");
        }

        public bool ImportEbxSkinnedMesh(string path, string skeletonEntryPath)
        {
            if (!SelectedEntry.Type.StartsWith("SkinnedMeshAsset", StringComparison.OrdinalIgnoreCase))
                return false;

            FBXImporter importer = new FBXImporter();
            importer.ImportFBX(path, (EbxAssetEntry)SelectedEntry
                , new MeshImportSettings()
                {
                    SkeletonAsset = skeletonEntryPath
                });
            importer = null;
            return true;
        }


    }
}
