using System.IO;

namespace FifaLibrary
{
	public class Compobj : IdObject
	{
		private Compobj m_ParentObj;

		protected ConfederationList m_Confederations;

		protected NationList m_Nations;

		protected TrophyList m_Trophies;

		protected StageList m_Stages;

		protected GroupList m_Groups;

		private int m_TypeNumber;

		private string m_TypeString;

		private string m_Description;

		private CompetitionSettings m_CompetitionSettings;

		public Compobj ParentObj
		{
			get
			{
				return m_ParentObj;
			}
			set
			{
				m_ParentObj = value;
			}
		}

		public ConfederationList Confederations => m_Confederations;

		public NationList Nations => m_Nations;

		public TrophyList Trophies => m_Trophies;

		public StageList Stages => m_Stages;

		public GroupList Groups => m_Groups;

		public int TypeNumber
		{
			get
			{
				return m_TypeNumber;
			}
			set
			{
				m_TypeNumber = value;
			}
		}

		public string TypeString
		{
			get
			{
				return m_TypeString;
			}
			set
			{
				m_TypeString = value;
			}
		}

		public string Description
		{
			get
			{
				return m_Description;
			}
			set
			{
				m_Description = value;
			}
		}

		public CompetitionSettings Settings
		{
			get
			{
				return m_CompetitionSettings;
			}
			set
			{
				m_CompetitionSettings = value;
			}
		}

		public override string ToString()
		{
			return m_TypeString;
		}

		public bool IsWorld()
		{
			return m_TypeNumber == 0;
		}

		public bool IsConfederation()
		{
			return m_TypeNumber == 1;
		}

		public bool IsNation()
		{
			return m_TypeNumber == 2;
		}

		public bool IsTrophy()
		{
			return m_TypeNumber == 3;
		}

		public bool IsStage()
		{
			return m_TypeNumber == 4;
		}

		public bool IsGroup()
		{
			return m_TypeNumber == 5;
		}

		public Compobj(int id, int typeNumber, string typeString, string description, Compobj parentObj)
			: base(id)
		{
			m_TypeString = typeString;
			m_TypeNumber = typeNumber;
			m_Description = description;
			m_ParentObj = parentObj;
			m_CompetitionSettings = new CompetitionSettings(this);
		}

		public bool AddChild(Compobj childObject)
		{
			switch (childObject.TypeNumber)
			{
			case 1:
				m_Confederations.InsertId(childObject);
				break;
			case 2:
				m_Nations.InsertId(childObject);
				break;
			case 3:
				m_Trophies.InsertId(childObject);
				break;
			case 4:
				m_Stages.InsertId(childObject);
				break;
			case 5:
				m_Groups.InsertId(childObject);
				break;
			}
			return true;
		}

		public void SetProperty(string property, string val)
		{
			m_CompetitionSettings.LoadProperty(property, val);
		}

		public int Renumber(int id)
		{
			base.Id = id;
			id++;
			if (m_Trophies != null)
			{
				foreach (Trophy trophy3 in m_Trophies)
				{
					if (!trophy3.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						id = trophy3.Renumber(id);
					}
				}
			}
			if (m_Confederations != null)
			{
				foreach (Confederation confederation in m_Confederations)
				{
					id = confederation.Renumber(id);
				}
			}
			if (m_Nations != null)
			{
				foreach (Nation nation in m_Nations)
				{
					id = nation.Renumber(id);
					nation.Settings.m_rule_suspension = nation.Id;
				}
			}
			if (m_Stages != null)
			{
				foreach (Stage stage in m_Stages)
				{
					id = stage.Renumber(id);
					stage.Settings.UpdateIdUsingStageReference();
				}
			}
			if (m_Groups != null)
			{
				foreach (Group group in m_Groups)
				{
					id = group.Renumber(id);
				}
			}
			if (m_Trophies != null)
			{
				foreach (Trophy trophy4 in m_Trophies)
				{
					if (trophy4.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						id = trophy4.Renumber(id);
					}
				}
				return id;
			}
			return id;
		}

