namespace FrostySdk.Resources
{
	public struct GeometryDeclarationDesc
	{
		public struct Element
		{
			public VertexElementUsage Usage;

			public VertexElementFormat Format;

			public byte Offset;

			public byte StreamIndex;

			public int Size
			{
				get
				{
					switch (Format)
					{
					case VertexElementFormat.None:
						return 0;
					case VertexElementFormat.Float:
						return 4;
					case VertexElementFormat.Float2:
						return 8;
					case VertexElementFormat.Float3:
						return 12;
					case VertexElementFormat.Float4:
						return 16;
					case VertexElementFormat.Half:
						return 2;
					case VertexElementFormat.Half2:
						return 4;
					case VertexElementFormat.Half3:
						return 6;
					case VertexElementFormat.Half4:
						return 8;
					case VertexElementFormat.UByteN:
						return 1;
					case VertexElementFormat.Byte4:
						return 4;
					case VertexElementFormat.Byte4N:
						return 4;
					case VertexElementFormat.UByte4:
						return 4;
					case VertexElementFormat.UByte4N:
						return 4;
					case VertexElementFormat.Short:
						return 2;
					case VertexElementFormat.Short2:
						return 4;
					case VertexElementFormat.Short3:
						return 6;
					case VertexElementFormat.Short4:
						return 8;
					case VertexElementFormat.ShortN:
						return 2;
					case VertexElementFormat.Short2N:
						return 4;
					case VertexElementFormat.Short3N:
						return 6;
					case VertexElementFormat.Short4N:
						return 8;
					case VertexElementFormat.UShort2:
						return 4;
					case VertexElementFormat.UShort4:
						return 8;
					case VertexElementFormat.UShort2N:
						return 4;
					case VertexElementFormat.UShort4N:
						return 8;
					case VertexElementFormat.Int:
						return 4;
					case VertexElementFormat.Int2:
						return 8;
					case VertexElementFormat.Int3:
						return 12;
					case VertexElementFormat.Int4:
						return 16;
					case VertexElementFormat.IntN:
						return 4;
					case VertexElementFormat.Int2N:
						return 8;
					case VertexElementFormat.Int4N:
						return 16;
					case VertexElementFormat.UInt:
						return 4;
					case VertexElementFormat.UInt2:
						return 8;
					case VertexElementFormat.UInt3:
						return 12;
					case VertexElementFormat.UInt4:
						return 16;
					case VertexElementFormat.UIntN:
						return 4;
					case VertexElementFormat.UInt2N:
						return 8;
					case VertexElementFormat.UInt4N:
						return 16;
					case VertexElementFormat.Comp3_10_10_10:
						return 4;
					case VertexElementFormat.Comp3N_10_10_10:
						return 4;
					case VertexElementFormat.UComp3_10_10_10:
						return 4;
					case VertexElementFormat.UComp3N_10_10_10:
						return 4;
					case VertexElementFormat.Comp3_11_11_10:
						return 4;
					case VertexElementFormat.Comp3N_11_11_10:
						return 4;
					case VertexElementFormat.UComp3_11_11_10:
						return 4;
					case VertexElementFormat.UComp3N_11_11_10:
						return 4;
					case VertexElementFormat.Comp4_10_10_10_2:
						return 4;
					case VertexElementFormat.Comp4N_10_10_10_2:
						return 4;
					case VertexElementFormat.UComp4_10_10_10_2:
						return 4;
					case VertexElementFormat.UComp4N_10_10_10_2:
						return 4;
					default:
						return 0;
					}
				}
			}

			public override string ToString()
			{
				return Usage.ToString();
			}
		}

		public struct Stream
		{
			public byte VertexStride;

			public VertexElementClassification Classification;
		}

		public Element[] Elements;

		public Stream[] Streams;

		public byte ElementCount;

		public byte StreamCount;

		public static int MaxElements => 16;

		public static int MaxStreams
		{
			get
			{
				if (ProfileManager.LoadedProfile.GDDMaxStreams.HasValue)
					return ProfileManager.LoadedProfile.GDDMaxStreams.Value;

				switch (ProfileManager.DataVersion)
				{
				case 20131115:
					return 4;
				case 20140225:
					return 4;
				case 20141118:
					return 4;
				case 20141117:
					return 4;
				case 20150223:
					return 4;
				case 20151103:
					return 4;
				case 20171210:
					return 4;
				case 20151117:
					return 6;
				case 20180628:
					return 8;
				case 20171117:
					return 16;
				case 20170929:
					return 16;
				case 20180807:
					return 16;
				case 20171110:
					return 16;
				case 20180914:
					return 16;
				case 20181207:
					return 16;
				case 20190729:
					return 16;
				case 20190905:
					return 16;
				case (int)ProfileManager.EGame.FIFA20: // FIFA20DataVersion
				case (int)ProfileManager.EGame.FIFA21: // FIFA21DataVersion
				case (int)ProfileManager.EGame.MADDEN21: // Madden21DataVersion
					return 16;


					default:
					return 8;
				}
			}
		}

		public uint Hash => Utils.CalcFletcher32(this);

		public static GeometryDeclarationDesc Create(Element[] elements)
		{
			GeometryDeclarationDesc result = default(GeometryDeclarationDesc);
			result.Elements = new Element[MaxElements];
			int num = 0;
			for (int i = 0; i < MaxElements; i++)
			{
				if (i < elements.Length)
				{
					result.Elements[i] = elements[i];
					result.Elements[i].Offset = (byte)num;
					num += result.Elements[i].Size;
				}
				else
				{
					result.Elements[i].Offset = byte.MaxValue;
				}
			}
			result.Streams = new Stream[MaxStreams];
			result.Streams[0].Classification = VertexElementClassification.PerVertex;
			result.Streams[0].VertexStride = (byte)num;
			result.ElementCount = (byte)elements.Length;
			result.StreamCount = 1;
			return result;
		}
	}
}
