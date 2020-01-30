using System.IO;

namespace FifaLibrary
{
	public class Rx3Signatures
	{
		private int m_Offset;

		private int m_Length;

		private string[] m_Signatures;

		public int Offset
		{
			get
			{
				return m_Offset;
			}
			set
			{
				m_Offset = value;
			}
		}

		public int Length
		{
			get
			{
				return m_Length;
			}
			set
			{
				m_Length = value;
			}
		}

		public string[] Signatures
		{
			get
			{
				return m_Signatures;
			}
			set
			{
				m_Signatures = value;
			}
		}

		public Rx3Signatures(int offset, int length, string[] signatures)
		{
			Init(offset, length, signatures);
		}

		private void Init(int offset, int length, string[] signatures)
		{
			m_Offset = offset;
			m_Length = length;
			m_Signatures = signatures;
		}

		public bool Save(BinaryWriter w)
		{
			if (m_Signatures == null)
			{
				return false;
			}
			for (int i = 0; i < m_Signatures.Length; i++)
			{
				w.BaseStream.Position = m_Offset + m_Length * i + 4;
				w.Write(m_Length - 8);
				w.Write(m_Signatures[i].ToCharArray(0, m_Signatures[i].Length));
				for (int j = m_Signatures[i].Length; j < m_Length - 8; j++)
				{
					w.Write((byte)0);
				}
			}
			return true;
		}
	}
}
