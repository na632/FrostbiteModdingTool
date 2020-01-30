using System.IO;

namespace FifaLibrary
{
	public class Rx3RawData
	{
		private string m_FileName;

		private bool m_IsFifa14;

		private int m_Rx3bPosition;

		private byte[] m_Preface;

		private byte[][] m_RawData;

		private Rx3Header m_Rx3Header;

		private bool m_SwapEndian;

		private Rx3FileDescriptor[] m_Rx3FileDescriptors;

		public string FileName => m_FileName;

		public bool IsFifa14 => m_IsFifa14;

		public byte[][] RawData => m_RawData;

		public Rx3Header Rx3Headers => m_Rx3Header;

		public Rx3FileDescriptor[] Rx3FileDescriptors => m_Rx3FileDescriptors;

		public Rx3RawData(string fileName)
		{
			m_FileName = fileName;
			Load(fileName);
		}

		public bool Load(string fileName)
		{
			bool flag = false;
			if (!File.Exists(fileName))
			{
				return false;
			}
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			flag = Load(binaryReader);
			fileStream.Close();
			binaryReader.Close();
			m_FileName = fileName;
			return flag;
		}

		public bool Load(BinaryReader r)
		{
			string a = new string(r.ReadChars(4));
			if (a == "Rx3l" || a == "RX3l")
			{
				r.BaseStream.Seek(0L, SeekOrigin.Begin);
				m_Rx3bPosition = 0;
				m_SwapEndian = false;
				m_IsFifa14 = true;
			}
			else if (a != "RX3b")
			{
				r.BaseStream.Seek(0L, SeekOrigin.Begin);
				m_Rx3bPosition = 0;
				m_SwapEndian = true;
				m_IsFifa14 = false;
			}
			else
			{
				r.BaseStream.Position = 68L;
				m_Rx3bPosition = FifaUtil.SwapEndian(r.ReadInt32());
				r.BaseStream.Seek(0L, SeekOrigin.Begin);
				m_Preface = r.ReadBytes(m_Rx3bPosition);
				m_SwapEndian = true;
				m_IsFifa14 = false;
			}
			m_Rx3Header = new Rx3Header(r, m_SwapEndian);
			if (m_Rx3Header.NFiles == 0)
			{
				return false;
			}
			m_Rx3FileDescriptors = new Rx3FileDescriptor[m_Rx3Header.NFiles];
			for (int i = 0; i < m_Rx3Header.NFiles; i++)
			{
				m_Rx3FileDescriptors[i] = new Rx3FileDescriptor(r, m_SwapEndian);
			}
			m_RawData = new byte[m_Rx3Header.NFiles][];
			for (int j = 0; j < m_Rx3Header.NFiles; j++)
			{
				m_RawData[j] = r.ReadBytes(m_Rx3FileDescriptors[j].Size);
			}
			return true;
		}

		public bool SetRawData(int index, byte[] rawData)
		{
			int num = rawData.Length;
			int size = m_Rx3FileDescriptors[index].Size;
			m_Rx3FileDescriptors[index].Size = num;
			int num2 = num - size;
			for (int i = index + 1; i < m_Rx3FileDescriptors.Length; i++)
			{
				m_Rx3FileDescriptors[i].Offset += num2;
			}
			m_RawData[index] = rawData;
			m_Rx3Header.SizeOf_ += num2;
			return true;
		}

		public bool Save(string fileName)
		{
			bool flag = false;
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			flag = Save(binaryWriter);
			fileStream.Close();
			binaryWriter.Close();
			m_FileName = fileName;
			return flag;
		}

		public virtual bool Save(BinaryWriter w)
		{
			m_Rx3Header.Save(w);
			for (int i = 0; i < m_Rx3Header.NFiles; i++)
			{
				m_Rx3FileDescriptors[i].Save(w);
			}
			for (int j = 0; j < m_Rx3Header.NFiles; j++)
			{
				w.Write(m_RawData[j]);
			}
			return true;
		}
	}
}
