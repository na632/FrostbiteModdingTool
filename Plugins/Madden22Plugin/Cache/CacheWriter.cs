using FrostySdk;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Madden22Plugin.Cache
{
    public class CacheWriter : ICacheWriter
    {
        public void Write()
        {
            MemoryStream msCache = new MemoryStream();
            FileSystem fs = AssetManager.Instance.fs;
            //using (NativeWriter nativeWriter = new NativeWriter(new FileStream(fs.CacheName + ".cache", FileMode.Create)))
            using (NativeWriter nativeWriter = new NativeWriter(msCache, leaveOpen: true))
            {
                nativeWriter.WriteLengthPrefixedString(ProfilesLibrary.ProfileName);
                nativeWriter.Write(fs.SystemIteration);
                nativeWriter.Write(AssetManager.Instance.superBundles.Count);
                foreach (SuperBundleEntry superBundle in AssetManager.Instance.superBundles)
                {
                    nativeWriter.WriteNullTerminatedString(superBundle.Name);
                }
                nativeWriter.Write(AssetManager.Instance.bundles.Count);
                foreach (BundleEntry bundle in AssetManager.Instance.bundles)
                {
                    nativeWriter.WriteNullTerminatedString(bundle.Name);
                    nativeWriter.Write(bundle.SuperBundleId);
                }
                nativeWriter.Write(AssetManager.Instance.EBX.Values.Count);
                foreach (EbxAssetEntry ebxEntry in AssetManager.Instance.EBX.Values)
                {
                    nativeWriter.WriteLengthPrefixedString(ebxEntry.Name);
                    nativeWriter.Write(ebxEntry.Sha1);
                    nativeWriter.Write(ebxEntry.Size);
                    nativeWriter.Write(ebxEntry.OriginalSize);
                    nativeWriter.Write((int)ebxEntry.Location);
                    nativeWriter.Write(ebxEntry.IsInline);
                    nativeWriter.WriteLengthPrefixedString((ebxEntry.Type != null) ? ebxEntry.Type : "");
                    nativeWriter.Write(ebxEntry.Guid == Guid.Empty ? Guid.NewGuid() : ebxEntry.Guid);
                    nativeWriter.Write(ebxEntry.ExtraData != null);
                    if (ebxEntry.ExtraData != null)
                    {
                        nativeWriter.Write(ebxEntry.ExtraData.BaseSha1);
                        nativeWriter.Write(ebxEntry.ExtraData.DeltaSha1);
                        nativeWriter.Write(ebxEntry.ExtraData.DataOffset);
                        nativeWriter.Write(ebxEntry.ExtraData.SuperBundleId);
                        nativeWriter.Write(ebxEntry.ExtraData.IsPatch);
                        nativeWriter.WriteLengthPrefixedString(ebxEntry.ExtraData.CasPath);
                    }
                    nativeWriter.Write(ebxEntry.Bundles.Count);
                    foreach (int bundle2 in ebxEntry.Bundles)
                    {
                        nativeWriter.Write(bundle2);
                    }
                    nativeWriter.Write(ebxEntry.DependentAssets.Count);
                    foreach (Guid item in ebxEntry.EnumerateDependencies())
                    {
                        nativeWriter.Write(item);
                    }

                    //nativeWriter.Write(!string.IsNullOrEmpty(ebxEntry.SBFileLocation));
                    //if (!string.IsNullOrEmpty(ebxEntry.SBFileLocation))
                    //    nativeWriter.WriteLengthPrefixedString(ebxEntry.SBFileLocation);
                    //nativeWriter.Write(!string.IsNullOrEmpty(ebxEntry.TOCFileLocation));
                    //if (!string.IsNullOrEmpty(ebxEntry.TOCFileLocation))
                    //    nativeWriter.WriteLengthPrefixedString(ebxEntry.TOCFileLocation);

                    //nativeWriter.Write(!string.IsNullOrEmpty(ebxEntry.CASFileLocation));
                    //if (!string.IsNullOrEmpty(ebxEntry.CASFileLocation))
                    //    nativeWriter.WriteLengthPrefixedString(ebxEntry.CASFileLocation);

                    //nativeWriter.Write(ebxEntry.SB_CAS_Offset_Position);
                    //nativeWriter.Write(ebxEntry.SB_CAS_Size_Position);
                    //nativeWriter.Write(ebxEntry.SB_Sha1_Position);
                    //nativeWriter.Write(ebxEntry.SB_OriginalSize_Position);
                    //nativeWriter.Write(ebxEntry.ParentBundleOffset);
                    //nativeWriter.Write(ebxEntry.ParentBundleSize);
                    //nativeWriter.WriteLengthPrefixedString(ebxEntry.Bundle);


                }
                nativeWriter.Write(AssetManager.Instance.RES.Values.Count);
                foreach (ResAssetEntry resEntry in AssetManager.Instance.RES.Values)
                {

                    nativeWriter.WriteLengthPrefixedString(resEntry.Name);
                    nativeWriter.Write(resEntry.Sha1);
                    nativeWriter.Write(resEntry.Size);
                    nativeWriter.Write(resEntry.OriginalSize);
                    nativeWriter.Write((int)resEntry.Location);
                    nativeWriter.Write(resEntry.IsInline);
                    nativeWriter.Write(resEntry.ResRid);
                    nativeWriter.Write(resEntry.ResType);
                    nativeWriter.Write(resEntry.ResMeta.Length);
                    nativeWriter.Write(resEntry.ResMeta);
                    nativeWriter.Write(resEntry.ExtraData != null);
                    if (resEntry.ExtraData != null)
                    {
                        nativeWriter.Write(resEntry.ExtraData.BaseSha1);
                        nativeWriter.Write(resEntry.ExtraData.DeltaSha1);
                        nativeWriter.Write(resEntry.ExtraData.DataOffset);
                        nativeWriter.Write(resEntry.ExtraData.SuperBundleId);
                        nativeWriter.Write(resEntry.ExtraData.IsPatch);
                        nativeWriter.WriteLengthPrefixedString(resEntry.ExtraData.CasPath);
                    }
                    nativeWriter.Write(!string.IsNullOrEmpty(resEntry.SBFileLocation));
                    if (!string.IsNullOrEmpty(resEntry.SBFileLocation))
                        nativeWriter.WriteLengthPrefixedString(resEntry.SBFileLocation);
                    nativeWriter.Write(!string.IsNullOrEmpty(resEntry.TOCFileLocation));
                    if (!string.IsNullOrEmpty(resEntry.TOCFileLocation))
                        nativeWriter.WriteLengthPrefixedString(resEntry.TOCFileLocation);

                    nativeWriter.Write(!string.IsNullOrEmpty(resEntry.CASFileLocation));
                    if (!string.IsNullOrEmpty(resEntry.CASFileLocation))
                        nativeWriter.WriteLengthPrefixedString(resEntry.CASFileLocation);

                    nativeWriter.Write(resEntry.SB_CAS_Offset_Position);
                    nativeWriter.Write(resEntry.SB_CAS_Size_Position);
                    nativeWriter.Write(resEntry.SB_Sha1_Position);
                    nativeWriter.Write(resEntry.SB_OriginalSize_Position);
                    nativeWriter.Write(resEntry.ParentBundleOffset);
                    nativeWriter.Write(resEntry.ParentBundleSize);
                    //nativeWriter.WriteLengthPrefixedString(resEntry.Bundle);

                    nativeWriter.Write(resEntry.Bundles.Count);
                    foreach (int bundle3 in resEntry.Bundles)
                    {
                        nativeWriter.Write(bundle3);
                    }


                }

                nativeWriter.Write(AssetManager.Instance.Chunks.Count);
                foreach (ChunkAssetEntry chunkEntry in AssetManager.Instance.Chunks.Values)
                {
                    WriteChunkEntry(nativeWriter, chunkEntry);
                }

                nativeWriter.Write(AssetManager.Instance.BundleChunks.Count);
                foreach (ChunkAssetEntry chunkEntry in AssetManager.Instance.BundleChunks.Values)
                {
                    nativeWriter.WriteLengthPrefixedString(chunkEntry.Bundle);
                    WriteChunkEntry(nativeWriter, chunkEntry);
                }

            }

            AssetManager.CacheCompress(msCache);
        }

        private void WriteChunkEntry(NativeWriter nativeWriter, ChunkAssetEntry chunkEntry)
        {
            nativeWriter.Write(chunkEntry.Id);
            nativeWriter.Write(chunkEntry.Sha1);
            nativeWriter.Write(chunkEntry.Size);
            nativeWriter.Write((int)chunkEntry.Location);
            nativeWriter.Write(chunkEntry.IsInline);
            nativeWriter.Write(chunkEntry.BundledSize);
            nativeWriter.Write(chunkEntry.RangeStart);
            nativeWriter.Write(chunkEntry.RangeEnd);
            nativeWriter.Write(chunkEntry.LogicalOffset);
            nativeWriter.Write(chunkEntry.LogicalSize);
            nativeWriter.Write(chunkEntry.H32);
            nativeWriter.Write(chunkEntry.FirstMip);
            nativeWriter.Write(chunkEntry.ExtraData != null);
            if (chunkEntry.ExtraData != null)
            {
                nativeWriter.Write(chunkEntry.ExtraData.BaseSha1);
                nativeWriter.Write(chunkEntry.ExtraData.DeltaSha1);
                nativeWriter.Write(chunkEntry.ExtraData.DataOffset);
                nativeWriter.Write(chunkEntry.ExtraData.SuperBundleId);
                nativeWriter.Write(chunkEntry.ExtraData.IsPatch);
                nativeWriter.WriteLengthPrefixedString(chunkEntry.ExtraData.CasPath);
            }
            else
            {
                throw new Exception("No Extra Data!");
            }
            nativeWriter.Write(!string.IsNullOrEmpty(chunkEntry.SBFileLocation));
            if (!string.IsNullOrEmpty(chunkEntry.SBFileLocation))
                nativeWriter.WriteLengthPrefixedString(chunkEntry.SBFileLocation);
            nativeWriter.Write(!string.IsNullOrEmpty(chunkEntry.TOCFileLocation));
            if (!string.IsNullOrEmpty(chunkEntry.TOCFileLocation))
                nativeWriter.WriteLengthPrefixedString(chunkEntry.TOCFileLocation);

            nativeWriter.Write(!string.IsNullOrEmpty(chunkEntry.CASFileLocation));
            if (!string.IsNullOrEmpty(chunkEntry.CASFileLocation))
                nativeWriter.WriteLengthPrefixedString(chunkEntry.CASFileLocation);

            nativeWriter.Write(chunkEntry.SB_CAS_Offset_Position);
            nativeWriter.Write(chunkEntry.SB_CAS_Size_Position);
            nativeWriter.Write(chunkEntry.SB_Sha1_Position);
            nativeWriter.Write(chunkEntry.SB_OriginalSize_Position);

            nativeWriter.Write(chunkEntry.SB_LogicalOffset_Position);
            nativeWriter.Write(chunkEntry.SB_LogicalSize_Position);
            nativeWriter.Write(chunkEntry.ParentBundleOffset);
            nativeWriter.Write(chunkEntry.ParentBundleSize);

            nativeWriter.Write((!string.IsNullOrEmpty(chunkEntry.Bundle)));
            if (!string.IsNullOrEmpty(chunkEntry.Bundle))
                nativeWriter.WriteLengthPrefixedString(chunkEntry.Bundle);

            if(chunkEntry.Id.ToString() == "e35237c0-ebbe-bc31-1504-aaf3eb947c9b")
            {

            }
            nativeWriter.Write(chunkEntry.Bundles.Count);
            foreach (int bundleId in chunkEntry.Bundles)
            {
                nativeWriter.Write(bundleId);
            }
        }
    }
}
