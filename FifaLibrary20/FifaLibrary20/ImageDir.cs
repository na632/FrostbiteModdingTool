using System.IO;

namespace FifaLibrary
{
	public class ImageDir
	{
		private string m_Name;

		private int m_Offset;

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

		public ImageDir(BinaryReader r)
		{
			m_Name = new string(r.ReadChars(4));
			m_Offset = r.ReadInt32();
		}

		public void Save(BinaryWriter w)
		{
			char[] array = m_Name.ToCharArray();
			w.Write((byte)array[0]);
			w.Write((byte)array[1]);
			w.Write((byte)array[2]);
			w.Write((byte)array[3]);
			w.Write(m_Offset);
		}
	}
}
