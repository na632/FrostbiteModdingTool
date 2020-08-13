using FrostySdk.IO;
using FrostySdk.Managers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace FrostySdk.Resources
{
	public class MeshSet
	{
		private TangentSpaceCompressionType tangentSpaceCompressionType;

		private AxisAlignedBox boundingBox;

		private string fullname;

		private string name;

		private uint nameHash;

		private MeshType meshType;

		private MeshLayoutFlags flags;

		private List<MeshSetLod> lods = new List<MeshSetLod>();

		private List<uint> unknownUInts = new List<uint>();

		private List<ushort> unknownUShorts = new List<ushort>();

		private List<long> unknownOffsets = new List<long>();

		private byte[] meta = new byte[20];

		private ushort boneCount;

		private List<ushort> boneIndices = new List<ushort>();

		private List<AxisAlignedBox> boneBoundingBoxes = new List<AxisAlignedBox>();

		private List<AxisAlignedBox> partBoundingBoxes = new List<AxisAlignedBox>();

		private List<LinearTransform> partTransforms = new List<LinearTransform>();

		public TangentSpaceCompressionType TangentSpaceCompressionType
		{
			get
			{
				return tangentSpaceCompressionType;
			}
			set
			{
				tangentSpaceCompressionType = value;
				foreach (MeshSetLod lod in lods)
				{
					foreach (MeshSetSection section in lod.Sections)
					{
						section.TangentSpaceCompressionType = value;
					}
				}
			}
		}

		public AxisAlignedBox BoundingBox => boundingBox;

		public List<MeshSetLod> Lods => lods;

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

		public MeshLayoutFlags Flags => flags;

		public string FullName => fullname;

		public string Name => name;

		public int HeaderSize
		{
			get
			{
				switch (ProfilesLibrary.DataVersion)
				{
				case 20170321:
					if (meshType != MeshType.MeshType_Skinned)
					{
						return 128;
					}
					return 144;
				case 20151117:
					return 144;
				case 20160607:
					return 144;
				case 20141118:
					return 112;
				case 20141117:
					return 112;
				default:
					return 0;
				}
			}
		}

		public int MaxLodCount
		{
			get
			{
				int dataVersion = ProfilesLibrary.DataVersion;
				if (dataVersion == 20131115 || dataVersion == 20140225 || (uint)(dataVersion - 20141117) <= 1u)
				{
					return 6;
				}
				return 7;
			}
		}

		public byte[] Meta => meta;

		public MeshSet(Stream stream, AssetManager am)
		{
			using (NativeReader nativeReader = new NativeReader(stream))
			{
				boundingBox = nativeReader.ReadAxisAlignedBox();
				long[] array = new long[MaxLodCount];
				for (int i = 0; i < MaxLodCount; i++)
				{
					array[i] = nativeReader.ReadLong();
				}
				long position = nativeReader.ReadLong();
				long position2 = nativeReader.ReadLong();
				nameHash = nativeReader.ReadUInt();
				meshType = (MeshType)nativeReader.ReadUInt();
				flags = (MeshLayoutFlags)nativeReader.ReadUInt();
				switch (ProfilesLibrary.DataVersion)
				{
				case 20160927:
					unknownUInts.Add(nativeReader.ReadUInt());
					unknownUInts.Add(nativeReader.ReadUInt());
					break;
				case 20171210:
					unknownUInts.Add(nativeReader.ReadUShort());
					break;
				case 20170929:
				case 20180807:
				case 20180914:
				case 20190911:
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
				{
					for (int j = 0; j < 7; j++)
					{
						unknownUInts.Add(nativeReader.ReadUInt());
					}
					break;
				}
				case 20190729:
				{
					for (int l = 0; l < 8; l++)
					{
						unknownUInts.Add(nativeReader.ReadUInt());
					}
					unknownUInts.Add(nativeReader.ReadUShort());
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
				ushort num = nativeReader.ReadUShort();
				nativeReader.ReadUShort();
				ushort num2 = 0;
				if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20171110)
				{
					if (meshType == MeshType.MeshType_Skinned)
					{
						boneCount = nativeReader.ReadUShort();
						num2 = nativeReader.ReadUShort();
						if (boneCount != 0)
						{
							nativeReader.ReadLong();
							nativeReader.ReadLong();
						}
					}
					else if (meshType == MeshType.MeshType_Composite)
					{
						num2 = nativeReader.ReadUShort();
						boneCount = nativeReader.ReadUShort();
						long num3 = nativeReader.ReadLong();
						long num4 = nativeReader.ReadLong();
						long position3 = nativeReader.Position;
						if (num3 != 0L)
						{
							nativeReader.Position = num3;
							for (int n = 0; n < num2; n++)
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
				}
				if (ProfilesLibrary.DataVersion == 20170321)
				{
					num2 = nativeReader.ReadUShort();
					boneCount = nativeReader.ReadUShort();
					if (num2 != 0)
					{
						long num6 = nativeReader.ReadLong();
						long num7 = nativeReader.ReadLong();
						long position4 = nativeReader.Position;
						if (meshType == MeshType.MeshType_Skinned)
						{
							nativeReader.Position = num6;
							for (int num8 = 0; num8 < boneCount; num8++)
							{
								boneIndices.Add(nativeReader.ReadUShort());
							}
							nativeReader.Position = num7;
							for (int num9 = 0; num9 < boneCount; num9++)
							{
								boneBoundingBoxes.Add(nativeReader.ReadAxisAlignedBox());
							}
							boneCount = num2;
						}
						else if (meshType == MeshType.MeshType_Composite)
						{
							if (num6 != 0L)
							{
								nativeReader.Position = num6;
								for (int num10 = 0; num10 < num2; num10++)
								{
									partTransforms.Add(nativeReader.ReadLinearTransform());
								}
							}
							if (num7 != 0L)
							{
								nativeReader.Position = num7;
								for (int num11 = 0; num11 < num2; num11++)
								{
									partBoundingBoxes.Add(nativeReader.ReadAxisAlignedBox());
								}
							}
						}
						nativeReader.Position = position4;
					}
				}
				else if (ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20190911)
				{
					if ((meshType == MeshType.MeshType_Skinned || meshType == MeshType.MeshType_Composite) && array[0] != 128)
					{
						num2 = nativeReader.ReadUShort();
						boneCount = nativeReader.ReadUShort();
						if (meshType == MeshType.MeshType_Skinned && boneCount != 0)
						{
							nativeReader.ReadLong();
							nativeReader.ReadLong();
						}
						else if (meshType == MeshType.MeshType_Composite && num2 != 0)
						{
							nativeReader.ReadLong();
							nativeReader.ReadLong();
							_ = nativeReader.Position;
						}
					}
				}
				else if (ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190905)
				{
					num2 = nativeReader.ReadUShort();
					boneCount = nativeReader.ReadUShort();
					if (meshType == MeshType.MeshType_Skinned && boneCount != 0)
					{
						nativeReader.ReadLong();
						nativeReader.ReadLong();
					}
					else if (meshType == MeshType.MeshType_Composite && num2 != 0)
					{
						nativeReader.ReadLong();
						nativeReader.ReadLong();
						_ = nativeReader.Position;
					}
				}
				else if (ProfilesLibrary.DataVersion == 20190729 && (meshType == MeshType.MeshType_Skinned || meshType == MeshType.MeshType_Composite))
				{
					num2 = nativeReader.ReadUShort();
					boneCount = (ushort)nativeReader.ReadUInt();
					if (meshType == MeshType.MeshType_Skinned && boneCount != 0)
					{
						nativeReader.ReadLong();
						nativeReader.ReadLong();
					}
					else if (meshType == MeshType.MeshType_Composite && num2 != 0)
					{
						nativeReader.ReadLong();
						nativeReader.ReadLong();
						_ = nativeReader.Position;
					}
				}
				nativeReader.Pad(16);
				if (ProfilesLibrary.DataVersion == 20180628)
				{
					nativeReader.ReadBytes(16);
				}
				for (int num12 = 0; num12 < num; num12++)
				{
					lods.Add(new MeshSetLod(nativeReader, am));
					if (ProfilesLibrary.DataVersion == 20170321 || ProfilesLibrary.DataVersion == 20171117)
					{
						lods[num12].SetParts(partTransforms, partBoundingBoxes);
					}
				}
				foreach (MeshSetLod lod in lods)
				{
					for (int num13 = 0; num13 < lod.Sections.Count; num13++)
					{
						lod.Sections[num13] = new MeshSetSection(nativeReader, am);
					}
				}
				nativeReader.Pad(16);
				nativeReader.Position = position;
				fullname = nativeReader.ReadNullTerminatedString();
				nativeReader.Position = position2;
				name = nativeReader.ReadNullTerminatedString();
				if (ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914)
				{
					nativeReader.Pad(16);
					foreach (MeshSetLod lod2 in lods)
					{
						foreach (MeshSetSection section in lod2.Sections)
						{
							if (section.HasUnknown)
							{
								nativeReader.Position += section.VertexCount * 4;
							}
						}
					}
					nativeReader.Pad(16);
					List<int> list = new List<int>();
					foreach (MeshSetLod lod3 in lods)
					{
						foreach (MeshSetSection section2 in lod3.Sections)
						{
							if (section2.HasUnknown2)
							{
								int num14 = 0;
								for (int num15 = 0; num15 < section2.VertexCount; num15++)
								{
									num14 += nativeReader.ReadUShort();
								}
								list.Add(num14);
							}
						}
					}
					nativeReader.Pad(16);
					foreach (MeshSetLod lod4 in lods)
					{
						foreach (MeshSetSection section3 in lod4.Sections)
						{
							if (section3.HasUnknown3)
							{
								nativeReader.Position += list[0] * 2;
								list.RemoveAt(0);
							}
						}
					}
				}
				nativeReader.Pad(16);
				foreach (MeshSetLod lod5 in lods)
				{
					for (int num16 = 0; num16 < lod5.CategorySubsetIndices.Count; num16++)
					{
						for (int num17 = 0; num17 < lod5.CategorySubsetIndices[num16].Count; num17++)
						{
							lod5.CategorySubsetIndices[num16][num17] = nativeReader.ReadByte();
						}
					}
				}
				nativeReader.Pad(16);
				foreach (MeshSetLod lod6 in lods)
				{
					nativeReader.Position += lod6.AdjacencyBufferSize;
				}
				nativeReader.Pad(16);
				foreach (MeshSetLod lod7 in lods)
				{
					if (ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190729 || ProfilesLibrary.DataVersion == 20190911)
					{
						if (lod7.Type == MeshType.MeshType_Skinned)
						{
							nativeReader.Position += lod7.BoneCount * 4;
						}
						else if (lod7.Type == MeshType.MeshType_Composite)
						{
							nativeReader.Position += lod7.Sections.Count * 24;
						}
					}
					else if (lod7.Type == MeshType.MeshType_Skinned)
					{
						nativeReader.Position += lod7.BoneCount * 4 + lod7.BoneCount * 4;
					}
					else if (lod7.Type == MeshType.MeshType_Composite)
					{
						nativeReader.Position += lod7.PartCount * Marshal.SizeOf<AxisAlignedBox>() + lod7.PartCount * Marshal.SizeOf<LinearTransform>();
						nativeReader.Position += lod7.Sections.Count * 24;
					}
				}
				int num18 = boneCount;
				if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20171110)
				{
					num18 = num2;
				}
				if (meshType == MeshType.MeshType_Skinned)
				{
					nativeReader.Pad(16);
					for (int num19 = 0; num19 < num18; num19++)
					{
						boneIndices.Add(nativeReader.ReadUShort());
					}
					nativeReader.Pad(16);
					for (int num20 = 0; num20 < num18; num20++)
					{
						boneBoundingBoxes.Add(nativeReader.ReadAxisAlignedBox());
					}
				}
				else if (meshType == MeshType.MeshType_Composite)
				{
					if (ProfilesLibrary.DataVersion == 20170321)
					{
						nativeReader.Pad(16);
						for (int num21 = 0; num21 < num2; num21++)
						{
							partTransforms.Add(nativeReader.ReadLinearTransform());
						}
					}
					nativeReader.Pad(16);
					for (int num22 = 0; num22 < num2; num22++)
					{
						partBoundingBoxes.Add(nativeReader.ReadAxisAlignedBox());
					}
				}
				nativeReader.Pad(16);
				foreach (MeshSetLod lod8 in lods)
				{
					lod8.ReadInlineData(nativeReader);
				}
			}
		}
	}
}
