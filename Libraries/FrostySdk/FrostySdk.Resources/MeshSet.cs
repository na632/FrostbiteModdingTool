using System;
using System.Collections.Generic;
using System.IO;
using Frosty.Hash;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;

public class MeshSet
{
	private TangentSpaceCompressionType tangentSpaceCompressionType;

	private AxisAlignedBox boundingBox;

	private string fullName;

	private uint nameHash;

	private readonly uint headerSize;

	private readonly List<uint> unknownUInts = new List<uint>();

	private readonly ushort? unknownUShort;

	private readonly List<ushort> unknownUShorts = new List<ushort>();

	private readonly List<long> unknownOffsets = new List<long>();

	private readonly ushort boneCount;

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

	public List<MeshSetLod> Lods { get; } = new List<MeshSetLod>();


	public MeshType Type { get; set; }

	public MeshLayoutFlags Flags { get; }

	public string FullName
	{
		get
		{
			return fullName;
		}
		set
		{
			fullName = value.ToLower();
			nameHash = (uint)Fnv1.HashString(fullName);
			int num = fullName.LastIndexOf('/');
			Name = ((num != -1) ? fullName.Substring(num + 1) : string.Empty);
		}
	}

	public string Name { get; private set; }

	public int HeaderSize => BitConverter.ToUInt16(Meta, 12);

	public int MaxLodCount => 7;

	public byte[] Meta { get; } = new byte[16];

	ushort unknownUint2;

