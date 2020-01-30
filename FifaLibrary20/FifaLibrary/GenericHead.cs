using System;
using System.Drawing;

namespace FifaLibrary
{
	public static class GenericHead
	{
		public enum EHeadModelSet
		{
			Unknown,
			African,
			Asiatic,
			Caucasic,
			Latin,
			Female
		}

		public enum EHairModelSet
		{
			Shaven,
			VeryShort,
			Short,
			Modern,
			Medium,
			Long,
			Afro,
			Headbend,
			FemaleHair
		}

		public static int[] c_LowHair = new int[112]
		{
			0,
			1,
			9,
			2,
			3,
			6,
			4,
			4,
			8,
			8,
			13,
			7,
			8,
			13,
			5,
			8,
			9,
			9,
			10,
			9,
			5,
			9,
			9,
			9,
			9,
			1,
			0,
			10,
			1,
			9,
			9,
			9,
			13,
			6,
			13,
			14,
			12,
			1,
			9,
			9,
			1,
			1,
			10,
			1,
			8,
			9,
			1,
			9,
			4,
			5,
			16,
			2,
			2,
			4,
			9,
			12,
			12,
			9,
			9,
			14,
			11,
			10,
			14,
			10,
			10,
			1,
			14,
			14,
			13,
			14,
			11,
			1,
			1,
			13,
			9,
			9,
			3,
			9,
			9,
			5,
			6,
			13,
			9,
			9,
			3,
			14,
			9,
			13,
			11,
			14,
			9,
			8,
			9,
			9,
			11,
			14,
			2,
			9,
			14,
			8,
			9,
			9,
			9,
			8,
			14,
			9,
			9,
			9,
			12,
			7,
			1,
			9
		};

		public static Color[] c_FacialHairColorPalette = new Color[7]
		{
			Color.FromArgb(48, 36, 32),
			Color.FromArgb(160, 140, 112),
			Color.FromArgb(72, 52, 40),
			Color.FromArgb(184, 161, 130),
			Color.FromArgb(144, 108, 80),
			Color.FromArgb(112, 96, 72),
			Color.FromArgb(136, 84, 56)
		};

		public static int[] c_CaucasicModels = new int[66]
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			11,
			12,
			13,
			14,
			15,
			16,
			17,
			18,
			19,
			20,
			21,
			22,
			23,
			24,
			25,
			2000,
			2001,
			2002,
			2003,
			2004,
			2005,
			2006,
			2007,
			2008,
			2009,
			2010,
			2011,
			2012,
			2013,
			2014,
			2015,
			2016,
			2017,
			2019,
			2020,
			2021,
			2022,
			2023,
			2024,
			2025,
			2026,
			2027,
			2028,
			2029,
			2030,
			3500,
			3501,
			3502,
			3503,
			3504,
			3505,
			4000,
			4001,
			4002,
			4003
		};

		public static int[] c_AsiaticModels = new int[33]
		{
			500,
			501,
			502,
			503,
			504,
			505,
			506,
			507,
			508,
			509,
			510,
			511,
			512,
			513,
			514,
			515,
			516,
			517,
			518,
			519,
			520,
			521,
			522,
			523,
			524,
			525,
			526,
			527,
			528,
			529,
			530,
			531,
			532
		};

		public static int[] c_AfricanModels = new int[42]
		{
			1000,
			1001,
			1002,
			1003,
			1004,
			1005,
			1006,
			1007,
			1008,
			1009,
			1010,
			1011,
			1012,
			1013,
			1014,
			1015,
			1016,
			1017,
			1018,
			1019,
			1020,
			1021,
			1022,
			1023,
			1024,
			1025,
			1026,
			1027,
			3000,
			3001,
			3002,
			3003,
			3004,
			3005,
			4500,
			4501,
			4502,
			4525,
			5000,
			5001,
			5002,
			5003
		};

