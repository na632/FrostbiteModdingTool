using System.IO;

namespace FifaLibrary
{
	public class KitFile : Rx3File
	{
		public override bool Load(BinaryReader r)
		{
			return base.Load(r);
		}

		public override bool Save(BinaryWriter w, bool saveBitmaps, bool saveVertex)
		{
			return base.Save(w, saveBitmaps: true, saveVertex: false);
		}
	}
}
