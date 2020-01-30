using System.IO;

namespace FifaLibrary
{
	public class Rx3ImageDirectory
	{
		private int m_NFiles;

		private byte[] m_Padding;

		private Rx3ImageHeader[] m_Rx3ImageHeader;

		private bool m_SwapEndian;

		public int NFiles
		{
			get
			{
				return m_NFiles;
			}
			set
			{
				m_NFiles = value;
			}
		}

		public Rx3ImageHeader[] Rx3ImageHeader
		{
			get
			{
				return m_Rx3ImageHeader;
			}
			set
			{
				m_Rx3ImageHeader = value;
			}
		}

		public Rx3ImageDirectory(BinaryReader r, bool swapEndian)
		{
			m_SwapEndian = swapEndian;
			Load(r);
		}

		public bool Load(BinaryReader r)
		{
			if (m_SwapEndian)
			{
				m_NFiles = FifaUtil.SwapEndian(r.ReadInt32());
				m_Padding = r.ReadBytes(12);
				m_Rx3ImageHeader = new Rx3ImageHeader[m_NFiles];
				for (int i = 0; i < m_NFiles; i++)
				{
					m_Rx3ImageHeader[i] = new Rx3ImageHeader(r, m_SwapEndian);
				}
			}
			else
			{
				m_NFiles = r.ReadInt32();
				m_Padding = r.ReadBytes(12);
				m_Rx3ImageHeader = new Rx3ImageHeader[m_NFiles];
				for (int j = 0; j < m_NFiles; j++)
				{
					m_Rx3ImageHeader[j] = new Rx3ImageHeader(r, m_SwapEndian);
				}
			}
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			if (m_SwapEndian)
			{
				w.Write(FifaUtil.SwapEndian(m_NFiles));
				w.Write(m_Padding);
				for (int i = 0; i < m_NFiles; i++)
				{
					m_Rx3ImageHeader[i].Save(w);
				}
			}
			else
			{
				w.Write(m_NFiles);
				w.Write(m_Padding);
				for (int j = 0; j < m_NFiles; j++)
				{
					m_Rx3ImageHeader[j].Save(w);
				}
			}
			return true;
		}
	}
}
