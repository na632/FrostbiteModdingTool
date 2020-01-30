using System.IO;

namespace FifaLibrary
{
	public class Rx3Vertex
	{
		public enum EFloatType
		{
			Float16,
			Float32
		}

		private static EFloatType s_FloatType;

		private int m_VertexSize;

		private float m_X;

		private float m_Y;

		private float m_Z;

		private float m_U;

		private float m_V;

		private float[] m_Unknown = new float[20];

		public static EFloatType FloatType
		{
			get
			{
				return s_FloatType;
			}
			set
			{
				s_FloatType = value;
			}
		}

		public float X
		{
			get
			{
				return m_X;
			}
			set
			{
				m_X = value;
			}
		}

		public float Y
		{
			get
			{
				return m_Y;
			}
			set
			{
				m_Y = value;
			}
		}

		public float Z
		{
			get
			{
				return m_Z;
			}
			set
			{
				m_Z = value;
			}
		}

		public float U
		{
			get
			{
				return m_U;
			}
			set
			{
				m_U = value;
			}
		}

		public float V
		{
			get
			{
				return m_V;
			}
			set
			{
				m_V = value;
			}
		}

		public Rx3Vertex(BinaryReader r, int vertexSize)
		{
			m_VertexSize = vertexSize;
			if (m_VertexSize == 20)
			{
				Load20_ShortFloat(r);
			}
			if (m_VertexSize == 24)
			{
				Load24_ShortFloat(r);
			}
			if (m_VertexSize == 28)
			{
				Load28_ShortFloat(r);
			}
			if (m_VertexSize == 32)
			{
				if (s_FloatType == EFloatType.Float16)
				{
					Load32_ShortFloat(r);
				}
				else if (s_FloatType == EFloatType.Float32)
				{
					Load32_LongFloat(r);
				}
			}
			if (m_VertexSize == 36)
			{
				if (s_FloatType == EFloatType.Float16)
				{
					Load36_ShortFloat(r);
				}
				else if (s_FloatType == EFloatType.Float32)
				{
					Load36_LongFloat(r);
				}
			}
			if (m_VertexSize == 40)
			{
				if (s_FloatType == EFloatType.Float16)
				{
					Load40_ShortFloat(r);
				}
				else if (s_FloatType == EFloatType.Float32)
				{
					Load40_LongFloat(r);
				}
			}
			if (m_VertexSize == 44)
			{
				if (s_FloatType == EFloatType.Float16)
				{
					Load44_ShortFloat(r);
				}
				else if (s_FloatType == EFloatType.Float32)
				{
					Load44_LongFloat(r);
				}
			}
			if (m_VertexSize == 48)
			{
				if (s_FloatType == EFloatType.Float16)
				{
					Load48_ShortFloat(r);
				}
				else if (s_FloatType == EFloatType.Float32)
				{
					Load48_LongFloat(r);
				}
			}
			if (m_VertexSize == 52 && s_FloatType != 0 && s_FloatType == EFloatType.Float32)
			{
				Load52_LongFloat(r);
			}
		}

		public bool Load24_ShortFloat(BinaryReader r)
		{
			m_X = 0f - FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Y = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Z = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[5] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[6] = FifaUtil.ConvertToFloat(r.ReadInt16());
			return true;
		}

		public bool Load20_ShortFloat(BinaryReader r)
		{
			m_X = 0f - FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Y = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Z = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			return true;
		}

		public bool Load28_ShortFloat(BinaryReader r)
		{
			m_X = 0f - FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Y = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Z = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[5] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[6] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[7] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[8] = FifaUtil.ConvertToFloat(r.ReadInt16());
			return true;
		}

		public bool Load32_ShortFloat(BinaryReader r)
		{
			m_X = 0f - FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Y = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Z = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[5] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[6] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[7] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[8] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[9] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[10] = FifaUtil.ConvertToFloat(r.ReadInt16());
			return true;
		}

		public bool Load32_LongFloat(BinaryReader r)
		{
			m_X = 0f - r.ReadSingle();
			m_Y = r.ReadSingle();
			m_Z = r.ReadSingle();
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[5] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[6] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[7] = FifaUtil.ConvertToFloat(r.ReadInt16());
			return true;
		}

