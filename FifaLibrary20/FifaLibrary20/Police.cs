using System.Drawing;

namespace FifaLibrary
{
	public class Police : IdObject
	{
		public Police(int policeId)
			: base(policeId)
		{
		}

		public override IdObject Clone(int newId)
		{
			Police obj = (Police)base.Clone(newId);
			if (obj != null)
			{
				FifaEnvironment.CloneIntoZdata(PoliceFileName(base.Id, 0), PoliceFileName(newId, 0));
				FifaEnvironment.CloneIntoZdata(PoliceFileName(base.Id, 1), PoliceFileName(newId, 1));
			}
			return obj;
		}

		public override string ToString()
		{
			string str = FifaUtil.PadBlanks(base.Id.ToString(), 3);
			return "Police n. " + str;
		}

		public static string PoliceFileName(int policeid, int type)
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/sceneassets/policeofficer/policeofficer_" + policeid.ToString() + "_0_0_" + type.ToString() + "_textures.rx3";
			}
			return "data/sceneassets/slc/policeofficer_" + policeid.ToString() + "_0_0_" + type.ToString() + "_textures.rx3";
		}

		public static string PoliceTemplateName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/sceneassets/policeofficer/policeofficer_#_0_0_%_textures.rx3";
			}
			return "data/sceneassets/slc/policeofficer_#_0_0_%_textures.rx3";
		}

		public static Bitmap GetPolice(int policeid, int type)
		{
			return FifaEnvironment.GetBmpFromRx3(PoliceFileName(policeid, type), 0);
		}

		public static bool SetPolice(int policeid, int type, Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(ids: new int[2]
			{
				policeid,
				type
			}, templateRx3Name: PoliceTemplateName(), bitmap: bitmap, compressionMode: ECompressionMode.Chunkzip, signatures: PoliceSignature(policeid, type));
		}

		public static bool DeletePolice(int policeid, int type)
		{
			return FifaEnvironment.DeleteFromZdata(PoliceFileName(policeid, type));
		}

		private static Rx3Signatures PoliceSignature(int policeid, int type)
		{
			return new Rx3Signatures(43984, 48, new string[1]
			{
				"policeofficer_" + policeid.ToString() + "_0_0_" + type.ToString() + "_cm.Raster"
			});
		}
	}
}
