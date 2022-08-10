using FrostySdk;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA21Plugin
{
    public class DboToAssetProcessor
    {
        public void ProcessObjectLists(DbObject list)
        {

        }

        public void ProcessEbxList(DbObject list)
        {
            foreach (DbObject item in list)
            {
                ProcessEbx(item);
            }
        }

        public void ProcessEbx(DbObject item)
        {
            EbxAssetEntry ebxAssetEntry = null;

            var name = item.GetValue<string>("name");
            if (AssetManager.Instance.EBX.ContainsKey(name))
                //ebxAssetEntry = AssetManager.Instance.EBX[name];
                AssetManager.Instance.EBX.TryRemove(name, out EbxAssetEntry _);
            //else

                ebxAssetEntry = new EbxAssetEntry();

            ebxAssetEntry.Name = item.GetValue<string>("name");
            ebxAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
            ebxAssetEntry.BaseSha1 = AssetManager.Instance.rm.GetBaseSha1(ebxAssetEntry.Sha1);
            ebxAssetEntry.Size = item.GetValue("size", 0L);
            ebxAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
            ebxAssetEntry.Location = AssetDataLocation.CasNonIndexed;
            ebxAssetEntry.ExtraData = new AssetExtraData();
            ebxAssetEntry.ExtraData.DataOffset = (uint)item.GetValue("offset", 0L);

            int cas = item.GetValue("cas", 0);
            int catalog = item.GetValue("catalog", 0);
            bool patch = item.GetValue("patch", false);
            ebxAssetEntry.ExtraData.CasPath = FileSystem.Instance.GetFilePath(catalog, cas, patch);

            ebxAssetEntry.TOCFileLocation = item.GetValue("TOCFileLocation", string.Empty);
            ebxAssetEntry.SBFileLocation = item.GetValue("SBFileLocation", string.Empty);
            ebxAssetEntry.SB_OriginalSize_Position = item.GetValue("SB_OriginalSize_Position", 0);
            ebxAssetEntry.SB_CAS_Offset_Position = item.GetValue("SB_CAS_Offset_Position", 0);
            ebxAssetEntry.SB_CAS_Size_Position = item.GetValue("SB_CAS_Size_Position", 0);
            ebxAssetEntry.SB_Sha1_Position = item.GetValue("SB_Sha1_Position", 0);

            if(item.HasValue("Type"))
                ebxAssetEntry.Type = item.GetValue("Type", string.Empty);

            var bundleId = AssetManager.Instance.bundles.Count - 1;
            ebxAssetEntry.Bundle = AssetManager.Instance.bundles[bundleId].Name;
            ebxAssetEntry.Bundles.Add(bundleId);

            AssetManager.Instance.AddEbx(ebxAssetEntry);
        }

        public void ProcessResList(DbObject list)
        {
            foreach (DbObject item in list)
            {
                ProcessRes(item);
            }
        }

        public void ProcessRes(DbObject item)
        {
            ResAssetEntry resAssetEntry = null;

            var name = item.GetValue<string>("name");
            if (AssetManager.Instance.RES.ContainsKey(name))
                resAssetEntry = AssetManager.Instance.RES[name];
            else
                resAssetEntry = new ResAssetEntry();

            resAssetEntry.Name = item.GetValue<string>("name");
            resAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
            resAssetEntry.BaseSha1 = AssetManager.Instance.rm.GetBaseSha1(resAssetEntry.Sha1);
            resAssetEntry.Size = item.GetValue("size", 0L);
            resAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
            resAssetEntry.Location = AssetDataLocation.CasNonIndexed;
            resAssetEntry.ExtraData = new AssetExtraData();
            resAssetEntry.ExtraData.DataOffset = (uint)item.GetValue("offset", 0L);

            int cas = item.GetValue("cas", 0);
            int catalog = item.GetValue("catalog", 0);
            bool patch = item.GetValue("patch", false);
            resAssetEntry.ExtraData.CasPath = FileSystem.Instance.GetFilePath(catalog, cas, patch);

            resAssetEntry.ResRid = item.GetValue<ulong>("resRid", 0ul);
            resAssetEntry.ResType = item.GetValue<uint>("resType", 0);
            resAssetEntry.ResMeta = item.GetValue<byte[]>("resMeta", null);

            resAssetEntry.CASFileLocation = item.GetValue("CASFileLocation", string.Empty);
            resAssetEntry.TOCFileLocation = item.GetValue("TOCFileLocation", string.Empty);
            resAssetEntry.SBFileLocation = item.GetValue("SBFileLocation", string.Empty);
            resAssetEntry.SB_OriginalSize_Position = item.GetValue("SB_OriginalSize_Position", 0);
            resAssetEntry.SB_CAS_Offset_Position = item.GetValue("SB_CAS_Offset_Position", 0);
            resAssetEntry.SB_CAS_Size_Position = item.GetValue("SB_CAS_Size_Position", 0);
            resAssetEntry.SB_Sha1_Position = item.GetValue("SB_Sha1_Position", 0);

            var bundleId = AssetManager.Instance.bundles.Count - 1;
            resAssetEntry.Bundle = AssetManager.Instance.bundles[bundleId].Name;
            resAssetEntry.Bundles.Add(bundleId);

             AssetManager.Instance.AddRes(resAssetEntry);
        }

        public void ProcessChunkList(DbObject list)
        {

        }

        public void ProcessChunk(DbObject item)
        {

            ChunkAssetEntry chunkAssetEntry = null;

            var name = item.GetValue<Guid>("id");
            if (AssetManager.Instance.Chunks.ContainsKey(name))
                chunkAssetEntry = AssetManager.Instance.Chunks[name];
            else
                chunkAssetEntry = new ChunkAssetEntry();

            chunkAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");

            chunkAssetEntry.BaseSha1 = ResourceManager.Instance.GetBaseSha1(chunkAssetEntry.Sha1);
            chunkAssetEntry.Size = item.GetValue("size", 0L);
            chunkAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
            chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
            chunkAssetEntry.ExtraData = new AssetExtraData();
            chunkAssetEntry.ExtraData.DataOffset = (uint)item.GetValue("offset", 0L);

            int cas = item.GetValue("cas", 0);
            int catalog = item.GetValue("catalog", 0);
            bool patch = item.GetValue("patch", false);
            chunkAssetEntry.ExtraData.CasPath = FileSystem.Instance.GetFilePath(catalog, cas, patch);

            chunkAssetEntry.Id = item.GetValue<Guid>("id");
            chunkAssetEntry.LogicalOffset = item.GetValue<uint>("logicalOffset");
            chunkAssetEntry.LogicalSize = item.GetValue<uint>("logicalSize");

            chunkAssetEntry.CASFileLocation = item.GetValue("CASFileLocation", string.Empty);
            chunkAssetEntry.TOCFileLocation = item.GetValue("TOCFileLocation", string.Empty);

            chunkAssetEntry.SB_OriginalSize_Position = item.GetValue("SB_OriginalSize_Position", 0);
            chunkAssetEntry.SB_CAS_Offset_Position = item.GetValue("SB_CAS_Offset_Position", 0);
            chunkAssetEntry.SB_CAS_Size_Position = item.GetValue("SB_CAS_Size_Position", 0);
            chunkAssetEntry.SB_Sha1_Position = item.GetValue("SB_Sha1_Position", 0);

            var bundleId = AssetManager.Instance.bundles.Count - 1;
            chunkAssetEntry.Bundle = AssetManager.Instance.bundles[bundleId].Name;
            chunkAssetEntry.Bundles.Add(bundleId);

            AssetManager.Instance.AddChunk(chunkAssetEntry);

        }
    }
}
