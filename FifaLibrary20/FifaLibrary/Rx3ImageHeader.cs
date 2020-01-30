using System.IO;

namespace FifaLibrary
{
	public class Rx3ImageHeader
	{
		private int m_FileSize;

		private byte m_Unknown_04;

		private byte m_ImageType;

		private short m_Unknown_06;

		private short m_Width;

		private short m_Height;

		private short m_Unknown_0C;

		private short m_NMipMaps;

		private bool m_SwapEndian;

		public int FileSize
		{
			get
			{
				return m_FileSize;
			}
			set
			{
				m_FileSize = value;
			}
		}

		public EImageType ImageType
		{
			get
			{
				return (EImageType)m_ImageType;
			}
			set
			{
				m_ImageType = (byte)value;
			}
		}

		public short Width
		{
			get
			{
				return m_Width;
			}
			set
			{
				m_Width = value;
			}
		}

		public short Height
		{
			get
			{
				return m_Height;
			}
			set
			{
				m_Height = value;
			}
		}

		public short NMipMaps
		{
			get
			{
				return m_NMipMaps;
			}
			set
			{
				m_NMipMaps = value;
			}
		}

		public Rx3ImageHeader(BinaryReader r, bool swapEndian)
		{
			m_SwapEndian = swapEndian;
			Load(r);
		}

		public bool Load(BinaryReader r)
		{
			if (m_SwapEndian)
			{
				m_FileSize = FifaUtil.SwapEndian(r.ReadInt32());
				m_Unknown_04 = r.ReadByte();
				m_ImageType = r.ReadByte();
				m_Unknown_06 = FifaUtil.SwapEndian(r.ReadInt16());
				m_Width = FifaUtil.SwapEndian(r.ReadInt16());
				m_Height = FifaUtil.SwapEndian(r.ReadInt16());
				m_Unknown_0C = FifaUtil.SwapEndian(r.ReadInt16());
				m_NMipMaps = FifaUtil.SwapEndian(r.ReadInt16());
			}
			else
			{
				m_FileSize = r.ReadInt32();
				m_Unknown_04 = r.ReadByte();
				m_ImageType = r.ReadByte();
				m_Unknown_06 = r.ReadInt16();
				m_Width = r.ReadInt16();
				m_Height = r.ReadInt16();
				m_Unknown_0C = r.ReadInt16();
				m_NMipMaps = r.ReadInt16();
			}
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			if (m_SwapEndian)
			{
				w.Write(FifaUtil.SwapEndian(m_FileSize));
				w.Write(m_Unknown_04);
				w.Write(m_ImageType);
				w.Write(FifaUtil.SwapEndian(m_Unknown_06));
				w.Write(FifaUtil.SwapEndian(m_Width));
				w.Write(FifaUtil.SwapEndian(m_Height));
				w.Write(FifaUtil.SwapEndian(m_Unknown_0C));
				w.Write(FifaUtil.SwapEndian(m_NMipMaps));
			}
			else
			{
				w.Write(m_FileSize);
				w.Write(m_Unknown_04);
				w.Write(m_ImageType);
				w.Write(m_Unknown_06);
				w.Write(m_Width);
				w.Write(m_Height);
				w.Write(m_Unknown_0C);
				w.Write(m_NMipMaps);
			}
			return true;
		}
	}
}
