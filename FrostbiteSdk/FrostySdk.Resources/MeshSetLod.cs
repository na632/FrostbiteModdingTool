using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FrostySdk.Resources
{
	public class MeshSetLod
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct IndexBufferFormatStruct
		{
			[FieldOffset(0)]
			public uint format;

			[FieldOffset(0)]
			public IndexBufferFormat formatEnum;
		}

		private MeshType meshType;

		private uint maxInstances;

		private MeshLayoutFlags flags;

		private IndexBufferFormatStruct indexBufferFormat;

		private uint indexBufferSize;

		private uint vertexBufferSize;

		private uint adjacencyBufferSize;

		private Guid chunkId;

		private string shaderDebugName;

		private string name;

		private string shortName;

		private uint nameHash;

		private List<MeshSetSection> sections = new List<MeshSetSection>();

		private List<List<byte>> subsetCategories = new List<List<byte>>();

		private List<uint> boneIndexArray = new List<uint>();

		private List<uint> boneShortNameArray = new List<uint>();

		private List<AxisAlignedBox> partBoundingBoxes = new List<AxisAlignedBox>();

		private List<LinearTransform> partTransforms = new List<LinearTransform>();

		private List<uint> unknownUInts = new List<uint>();

		private List<long> unknownOffsets = new List<long>();

		private byte[] inlineData;

		public MeshType Type
		{
			get
			{
				return meshType;
			}
			set
			{
				meshType = value;
			}
		}

		public List<MeshSetSection> Sections => sections;

		public MeshLayoutFlags Flags => flags;

		public int IndexUnitSize
		{
			get
			{
				if (ProfilesLibrary.DataVersion == 20141118 || ProfilesLibrary.DataVersion == 20141117 || ProfilesLibrary.DataVersion == 20140225 || ProfilesLibrary.DataVersion == 20131115)
				{
					if (indexBufferFormat.formatEnum != 0)
					{
						return 32;
					}
					return 16;
				}
				int num = (int)Enum.Parse(TypeLibrary.GetType("RenderFormat"), "RenderFormat_R32_UINT");
				if (indexBufferFormat.format != num)
				{
					return 16;
				}
				return 32;
			}
		}

		public uint IndexBufferSize
		{
			get
			{
				return indexBufferSize;
			}
			set
			{
				indexBufferSize = value;
			}
		}

		public uint VertexBufferSize
		{
			get
			{
				return vertexBufferSize;
			}
			set
			{
				vertexBufferSize = value;
			}
		}

		public uint AdjacencyBufferSize => adjacencyBufferSize;

		public Guid ChunkId
		{
			get
			{
				return chunkId;
			}
			set
			{
				chunkId = value;
			}
		}

		public string String01 => shaderDebugName;

		public string String02 => name;

		public string String03 => shortName;

		public int BoneCount => boneIndexArray.Count;

		public List<uint> BoneIndexArray => boneIndexArray;

		public List<uint> BoneShortNameArray => boneShortNameArray;

		public int PartCount => partBoundingBoxes.Count;

		public List<AxisAlignedBox> PartBoundingBoxes => partBoundingBoxes;

		public List<LinearTransform> PartTransforms => partTransforms;

		public byte[] InlineData => inlineData;

		public List<List<byte>> CategorySubsetIndices => subsetCategories;

		public int Size
		{
			get
			{
				if (meshType == MeshType.MeshType_Rigid)
				{
					switch (ProfilesLibrary.DataVersion)
					{
					case 20170321:
						return 160;
					case 20151117:
						return 160;
					case 20160607:
						return 176;
					case 20141118:
						return 176;
					}
				}
				else if (meshType == MeshType.MeshType_Skinned)
				{
					int dataVersion = ProfilesLibrary.DataVersion;
					if (dataVersion == 20160607)
					{
						return 192;
					}
					return 176;
				}
				return 0;
			}
		}

		public int MaxCategories
		{
			get
			{
				int dataVersion = ProfilesLibrary.DataVersion;
				if (dataVersion == 20131115 || dataVersion == 20140225 || (uint)(dataVersion - 20141117) <= 1u)
				{
					return 4;
				}
				return 5;
			}
		}

		public bool HasAdjacencyInMesh
		{
			get
			{
				switch (ProfilesLibrary.DataVersion)
				{
				case 20131115:
				case 20140225:
				case 20141117:
				case 20141118:
				case 20160607:
				case 20161021:
				case 20171117:
				case 20180628:
					return true;
				default:
					return false;
				}
			}
		}

		public MeshSetLod(NativeReader reader, AssetManager am)
		{
			meshType = (MeshType)reader.ReadUInt();
			maxInstances = reader.ReadUInt();
			if (ProfilesLibrary.DataVersion == 20181207)
			{
				reader.ReadUInt();
			}
			uint num = reader.ReadUInt();
			reader.ReadLong();
			for (uint num2 = 0u; num2 < num; num2++)
			{
				sections.Add(null);
			}
			for (int i = 0; i < MaxCategories; i++)
			{
				int num3 = reader.ReadInt();
				reader.ReadLong();
				subsetCategories.Add(new List<byte>());
				for (int j = 0; j < num3; j++)
				{
					subsetCategories[i].Add(byte.MaxValue);
				}
			}
			if (ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807)
			{
				reader.ReadUInt();
			}
			flags = (MeshLayoutFlags)reader.ReadUInt();
			if (ProfilesLibrary.DataVersion != 20141118 && ProfilesLibrary.DataVersion != 20140225 && ProfilesLibrary.DataVersion != 20141117 && ProfilesLibrary.DataVersion != 20131115)
			{
				indexBufferFormat.format = reader.ReadUInt();
			}
			else
			{
				indexBufferFormat.formatEnum = (IndexBufferFormat)reader.ReadInt();
			}
			indexBufferSize = reader.ReadUInt();
			vertexBufferSize = reader.ReadUInt();
			if (HasAdjacencyInMesh)
			{
				adjacencyBufferSize = reader.ReadUInt();
			}
			chunkId = reader.ReadGuid();
			reader.ReadUInt();
			if (HasAdjacencyInMesh)
			{
				reader.ReadLong();
			}
			long position = reader.ReadLong();
			long position2 = reader.ReadLong();
			long position3 = reader.ReadLong();
			nameHash = reader.ReadUInt();
			uint num4 = 0u;
			long num5 = 0L;
			long num6 = 0L;
			long num7 = 0L;
			reader.ReadLong();
			if (ProfilesLibrary.DataVersion == 20170321)
			{
				if (meshType == MeshType.MeshType_Skinned)
				{
					num4 = reader.ReadUInt();
					num5 = reader.ReadLong();
					num6 = reader.ReadLong();
				}
				else if (meshType == MeshType.MeshType_Composite)
				{
					num7 = reader.ReadLong();
				}
			}
			else if (ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20190729 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905)
			{
				if (meshType == MeshType.MeshType_Skinned)
				{
					num4 = reader.ReadUInt();
					num5 = reader.ReadLong();
				}
				else if (meshType == MeshType.MeshType_Composite)
				{
					num7 = reader.ReadLong();
				}
			}
			else if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20171110)
			{
				if (meshType == MeshType.MeshType_Skinned)
				{
					num4 = reader.ReadUInt();
					num5 = reader.ReadLong();
				}
				else if (meshType == MeshType.MeshType_Composite)
				{
					num7 = reader.ReadLong();
				}
			}
			else if (ProfilesLibrary.DataVersion == 20181207)
			{
				if (meshType == MeshType.MeshType_Skinned)
				{
					num4 = reader.ReadUInt();
					num5 = reader.ReadLong();
				}
				else if (meshType == MeshType.MeshType_Composite)
				{
					num6 = reader.ReadLong();
					num7 = reader.ReadLong();
				}
			}
			else
			{
				num4 = reader.ReadUInt();
				if (meshType >= MeshType.MeshType_Skinned)
				{
					num5 = reader.ReadLong();
					num6 = reader.ReadLong();
					if (meshType == MeshType.MeshType_Composite)
					{
						num7 = reader.ReadLong();
					}
				}
			}
			reader.Pad(16);
			long position4 = reader.Position;
			if (meshType == MeshType.MeshType_Skinned)
			{
				reader.Position = num5;
				for (int k = 0; k < num4; k++)
				{
					boneIndexArray.Add(reader.ReadUInt());
				}
				if (num6 != 0L)
				{
					reader.Position = num6;
					for (int l = 0; l < num4; l++)
					{
						boneShortNameArray.Add(reader.ReadUInt());
					}
				}
			}
			else if (meshType == MeshType.MeshType_Composite)
			{
				if (num5 != 0L)
				{
					reader.Position = num5;
					for (int m = 0; m < num4; m++)
					{
						partBoundingBoxes.Add(reader.ReadAxisAlignedBox());
					}
				}
				if (num6 != 0L)
				{
					reader.Position = num6;
					for (int n = 0; n < num4; n++)
					{
						partTransforms.Add(reader.ReadLinearTransform());
					}
				}
				if (num7 != 0L)
				{
					reader.Position = num7;
					List<int> list = new List<int>();
					for (int num8 = 0; num8 < 24; num8++)
					{
						int num9 = reader.ReadByte();
						for (int num10 = 0; num10 < 8; num10++)
						{
							if ((num9 & 1) != 0)
							{
								list.Add(num8 * 8 + num10);
							}
							num9 >>= 1;
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
		}

		public bool IsSectionInCategory(MeshSetSection section, MeshSubsetCategory category)
		{
			int sectionIndex = GetSectionIndex(section);
			if ((int)category >= subsetCategories.Count)
			{
				return false;
			}
			return subsetCategories[(int)category].Contains((byte)sectionIndex);
		}

		public bool IsSectionRenderable(MeshSetSection section)
		{
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
			byte b = (byte)GetSectionIndex(inSection);
			if (b != byte.MaxValue && !subsetCategories[(int)category].Contains(b))
			{
				subsetCategories[(int)category].Add(b);
			}
		}

		public void SetParts(List<LinearTransform> inPartTransforms, List<AxisAlignedBox> inPartBBoxes)
		{
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
					Vec3 vec = new Vec3
					{
						x = 1f,
						y = 0f,
						z = 0f
					};
					item.right = vec;
					vec = new Vec3
					{
						x = 0f,
						y = 1f,
						z = 0f
					};
					item.up = vec;
					vec = new Vec3
					{
						x = 0f,
						y = 0f,
						z = 1f
					};
					item.forward = vec;
					vec = new Vec3
					{
						x = 0f,
						y = 0f,
						z = 0f
					};
					item.trans = vec;
					list.Add(item);
				}
			}
		}

		public MeshSubsetCategoryFlags GetSectionCategories(int index)
		{
			MeshSubsetCategoryFlags meshSubsetCategoryFlags = (MeshSubsetCategoryFlags)0;
			for (int i = 0; i < subsetCategories.Count; i++)
			{
				if (subsetCategories[i].Contains((byte)index))
				{
					meshSubsetCategoryFlags = (MeshSubsetCategoryFlags)((int)meshSubsetCategoryFlags | (1 << i));
				}
			}
			return meshSubsetCategoryFlags;
		}

		public void ClearCategories()
		{
			for (int i = 0; i < subsetCategories.Count; i++)
			{
				subsetCategories[i].Clear();
			}
		}

		public void ReadInlineData(NativeReader reader)
		{
			if (chunkId == Guid.Empty)
			{
				inlineData = reader.ReadBytes((int)(vertexBufferSize + indexBufferSize));
				while (reader.Position % 16 != 0L)
				{
					reader.Position++;
				}
			}
		}

		public void SetInlineData(byte[] inBuffer)
		{
			inlineData = inBuffer;
			chunkId = Guid.Empty;
			flags = MeshLayoutFlags.Inline;
		}

		private int GetSectionIndex(MeshSetSection inSection)
		{
			int result = -1;
			for (int i = 0; i < sections.Count; i++)
			{
				if (sections[i] == inSection)
				{
					result = i;
					break;
				}
			}
			return result;
		}
	}
}