		public static int[] c_LatinModels = new int[48]
		{
			1500,
			1501,
			1502,
			1503,
			1504,
			1505,
			1506,
			1507,
			1508,
			1509,
			1510,
			1511,
			1512,
			1513,
			1514,
			1515,
			1516,
			1517,
			1518,
			1519,
			1520,
			1521,
			1522,
			1523,
			1524,
			1525,
			1526,
			1527,
			1528,
			2500,
			2501,
			2502,
			2503,
			2504,
			2505,
			2506,
			2507,
			2508,
			2509,
			2510,
			2511,
			2512,
			2513,
			2514,
			2515,
			2516,
			2517,
			2518
		};

		public static int[] c_FemaleModels = new int[33]
		{
			5500,
			5501,
			5502,
			6000,
			6001,
			6002,
			6500,
			6501,
			6502,
			7000,
			7001,
			7002,
			7500,
			7501,
			7502,
			8000,
			8001,
			8002,
			8500,
			8501,
			8502,
			9000,
			9001,
			9002,
			9500,
			9501,
			9502,
			10000,
			10001,
			10002,
			10500,
			10501,
			10502
		};

		public static int[] c_ShavenModels = new int[7]
		{
			0,
			25,
			1,
			43,
			41,
			46,
			120
		};

		public static int[] c_VeryShortModels = new int[15]
		{
			26,
			29,
			47,
			72,
			92,
			16,
			28,
			31,
			37,
			40,
			45,
			65,
			77,
			90,
			117
		};

		public static int[] c_ShortModels = new int[28]
		{
			2,
			21,
			22,
			30,
			38,
			54,
			57,
			70,
			75,
			78,
			82,
			97,
			101,
			102,
			104,
			105,
			106,
			107,
			108,
			111,
			112,
			113,
			115,
			114,
			118,
			121,
			122,
			124
		};

		public static int[] c_ModernModels = new int[14]
		{
			17,
			18,
			19,
			24,
			39,
			60,
			61,
			63,
			64,
			86,
			88,
			89,
			94,
			123
		};

		public static int[] c_MediumModels = new int[30]
		{
			36,
			74,
			13,
			35,
			42,
			59,
			69,
			73,
			85,
			93,
			32,
			66,
			67,
			68,
			14,
			20,
			23,
			58,
			62,
			83,
			95,
			22,
			52,
			87,
			98,
			99,
			100,
			103,
			116,
			119
		};

		public static int[] c_LongModels = new int[16]
		{
			8,
			9,
			15,
			44,
			84,
			34,
			10,
			33,
			12,
			80,
			11,
			50,
			51,
			79,
			53,
			7
		};

		public static int[] c_AfroModels = new int[9]
		{
			71,
			4,
			27,
			5,
			6,
			96,
			3,
			109,
			110
		};

		public static int[] c_HeadbendModels = new int[7]
		{
			55,
			56,
			76,
			81,
			49,
			91,
			48
		};

		public static int[] c_FemaleHairModels = new int[42]
		{
			500,
			501,
			502,
			503,
			504,
			505,
			506,
			507,
			508,
			509,
			510,
			511,
			512,
			513,
			514,
			515,
			516,
			517,
			518,
			519,
			520,
			521,
			522,
			523,
			524,
			525,
			526,
			527,
			528,
			529,
			530,
			531,
			532,
			533,
			534,
			535,
			536,
			537,
			538,
			539,
			540,
			541
		};

		private static Random m_Randomizer = new Random();

		public static int GetModelId(EHeadModelSet modelSet, int index)
		{
			switch (modelSet)
			{
			case EHeadModelSet.African:
				if (index < c_AfricanModels.Length)
				{
					return c_AfricanModels[index];
				}
				break;
			case EHeadModelSet.Asiatic:
				if (index < c_AsiaticModels.Length)
				{
					return c_AsiaticModels[index];
				}
				break;
			case EHeadModelSet.Caucasic:
				if (index < c_CaucasicModels.Length)
				{
					return c_CaucasicModels[index];
				}
				break;
			case EHeadModelSet.Latin:
				if (index < c_LatinModels.Length)
				{
					return c_LatinModels[index];
				}
				break;
			case EHeadModelSet.Female:
				if (index < c_FemaleModels.Length)
				{
					return c_FemaleModels[index];
				}
				break;
			}
			return -1;
		}

