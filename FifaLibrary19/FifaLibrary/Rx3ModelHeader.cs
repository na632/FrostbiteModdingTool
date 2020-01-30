using System.IO;

namespace FifaLibrary
{
	public class Rx3ModelHeader
	{
		private int m_Unknown_00;

		private int m_Unknown_04;

		private int m_Unknown_08;

		private int m_Unknown_0c;

		private bool m_SwapEndian;

		public Rx3ModelHeader(BinaryReader r, bool swapEndian)
		{
			m_SwapEndian = swapEndian;
			Load(r);
		}

		public bool Load(BinaryReader r)
		{
			if (m_SwapEndian)
			{
				m_Unknown_00 = FifaUtil.SwapEndian(r.ReadInt32());
				m_Unknown_04 = FifaUtil.SwapEndian(r.ReadInt32());
				m_Unknown_08 = r.ReadInt32();
				m_Unknown_0c = r.ReadInt32();
			}
			else
			{
				m_Unknown_00 = r.ReadInt32();
				m_Unknown_04 = r.ReadInt32();
				m_Unknown_08 = r.ReadInt32();
				m_Unknown_0c = r.ReadInt32();
			}
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			if (m_SwapEndian)
			{
				w.Write(FifaUtil.SwapEndian(m_Unknown_00));
				w.Write(FifaUtil.SwapEndian(m_Unknown_04));
				w.Write(m_Unknown_08);
				w.Write(m_Unknown_0c);
			}
			else
			{
				w.Write(m_Unknown_00);
				w.Write(m_Unknown_04);
				w.Write(m_Unknown_08);
				w.Write(m_Unknown_0c);
			}
			return true;
		}
	}
}