		public bool SaveToCompobj(StreamWriter w)
		{
			int num = (ParentObj != null) ? ParentObj.Id : (-1);
			string value = base.Id.ToString() + "," + m_TypeNumber + "," + m_TypeString + "," + m_Description + "," + num;
			w.WriteLine(value);
			return true;
		}

		public bool SaveRecursivelyToCompobj(StreamWriter w)
		{
			SaveToCompobj(w);
			if (m_Trophies != null)
			{
				foreach (Trophy trophy3 in m_Trophies)
				{
					if (!trophy3.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy3.SaveRecursivelyToCompobj(w);
					}
				}
			}
			if (m_Confederations != null)
			{
				foreach (Confederation confederation in m_Confederations)
				{
					confederation.SaveRecursivelyToCompobj(w);
				}
			}
			if (m_Nations != null)
			{
				foreach (Nation nation in m_Nations)
				{
					nation.SaveRecursivelyToCompobj(w);
				}
			}
			if (m_Stages != null)
			{
				foreach (Stage stage in m_Stages)
				{
					stage.SaveRecursivelyToCompobj(w);
				}
			}
			if (m_Groups != null)
			{
				foreach (Group group in m_Groups)
				{
					group.SaveRecursivelyToCompobj(w);
				}
			}
			if (m_Trophies != null)
			{
				foreach (Trophy trophy4 in m_Trophies)
				{
					if (trophy4.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy4.SaveRecursivelyToCompobj(w);
					}
				}
			}
			return true;
		}

		public virtual bool SaveToCompids(StreamWriter w)
		{
			return false;
		}

		public bool SaveRecursivelyToCompids(StreamWriter w)
		{
			SaveToCompids(w);
			if (m_Trophies != null)
			{
				foreach (Trophy trophy3 in m_Trophies)
				{
					if (!trophy3.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy3.SaveRecursivelyToCompids(w);
					}
				}
			}
			if (m_Confederations != null)
			{
				foreach (Confederation confederation in m_Confederations)
				{
					confederation.SaveRecursivelyToCompids(w);
				}
			}
			if (m_Nations != null)
			{
				foreach (Nation nation in m_Nations)
				{
					nation.SaveRecursivelyToCompids(w);
				}
			}
			if (m_Trophies != null)
			{
				foreach (Trophy trophy4 in m_Trophies)
				{
					if (trophy4.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy4.SaveRecursivelyToCompids(w);
					}
				}
			}
			return true;
		}

		public bool SaveToSettings(StreamWriter w)
		{
			if (w == null)
			{
				return false;
			}
			m_CompetitionSettings.SaveToSettings(base.Id, w);
			return true;
		}

		public bool SaveRecursivelyToSettings(StreamWriter w)
		{
			SaveToSettings(w);
			if (m_Trophies != null)
			{
				foreach (Trophy trophy3 in m_Trophies)
				{
					if (!trophy3.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy3.SaveRecursivelyToSettings(w);
					}
				}
			}
			if (m_Confederations != null)
			{
				foreach (Confederation confederation in m_Confederations)
				{
					confederation.SaveRecursivelyToSettings(w);
				}
			}
			if (m_Nations != null)
			{
				foreach (Nation nation in m_Nations)
				{
					nation.SaveRecursivelyToSettings(w);
				}
			}
			if (m_Stages != null)
			{
				foreach (Stage stage in m_Stages)
				{
					stage.SaveRecursivelyToSettings(w);
				}
			}
			if (m_Groups != null)
			{
				foreach (Group group in m_Groups)
				{
					group.SaveRecursivelyToSettings(w);
				}
			}
			if (m_Trophies != null)
			{
				foreach (Trophy trophy4 in m_Trophies)
				{
					if (trophy4.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy4.SaveRecursivelyToSettings(w);
					}
				}
			}
			return true;
		}

		public virtual bool SaveToStandings(StreamWriter w)
		{
			return false;
		}

