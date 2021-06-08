﻿using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrostySdk.FrostySdk.Resources.Mesh2
{
    public class MeshSetSection2
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

		internal MeshSetSection2()
		{
		}

		public MeshSetSection2(NativeReader reader, AssetManager am, int index)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			sectionIndex = index;
			offset1 = reader.ReadLong();
			long position = reader.ReadLong();
			MaterialId = reader.ReadInt();
			UnknownInt = reader.ReadUInt();
			for (int l = 0; l < 6; l++)
			{
				texCoordRatios.Add(reader.ReadFloat());
			}
			PrimitiveCount = reader.ReadUInt();
			StartIndex = reader.ReadUInt();
			VertexOffset = reader.ReadUInt();
			VertexCount = reader.ReadUInt();
			unknownData = reader.ReadBytes(32);
			vertexStride = reader.ReadByte();
			PrimitiveType = (PrimitiveType)reader.ReadByte();
			reader.ReadUShort();
			bonesPerVertex = (byte)reader.ReadUShort();
			ushort boneCount = reader.ReadUShort();
			long position2 = reader.ReadLong();
			unknownLong = reader.ReadULong();
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
			unknownInt2 = reader.ReadUInt();
			long position3 = reader.Position;
			reader.Position = position2;
			for (int m = 0; m < boneCount; m++)
			{
				BoneList.Add(reader.ReadUShort());
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

}
