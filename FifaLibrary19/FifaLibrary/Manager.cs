using System.Drawing;

namespace FifaLibrary
{
	public class Manager : IdObject
	{
		public static string RevModManagerTextureFileName(int teamId)
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/sceneassets/manager/specificmanager_" + teamId.ToString() + "_0_0_textures.rx3";
			}
			return "data/sceneassets/slc/specificmanager_" + teamId.ToString() + "_0_0_textures.rx3";
		}

		public static string ManagerTextureFileName(int dress, int skin, int color, int coat)
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/sceneassets/manager/manager_" + dress.ToString() + "_" + skin.ToString() + "_" + color.ToString() + "_" + coat.ToString() + "_textures.rx3";
			}
			return "data/sceneassets/slc/manager_" + dress.ToString() + "_0_" + skin.ToString() + "_" + color.ToString() + "_" + coat.ToString() + "_textures.rx3";
		}

		public static string ManagerModelFileName(int dress, int body, int coat)
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/sceneassets/manager/manager_" + dress.ToString() + "_" + body.ToString() + "_" + coat.ToString() + ".rx3";
			}
			return "data/sceneassets/slc/manager_" + dress.ToString() + "_" + body.ToString() + "_1_" + coat.ToString() + ".rx3";
		}

		public static string RevModManagerModelFileName(int teamId)
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/sceneassets/manager/specificmanager_" + teamId.ToString() + "_0_0.rx3";
			}
			return "data/sceneassets/slc/specificmanager_" + teamId.ToString() + "_0_0.rx3";
		}

		public static Bitmap GetManagerTextures(int dress, int skin, int color, int coat)
		{
			return FifaEnvironment.GetBmpFromRx3(ManagerTextureFileName(dress, skin, color, coat), 0);
		}

		public static Bitmap GetRevModManagerTextures(int teamId)
		{
			return FifaEnvironment.GetBmpFromRx3(RevModManagerTextureFileName(teamId), 0);
		}

		public static bool SetRevModManagerTexture(int teamId, Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(bitmaps: new Bitmap[1]
			{
				bitmap
			}, templateRx3Name: "data/sceneassets/manager/specificmanager_#_0_0_textures.rx3", rx3FileName: RevModManagerTextureFileName(teamId), compressionMode: ECompressionMode.None, signatures: null);
		}

		public static bool SetRevModManagerModel(int teamId, string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, RevModManagerModelFileName(teamId), delete: false, ECompressionMode.None, null);
		}

		public static bool SetRevModManagerTexture(int teamId, string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, RevModManagerTextureFileName(teamId), delete: false, ECompressionMode.None, null);
		}

		public static Rx3File GetManagerModel(int dress, int body, int coat)
		{
			return FifaEnvironment.GetRx3FromZdata(ManagerModelFileName(dress, body, coat));
		}

		public static Rx3File GetRevModManagerModel(int teamId)
		{
			return FifaEnvironment.GetRx3FromZdata(RevModManagerModelFileName(teamId));
		}

		public static bool SetManager(int dress, int skin, int color, int coat, Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(bitmaps: new Bitmap[1]
			{
				bitmap
			}, templateRx3Name: ManagerTextureFileName(0, 0, 0, 0), rx3FileName: ManagerTextureFileName(dress, skin, color, coat), compressionMode: ECompressionMode.Chunkzip, signatures: ManagerSignature(dress, skin, color, coat));
		}

		public static bool DeleteManagerTexture(int dress, int skin, int color, int coat)
		{
			return FifaEnvironment.DeleteFromZdata(ManagerTextureFileName(dress, skin, color, coat));
		}

		public static bool DeleteRevModManagerTexture(int teamId)
		{
			return FifaEnvironment.DeleteFromZdata(RevModManagerTextureFileName(teamId));
		}

		public static bool DeleteRevModManagerModel(int teamId)
		{
			return FifaEnvironment.DeleteFromZdata(RevModManagerModelFileName(teamId));
		}

		private static Rx3Signatures ManagerSignature(int dress, int skin, int color, int coat)
		{
			if (FifaEnvironment.Year == 14)
			{
				return new Rx3Signatures(1398432, 36, new string[1]
				{
					"manager_" + dress.ToString() + "_" + skin.ToString() + "_" + color.ToString() + "_" + coat.ToString() + "_cm.Raster"
				});
			}
			return new Rx3Signatures(2097968, 27, new string[1]
			{
				"manager_" + dress.ToString() + "_" + skin.ToString() + "_" + color.ToString() + "_" + coat.ToString() + "_cm"
			});
		}
	}
}
