using System.Drawing;

namespace FifaLibrary
{
	public class Gloves : IdObject
	{
		public Gloves(int ballId)
			: base(ballId)
		{
		}

		public static string GlovesFileName(int id)
		{
			return "data/sceneassets/gkgloves/gkgloves_0_" + id.ToString() + "_textures.rx3";
		}

		public static Rx3File GetGloves(int id)
		{
			return FifaEnvironment.GetRx3FromZdata(GlovesFileName(id));
		}

		public static bool SetGloves(int id, Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoZdata("data/sceneassets/gkgloves/gkgloves_0_#_textures.rx3", id, bitmaps, ECompressionMode.Chunkzip);
		}

		public static bool SetGloves(int id, string rx3FileName)
		{
			string archivedName = GlovesFileName(id);
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public static bool DeleteGloves(int id)
		{
			return FifaEnvironment.DeleteFromZdata(GlovesFileName(id));
		}
	}
}
