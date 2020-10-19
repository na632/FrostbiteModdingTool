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
        public string FileLocation { get; internal set; }

        //public int[] ArrayOfInitialHeaderData = new int[12];

        public ContainerMetaData MetaData = new ContainerMetaData();
		public List<BaseBundleInfo> Bundles = new List<BaseBundleInfo>();

		public string SuperBundleName;

		private TocSbReader_FIFA21 ParentReader;
		public TOCFile(TocSbReader_FIFA21 parent)
        {
			ParentReader = parent;
        }

		public class ContainerMetaData
        {
			public int Magic { get; set; }
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
			public int Offset8 { get; set; }

			public void Read(NativeReader nativeReader)
			{
				Magic = nativeReader.ReadInt(Endian.Big);
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
				Offset8 = nativeReader.ReadInt(Endian.Big);

			}

        }

		public int[] tocMetaData = new int[12];

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

			MetaData.Read(nativeReader);
			nativeReader.Position -= 12 * 4;

			for (int tmdIndex = 0; tmdIndex < 12; tmdIndex++)
			{
				tocMetaData[tmdIndex] = nativeReader.ReadInt(Endian.Big);
			}

			List<int> bundleReferences = new List<int>();
			if (tocMetaData[0] != 0 && MetaData.ItemCount > 0)
			{
				//for (int num8 = 0; num8 < tocMetaData[2]; num8++)
				for (int index = 0; index < MetaData.ItemCount; index++)
				{
					bundleReferences.Add(nativeReader.ReadInt(Endian.Big));
				}
                nativeReader.Position = 556 + tocMetaData[1];
				if (FileLocation.Contains("fifagame"))
				{

				}
				//nativeReader.Position = 556 + tocMetaData[3] - ((tocMetaData[2] + 2) * 4);
				//for (int indexOfBundleCount = 0; indexOfBundleCount < tocMetaData[2]; indexOfBundleCount++)
				for (int indexOfBundleCount = 0; indexOfBundleCount < MetaData.ItemCount; indexOfBundleCount++)
				{
					int casStringOffset = nativeReader.ReadInt(Endian.Big);

					//var anthemSearcher = 556 + tocMetaData[8] - 48 + casStringOffset;
					var anthemSearcher = 556 + tocMetaData[8] + casStringOffset;
					if (casStringOffset > 0)
                    {

                    }
					int size = nativeReader.ReadInt(Endian.Big);
					int casIndex = nativeReader.ReadInt(Endian.Big);
					if(casIndex > 0)
                    {

                    }
					int dataOffset = nativeReader.ReadInt(Endian.Big);
					//uint unk4 = nativeReader.ReadUInt(Endian.Big);
					BaseBundleInfo newBundleInfo = new BaseBundleInfo
					{
						CasStringOffset = casStringOffset,
                        //Name = name,
                        Offset = dataOffset,
                        CasIndex = casIndex,
                        Size = size
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
				List<Guid> tocChunkGuids = new List<Guid>();
				for (int num14 = 0; num14 < tocMetaData[5]; num14++)
				{
                    byte[] array6 = nativeReader.ReadBytes(16);
                    Guid tocChunkGuid = new Guid(new byte[16]
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
                    //Guid value2 = nativeReader.ReadGuid(Endian.Little);
                    //if (tocChunkGuid == Guid.Parse("cc9e36b9-9304-2832-01ff-e8820db10773"))
                    //{

                    //}
					int num15 = nativeReader.ReadInt(Endian.Big) & 0xFFFFFF;
					while (tocChunkGuids.Count <= num15)
					{
						tocChunkGuids.Add(Guid.Empty);
					}
					tocChunkGuids[num15 / 3] = tocChunkGuid;
				}
				nativeReader.Position = 556 + tocMetaData[6];
				for (int num16 = 0; num16 < tocMetaData[5]; num16++)
				{
					var unk2 = nativeReader.ReadByte();
					bool patch2 = nativeReader.ReadBoolean();
					byte catalog2 = nativeReader.ReadByte();
					byte cas2 = nativeReader.ReadByte();
					uint chunkOffset = nativeReader.ReadUInt(Endian.Big);
					uint chunkSize = nativeReader.ReadUInt(Endian.Big);
					ChunkAssetEntry chunkAssetEntry2 = new ChunkAssetEntry();
					chunkAssetEntry2.Id = tocChunkGuids[num16];
					chunkAssetEntry2.Size = chunkSize;
					chunkAssetEntry2.Location = AssetDataLocation.CasNonIndexed;
					chunkAssetEntry2.ExtraData = new AssetExtraData();
					chunkAssetEntry2.ExtraData.CasPath = AssetManager.Instance.fs.GetFilePath(catalog2, cas2, patch2);
					chunkAssetEntry2.ExtraData.DataOffset = chunkOffset;
					if (AssetManager.Instance.chunkList.ContainsKey(chunkAssetEntry2.Id))
					{
						AssetManager.Instance.chunkList.Remove(chunkAssetEntry2.Id);
					}
					AssetManager.Instance.chunkList.Add(chunkAssetEntry2.Id, chunkAssetEntry2);
				}
				nativeReader.Position = 556 + tocMetaData[8];

			}

			if(MetaData.Offset2 > MetaData.MetaDataLength && MetaData.Offset2 > MetaData.Offset1)
            {
				List<DbObject> resObjects = new List<DbObject>();

				// need to fix this part and then build res objects
				nativeReader.Position = MetaData.Offset2 + 556;
				for (int indexOfRes = 0; indexOfRes < MetaData.ResCount; indexOfRes++)
				{
					DbObject resObject = new DbObject();
					resObject.AddValue("id", nativeReader.ReadGuid());
					resObject.AddValue("offset", nativeReader.ReadInt(Endian.Big));
					resObject.AddValue("size", nativeReader.ReadInt(Endian.Big));

					resObjects.Add(resObject);
				}

				nativeReader.Position = MetaData.Offset5 + 556;
				for (int indexOfRes = 0; indexOfRes < MetaData.ResCount; indexOfRes++)
				{
					DbObject resObject = resObjects[indexOfRes];
					resObject.AddValue("ukn1", nativeReader.ReadByte());
					resObject.AddValue("patch", nativeReader.ReadBoolean());
					resObject.AddValue("catalog", nativeReader.ReadByte());
					resObject.AddValue("cas", nativeReader.ReadByte());
					resObject.AddValue("offset", nativeReader.ReadInt(Endian.Big));
					resObject.AddValue("size", nativeReader.ReadInt(Endian.Big));

					resObject.AddValue("casPath", AssetManager.Instance.fs.GetFilePath(
						resObject.GetValue<int>("catalog")
						, resObject.GetValue<int>("cas")
						, resObject.GetValue<bool>("patch")));


					resObjects.Add(resObject);
				}
			}
		}
	}
}