		public static EHeadModelSet GetModelSet(int modelId)
		{
			for (int i = 0; i < c_CaucasicModels.Length; i++)
			{
				if (modelId == c_CaucasicModels[i])
				{
					return EHeadModelSet.Caucasic;
				}
			}
			for (int j = 0; j < c_LatinModels.Length; j++)
			{
				if (modelId == c_LatinModels[j])
				{
					return EHeadModelSet.Latin;
				}
			}
			for (int k = 0; k < c_AfricanModels.Length; k++)
			{
				if (modelId == c_AfricanModels[k])
				{
					return EHeadModelSet.African;
				}
			}
			for (int l = 0; l < c_AsiaticModels.Length; l++)
			{
				if (modelId == c_AsiaticModels[l])
				{
					return EHeadModelSet.Asiatic;
				}
			}
			for (int m = 0; m < c_FemaleModels.Length; m++)
			{
				if (modelId == c_FemaleModels[m])
				{
					return EHeadModelSet.Female;
				}
			}
			return EHeadModelSet.Unknown;
		}

		public static int GetModelSetIndex(EHeadModelSet modelSet, int modelId)
		{
			switch (modelSet)
			{
			case EHeadModelSet.African:
			{
				for (int j = 0; j < c_AfricanModels.Length; j++)
				{
					if (modelId == c_AfricanModels[j])
					{
						return j;
					}
				}
				break;
			}
			case EHeadModelSet.Asiatic:
			{
				for (int l = 0; l < c_AsiaticModels.Length; l++)
				{
					if (modelId == c_AsiaticModels[l])
					{
						return l;
					}
				}
				break;
			}
			case EHeadModelSet.Caucasic:
			{
				for (int m = 0; m < c_CaucasicModels.Length; m++)
				{
					if (modelId == c_CaucasicModels[m])
					{
						return m;
					}
				}
				break;
			}
			case EHeadModelSet.Latin:
			{
				for (int k = 0; k < c_LatinModels.Length; k++)
				{
					if (modelId == c_LatinModels[k])
					{
						return k;
					}
				}
				break;
			}
			case EHeadModelSet.Female:
			{
				for (int i = 0; i < c_FemaleModels.Length; i++)
				{
					if (modelId == c_FemaleModels[i])
					{
						return i;
					}
				}
				break;
			}
			}
			return -1;
		}

		public static int GetHairModelId(EHairModelSet modelSet, int index)
		{
			switch (modelSet)
			{
			case EHairModelSet.Shaven:
				if (index < c_ShavenModels.Length)
				{
					return c_ShavenModels[index];
				}
				break;
			case EHairModelSet.VeryShort:
				if (index < c_VeryShortModels.Length)
				{
					return c_VeryShortModels[index];
				}
				break;
			case EHairModelSet.Short:
				if (index < c_ShortModels.Length)
				{
					return c_ShortModels[index];
				}
				break;
			case EHairModelSet.Modern:
				if (index < c_ModernModels.Length)
				{
					return c_ModernModels[index];
				}
				break;
			case EHairModelSet.Medium:
				if (index < c_MediumModels.Length)
				{
					return c_MediumModels[index];
				}
				break;
			case EHairModelSet.Long:
				if (index < c_LongModels.Length)
				{
					return c_LongModels[index];
				}
				break;
			case EHairModelSet.Afro:
				if (index < c_AfroModels.Length)
				{
					return c_AfroModels[index];
				}
				break;
			case EHairModelSet.Headbend:
				if (index < c_HeadbendModels.Length)
				{
					return c_HeadbendModels[index];
				}
				break;
			case EHairModelSet.FemaleHair:
				if (index < c_FemaleHairModels.Length)
				{
					return c_FemaleHairModels[index];
				}
				break;
			}
			return -1;
		}

