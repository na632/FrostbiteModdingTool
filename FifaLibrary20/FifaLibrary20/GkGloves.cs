using System.Drawing;

namespace FifaLibrary
{
	public class GkGloves : IdObject
	{
		private static Model3D s_GkGlovesModel;

		public static Model3D GkGlovesModel => s_GkGlovesModel;

		public void Set3DModelTexture(Bitmap bitmaps)
		{
			if (s_GkGlovesModel != null)
			{
				s_GkGlovesModel.TextureBitmap = bitmaps;
			}
		}

		public GkGloves(int gkglovesId)
			: base(gkglovesId)
		{
			if (s_GkGlovesModel == null)
			{
				Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float32;
				Rx3File rx3FromZdata = FifaEnvironment.GetRx3FromZdata(GkGlovesModelFileName(1));
				s_GkGlovesModel = new Model3D(rx3FromZdata.Rx3IndexArrays[0], rx3FromZdata.Rx3VertexArrays[0], null);
			}
		}

		public override string ToString()
		{
			return "GK Gloves n. " + base.Id.ToString();
		}

		public static string GkGlovesTextureFileName(int gkglovesId)
		{
			return "data/sceneassets/gkglove/gkglove_" + gkglovesId.ToString() + "_textures.rx3";
		}

		public static Bitmap[] GetGkGlovesTextures(int gkglovesId)
		{
			return FifaEnvironment.GetBmpsFromRx3(GkGlovesTextureFileName(gkglovesId));
		}

		public static string GkGloveTextureTemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data\\sceneassets\\gkglove\\2014_gkglove_#_textures.rx3";
			}
			return "data\\sceneassets\\gkglove\\gkglove_#_textures.rx3";
		}

		public static bool SetGkGlovesTextures(int gkglovesId, Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(GkGloveTextureTemplateFileName(), gkglovesId, bitmaps, ECompressionMode.Chunkzip);
		}

		public static bool SetGkGlovesTextures(int gkglovesId, string rx3FileName)
		{
			string archivedName = GkGlovesTextureFileName(gkglovesId);
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public static bool ExportGkGlovesTextures(int gkglovesId, string exportDir)
		{
			return FifaEnvironment.ExportFileFromZdata(GkGlovesTextureFileName(gkglovesId), exportDir);
		}

		public static bool DeleteGkGlovesTextures(int gkglovesId)
		{
			return FifaEnvironment.DeleteFromZdata(GkGlovesTextureFileName(gkglovesId));
		}

		public static string GkGlovesModelFileName(int id)
		{
			return "data/sceneassets/gkglove/gkglove_" + id.ToString() + ".rx3";
		}

		public static Rx3File GetGkGlovesModel(int gkglovesId)
		{
			return FifaEnvironment.GetRx3FromZdata(GkGlovesTextureFileName(gkglovesId));
		}

		public override IdObject Clone(int newId)
		{
			GkGloves obj = (GkGloves)base.Clone(newId);
			if (obj != null)
			{
				FifaEnvironment.CloneIntoZdata(GkGlovesTextureFileName(base.Id), GkGlovesTextureFileName(newId));
				FifaEnvironment.CloneIntoZdata(GkGlovesModelFileName(base.Id), GkGlovesModelFileName(newId));
			}
			return obj;
		}
	}
}
