using FMT.FileTools;

using FrostySdk;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.Managers;
using System;

namespace Madden21Plugin.Cache
{
    public class CacheReader : ICacheReader
    {
        public ulong EbxDataOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ulong ResDataOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ulong ChunkDataOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ulong NameToPositionOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Read()
        {
            var fs = AssetManager.Instance.FileSystem;
            bool flag = false;
            //using (NativeReader nativeReader = new NativeReader(new FileStream(fs.CacheName + ".cache", FileMode.Open, FileAccess.Read)))
            using (NativeReader nativeReader = new NativeReader(AssetManager.CacheDecompress()))
            {
                if (nativeReader.ReadLengthPrefixedString() != ProfileManager.ProfileName)
                    return false;

                var cacheHead = nativeReader.ReadUInt();
                if (cacheHead != fs.Head)
                {
                    flag = true;
                }
                int count = nativeReader.ReadInt();
                if (ProfileManager.DataVersion == 20171117 || ProfileManager.DataVersion == 20180628)
                {
                    AssetManager.Instance.superBundles.Add(new SuperBundleEntry
                    {
                        Name = "<none>"
                    });
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        SuperBundleEntry superBundleEntry = new SuperBundleEntry();
                        superBundleEntry.Name = nativeReader.ReadNullTerminatedString();
                        AssetManager.Instance.superBundles.Add(superBundleEntry);
                    }
                }
                count = nativeReader.ReadInt();
                if (!ProfileManager.IsFIFA21DataVersion() && count == 0)
                {
                    return false;
                }
                for (int j = 0; j < count; j++)
                {
                    BundleEntry bundleEntry = new BundleEntry();
                    bundleEntry.Name = nativeReader.ReadNullTerminatedString();
                    bundleEntry.SuperBundleId = nativeReader.ReadInt();
                    if (!flag)
                    {
                        AssetManager.Instance.Bundles.Add(bundleEntry);
                    }
                }
                count = nativeReader.ReadInt();
                for (int k = 0; k < count; k++)
                {
                    EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
                    ebxAssetEntry.Name = nativeReader.ReadLengthPrefixedString();
                    ebxAssetEntry.Sha1 = nativeReader.ReadSha1();
                    ebxAssetEntry.BaseSha1 = AssetManager.Instance.GetBaseSha1(ebxAssetEntry.Sha1);
                    ebxAssetEntry.Size = nativeReader.ReadLong();
                    ebxAssetEntry.OriginalSize = nativeReader.ReadLong();
                    ebxAssetEntry.Location = (AssetDataLocation)nativeReader.ReadInt();
                    ebxAssetEntry.IsInline = nativeReader.ReadBoolean();
                    ebxAssetEntry.Type = nativeReader.ReadLengthPrefixedString();
                    Guid guid = nativeReader.ReadGuid();
                    if (nativeReader.ReadBoolean())
                    {
                        ebxAssetEntry.ExtraData = new AssetExtraData();
                        //ebxAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
                        //ebxAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
                        ebxAssetEntry.ExtraData.DataOffset = nativeReader.ReadUInt();
                        //ebxAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
                        ebxAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
                        ebxAssetEntry.ExtraData.CasPath = nativeReader.ReadLengthPrefixedString();
                    }
                    int num2 = nativeReader.ReadInt();
                    for (int l = 0; l < num2; l++)
                    {
                        ebxAssetEntry.Bundles.Add(nativeReader.ReadInt());
                    }
                    //num2 = nativeReader.ReadInt();
                    //for (int m = 0; m < num2; m++)
                    //{
                    //	ebxAssetEntry.DependentAssets.Add(nativeReader.ReadGuid());
                    //}
                    //if (nativeReader.ReadBoolean())
                    //	ebxAssetEntry.SBFileLocation = nativeReader.ReadLengthPrefixedString();
                    //if (nativeReader.ReadBoolean())
                    //	ebxAssetEntry.TOCFileLocation = nativeReader.ReadLengthPrefixedString();
                    //if (nativeReader.ReadBoolean())
                    //	ebxAssetEntry.CASFileLocation = nativeReader.ReadLengthPrefixedString();

                    //ebxAssetEntry.SB_CAS_Offset_Position = nativeReader.ReadInt();
                    //ebxAssetEntry.SB_CAS_Size_Position = nativeReader.ReadInt();
                    //ebxAssetEntry.SB_Sha1_Position = nativeReader.ReadInt();
                    //ebxAssetEntry.SB_OriginalSize_Position = nativeReader.ReadInt();
                    //ebxAssetEntry.ParentBundleOffset = nativeReader.ReadInt();
                    //ebxAssetEntry.ParentBundleSize = nativeReader.ReadInt();
                    //ebxAssetEntry.Bundle = nativeReader.ReadLengthPrefixedString();

                    //if (flag)
                    //{
                    //	ebxAssetEntry.Guid = guid;

                    //}
                    //else
                    //{
                    //	AssetManager.Instance.EBX.TryAdd(ebxAssetEntry.Name, ebxAssetEntry);
                    //}
                    AssetManager.Instance.AddEbx(ebxAssetEntry);

                }
                count = nativeReader.ReadInt();
                for (int n = 0; n < count; n++)
                {
                    ResAssetEntry resAssetEntry = new ResAssetEntry();
                    resAssetEntry.Name = nativeReader.ReadLengthPrefixedString();
                    resAssetEntry.Sha1 = nativeReader.ReadSha1();
                    resAssetEntry.BaseSha1 = AssetManager.Instance.GetBaseSha1(resAssetEntry.Sha1);
                    resAssetEntry.Size = nativeReader.ReadLong();
                    resAssetEntry.OriginalSize = nativeReader.ReadLong();
                    resAssetEntry.Location = (AssetDataLocation)nativeReader.ReadInt();
                    resAssetEntry.IsInline = nativeReader.ReadBoolean();
                    resAssetEntry.ResRid = nativeReader.ReadULong();
                    resAssetEntry.ResType = nativeReader.ReadUInt();
                    resAssetEntry.ResMeta = nativeReader.ReadBytes(nativeReader.ReadInt());
                    if (nativeReader.ReadBoolean())
                    {
                        resAssetEntry.ExtraData = new AssetExtraData();
                        //resAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
                        //resAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
                        resAssetEntry.ExtraData.DataOffset = nativeReader.ReadUInt();
                        //resAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
                        resAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
                        resAssetEntry.ExtraData.CasPath = nativeReader.ReadLengthPrefixedString();
                    }
                    if (nativeReader.ReadBoolean())
                        resAssetEntry.SBFileLocation = nativeReader.ReadLengthPrefixedString();
                    if (nativeReader.ReadBoolean())
                        resAssetEntry.TOCFileLocation = nativeReader.ReadLengthPrefixedString();
                    if (nativeReader.ReadBoolean())
                        resAssetEntry.CASFileLocation = nativeReader.ReadLengthPrefixedString();

                    resAssetEntry.SB_CAS_Offset_Position = nativeReader.ReadInt();
                    resAssetEntry.SB_CAS_Size_Position = nativeReader.ReadInt();
                    resAssetEntry.SB_Sha1_Position = nativeReader.ReadInt();
                    resAssetEntry.SB_OriginalSize_Position = nativeReader.ReadInt();
                    //resAssetEntry.ParentBundleOffset = nativeReader.ReadInt();
                    //resAssetEntry.ParentBundleSize = nativeReader.ReadInt();
                    //resAssetEntry.Bundle = nativeReader.ReadLengthPrefixedString();



                    int bundleCount = nativeReader.ReadInt();
                    for (int num4 = 0; num4 < bundleCount; num4++)
                    {
                        resAssetEntry.Bundles.Add(nativeReader.ReadInt());
                    }


                    AssetManager.Instance.RES.TryAdd(resAssetEntry.Name, resAssetEntry);
                    if (resAssetEntry.ResRid != 0L)
                    {
                        if (!AssetManager.Instance.resRidList.ContainsKey(resAssetEntry.ResRid))
                            AssetManager.Instance.resRidList.TryAdd(resAssetEntry.ResRid, resAssetEntry);
                    }

                }

                // ------------------------------------------------------------------------
                // Chunks
                count = nativeReader.ReadInt();
                for (int num5 = 0; num5 < count; num5++)
                {
                    ChunkAssetEntry chunkAssetEntry = ReadChunkFromCache(nativeReader);
                    AssetManager.Instance.AddChunk(chunkAssetEntry);
                }

                // ------------------------------------------------------------------------
                // Chunks in Bundles
                //count = nativeReader.ReadInt();
                //for (int num5 = 0; num5 < count; num5++)
                //{
                //	var bundle = nativeReader.ReadLengthPrefixedString();
                //	ChunkAssetEntry chunkAssetEntry = ReadChunkFromCache(nativeReader);
                //	AssetManager.Instance.SuperBundleChunks.TryAdd((chunkAssetEntry.Bundle, chunkAssetEntry.Id), chunkAssetEntry);
                //}
            }
            return !flag;
        }

