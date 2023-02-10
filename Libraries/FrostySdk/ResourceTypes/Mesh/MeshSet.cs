using FMT.FileTools;
using FMT.FileTools.Modding;
using FrostySdk;
using FrostySdk.FrostySdk.IO;
using FrostySdk.FrostySdk.Resources;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

public class MeshSet
{
	private TangentSpaceCompressionType tangentSpaceCompressionType { get; set; }

    private AxisAlignedBox boundingBox { get; set; }

    private string fullName { get; set; }

    private uint nameHash { get; set; }

	private uint headerSize { get; set; }

    private List<uint> unknownUInts = new List<uint>();

	//private readonly ushort? unknownUShort;

	private List<ushort> unknownUShorts = new List<ushort>();

	private List<long> unknownOffsets = new List<long>();

	private List<byte[]> unknownBytes = new List<byte[]>();

	private ushort boneCount { get; set; }

	private ushort CullBoxCount { get; set; }

    private readonly List<ushort> boneIndices = new List<ushort>();

	private readonly List<AxisAlignedBox> boneBoundingBoxes = new List<AxisAlignedBox>();

	private readonly List<AxisAlignedBox> partBoundingBoxes = new List<AxisAlignedBox>();

	private readonly List<LinearTransform> partTransforms = new List<LinearTransform>();

	public TangentSpaceCompressionType TangentSpaceCompressionType
	{
		get
		{
			return tangentSpaceCompressionType;
		}
		set
		{
			tangentSpaceCompressionType = value;
			foreach (MeshSetLod lod in Lods)
			{
				foreach (MeshSetSection section in lod.Sections)
				{
					section.TangentSpaceCompressionType = value;
				}
			}
		}
	}

	public AxisAlignedBox BoundingBox => boundingBox;

	public List<MeshSetLod> Lods { get; private set; } = new List<MeshSetLod>();


	public MeshType Type { get; private set; }

    //public EMeshLayout MeshLayout { get; }

    public MeshSetLayoutFlags MeshSetLayoutFlags { get; private set; }

    public string FullName
	{
		get
		{
			return fullName;
		}
		set
		{
			fullName = value.ToLower();
			if(nameHash == 0)
				nameHash = (uint)FMT.FileTools.Fnv1a.HashString(fullName);
			int num = fullName.LastIndexOf('/');
			Name = ((num != -1) ? fullName.Substring(num + 1) : string.Empty);
		}
	}

	public string Name { get; private set; }

	public int HeaderSize => BitConverter.ToUInt16(Meta, 12);

	public int MaxLodCount => (int)MeshLimits.MaxMeshLodCount;

	public byte[] Meta { get; private set; } = new byte[16];

	public ushort MeshCount { get; set; }

	public MeshType FIFA23_Type2 { get; private set; }
    public byte[] FIFA23_TypeUnknownBytes { get; private set; }
    public byte[] FIFA23_SkinnedUnknownBytes { get; private set; }

    public long SkinnedBoneCountUnkLong1 { get; private set; }
    public long SkinnedBoneCountUnkLong2 { get; private set; }

    public ShaderDrawOrder ShaderDrawOrder { get; private set; }

    public ShaderDrawOrderUserSlot ShaderDrawOrderUserSlot { get; private set; }

    public ShaderDrawOrderSubOrder ShaderDrawOrderSubOrder { get; private set; }

    public long UnknownPostLODCount { get; private set; }

    public List<ushort> LodFade { get; } = new List<ushort>();

	public MeshSet(ResAssetEntry entry, EGame gameVersion = EGame.UNSET)
	{
		Read(AssetManager.Instance.GetRes(entry), gameVersion, entry);
    }

    public MeshSet(Stream stream, EGame gameVersion = EGame.UNSET, ResAssetEntry entry = null)
	{
		Read(stream, gameVersion, entry);

    }

