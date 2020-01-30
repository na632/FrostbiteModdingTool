using System.IO;

namespace FifaLibrary
{
	public class Rx3Header
	{
		private string m_Signature;

		private int m_Unknown_04;

		private int m_SizeOf_;

		private int m_NFiles;

		private bool m_SwapEndian;

		public int SizeOf_
		{
			get
			{
				return m_SizeOf_;
			}
			set
			{
				m_SizeOf_ = value;
			}
		}

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

		public Rx3Header(BinaryReader r, bool swapEndian)
		{
			m_SwapEndian = swapEndian;
			Load(r);
		}

		public bool Load(BinaryReader r)
		{
			m_Signature = new string(r.ReadChars(4));
			if (m_SwapEndian)
			{
				if (!(m_Signature == "RX3b"))
				{
					return false;
				}
				m_Unknown_04 = FifaUtil.SwapEndian(r.ReadInt32());
				m_SizeOf_ = FifaUtil.SwapEndian(r.ReadInt32());
				m_NFiles = FifaUtil.SwapEndian(r.ReadInt32());
			}
			else
			{
				if (!(m_Signature == "RX3l"))
				{
					return false;
				}
				m_Unknown_04 = r.ReadInt32();
				m_SizeOf_ = r.ReadInt32();
				m_NFiles = r.ReadInt32();
			}
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			if (m_SwapEndian)
			{
				w.Write('R');
				w.Write('X');
				w.Write('3');
				w.Write('b');
				w.Write(FifaUtil.SwapEndian(m_Unknown_04));
				w.Write(FifaUtil.SwapEndian(m_SizeOf_));
				w.Write(FifaUtil.SwapEndian(m_NFiles));
			}
			else
			{
				w.Write('R');
				w.Write('X');
				w.Write('3');
				w.Write('l');
				w.Write(m_Unknown_04);
				w.Write(m_SizeOf_);
				w.Write(m_NFiles);
			}
			return true;
		}
	}
}
