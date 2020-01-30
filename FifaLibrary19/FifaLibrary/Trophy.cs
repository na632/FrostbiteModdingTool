using System;
using System.Drawing;
using System.IO;

namespace FifaLibrary
{
	public class Trophy : Compobj
	{
		private int m_ballid = -1;

		private string m_ShortName;

		private string m_LongName;

		private InitTeam[] m_InitTeamArray = new InitTeam[48];

		public Task[] m_StartTask = new Task[48];

		public int m_NStartTasks;

		public Task[] m_EndTask = new Task[48];

		public int m_NEndTasks;

		public int ballid
		{
			get
			{
				return m_ballid;
			}
			set
			{
				m_ballid = value;
			}
		}

		public Nation Nation
		{
			get
			{
				if (base.ParentObj.TypeNumber == 2)
				{
					return (Nation)base.ParentObj;
				}
				return null;
			}
		}

		public Confederation Confederation
		{
			get
			{
				if (base.ParentObj.TypeNumber == 1)
				{
					return (Confederation)base.ParentObj;
				}
				return null;
			}
		}

		public string ShortName
		{
			get
			{
				return m_ShortName;
			}
			set
			{
				m_ShortName = value;
			}
		}

		public string LongName
		{
			get
			{
				return m_LongName;
			}
			set
			{
				m_LongName = value;
			}
		}

		public InitTeam[] InitTeamArray
		{
			get
			{
				return m_InitTeamArray;
			}
			set
			{
				m_InitTeamArray = value;
			}
		}

		public override string ToString()
		{
			if (m_LongName != null && m_LongName != string.Empty)
			{
				return m_LongName;
			}
			if (m_ShortName != null && m_ShortName != string.Empty)
			{
				return m_ShortName;
			}
			return "Trophy n. " + base.Settings.m_asset_id.ToString();
		}

		public bool SetLanguageName(string languageName)
		{
			if (FifaEnvironment.Language != null && base.Description != null)
			{
				FifaEnvironment.Language.SetString(base.Description, languageName);
				return true;
			}
			return false;
		}

		public override void LinkLeague(LeagueList leagueList)
		{
			if (leagueList != null)
			{
				if (base.Settings.m_info_league_promo >= 0)
				{
					base.Settings.LeaguePromo = (League)leagueList.SearchId(base.Settings.m_info_league_promo);
				}
				else
				{
					base.Settings.LeaguePromo = null;
				}
				if (base.Settings.m_info_league_releg >= 0)
				{
					base.Settings.LeagueReleg = (League)leagueList.SearchId(base.Settings.m_info_league_releg);
				}
				else
				{
					base.Settings.LeagueReleg = null;
				}
			}
		}

		public override void LinkTeam(TeamList teamList)
		{
			for (int i = 0; i < m_InitTeamArray.Length; i++)
			{
				if (m_InitTeamArray[i] != null)
				{
					m_InitTeamArray[i].LinkTeam(teamList);
				}
			}
		}

		public Trophy(int id, string typeString, string description, Compobj parentObj)
			: base(id, 3, typeString, description, parentObj)
		{
			m_Stages = new StageList();
		}

