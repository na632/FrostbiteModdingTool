using FMT.FileTools;
using FMT.FileTools.Modding;
using FrostbiteSdk.FrostbiteSdk.Managers;
using FrostySdk.Frostbite.IO.Output;
using FrostySdk.Frosty.FET;
using FrostySdk.IO;
using FrostySdk.Managers;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModding.Frosty;

namespace FrostySdk.ModsAndProjects.Projects
{
    public class FMTProject
    {
        public static int MasterProjectVersion { get; } = 1;
        private static string projectExtension { get; } = ".fmtproj";
        private static string projectFilter { get; } = $"FMT Project file (*{projectExtension})|*{projectExtension}";
        private string projectFilePath { get; set; }

        public FileInfo projectFileInfo { get { return new FileInfo(projectFilePath); } }


        public ModSettings GetModSettings()
        {
            return ProjectManagement.Instance.Project.ModSettings;
        }


        public FMTProject(string filePath)
        {
            projectFilePath = filePath;
        }

        public static FMTProject Create(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new NullReferenceException(nameof(filePath));

            if(!filePath.EndsWith(projectExtension, StringComparison.Ordinal))  
                filePath += projectExtension;    

            if (File.Exists(filePath))
                File.Delete(filePath);

            FMTProject project = new FMTProject(filePath);// this;
            project.projectFilePath = filePath;
            if (!project.projectFileInfo.Directory.Exists)
                Directory.CreateDirectory(project.projectFileInfo.DirectoryName);

            project.Update();
            return project;
        }

