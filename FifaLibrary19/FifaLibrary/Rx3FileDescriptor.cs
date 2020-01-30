using System.IO;

namespace FifaLibrary
{
	public class Rx3FileDescriptor
	{
		private uint m_Signature;

		private int m_FileOffset;

		private int m_FileSize;

		private int m_Unknown_0c;

		private bool m_SwapEndian;

		public uint Signature => m_Signature;

		public int Offset
		{
			get
			{
				return m_FileOffset;
			}
			set
			{
				m_FileOffset = value;
			}
		}

		public int Size
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

		public Rx3FileDescriptor(BinaryReader r, bool swapEndian)
		{
			m_SwapEndian = swapEndian;
			Load(r);
		}

		public bool Load(BinaryReader r)
		{
			if (m_SwapEndian)
			{
				m_Signature = FifaUtil.SwapEndian(r.ReadUInt32());
				m_FileOffset = FifaUtil.SwapEndian(r.ReadInt32());
				m_FileSize = FifaUtil.SwapEndian(r.ReadInt32());
				m_Unknown_0c = FifaUtil.SwapEndian(r.ReadInt32());
			}
			else
			{
				m_Signature = r.ReadUInt32();
				m_FileOffset = r.ReadInt32();
				m_FileSize = r.ReadInt32();
				m_Unknown_0c = r.ReadInt32();
			}
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			if (m_SwapEndian)
			{
				w.Write(FifaUtil.SwapEndian(m_Signature));
				w.Write(FifaUtil.SwapEndian(m_FileOffset));
				w.Write(FifaUtil.SwapEndian(m_FileSize));
				w.Write(FifaUtil.SwapEndian(m_Unknown_0c));
			}
			else
			{
				w.Write(m_Signature);
				w.Write(m_FileOffset);
				w.Write(m_FileSize);
				w.Write(m_Unknown_0c);
			}
			return true;
		}

		public bool Is3dDirectory()
		{
			return m_Signature == 582139446;
		}

		public bool IsTexture()
		{
			return m_Signature == 1879793882;
		}

		public bool IsImageDirectory()
		{
			return m_Signature == 1808827868;
		}

		public bool IsIndexStream()
		{
			return m_Signature == 5798132;
		}

		public bool IsVertexVector()
		{
			return m_Signature == 5798561;
		}
	}
}
