using Microsoft.DirectX.Direct3D;
using System.IO;

namespace FifaLibrary
{
	public class Rx3VertexArray
	{
		private int m_Size;

		private int m_nVertex;

		private int m_VertexSize;

		private int m_Unknown;

		private Rx3Vertex[] m_Vertex;

		private bool m_SwapEndian;

		public int nVertex => m_nVertex;

		public int VertexSize => m_VertexSize;

		public Rx3Vertex[] Vertexes => m_Vertex;

		public Rx3VertexArray(BinaryReader r, bool swapEndian)
		{
			m_SwapEndian = swapEndian;
			Load(r);
		}

		public bool Load(BinaryReader r)
		{
			if (m_SwapEndian)
			{
				m_Size = FifaUtil.SwapEndian(r.ReadInt32());
				m_nVertex = FifaUtil.SwapEndian(r.ReadInt32());
				m_VertexSize = FifaUtil.SwapEndian(r.ReadInt32());
				m_Unknown = r.ReadInt32();
				m_Vertex = new Rx3Vertex[m_nVertex];
				for (int i = 0; i < m_nVertex; i++)
				{
					m_Vertex[i] = new Rx3Vertex(r, m_VertexSize);
				}
			}
			else
			{
				m_Size = r.ReadInt32();
				m_nVertex = r.ReadInt32();
				m_VertexSize = r.ReadInt32();
				m_Unknown = r.ReadInt32();
				m_Vertex = new Rx3Vertex[m_nVertex];
				for (int j = 0; j < m_nVertex; j++)
				{
					m_Vertex[j] = new Rx3Vertex(r, m_VertexSize);
				}
			}
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			if (m_SwapEndian)
			{
				w.Write(FifaUtil.SwapEndian(m_Size));
				w.Write(FifaUtil.SwapEndian(m_nVertex));
				w.Write(FifaUtil.SwapEndian(m_VertexSize));
				w.Write(m_Unknown);
				for (int i = 0; i < m_nVertex; i++)
				{
					m_Vertex[i].Save(w);
				}
			}
			else
			{
				w.Write(m_Size);
				w.Write(m_nVertex);
				w.Write(m_VertexSize);
				w.Write(m_Unknown);
				for (int j = 0; j < m_nVertex; j++)
				{
					m_Vertex[j].Save(w);
				}
			}
			return true;
		}

		public bool SetVertex(CustomVertex.PositionNormalTextured[] newVertexes)
		{
			if (m_nVertex != newVertexes.Length)
			{
				return false;
			}
			for (int i = 0; i < m_nVertex; i++)
			{
				m_Vertex[i].X = newVertexes[i].X;
				m_Vertex[i].Y = newVertexes[i].Y;
				m_Vertex[i].Z = newVertexes[i].Z;
			}
			return true;
		}
	}
}
