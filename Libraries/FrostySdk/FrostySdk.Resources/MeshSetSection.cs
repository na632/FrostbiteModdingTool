using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;

namespace FrostySdk.Resources
{
	/*
	public class MeshSetSection
	{
		public TangentSpaceCompressionType TangentSpaceCompressionType;

		private readonly long offset1;

		private readonly long offset2;

		private readonly uint unknownInt2;

		private readonly ulong unknownLong;

		private readonly byte vertexStride;

		private byte bonesPerVertex;

		private readonly List<float> texCoordRatios = new List<float>();

		private readonly byte[] unknownData;

		private readonly int sectionIndex;

		public string Name { get; }

		public int MaterialId { get; }

		public uint UnknownInt { get; }

		public uint PrimitiveCount { get; set; }

		public uint StartIndex { get; set; }

		public uint VertexOffset { get; set; }

		public uint VertexCount { get; set; }

		public GeometryDeclarationDesc[] GeometryDeclDesc { get; } = new GeometryDeclarationDesc[2];


		public List<ushort> BoneList { get; } = new List<ushort>();


		public uint VertexStride => vertexStride;

		public PrimitiveType PrimitiveType { get; }

		public byte BonesPerVertex
		{
			get
			{
				return bonesPerVertex;
			}
			set
			{
				bonesPerVertex = value;
				if (bonesPerVertex > 8)
				{
					bonesPerVertex = 8;
				}
			}
		}

		public int DeclCount => 1;

		public bool HasUnknown { get; }

		public bool HasUnknown2 { get; }

		public bool HasUnknown3 { get; }

		internal MeshSetSection()
		{
		}

		public MeshSetSection(NativeReader reader, int index)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			sectionIndex = index;
			offset1 = reader.ReadInt64LittleEndian();
			long position = reader.ReadInt64LittleEndian();
			MaterialId = reader.ReadInt32LittleEndian();
			UnknownInt = reader.ReadUInt32LittleEndian();
			for (int l = 0; l < 6; l++)
			{
				texCoordRatios.Add(reader.ReadSingleLittleEndian());
			}
			PrimitiveCount = reader.ReadUInt32LittleEndian();
			StartIndex = reader.ReadUInt32LittleEndian();
			VertexOffset = reader.ReadUInt32LittleEndian();
			VertexCount = reader.ReadUInt32LittleEndian();
			unknownData = reader.ReadBytes(32);
			vertexStride = reader.ReadByte();
			PrimitiveType = (PrimitiveType)reader.ReadByte();
			reader.ReadUInt16LittleEndian();
			bonesPerVertex = (byte)reader.ReadUInt16LittleEndian();
			ushort boneCount = reader.ReadUInt16LittleEndian();
			long position2 = reader.ReadInt64LittleEndian();
			unknownLong = reader.ReadUInt64LittleEndian();
			for (int i = 0; i < DeclCount; i++)
			{
				GeometryDeclDesc[i].Elements = new GeometryDeclarationDesc.Element[GeometryDeclarationDesc.MaxElements];
				GeometryDeclDesc[i].Streams = new GeometryDeclarationDesc.Stream[GeometryDeclarationDesc.MaxStreams];
				for (int j = 0; j < GeometryDeclarationDesc.MaxElements; j++)
				{
					GeometryDeclarationDesc.Element element = new GeometryDeclarationDesc.Element
					{
						Usage = (VertexElementUsage)reader.ReadByte(),
						Format = (VertexElementFormat)reader.ReadByte(),
						Offset = reader.ReadByte(),
						StreamIndex = reader.ReadByte()
					};
					GeometryDeclDesc[i].Elements[j] = element;
				}
				for (int k = 0; k < GeometryDeclarationDesc.MaxStreams; k++)
				{
					GeometryDeclarationDesc.Stream stream = new GeometryDeclarationDesc.Stream
					{
						VertexStride = reader.ReadByte(),
						Classification = (VertexElementClassification)reader.ReadByte()
					};
					GeometryDeclDesc[i].Streams[k] = stream;
				}
				GeometryDeclDesc[i].ElementCount = reader.ReadByte();
				GeometryDeclDesc[i].StreamCount = reader.ReadByte();
				reader.ReadBytes(2);
			}
			unknownInt2 = reader.ReadUInt32LittleEndian();
			long position3 = reader.Position;
			reader.Position = position2;
			for (int m = 0; m < boneCount; m++)
			{
				BoneList.Add(reader.ReadUInt16LittleEndian());
			}
			reader.Position = position;
			Name = reader.ReadNullTerminatedString();
			reader.Position = position3;
		}

		public void SetBones(IEnumerable<ushort> bones)
		{
			if (bones == null)
			{
				throw new ArgumentNullException("bones");
			}
			BoneList.Clear();
			foreach (ushort bone in bones)
			{
				BoneList.Add(bone);
			}
		}

		internal void PreProcess(MeshContainer meshContainer)
		{
			if (meshContainer == null)
			{
				throw new ArgumentNullException("meshContainer");
			}
			meshContainer.AddString(sectionIndex + ":" + Name, Name);
			if (BoneList.Count > 0)
			{
				meshContainer.AddRelocPtr("BONELIST", BoneList);
			}
		}

		internal void Process(NativeWriter writer, MeshContainer meshContainer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (meshContainer == null)
			{
				throw new ArgumentNullException("meshContainer");
			}
			writer.WriteInt64LittleEndian(offset1);
			meshContainer.WriteRelocPtr("STR", sectionIndex + ":" + Name, writer);
			writer.WriteInt32LittleEndian(MaterialId);
			writer.WriteUInt32LittleEndian(UnknownInt);
			for (int l = 0; l < 6; l++)
			{
				writer.WriteSingleLittleEndian(texCoordRatios[l]);
			}
			writer.WriteUInt32LittleEndian(PrimitiveCount);
			writer.WriteUInt32LittleEndian(StartIndex);
			writer.WriteUInt32LittleEndian(VertexOffset);
			writer.WriteUInt32LittleEndian(VertexCount);
			writer.WriteBytes(unknownData);
			writer.Write(vertexStride);
			writer.Write((byte)PrimitiveType);
			writer.WriteUInt16LittleEndian(0);
			writer.WriteUInt16LittleEndian(bonesPerVertex);
			writer.WriteUInt16LittleEndian((ushort)BoneList.Count);
			if (BoneList.Count > 0)
			{
				meshContainer.WriteRelocPtr("BONELIST", BoneList, writer);
			}
			else
			{
				writer.WriteUInt64LittleEndian(0uL);
			}
			writer.WriteUInt64LittleEndian(unknownLong);
			for (int i = 0; i < DeclCount; i++)
			{
				for (int j = 0; j < GeometryDeclDesc[i].Elements.Length; j++)
				{
					writer.Write((byte)GeometryDeclDesc[i].Elements[j].Usage);
					writer.Write((byte)GeometryDeclDesc[i].Elements[j].Format);
					writer.Write(GeometryDeclDesc[i].Elements[j].Offset);
					writer.Write(GeometryDeclDesc[i].Elements[j].StreamIndex);
				}
				for (int k = 0; k < GeometryDeclDesc[i].Streams.Length; k++)
				{
					writer.Write(GeometryDeclDesc[i].Streams[k].VertexStride);
					writer.Write((byte)GeometryDeclDesc[i].Streams[k].Classification);
				}
				writer.Write(GeometryDeclDesc[i].ElementCount);
				writer.Write(GeometryDeclDesc[i].StreamCount);
				writer.WriteUInt16LittleEndian(0);
			}
			writer.WriteUInt32LittleEndian(unknownInt2);
		}
	}
	*/
	
