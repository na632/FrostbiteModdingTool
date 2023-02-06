using FMT.FileTools;
using Frostbite.Textures;
using FrostbiteSdk;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

            if(new FileInfo(path).Extension.ToLower() == ".json")
                return ImportWithJSON(path);

            return ImportBinary(path);
        }

        private bool ImportBinary(string path)
        {
            if (new FileInfo(path).Extension.ToLower() != ".bin")
                return false;


            return false;
        }

        private bool ImportWithJSON(string path)
        {
            if(new FileInfo(path).Extension.ToLower() != ".json")
                return false;

            var ebxAssetEntry = SelectedEntry as EbxAssetEntry;
            if (ebxAssetEntry == null)
                return false;

            var ebx = AssetManager.Instance.GetEbx(ebxAssetEntry);
            if (ebx == null) 
                return false;

            if (ebx.RootObject == null)
                return false;

            var replicatedObjFromJson = Activator.CreateInstance(ebx.RootObject.GetType());

            try
            {
                JsonConvert.PopulateObject(File.ReadAllText(path), replicatedObjFromJson);
            }
            catch
            {
                FileLogger.WriteLine($"Unable to process JSON file {path} into Object {ebx.RootObject.GetType().FullName}");
                //EventLog.WriteEntry("FMT", $"Unable to process JSON file {path} into Object {ebx.RootObject.GetType().FullName}");
                return false;
            }

            foreach(var ebxRootProperty in ebx.RootObject.GetProperties())
            {
                var jsonVersionOfProperty = replicatedObjFromJson.GetProperty(ebxRootProperty.Name);
                if (jsonVersionOfProperty == null)
                    continue;

                if (ebxRootProperty.PropertyType.Name.StartsWith("PointerRef"))
                {
                    //var jsonVerOfPropPointerRef = jsonVersionOfProperty.GetValue(replicatedObjFromJson);
                    //if (jsonVerOfPropPointerRef == null)
                    //    continue;

                    //var ebxVerOfPropPointerRef = ebxRootProperty.GetValue(ebx.RootObject);
                    //if (ebxVerOfPropPointerRef == null)
                    //    continue;

                    //var jsonInternalV = jsonVerOfPropPointerRef.GetPropertyValue("Internal");
                    //if (jsonInternalV == null)
                    //    continue;

                    //var jsonInternalType = jsonInternalV.GetType();

                    //var ebxInternalV = ebxVerOfPropPointerRef.GetPropertyValue("Internal");
                    //if (ebxInternalV == null)
                    //    continue;

                    //var ebxInternalType = ebxInternalV.GetType();

                    //if (ebxInternalType != jsonInternalType)
                    //    continue;

                    //foreach (var ebxPRInternalProperty in ebxInternalV.GetProperties())
                    //{
                    //    if (ebxPRInternalProperty.PropertyType.Name.StartsWith("__Guid"))
                    //        continue;

                    //    if (ebxPRInternalProperty.PropertyType.Name.StartsWith("__InstanceGuid"))
                    //        continue;

                    //    var jsonVersionOfPropertyInternal = jsonInternalV.GetProperty(ebxPRInternalProperty.Name);
                    //    if (jsonVersionOfPropertyInternal == null)
                    //        continue;

                    //    ebxPRInternalProperty.SetValue(ebxInternalV, jsonVersionOfPropertyInternal.GetValue(jsonInternalV));

                    //}
                }
                else
                {
                    try 
                    {
                        ebxRootProperty.SetValue(ebx.RootObject, jsonVersionOfProperty.GetValue(replicatedObjFromJson));
                    }
                    catch
                    {
                        FileLogger.WriteLine($"Unable to process JSON property {jsonVersionOfProperty.Name} into Object {ebx.RootObject.GetType().FullName}");
                    }
                }

            }

            AssetManager.Instance.ModifyEbx(ebxAssetEntry.Name, ebx);

            // This is an assumption that everything above worked fine
            return true;
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
                openFileDialog.Filter = $"Files (*.json,*.bin)|*.json;*.bin";

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
                result = textureImporter.Import(path, (EbxAssetEntry)SelectedEntry, ref texture);
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
