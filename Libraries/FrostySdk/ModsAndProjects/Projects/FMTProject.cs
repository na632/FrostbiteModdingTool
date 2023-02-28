using FMT.FileTools;
using FMT.FileTools.Modding;
using FrostbiteSdk.FrostbiteSdk.Managers;
using FrostySdk.Frostbite.IO.Input;
using FrostySdk.Frostbite.IO.Output;
using FrostySdk.IO;
using FrostySdk.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            if (!filePath.EndsWith(projectExtension, StringComparison.Ordinal))
                filePath += projectExtension;

            projectFilePath = filePath;
        }

        public static FMTProject Create(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new NullReferenceException(nameof(filePath));

            if (!filePath.EndsWith(projectExtension, StringComparison.Ordinal))
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
            if (!filePath.EndsWith(projectExtension, StringComparison.Ordinal))
                filePath += projectExtension;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            FMTProject project = new FMTProject(filePath);
            using (NativeReader nr = new NativeReader(filePath))
            {
                nr.ReadInt();
                nr.ReadInt();
                nr.ReadLengthPrefixedString();

                var assetManagerPositions = new Dictionary<string, long>();
                var countOfAssetManagers = nr.ReadInt();
                for (var indexAM = 0; indexAM < countOfAssetManagers; indexAM++)
                {
                    assetManagerPositions.Add(nr.ReadLengthPrefixedString(), nr.ReadLong());
                }
                // Read Data
                nr.Position = assetManagerPositions["ebx"];
                EBXAssetsRead(nr);
                nr.Position = assetManagerPositions["res"];
                ResourceAssetsRead(nr);
                nr.Position = assetManagerPositions["chunks"];
                ChunkAssetsRead(nr);
                foreach (var kvp in assetManagerPositions.Skip(3))
                {
                    nr.Position = kvp.Value;
                    if (kvp.Key == "legacy")
                    {
                    }
                }
            }
            return project;
        }


        public FMTProject Update()
        {
            if (File.Exists(projectFilePath))
                File.Delete(projectFilePath);

            Dictionary<string, long> writePositions = new Dictionary<string, long>();

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

                    // number of positional stuff. ebx, res, chunks + legacy etc
                    nw.Write(3 + AssetManager.Instance.CustomAssetManagers.Count);
                    // --------- TODO: Convert these to "Custom" Asset Managers -----------------
                    nw.WriteLengthPrefixedString("ebx"); // key of cam
                    var ebxWritePosition = nw.Position;
                    nw.WriteULong(0ul); // position of data

                    nw.WriteLengthPrefixedString("res"); // key of cam
                    var resWritePosition = nw.Position;
                    nw.WriteULong(0ul); // position of data

                    nw.WriteLengthPrefixedString("chunks"); // key of cam
                    var chunksWritePosition = nw.Position;
                    nw.WriteULong(0ul); // position of data

                    var startOfCAMPosition = nw.Position;
                    // Custom Asset Managers
                    foreach (var cam in AssetManager.Instance.CustomAssetManagers)
                    {
                        nw.WriteLengthPrefixedString(cam.Key); // key of cam
                        nw.WriteULong(0ul); // position of data
                    }

                    // EBX
                    writePositions.Add("ebx", nw.Position);
                    EBXAssetsWrite(nw);
                    // RES
                    writePositions.Add("res", nw.Position);
                    ResourceAssetsWrite(nw);
                    // CHUNK
                    writePositions.Add("chunks", nw.Position);
                    ChunkAssetsWrite(nw);

                    // Legacy
                    writePositions.Add("legacy", nw.Position);
                    LegacyFilesModifiedWrite(nw);
                    LegacyFilesAddedWrite(nw);
                    // -----------------------
                    // Embedded files
                    writePositions.Add("embedded", nw.Position);
                    EmbeddedFilesWrite(nw);
                    // ------------------------
                    // Locale.ini mod
                    writePositions.Add("localeini", nw.Position);
                    LocaleINIWrite(nw);

                    nw.Position = ebxWritePosition;
                    nw.Write(writePositions["ebx"]);
                    nw.Position = resWritePosition;
                    nw.Write(writePositions["res"]);
                    nw.Position = chunksWritePosition;
                    nw.Write(writePositions["chunks"]);

                    nw.Position = startOfCAMPosition;
                    // Custom Asset Managers
                    foreach (var cam in AssetManager.Instance.CustomAssetManagers)
                    {
                        nw.WriteLengthPrefixedString(cam.Key); // key of cam
                        nw.Write(writePositions[cam.Key]);
                    }
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

        private static void EBXAssetsRead(NativeReader nr)
        {
            // EBX Count
            var count = nr.ReadInt();
            for (var i = 0; i < count; i++)
            {
                // Item Name
                //nw.WriteLengthPrefixedString(item.Name);
                var assetName = nr.ReadLengthPrefixedString();
                // EBX Stream to Json
                //nw.WriteLengthPrefixedString(assetEntryExporter.ExportToJson());
                var json = nr.ReadLengthPrefixedString();

                AssetEntryImporter assetEntryImporter = new AssetEntryImporter(AssetManager.Instance.GetEbxEntry(assetName));
                assetEntryImporter.Import(Encoding.UTF8.GetBytes(json));
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
                nw.WriteLengthPrefixedBytes(item.ModifiedEntry.Data);
            }
        }

        private static void ResourceAssetsRead(NativeReader nr)
        {
            // RES Count
            var count = nr.ReadInt();
            for (var i = 0; i < count; i++)
            {
                // Item Name
                var assetName = nr.ReadLengthPrefixedString();
                // Item Data
                var data = nr.ReadLengthPrefixedBytes();
                AssetManager.Instance.ModifyRes(assetName, data);
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
                nw.Write(item.Id);
                // Item Data
                nw.WriteLengthPrefixedBytes(item.ModifiedEntry.Data);
            }
        }

        private static void ChunkAssetsRead(NativeReader nr)
        {
            // CHUNK Count
            var count = nr.ReadInt();
            for (var i = 0; i < count; i++)
            {
                // Item Name
                var assetName = nr.ReadGuid();
                // Item Data
                var data = nr.ReadLengthPrefixedBytes();
                AssetManager.Instance.ModifyChunk(assetName, data);
            }
        }

        private static void LegacyFilesAddedWrite(NativeWriter nw)
        {
            // -----------------------
            // Added Legacy Files
            nw.WriteLengthPrefixedString("legacy"); // CFC 
            nw.Write(AssetManager.Instance.CustomAssetManagers["legacy"].AddedFileEntries.Count); // Count Added
            foreach (var lfe in AssetManager.Instance.CustomAssetManagers["legacy"].AddedFileEntries)
            {
                nw.WriteLengthPrefixedString(JsonConvert.SerializeObject(lfe));
            }
        }

        private static void LegacyFilesAddedRead(NativeReader nr)
        {
            // -----------------------
            // Added Legacy Files
            nr.ReadLengthPrefixedString(); // CFC 
            var countAdded = nr.ReadInt(); // Count Added
            for (var iCount = 0; iCount < countAdded; iCount++)
            {
                nr.ReadLengthPrefixedString();
            }
        }

        private static void LegacyFilesModifiedWrite(NativeWriter nw)
        {
            nw.WriteLengthPrefixedString("legacy"); // CFC
            nw.Write(AssetManager.Instance.EnumerateCustomAssets("legacy", modifiedOnly: true).Count()); // Count Added
            foreach (LegacyFileEntry lfe in AssetManager.Instance.EnumerateCustomAssets("legacy", modifiedOnly: true))
            {
                if (lfe.Name != null)
                {
                    var serialisedLFE = JsonConvert.SerializeObject(lfe);
                    nw.WriteLengthPrefixedString(serialisedLFE);
                }
            }
        }

        private static void LegacyFilesModifiedRead(NativeReader nr)
        {
            // -----------------------
            // Modified Legacy Files
            nr.ReadLengthPrefixedString(); // CFC 
            var count = nr.ReadInt(); // Count Added
            for (var iCount = 0; iCount < count; iCount++)
            {
                nr.ReadLengthPrefixedString();
            }
        }

        private static void ChunkFileCollectorAssetsRead(NativeReader nr)
        {
            foreach (var cam in AssetManager.Instance.CustomAssetManagers)
            {
                // CHUNK Count
                var count = nr.ReadInt();
                for (var i = 0; i < count; i++)
                {
                    // Item Name
                    var assetName = nr.ReadGuid();
                    // Item Data
                    var data = nr.ReadLengthPrefixedBytes();
                    AssetManager.Instance.ModifyChunk(assetName, data);
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
                        entry = AssetManager.Instance.GetEbxEntry(r.Name);
                        break;
                    case "ResResource":
                        entry = AssetManager.Instance.GetResEntry(r.Name);
                        break;
                    case "ChunkResource":
                        entry = AssetManager.Instance.GetChunkEntry(Guid.Parse(r.Name));
                        break;
                    default:
                        entry = null;
                        break;
                }

                if (entry != null)
                {
                    r.FillAssetEntry(entry);
                    if (entry is ChunkAssetEntry)
                    {
                    }
                    var d = reader.GetResourceData(r);
                    using (CasReader casReader = new CasReader(new MemoryStream(d)))
                    {
                        var d2 = casReader.Read();
                        AssetManager.Instance.ModifyEntry(entry, d2);
                        if (entry is ChunkAssetEntry)
                        {
                            if (r.IsLegacyFile)
                            {

                            }
                            if (entry.ModifiedEntry != null && !string.IsNullOrEmpty(entry.ModifiedEntry.UserData))
                            {

                            }
                        }
                    }
                    d = null;
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
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
