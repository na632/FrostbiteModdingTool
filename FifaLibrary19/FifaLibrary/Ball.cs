using System.Drawing;

namespace FifaLibrary
{
	public class Ball : IdObject
	{
		private bool m_IsLicensed;

		private bool m_IsAvailable;

		private bool m_IsEmbargoed;

		private int m_powid;

		private string m_LanguageName;

		private bool m_IsGeneric;

		public bool IsLicensed
		{
			get
			{
				return m_IsLicensed;
			}
			set
			{
				m_IsLicensed = value;
			}
		}

		public bool IsAvailable
		{
			get
			{
				return m_IsAvailable;
			}
			set
			{
				m_IsAvailable = value;
			}
		}

		public bool IsEmbargoed
		{
			get
			{
				return m_IsEmbargoed;
			}
			set
			{
				m_IsEmbargoed = value;
			}
		}

		public int powid
		{
			get
			{
				return m_powid;
			}
			set
			{
				m_powid = value;
			}
		}

		public string Name
		{
			get
			{
				return m_LanguageName;
			}
			set
			{
				m_LanguageName = value;
				FifaEnvironment.Language.SetBallName(base.Id, m_LanguageName, m_IsGeneric);
			}
		}

		public bool IsGeneric
		{
			get
			{
				return m_IsGeneric;
			}
			set
			{
				if (value != m_IsGeneric)
				{
					FifaEnvironment.Language.RemoveBallName(base.Id);
					FifaEnvironment.Language.SetBallName(base.Id, m_LanguageName, value);
				}
				m_IsGeneric = value;
			}
		}

		public Ball(int ballId)
			: base(ballId)
		{
			m_LanguageName = FifaEnvironment.Language.GetBallName(base.Id, out m_IsGeneric);
			if (m_LanguageName == null)
			{
				m_LanguageName = "Ball n." + base.Id.ToString();
				m_IsGeneric = false;
				FifaEnvironment.Language.SetBallName(base.Id, m_LanguageName, m_IsGeneric);
			}
			m_IsLicensed = false;
			m_IsAvailable = true;
			m_IsEmbargoed = false;
			m_powid = -1;
		}

		public Ball(Record r)
			: base(r.IntField[FI.teamballs_ballid])
		{
			m_LanguageName = FifaEnvironment.Language.GetBallName(base.Id, out m_IsGeneric);
			if (m_LanguageName == null)
			{
				m_LanguageName = "Ball n." + base.Id.ToString();
				m_IsGeneric = false;
				FifaEnvironment.Language.SetBallName(base.Id, m_LanguageName, m_IsGeneric);
			}
			m_IsLicensed = ((r.GetAndCheckIntField(FI.teamballs_islicensed) != 0) ? true : false);
			m_IsAvailable = ((r.GetAndCheckIntField(FI.teamballs_isavailableinstore) != 0) ? true : false);
			m_IsEmbargoed = ((r.GetAndCheckIntField(FI.teamballs_isembargoed) != 0) ? true : false);
			m_powid = r.GetAndCheckIntField(FI.teamballs_powid);
		}

		public void SaveBall(Record r)
		{
			r.IntField[FI.teamballs_ballid] = base.Id;
			r.IntField[FI.teamballs_islicensed] = (m_IsLicensed ? 1 : 0);
			r.IntField[FI.teamballs_isavailableinstore] = (m_IsAvailable ? 1 : 0);
			r.IntField[FI.teamballs_isembargoed] = 0;
			r.IntField[FI.teamballs_powid] = -1;
		}

		public override string ToString()
		{
			return m_LanguageName;
		}

		public static string BallTextureFileName(int ballId)
		{
			return "data/sceneassets/ball/ball_" + ballId.ToString() + "_textures.rx3";
		}

		public static string RevModTeamBallTextureFileName(int teamId)
		{
			return "data/sceneassets/ball/specificball_" + teamId.ToString() + "_0_0_textures.rx3";
		}

		public static string RevModTrophyBallTextureFileName(int assetId)
		{
			return "data/sceneassets/ball/specificball_0_" + assetId.ToString() + "_0_textures.rx3";
		}

		public string BallTextureFileName()
		{
			return BallTextureFileName(base.Id);
		}

