using System.IO;

namespace FifaLibrary
{
	public class BhFileReference
	{
		private uint m_StartPosition;

		private int m_Size;

		private int m_UncompressedSize;

		private ulong m_Hash;

		public uint StartPosition
		{
			get
			{
				return m_StartPosition;
			}
			set
			{
				m_StartPosition = value;
			}
		}

		public int Size
		{
			get
			{
				return m_Size;
			}
			set
			{
				m_Size = value;
			}
		}

		public int UncompressedSize
		{
			get
			{
				return m_UncompressedSize;
			}
			set
			{
				m_UncompressedSize = value;
			}
		}

		public ulong Hash
		{
			get
			{
				return m_Hash;
			}
			set
			{
				m_Hash = value;
			}
		}

		public BhFileReference(uint startPosition, int size, int uncompressedSize, string name)
		{
			m_StartPosition = startPosition;
			m_Size = size;
			m_UncompressedSize = uncompressedSize;
			if (m_UncompressedSize != 0)
			{
				m_Size = (m_Size + 15 >> 4) * 16;
			}
			m_Hash = FifaUtil.ComputeBhHash(name);
		}

		public BhFileReference()
		{
		}

		public bool Load(BinaryReader r)
		{
			m_StartPosition = FifaUtil.SwapEndian(r.ReadUInt32());
			m_Size = FifaUtil.SwapEndian(r.ReadInt32());
			m_UncompressedSize = FifaUtil.SwapEndian(r.ReadInt32());
			m_Hash = FifaUtil.SwapEndian(r.ReadUInt64());
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			w.Write(FifaUtil.SwapEndian(m_StartPosition));
			w.Write(FifaUtil.SwapEndian(m_Size));
			w.Write(FifaUtil.SwapEndian(m_UncompressedSize));
			w.Write(FifaUtil.SwapEndian(m_Hash));
			return true;
		}

		public void Hide()
		{
			m_Hash = 0uL;
		}

		public bool IsHidden()
		{
			return m_Hash == 0;
		}

		public void Restore(string name)
		{
			m_Hash = FifaUtil.ComputeBhHash(name);
		}
	}
}