	private void Read(Stream stream, EGame gameVersion = EGame.UNSET, ResAssetEntry entry = null)
	{
        if (stream == null)
            return;

		if (entry != null)
			Meta = entry.ResMeta;

        if (gameVersion == EGame.UNSET)
            gameVersion = (EGame)ProfileManager.LoadedProfile.DataVersion;

#if DEBUG
        NativeWriter nativeWriterTest = new NativeWriter(new FileStream("_MeshSet.dat", FileMode.Create));
        nativeWriterTest.Write(((MemoryStream)stream).ToArray());
        nativeWriterTest.Close();
		nativeWriterTest.Dispose();

#endif

        FileReader nativeReader = new FileReader(stream);
        // useful for resetting when live debugging
        nativeReader.Position = 0;

        boundingBox = nativeReader.ReadAxisAlignedBox();
        long[] lodOffsets = new long[MaxLodCount];
        for (int i2 = 0; i2 < MaxLodCount; i2++)
        {
            lodOffsets[i2] = nativeReader.ReadLong();
        }
        UnknownPostLODCount = nativeReader.ReadLong();
        long offsetNameLong = nativeReader.ReadLong();
        long offsetNameShort = nativeReader.ReadLong();

        //nativeReader.Position = offsetNameLong;
        FullName = nativeReader.ReadNullTerminatedString(false, offset: offsetNameLong);
        //nativeReader.Position = offsetNameShort;
        Name = nativeReader.ReadNullTerminatedString(false, offset: offsetNameShort);


        nameHash = nativeReader.ReadUInt();
        Type = (MeshType)nativeReader.ReadUInt();
        if (gameVersion == EGame.FIFA23)
        {
            nativeReader.Position -= 4;
            Type = (MeshType)nativeReader.ReadByte();
            // another type?
            FIFA23_Type2 = (MeshType)nativeReader.ReadByte();
            // lots of zeros?
            FIFA23_TypeUnknownBytes = nativeReader.ReadBytes(18);
            //FIFA23_TypeUnknownBytes = nativeReader.ReadBytes(19);

            // we should be at 128 anyway?
            //nativeReader.Position = 128;
        }
        for (int n = 0; n < MaxLodCount * 2; n++)
        {
            LodFade.Add(nativeReader.ReadUInt16LittleEndian());
        }
        MeshSetLayoutFlags = (MeshSetLayoutFlags)nativeReader.ReadULong();
        ShaderDrawOrder = (ShaderDrawOrder)nativeReader.ReadByte();
        ShaderDrawOrderUserSlot = (ShaderDrawOrderUserSlot)nativeReader.ReadByte();
        ShaderDrawOrderSubOrder = (ShaderDrawOrderSubOrder)nativeReader.ReadUShort();

        ushort lodsCount = 0;
        if (
            ProfileManager.IsMadden21DataVersion(gameVersion)
            || ProfileManager.IsMadden22DataVersion(gameVersion)
            || ProfileManager.IsMadden23DataVersion(gameVersion)
            )
        {
            nativeReader.ReadUShort();
            lodsCount = nativeReader.ReadUShort();
            nativeReader.ReadUInt();
            nativeReader.ReadUShort();
        }
        else
        {
            lodsCount = nativeReader.ReadUShort();
            MeshCount = nativeReader.ReadUShort();
        }
        //boneCount = 0;
        if (ProfileManager.IsMadden21DataVersion(gameVersion) || ProfileManager.IsMadden22DataVersion(gameVersion) || ProfileManager.IsMadden23DataVersion(gameVersion))
        {
            unknownBytes.Add(nativeReader.ReadBytes(8));
        }

#if DEBUG
        // useful for resetting when live debugging
        var positionBeforeMeshTypeRead = nativeReader.Position;
        nativeReader.Position = positionBeforeMeshTypeRead;
#endif

        if (Type == MeshType.MeshType_Skinned)
        {
            if (gameVersion == EGame.FIFA23)
            {
                // 12 bytes of unknowness
                FIFA23_SkinnedUnknownBytes = nativeReader.ReadBytes(12);
            }
            boneCount = nativeReader.ReadUInt16LittleEndian();
            CullBoxCount = nativeReader.ReadUInt16LittleEndian();
            if (CullBoxCount != 0)
            {
                long cullBoxBoneIndicesOffset = nativeReader.ReadInt64LittleEndian();
                long cullBoxBoundingBoxOffset = nativeReader.ReadInt64LittleEndian();
                long position = nativeReader.Position;
                if (cullBoxBoneIndicesOffset != 0L)
                {
                    nativeReader.Position = cullBoxBoneIndicesOffset;
                    for (int m = 0; m < CullBoxCount; m++)
                    {
                        boneIndices.Add(nativeReader.ReadUInt16LittleEndian());
                    }
                }
                if (cullBoxBoundingBoxOffset != 0L)
                {
                    nativeReader.Position = cullBoxBoundingBoxOffset;
                    for (int l = 0; l < CullBoxCount; l++)
                    {
                        boneBoundingBoxes.Add(nativeReader.ReadAxisAlignedBox());
                    }
                }
                nativeReader.Position = position;
            }
        }
        else if (Type == MeshType.MeshType_Composite)
        {
            boneCount = nativeReader.ReadUShort();
            boneCount = nativeReader.ReadUShort();
            long num3 = nativeReader.ReadLong();
            long num4 = nativeReader.ReadLong();
            long position3 = nativeReader.Position;
            if (num3 != 0L)
            {
                nativeReader.Position = num3;
                for (int n2 = 0; n2 < boneCount; n2++)
                {
                    partTransforms.Add(nativeReader.ReadLinearTransform());
                }
            }
            if (num4 != 0L)
            {
                nativeReader.Position = num4;
                for (int num5 = 0; num5 < boneCount; num5++)
                {
                    partBoundingBoxes.Add(nativeReader.ReadAxisAlignedBox());
                }
            }
            nativeReader.Position = position3;
        }
        nativeReader.Pad(16);
        headerSize = (uint)nativeReader.Position;
        for (int n = 0; n < lodsCount; n++)
        {
            nativeReader.Position = lodOffsets[n];
            var mslod = new MeshSetLod(nativeReader);
            mslod.SetParts(partTransforms, partBoundingBoxes);
            Lods.Add(mslod);
        }
        uint meshSetLayoutSize = BinaryPrimitives.ReadUInt32LittleEndian(Meta);
        uint vertexIndexSize = BinaryPrimitives.ReadUInt32LittleEndian(Meta.AsSpan(4));
        if (meshSetLayoutSize == 0 || vertexIndexSize == 0)
        {
            return;
        }
        nativeReader.Position = meshSetLayoutSize;
        foreach (MeshSetLod lod2 in Lods)
        {
            lod2.ReadInlineData(nativeReader);
        }
    }

