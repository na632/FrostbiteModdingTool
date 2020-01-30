using System.IO;

namespace FifaLibrary
{
	public class MipMap : RawImage
	{
		private int m_Unknown0;

		private int m_Unknown4;

		private int m_UnknownC;

		public MipMap(int width, int height, EImageType dxtType, bool swapEndian)
			: base(width, height, dxtType, 0)
		{
			m_SwapEndian = swapEndian;
		}

		public new bool Load(BinaryReader r)
		{
			if (m_SwapEndian)
			{
				m_Unknown0 = FifaUtil.SwapEndian(r.ReadInt32());
				m_Unknown4 = FifaUtil.SwapEndian(r.ReadInt32());
				m_Size = FifaUtil.SwapEndian(r.ReadInt32());
				m_UnknownC = FifaUtil.SwapEndian(r.ReadInt32());
				base.Load(r);
			}
			else
			{
				m_Unknown0 = r.ReadInt32();
				m_Unknown4 = r.ReadInt32();
				m_Size = r.ReadInt32();
				m_UnknownC = r.ReadInt32();
				base.Load(r);
			}
			return true;
		}

		public new bool Save(BinaryWriter w)
		{
			if (m_SwapEndian)
			{
				w.Write(FifaUtil.SwapEndian(m_Unknown0));
				w.Write(FifaUtil.SwapEndian(m_Unknown4));
				w.Write(FifaUtil.SwapEndian(m_Size));
				w.Write(FifaUtil.SwapEndian(m_UnknownC));
				base.Save(w);
			}
			else
			{
				w.Write(m_Unknown0);
				w.Write(m_Unknown4);
				w.Write(m_Size);
				w.Write(m_UnknownC);
				base.Save(w);
			}
			return true;
		}
	}
}