		public bool SaveRecursivelyToStandings(StreamWriter w)
		{
			SaveToStandings(w);
			if (m_Trophies != null)
			{
				foreach (Trophy trophy3 in m_Trophies)
				{
					if (!trophy3.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy3.SaveRecursivelyToStandings(w);
					}
				}
			}
			if (m_Confederations != null)
			{
				foreach (Confederation confederation in m_Confederations)
				{
					confederation.SaveRecursivelyToStandings(w);
				}
			}
			if (m_Nations != null)
			{
				foreach (Nation nation in m_Nations)
				{
					nation.SaveRecursivelyToStandings(w);
				}
			}
			if (m_Stages != null)
			{
				foreach (Stage stage in m_Stages)
				{
					stage.SaveRecursivelyToStandings(w);
				}
			}
			if (m_Groups != null)
			{
				foreach (Group group in m_Groups)
				{
					group.SaveRecursivelyToStandings(w);
				}
			}
			if (m_Trophies != null)
			{
				foreach (Trophy trophy4 in m_Trophies)
				{
					if (trophy4.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy4.SaveRecursivelyToStandings(w);
					}
				}
			}
			return true;
		}

		public virtual bool SaveToTasks(StreamWriter w)
		{
			return false;
		}

		public bool SaveRecursivelyToTasks(StreamWriter w)
		{
			SaveToTasks(w);
			if (m_Trophies != null)
			{
				foreach (Trophy trophy3 in m_Trophies)
				{
					if (!trophy3.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy3.SaveRecursivelyToTasks(w);
					}
				}
			}
			if (m_Confederations != null)
			{
				foreach (Confederation confederation in m_Confederations)
				{
					confederation.SaveRecursivelyToTasks(w);
				}
			}
			if (m_Nations != null)
			{
				foreach (Nation nation in m_Nations)
				{
					nation.SaveRecursivelyToTasks(w);
				}
			}
			if (m_Stages != null)
			{
				foreach (Stage stage in m_Stages)
				{
					stage.SaveRecursivelyToTasks(w);
				}
			}
			if (m_Groups != null)
			{
				foreach (Group group in m_Groups)
				{
					group.SaveRecursivelyToTasks(w);
				}
			}
			if (m_Trophies != null)
			{
				foreach (Trophy trophy4 in m_Trophies)
				{
					if (trophy4.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy4.SaveRecursivelyToTasks(w);
					}
				}
			}
			return true;
		}

		public virtual bool SaveToAdvancement(StreamWriter w)
		{
			return false;
		}

		public bool SaveRecursivelyToAdvancement(StreamWriter w)
		{
			SaveToAdvancement(w);
			if (m_Trophies != null)
			{
				foreach (Trophy trophy3 in m_Trophies)
				{
					if (!trophy3.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy3.SaveRecursivelyToAdvancement(w);
					}
				}
			}
			if (m_Confederations != null)
			{
				foreach (Confederation confederation in m_Confederations)
				{
					confederation.SaveRecursivelyToAdvancement(w);
				}
			}
			if (m_Nations != null)
			{
				foreach (Nation nation in m_Nations)
				{
					nation.SaveRecursivelyToAdvancement(w);
				}
			}
			if (m_Stages != null)
			{
				foreach (Stage stage in m_Stages)
				{
					stage.SaveRecursivelyToAdvancement(w);
				}
			}
			if (m_Groups != null)
			{
				foreach (Group group in m_Groups)
				{
					group.SaveRecursivelyToAdvancement(w);
				}
			}
			if (m_Trophies != null)
			{
				foreach (Trophy trophy4 in m_Trophies)
				{
					if (trophy4.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy4.SaveRecursivelyToAdvancement(w);
					}
				}
			}
			return true;
		}

		public virtual bool SaveToSchedule(StreamWriter w)
		{
			return false;
		}

