using System.Drawing;
using System.IO;

namespace FifaLibrary
{
	public class DxtBlock
	{
		private EImageType m_DxtType;

		private ushort m_Col0;

		private ushort m_Col1;

		private byte m_Alfa0;

		private byte m_Alfa1;

		private int[,] m_ColorLut;

		private int[,] m_AlfaLut;

		private int[,] m_Alfa;

		private int[,] m_TempLut;

		private Color[,] m_Colors;

		public Color[,] Colors
		{
			get
			{
				return m_Colors;
			}
			set
			{
				m_Colors = value;
			}
		}

		public DxtBlock(int dxtType)
		{
			m_ColorLut = new int[4, 4];
			m_AlfaLut = new int[4, 4];
			m_Colors = new Color[4, 4];
			m_Alfa = new int[4, 4];
			Init(dxtType);
		}

		public DxtBlock(int dxtType, BinaryReader br)
		{
			m_DxtType = (EImageType)dxtType;
			m_ColorLut = new int[4, 4];
			m_Colors = new Color[4, 4];
			Load(br);
		}

		public void Load(BinaryReader br)
		{
			switch (m_DxtType)
			{
			case EImageType.DXT1:
				ReadColorLut(br);
				ConvertTo4x4();
				break;
			case EImageType.DXT3:
				ReadAlfaChannel(br);
				ReadColorLut(br);
				ConvertTo4x4();
				break;
			case EImageType.DXT5:
				ReadAlfaLut(br);
				ReadColorLut(br);
				ConvertTo4x4();
				break;
			case EImageType.DC_XY_NORMAL_MAP:
				ReadAlfaLut(br);
				ConvertTo3DC(0);
				ReadAlfaLut(br);
				ConvertTo3DC(1);
				break;
			}
		}

		public void Save(BinaryWriter bw)
		{
			switch (m_DxtType)
			{
			case EImageType.DXT1:
				ConvertFrom4x4();
				WriteColorLut(bw);
				break;
			case EImageType.DXT3:
				ConvertFrom4x4();
				WriteAlfaChannel(bw);
				WriteColorLut(bw);
				break;
			case EImageType.DXT5:
				ConvertFrom4x4();
				WriteAlfaLut(bw);
				WriteColorLut(bw);
				break;
			case EImageType.DC_XY_NORMAL_MAP:
				ConvertFrom3DC(0);
				WriteAlfaLut(bw);
				ConvertFrom3DC(1);
				WriteAlfaLut(bw);
				break;
			}
		}