        public static FMTProject Read(string filePath)
        {
            if(!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            FMTProject project = new FMTProject(filePath);
            return project;
        }

        public FMTProject Update()
        {
            if (File.Exists(projectFilePath))
                File.Delete(projectFilePath);

            //var entryExporter = new AssetEntryExporter();
            using (var ms = new MemoryStream())
            {
                using (var nw = new NativeWriter(ms, true))
                {
                    // Master Version
                    nw.Write(MasterProjectVersion);
                    // Profile Version
                    nw.Write(ProfileManager.DataVersion);
                    // Mod Settings Json
                    nw.WriteLengthPrefixedString(JsonConvert.SerializeObject(GetModSettings()));
                    // EBX
                    EBXAssetsWrite(nw);
                    // RES
                    ResourceAssetsWrite(nw);
                    // CHUNK
                    ChunkAssetsWrite(nw);
                    // Legacy
                    LegacyFilesModifiedWrite(nw);
                    LegacyFilesAddedWrite(nw);
                    // -----------------------
                    // Embedded files
                    EmbeddedFilesWrite(nw);
                    // ------------------------
                    // Locale.ini mod
                    LocaleINIWrite(nw);
                }

                //var msComp = new MemoryStream();
                //new System.IO.Compression.ZLibStream(ms, System.IO.Compression.CompressionLevel.Optimal).CopyTo(msComp);
                File.WriteAllBytes(projectFilePath, ms.ToArray());
            }

            return this;
        }

        private static void EBXAssetsWrite(NativeWriter nw)
        {

            var modifiedEbxAssets = AssetManager.Instance.EnumerateEbx("", modifiedOnly: true, includeLinked: true);
            // EBX Count
            nw.Write(modifiedEbxAssets != null ? modifiedEbxAssets.Count() : 0);
            foreach (var item in modifiedEbxAssets)
            {
                AssetEntryExporter assetEntryExporter = new AssetEntryExporter(item);
                // Item Name
                nw.WriteLengthPrefixedString(item.Name);
                // EBX Stream to Json
                nw.WriteLengthPrefixedString(assetEntryExporter.ExportToJson());
            }
        }

        private static void ResourceAssetsWrite(NativeWriter nw)
        {
            var modifiedResourceAssets = AssetManager.Instance.EnumerateRes(modifiedOnly: true);
            // RES Count
            nw.Write(modifiedResourceAssets != null ? modifiedResourceAssets.Count() : 0);
            foreach (var item in modifiedResourceAssets)
            {
                // Item Name
                nw.WriteLengthPrefixedString(item.Name);
                // Item Data
                nw.Write(item.ModifiedEntry.Data);
            }
        }

        private static void ChunkAssetsWrite(NativeWriter nw)
        {
            var modifiedChunkAssets = AssetManager.Instance.EnumerateChunks(modifiedOnly: true);
            // CHUNK Count
            nw.Write(modifiedChunkAssets != null ? modifiedChunkAssets.Count() : 0);
            foreach (var item in modifiedChunkAssets)
            {
                // Item Name
                nw.WriteLengthPrefixedString(item.Name);
                // Item Data
                nw.Write(item.ModifiedEntry.Data);
            }
        }

        private static void LegacyFilesAddedWrite(NativeWriter nw)
        {
            // -----------------------
            // Added Legacy Files
            var hasAddedLegacyFiles = AssetManager.Instance.CustomAssetManagers["legacy"].AddedFileEntries.Count > 0;
            nw.Write(hasAddedLegacyFiles);
            if (hasAddedLegacyFiles)
            {
                nw.Write(AssetManager.Instance.CustomAssetManagers["legacy"].AddedFileEntries.Count);
                foreach (var lfe in AssetManager.Instance.CustomAssetManagers["legacy"].AddedFileEntries)
                {
                    nw.WriteLengthPrefixedString(JsonConvert.SerializeObject(lfe));
                }
            }
        }

        private static void LegacyFilesModifiedWrite(NativeWriter nw)
        {
            nw.Write(AssetManager.Instance.EnumerateCustomAssets("legacy", modifiedOnly: true).Count());
            foreach (LegacyFileEntry lfe in AssetManager.Instance.EnumerateCustomAssets("legacy", modifiedOnly: true))
            {
                if (lfe.Name != null)
                {
                    var serialisedLFE = JsonConvert.SerializeObject(lfe);
                    nw.WriteLengthPrefixedString(serialisedLFE);
                }
            }
        }

        private static void LocaleINIWrite(NativeWriter nw)
        {
            var hasLocaleIniMod = AssetManager.Instance.LocaleINIMod.HasUserData;
            nw.Write(hasLocaleIniMod);
            if (hasLocaleIniMod)
            {
                nw.Write(FileSystem.Instance.LocaleIsEncrypted);
                nw.Write(AssetManager.Instance.LocaleINIMod.UserData.Length);
                nw.Write(AssetManager.Instance.LocaleINIMod.UserData);
            }
        }

        private static void EmbeddedFilesWrite(NativeWriter nw)
        {
            nw.Write(AssetManager.Instance.EmbeddedFileEntries.Count > 0);
            nw.Write(AssetManager.Instance.EmbeddedFileEntries.Count);
            foreach (EmbeddedFileEntry efe in AssetManager.Instance.EmbeddedFileEntries)
            {
                var serialisedEFE = JsonConvert.SerializeObject(efe);
                nw.WriteLengthPrefixedString(serialisedEFE);
            }
        }

        public void WriteToMod(string filename, ModSettings overrideSettings)
        {
            byte[] projectbytes;

            if (File.Exists(filename))
                File.Delete(filename);

            var memoryStream = new MemoryStream();
            FrostbiteModWriter frostyModWriter = new FrostbiteModWriter(memoryStream, overrideSettings);
            frostyModWriter.WriteProject();

            memoryStream.Position = 0;
            projectbytes = new NativeReader(memoryStream).ReadToEnd();
            using NativeWriter nwFinal = new NativeWriter(new FileStream(filename, FileMode.CreateNew));
            nwFinal.Write(projectbytes);

        }

        public bool ReadFromFIFAMod(in FIFAModReader reader)
        {
            var resources = reader.ReadResources()
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Name);

            foreach (BaseModResource r in resources)
            {
                IAssetEntry entry = new AssetEntry();
                var t = r.GetType().Name;
                switch (t)
                {
                    case "EbxResource":
                        entry = new EbxAssetEntry();
                        break;
                    case "ResResource":
                        entry = new ResAssetEntry();
                        break;
                    case "ChunkResource":
                        entry = new ChunkAssetEntry();
                        break;
                    default:
                        entry = null;
                        break;
                }

                if (entry != null)
                {
                    r.FillAssetEntry(entry);
                    var d = reader.GetResourceData(r);
                    CasReader casReader = new CasReader(new MemoryStream(d));
                    var d2 = casReader.Read();
                    AssetManager.Instance.ModifyEntry(entry, d2);
                    if(entry is ChunkAssetEntry)
                    {
                        if(entry.ModifiedEntry != null && !string.IsNullOrEmpty(entry.ModifiedEntry.UserData))
                        {

                        }
                    }
                }
            }

            var modifiedEntries = AssetManager.Instance.ModifiedEntries;
            return modifiedEntries.Any();
        }

        public void WriteToFIFAMod(string filename, ModSettings overrideSettings)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