	private void PreProcess(MeshContainer meshContainer)
	{
        if (meshContainer == null)
        {
            throw new ArgumentNullException("meshContainer");
        }
        uint inlineDataOffset = 0u;
        foreach (MeshSetLod lod2 in Lods)
        {
            lod2.PreProcess(meshContainer, ref inlineDataOffset);
        }
        foreach (MeshSetLod lod in Lods)
        {
            meshContainer.AddRelocPtr("LOD", lod);
        }
        meshContainer.AddString(fullName, fullName.Replace(Name, string.Empty), ignoreNull: true);
        meshContainer.AddString(Name, Name);
        if (Type == MeshType.MeshType_Skinned)
        {
            meshContainer.AddRelocPtr("BONEINDICES", boneIndices);
            meshContainer.AddRelocPtr("BONEBBOXES", boneBoundingBoxes);
        }
        else if (Type == MeshType.MeshType_Composite)
        {
            meshContainer.AddRelocPtr("PARTTRANSFORMS", partTransforms);
            meshContainer.AddRelocPtr("PARTBOUNDINGBOXES", partBoundingBoxes);
        }
    }

	public byte[] ToBytes()
	{
		MeshContainer meshContainer = new MeshContainer();
		PreProcess(meshContainer);
		using FileWriter nativeWriter = new FileWriter(new MemoryStream());
		Write(nativeWriter, meshContainer);
		uint resSize = (uint)nativeWriter.BaseStream.Position;
		uint num2 = 0u;
		uint num3 = 0u;
		foreach (MeshSetLod lod in Lods)
		{
			if (lod.ChunkId == Guid.Empty)
			{
				nativeWriter.WriteBytes(lod.InlineData);
				nativeWriter.WritePadding(16);
			}
		}
		num2 = (uint)(nativeWriter.BaseStream.Position - resSize);
		meshContainer.WriteRelocPtrs(nativeWriter);
		meshContainer.WriteRelocTable(nativeWriter);
		num3 = (uint)(nativeWriter.BaseStream.Position - resSize - num2);
		BitConverter.TryWriteBytes(Meta, resSize);
		BitConverter.TryWriteBytes(Meta.AsSpan(4), num2);
		BitConverter.TryWriteBytes(Meta.AsSpan(8), num3);
		BitConverter.TryWriteBytes(Meta.AsSpan(12), headerSize);
		var array = ((MemoryStream)nativeWriter.BaseStream).ToArray();
#if DEBUG
		if (File.Exists("_MeshSetNEW.dat")) File.Delete("_MeshSetNEW.dat");
        File.WriteAllBytes("_MeshSetNEW.dat", array);
#endif
		return array;
	}

