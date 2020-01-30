using System.Drawing;

namespace FifaLibrary
{
	public class MowingPattern : IdObject
	{
		public MowingPattern(int mowingPatternId)
			: base(mowingPatternId)
		{
		}

		public override IdObject Clone(int newMowingPatternId)
		{
			MowingPattern obj = (MowingPattern)base.Clone(newMowingPatternId);
			if (obj != null)
			{
				FifaEnvironment.CloneIntoZdata(MowingPatternFileName(base.Id), MowingPatternFileName(newMowingPatternId));
			}
			return obj;
		}

		public override string ToString()
		{
			string str = base.Id.ToString("000");
			return "Mowing Pattern n. " + str;
		}

		public static string MowingPatternFileName(int mowingPatternId)
		{
			return "data/sceneassets/pitch/pitchmowpattern_" + mowingPatternId.ToString() + "_textures.rx3";
		}

		public static Bitmap GetMowingPattern(int mowingPatternId)
		{
			return FifaEnvironment.GetBmpFromRx3(MowingPatternFileName(mowingPatternId));
		}

		public static bool SetMowingPattern(int mowingPatternId, string srcFileName)
		{
			string archivedName = MowingPatternFileName(mowingPatternId);
			return FifaEnvironment.ImportFileIntoZdataAs(srcFileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public static bool DeleteMowingPattern(int mowingPatternId)
		{
			return FifaEnvironment.DeleteFromZdata(MowingPatternFileName(mowingPatternId));
		}

		public static string MowingPatternTemplate()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/sceneassets/pitch/2014_pitchmowpattern_#_textures.rx3";
			}
			return "data/sceneassets/pitch/pitchmowpattern_#_textures.rx3";
		}

		public static bool SetMowingPattern(int mowingPatternId, Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(MowingPatternTemplate(), mowingPatternId, bitmap, ECompressionMode.Chunkzip);
		}
	}
}
