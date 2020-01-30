using System.IO;

namespace FifaLibrary
{
	public class SbrFile
	{
		private int m_Length;

		private string m_SbrFileName;

		protected char[] m_Signature;

		public int Length => m_Length;

		public string SbrFileName => m_SbrFileName;

		public SbrFile()
		{
		}

		public SbrFile(string sbrFileName)
		{
			m_SbrFileName = sbrFileName;
		}

		public bool Load()
		{
			string path = FifaEnvironment.GameDir + m_SbrFileName;
			if (!File.Exists(path))
			{
				return false;
			}
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			if (fileStream == null)
			{
				return false;
			}
			BinaryReader binaryReader = new BinaryReader(fileStream);
			if (binaryReader == null)
			{
				return false;
			}
			m_Signature = binaryReader.ReadChars(4);
			if (m_Signature[0] != 'S' || m_Signature[1] != 'B' || m_Signature[2] != 'l' || m_Signature[3] != 'e')
			{
				return false;
			}
			m_Length = binaryReader.ReadInt32();
			return true;
		}
	}
}