		public bool Load36_ShortFloat(BinaryReader r)
		{
			m_X = 0f - FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Y = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Z = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[5] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[6] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[7] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[8] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[9] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[10] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[11] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[12] = FifaUtil.ConvertToFloat(r.ReadInt16());
			return true;
		}

		public bool Load36_LongFloat(BinaryReader r)
		{
			m_X = 0f - r.ReadSingle();
			m_Y = r.ReadSingle();
			m_Z = r.ReadSingle();
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = r.ReadSingle();
			m_Unknown[5] = r.ReadSingle();
			m_Unknown[6] = r.ReadSingle();
			return true;
		}

		public bool Load40_ShortFloat(BinaryReader r)
		{
			m_X = 0f - FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Y = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Z = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[5] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[6] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[7] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[8] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[9] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[10] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[11] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[12] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[13] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[14] = FifaUtil.ConvertToFloat(r.ReadInt16());
			return true;
		}

		public bool Load40_LongFloat(BinaryReader r)
		{
			m_X = 0f - r.ReadSingle();
			m_Y = r.ReadSingle();
			m_Z = r.ReadSingle();
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = r.ReadSingle();
			m_Unknown[5] = r.ReadSingle();
			m_Unknown[6] = r.ReadSingle();
			m_Unknown[7] = r.ReadSingle();
			return true;
		}

		public bool Load44_ShortFloat(BinaryReader r)
		{
			m_X = 0f - FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Y = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Z = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[5] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[6] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[7] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[8] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[9] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[10] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[11] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[12] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[13] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[14] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[15] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[16] = FifaUtil.ConvertToFloat(r.ReadInt16());
			return true;
		}

		public bool Load44_LongFloat(BinaryReader r)
		{
			m_X = 0f - r.ReadSingle();
			m_Y = r.ReadSingle();
			m_Z = r.ReadSingle();
			m_Unknown[0] = r.ReadSingle();
			m_Unknown[1] = r.ReadSingle();
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = r.ReadSingle();
			m_Unknown[3] = r.ReadSingle();
			m_Unknown[4] = r.ReadSingle();
			m_Unknown[5] = r.ReadSingle();
			m_Unknown[6] = r.ReadSingle();
			return true;
		}

		public bool Load48_ShortFloat(BinaryReader r)
		{
			m_X = 0f - FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Y = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Z = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[4] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[5] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[6] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[7] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[8] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[9] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[10] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[11] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[12] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[13] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[14] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[15] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[16] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[17] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[18] = FifaUtil.ConvertToFloat(r.ReadInt16());
			return true;
		}

		public bool Load48_LongFloat(BinaryReader r)
		{
			m_X = 0f - r.ReadSingle();
			m_Y = r.ReadSingle();
			m_Z = r.ReadSingle();
			m_Unknown[0] = r.ReadSingle();
			m_Unknown[1] = r.ReadSingle();
			m_U = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_V = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = r.ReadSingle();
			m_Unknown[3] = r.ReadSingle();
			m_Unknown[4] = r.ReadSingle();
			m_Unknown[5] = r.ReadSingle();
			m_Unknown[6] = r.ReadSingle();
			m_Unknown[7] = r.ReadSingle();
			return true;
		}