        public EbxAssetEntry ReadEbxAssetEntry(NativeReader nativeReader)
        {
            throw new NotImplementedException();
        }

        private ChunkAssetEntry ReadChunkFromCache(NativeReader nativeReader)
        {
            ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
            chunkAssetEntry.Id = nativeReader.ReadGuid();
            chunkAssetEntry.Sha1 = nativeReader.ReadSha1();
            chunkAssetEntry.BaseSha1 = AssetManager.Instance.GetBaseSha1(chunkAssetEntry.Sha1);
            chunkAssetEntry.Size = nativeReader.ReadLong();
            chunkAssetEntry.Location = (AssetDataLocation)nativeReader.ReadInt();
            chunkAssetEntry.IsInline = nativeReader.ReadBoolean();
            chunkAssetEntry.BundledSize = nativeReader.ReadUInt();
            chunkAssetEntry.RangeStart = nativeReader.ReadUInt();
            chunkAssetEntry.RangeEnd = nativeReader.ReadUInt();
            chunkAssetEntry.LogicalOffset = nativeReader.ReadUInt();
            chunkAssetEntry.LogicalSize = nativeReader.ReadUInt();
            chunkAssetEntry.H32 = nativeReader.ReadInt();
            chunkAssetEntry.FirstMip = nativeReader.ReadInt();
            if (nativeReader.ReadBoolean())
            {
                chunkAssetEntry.ExtraData = new AssetExtraData();
                //chunkAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
                //chunkAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
                chunkAssetEntry.ExtraData.DataOffset = nativeReader.ReadUInt();
                //chunkAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
                chunkAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
                chunkAssetEntry.ExtraData.CasPath = nativeReader.ReadLengthPrefixedString();
            }
            else
            {
                throw new Exception("No Extra Data!");
            }
            if (nativeReader.ReadBoolean())
                chunkAssetEntry.SBFileLocation = nativeReader.ReadLengthPrefixedString();
            if (nativeReader.ReadBoolean())
                chunkAssetEntry.TOCFileLocation = nativeReader.ReadLengthPrefixedString();
            if (nativeReader.ReadBoolean())
                chunkAssetEntry.CASFileLocation = nativeReader.ReadLengthPrefixedString();




            chunkAssetEntry.SB_CAS_Offset_Position = nativeReader.ReadInt();
            chunkAssetEntry.SB_CAS_Size_Position = nativeReader.ReadInt();
            chunkAssetEntry.SB_Sha1_Position = nativeReader.ReadInt();
            chunkAssetEntry.SB_OriginalSize_Position = nativeReader.ReadInt();

            chunkAssetEntry.SB_LogicalOffset_Position = nativeReader.ReadUInt();
            chunkAssetEntry.SB_LogicalSize_Position = nativeReader.ReadUInt();
            //chunkAssetEntry.ParentBundleOffset = nativeReader.ReadInt();
            //chunkAssetEntry.ParentBundleSize = nativeReader.ReadInt();


            if (nativeReader.ReadBoolean())
                chunkAssetEntry.Bundle = nativeReader.ReadLengthPrefixedString();

            int num6 = nativeReader.ReadInt();
            for (int num7 = 0; num7 < num6; num7++)
            {
                chunkAssetEntry.Bundles.Add(nativeReader.ReadInt());
            }

            return chunkAssetEntry;
        }

    }
}