	public MeshSet(Stream stream)
	{
		NativeWriter nativeWriterTest = new NativeWriter(new FileStream("MeshSet.dat", FileMode.Create));
		nativeWriterTest.Write(((MemoryStream)stream).ToArray());
		nativeWriterTest.Close();
		nativeWriterTest.Dispose();

		FileReader nativeReader = new FileReader(stream);
		boundingBox = nativeReader.ReadAxisAlignedBox();
		long[] array = new long[MaxLodCount];
		for (int i2 = 0; i2 < MaxLodCount; i2++)
		{
			array[i2] = nativeReader.ReadLong();
		}
		long position = nativeReader.ReadLong();
		long position2 = nativeReader.ReadLong();
		nameHash = nativeReader.ReadUInt();
		Type = (MeshType)nativeReader.ReadUInt();
		Flags = (MeshLayoutFlags)nativeReader.ReadUInt();
		ReadUnknownUInts(nativeReader);
		
		if (ProfilesLibrary.IsMadden21DataVersion())
			unknownUShort = nativeReader.ReadUShort();
		
		ushort lodsCount = nativeReader.ReadUShort();

		// num 2's actual number
		unknownUint2 = nativeReader.ReadUShort();
		ushort num2 = 0;
		if (Type == MeshType.MeshType_Skinned)
		{
			boneCount = nativeReader.ReadUShort();
			num2 = nativeReader.ReadUShort();
			if (boneCount != 0)
			{
				nativeReader.ReadLong();
				nativeReader.ReadLong();
			}
		}
		else if (Type == MeshType.MeshType_Composite)
		{
			num2 = nativeReader.ReadUShort();
			boneCount = nativeReader.ReadUShort();
			long num3 = nativeReader.ReadLong();
			long num4 = nativeReader.ReadLong();
			long position3 = nativeReader.Position;
			if (num3 != 0L)
			{
				nativeReader.Position = num3;
				for (int n2 = 0; n2 < num2; n2++)
				{
					partTransforms.Add(nativeReader.ReadLinearTransform());
				}
			}
			if (num4 != 0L)
			{
				nativeReader.Position = num4;
				for (int num5 = 0; num5 < num2; num5++)
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
			Lods.Add(new MeshSetLod(nativeReader));
		}
		int sectionIndex = 0;
		foreach (MeshSetLod lod4 in Lods)
		{
			for (int m = 0; m < lod4.Sections.Count; m++)
			{
				lod4.Sections[m] = new MeshSetSection(nativeReader, sectionIndex++);
			}
		}
		nativeReader.Pad(16);
		nativeReader.Position = position;
		FullName = nativeReader.ReadNullTerminatedString();
		nativeReader.Position = position2;
		Name = nativeReader.ReadNullTerminatedString();
		nativeReader.Pad(16);
		foreach (MeshSetLod lod3 in Lods)
		{
			for (int l = 0; l < lod3.CategorySubsetIndices.Count; l++)
			{
				for (int j2 = 0; j2 < lod3.CategorySubsetIndices[l].Count; j2++)
				{
					lod3.CategorySubsetIndices[l][j2] = nativeReader.ReadByte();
				}
			}
		}
		nativeReader.Pad(16);
		foreach (MeshSetLod lod2 in Lods)
		{
			nativeReader.Position += lod2.AdjacencyBufferSize;
		}
		nativeReader.Pad(16);
		foreach (MeshSetLod lod in Lods)
		{
			if (lod.Type == MeshType.MeshType_Skinned)
			{
				nativeReader.Position += lod.BoneCount * 4;
			}
			else if (lod.Type == MeshType.MeshType_Composite)
			{
				nativeReader.Position += lod.Sections.Count * 24;
			}
		}
		if (Type == MeshType.MeshType_Skinned)
		{
			nativeReader.Pad(16);
			for (int k = 0; k < num2; k++)
			{
				boneIndices.Add(nativeReader.ReadUShort());
			}
			nativeReader.Pad(16);
			for (int j = 0; j < num2; j++)
			{
				boneBoundingBoxes.Add(nativeReader.ReadAxisAlignedBox());
			}
		}
		else if (Type == MeshType.MeshType_Composite)
		{
			nativeReader.Pad(16);
			for (int i = 0; i < num2; i++)
			{
				partBoundingBoxes.Add(nativeReader.ReadAxisAlignedBox());
			}
		}
		nativeReader.Pad(16);
		foreach (MeshSetLod lod5 in Lods)
		{
			lod5.ReadInlineData(nativeReader);
		}
	}

	private void ReadUnknownUInts(NativeReader nativeReader)
	{
		switch (ProfilesLibrary.DataVersion)
		{
			case 20160927:
				unknownUInts.Add(nativeReader.ReadUInt());
				unknownUInts.Add(nativeReader.ReadUInt());
				break;
			case 20171210:
				unknownUInts.Add(nativeReader.ReadUShort());
				break;
			case (int)ProfilesLibrary.DataVersions.FIFA21:
			case 20170929:
			case 20180807:
			case 20180914:
			case (int)ProfilesLibrary.DataVersions.FIFA20:
				{
					for (int m = 0; m < 8; m++)
					{
						unknownUInts.Add(nativeReader.ReadUInt());
					}
					break;
				}
			case 20180628:
				{
					for (int k = 0; k < 6; k++)
					{
						unknownUInts.Add(nativeReader.ReadUInt());
					}
					break;
				}
			case 20181207:
			case 20190905:
			case 20191101:
				{
					for (int j = 0; j < 7; j++)
					{
						unknownUInts.Add(nativeReader.ReadUInt());
					}
					break;
				}
			case (int)ProfilesLibrary.DataVersions.MADDEN21:
			case 20190729:
				{
					for (int l = 0; l < 8; l++)
					{
						unknownUInts.Add(nativeReader.ReadUInt());
					}
					break;
				}
			default:
				unknownUInts.Add(nativeReader.ReadUInt());
				if (ProfilesLibrary.DataVersion != 20170321)
				{
					unknownUInts.Add(nativeReader.ReadUInt());
					unknownUInts.Add(nativeReader.ReadUInt());
					unknownUInts.Add(nativeReader.ReadUInt());
					unknownUInts.Add(nativeReader.ReadUInt());
					unknownUInts.Add(nativeReader.ReadUInt());
					if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20171110)
					{
						unknownUInts.Add(nativeReader.ReadUInt());
					}
				}
				break;
			case 20131115:
			case 20140225:
			case 20141117:
			case 20141118:
			case 20150223:
			case 20151103:
				break;
		}
	}

