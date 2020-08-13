using FrostySdk.IO;
using FrostySdk.Managers;
using System.Collections.Generic;

namespace FrostySdk.Resources
{
	public class MeshSetSection
	{
		public TangentSpaceCompressionType TangentSpaceCompressionType;

		private long offset1;

		private long offset2;

		private string materialName;

		private int materialId;

		private uint unknownInt1;

		private uint primitiveCount;

		private uint startIndex;

		private uint vertexOffset;

		private uint vertexCount;

		private GeometryDeclarationDesc[] geometryDeclarationDesc = new GeometryDeclarationDesc[2];

		private byte vertexStride;

		private PrimitiveType primitiveType;

		private byte bonesPerVertex;

		private ushort boneCount;

		private bool hasUnknown;

		private bool hasUnknown2;

		private bool hasUnknown3;

		private List<ushort> boneList = new List<ushort>();

		private List<float> texCoordRatios = new List<float>();

		private byte[] unknownData;

		public string Name => materialName;

		public int MaterialId => materialId;

		public uint UnknownInt => unknownInt1;

		public uint PrimitiveCount
		{
			get
			{
				return primitiveCount;
			}
			set
			{
				primitiveCount = value;
			}
		}

		public uint StartIndex
		{
			get
			{
				return startIndex;
			}
			set
			{
				startIndex = value;
			}
		}

		public uint VertexOffset
		{
			get
			{
				return vertexOffset;
			}
			set
			{
				vertexOffset = value;
			}
		}

		public uint VertexCount
		{
			get
			{
				return vertexCount;
			}
			set
			{
				vertexCount = value;
			}
		}

		public GeometryDeclarationDesc[] GeometryDeclDesc => geometryDeclarationDesc;

		public List<ushort> BoneList => boneList;

		public uint VertexStride => vertexStride;

		public PrimitiveType PrimitiveType => primitiveType;

		public uint BoneCount => boneCount;

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

		public static int Size
		{
			get
			{
				switch (ProfilesLibrary.DataVersion)
				{
				case 20170321:
					return 208;
				case 20151117:
					return 192;
				case 20160607:
					return 304;
				case 20141118:
					return 192;
				case 20141117:
					return 192;
				default:
					return 0;
				}
			}
		}

		public int DeclCount
		{
			get
			{
				switch (ProfilesLibrary.DataVersion)
				{
				case 20160607:
				case 20161021:
				case 20171117:
				case 20180628:
					return 2;
				default:
					return 1;
				}
			}
		}

		public bool HasUnknown => hasUnknown;

		public bool HasUnknown2 => hasUnknown2;

		public bool HasUnknown3 => hasUnknown3;

		public MeshSetSection(string inName, int inMaterialId)
		{
			materialName = inName;
			materialId = inMaterialId;
			int num = 44;
			if (ProfilesLibrary.DataVersion == 20170321)
			{
				num = 48;
			}
			else if (ProfilesLibrary.DataVersion != 20141118 && ProfilesLibrary.DataVersion != 20141117 && ProfilesLibrary.DataVersion != 20131115)
			{
				num = 36;
			}
			unknownData = new byte[num];
			texCoordRatios = new List<float>
			{
				1f,
				1f,
				1f,
				1f,
				1f,
				1f
			};
			primitiveType = PrimitiveType.PrimitiveType_TriangleList;
		}

