using System.Drawing;

namespace FifaLibrary
{
	public class Adboard : IdObject
	{
		public Adboard(int adboardId)
			: base(adboardId)
		{
		}

		public override IdObject Clone(int newId)
		{
			Adboard obj = (Adboard)base.Clone(newId);
			if (obj != null)
			{
				FifaEnvironment.CloneIntoZdata(AdboardFileName(base.Id), AdboardFileName(newId));
			}
			return obj;
		}

		public override string ToString()
		{
			string str = FifaUtil.PadBlanks(base.Id.ToString(), 3);
			return "Adboard n. " + str;
		}

		public static string AdboardFileName(int adboardid)
		{
			return "data/sceneassets/adboard/adboard_" + adboardid.ToString() + "_0.rx3";
		}

		public static string RevModTeamAdboardFileName(int teamId)
		{
			return "data/sceneassets/adboard/specificadboard_" + teamId.ToString() + "_0_0_0.rx3";
		}

		public static string RevModTornamentAdboardFileName(int tournamentAssetId)
		{
			return "data/sceneassets/adboard/specificadboard_0_" + tournamentAssetId.ToString() + "_0_0.rx3";
		}

		public static Bitmap GetAdboard(int adboardId)
		{
			string rx3FileName = AdboardFileName(adboardId);
			bool verbose = adboardId <= 1000000;
			return FifaEnvironment.GetBmpFromRx3(rx3FileName, verbose);
		}

		public static Bitmap GetRevModTeamAdboard(int teamId)
		{
			return FifaEnvironment.GetBmpFromRx3(RevModTeamAdboardFileName(teamId), verbose: false);
		}

		public static bool SetRevModTeamAdboard(int teamId, Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata("data/sceneassets/adboard/specificadboard_#_0_0_0.rx3", teamId, bitmap, ECompressionMode.None);
		}

		public static bool SetRevModTeamAdboard(int adboardId, string srcFileName)
		{
			string archivedName = RevModTeamAdboardFileName(adboardId);
			return FifaEnvironment.ImportFileIntoZdataAs(srcFileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public static bool SetRevModTournamentAdboard(int tournamentId, Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata("data/sceneassets/adboard/specificadboard_0_#_0_0.rx3", tournamentId, bitmap, ECompressionMode.None);
		}

		public static bool SetRevModTournamentAdboard(int adboardId, string srcFileName)
		{
			string archivedName = RevModTornamentAdboardFileName(adboardId);
			return FifaEnvironment.ImportFileIntoZdataAs(srcFileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public static Bitmap GetRevModTournamentAdboard(int tournamentAssetId)
		{
			return FifaEnvironment.GetBmpFromRx3(RevModTornamentAdboardFileName(tournamentAssetId), verbose: false);
		}

		public Bitmap GetAdboard()
		{
			return GetAdboard(base.Id);
		}

		public static bool SetAdboard(int adboardId, Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata("data/sceneassets/adboard/adboard_#_0.rx3", adboardId, bitmap, ECompressionMode.Chunkref);
		}

		public bool SetAdboard(Bitmap bitmap)
		{
			return SetAdboard(base.Id, bitmap);
		}

		public static bool SetAdboard(int adboardId, string srcFileName)
		{
			string archivedName = AdboardFileName(adboardId);
			return FifaEnvironment.ImportFileIntoZdataAs(srcFileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public bool SetAdboard(string srcFileName)
		{
			return SetAdboard(base.Id, srcFileName);
		}

		public static bool DeleteAdboard(int adboardId)
		{
			return FifaEnvironment.DeleteFromZdata(AdboardFileName(adboardId));
		}

		public static bool DeleteRevModTeamAdboard(int teamId)
		{
			return FifaEnvironment.DeleteFromZdata(RevModTeamAdboardFileName(teamId));
		}

		public static bool DeleteRevModTournamentAdboard(int tournamentId)
		{
			return FifaEnvironment.DeleteFromZdata(RevModTornamentAdboardFileName(tournamentId));
		}
	}
}
