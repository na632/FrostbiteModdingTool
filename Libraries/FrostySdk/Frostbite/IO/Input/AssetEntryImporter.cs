﻿using FMT.FileTools;
using Frostbite.Textures;
using FrostbiteSdk;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
//using System.Text.Json;
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
            if (AssetEntry is LegacyFileEntry)
            {
                return ImportChunkFileAsset(path);
            }

            ReadOnlySpan<char> entryType = SelectedEntry.Type.ToCharArray();
            if (entryType.StartsWith("TextureAsset", StringComparison.OrdinalIgnoreCase) || entryType.StartsWith("Texture", StringComparison.OrdinalIgnoreCase))
                return ImportEbxTexture(path);

            if (entryType.StartsWith("SkinnedMeshAsset", StringComparison.OrdinalIgnoreCase))
                return ImportEbxSkinnedMesh(path);

            if (new FileInfo(path).Extension.ToLower() == ".json")
                return ImportWithJSON(path);

            return ImportBinary(path);
        }

        private bool ImportBinary(string path)
        {
            if (new FileInfo(path).Extension.ToLower() != ".bin")
                return false;


            return false;
        }

        public bool Import(byte[] bytes)
        {
            return false;
        }

        public bool ImportWithJSON(string path)
        {
            if (new FileInfo(path).Extension.ToLower() != ".json")
                return false;

            var ebxAssetEntry = SelectedEntry as EbxAssetEntry;
            if (ebxAssetEntry == null)
                return false;

            var ebx = AssetManager.Instance.GetEbx(ebxAssetEntry);
            if (ebx == null)
                return false;

            if (ebx.RootObject == null)
                return false;

            try
            {
                return ImportWithJSON(File.ReadAllBytes(path));
            }
            catch
            {
                FileLogger.WriteLine($"Unable to process JSON {path} into Object {ebx.RootObject.GetType().FullName}");
            }

            return false;
        }

        public bool ImportWithJSON(byte[] bytes)
        {
            var ebxAssetEntry = SelectedEntry as EbxAssetEntry;
            if (ebxAssetEntry == null)
                return false;

            var ebx = AssetManager.Instance.GetEbx(ebxAssetEntry);
            if (ebx == null)
                return false;

            if (ebx.RootObject == null)
                return false;

            JObject jobjectFromJson = new JObject();
            try
            {
                JsonConvert.PopulateObject(Encoding.UTF8.GetString(bytes), ebx.RootObject, new JsonSerializerSettings()
                {
                    ObjectCreationHandling = ObjectCreationHandling.Auto,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = {
                        new ReplaceArrayConverter()
                        , new PointerRefConverter()
                    },
                    MaxDepth = 10
                });


                jobjectFromJson = JObject.Parse(Encoding.UTF8.GetString(bytes));

            }
            catch (Exception populationException)
            {
                FileLogger.WriteLine($"Unable to process JSON bytes into Object {ebx.RootObject.GetType().FullName}");
                FileLogger.WriteLine($"ERROR:");
                FileLogger.WriteLine(populationException.ToString());
                return false;
            }

            // Modify the Ebx after setting object
            AssetManager.Instance.ModifyEbx(ebxAssetEntry.Name, ebx);

            // This is an assumption that everything above worked fine
            return true;
        }

        public class ReplaceArrayConverter : JsonConverter
        {
            public override bool CanRead => base.CanRead;

            public override bool CanWrite => base.CanWrite;

            public override bool CanConvert(Type objectType)
            {
                // check for Array, IList, etc.
                //return objectType.IsArray || objectType.Name == "EbxImportReference";
                return objectType.IsArray || objectType.Name == "List`1";
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                // ignore existingValue and just create a new collection
                return JsonSerializer.CreateDefault().Deserialize(reader, objectType);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JsonSerializer.CreateDefault().Serialize(writer, value);
            }
        }

        public class PointerRefConverter : JsonConverter
        {
            public override bool CanRead => base.CanRead;

            public override bool CanWrite => base.CanWrite;

            public override bool CanConvert(Type objectType)
            {
                // check for Array, IList, etc.
                //return objectType.IsArray || objectType.Name == "EbxImportReference";
                return objectType.Name == "PointerRef";
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                // -----------------------------------------------------------------------
                // This will ATTEMPT to resolve the object dynamically

                try
                {

                    // Load JObject from stream
                    JObject jObject = JObject.Load(reader);
                    var externalObject = jObject["External"];
                    var internalObject = jObject["Internal"];
                    var nwInternal = jObject["Internal"].ToObject(existingValue.GetProperty("Internal").PropertyType);

                    foreach (var p in existingValue.GetProperties())
                    {
                        var vRoot = p.GetValue(existingValue);
                        if (vRoot != null)
                        {
                            foreach (var pVRoot in vRoot.GetProperties().Where(x => !x.Name.Contains("Guid", StringComparison.OrdinalIgnoreCase)))
                            {
                                var jsonInternal = internalObject[pVRoot.Name];
                                if (jsonInternal != null)
                                {
                                    pVRoot.SetValue(vRoot, jsonInternal.ToObject(pVRoot.PropertyType));
                                }
                            }
                        }
                    }

                }
                finally
                {

                }

                return existingValue;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JsonSerializer.CreateDefault().Serialize(writer, value);
            }
        }

        public async ValueTask<bool> ImportAsync(string path)
        {
            return await Task.FromResult(Import(path));
        }

        public OpenFileDialog GetOpenDialogWithFilter()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            ReadOnlySpan<char> entryType = SelectedEntry.Type.ToCharArray();
            if (entryType.Contains("DDS", StringComparison.OrdinalIgnoreCase))
                openFileDialog.Filter = imageFilter;
            else if (SelectedEntry is LegacyFileEntry SelectedLegacyEntry)
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
