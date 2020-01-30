using System.Drawing;

namespace FifaLibrary
{
	public class TeamNews : IdObject
	{
		private int m_crestweight;

		private int m_genericweight;

		private int m_teamweight;

		private int m_maxvariationsneg;

		private int m_maxvariationspos;

		private int m_maxvariationsstd;

		private int m_teamId;

		public int maxvariationsneg
		{
			get
			{
				return m_maxvariationsneg;
			}
			set
			{
				m_maxvariationsneg = value;
			}
		}

		public int maxvariationspos
		{
			get
			{
				return m_maxvariationspos;
			}
			set
			{
				m_maxvariationspos = value;
			}
		}

		public int maxvariationsstd
		{
			get
			{
				return m_maxvariationsstd;
			}
			set
			{
				m_maxvariationsstd = value;
			}
		}

		public int teamId
		{
			get
			{
				return m_teamId;
			}
			set
			{
				m_teamId = value;
			}
		}

		public TeamNews(int teamId)
			: base(teamId)
		{
			m_crestweight = 25;
			m_genericweight = 25;
			m_teamweight = 50;
			m_maxvariationsneg = 0;
			m_maxvariationspos = 0;
			m_maxvariationsstd = 0;
		}

		public TeamNews(Record r)
			: base(r.IntField[FI.career_newspicweights_teamid])
		{
			m_crestweight = r.GetAndCheckIntField(FI.career_newspicweights_crestweight);
			m_genericweight = r.GetAndCheckIntField(FI.career_newspicweights_genericweight);
			m_teamweight = r.GetAndCheckIntField(FI.career_newspicweights_teamweight);
			m_maxvariationsneg = r.GetAndCheckIntField(FI.career_newspicweights_maxvariationsneg);
			m_maxvariationspos = r.GetAndCheckIntField(FI.career_newspicweights_maxvariationspos);
			m_maxvariationsstd = r.GetAndCheckIntField(FI.career_newspicweights_maxvariationsstd);
			m_teamId = r.GetAndCheckIntField(FI.career_newspicweights_teamid);
		}

		public void Save(Record r)
		{
			r.IntField[FI.career_newspicweights_teamid] = base.Id;
			r.IntField[FI.career_newspicweights_crestweight] = m_crestweight;
			r.IntField[FI.career_newspicweights_genericweight] = m_genericweight;
			r.IntField[FI.career_newspicweights_teamweight] = m_teamweight;
			r.IntField[FI.career_newspicweights_maxvariationsneg] = m_maxvariationsneg;
			r.IntField[FI.career_newspicweights_maxvariationspos] = m_maxvariationspos;
			r.IntField[FI.career_newspicweights_maxvariationsstd] = m_maxvariationsstd;
		}

		public static string TeamNewsDdsFileName(int teamid, int newsType, int order)
		{
			return "data/ui/imgassets/cmnews/nw_" + teamid.ToString() + "_" + newsType.ToString() + "_" + order.ToString() + ".dds";
		}

		public static Bitmap GetTeamNews(int teamid, int newsType, int order)
		{
			return FifaEnvironment.GetDdsArtasset(TeamNewsDdsFileName(teamid, newsType, order));
		}

		public static string TeamNewsDdsTemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/imgassets/cmnews/2014_nw_#_%_@.dds";
			}
			return "data/ui/imgassets/cmnews/nw_#_%_@.dds";
		}

		public static bool SetTeamNews(int teamid, int newsType, int order, Bitmap bitmap)
		{
			int[] array = new int[3];
			string[] array2 = new string[3];
			array[0] = teamid;
			array[1] = newsType;
			array[2] = order;
			array2[0] = (array2[1] = (array2[2] = "D"));
			return FifaEnvironment.SetDdsArtasset(TeamNewsDdsTemplateFileName(), array, bitmap, array2);
		}

		public static bool DeleteTeamNews(int teamid, int newsType, int order)
		{
			return FifaEnvironment.DeleteFromZdata(TeamNewsDdsFileName(teamid, newsType, order));
		}
	}
}