	public class MeshSetSection
	{
		public TangentSpaceCompressionType TangentSpaceCompressionType;

		private readonly long offset1;

		private readonly long offset2;

		private readonly uint unknownInt2;

		private readonly ulong unknownLong;

		private readonly byte vertexStride;

		private byte bonesPerVertex;

		private readonly List<float> texCoordRatios = new List<float>();

		private readonly byte[] unknownData;

		private readonly int sectionIndex;

		public string Name { get; }

		public int materialId { get; }

		public string materialName { get; set; }

		public uint UnknownInt { get; }

		public uint PrimitiveCount { get; set; }

		public uint StartIndex { get; set; }

		public uint VertexOffset { get; set; }

		public uint VertexCount { get; set; }

		public GeometryDeclarationDesc[] GeometryDeclDesc { get; } = new GeometryDeclarationDesc[2];


		public List<ushort> BoneList { get; } = new List<ushort>();


		public uint VertexStride => vertexStride;

		public PrimitiveType PrimitiveType { get; }

		public byte BonesPerVertex
		{
			get
			{
				return bonesPerVertex;
			}
			set
			{
				bonesPerVertex = value;
				if (bonesPerVertex > 8)
				{
					bonesPerVertex = 8;
				}
			}
		}

		public int DeclCount => 1;

		public bool hasUnknown { get; }

		public bool hasUnknown2 { get; }

		public bool hasUnknown3 { get; }

		public int SectionIndex { get; set; }

		public int BoneCount { get; set; }
		