		public static Bitmap GetBallTexture(int ballId)
		{
			return FifaEnvironment.GetBmpFromRx3(BallTextureFileName(ballId), 0);
		}

		public Bitmap GetBallTexture()
		{
			return GetBallTexture(base.Id);
		}

		public static Bitmap[] GetBallTextures(int ballId)
		{
			return FifaEnvironment.GetBmpsFromRx3(BallTextureFileName(ballId));
		}

		public static Bitmap[] GetRevModTeamBallTextures(int teamId)
		{
			return FifaEnvironment.GetBmpsFromRx3(RevModTeamBallTextureFileName(teamId), verbose: false);
		}

		public static Bitmap[] GetRevModTrophyBallTextures(int tournamentAssetId)
		{
			return FifaEnvironment.GetBmpsFromRx3(RevModTrophyBallTextureFileName(tournamentAssetId), verbose: false);
		}

		public Bitmap[] GetBallTextures()
		{
			return GetBallTextures(base.Id);
		}

		public static string BallTextureTemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data\\sceneassets\\ball\\2014_ball_#_textures.rx3";
			}
			return "data\\sceneassets\\ball\\ball_#_textures.rx3";
		}

		public static bool SetBallTextures(int ballId, Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(BallTextureTemplateFileName(), ballId, bitmaps, ECompressionMode.Chunkzip);
		}

		public static bool SetRevModTeamBallTextures(int teamId, Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoZdata("data\\sceneassets\\ball\\specificball_#_0_0_textures.rx3", teamId, bitmaps, ECompressionMode.None);
		}

		public static bool SetRevModTrophyBallTextures(int assetId, Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoZdata("data\\sceneassets\\ball\\specificball_0_#_0_textures.rx3", assetId, bitmaps, ECompressionMode.None);
		}

		public bool SetBallTextures(Bitmap[] bitmaps)
		{
			return SetBallTextures(base.Id, bitmaps);
		}

		public static bool SetBallTextures(int ballId, string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, BallTextureFileName(ballId), delete: false, ECompressionMode.Chunkzip);
		}

		public static bool SetRevModTeamBallTextures(int teamId, string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, RevModTeamBallTextureFileName(teamId), delete: false, ECompressionMode.None);
		}

		public static bool SetRevModTrophyBallTextures(int assetId, string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, RevModTrophyBallTextureFileName(assetId), delete: false, ECompressionMode.None);
		}

		public bool SetBallTextures(string srcFileName)
		{
			return SetBallTextures(base.Id, srcFileName);
		}

		public bool DeleteBallTextures()
		{
			return FifaEnvironment.DeleteFromZdata(BallTextureFileName());
		}

		public static bool DeleteRevModTrophyBallTextures(int assetId)
		{
			return FifaEnvironment.DeleteFromZdata(RevModTrophyBallTextureFileName(assetId));
		}

		public static bool DeleteRevModTeamBallTextures(int teamId)
		{
			return FifaEnvironment.DeleteFromZdata(RevModTeamBallTextureFileName(teamId));
		}

		public static string BallPictureTemplateBigFileName()
		{
			return "data/ui/artassets/settingsimg/ball_#.big";
		}

		public static string BallPictureTemplateDdsName()
		{
			return "2";
		}

		public static string BallPictureBigFileName(int ballId)
		{
			return "data/ui/artassets/settingsimg/ball_" + ballId.ToString() + ".big";
		}

		public string BallPictureBigFileName()
		{
			return BallPictureBigFileName(base.Id);
		}

		public static Bitmap GetBallPicture(int ballId)
		{
			if (FifaEnvironment.Year == 14)
			{
				return FifaEnvironment.GetArtasset(BallPictureBigFileName(ballId));
			}
			return FifaEnvironment.GetDdsArtasset(BallDdsFileName(ballId));
		}

		public Bitmap GetBallPicture()
		{
			return GetBallPicture(base.Id);
		}

		public bool DeleteBallPicture()
		{
			if (FifaEnvironment.Year == 14)
			{
				return FifaEnvironment.DeleteFromZdata(BallPictureBigFileName());
			}
			return FifaEnvironment.DeleteFromZdata(BallDdsFileName());
		}

		public static bool SetBallPicture(int ballId, Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			if (FifaEnvironment.Year == 14)
			{
				return FifaEnvironment.SetArtasset(BallPictureTemplateBigFileName(), BallPictureTemplateDdsName(), ballId, bitmap);
			}
			return FifaEnvironment.SetDdsArtasset(BallDdsTemplateFileName(), ballId, bitmap);
		}

		public bool SetBallPicture(Bitmap bitmap)
		{
			return SetBallPicture(base.Id, bitmap);
		}

		public static string BallDdsFileName(int id)
		{
			return "data/ui/imgassets/settingsimg/ball_" + id.ToString() + ".dds";
		}

		public static string BallDdsTemplateFileName()
		{
			return "data/ui/imgassets/settingsimg/ball_#.dds";
		}

		public string BallDdsFileName()
		{
			return BallDdsFileName(base.Id);
		}

		public static string BallModelFileName(int ballId)
		{
			return "data/sceneassets/ball/ball_" + ballId.ToString() + ".rx3";
		}

		public static string RevModTeamBallModelFileName(int teamId)
		{
			return "data/sceneassets/ball/specificball_" + teamId.ToString() + "_0_0.rx3";
		}

		public static string RevModTrophyBallModelFileName(int assetId)
		{
			return "data/sceneassets/ball/specificball_0_" + assetId.ToString() + "_0.rx3";
		}

		public string BallModelFileName()
		{
			return BallModelFileName(base.Id);
		}

		public static string BallModelTemplateFileName()
		{
			return "data/sceneassets/ball/ball_#.rx3";
		}

		public static Rx3File GetBallModel(int ballId)
		{
			string rx3FileName = BallModelFileName(ballId);
			Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float16;
			return FifaEnvironment.GetRx3FromZdata(rx3FileName);
		}

		public static Rx3File GetRevModTrophyBallModel(int assetId)
		{
			return FifaEnvironment.GetRx3FromZdata(RevModTrophyBallModelFileName(assetId), verbose: false);
		}

		public static Rx3File GetRevModTeamBallModel(int teamId)
		{
			return FifaEnvironment.GetRx3FromZdata(RevModTeamBallModelFileName(teamId), verbose: false);
		}

		public Rx3File GetBallModel()
		{
			return GetBallModel(base.Id);
		}

		public static bool SetBallModel(int ballId, string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, BallModelFileName(ballId), delete: false, ECompressionMode.Chunkzip, null);
		}

		public static bool SetRevModTrophyBallModel(int assetId, string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, RevModTrophyBallModelFileName(assetId), delete: false, ECompressionMode.None, null);
		}

		public static bool SetRevModTeamBallModel(int teamId, string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, RevModTeamBallModelFileName(teamId), delete: false, ECompressionMode.None, null);
		}

		public bool SetBallModel(string srcFileName)
		{
			return SetBallModel(base.Id, srcFileName);
		}

		public bool DeleteBallModel()
		{
			return FifaEnvironment.DeleteFromZdata(BallModelFileName());
		}

		public static bool DeleteRevModTrophyBallModel(int assetId)
		{
			return FifaEnvironment.DeleteFromZdata(RevModTrophyBallModelFileName(assetId));
		}

		public static bool DeleteRevModTeamBallModel(int teamId)
		{
			return FifaEnvironment.DeleteFromZdata(RevModTeamBallModelFileName(teamId));
		}

		public bool DeleteBall()
		{
			DeleteBallTextures();
			DeleteBallModel();
			DeleteBallPicture();
			return true;
		}

		public override IdObject Clone(int newId)
		{
			Ball ball = (Ball)base.Clone(newId);
			if (ball != null)
			{
				ball.Name = "Ball n." + newId.ToString();
				FifaEnvironment.CloneIntoZdata(BallTextureFileName(base.Id), BallTextureFileName(newId));
				FifaEnvironment.CloneIntoZdata(BallModelFileName(base.Id), BallModelFileName(newId));
				if (FifaEnvironment.Year == 14)
				{
					FifaEnvironment.CloneIntoZdata(BallPictureBigFileName(base.Id), BallPictureBigFileName(newId));
				}
				else
				{
					FifaEnvironment.CloneIntoZdata(BallDdsFileName(base.Id), BallDdsFileName(newId));
				}
			}
			return ball;
		}
	}
}
