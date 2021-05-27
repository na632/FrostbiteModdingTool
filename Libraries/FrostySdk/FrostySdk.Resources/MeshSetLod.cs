using Frosty.Hash;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FrostySdk.Resources
{
	public class MeshSetLod
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct IndexBufferFormatStruct
		{
			[FieldOffset(0)]
			public int format;

			[FieldOffset(0)]
			public IndexBufferFormat formatEnum;
		}

		private readonly uint maxInstances;

		private IndexBufferFormatStruct indexBufferFormat;

		private readonly string shaderDebugName;

		private readonly string name;

		private readonly string shortName;

		private readonly uint nameHash;

		private List<AxisAlignedBox> partBoundingBoxes = new List<AxisAlignedBox>();

		private List<LinearTransform> partTransforms = new List<LinearTransform>();

		private uint inlineDataOffset;

		private readonly byte[] adjacencyData;

		private readonly bool hasBoneShortNames;

		public MeshType Type { get; set; }

		public List<MeshSetSection> Sections { get; } = new List<MeshSetSection>();


		public MeshLayoutFlags Flags { get; private set; }

		public int IndexUnitSize
		{
			get
			{
				int num = (int)Enum.Parse(TypeLibrary.GetType("RenderFormat"), "RenderFormat_R32_UINT");
				if (indexBufferFormat.format != num)
				{
					return 16;
				}
				return 32;
			}
		}

		public uint IndexBufferSize { get; set; }

		public uint VertexBufferSize { get; set; }

		public uint AdjacencyBufferSize { get; }

		public Guid ChunkId { get; set; }

		public string String01 => shaderDebugName;

		public string String02 => name;

		public string String03 => shortName;

		public int BoneCount => BoneIndexArray.Count;

		public List<uint> BoneIndexArray { get; } = new List<uint>();


		public List<uint> BoneShortNameArray { get; } = new List<uint>();


		public int PartCount => partBoundingBoxes.Count;

		public List<AxisAlignedBox> PartBoundingBoxes => partBoundingBoxes;

		public List<LinearTransform> PartTransforms => partTransforms;

		public byte[] InlineData { get; set; }

		public List<List<byte>> CategorySubsetIndices { get; } = new List<List<byte>>();


		public int MaxCategories => 5;

		public bool HasAdjacencyInMesh => false;

		public MeshSetLod(NativeReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			Type = (MeshType)reader.ReadUInt32LittleEndian();
			maxInstances = reader.ReadUInt32LittleEndian();
			uint num = reader.ReadUInt32LittleEndian();
			reader.ReadInt64LittleEndian();
			for (uint num3 = 0u; num3 < num; num3++)
			{
				Sections.Add(null);
			}
			for (int i = 0; i < MaxCategories; i++)
			{
				int num4 = reader.ReadInt32LittleEndian();
				reader.ReadInt64LittleEndian();
				CategorySubsetIndices.Add(new List<byte>());
				for (int j = 0; j < num4; j++)
				{
					CategorySubsetIndices[i].Add(byte.MaxValue);
				}
			}
			Flags = (MeshLayoutFlags)reader.ReadUInt32LittleEndian();
			indexBufferFormat.format = reader.ReadInt32LittleEndian();
			IndexBufferSize = reader.ReadUInt32LittleEndian();
			VertexBufferSize = reader.ReadUInt32LittleEndian();
			if (HasAdjacencyInMesh)
			{
				AdjacencyBufferSize = reader.ReadUInt32LittleEndian();
				adjacencyData = new byte[AdjacencyBufferSize];
			}
			ChunkId = reader.ReadGuid();
			inlineDataOffset = reader.ReadUInt32LittleEndian();
			if (HasAdjacencyInMesh)
			{
				reader.ReadInt64LittleEndian();
			}
			long position = reader.ReadInt64LittleEndian();
			long position2 = reader.ReadInt64LittleEndian();
			long position3 = reader.ReadInt64LittleEndian();
			nameHash = reader.ReadUInt32LittleEndian();
			uint num5 = 0u;
			long num6 = 0L;
			long num7 = 0L;
			long num8 = 0L;
			reader.ReadInt64LittleEndian();
			if (Type == MeshType.MeshType_Skinned)
			{
				num5 = reader.ReadUInt32LittleEndian();
				num6 = reader.ReadInt64LittleEndian();
			}
			else if (Type == MeshType.MeshType_Composite)
			{
				num8 = reader.ReadInt64LittleEndian();
			}
			reader.Pad(16);
			long position4 = reader.Position;
			if (Type == MeshType.MeshType_Skinned)
			{
				reader.Position = num6;
				for (int k = 0; k < num5; k++)
				{
					BoneIndexArray.Add(reader.ReadUInt32LittleEndian());
				}
				if (num7 != 0L)
				{
					reader.Position = num7;
					for (int l = 0; l < num5; l++)
					{
						BoneShortNameArray.Add(reader.ReadUInt32LittleEndian());
					}
				}
			}
			else if (Type == MeshType.MeshType_Composite)
			{
				if (num6 != 0L)
				{
					reader.Position = num6;
					for (int m = 0; m < num5; m++)
					{
						partBoundingBoxes.Add(reader.ReadAxisAlignedBox());
					}
				}
				if (num7 != 0L)
				{
					reader.Position = num7;
					for (int n = 0; n < num5; n++)
					{
						partTransforms.Add(reader.ReadLinearTransform());
					}
				}
				if (num8 != 0L)
				{
					reader.Position = num8;
					List<int> list = new List<int>();
					for (int num9 = 0; num9 < 24; num9++)
					{
						int num10 = reader.ReadByte();
						for (int num2 = 0; num2 < 8; num2++)
						{
							if (((uint)num10 & (true ? 1u : 0u)) != 0)
							{
								list.Add(num9 * 8 + num2);
							}
							num10 >>= 1;
						}
					}
				}
			}
			reader.Position = position;
			shaderDebugName = reader.ReadNullTerminatedString();
			reader.Position = position2;
			name = reader.ReadNullTerminatedString();
			reader.Position = position3;
			shortName = reader.ReadNullTerminatedString();
			reader.Position = position4;
			hasBoneShortNames = BoneShortNameArray.Count > 0;
		}

		internal void PreProcess(MeshContainer meshContainer, ref uint inInlineDataOffset)
		{
			if (meshContainer == null)
			{
				throw new ArgumentNullException("meshContainer");
			}
			inlineDataOffset = uint.MaxValue;
			if (InlineData != null)
			{
				inlineDataOffset = inInlineDataOffset;
				inInlineDataOffset += (uint)InlineData.Length;
			}
			meshContainer.AddRelocArray("SECTION", Sections.Count, Sections);
			foreach (MeshSetSection section in Sections)
			{
				section.PreProcess(meshContainer);
			}
			foreach (List<byte> subsetCategory in CategorySubsetIndices)
			{
				meshContainer.AddRelocArray("SUBSET", subsetCategory.Count, subsetCategory);
			}
			if (HasAdjacencyInMesh && inlineDataOffset != uint.MaxValue)
			{
				meshContainer.AddRelocPtr("ADJACENCY", adjacencyData);
			}
			meshContainer.AddString(shaderDebugName, "Mesh:", ignoreNull: true);
			meshContainer.AddString(name, name.Replace(shortName, ""), ignoreNull: true);
			meshContainer.AddString(shortName, shortName);
			if (Type == MeshType.MeshType_Skinned)
			{
				meshContainer.AddRelocPtr("BONES", BoneIndexArray);
			}
		}

		public void ClearBones()
		{
			BoneIndexArray.Clear();
			BoneShortNameArray.Clear();
		}

		public bool IsSectionInCategory(MeshSetSection section, MeshSubsetCategory category)
		{
			if (section == null)
			{
				throw new ArgumentNullException("section");
			}
			int sectionIndex = GetSectionIndex(section);
			if ((int)category >= CategorySubsetIndices.Count)
			{
				return false;
			}
			return CategorySubsetIndices[(int)category].Contains((byte)sectionIndex);
		}

		public bool IsSectionRenderable(MeshSetSection section)
		{
			if (section == null)
			{
				throw new ArgumentNullException("section");
			}
			if (section.PrimitiveCount != 0)
			{
				if (!IsSectionInCategory(section, MeshSubsetCategory.MeshSubsetCategory_Opaque) && !IsSectionInCategory(section, MeshSubsetCategory.MeshSubsetCategory_Transparent))
				{
					return IsSectionInCategory(section, MeshSubsetCategory.MeshSubsetCategory_TransparentDecal);
				}
				return true;
			}
			return false;
		}

		public void SetSectionCategory(MeshSetSection inSection, MeshSubsetCategory category)
		{
			if (inSection == null)
			{
				throw new ArgumentNullException("inSection");
			}
			byte b = (byte)GetSectionIndex(inSection);
			if (b != byte.MaxValue && !CategorySubsetIndices[(int)category].Contains(b))
			{
				CategorySubsetIndices[(int)category].Add(b);
			}
		}

		public void SetParts(List<LinearTransform> inPartTransforms, List<AxisAlignedBox> inPartBBoxes)
		{
			if (inPartTransforms == null)
			{
				throw new ArgumentNullException("inPartTransforms");
			}
			if (inPartBBoxes == null)
			{
				throw new ArgumentNullException("inPartBBoxes");
			}
			partTransforms = inPartTransforms;
			partBoundingBoxes = inPartBBoxes;
			if (partTransforms.Count == partBoundingBoxes.Count)
			{
				return;
			}
			for (int i = 0; i < partBoundingBoxes.Count; i++)
			{
				if (i >= partTransforms.Count)
				{
					List<LinearTransform> list = partTransforms;
					LinearTransform item = default(LinearTransform);
					Vec3 vec2 = new Vec3
					{
						x = 1f,
						y = 0f,
						z = 0f
					};
					Vec3 vec = (item.right = vec2);
					vec2 = new Vec3
					{
						x = 0f,
						y = 1f,
						z = 0f
					};
					vec = (item.up = vec2);
					vec2 = new Vec3
					{
						x = 0f,
						y = 0f,
						z = 1f
					};
					vec = (item.forward = vec2);
					vec2 = new Vec3
					{
						x = 0f,
						y = 0f,
						z = 0f
					};
					vec = (item.trans = vec2);
					list.Add(item);
				}
			}
		}

		public void SetIndexBufferFormatSize(int newSize)
		{
			indexBufferFormat.format = (int)((newSize == 2) ? Enum.Parse(TypeLibrary.GetType("RenderFormat"), "RenderFormat_R16_UINT") : Enum.Parse(TypeLibrary.GetType("RenderFormat"), "RenderFormat_R32_UINT"));
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
			writer.WriteInt32LittleEndian((int)Type);
			writer.WriteUInt32LittleEndian(maxInstances);
			meshContainer.WriteRelocArray("SECTION", Sections, writer);
			foreach (List<byte> subsetCategory in CategorySubsetIndices)
			{
				meshContainer.WriteRelocArray("SUBSET", subsetCategory, writer);
			}
			writer.WriteInt32LittleEndian((int)Flags);
			writer.WriteInt32LittleEndian(indexBufferFormat.format);
			writer.WriteUInt32LittleEndian(IndexBufferSize);
			writer.WriteUInt32LittleEndian(VertexBufferSize);
			if (HasAdjacencyInMesh)
			{
				writer.WriteInt32LittleEndian(0);
			}
			writer.WriteGuid(ChunkId);
			writer.WriteUInt32LittleEndian(inlineDataOffset);
			if (HasAdjacencyInMesh)
			{
				if (inlineDataOffset != uint.MaxValue)
				{
					meshContainer.WriteRelocPtr("ADJACENCY", adjacencyData, writer);
				}
				else
				{
					writer.WriteUInt64LittleEndian(0uL);
				}
			}
			meshContainer.WriteRelocPtr("STR", shaderDebugName, writer);
			meshContainer.WriteRelocPtr("STR", name, writer);
			meshContainer.WriteRelocPtr("STR", shortName, writer);
			writer.WriteUInt32LittleEndian(nameHash);
			writer.WriteInt64LittleEndian(0L);
			if (Type == MeshType.MeshType_Skinned)
			{
				writer.WriteInt32LittleEndian(BoneIndexArray.Count);
				meshContainer.WriteRelocPtr("BONES", BoneIndexArray, writer);
			}
			writer.WritePadding(16);
		}

		public MeshSubsetCategoryFlags GetSectionCategories(int index)
		{
			MeshSubsetCategoryFlags meshSubsetCategoryFlags = (MeshSubsetCategoryFlags)0;
			for (int i = 0; i < CategorySubsetIndices.Count; i++)
			{
				if (CategorySubsetIndices[i].Contains((byte)index))
				{
					meshSubsetCategoryFlags = (MeshSubsetCategoryFlags)((int)meshSubsetCategoryFlags | (1 << i));
				}
			}
			return meshSubsetCategoryFlags;
		}

		public void ClearCategories()
		{
			for (int i = 0; i < CategorySubsetIndices.Count; i++)
			{
				CategorySubsetIndices[i].Clear();
			}
		}

		public void ReadInlineData(NativeReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			if (ChunkId == Guid.Empty)
			{
				InlineData = reader.ReadBytes((int)(VertexBufferSize + IndexBufferSize));
				reader.Pad(16);
			}
		}

		public void SetInlineData(byte[] inBuffer)
		{
			InlineData = inBuffer;
			ChunkId = Guid.Empty;
			Flags = MeshLayoutFlags.Inline;
		}

		private int GetSectionIndex(MeshSetSection inSection)
		{
			return Sections.IndexOf(inSection);
		}

		public void AddBones(IEnumerable<ushort> bones, IEnumerable<string> boneNames)
		{
			if (bones == null)
			{
				throw new ArgumentNullException("bones");
			}
			if (hasBoneShortNames && boneNames == null)
			{
				throw new ArgumentNullException("boneNames");
			}
			foreach (ushort bone in bones)
			{
				if (!BoneIndexArray.Contains(bone))
				{
					BoneIndexArray.Add(bone);
				}
			}
			if (!hasBoneShortNames)
			{
				return;
			}
			foreach (string boneName in boneNames)
			{
				uint item = (uint)Fnv1.HashString(boneName.ToLower());
				if (!BoneShortNameArray.Contains(item))
				{
					BoneShortNameArray.Add(item);
				}
			}
		}
	}
}


	/*
	public class MeshSetLod
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct IndexBufferFormatStruct
		{
			[FieldOffset(0)]
			public int format;

			[FieldOffset(0)]
			public IndexBufferFormat formatEnum;
		}

		private readonly uint maxInstances;

		private IndexBufferFormatStruct indexBufferFormat;

		private readonly string shaderDebugName;

		private readonly string name;

		private readonly string shortName;

		private readonly uint nameHash;

		private List<AxisAlignedBox> partBoundingBoxes = new List<AxisAlignedBox>();

		private List<LinearTransform> partTransforms = new List<LinearTransform>();

		private byte[] inlineData;

		private uint inlineDataOffset;

		private readonly byte[] adjacencyData;

		private readonly bool hasBoneShortNames;

		public MeshType Type { get; set; }

		public List<MeshSetSection> Sections { get; } = new List<MeshSetSection>();


		public MeshLayoutFlags Flags { get; private set; }

		public int IndexUnitSize
		{
			get
			{
				int num = (int)Enum.Parse(TypeLibrary.GetType("RenderFormat"), "RenderFormat_R32_UINT");
				if (indexBufferFormat.format != num)
				{
					return 16;
				}
				return 32;
			}
		}

		public uint IndexBufferSize { get; set; }

		public uint VertexBufferSize { get; set; }

		public uint AdjacencyBufferSize { get; }

		public Guid ChunkId { get; set; }

		public string String01 => shaderDebugName;

		public string String02 => name;

		public string String03 => shortName;

		public uint BoneCount1;

		public int BoneCount => BoneIndexArray.Count;

		public List<uint> BoneIndexArray { get; } = new List<uint>();


		public List<uint> BoneShortNameArray { get; } = new List<uint>();


		public int PartCount => partBoundingBoxes.Count;

		public List<AxisAlignedBox> PartBoundingBoxes => partBoundingBoxes;

		public List<LinearTransform> PartTransforms => partTransforms;

		public byte[] InlineData => inlineData;

		public List<List<byte>> CategorySubsetIndices { get; } = new List<List<byte>>();


		public int MaxCategories => 5;

		public bool HasAdjacencyInMesh => false;

		public MeshSetLod(NativeReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			Type = (MeshType)reader.ReadUInt();
			maxInstances = reader.ReadUInt();
			uint NumberOfSections = reader.ReadUInt();
			var positionOfSomething1 = reader.ReadULong();
			for (uint sectionIndex = 0u; sectionIndex < NumberOfSections; sectionIndex++)
			{
				Sections.Add(null);
			}
			for (int categoryIndex = 0; categoryIndex < MaxCategories; categoryIndex++)
			{
				int num4 = reader.ReadInt();
				var positionOfSomething2 = reader.ReadLong();
				CategorySubsetIndices.Add(new List<byte>());
				for (int j = 0; j < num4; j++)
				{
					CategorySubsetIndices[categoryIndex].Add(byte.MaxValue);
				}
			}
			Flags = (MeshLayoutFlags)reader.ReadUInt();
			indexBufferFormat.format = reader.ReadInt();
			var ibftype = (IndexBufferFormat)indexBufferFormat.format;
			IndexBufferSize = reader.ReadUInt();
			VertexBufferSize = reader.ReadUInt();
			if (HasAdjacencyInMesh)
			{
				AdjacencyBufferSize = reader.ReadUInt();
				adjacencyData = new byte[AdjacencyBufferSize];
			}
			ChunkId = reader.ReadGuid();
			inlineDataOffset = reader.ReadUInt();
			if (HasAdjacencyInMesh)
			{
				reader.ReadLong();
			}
			long position = reader.ReadLong();
			long position2 = reader.ReadLong();
			long position3 = reader.ReadLong();
			nameHash = reader.ReadUInt();
			uint num5 = 0u;
			long num6 = 0L;
			long num7 = 0L;
			long num8 = 0L;
			var unkPos2 = reader.ReadLong();
			if (Type == MeshType.MeshType_Skinned)
			{
				BoneCount1 = reader.ReadUInt();
				var unkPos1 =  num6 = reader.ReadLong();
			}
			else if (Type == MeshType.MeshType_Composite)
			{
				num8 = reader.ReadLong();
			}
			reader.Pad(16);
			long position4 = reader.Position;
			if (Type == MeshType.MeshType_Skinned)
			{
				reader.Position = num6;
				for (int k = 0; k < BoneCount1; k++)
				{
					BoneIndexArray.Add(reader.ReadUInt());
				}
				//if (num7 != 0L)
				//{
				//	reader.Position = num7;
				//	for (int l = 0; l < num5; l++)
				//	{
				//		BoneShortNameArray.Add(reader.ReadUInt());
				//	}
				//}
			}
			else if (Type == MeshType.MeshType_Composite)
			{
				if (num6 != 0L)
				{
					reader.Position = num6;
					for (int m = 0; m < num5; m++)
					{
						partBoundingBoxes.Add(reader.ReadAxisAlignedBox());
					}
				}
				if (num7 != 0L)
				{
					reader.Position = num7;
					for (int n = 0; n < num5; n++)
					{
						partTransforms.Add(reader.ReadLinearTransform());
					}
				}
				if (num8 != 0L)
				{
					reader.Position = num8;
					List<int> list = new List<int>();
					for (int num9 = 0; num9 < 24; num9++)
					{
						int num10 = reader.ReadByte();
						for (int num2 = 0; num2 < 8; num2++)
						{
							if (((uint)num10 & (true ? 1u : 0u)) != 0)
							{
								list.Add(num9 * 8 + num2);
							}
							num10 >>= 1;
						}
					}
				}
			}
			reader.Position = position;
			shaderDebugName = reader.ReadNullTerminatedString();
			reader.Position = position2;
			name = reader.ReadNullTerminatedString();
			reader.Position = position3;
			shortName = reader.ReadNullTerminatedString();
			reader.Position = position4;
			hasBoneShortNames = BoneShortNameArray.Count > 0;
		}

		internal void PreProcess(MeshContainer meshContainer, ref uint inInlineDataOffset)
		{
			if (meshContainer == null)
			{
				throw new ArgumentNullException("meshContainer");
			}
			inlineDataOffset = uint.MaxValue;
			if (inlineData != null)
			{
				inlineDataOffset = inInlineDataOffset;
				inInlineDataOffset += (uint)inlineData.Length;
			}
			meshContainer.AddRelocArray("SECTION", Sections.Count, Sections);
			foreach (MeshSetSection section in Sections)
			{
				section.PreProcess(meshContainer);
			}
			foreach (List<byte> subsetCategory in CategorySubsetIndices)
			{
				meshContainer.AddRelocArray("SUBSET", subsetCategory.Count, subsetCategory);
			}
			if (HasAdjacencyInMesh && inlineDataOffset != uint.MaxValue)
			{
				meshContainer.AddRelocPtr("ADJACENCY", adjacencyData);
			}
			meshContainer.AddString(shaderDebugName, "Mesh:", ignoreNull: true);
			meshContainer.AddString(name, name.Replace(shortName, ""), ignoreNull: true);
			meshContainer.AddString(shortName, shortName);
			if (Type == MeshType.MeshType_Skinned)
			{
				meshContainer.AddRelocPtr("BONES", BoneIndexArray);
			}
		}

		public void ClearBones()
		{
			BoneIndexArray.Clear();
			BoneShortNameArray.Clear();
		}

		public bool IsSectionInCategory(MeshSetSection section, MeshSubsetCategory category)
		{
			if (section == null)
			{
				throw new ArgumentNullException("section");
			}
			int sectionIndex = GetSectionIndex(section);
			if ((int)category >= CategorySubsetIndices.Count)
			{
				return false;
			}
			return CategorySubsetIndices[(int)category].Contains((byte)sectionIndex);
		}

		public bool IsSectionRenderable(MeshSetSection section)
		{
			if (section == null)
			{
				throw new ArgumentNullException("section");
			}
			if (section.PrimitiveCount != 0)
			{
				if (!IsSectionInCategory(section, MeshSubsetCategory.MeshSubsetCategory_Opaque) && !IsSectionInCategory(section, MeshSubsetCategory.MeshSubsetCategory_Transparent))
				{
					return IsSectionInCategory(section, MeshSubsetCategory.MeshSubsetCategory_TransparentDecal);
				}
				return true;
			}
			return false;
		}

		public void SetSectionCategory(MeshSetSection inSection, MeshSubsetCategory category)
		{
			if (inSection == null)
			{
				throw new ArgumentNullException("inSection");
			}
			byte b = (byte)GetSectionIndex(inSection);
			if (b != byte.MaxValue && !CategorySubsetIndices[(int)category].Contains(b))
			{
				CategorySubsetIndices[(int)category].Add(b);
			}
		}

		public void SetParts(List<LinearTransform> inPartTransforms, List<AxisAlignedBox> inPartBBoxes)
		{
			if (inPartTransforms == null)
			{
				throw new ArgumentNullException("inPartTransforms");
			}
			if (inPartBBoxes == null)
			{
				throw new ArgumentNullException("inPartBBoxes");
			}
			partTransforms = inPartTransforms;
			partBoundingBoxes = inPartBBoxes;
			if (partTransforms.Count == partBoundingBoxes.Count)
			{
				return;
			}
			for (int i = 0; i < partBoundingBoxes.Count; i++)
			{
				if (i >= partTransforms.Count)
				{
					List<LinearTransform> list = partTransforms;
					LinearTransform item = default(LinearTransform);
					Vec3 vec2 = new Vec3
					{
						x = 1f,
						y = 0f,
						z = 0f
					};
					Vec3 vec = (item.right = vec2);
					vec2 = new Vec3
					{
						x = 0f,
						y = 1f,
						z = 0f
					};
					vec = (item.up = vec2);
					vec2 = new Vec3
					{
						x = 0f,
						y = 0f,
						z = 1f
					};
					vec = (item.forward = vec2);
					vec2 = new Vec3
					{
						x = 0f,
						y = 0f,
						z = 0f
					};
					vec = (item.trans = vec2);
					list.Add(item);
				}
			}
		}

		public void SetIndexBufferFormatSize(int newSize)
		{
			indexBufferFormat.format = (int)((newSize == 2) ? Enum.Parse(TypeLibrary.GetType("RenderFormat"), "RenderFormat_R16_UINT") : Enum.Parse(TypeLibrary.GetType("RenderFormat"), "RenderFormat_R32_UINT"));
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
			writer.WriteInt32LittleEndian((int)Type);
			writer.WriteUInt32LittleEndian(maxInstances);
			meshContainer.WriteRelocArray("SECTION", Sections, writer);
			foreach (List<byte> subsetCategory in CategorySubsetIndices)
			{
				meshContainer.WriteRelocArray("SUBSET", subsetCategory, writer);
			}
			writer.WriteInt32LittleEndian((int)Flags);
			writer.WriteInt32LittleEndian(indexBufferFormat.format);
			writer.WriteUInt32LittleEndian(IndexBufferSize);
			writer.WriteUInt32LittleEndian(VertexBufferSize);
			if (HasAdjacencyInMesh)
			{
				writer.WriteInt32LittleEndian(0);
			}
			writer.WriteGuid(ChunkId);
			writer.WriteUInt32LittleEndian(inlineDataOffset);
			if (HasAdjacencyInMesh)
			{
				if (inlineDataOffset != uint.MaxValue)
				{
					meshContainer.WriteRelocPtr("ADJACENCY", adjacencyData, writer);
				}
				else
				{
					writer.Write(0uL);
				}
			}
			meshContainer.WriteRelocPtr("STR", shaderDebugName, writer);
			meshContainer.WriteRelocPtr("STR", name, writer);
			meshContainer.WriteRelocPtr("STR", shortName, writer);
			writer.WriteUInt32LittleEndian(nameHash);
			writer.WriteInt64LittleEndian(0L);
			if (Type == MeshType.MeshType_Skinned)
			{
				writer.WriteInt32LittleEndian(BoneIndexArray.Count);
				meshContainer.WriteRelocPtr("BONES", BoneIndexArray, writer);
			}
			writer.WritePadding(16);
		}

		public MeshSubsetCategoryFlags GetSectionCategories(int index)
		{
			MeshSubsetCategoryFlags meshSubsetCategoryFlags = (MeshSubsetCategoryFlags)0;
			for (int i = 0; i < CategorySubsetIndices.Count; i++)
			{
				if (CategorySubsetIndices[i].Contains((byte)index))
				{
					meshSubsetCategoryFlags = (MeshSubsetCategoryFlags)((int)meshSubsetCategoryFlags | (1 << i));
				}
			}
			return meshSubsetCategoryFlags;
		}

		public void ClearCategories()
		{
			for (int i = 0; i < CategorySubsetIndices.Count; i++)
			{
				CategorySubsetIndices[i].Clear();
			}
		}

		public void ReadInlineData(NativeReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			if (ChunkId == Guid.Empty)
			{
				inlineData = reader.ReadBytes((int)(VertexBufferSize + IndexBufferSize));
				reader.Pad(16);
			}
		}

		public void SetInlineData(byte[] inBuffer)
		{
			inlineData = inBuffer;
			ChunkId = Guid.Empty;
			Flags = MeshLayoutFlags.Inline;
		}

		private int GetSectionIndex(MeshSetSection inSection)
		{
			return Sections.IndexOf(inSection);
		}

		public void AddBones(IEnumerable<ushort> bones, IEnumerable<string> boneNames)
		{
			if (bones == null)
			{
				throw new ArgumentNullException("bones");
			}
			if (hasBoneShortNames && boneNames == null)
			{
				throw new ArgumentNullException("boneNames");
			}
			foreach (ushort bone in bones)
			{
				if (!BoneIndexArray.Contains(bone))
				{
					BoneIndexArray.Add(bone);
				}
			}
			if (!hasBoneShortNames)
			{
				return;
			}
			foreach (string boneName in boneNames)
			{
				uint item = (uint)Fnv1.HashString(boneName.ToLower());
				if (!BoneShortNameArray.Contains(item))
				{
					BoneShortNameArray.Add(item);
				}
			}
		}
	}

}
*/