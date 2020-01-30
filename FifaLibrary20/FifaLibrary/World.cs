using System.IO;

namespace FifaLibrary
{
	public class World : Compobj
	{
		private int[] m_DryProb = new int[12];

		private int[] m_ClearProb = new int[12];

		private int[] m_HazyProb = new int[12];

		private int[] m_CloudyProb = new int[12];

		private int[] m_RainProb = new int[12];

		private int[] m_ShowersProb = new int[12];

		private int[] m_SnowProb = new int[12];

		private int[] m_FlurriesProb = new int[12];

		private int[] m_OvercastProb = new int[12];

		private int[] m_FoggyProb = new int[12];

		private int[] m_SunsetTime = new int[12];

		private int[] m_DarkTime = new int[12];

		public int[] ClearProb
		{
			get
			{
				return m_ClearProb;
			}
			set
			{
				m_ClearProb = value;
			}
		}

		public int[] HazyProb
		{
			get
			{
				return m_HazyProb;
			}
			set
			{
				m_HazyProb = value;
			}
		}

		public int[] CloudyProb
		{
			get
			{
				return m_CloudyProb;
			}
			set
			{
				m_CloudyProb = value;
			}
		}

		public int[] RainProb
		{
			get
			{
				return m_RainProb;
			}
			set
			{
				m_RainProb = value;
			}
		}

		public int[] ShowersProb
		{
			get
			{
				return m_ShowersProb;
			}
			set
			{
				m_ShowersProb = value;
			}
		}

		public int[] SnowProb
		{
			get
			{
				return m_SnowProb;
			}
			set
			{
				m_SnowProb = value;
			}
		}

		public int[] FlurriesProb
		{
			get
			{
				return m_FlurriesProb;
			}
			set
			{
				m_FlurriesProb = value;
			}
		}

		public int[] OvercastProb
		{
			get
			{
				return m_OvercastProb;
			}
			set
			{
				m_OvercastProb = value;
			}
		}

		public int[] FoggyProb
		{
			get
			{
				return m_FoggyProb;
			}
			set
			{
				m_FoggyProb = value;
			}
		}

		public int[] SunsetTime
		{
			get
			{
				return m_SunsetTime;
			}
			set
			{
				m_SunsetTime = value;
			}
		}

		public int[] DarkTime
		{
			get
			{
				return m_DarkTime;
			}
			set
			{
				m_DarkTime = value;
			}
		}

		public World(int id, string typeString, string description)
			: base(id, 0, typeString, description, null)
		{
			m_Confederations = new ConfederationList();
			m_Trophies = new TrophyList();
		}

		public override bool SaveToWeather(StreamWriter w)
		{
			if (w == null)
			{
				return false;
			}
			for (int i = 0; i < 12; i++)
			{
				if (m_ClearProb[i] + m_HazyProb[i] + m_CloudyProb[i] + m_RainProb[i] + m_ShowersProb[i] + m_SnowProb[i] + m_FlurriesProb[i] + m_OvercastProb[i] + m_FoggyProb[i] == 100)
				{
					string value = base.Id.ToString() + "," + (i + 1).ToString() + "," + m_ClearProb[i].ToString() + "," + m_HazyProb[i].ToString() + "," + m_CloudyProb[i].ToString() + "," + m_RainProb[i].ToString() + "," + m_ShowersProb[i].ToString() + "," + m_SnowProb[i].ToString() + "," + m_FlurriesProb[i].ToString() + "," + m_OvercastProb[i].ToString() + "," + m_FoggyProb[i].ToString() + "," + m_SunsetTime[i].ToString() + "," + m_DarkTime[i].ToString();
					w.WriteLine(value);
				}
			}
			return true;
		}
	}
}
