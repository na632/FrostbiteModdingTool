namespace FrostySdk.Resources
{
	public struct VertexElement
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
}