		public MeshSetSection(NativeReader reader, int sIndex = 0)
		{
			SectionIndex = sIndex;

			offset1 = reader.ReadLong();
			
			long positionOfMaterialName = reader.ReadLong();
			var posBeforeSearch = reader.Position;
			if (positionOfMaterialName < reader.Length)
			{
				reader.Position = positionOfMaterialName;
				materialName = reader.ReadNullTerminatedString();
			}
			reader.Position = posBeforeSearch;

			materialId = reader.ReadInt();
			UnknownInt = reader.ReadUInt();
			for (int l = 0; l < 6; l++)
			{
                texCoordRatios.Add(reader.ReadFloat());
                //texCoordRatios.Add(1f);
            }
			//reader.Position += 4 * 6;
			PrimitiveCount = reader.ReadUInt();
			StartIndex = reader.ReadUInt();
			VertexOffset = reader.ReadUInt();
			VertexCount = reader.ReadUInt();
			unknownData = reader.ReadBytes(32);
			vertexStride = reader.ReadByte();
			PrimitiveType = (PrimitiveType)reader.ReadByte();
			//bonesPerVertex = (byte)reader.ReadShort();
			reader.Position += 2;
			bonesPerVertex = (byte)reader.ReadShort();
			BoneCount = reader.ReadShort();
			//BoneCount = (byte)reader.ReadByte();
			//reader.ReadByte();

			//long position2 = reader.ReadInt();
			long position2 = reader.ReadLong();
			//reader.ReadInt();
			//reader.ReadULong();
			reader.Position += 8;
            for (int i = 0; i < 1; i++)
            {
                GeometryDeclDesc[i].Elements = new GeometryDeclarationDesc.Element[GeometryDeclarationDesc.MaxElements];
				GeometryDeclDesc[i].Streams = new GeometryDeclarationDesc.Stream[GeometryDeclarationDesc.MaxStreams];
				for (int j = 0; j < GeometryDeclarationDesc.MaxElements; j++)
				{
					GeometryDeclarationDesc.Element element = new GeometryDeclarationDesc.Element
					{
						Usage = (VertexElementUsage)reader.ReadByte(),
						Format = (VertexElementFormat)reader.ReadByte(),
						Offset = reader.ReadByte(),
						StreamIndex = reader.ReadByte()
					};
					GeometryDeclDesc[i].Elements[j] = element;
				}
				for (int k = 0; k < GeometryDeclarationDesc.MaxStreams; k++)
				{
					GeometryDeclarationDesc.Stream stream = new GeometryDeclarationDesc.Stream
					{
						VertexStride = reader.ReadByte(),
						Classification = (VertexElementClassification)reader.ReadByte()
					};
					GeometryDeclDesc[i].Streams[k] = stream;
				}
				GeometryDeclDesc[i].ElementCount = reader.ReadByte();
				GeometryDeclDesc[i].StreamCount = reader.ReadByte();
                reader.ReadBytes(2);
                //reader.Pad(8);
            }
			int num = 0;

			unknownInt2 = reader.ReadUInt();
			//reader.ReadInt();

			//var longUnk1 = reader.ReadLong();
			//var longUnk2 = reader.ReadLong();
			//unknownData = reader.ReadBytes(20);
			//unknownData = reader.ReadBytes(16);
			//unknownData = reader.ReadBytes(36);
			long position3 = reader.Position;
			reader.Position = position2;
			for (int m = 0; m < BoneCount; m++)
			{
				BoneList.Add(reader.ReadUShort());
			}
			reader.Position = position3;
		}
		

		public void SetBones(IEnumerable<ushort> bones)
		{
			BoneList.Clear();
			foreach (ushort bone in bones)
			{
				BoneList.Add(bone);
			}
		}

		internal void PreProcess(MeshContainer meshContainer)
		{
			meshContainer.AddString(SectionIndex + ":" + materialName, materialName);
			if (BoneList.Count > 0)
			{
				meshContainer.AddRelocPtr("BONELIST", BoneList);
			}
		}

		internal void Process(NativeWriter writer, MeshContainer meshContainer)
		{
			writer.Write(offset1);
			if (ProfilesLibrary.DataVersion == 20160607 || ProfilesLibrary.DataVersion == 20161021 || ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
			{
				writer.Write(offset2);
			}
			meshContainer.WriteRelocPtr("STR", SectionIndex + ":" + materialName, writer);
			writer.Write(materialId);
			writer.Write(UnknownInt);
			writer.Write(PrimitiveCount);
			writer.Write(StartIndex);
			writer.Write(VertexOffset);
			writer.Write(VertexCount);
			writer.Write(vertexStride);
			writer.Write((byte)PrimitiveType);
				writer.Write(bonesPerVertex);
				writer.Write((byte)0);
				writer.Write(BoneList.Count);
			if (BoneList.Count > 0)
			{
				meshContainer.WriteRelocPtr("BONELIST", BoneList, writer);
			}
			else
			{
				writer.Write(0uL);
			}

			writer.Write(0uL);
			
			for (int i = 0; i < DeclCount; i++)
			{
				for (int j = 0; j < GeometryDeclDesc[i].Elements.Length; j++)
				{
					writer.Write((byte)GeometryDeclDesc[i].Elements[j].Usage);
					writer.Write((byte)GeometryDeclDesc[i].Elements[j].Format);
					writer.Write(GeometryDeclDesc[i].Elements[j].Offset);
					writer.Write(GeometryDeclDesc[i].Elements[j].StreamIndex);
				}
				for (int k = 0; k < GeometryDeclDesc[i].Streams.Length; k++)
				{
					writer.Write(GeometryDeclDesc[i].Streams[k].VertexStride);
					writer.Write((byte)GeometryDeclDesc[i].Streams[k].Classification);
				}
				writer.Write(GeometryDeclDesc[i].ElementCount);
				writer.Write(GeometryDeclDesc[i].StreamCount);
				writer.Write((ushort)0);
			}
			for (int l = 0; l < 6; l++)
			{
				writer.Write(texCoordRatios[l]);
			}
			writer.Write(unknownData);
		}
	}
	


}