		private void Init(int dxtType)
		{
			m_DxtType = (EImageType)dxtType;
			m_Col0 = 0;
			m_Col1 = 0;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					m_ColorLut[i, j] = 0;
					m_Alfa[i, j] = 255;
					m_Colors[i, j] = Color.FromArgb(0, 0, 0);
				}
			}
		}

		private void ReadAlfaChannel(BinaryReader br)
		{
			for (int i = 0; i < 4; i++)
			{
				int num = br.ReadInt16();
				m_Alfa[0, i] = (num & 0xF) << 4;
				m_Alfa[1, i] = ((num >> 4) & 0xF) << 4;
				m_Alfa[2, i] = ((num >> 8) & 0xF) << 4;
				m_Alfa[3, i] = ((num >> 12) & 0xF) << 4;
			}
		}

		private void ReadColorLut(BinaryReader br)
		{
			m_Col0 = br.ReadUInt16();
			m_Col1 = br.ReadUInt16();
			for (int i = 0; i < 4; i++)
			{
				byte b = br.ReadByte();
				m_ColorLut[0, i] = (b & 3);
				m_ColorLut[1, i] = ((b >> 2) & 3);
				m_ColorLut[2, i] = ((b >> 4) & 3);
				m_ColorLut[3, i] = ((b >> 6) & 3);
			}
		}

		private void ReadNormalLut(BinaryReader br)
		{
			m_Col0 = br.ReadByte();
			m_Col1 = br.ReadByte();
			byte[] array = br.ReadBytes(6);
			m_ColorLut[0, 0] = (array[0] & 7);
			m_ColorLut[1, 0] = ((array[0] >> 3) & 7);
			m_ColorLut[2, 0] = ((array[0] >> 6) & 3) + ((array[1] & 1) << 2);
			m_ColorLut[3, 0] = ((array[1] >> 1) & 7);
			m_ColorLut[0, 1] = ((array[1] >> 4) & 7);
			m_ColorLut[1, 1] = ((array[1] >> 7) & 1) + ((array[2] & 3) << 1);
			m_ColorLut[2, 1] = ((array[2] >> 2) & 7);
			m_ColorLut[3, 1] = ((array[2] >> 5) & 7);
			m_ColorLut[0, 2] = (array[0] & 7);
			m_ColorLut[1, 2] = ((array[3] >> 3) & 7);
			m_ColorLut[2, 2] = ((array[3] >> 6) & 3) + ((array[4] & 1) << 2);
			m_ColorLut[3, 2] = ((array[3] >> 1) & 7);
			m_ColorLut[0, 3] = ((array[4] >> 4) & 7);
			m_ColorLut[1, 3] = ((array[4] >> 7) & 1) + ((array[5] & 3) << 1);
			m_ColorLut[2, 3] = ((array[5] >> 2) & 7);
			m_ColorLut[3, 3] = ((array[5] >> 5) & 7);
		}

		private void ReadAlfaLut(BinaryReader br)
		{
			m_Alfa0 = br.ReadByte();
			m_Alfa1 = br.ReadByte();
			byte[] array = br.ReadBytes(6);
			ulong num = 0uL;
			for (int num2 = 5; num2 >= 0; num2--)
			{
				num = ((num * 256) | array[num2]);
			}
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					m_AlfaLut[j, i] = (int)(num & 7);
					num >>= 3;
				}
			}
		}

		private void WriteAlfaChannel(BinaryWriter bw)
		{
			for (int i = 0; i < 4; i++)
			{
				ushort num = 0;
				num = (ushort)((m_Alfa[0, i] & 0xF0) >> 4);
				num = (ushort)(num | (ushort)(m_Alfa[1, i] & 0xF0));
				num = (ushort)(num | (ushort)((m_Alfa[2, i] & 0xF0) << 4));
				num = (ushort)(num | (ushort)((m_Alfa[3, i] & 0xF0) << 8));
				bw.Write(num);
			}
		}

		private void WriteColorLut(BinaryWriter bw)
		{
			bw.Write(m_Col0);
			bw.Write(m_Col1);
			for (int i = 0; i < 4; i++)
			{
				byte b = 0;
				b = (byte)(m_ColorLut[0, i] & 3);
				b = (byte)(b | (byte)((m_ColorLut[1, i] & 3) << 2));
				b = (byte)(b | (byte)((m_ColorLut[2, i] & 3) << 4));
				b = (byte)(b | (byte)((m_ColorLut[3, i] & 3) << 6));
				bw.Write(b);
			}
		}

		private void WriteAlfaLut(BinaryWriter bw)
		{
			bw.Write(m_Alfa0);
			bw.Write(m_Alfa1);
			ulong num = 0uL;
			for (int num2 = 3; num2 >= 0; num2--)
			{
				for (int num3 = 3; num3 >= 0; num3--)
				{
					num <<= 3;
					num |= (uint)(m_AlfaLut[num3, num2] & 7);
				}
			}
			byte[] array = new byte[6];
			for (int i = 0; i < 6; i++)
			{
				array[i] = (byte)(num & 0xFF);
				num >>= 8;
			}
			bw.Write(array);
		}

		private void CleanLuts()
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					m_AlfaLut[i, j] = 0;
					m_ColorLut[i, j] = 0;
				}
			}
		}

		private void ConvertFrom4x4()
		{
			Color[] array = new Color[16];
			_ = new int[16];
			bool flag = false;
			int num = 0;
			int num2 = 255;
			int num3 = 0;
			int num4 = 4;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					uint num5 = (uint)m_Colors[i, j].ToArgb();
					num5 = (uint)((int)num5 & -459528);
					m_Colors[i, j] = Color.FromArgb((int)num5);
				}
			}
			for (int k = 0; k < 4; k++)
			{
				for (int l = 0; l < 4; l++)
				{
					bool flag2 = false;
					if (m_DxtType == EImageType.DXT1)
					{
						if (m_Colors[k, l].A == 0)
						{
							flag = true;
							num4 = 3;
							continue;
						}
					}
					else
					{
						if (m_Colors[k, l].A < num2)
						{
							num2 = m_Colors[k, l].A;
						}
						if (m_Colors[k, l].A > num3)
						{
							num3 = m_Colors[k, l].A;
						}
					}
					for (int m = 0; m < num; m++)
					{
						if (m_Colors[k, l].R == array[m].R && m_Colors[k, l].G == array[m].G && m_Colors[k, l].B == array[m].B)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						array[num] = m_Colors[k, l];
						num++;
					}
				}
			}
			int num6 = 16777215;
			m_TempLut = new int[4, 4];
			if (m_DxtType == EImageType.DXT1 && flag)
			{
				switch (num)
				{
				case 0:
				{
					for (int num10 = 0; num10 < 4; num10++)
					{
						for (int num11 = 0; num11 < 4; num11++)
						{
							m_ColorLut[num10, num11] = 3;
						}
					}
					m_Col0 = 0;
					m_Col1 = 0;
					return;
				}
				case 1:
				{
					for (int num8 = 0; num8 < 4; num8++)
					{
						for (int num9 = 0; num9 < 4; num9++)
						{
							if (m_Colors[num8, num9].A == 0)
							{
								m_ColorLut[num8, num9] = 3;
							}
							else
							{
								m_ColorLut[num8, num9] = 0;
							}
						}
					}
					m_Col0 = (m_Col1 = (ushort)(((array[0].R & 0xF8) << 8) | ((array[0].G & 0xFC) << 3) | ((array[0].B & 0xF8) >> 3)));
					return;
				}
				case 2:
				{
					m_Col0 = (ushort)(((array[0].R & 0xF8) << 8) | ((array[0].G & 0xFC) << 3) | ((array[0].B & 0xF8) >> 3));
					m_Col1 = (ushort)(((array[1].R & 0xF8) << 8) | ((array[1].G & 0xFC) << 3) | ((array[1].B & 0xF8) >> 3));
					if (m_Col0 >= m_Col1)
					{
						ushort col = m_Col0;
						m_Col0 = m_Col1;
						m_Col1 = col;
						Color color = array[0];
						array[0] = array[1];
						array[1] = color;
					}
					for (int n = 0; n < 4; n++)
					{
						for (int num7 = 0; num7 < 4; num7++)
						{
							if (m_Colors[n, num7].A == 0)
							{
								m_ColorLut[n, num7] = 3;
							}
							else if (m_Colors[n, num7] == array[0])
							{
								m_ColorLut[n, num7] = 0;
							}
							else
							{
								m_ColorLut[n, num7] = 1;
							}
						}
					}
					return;
				}
				}
			}
			CleanLuts();
			if (num3 == 0 && num2 == 0)
			{
				m_Alfa0 = 0;
				m_Col0 = 0;
				m_Alfa1 = 1;
				m_Col1 = 1;
				num = 0;
			}
			if (num == 1)
			{
				for (int num12 = 0; num12 < 4; num12++)
				{
					for (int num13 = 0; num13 < 4; num13++)
					{
						m_ColorLut[num12, num13] = 0;
					}
				}
				m_Col0 = (m_Col1 = (ushort)(((array[0].R & 0xF8) << 8) | ((array[0].G & 0xFC) << 3) | ((array[0].B & 0xF8) >> 3)));
			}
			for (int num14 = 0; num14 < num; num14++)
			{
				for (int num15 = num14 + 1; num15 < num; num15++)
				{
					ushort num16 = (ushort)(((array[num14].R & 0xF8) << 8) | ((array[num14].G & 0xFC) << 3) | ((array[num14].B & 0xF8) >> 3));
					ushort num17 = (ushort)(((array[num15].R & 0xF8) << 8) | ((array[num15].G & 0xFC) << 3) | ((array[num15].B & 0xF8) >> 3));
					ushort num18;
					ushort num19;
					int num20;
					int num21;
					if (num16 < num17)
					{
						num18 = num16;
						num19 = num17;
						num20 = num14;
						num21 = num15;
					}
					else
					{
						num18 = num17;
						num19 = num16;
						num20 = num15;
						num21 = num14;
					}
					int num22;
					if (num4 == 4)
					{
						num22 = ScoreColors(array[num21], array[num20], num4);
						if (num22 < num6)
						{
							num6 = num22;
							for (int num23 = 0; num23 < 4; num23++)
							{
								for (int num24 = 0; num24 < 4; num24++)
								{
									m_ColorLut[num23, num24] = m_TempLut[num23, num24];
								}
							}
							m_Col0 = num19;
							m_Col1 = num18;
							if (num22 == 0)
							{
								break;
							}
						}
					}
					if (m_DxtType != 0)
					{
						continue;
					}
					num22 = ScoreColors(array[num20], array[num21], 3);
					if (num22 >= num6)
					{
						continue;
					}
					num6 = num22;
					for (int num25 = 0; num25 < 4; num25++)
					{
						for (int num26 = 0; num26 < 4; num26++)
						{
							m_ColorLut[num25, num26] = m_TempLut[num25, num26];
						}
					}
					m_Col0 = num18;
					m_Col1 = num19;
					if (num22 == 0)
					{
						break;
					}
				}
			}
			if (m_DxtType == EImageType.DXT3 || m_DxtType == EImageType.DXT3)
			{
				for (int num27 = 0; num27 < 4; num27++)
				{
					for (int num28 = 0; num28 < 4; num28++)
					{
						m_Alfa[num27, num28] = (m_Colors[num27, num28].A & 0xF0);
					}
				}
			}
			if ((m_DxtType != EImageType.DXT5 && m_DxtType != EImageType.DXT5) || (num2 == 0 && num3 == 0))
			{
				return;
			}
			m_Alfa1 = (byte)num2;
			m_Alfa0 = (byte)num3;
			int num29 = num3 - num2;
			int[] array2 = new int[8];
			if (num29 != 0)
			{
				for (int num30 = 2; num30 < 8; num30++)
				{
					array2[num30] = (m_Alfa0 * (8 - num30) + m_Alfa1 * (num30 - 1)) / 7;
				}
			}
			int num31 = num29 / 7 / 2;
			for (int num32 = 0; num32 < 4; num32++)
			{
				for (int num33 = 0; num33 < 4; num33++)
				{
					int a = m_Colors[num32, num33].A;
					if (a <= num2 + num31)
					{
						m_AlfaLut[num32, num33] = 1;
					}
					else if (a >= num3 - num31)
					{
						m_AlfaLut[num32, num33] = 0;
					}
					else if (num29 != 0)
					{
						m_AlfaLut[num32, num33] = 0;
						for (int num34 = 2; num34 < 8; num34++)
						{
							if (a > array2[num34] - num31)
							{
								m_AlfaLut[num32, num33] = num34;
								break;
							}
						}
					}
					else
					{
						m_AlfaLut[num32, num33] = 0;
					}
				}
			}
		}

		private void ConvertFrom3DC(int rgb)
		{
			int num = 255;
			int num2 = 0;
			switch (rgb)
			{
			case 0:
			{
				for (int k = 0; k < 4; k++)
				{
					for (int l = 0; l < 4; l++)
					{
						m_Alfa[k, l] = m_Colors[k, l].R;
						if (m_Alfa[k, l] < num)
						{
							num = m_Alfa[k, l];
						}
						if (m_Alfa[k, l] > num2)
						{
							num2 = m_Alfa[k, l];
						}
					}
				}
				break;
			}
			case 1:
			{
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						m_Alfa[i, j] = m_Colors[i, j].G;
						if (m_Alfa[i, j] < num)
						{
							num = m_Alfa[i, j];
						}
						if (m_Alfa[i, j] > num2)
						{
							num2 = m_Alfa[i, j];
						}
					}
				}
				break;
			}
			}
			CleanLuts();
			if (num2 == 0 && num == 0)
			{
				m_Alfa0 = 0;
				m_Alfa1 = 1;
			}
			if (m_DxtType != EImageType.DC_XY_NORMAL_MAP || (num == 0 && num2 == 0))
			{
				return;
			}
			m_Alfa1 = (byte)num;
			m_Alfa0 = (byte)num2;
			int num3 = num2 - num;
			int[] array = new int[8];
			if (num3 != 0)
			{
				for (int m = 2; m < 8; m++)
				{
					array[m] = (m_Alfa0 * (8 - m) + m_Alfa1 * (m - 1)) / 7;
				}
			}
			int num4 = num3 / 7 / 2;
			for (int n = 0; n < 4; n++)
			{
				for (int num5 = 0; num5 < 4; num5++)
				{
					int num6 = m_Alfa[n, num5];
					if (num6 <= num + num4)
					{
						m_AlfaLut[n, num5] = 1;
					}
					else if (num6 >= num2 - num4)
					{
						m_AlfaLut[n, num5] = 0;
					}
					else if (num3 != 0)
					{
						m_AlfaLut[n, num5] = 0;
						for (int num7 = 2; num7 < 8; num7++)
						{
							if (num6 > array[num7] - num4)
							{
								m_AlfaLut[n, num5] = num7;
								break;
							}
						}
					}
					else
					{
						m_AlfaLut[n, num5] = 0;
					}
				}
			}
		}

		private void ConvertTo3DC(int rgb)
		{
			int[] array = new int[8]
			{
				m_Alfa0,
				m_Alfa1,
				0,
				0,
				0,
				0,
				0,
				0
			};
			if (m_Alfa0 > m_Alfa1)
			{
				for (int i = 2; i < 8; i++)
				{
					array[i] = (m_Alfa0 * (8 - i) + m_Alfa1 * (i - 1)) / 7;
				}
			}
			else
			{
				for (int j = 2; j < 6; j++)
				{
					array[j] = (m_Alfa0 * (6 - j) + m_Alfa1 * (j - 1)) / 5;
				}
				array[6] = 0;
				array[7] = 255;
			}
			for (int k = 0; k < 4; k++)
			{
				for (int l = 0; l < 4; l++)
				{
					int num = m_AlfaLut[k, l];
					m_Alfa[k, l] = array[num];
				}
			}
			if (rgb == 0)
			{
				for (int m = 0; m < 4; m++)
				{
					for (int n = 0; n < 4; n++)
					{
						m_Colors[m, n] = Color.FromArgb(array[m_AlfaLut[m, n]], 0, 0);
					}
				}
			}
			if (rgb != 1)
			{
				return;
			}
			for (int num2 = 0; num2 < 4; num2++)
			{
				for (int num3 = 0; num3 < 4; num3++)
				{
					m_Colors[num2, num3] = Color.FromArgb(m_Colors[num2, num3].R, array[m_ColorLut[num2, num3]], 255);
				}
			}
		}

		private void ConvertTo4x4()
		{
			int[] array = new int[8]
			{
				m_Alfa0,
				m_Alfa1,
				0,
				0,
				0,
				0,
				0,
				0
			};
			if (m_DxtType == EImageType.DXT5)
			{
				if (m_Alfa0 > m_Alfa1)
				{
					for (int i = 2; i < 8; i++)
					{
						array[i] = (m_Alfa0 * (8 - i) + m_Alfa1 * (i - 1)) / 7;
					}
				}
				else
				{
					for (int j = 2; j < 6; j++)
					{
						array[j] = (m_Alfa0 * (6 - j) + m_Alfa1 * (j - 1)) / 5;
					}
					array[6] = 0;
					array[7] = 255;
				}
			}
			if (m_DxtType == EImageType.DXT5)
			{
				for (int k = 0; k < 4; k++)
				{
					for (int l = 0; l < 4; l++)
					{
						int num = m_AlfaLut[k, l];
						m_Alfa[k, l] = array[num];
					}
				}
			}
			if (m_DxtType == EImageType.DXT1)
			{
				for (int m = 0; m < 4; m++)
				{
					for (int n = 0; n < 4; n++)
					{
						int num2 = m_ColorLut[m, n];
						if (m_Col0 <= m_Col1 && num2 == 3)
						{
							m_Alfa[m, n] = 0;
						}
						else
						{
							m_Alfa[m, n] = 255;
						}
					}
				}
			}
			int[] array2 = new int[4];
			int[] array3 = new int[4];
			int[] array4 = new int[4]
			{
				8 * (m_Col0 & 0x1F),
				0,
				0,
				0
			};
			array3[0] = 4 * ((m_Col0 >> 5) & 0x3F);
			array2[0] = 8 * (m_Col0 >> 11);
			array4[1] = 8 * (m_Col1 & 0x1F);
			array3[1] = 4 * ((m_Col1 >> 5) & 0x3F);
			array2[1] = 8 * (m_Col1 >> 11);
			if (m_Col0 > m_Col1 || m_DxtType != 0)
			{
				array2[2] = (2 * array2[0] + array2[1]) / 3;
				array3[2] = (2 * array3[0] + array3[1]) / 3;
				array4[2] = (2 * array4[0] + array4[1]) / 3;
				array2[3] = (array2[0] + 2 * array2[1]) / 3;
				array3[3] = (array3[0] + 2 * array3[1]) / 3;
				array4[3] = (array4[0] + 2 * array4[1]) / 3;
			}
			else
			{
				array2[2] = (array2[0] + array2[1]) / 2;
				array3[2] = (array3[0] + array3[1]) / 2;
				array4[2] = (array4[0] + array4[1]) / 2;
				array2[3] = 0;
				array3[3] = 0;
				array4[3] = 0;
			}
			for (int num3 = 0; num3 < 4; num3++)
			{
				for (int num4 = 0; num4 < 4; num4++)
				{
					int num5 = m_ColorLut[num3, num4];
					m_Colors[num3, num4] = Color.FromArgb(m_Alfa[num3, num4], array2[num5], array3[num5], array4[num5]);
				}
			}
		}

		private int[] ComputeInterpolatedAlfa(int alfa0, int alfa1)
		{
			int[] array = new int[8]
			{
				alfa0,
				alfa1,
				0,
				0,
				0,
				0,
				0,
				0
			};
			for (int i = 2; i < 8; i++)
			{
				int num = 8 - i;
				int num2 = i - 1;
				array[i] = (num * alfa0 + num2 * alfa1) / 7;
			}
			return array;
		}

		private int[,] ComputeInterpolatedRgb(Color col0, Color col1, int nColors)
		{
			int[,] array = new int[4, 3]
			{
				{
					col0.R,
					col0.G,
					col0.B
				},
				{
					col1.R,
					col1.G,
					col1.B
				},
				{
					0,
					0,
					0
				},
				{
					0,
					0,
					0
				}
			};
			if (nColors <= 3)
			{
				nColors = 3;
				for (int i = 2; i < nColors; i++)
				{
					int num = nColors - i;
					int num2 = i - 1;
					array[i, 0] = (num * col0.R + num2 * col1.R) / 2;
					array[i, 1] = (num * col0.G + num2 * col1.G) / 2;
					array[i, 2] = (num * col0.B + num2 * col1.B) / 2;
				}
				array[3, 0] = 0;
				array[3, 1] = 0;
				array[3, 2] = 0;
			}
			else
			{
				nColors = 4;
				for (int j = 2; j < nColors; j++)
				{
					int num3 = nColors - j;
					int num4 = j - 1;
					array[j, 0] = (num3 * col0.R + num4 * col1.R) / 3;
					array[j, 1] = (num3 * col0.G + num4 * col1.G) / 3;
					array[j, 2] = (num3 * col0.B + num4 * col1.B) / 3;
				}
			}
			return array;
		}

		private int ScoreColors(Color col0, Color col1, int nColors)
		{
			int[,] array = ComputeInterpolatedRgb(col0, col1, nColors);
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int num2 = 16777215;
					int r = m_Colors[i, j].R;
					int g = m_Colors[i, j].G;
					int b = m_Colors[i, j].B;
					if (m_Colors[i, j].A == 0 && nColors == 3)
					{
						m_TempLut[i, j] = 3;
						continue;
					}
					for (int k = 0; k < nColors; k++)
					{
						int num3 = array[k, 0] - r;
						int num4 = array[k, 1] - g;
						int num5 = array[k, 2] - b;
						int num6 = num3 * num3 + num4 * num4 + num5 * num5;
						if (num6 < num2)
						{
							num2 = num6;
							m_TempLut[i, j] = k;
							if (num6 == 0)
							{
								break;
							}
						}
					}
					num += num2;
				}
			}
			return num;
		}
	}
}
