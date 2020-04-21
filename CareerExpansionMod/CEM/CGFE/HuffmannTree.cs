using System.IO;

namespace v2k4FIFAModdingCL.CGFE
{
	public class HuffmannTree
	{
		private byte[,] m_Child;

		private byte[,] m_Leaf;

		private ushort[] m_EncodingValue = new ushort[256];

		private byte[] m_nBitsForEncoding = new byte[256];

		private int m_NNodes;

		public int NNodes
		{
			get
			{
				return m_NNodes;
			}
			set
			{
				m_NNodes = value;
			}
		}

		public int Size => m_NNodes * 4;

		public HuffmannTree(int nNodes)
		{
			m_NNodes = nNodes;
			m_Child = new byte[m_NNodes, 2];
			m_Leaf = new byte[m_NNodes, 2];
			for (int i = 0; i < m_NNodes; i++)
			{
				m_Child[i, 0] = 0;
				m_Child[i, 1] = 0;
				m_Leaf[i, 0] = 0;
				m_Leaf[i, 1] = 0;
			}
		}

		public void Load(DbReader r)
		{
			for (int i = 0; i < m_NNodes; i++)
			{
				m_Child[i, 0] = r.ReadByte();
				m_Leaf[i, 0] = r.ReadByte();
				m_Child[i, 1] = r.ReadByte();
				m_Leaf[i, 1] = r.ReadByte();
			}
			BuildEncodingTable();
		}

		public void Save(BinaryWriter w)
		{
			for (int i = 0; i < m_NNodes; i++)
			{
				w.Write(m_Child[i, 0]);
				w.Write(m_Leaf[i, 0]);
				w.Write(m_Child[i, 1]);
				w.Write(m_Leaf[i, 1]);
			}
		}

		private void BuildEncodingTable()
		{
			ushort[] array = new ushort[m_NNodes];
			byte[] array2 = new byte[m_NNodes];
			for (int i = 0; i < m_NNodes; i++)
			{
				ushort num = (ushort)(array[i] * 2);
				byte b = (byte)(array2[i] + 1);
				byte b2 = m_Child[i, 0];
				if (b2 != 0)
				{
					array[b2] = num;
					array2[b2] = b;
				}
				else
				{
					byte b3 = m_Leaf[i, 0];
					m_EncodingValue[b3] = num;
					m_nBitsForEncoding[b3] = b;
				}
				num = (ushort)(num + 1);
				byte b4 = m_Child[i, 1];
				if (b4 != 0)
				{
					array[b4] = num;
					array2[b4] = b;
				}
				else
				{
					byte b5 = m_Leaf[i, 1];
					m_EncodingValue[b5] = num;
					m_nBitsForEncoding[b5] = b;
				}
			}
		}

		public string ReadString(DbReader r, int outputLength)
		{
			int num = 0;
			int num2 = 0;
			if (outputLength <= 0)
			{
				return string.Empty;
			}
			byte[] array;
			if (m_NNodes == 0)
			{
				array = r.ReadBytes(outputLength);
			}
			else
			{
				array = new byte[outputLength];
				do
				{
					byte b = r.ReadByte();
					for (int num3 = 7; num3 >= 0; num3--)
					{
						int num4 = (b >> num3) & 1;
						int num5 = m_Child[num2, num4];
						if (num5 == 0)
						{
							array[num++] = m_Leaf[num2, num4];
							if (num == outputLength)
							{
								break;
							}
							num2 = 0;
						}
						else
						{
							num2 = num5;
						}
					}
				}
				while (num < outputLength);
			}
			return FifaUtil.ConvertBytesToString(array);
		}

		public int WriteString(BinaryWriter w, string str, bool longString)
		{
			if (str == null || str.Length == 0)
			{
				return 0;
			}
			byte[] array = FifaUtil.ConvertStringToBytes(str);
			int num;
			if (longString)
			{
				ushort x = (ushort)array.Length;
				w.Write(FifaUtil.SwapEndian(x));
				num = 2;
			}
			else
			{
				w.Write((byte)array.Length);
				num = 1;
			}
			if (m_NNodes == 0)
			{
				w.Write(array);
				return num + array.Length;
			}
			int num2 = 7;
			byte b = 0;
			foreach (byte b2 in array)
			{
				int num3 = m_EncodingValue[b2];
				int num4 = m_nBitsForEncoding[b2];
				if (num4 == 0)
				{
					num3 = m_EncodingValue[32];
					num4 = m_nBitsForEncoding[32];
				}
				for (int num5 = num4 - 1; num5 >= 0; num5--)
				{
					if ((num3 & (1 << num5)) != 0)
					{
						b = (byte)(b + (1 << num2));
					}
					num2--;
					if (num2 == -1)
					{
						num2 = 7;
						w.Write(b);
						num++;
						b = 0;
					}
				}
			}
			w.Write(b);
			return num + 1;
		}
	}
}
