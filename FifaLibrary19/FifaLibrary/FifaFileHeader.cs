using System.IO;

namespace FifaLibrary
{
	public class FifaFileHeader
	{
		private FifaBigFile m_BigFile;

		private uint m_StartPosition;

		private int m_Size;

		private string m_Name;

		public FifaBigFile BigFile => m_BigFile;

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

		public string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				m_Name = value;
			}
		}

		public FifaFileHeader(FifaBigFile bigFile)
		{
			m_BigFile = bigFile;
		}

		public FifaFileHeader()
		{
			m_BigFile = null;
			m_StartPosition = 0u;
			m_Name = null;
			m_Size = 0;
		}

		public bool Load(BinaryReader r)
		{
			m_StartPosition = FifaUtil.SwapEndian(r.ReadUInt32());
			m_Size = FifaUtil.SwapEndian(r.ReadInt32());
			m_Name = FifaUtil.ReadNullTerminatedString(r);
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			w.Write(FifaUtil.SwapEndian(m_StartPosition));
			w.Write(FifaUtil.SwapEndian(m_Size));
			FifaUtil.WriteNullTerminatedString(w, m_Name);
			return true;
		}
	}
}