		public void FillFromCompetition(Table t)
		{
			m_ballid = -1;
			int num = 0;
			Record record;
			while (true)
			{
				if (num < t.NRecords)
				{
					record = t.Records[num];
					if (record.GetAndCheckIntField(FI.competition_competitionid) == base.Settings.m_asset_id)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			m_ballid = record.GetAndCheckIntField(FI.competition_ballid);
		}

		public void SaveCompetition(Record r)
		{
			if (m_ballid >= 0)
			{
				r.IntField[FI.competition_competitionid] = base.Settings.m_asset_id;
				r.IntField[FI.competition_ballid] = m_ballid;
			}
		}

		public override bool SaveToInitteams(StreamWriter w)
		{
			for (int i = 0; i < m_NEndTasks; i++)
			{
				Task task = m_EndTask[i];
				if (task.Action == "UpdateTable")
				{
					int num = task.Parameter3 - 1;
					InitTeam initTeam = m_InitTeamArray[num];
					if (initTeam != null)
					{
						string text = null;
						text = ((initTeam.teamid < 0) ? (base.Id.ToString() + "," + num.ToString() + ",-1") : (base.Id.ToString() + "," + num.ToString() + "," + initTeam.teamid.ToString()));
						w.WriteLine(text);
					}
				}
			}
			return true;
		}

		public override bool SaveToCompids(StreamWriter w)
		{
			if (w == null)
			{
				return false;
			}
			string value = base.Id.ToString();
			w.WriteLine(value);
			return true;
		}

		public override bool SaveToTasks(StreamWriter w)
		{
			if (w == null)
			{
				return false;
			}
			for (int i = 0; i < m_NStartTasks; i++)
			{
				m_StartTask[i].SaveToTasks(w);
			}
			for (int j = 0; j < m_NEndTasks; j++)
			{
				m_EndTask[j].SaveToTasks(w);
			}
			return true;
		}

		public static int AutoAsset()
		{
			for (int i = 1; i < 9999; i++)
			{
				bool flag = false;
				foreach (Compobj competitionObject in FifaEnvironment.CompetitionObjects)
				{
					if (competitionObject.Settings.m_asset_id == i)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					foreach (League league in FifaEnvironment.Leagues)
					{
						if (league.Id == i)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return i;
					}
				}
			}
			return 0;
		}

		public override bool FillFromLanguage()
		{
			if (FifaEnvironment.Language != null)
			{
				string value = base.TypeString.Substring(1);
				int assetId;
				try
				{
					assetId = Convert.ToInt32(value);
				}
				catch
				{
					m_ShortName = string.Empty;
					m_LongName = string.Empty;
					return false;
				}
				m_ShortName = FifaEnvironment.Language.GetTournamentString(assetId, Language.ETournamentStringType.Abbr15);
				m_LongName = FifaEnvironment.Language.GetTournamentString(assetId, Language.ETournamentStringType.Full);
				if (m_LongName == null && m_ShortName == null)
				{
					m_ShortName = FifaEnvironment.Language.GetString(base.Description);
				}
				if (m_LongName == null)
				{
					m_LongName = string.Empty;
				}
				if (m_ShortName == null)
				{
					m_ShortName = string.Empty;
				}
				if (m_LongName == string.Empty)
				{
					m_LongName = m_ShortName;
				}
				if (m_ShortName == string.Empty)
				{
					if (m_LongName.Length > 15)
					{
						m_ShortName = m_LongName.Substring(0, 15);
					}
					else
					{
						m_ShortName = m_LongName;
					}
				}
			}
			else
			{
				m_ShortName = string.Empty;
				m_LongName = string.Empty;
			}
			return true;
		}

		public override bool SaveToLanguage()
		{
			int assetId = Convert.ToInt32(base.TypeString.Substring(1));
			if (FifaEnvironment.Language != null)
			{
				FifaEnvironment.Language.SetTournamentString(assetId, Language.ETournamentStringType.Abbr15, m_ShortName);
				FifaEnvironment.Language.SetTournamentString(assetId, Language.ETournamentStringType.Full, m_LongName);
				return true;
			}
			return false;
		}

		public bool InsertStage(int stageIndex)
		{
			if (stageIndex > base.Stages.Count)
			{
				return false;
			}
			string typeString = "S" + (stageIndex + 1).ToString();
			Stage stage = new Stage(FifaEnvironment.CompetitionObjects.GetNewId(), typeString, "FCE_Setup_Stage", this);
			if (stage == null)
			{
				return false;
			}
			stage.Settings.m_match_stagetype = "SETUP";
			base.Stages.Insert(stageIndex, stage);
			FifaEnvironment.CompetitionObjects.Add(stage);
			for (int i = 0; i < base.Stages.Count; i++)
			{
				typeString = "S" + (i + 1).ToString();
				((Stage)base.Stages[i]).TypeString = typeString;
			}
			return true;
		}

		public bool RemoveStage(Stage stage)
		{
			int num = base.Stages.IndexOf(stage);
			if (num < 0)
			{
				return false;
			}
			RemoveStage(num);
			return true;
		}

		public bool RemoveStage(int stageIndex)
		{
			if (stageIndex > base.Stages.Count)
			{
				return false;
			}
			Stage idObject = (Stage)base.Stages[stageIndex];
			base.Stages.RemoveAt(stageIndex);
			FifaEnvironment.CompetitionObjects.RemoveId(idObject);
			for (int i = 0; i < base.Stages.Count; i++)
			{
				string typeString = "S" + (i + 1).ToString();
				((Stage)base.Stages[i]).TypeString = typeString;
			}
			return true;
		}

		public bool AddStage()
		{
			string typeString = "S" + (base.Stages.Count + 1).ToString();
			Stage stage = new Stage(FifaEnvironment.CompetitionObjects.GetNewId(), typeString, string.Empty, this);
			if (stage == null)
			{
				return false;
			}
			base.Stages.Add(stage);
			FifaEnvironment.CompetitionObjects.Add(stage);
			return true;
		}

		public bool RemoveStage()
		{
			if (base.Stages.Count < 1)
			{
				return false;
			}
			Stage stage = (Stage)base.Stages[base.Stages.Count - 1];
			base.Stages.Remove(stage);
			FifaEnvironment.CompetitionObjects.Remove(stage);
			stage.RemoveAllGroups();
			return true;
		}

		public bool RemoveAllStages()
		{
			int count = base.Stages.Count;
			for (int i = 0; i < count; i++)
			{
				RemoveStage();
			}
			return true;
		}

		public override void Normalize()
		{
			if (base.Settings.m_match_matchsituation != null)
			{
				foreach (Stage stage3 in m_Stages)
				{
					if (stage3.Settings.m_match_stagetype != "SETUP" && stage3.Settings.m_match_matchsituation == null)
					{
						stage3.Settings.m_match_matchsituation = base.Settings.m_match_matchsituation;
					}
				}
				base.Settings.m_match_matchsituation = null;
			}
			if (base.Settings.m_N_endruleko1leg != 0)
			{
				foreach (Stage stage4 in m_Stages)
				{
					if (stage4.Settings.m_match_stagetype != "SETUP")
					{
						for (int i = 0; i < base.Settings.m_N_endruleko1leg; i++)
						{
							if (base.Settings.m_match_endruleko1leg[i] != null && stage4.Settings.m_match_endruleko1leg[i] == null)
							{
								stage4.Settings.m_match_endruleko1leg[i] = base.Settings.m_match_endruleko1leg[i];
							}
						}
					}
				}
				for (int j = 0; j < base.Settings.m_N_endruleko1leg; j++)
				{
					base.Settings.m_match_endruleko1leg[j] = null;
				}
				base.Settings.m_N_endruleko1leg = 0;
			}
		}

		public void LinkTaskGroup()
		{
		}

		public override void LinkCompetitions()
		{
			_ = base.Settings.TrophyCompdependency;
			_ = base.Settings.TrophyForcecomp;
			for (int i = 0; i < m_NStartTasks; i++)
			{
				m_StartTask[i].LinkTrophy(this);
			}
			for (int j = 0; j < m_NEndTasks; j++)
			{
				m_EndTask[j].LinkTrophy(this);
			}
		}

		public bool AddTask(Task action)
		{
			if (action.When == "start")
			{
				if (m_NStartTasks == 0)
				{
					m_StartTask = new Task[48];
				}
				if (m_NStartTasks < m_StartTask.Length)
				{
					m_StartTask[m_NStartTasks] = action;
					m_NStartTasks++;
					return true;
				}
			}
			else if (action.When == "end")
			{
				if (m_NEndTasks == 0)
				{
					m_EndTask = new Task[48];
				}
				if (m_NEndTasks < m_EndTask.Length)
				{
					m_EndTask[m_NEndTasks] = action;
					m_NEndTasks++;
					return true;
				}
			}
			return false;
		}

		public bool RemoveLastTask(string when)
		{
			if (when == "start")
			{
				if (m_NStartTasks > 0)
				{
					m_NStartTasks--;
					m_StartTask[m_NStartTasks] = null;
					return true;
				}
				return false;
			}
			if (when == "end")
			{
				if (m_NEndTasks > 0)
				{
					m_NEndTasks--;
					m_EndTask[m_NEndTasks] = null;
					return true;
				}
				return false;
			}
			return false;
		}

		public int SearchTaskIndex(string when, string action, int par1, int par2, int par3)
		{
			if (when == "start")
			{
				for (int i = 0; i < m_NStartTasks; i++)
				{
					if ((action == null || m_StartTask[i].Action == action) && (par1 < 0 || m_StartTask[i].Parameter1 == par1) && (par2 < 0 || m_StartTask[i].Parameter2 == par2) && (par3 < 0 || m_StartTask[i].Parameter3 == par3))
					{
						return i;
					}
				}
			}
			else if (when == "end")
			{
				for (int j = 0; j < m_NEndTasks; j++)
				{
					if ((action == null || m_EndTask[j].Action == action) && (par1 < 0 || m_EndTask[j].Parameter1 == par1) && (par2 < 0 || m_EndTask[j].Parameter2 == par2) && (par3 < 0 || m_EndTask[j].Parameter3 == par3))
					{
						return j;
					}
				}
			}
			return -1;
		}

		public Task SearchTask(string when, string action, int par1, int par2, int par3)
		{
			int num = SearchTaskIndex(when, action, par1, par2, par3);
			if (num >= 0)
			{
				return GetTask(when, num);
			}
			return null;
		}

		public bool RemoveTask(string when, int index)
		{
			if (when == "start")
			{
				if (index < m_NStartTasks)
				{
					m_NStartTasks--;
					for (int i = index; i < m_NStartTasks; i++)
					{
						m_StartTask[i] = m_StartTask[i + 1];
					}
					return true;
				}
			}
			else if (index < m_NEndTasks)
			{
				m_NEndTasks--;
				for (int j = index; j < m_NEndTasks; j++)
				{
					m_EndTask[j] = m_EndTask[j + 1];
				}
				return true;
			}
			return false;
		}

		public bool RemoveTask(string when, string action, int par1, int par2, int par3)
		{
			int num = SearchTaskIndex(when, action, par1, par2, par3);
			if (num >= 0)
			{
				return RemoveTask(when, num);
			}
			return false;
		}

		public bool RemoveTask(Task task)
		{
			return RemoveTask(task.When, task.Action, task.Parameter1, task.Parameter2, task.Parameter3);
		}

		public bool ReplaceTask(Task task, int index)
		{
			if (task.When == "start")
			{
				if (index < m_NStartTasks)
				{
					m_StartTask[index] = task;
					return true;
				}
			}
			else if (index < m_NEndTasks)
			{
				m_EndTask[index] = task;
				return true;
			}
			return false;
		}

		public bool ReplaceTask(Task currentTask, Task newTask)
		{
			int num = SearchTaskIndex(currentTask.When, currentTask.Action, currentTask.Parameter1, currentTask.Parameter2, currentTask.Parameter3);
			if (num >= 0)
			{
				return ReplaceTask(newTask, num);
			}
			return false;
		}

		public Task GetTask(string when, int index)
		{
			if (when == "start")
			{
				if (index < m_NStartTasks)
				{
					return m_StartTask[index];
				}
			}
			else if (index < m_NEndTasks)
			{
				return m_EndTask[index];
			}
			return null;
		}

		public void CopyTasks(Trophy newTrophy, League targetLeague)
		{
			for (int i = 0; i < m_NStartTasks; i++)
			{
				newTrophy.AddTask(m_StartTask[i].CopyTask(newTrophy, targetLeague));
			}
			for (int j = 0; j < m_NEndTasks; j++)
			{
				newTrophy.AddTask(m_EndTask[j].CopyTask(newTrophy, targetLeague));
			}
		}

		public static string TrophyDdsFileName(int id)
		{
			return "data/ui/imgassets/trophy/t" + id.ToString() + ".dds";
		}

		public string TrophyTemplateFileName()
		{
			return "data/ui/imgassets/trophy/t#.dds";
		}

		public string TrophyDdsFileName()
		{
			return TrophyDdsFileName(base.Settings.m_asset_id);
		}

		public Bitmap GetTrophy()
		{
			return FifaEnvironment.GetDdsArtasset(TrophyDdsFileName());
		}

		public bool SetTrophy(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(TrophyTemplateFileName(), base.Settings.m_asset_id, bitmap);
		}

		public bool DeleteTrophy()
		{
			return FifaEnvironment.DeleteFromZdata(TrophyDdsFileName());
		}

		public Bitmap GetAdboard()
		{
			return Adboard.GetRevModTournamentAdboard(base.Settings.m_asset_id);
		}

		public bool SetAdboard(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return Adboard.SetRevModTournamentAdboard(base.Settings.m_asset_id, bitmap);
		}

		public bool DeleteAdboard()
		{
			return Adboard.DeleteRevModTournamentAdboard(base.Settings.m_asset_id);
		}

		private static string PitchDressingFileName(int assetId)
		{
			return "data/sceneassets/tournament/tournament_" + assetId.ToString() + "_0.rx3";
		}

		public Bitmap GetPitchDressing()
		{
			return FifaEnvironment.GetBmpFromRx3(PitchDressingFileName(base.Settings.m_asset_id), verbose: false);
		}

		public bool SetPitchDressing(Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata("data/sceneassets/tournament/tournament_#_0.rx3", base.Settings.m_asset_id, bitmap, ECompressionMode.None, PitchDressingSignature(base.Settings.m_asset_id));
		}

		public bool DeletePitchDressing()
		{
			return FifaEnvironment.DeleteFromZdata(PitchDressingFileName(base.Settings.m_asset_id));
		}

		public static Rx3Signatures PitchDressingSignature(int id)
		{
			return new Rx3Signatures(349856, 32, new string[1]
			{
				"tournament_" + id.ToString()
			});
		}

		public static string TrophyDdsFileName256(int id)
		{
			return "data/ui/imgassets/trophy256x256/t" + id.ToString() + ".dds";
		}

		public string TrophyTemplateFileName256()
		{
			return "data/ui/imgassets/trophy256x256/t#.dds";
		}

		public string TrophyDdsFileName256()
		{
			return TrophyDdsFileName256(base.Settings.m_asset_id);
		}

		public Bitmap GetTrophy256()
		{
			return FifaEnvironment.GetDdsArtasset(TrophyDdsFileName256());
		}

		public bool SetTrophy256(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(TrophyTemplateFileName256(), base.Settings.m_asset_id, bitmap);
		}

		public bool DeleteTrophy256()
		{
			return FifaEnvironment.DeleteFromZdata(TrophyDdsFileName256());
		}

		public static string TrophyDdsFileName128(int id)
		{
			return "data/ui/imgassets/trophy128x128/t" + id.ToString() + ".dds";
		}

		public string TrophyTemplateFileName128()
		{
			return "data/ui/imgassets/trophy128x128/t#.dds";
		}

		public string TrophyDdsFileName128()
		{
			return TrophyDdsFileName128(base.Settings.m_asset_id);
		}

		public Bitmap GetTrophy128()
		{
			return FifaEnvironment.GetDdsArtasset(TrophyDdsFileName128());
		}

		public bool SetTrophy128(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(TrophyTemplateFileName128(), base.Settings.m_asset_id, bitmap);
		}

		public bool DeleteTrophy128()
		{
			return FifaEnvironment.DeleteFromZdata(TrophyDdsFileName128());
		}

		public static string TexturesFileName(int id)
		{
			return "data/sceneassets/trophy/trophy_" + id.ToString() + "_textures.rx3";
		}

		public string TexturesFileName()
		{
			return TexturesFileName(base.Settings.m_asset_id);
		}

		public string TexturesTemplateFileName()
		{
			return "data/sceneassets/trophy/trophy_#_textures.rx3";
		}

		public Bitmap[] GetTextures()
		{
			return FifaEnvironment.GetBmpsFromRx3(TexturesFileName());
		}

		public bool SetTextures(Bitmap[] bitmaps)
		{
			if (bitmaps == null)
			{
				return false;
			}
			return FifaEnvironment.ImportBmpsIntoZdata(TexturesTemplateFileName(), base.Settings.m_asset_id, bitmaps, ECompressionMode.Chunkzip2);
		}

		public bool SetTextures(string rx3FileName)
		{
			if (rx3FileName == null)
			{
				return false;
			}
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, TexturesFileName(), delete: false, ECompressionMode.Chunkzip);
		}

		public bool DeleteContainer(int timeofday)
		{
			return FifaEnvironment.DeleteFromZdata(TexturesFileName());
		}

		public static string ModelFileName(int trophyId)
		{
			return "data/sceneassets/trophy/trophy_" + trophyId.ToString() + ".rx3";
		}

		public string ModelFileName()
		{
			return ModelFileName(base.Settings.m_asset_id);
		}

		public string ModelTemplateFileName()
		{
			return "data/sceneassets/trophy/trophy_#.rx3";
		}

		public Rx3File GetModel()
		{
			Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float32;
			return FifaEnvironment.GetRx3FromZdata(ModelFileName());
		}

		public string ExportModelFile()
		{
			if (FifaEnvironment.ExportFileFromZdata(ModelFileName(), FifaEnvironment.ExportFolder))
			{
				return FifaEnvironment.ExportFolder + "\\" + ModelFileName();
			}
			return null;
		}

		public bool SetModel(string rx3FileName)
		{
			if (rx3FileName == null)
			{
				return false;
			}
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, ModelFileName(), delete: false, ECompressionMode.Chunkzip);
		}

		public bool DeleteModel()
		{
			return FifaEnvironment.DeleteFromZdata(ModelFileName());
		}
	}
}