	private void Write(NativeWriter writer, MeshContainer meshContainer, EGame gameVersion = EGame.UNSET)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (meshContainer == null)
		{
			throw new ArgumentNullException("meshContainer");
		}

        if (gameVersion == EGame.UNSET)
            gameVersion = (EGame)ProfileManager.LoadedProfile.DataVersion;

        writer.WriteAxisAlignedBox(boundingBox);
		for (int i = 0; i < MaxLodCount; i++)
		{
			if (i < Lods.Count)
			{
				meshContainer.WriteRelocPtr("LOD", Lods[i], writer);
			}
			else
			{
				writer.WriteUInt64LittleEndian(0uL);
			}
		}
        //UnknownPostLODCount = nativeReader.ReadLong();
        writer.Write(UnknownPostLODCount);

        meshContainer.WriteRelocPtr("STR", fullName, writer);
		meshContainer.WriteRelocPtr("STR", Name, writer);
		writer.Write((uint)nameHash);

		if (ProfileManager.IsFIFA23DataVersion())
		{
			writer.Write((byte)Type);
			writer.Write((byte)FIFA23_Type2);
			writer.Write(FIFA23_TypeUnknownBytes);
        }
        else
		{
			writer.Write((uint)Type);
		}
        for (int n = 0; n < MaxLodCount * 2; n++)
        {
			//LodFade.Add(nativeReader.ReadUInt16LittleEndian());
			writer.Write((ushort)LodFade[n]);
        }
		//MeshLayout = (EMeshLayout)nativeReader.ReadByte();
		//writer.Write((byte)MeshLayout);
		//unknownUInts.Add(nativeReader.ReadUInt());
		writer.Write((ulong)MeshSetLayoutFlags);
		//writer.Write((uint)unknownUInts[0]);
		//writer.Write((uint)unknownUInts[1]);
		//writer.Position -= 1;
        //ShaderDrawOrder = (ShaderDrawOrder)nativeReader.ReadByte();
        writer.Write((byte)ShaderDrawOrder);
        //ShaderDrawOrderUserSlot = (ShaderDrawOrderUserSlot)nativeReader.ReadByte();
        writer.Write((byte)ShaderDrawOrderUserSlot);
        //ShaderDrawOrderSubOrder = (ShaderDrawOrderSubOrder)nativeReader.ReadUShort();
        writer.Write((ushort)ShaderDrawOrderSubOrder);

        //writer.Write((uint)MeshLayout);

        //foreach (uint unknownUInt in unknownUInts)
        //{
        //	writer.Write((uint)unknownUInt);
        //}
        //var sumOfLOD = (ushort)(Lods.Sum(x => x.Sections.Count));
		if (ProfileManager.IsMadden21DataVersion(ProfileManager.Game) || ProfileManager.IsMadden22DataVersion(ProfileManager.Game))
		{
			writer.WriteUInt16LittleEndian(0);
			writer.Write((ushort)Lods.Count);
			writer.WriteUInt32LittleEndian(MeshCount);
			writer.Write((ushort)Lods.Count);
			writer.Write(unknownBytes[0]);
		}
		else
		{
			writer.WriteUInt16LittleEndian((ushort)Lods.Count);
			//writer.WriteUInt16LittleEndian(sumOfLOD);
			writer.WriteUInt16LittleEndian(MeshCount);
		}
		
		//// Madden 21 Ushort
		//if (unknownUShort.HasValue)
		//          writer.Write((ushort)unknownUShort);