		public bool Load52_LongFloat(BinaryReader r)
		{
			m_X = 0f - r.ReadSingle();
			m_Y = r.ReadSingle();
			m_Z = r.ReadSingle();
			m_Unknown[0] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[1] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[2] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[3] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_U = (m_Unknown[4] = FifaUtil.ConvertToFloat(r.ReadInt16()));
			m_V = (m_Unknown[5] = FifaUtil.ConvertToFloat(r.ReadInt16()));
			m_Unknown[6] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[7] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[8] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[9] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[10] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[11] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[12] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[13] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[14] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[15] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[16] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[17] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[18] = FifaUtil.ConvertToFloat(r.ReadInt16());
			m_Unknown[19] = FifaUtil.ConvertToFloat(r.ReadInt16());
			if (!(m_U < 0f) && !(m_U > 1f) && !(m_V < 0f))
			{
				_ = m_V;
				_ = 1f;
			}
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			if (m_VertexSize == 20)
			{
				Save20_ShortFloat(w);
			}
			if (m_VertexSize == 32)
			{
				if (s_FloatType == EFloatType.Float16)
				{
					Save32_ShortFloat(w);
				}
				else if (s_FloatType == EFloatType.Float32)
				{
					Save32_LongFloat(w);
				}
			}
			if (m_VertexSize == 36)
			{
				if (s_FloatType == EFloatType.Float16)
				{
					Save36_ShortFloat(w);
				}
				else if (s_FloatType == EFloatType.Float32)
				{
					Save36_LongFloat(w);
				}
			}
			if (m_VertexSize == 40)
			{
				if (s_FloatType == EFloatType.Float16)
				{
					Save40_ShortFloat(w);
				}
				else if (s_FloatType == EFloatType.Float32)
				{
					Save40_LongFloat(w);
				}
			}
			if (m_VertexSize == 44)
			{
				if (s_FloatType == EFloatType.Float16)
				{
					Save44_ShortFloat(w);
				}
				else if (s_FloatType == EFloatType.Float32)
				{
					Save44_LongFloat(w);
				}
			}
			if (m_VertexSize == 48)
			{
				if (s_FloatType == EFloatType.Float16)
				{
					Save48_ShortFloat(w);
				}
				else if (s_FloatType == EFloatType.Float32)
				{
					Save48_LongFloat(w);
				}
			}
			if (m_VertexSize == 52)
			{
				Save52_LongFloat(w);
			}
			return true;
		}

		public bool Save20_ShortFloat(BinaryWriter w)
		{
			w.Write(FifaUtil.ConvertFloat16ToShort(0f - m_X));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Y));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Z));
			w.Seek(14, SeekOrigin.Current);
			return true;
		}

		public bool Save32_ShortFloat(BinaryWriter w)
		{
			w.Write(FifaUtil.ConvertFloat16ToShort(0f - m_X));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Y));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Z));
			w.Seek(26, SeekOrigin.Current);
			return true;
		}

		public bool Save32_LongFloat(BinaryWriter w)
		{
			w.Write(0f - m_X);
			w.Write(m_Y);
			w.Write(m_Z);
			w.Seek(20, SeekOrigin.Current);
			return true;
		}

		public bool Save36_ShortFloat(BinaryWriter w)
		{
			w.Write(FifaUtil.ConvertFloat16ToShort(0f - m_X));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Y));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Z));
			w.Seek(30, SeekOrigin.Current);
			return true;
		}

		public bool Save36_LongFloat(BinaryWriter w)
		{
			w.Write(0f - m_X);
			w.Write(m_Y);
			w.Write(m_Z);
			w.Seek(24, SeekOrigin.Current);
			return true;
		}

		public bool Save40_ShortFloat(BinaryWriter w)
		{
			w.Write(FifaUtil.ConvertFloat16ToShort(0f - m_X));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Y));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Z));
			w.Seek(34, SeekOrigin.Current);
			return true;
		}

		public bool Save40_LongFloat(BinaryWriter w)
		{
			w.Write(m_X);
			w.Write(m_Y);
			w.Write(m_Z);
			w.Seek(28, SeekOrigin.Current);
			return true;
		}

		public bool Save44_ShortFloat(BinaryWriter w)
		{
			w.Write(FifaUtil.ConvertFloat16ToShort(0f - m_X));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Y));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Z));
			w.Seek(38, SeekOrigin.Current);
			return true;
		}

		public bool Save44_LongFloat(BinaryWriter w)
		{
			w.Write(0f - m_X);
			w.Write(m_Y);
			w.Write(m_Z);
			w.Seek(32, SeekOrigin.Current);
			return true;
		}

		public bool Save48_ShortFloat(BinaryWriter w)
		{
			w.Write(FifaUtil.ConvertFloat16ToShort(0f - m_X));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Y));
			w.Write(FifaUtil.ConvertFloat16ToShort(m_Z));
			w.Seek(42, SeekOrigin.Current);
			return true;
		}

		public bool Save48_LongFloat(BinaryWriter w)
		{
			w.Write(0f - m_X);
			w.Write(m_Y);
			w.Write(m_Z);
			w.Seek(36, SeekOrigin.Current);
			return true;
		}

		public bool Save52_LongFloat(BinaryWriter w)
		{
			w.Write(0f - m_X);
			w.Write(m_Y);
			w.Write(m_Z);
			w.Seek(40, SeekOrigin.Current);
			return true;
		}
	}
}
