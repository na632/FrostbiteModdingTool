using System.IO;

namespace FifaLibrary
{
	public class UgcDirEntry
	{
		private uint m_Offset;

		private string m_FileName;

		public uint Offset => m_Offset;

		public string FileName => m_FileName;

		public override string ToString()
		{
			return Path.GetFileName(m_FileName);
		}

		public UgcDirEntry(BinaryReader r)
		{
			Load(r);
		}

		public bool Load(BinaryReader r)
		{
			r.ReadBytes(16);
			m_Offset = r.ReadUInt32();
			r.ReadInt16();
			m_FileName = FifaUtil.ReadNullPaddedString(r, 66);
			return true;
		}

		public bool IsPng()
		{
			return m_FileName.EndsWith("png");
		}

		public bool IsDb()
		{
			return m_FileName.EndsWith("db");
		}
	}
}
