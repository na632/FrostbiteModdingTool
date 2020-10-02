using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FIFA21Plugin
{

	public class BundleInfo
    {
		public string Name { get; set; }
		public long Offset { get; set; }
		public long Size { get; set; }
    }

    public class TOCFile
    {
        public SBFile AssociatedSBFile { get; set; }

		public int[] ArrayOfInitialHeaderData = new int[12];

		public List<BundleInfo> Bundles = new List<BundleInfo>();


		public void Read(NativeReader nativeReader)
        {
			var magic = nativeReader.ReadInt(Endian.Big);
			if (magic != 0x3c)
				throw new Exception("Magic is not the expected value of 0x3c");

			nativeReader.Position = 556;


			int[] array = new int[12];
				for (int i = 0; i < 12; i++)
				{
					array[i] = nativeReader.ReadInt(Endian.Big);
				}
				List<int> list3 = new List<int>();
				if (array[0] != 0)
				{
					for (int j = 0; j < array[2]; j++)
					{
						list3.Add(nativeReader.ReadInt(Endian.Big));
					}
				nativeReader.Position = array[1] - 48;
					for (int k = 0; k < array[2]; k++)
					{
						int num2 = nativeReader.ReadInt(Endian.Big);
						uint num3 = nativeReader.ReadUInt(Endian.Big);
						long offset = nativeReader.ReadLong(Endian.Big);
						long position = nativeReader.Position;
					nativeReader.Position = array[8] - 48 + num2;
						string name2 = nativeReader.ReadNullTerminatedString();
					nativeReader.Position = position;
						BundleInfo item = new BundleInfo
						{
							Name = name2,
							Offset = offset,
							Size = num3
						};
					Bundles.Add(item);
					}
				}
				if (array[3] != 0)
				{
					nativeReader.Position = array[3] - 48;
					List<int> list4 = new List<int>();
					for (int l = 0; l < array[5]; l++)
					{
						list4.Add(nativeReader.ReadInt(Endian.Big));
					}
					nativeReader.Position = array[4] - 48;
					List<Guid> list5 = new List<Guid>();
					for (int m = 0; m < array[5]; m++)
					{
						byte[] array3 = nativeReader.ReadBytes(16);
						Guid value = new Guid(new byte[16]
						{
										array3[15],
										array3[14],
										array3[13],
										array3[12],
										array3[11],
										array3[10],
										array3[9],
										array3[8],
										array3[7],
										array3[6],
										array3[5],
										array3[4],
										array3[3],
										array3[2],
										array3[1],
										array3[0]
						});
						int num4 = nativeReader.ReadInt(Endian.Big) & 0xFFFFFF;
						while (list5.Count <= num4)
						{
							list5.Add(Guid.Empty);
						}
						list5[num4 / 3] = value;
					}
					nativeReader.Position = array[6] - 48;
				for (int n = 0; n < array[5]; n++)
				{
					nativeReader.ReadByte();
					bool patch = nativeReader.ReadBoolean();
					byte catalog = nativeReader.ReadByte();
					byte cas = nativeReader.ReadByte();
					uint num5 = nativeReader.ReadUInt(Endian.Big);
					uint num6 = nativeReader.ReadUInt(Endian.Big);
					ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
					chunkAssetEntry.Id = list5[n];
					chunkAssetEntry.Size = num6;
					chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
					chunkAssetEntry.ExtraData = new AssetExtraData();
					chunkAssetEntry.ExtraData.CasPath = AssetManager.Instance.fs.GetFilePath(catalog, cas, patch);
					chunkAssetEntry.ExtraData.DataOffset = num5;
					if (AssetManager.Instance.chunkList.ContainsKey(chunkAssetEntry.Id))
					{
						AssetManager.Instance.chunkList.Remove(chunkAssetEntry.Id);
					}
					AssetManager.Instance.chunkList.Add(chunkAssetEntry.Id, chunkAssetEntry);

				}
			}
		}
    }
}