		public MeshSetSection(NativeReader reader, AssetManager am)
		{
			offset1 = reader.ReadLong();
			if (ProfilesLibrary.DataVersion == 20160607 || ProfilesLibrary.DataVersion == 20161021 || ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
			{
				offset2 = reader.ReadLong();
			}
			long position = reader.ReadLong();
			materialId = reader.ReadInt();
			if (ProfilesLibrary.DataVersion != 20141118 && ProfilesLibrary.DataVersion != 20141117 && ProfilesLibrary.DataVersion != 20140225 && ProfilesLibrary.DataVersion != 20131115 && ProfilesLibrary.DataVersion != 20171210)
			{
				unknownInt1 = reader.ReadUInt();
			}
			primitiveCount = reader.ReadUInt();
			startIndex = reader.ReadUInt();
			vertexOffset = reader.ReadUInt();
			vertexCount = reader.ReadUInt();
			vertexStride = reader.ReadByte();
			primitiveType = (PrimitiveType)reader.ReadByte();
			bonesPerVertex = reader.ReadByte();
			boneCount = reader.ReadByte();
			if (ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190729 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905)
			{
				boneCount = (byte)reader.ReadUInt();
			}
			if (ProfilesLibrary.DataVersion == 20160607 || ProfilesLibrary.DataVersion == 20161021 || ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
			{
				reader.ReadUInt();
				reader.ReadUInt();
				reader.ReadUInt();
				if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
				{
					bonesPerVertex = reader.ReadByte();
					reader.ReadByte();
					boneCount = reader.ReadUShort();
				}
				else
				{
					bonesPerVertex = reader.ReadByte();
					boneCount = reader.ReadUShort();
					reader.ReadByte();
				}
			}
			long position2 = reader.ReadLong();
			if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20171110 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190729 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905)
			{
				reader.ReadULong();
			}
			if (ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807)
			{
				if (ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807)
				{
					reader.ReadLong();
				}
				if (reader.ReadLong() != 0L)
				{
					hasUnknown = true;
				}
				if (reader.ReadLong() != 0L)
				{
					hasUnknown2 = true;
				}
				if (reader.ReadLong() != 0L)
				{
					hasUnknown3 = true;
				}
			}
			for (int i = 0; i < DeclCount; i++)
			{
				geometryDeclarationDesc[i].Elements = new GeometryDeclarationDesc.Element[GeometryDeclarationDesc.MaxElements];
				geometryDeclarationDesc[i].Streams = new GeometryDeclarationDesc.Stream[GeometryDeclarationDesc.MaxStreams];
				for (int j = 0; j < GeometryDeclarationDesc.MaxElements; j++)
				{
					GeometryDeclarationDesc.Element element = new GeometryDeclarationDesc.Element
					{
						Usage = (VertexElementUsage)reader.ReadByte(),
						Format = (VertexElementFormat)reader.ReadByte(),
						Offset = reader.ReadByte(),
						StreamIndex = reader.ReadByte()
					};
					geometryDeclarationDesc[i].Elements[j] = element;
				}
				for (int k = 0; k < GeometryDeclarationDesc.MaxStreams; k++)
				{
					GeometryDeclarationDesc.Stream stream = new GeometryDeclarationDesc.Stream
					{
						VertexStride = reader.ReadByte(),
						Classification = (VertexElementClassification)reader.ReadByte()
					};
					geometryDeclarationDesc[i].Streams[k] = stream;
				}
				geometryDeclarationDesc[i].ElementCount = reader.ReadByte();
				geometryDeclarationDesc[i].StreamCount = reader.ReadByte();
				reader.ReadBytes(2);
			}
			for (int l = 0; l < 6; l++)
			{
				texCoordRatios.Add(reader.ReadFloat());
			}
			int num = 0;
			switch (ProfilesLibrary.DataVersion)
			{
			case 20170321:
				num = 48;
				break;
			case 20150223:
			case 20151103:
			case 20171110:
				num = 40;
				break;
			case 20151117:
			case 20160607:
			case 20160927:
			case 20161021:
			case 20180628:
			case 20180914:
			case 20181207:
			case 20190729:
			case 20190905:
			case 20190911:
				num = 36;
				break;
			default:
				num = 44;
				break;
			}
			unknownData = reader.ReadBytes(num);
			long position3 = reader.Position;
			reader.Position = position2;
			for (int m = 0; m < boneCount; m++)
			{
				BoneList.Add(reader.ReadUShort());
			}
			reader.Position = position;
			materialName = reader.ReadNullTerminatedString();
			reader.Position = position3;
		}

		public void SetVertexElements(List<GeometryDeclarationDesc.Element> inVertexElements)
		{
			for (int i = 0; i < DeclCount; i++)
			{
				vertexStride = 0;
				geometryDeclarationDesc[i].Elements = new GeometryDeclarationDesc.Element[GeometryDeclarationDesc.MaxElements];
				for (int j = 0; j < GeometryDeclarationDesc.MaxElements; j++)
				{
					geometryDeclarationDesc[i].Elements[j].Offset = byte.MaxValue;
					if (j < inVertexElements.Count)
					{
						GeometryDeclarationDesc.Element element = inVertexElements[j];
						element.Offset = vertexStride;
						geometryDeclarationDesc[i].Elements[j] = element;
						geometryDeclarationDesc[i].ElementCount++;
						vertexStride += (byte)element.Size;
					}
				}
				geometryDeclarationDesc[i].Streams = new GeometryDeclarationDesc.Stream[GeometryDeclarationDesc.MaxStreams];
				geometryDeclarationDesc[i].Streams[0].VertexStride = vertexStride;
				geometryDeclarationDesc[i].StreamCount = 1;
				if (i == 1)
				{
					geometryDeclarationDesc[i].StreamCount = 2;
				}
			}
		}
	}
}
