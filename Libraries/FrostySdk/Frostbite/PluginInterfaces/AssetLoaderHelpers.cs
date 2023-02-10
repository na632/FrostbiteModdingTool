using FMT.FileTools;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FrostySdk.Frostbite.PluginInterfaces
{
    public class AssetLoaderHelpers
    {
        public static AssetEntry ConvertDbObjectToAssetEntry(DbObject item, AssetEntry assetEntry)
        {
            assetEntry.Sha1 = new FMT.FileTools.Sha1(item.GetValue<byte[]>("sha1"));

            assetEntry.BaseSha1 = AssetManager.Instance.GetBaseSha1(assetEntry.Sha1);
            assetEntry.Size = item.GetValue("size", 0L);
            assetEntry.OriginalSize = item.GetValue("originalSize", 0L);
            assetEntry.Location = AssetDataLocation.CasNonIndexed;
            assetEntry.ExtraData = new AssetExtraData();
            assetEntry.ExtraData.DataOffset = (uint)item.GetValue("offset", 0L);

            int cas = item.GetValue("cas", 0);
            int catalog = item.GetValue("catalog", 0);
            bool patch = item.GetValue("patch", false);
            assetEntry.ExtraData.Cas = (ushort)cas;
            assetEntry.ExtraData.Catalog = (ushort)catalog;
            assetEntry.ExtraData.IsPatch = patch;
            //chunkAssetEntry.ExtraData.CasPath = FileSystem.Instance.GetFilePath(catalog, cas, patch);

            assetEntry.Id = item.GetValue<Guid>("id", Guid.Empty);

            if (assetEntry is EbxAssetEntry || (item.HasValue("Type") && item.HasValue("name")))
            {
                ((EbxAssetEntry)assetEntry).Name = item.GetValue("name", string.Empty);
                ((EbxAssetEntry)assetEntry).Type = item.GetValue("Type", string.Empty);
            }
            else if (assetEntry is ResAssetEntry || item.HasValue("resRid"))
            {
                ((ResAssetEntry)assetEntry).Name = item.GetValue("name", string.Empty);
                ((ResAssetEntry)assetEntry).ResRid = item.GetValue<ulong>("resRid", 0ul);
                ((ResAssetEntry)assetEntry).ResType = item.GetValue<uint>("resType", 0);
                ((ResAssetEntry)assetEntry).ResMeta = item.GetValue<byte[]>("resMeta", null);
            }
            else if (assetEntry is ChunkAssetEntry || item.HasValue("logicalOffset"))
            {
                ((ChunkAssetEntry)assetEntry).LogicalOffset = item.GetValue<uint>("logicalOffset");
                ((ChunkAssetEntry)assetEntry).LogicalSize = item.GetValue<uint>("logicalSize");
            }
            //assetEntry.CASFileLocation = NativeFileLocation;
            //assetEntry.TOCFileLocation = AssociatedTOCFile.NativeFileLocation;

            assetEntry.SB_OriginalSize_Position = item.GetValue("SB_OriginalSize_Position", 0);
            assetEntry.SB_CAS_Offset_Position = item.GetValue("SB_CAS_Offset_Position", 0);
            assetEntry.SB_CAS_Size_Position = item.GetValue("SB_CAS_Size_Position", 0);
            assetEntry.SB_Sha1_Position = item.GetValue("SB_Sha1_Position", 0);

            if(item.HasValue("SBFileLocation"))
                assetEntry.SBFileLocation = item.GetValue<string>("SBFileLocation");

            if (item.HasValue("TOCFileLocation"))
                assetEntry.TOCFileLocation = item.GetValue<string>("TOCFileLocation");

            if (item.HasValue("Bundles"))
            {
                var bundles = item.GetValue<List<string>>("Bundles");
                foreach (var bundle in bundles)
                {
                    assetEntry.AddToBundle(Fnv1a.HashString(bundle));
                }
            }
            if (item.HasValue("Bundle"))
            {
                assetEntry.Bundle = item.GetValue<string>("Bundle");
                assetEntry.AddToBundle(Fnv1a.HashString(assetEntry.Bundle));
                assetEntry.Bundles.Add(Fnv1a.HashString(assetEntry.Bundle));
            }
            return assetEntry;
        }
    }
}
