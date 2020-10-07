using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using static FIFA21Plugin.FIFA21AssetLoader;

namespace FIFA21Plugin
{

	public class BundleEntryInfo
    {
		/// <summary>
		/// Is it a EBX, RES or Chunk
		/// </summary>
		public string Type { get; set; }
		public Guid? ChunkGuid { get; set; }
		public Sha1? Sha { get; set; }
		public string Name { get; set; }
		public long Offset { get; set; }
		public long Size { get; set; }
		public int Flag { get; set; }
		public long StringOffset { get; set; }
		public int Index { get; set; }
        public int CasIndex { get; internal set; }
        public int Offset2 { get; internal set; }
        public int OriginalSize { get; internal set; }

        public override string ToString()
        {
			var builtString = string.Empty;

			if (!string.IsNullOrEmpty(Type))
			{
				builtString += Type;
			}

			if (!string.IsNullOrEmpty(Name))
            {
				builtString += " " + Name;
            }
			
			if (Sha.HasValue)
			{
				builtString += " " + Sha.Value.ToString();
			}


			if (!string.IsNullOrEmpty(builtString))
			{
				builtString = base.ToString();
			}

			return builtString;


		}
    }

    public class TOCFile
    {
        public SBFile AssociatedSBFile { get; set; }

		//public int[] ArrayOfInitialHeaderData = new int[12];

		public ContainerMetaData MetaData = new ContainerMetaData();
		public List<BaseBundleInfo> Bundles = new List<BaseBundleInfo>();

		private TocSbReader_FIFA21 ParentReader;
		public TOCFile(TocSbReader_FIFA21 parent)
        {
			ParentReader = parent;
        }

		public class ContainerMetaData
        {

			public int MetaDataLength { get; set; }
			public int ItemCount { get; set; }
			public int Offset1 { get; set; }
			public int Offset2 { get; set; }
			public int ResCount { get; set; }
			public int Offset4 { get; set; }
			public int Offset5 { get; set; }
			public int Offset6 { get; set; }
			public int Offset7 { get; set; }
			public int Sec4Size { get; set; }

			public void Read(NativeReader nativeReader)
			{
				MetaDataLength = nativeReader.ReadInt(Endian.Big);
				ItemCount = nativeReader.ReadInt(Endian.Big);
				Offset1 = nativeReader.ReadInt(Endian.Big);
				Offset2 = nativeReader.ReadInt(Endian.Big);
				ResCount = nativeReader.ReadInt(Endian.Big);
				Offset4 = nativeReader.ReadInt(Endian.Big);
				Offset5 = nativeReader.ReadInt(Endian.Big);
				Offset6 = nativeReader.ReadInt(Endian.Big);
				Offset7 = nativeReader.ReadInt(Endian.Big);
				Sec4Size = nativeReader.ReadInt(Endian.Big);

			}

        }
		public void Read(NativeReader nativeReader)
		{
			var startPosition = nativeReader.Position;
			if (File.Exists("debugToc.dat"))
				File.Delete("debugToc.dat");

			nativeReader.Position = 0;
			using (NativeWriter writer = new NativeWriter(new FileStream("debugToc.dat", FileMode.OpenOrCreate)))
			{
				writer.Write(nativeReader.ReadToEnd());
			}
			nativeReader.Position = startPosition;


			var magic = nativeReader.ReadInt(Endian.Big);
			if (magic != 0x3c)
				throw new Exception("Magic is not the expected value of 0x3c");

			nativeReader.Position -= 4;

			int[] tocMetaData = new int[12];

			
			for (int num7 = 0; num7 < 12; num7++)
			{
				tocMetaData[num7] = nativeReader.ReadInt(Endian.Big);
			}

			List<int> list6 = new List<int>();
			if (tocMetaData[0] != 0)
			{
				for (int num8 = 0; num8 < tocMetaData[2]; num8++)
				{
					list6.Add(nativeReader.ReadInt(Endian.Big));
				}
                nativeReader.Position = 556 + tocMetaData[1];
                //nativeReader.Position = 556 + tocMetaData[3] - ((tocMetaData[2] + 2) * 4);
                for (int indexOfBundleCount = 0; indexOfBundleCount < tocMetaData[2]; indexOfBundleCount++)
				{
					int unk1 = nativeReader.ReadInt(Endian.Big);
					int unk2 = nativeReader.ReadInt(Endian.Big);
					int unk3 = nativeReader.ReadInt(Endian.Big);
					int offset = nativeReader.ReadInt(Endian.Big);
					//uint size = nativeReader.ReadUInt(Endian.Big);
					BaseBundleInfo newBundleInfo = new BaseBundleInfo
					{
						//Name = name,
						Offset = offset,
						Size = unk2
					};
					Bundles.Add(newBundleInfo);
				}
			}
			if (tocMetaData[3] != 0)
			{
				nativeReader.Position = 556 + tocMetaData[3];
				List<int> list7 = new List<int>();
				for (int num13 = 0; num13 < tocMetaData[5]; num13++)
				{
					list7.Add(nativeReader.ReadInt(Endian.Big));
				}
				nativeReader.Position = 556 + tocMetaData[4];
				List<Guid> list8 = new List<Guid>();
				for (int num14 = 0; num14 < tocMetaData[5]; num14++)
				{
					byte[] array6 = nativeReader.ReadBytes(16);
					Guid value2 = new Guid(new byte[16]
					{
										array6[15],
										array6[14],
										array6[13],
										array6[12],
										array6[11],
										array6[10],
										array6[9],
										array6[8],
										array6[7],
										array6[6],
										array6[5],
										array6[4],
										array6[3],
										array6[2],
										array6[1],
										array6[0]
					});
					int num15 = nativeReader.ReadInt(Endian.Big) & 0xFFFFFF;
					while (list8.Count <= num15)
					{
						list8.Add(Guid.Empty);
					}
					list8[num15 / 3] = value2;
				}
				nativeReader.Position = 556 + tocMetaData[6];
				for (int num16 = 0; num16 < tocMetaData[5]; num16++)
				{
					nativeReader.ReadByte();
					bool patch2 = nativeReader.ReadBoolean();
					byte catalog2 = nativeReader.ReadByte();
					byte cas2 = nativeReader.ReadByte();
					uint num17 = nativeReader.ReadUInt(Endian.Big);
					uint num18 = nativeReader.ReadUInt(Endian.Big);
					ChunkAssetEntry chunkAssetEntry2 = new ChunkAssetEntry();
					chunkAssetEntry2.Id = list8[num16];
					chunkAssetEntry2.Size = num18;
					chunkAssetEntry2.Location = AssetDataLocation.CasNonIndexed;
					chunkAssetEntry2.ExtraData = new AssetExtraData();
					chunkAssetEntry2.ExtraData.CasPath = AssetManager.Instance.fs.GetFilePath(catalog2, cas2, patch2);
					chunkAssetEntry2.ExtraData.DataOffset = num17;
					if (AssetManager.Instance.chunkList.ContainsKey(chunkAssetEntry2.Id))
					{
						AssetManager.Instance.chunkList.Remove(chunkAssetEntry2.Id);
					}
					AssetManager.Instance.chunkList.Add(chunkAssetEntry2.Id, chunkAssetEntry2);
				}
			}


		}
	}
}