		//      writer.Write((ushort)Lods.Count);
		//ushort num = 0;
		//foreach (MeshSetLod lod in Lods)
		//{
		//	num = (ushort)(num + (ushort)lod.Sections.Count);
		//}
		//writer.Write(num);
		if (Type == MeshType.MeshType_Skinned)
		{
			if (ProfileManager.IsFIFA23DataVersion())
			{
				writer.Write(FIFA23_SkinnedUnknownBytes);
			}

			writer.WriteUInt16LittleEndian((ushort)boneCount);
			if (ProfileManager.IsMadden21DataVersion(ProfileManager.Game) || ProfileManager.IsMadden22DataVersion(ProfileManager.Game))
            {
				writer.WriteUInt32LittleEndian((uint)CullBoxCount);
			}
			else
			{
				writer.WriteUInt16LittleEndian((ushort)CullBoxCount);
            }
			//writer.WriteUInt16LittleEndian(boneCount);
			//writer.WriteUInt16LittleEndian((ushort)boneIndices.Count);
			if (CullBoxCount > 0)
			{
				meshContainer.WriteRelocPtr("BONEINDICES", boneIndices, writer);
				meshContainer.WriteRelocPtr("BONEBBOXES", boneBoundingBoxes, writer);
			}
		}
		else if (Type == MeshType.MeshType_Composite)
		{
			writer.WriteUInt16LittleEndian((ushort)boneIndices.Count);
			writer.WriteUInt16LittleEndian(0);
            meshContainer.WriteRelocPtr("BONEINDICES", boneIndices, writer);
            meshContainer.WriteRelocPtr("BONEBBOXES", boneBoundingBoxes, writer);
        }
		writer.WritePadding(16);
		foreach (MeshSetLod lod2 in Lods)
		{
			meshContainer.AddOffset("LOD", lod2, writer);
			lod2.Write(writer, meshContainer);
		}
		foreach (MeshSetLod lod3 in Lods)
		{
			meshContainer.AddOffset("SECTION", lod3.Sections, writer);
			foreach (MeshSetSection section3 in lod3.Sections)
			{
				section3.Process(writer, meshContainer);
			}
		}
		writer.WritePadding(16);
		foreach (MeshSetLod lod5 in Lods)
		{
			foreach (MeshSetSection section2 in lod5.Sections)
			{
				if (section2.BoneList.Count == 0)
				{
					continue;
				}
				meshContainer.AddOffset("BONELIST", section2.BoneList, writer);
				foreach (ushort bone in section2.BoneList)
				{
					writer.WriteUInt16LittleEndian(bone);
				}
			}
		}
		writer.WritePadding(16);
		meshContainer.WriteStrings(writer);
		writer.WritePadding(16);
		foreach (MeshSetLod lod6 in Lods)
		{
			foreach (List<byte> categorySubsetIndex in lod6.CategorySubsetIndices)
			{
				meshContainer.AddOffset("SUBSET", categorySubsetIndex, writer);
				writer.WriteBytes(categorySubsetIndex.ToArray());
			}
		}
		writer.WritePadding(16);
		if (Type != MeshType.MeshType_Skinned)
		{
			return;
		}
		foreach (MeshSetLod lod4 in Lods)
		{
			meshContainer.AddOffset("BONES", lod4.BoneIndexArray, writer);
			foreach (uint item in lod4.BoneIndexArray)
			{
				writer.Write((uint)item);
			}
		}
		writer.WritePadding(16);
		meshContainer.AddOffset("BONEINDICES", boneIndices, writer);
		//foreach (ushort boneIndex in CullBoxCount)
		for(var iCB = 0; iCB < CullBoxCount; iCB++)
        {
			//writer.WriteUInt16LittleEndian(boneIndex);
			writer.WriteUInt16LittleEndian(boneIndices[iCB]);
		}
		writer.WritePadding(16);
		meshContainer.AddOffset("BONEBBOXES", boneBoundingBoxes, writer);
		for(var iCB = 0; iCB < CullBoxCount; iCB++)
		//foreach (AxisAlignedBox boneBoundingBox in boneBoundingBoxes)
        {
			writer.WriteAxisAlignedBox(boneBoundingBoxes[iCB]);
		}
		writer.WritePadding(16);
	}
}