		public bool SaveRecursivelyToSchedule(StreamWriter w)
		{
			SaveToSchedule(w);
			if (m_Trophies != null)
			{
				foreach (Trophy trophy3 in m_Trophies)
				{
					if (!trophy3.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy3.SaveRecursivelyToSchedule(w);
					}
				}
			}
			if (m_Confederations != null)
			{
				foreach (Confederation confederation in m_Confederations)
				{
					confederation.SaveRecursivelyToSchedule(w);
				}
			}
			if (m_Nations != null)
			{
				foreach (Nation nation in m_Nations)
				{
					nation.SaveRecursivelyToSchedule(w);
				}
			}
			if (m_Stages != null)
			{
				foreach (Stage stage in m_Stages)
				{
					stage.SaveRecursivelyToSchedule(w);
				}
			}
			if (m_Groups != null)
			{
				foreach (Group group in m_Groups)
				{
					group.SaveRecursivelyToSchedule(w);
				}
			}
			if (m_Trophies != null)
			{
				foreach (Trophy trophy4 in m_Trophies)
				{
					if (trophy4.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy4.SaveRecursivelyToSchedule(w);
					}
				}
			}
			return true;
		}

		public virtual bool SaveToWeather(StreamWriter w)
		{
			return false;
		}

		public bool SaveRecursivelyToWeather(StreamWriter w)
		{
			SaveToWeather(w);
			if (m_Trophies != null)
			{
				foreach (Trophy trophy3 in m_Trophies)
				{
					if (!trophy3.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy3.SaveRecursivelyToWeather(w);
					}
				}
			}
			if (m_Confederations != null)
			{
				foreach (Confederation confederation in m_Confederations)
				{
					confederation.SaveRecursivelyToWeather(w);
				}
			}
			if (m_Nations != null)
			{
				foreach (Nation nation in m_Nations)
				{
					nation.SaveRecursivelyToWeather(w);
				}
			}
			if (m_Stages != null)
			{
				foreach (Stage stage in m_Stages)
				{
					stage.SaveRecursivelyToWeather(w);
				}
			}
			if (m_Groups != null)
			{
				foreach (Group group in m_Groups)
				{
					group.SaveRecursivelyToWeather(w);
				}
			}
			if (m_Trophies != null)
			{
				foreach (Trophy trophy4 in m_Trophies)
				{
					if (trophy4.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy4.SaveRecursivelyToWeather(w);
					}
				}
			}
			return true;
		}

		public virtual bool SaveToInitteams(StreamWriter w)
		{
			return false;
		}

		public bool SaveRecursivelyToInitteams(StreamWriter w)
		{
			SaveToInitteams(w);
			if (m_Trophies != null)
			{
				foreach (Trophy trophy3 in m_Trophies)
				{
					if (!trophy3.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy3.SaveRecursivelyToInitteams(w);
					}
				}
			}
			if (m_Confederations != null)
			{
				foreach (Confederation confederation in m_Confederations)
				{
					confederation.SaveRecursivelyToInitteams(w);
				}
			}
			if (m_Nations != null)
			{
				foreach (Nation nation in m_Nations)
				{
					nation.SaveRecursivelyToInitteams(w);
				}
			}
			if (m_Stages != null)
			{
				foreach (Stage stage in m_Stages)
				{
					stage.SaveRecursivelyToInitteams(w);
				}
			}
			if (m_Groups != null)
			{
				foreach (Group group in m_Groups)
				{
					group.SaveRecursivelyToInitteams(w);
				}
			}
			if (m_Trophies != null)
			{
				foreach (Trophy trophy4 in m_Trophies)
				{
					if (trophy4.Settings.m_comp_type.Contains("FRIENDLY"))
					{
						trophy4.SaveRecursivelyToInitteams(w);
					}
				}
			}
			return true;
		}

		public virtual bool FillFromLanguage()
		{
			return false;
		}

		public virtual bool SaveToLanguage()
		{
			return false;
		}

		public virtual void LinkLeague(LeagueList leagueList)
		{
		}

		public virtual void LinkTeam(TeamList teamList)
		{
		}

		public virtual void LinkCountry(CountryList countryList)
		{
		}

		public virtual void LinkStadium(StadiumList countryList)
		{
		}

		public virtual void LinkCompetitions()
		{
		}

		public virtual void Normalize()
		{
		}
	}
}
