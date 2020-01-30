using System.Drawing;

namespace FifaLibrary
{
	public class Net : IdObject
	{
		public Net(int netId)
			: base(netId)
		{
		}

		public override IdObject Clone(int newId)
		{
			Net obj = (Net)base.Clone(newId);
			if (obj != null)
			{
				FifaEnvironment.CloneIntoZdata(NetFileName(base.Id), NetFileName(newId));
			}
			return obj;
		}

		public override string ToString()
		{
			string str = FifaUtil.PadBlanks(base.Id.ToString(), 3);
			return "Net n. " + str;
		}

		public static string NetFileName(int netid)
		{
			return "data/sceneassets/goalnet/netcolor_" + netid.ToString() + "_textures.rx3";
		}

		public static string RevModNetFileName(int teamId)
		{
			return "data/sceneassets/goalnet/specificnetcolor_" + teamId.ToString() + "_0_textures.rx3";
		}

		public static Bitmap GetNet(int netId)
		{
			return FifaEnvironment.GetBmpFromRx3(NetFileName(netId), 0);
		}

		public static Bitmap GetRevModNet(int teamId)
		{
			return FifaEnvironment.GetBmpFromRx3(RevModNetFileName(teamId), 0);
		}

		public static bool SetNet(int netId, Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata("data/sceneassets/goalnet/netcolor_#_textures.rx3", netId, bitmap, ECompressionMode.Chunkzip);
		}

		public static bool SetRevModNet(int teamId, Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata("data/sceneassets/goalnet/specificnetcolor_#_0_textures.rx3", teamId, bitmap, ECompressionMode.Chunkzip);
		}

		public static bool SetRevModNet(int netId, string srcFileName)
		{
			string archivedName = RevModNetFileName(netId);
			return FifaEnvironment.ImportFileIntoZdataAs(srcFileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public static bool SetNet(int netId, string srcFileName)
		{
			string archivedName = NetFileName(netId);
			return FifaEnvironment.ImportFileIntoZdataAs(srcFileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public static bool DeleteNet(int netId)
		{
			return FifaEnvironment.DeleteFromZdata(NetFileName(netId));
		}

		public static bool DeleteRevModNet(int teamId)
		{
			return FifaEnvironment.DeleteFromZdata(RevModNetFileName(teamId));
		}
	}
}
