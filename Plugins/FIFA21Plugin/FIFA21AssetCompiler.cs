﻿using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using paulv2k4ModdingExecuter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FIFA21Plugin
{

    /// <summary>
    /// Currently. The Madden 21 Compiler does not work in game.
    /// </summary>
    public class FIFA21AssetCompiler : IAssetCompiler
    {
        public const string ModDirectory = "ModData";
        public const string PatchDirectory = "Patch";

        /// <summary>
        /// This is run AFTER the compilation of the fbmod into resource files ready for the Actions to TOC/SB/CAS to be taken
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="logger"></param>
        /// <param name="frostyModExecuter">Frosty Mod Executer object</param>
        /// <returns></returns>
        public bool Compile(FileSystem fs, ILogger logger, object frostyModExecuter)
        {

            // ------------------------------------------------------------------------------------------
            // You will need to change this to ProfilesLibrary.DataVersion if you change the Profile.json DataVersion field
            if (ProfilesLibrary.IsMaddenDataVersion())
            {
                DbObject layoutToc = null;

                // Read the original Layout TOC into a DB Object
                using (DbReader dbReaderOfLayoutTOC = new DbReader(new FileStream(fs.BasePath + PatchDirectory + "/layout.toc", FileMode.Open, FileAccess.Read), fs.CreateDeobfuscator()))
                {
                    layoutToc = dbReaderOfLayoutTOC.ReadDbObject();
                }

                // Notify the Bundle Action of the Cas File Count
                FIFA21BundleAction.CasFileCount = fs.CasFileCount;
                List<FIFA21BundleAction> madden21BundleActions = new List<FIFA21BundleAction>();

                var numberOfCatalogs = fs.Catalogs.Count();
                var numberOfCatalogsCompleted = 0;

                // --------------------------------------------------------------------------------------
                // Run a check against all changes and build your new TOC/SB/CAS files
                foreach (CatalogInfo catalogItem in fs.EnumerateCatalogInfos())
                {
                    FIFA21BundleAction maddenBundleAction = new FIFA21BundleAction(catalogItem, (FrostyModExecutor)frostyModExecuter);
                    maddenBundleAction.Run();
                    numberOfCatalogsCompleted++;
                    logger.Log($"Compiling Mod Progress: { Math.Round((double)numberOfCatalogsCompleted / numberOfCatalogs, 2) * 100} %");

                    madden21BundleActions.Add(maddenBundleAction);
                }
                //
                // --------------------------------------------------------------------------------------


                // --------------------------------------------------------------------------------------
                // From the new bundles that have been created that has generated new CAS files, add these new CAS files to the Layout TOC
                foreach (FIFA21BundleAction bundleAction in madden21BundleActions)
                {
                    if (bundleAction.HasErrored)
                    {
                        throw bundleAction.Exception;
                    }
                    if (bundleAction.CasFiles.Count > 0)
                    {
                        var installManifest = layoutToc.GetValue<DbObject>("installManifest");
                        var installChunks = installManifest.GetValue<DbObject>("installChunks");
                        foreach (DbObject installChunk in installChunks)
                        {
                            if (bundleAction.CatalogInfo.Name.Equals("win32/" + installChunk.GetValue<string>("name")))
                            {
                                foreach (int key in bundleAction.CasFiles.Keys)
                                {
                                    DbObject newFile = DbObject.CreateObject();
                                    newFile.SetValue("id", key);
                                    newFile.SetValue("path", bundleAction.CasFiles[key]);

                                    var installChunkFiles = installChunk.GetValue<DbObject>("files");
                                    installChunkFiles.Add(newFile);


                                }
                                break;
                            }
                        }
                    }
                }


                // --------------------------------------------------------------------------------------
                // Write a new Layout file
                logger.Log("Writing new Layout file to Game");
                using (DbWriter dbWriter = new DbWriter(new FileStream(ModDirectory + PatchDirectory + "/layout.toc", FileMode.Create), inWriteHeader: true))
                {
                    dbWriter.Write(layoutToc);
                }
                // --------------------------------------------------------------------------------------

                return true;
            }
            return false;
        }


    }
}