		public static EHairModelSet GetHairModelSet(int modelId)
		{
			for (int i = 0; i < c_ShavenModels.Length; i++)
			{
				if (modelId == c_ShavenModels[i])
				{
					return EHairModelSet.Shaven;
				}
			}
			for (int j = 0; j < c_VeryShortModels.Length; j++)
			{
				if (modelId == c_VeryShortModels[j])
				{
					return EHairModelSet.VeryShort;
				}
			}
			for (int k = 0; k < c_ShortModels.Length; k++)
			{
				if (modelId == c_ShortModels[k])
				{
					return EHairModelSet.Short;
				}
			}
			for (int l = 0; l < c_ModernModels.Length; l++)
			{
				if (modelId == c_ModernModels[l])
				{
					return EHairModelSet.Modern;
				}
			}
			for (int m = 0; m < c_MediumModels.Length; m++)
			{
				if (modelId == c_MediumModels[m])
				{
					return EHairModelSet.Medium;
				}
			}
			for (int n = 0; n < c_LongModels.Length; n++)
			{
				if (modelId == c_LongModels[n])
				{
					return EHairModelSet.Long;
				}
			}
			for (int num = 0; num < c_AfroModels.Length; num++)
			{
				if (modelId == c_AfroModels[num])
				{
					return EHairModelSet.Afro;
				}
			}
			for (int num2 = 0; num2 < c_HeadbendModels.Length; num2++)
			{
				if (modelId == c_HeadbendModels[num2])
				{
					return EHairModelSet.Headbend;
				}
			}
			for (int num3 = 0; num3 < c_FemaleHairModels.Length; num3++)
			{
				if (modelId == c_FemaleHairModels[num3])
				{
					return EHairModelSet.FemaleHair;
				}
			}
			return EHairModelSet.Shaven;
		}

		public static int GetHairModelSetIndex(EHairModelSet modelSet, int modelId)
		{
			switch (modelSet)
			{
			case EHairModelSet.Shaven:
			{
				for (int j = 0; j < c_ShavenModels.Length; j++)
				{
					if (modelId == c_ShavenModels[j])
					{
						return j;
					}
				}
				break;
			}
			case EHairModelSet.VeryShort:
			{
				for (int n = 0; n < c_VeryShortModels.Length; n++)
				{
					if (modelId == c_VeryShortModels[n])
					{
						return n;
					}
				}
				break;
			}
			case EHairModelSet.Short:
			{
				for (int num2 = 0; num2 < c_ShortModels.Length; num2++)
				{
					if (modelId == c_ShortModels[num2])
					{
						return num2;
					}
				}
				break;
			}
			case EHairModelSet.Modern:
			{
				for (int l = 0; l < c_ModernModels.Length; l++)
				{
					if (modelId == c_ModernModels[l])
					{
						return l;
					}
				}
				break;
			}
			case EHairModelSet.Medium:
			{
				for (int num3 = 0; num3 < c_MediumModels.Length; num3++)
				{
					if (modelId == c_MediumModels[num3])
					{
						return num3;
					}
				}
				break;
			}
			case EHairModelSet.Long:
			{
				for (int num = 0; num < c_LongModels.Length; num++)
				{
					if (modelId == c_LongModels[num])
					{
						return num;
					}
				}
				break;
			}
			case EHairModelSet.Afro:
			{
				for (int m = 0; m < c_AfroModels.Length; m++)
				{
					if (modelId == c_AfroModels[m])
					{
						return m;
					}
				}
				break;
			}
			case EHairModelSet.Headbend:
			{
				for (int k = 0; k < c_HeadbendModels.Length; k++)
				{
					if (modelId == c_HeadbendModels[k])
					{
						return k;
					}
				}
				break;
			}
			case EHairModelSet.FemaleHair:
			{
				for (int i = 0; i < c_FemaleHairModels.Length; i++)
				{
					if (modelId == c_FemaleHairModels[i])
					{
						return i;
					}
				}
				break;
			}
			}
			return -1;
		}

		public static int RandomizeModel(EHeadModelSet modelSet)
		{
			switch (modelSet)
			{
			case EHeadModelSet.African:
				return c_AfricanModels[m_Randomizer.Next(c_AfricanModels.Length)];
			case EHeadModelSet.Asiatic:
				return c_AsiaticModels[m_Randomizer.Next(c_AsiaticModels.Length)];
			case EHeadModelSet.Caucasic:
				return c_CaucasicModels[m_Randomizer.Next(c_CaucasicModels.Length)];
			case EHeadModelSet.Latin:
				return c_LatinModels[m_Randomizer.Next(c_LatinModels.Length)];
			default:
				return 1;
			}
		}
	}
}
