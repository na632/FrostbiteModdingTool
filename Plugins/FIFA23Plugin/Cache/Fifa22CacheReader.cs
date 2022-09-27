using Frosty.Hash;
using FrostySdk;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FIFA23Plugin.Cache
{
    public class Fifa22CacheReader : ICacheReader
    {
        public bool Read()
        {
			var fs = AssetManager.Instance.fs;
			bool flag = false;
			//using (NativeReader nativeReader = new NativeReader(new FileStream(fs.CacheName + ".cache", FileMode.Open, FileAccess.Read)))
			using (NativeReader nativeReader = new NativeReader(AssetManager.CacheDecompress()))
			{
				if (nativeReader.ReadLengthPrefixedString() != ProfilesLibrary.ProfileName)
					return false;

				var cacheHead = nativeReader.ReadULong();
				if (cacheHead != fs.SystemIteration)
				{
					flag = true;
				}
				int count = nativeReader.ReadInt();
				if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
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
				//if (!ProfilesLibrary.IsFIFA21DataVersion() && count == 0)
				//{
				//	return false;
				//}
				for (int j = 0; j < count; j++)
				{
					BundleEntry bundleEntry = new BundleEntry();
					bundleEntry.Name = nativeReader.ReadNullTerminatedString();
					bundleEntry.SuperBundleId = nativeReader.ReadInt();
					if (!flag)
					{
						AssetManager.Instance.bundles.Add(bundleEntry);
					}
				}
				count = nativeReader.ReadInt();
				for (int k = 0; k < count; k++)
				{
					EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
					ebxAssetEntry.Name = nativeReader.ReadLengthPrefixedString();
					ebxAssetEntry.Sha1 = nativeReader.ReadSha1();
					ebxAssetEntry.BaseSha1 = AssetManager.Instance.rm.GetBaseSha1(ebxAssetEntry.Sha1);
					ebxAssetEntry.Size = nativeReader.ReadLong();
					ebxAssetEntry.OriginalSize = nativeReader.ReadLong();
					ebxAssetEntry.Location = (AssetDataLocation)nativeReader.ReadInt();
					ebxAssetEntry.IsInline = nativeReader.ReadBoolean();
					ebxAssetEntry.Type = nativeReader.ReadLengthPrefixedString();
					Guid guid = nativeReader.ReadGuid();
					if (nativeReader.ReadBoolean())
					{
						ebxAssetEntry.ExtraData = new AssetExtraData();
						ebxAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
						ebxAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
						ebxAssetEntry.ExtraData.DataOffset = nativeReader.ReadUInt();
						ebxAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
						ebxAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
						ebxAssetEntry.ExtraData.CasPath = nativeReader.ReadLengthPrefixedString();
					}

					var numTFL = nativeReader.ReadInt();
					for (int iTFL = 0; iTFL < numTFL; iTFL++)
					{
						ebxAssetEntry.TOCFileLocations.Add(nativeReader.ReadLengthPrefixedString());
					}

					if (nativeReader.ReadBoolean())
                        ebxAssetEntry.TOCFileLocation = nativeReader.ReadLengthPrefixedString();
                    if (nativeReader.ReadBoolean())
                        ebxAssetEntry.CASFileLocation = nativeReader.ReadLengthPrefixedString();

					ebxAssetEntry.SB_CAS_Offset_Position = nativeReader.ReadInt();
					ebxAssetEntry.SB_CAS_Size_Position = nativeReader.ReadInt();
					ebxAssetEntry.SB_Sha1_Position = nativeReader.ReadInt();
					ebxAssetEntry.SB_OriginalSize_Position = nativeReader.ReadInt();


					int num2 = nativeReader.ReadInt();
					for (int l = 0; l < num2; l++)
					{
						ebxAssetEntry.Bundles.Add(nativeReader.ReadInt());
					}
					num2 = nativeReader.ReadInt();
					for (int m = 0; m < num2; m++)
					{
						ebxAssetEntry.DependentAssets.Add(nativeReader.ReadGuid());
					}

                    if (flag)
					{
						ebxAssetEntry.Guid = guid;

					}
					else
					{
						AssetManager.Instance.EBX.TryAdd(ebxAssetEntry.Name, ebxAssetEntry);
					}
				}
				count = nativeReader.ReadInt();
				for (int n = 0; n < count; n++)
				{
					ResAssetEntry resAssetEntry = new ResAssetEntry();
					resAssetEntry.Name = nativeReader.ReadLengthPrefixedString();
					resAssetEntry.Sha1 = nativeReader.ReadSha1();
					resAssetEntry.BaseSha1 = AssetManager.Instance.rm.GetBaseSha1(resAssetEntry.Sha1);
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
						resAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
						resAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
						resAssetEntry.ExtraData.DataOffset = nativeReader.ReadUInt();
						resAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
						resAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
						resAssetEntry.ExtraData.CasPath = nativeReader.ReadLengthPrefixedString();
					}

					var numTFL = nativeReader.ReadInt();
					//resAssetEntry.TOCFileLocations = new HashSet<string>();
					for (int iTFL = 0; iTFL < numTFL; iTFL++)
					{
						resAssetEntry.TOCFileLocations.Add(nativeReader.ReadLengthPrefixedString());
					}

					if (nativeReader.ReadBoolean())
						resAssetEntry.TOCFileLocation = nativeReader.ReadLengthPrefixedString();
					if (nativeReader.ReadBoolean())
						resAssetEntry.CASFileLocation = nativeReader.ReadLengthPrefixedString();

					resAssetEntry.SB_CAS_Offset_Position = nativeReader.ReadInt();
					resAssetEntry.SB_CAS_Size_Position = nativeReader.ReadInt();
					resAssetEntry.SB_Sha1_Position = nativeReader.ReadInt();
					resAssetEntry.SB_OriginalSize_Position = nativeReader.ReadInt();

					int bundleCount = nativeReader.ReadInt();
					for (int num4 = 0; num4 < bundleCount; num4++)
					{
						resAssetEntry.Bundles.Add(nativeReader.ReadInt());
					}

					AssetManager.Instance.RES.Add(resAssetEntry.Name, resAssetEntry);
					if (resAssetEntry.ResRid != 0L)
					{
						if (!AssetManager.Instance.resRidList.ContainsKey(resAssetEntry.ResRid))
							AssetManager.Instance.resRidList.Add(resAssetEntry.ResRid, resAssetEntry);
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
				count = nativeReader.ReadInt();
				for (int num5 = 0; num5 < count; num5++)
				{
					var bundle = nativeReader.ReadLengthPrefixedString();
					ChunkAssetEntry chunkAssetEntry = ReadChunkFromCache(nativeReader);
					AssetManager.Instance.BundleChunks.TryAdd((chunkAssetEntry.Bundle, chunkAssetEntry.Id), chunkAssetEntry);
				}
			}
			return !flag;
		}

		private ChunkAssetEntry ReadChunkFromCache(NativeReader nativeReader)
		{
			ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
			chunkAssetEntry.Id = nativeReader.ReadGuid();
			chunkAssetEntry.Sha1 = nativeReader.ReadSha1();
			chunkAssetEntry.BaseSha1 = AssetManager.Instance.rm.GetBaseSha1(chunkAssetEntry.Sha1);
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
				chunkAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
				chunkAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
				chunkAssetEntry.ExtraData.DataOffset = nativeReader.ReadUInt();
				chunkAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
				chunkAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
				chunkAssetEntry.ExtraData.CasPath = nativeReader.ReadLengthPrefixedString();
			}
			else
			{
				throw new Exception("No Extra Data!");
			}

			var numTFL = nativeReader.ReadInt();
			//chunkAssetEntry.TOCFileLocations = new HashSet<string>();
			for (int iTFL = 0; iTFL < numTFL; iTFL++)
			{
				chunkAssetEntry.TOCFileLocations.Add(nativeReader.ReadLengthPrefixedString());
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