	private void PreProcess(MeshContainer meshContainer)
	{
		if (meshContainer == null)
		{
			throw new ArgumentNullException("meshContainer");
		}
		uint inInlineDataOffset = 0u;
		foreach (MeshSetLod lod3 in Lods)
		{
			lod3.PreProcess(meshContainer, ref inInlineDataOffset);
		}
		foreach (MeshSetLod lod2 in Lods)
		{
			meshContainer.AddRelocPtr("LOD", lod2);
		}
		meshContainer.AddString(fullName, fullName.Replace(Name, ""), ignoreNull: true);
		meshContainer.AddString(Name, Name);
		if (Type == MeshType.MeshType_Skinned)
		{
			meshContainer.AddRelocPtr("BONEINDICES", boneIndices);
			meshContainer.AddRelocPtr("BONEBBOXES", boneBoundingBoxes);
		}
	}

	public byte[] ToBytes()
	{
		MeshContainer meshContainer = new MeshContainer();
		PreProcess(meshContainer);
		using FileWriter nativeWriter = new FileWriter(new MemoryStream());
		Write(nativeWriter, meshContainer);
		uint num = (uint)nativeWriter.BaseStream.Position;
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
		num2 = (uint)(nativeWriter.BaseStream.Position - num);
		meshContainer.FixupRelocPtrs(nativeWriter);
		meshContainer.WriteRelocTable(nativeWriter);
		num3 = (uint)(nativeWriter.BaseStream.Position - num - num2);
		BitConverter.TryWriteBytes(Meta, num);
		BitConverter.TryWriteBytes(Meta.AsSpan(4), num2);
		BitConverter.TryWriteBytes(Meta.AsSpan(8), num3);
		BitConverter.TryWriteBytes(Meta.AsSpan(12), headerSize);
		return ((MemoryStream)nativeWriter.BaseStream).ToArray();
	}

	private void Write(NativeWriter writer, MeshContainer meshContainer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (meshContainer == null)
		{
			throw new ArgumentNullException("meshContainer");
		}
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
		meshContainer.WriteRelocPtr("STR", fullName, writer);
		meshContainer.WriteRelocPtr("STR", Name, writer);
		writer.Write((uint)nameHash);
		writer.Write((uint)(uint)Type);
		writer.Write((uint)(uint)Flags);

		// Unknown UInts that all of the systems have
		foreach (uint unknownUInt in unknownUInts)
		{
			writer.Write((uint)unknownUInt);
		}

        // Madden 21 Ushort
        if (unknownUShort.HasValue)
            writer.Write((ushort)unknownUShort);

        writer.WriteUInt16LittleEndian((ushort)Lods.Count);
		ushort num = 0;
		foreach (MeshSetLod lod in Lods)
		{
			num = (ushort)(num + (ushort)lod.Sections.Count);
		}
		writer.WriteUInt16LittleEndian(num);
		if (Type == MeshType.MeshType_Skinned)
		{
			writer.WriteUInt16LittleEndian(boneCount);
			writer.WriteUInt16LittleEndian((ushort)boneIndices.Count);
			if (boneCount > 0)
			{
				meshContainer.WriteRelocPtr("BONEINDICES", boneIndices, writer);
				meshContainer.WriteRelocPtr("BONEBBOXES", boneBoundingBoxes, writer);
			}
		}
		else if (Type == MeshType.MeshType_Composite)
		{
			writer.WriteUInt16LittleEndian((ushort)boneIndices.Count);
			writer.WriteUInt16LittleEndian(boneCount);
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
		foreach (ushort boneIndex in boneIndices)
		{
			writer.WriteUInt16LittleEndian(boneIndex);
		}
		writer.WritePadding(16);
		meshContainer.AddOffset("BONEBBOXES", boneBoundingBoxes, writer);
		foreach (AxisAlignedBox boneBoundingBox in boneBoundingBoxes)
		{
			writer.WriteAxisAlignedBox(boneBoundingBox);
		}
		writer.WritePadding(16);
	}
}
