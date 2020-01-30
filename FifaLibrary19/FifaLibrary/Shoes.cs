using System.Drawing;

namespace FifaLibrary
{
	public class Shoes : IdObject
	{
		private static Color[] c_ShoesColor = new Color[16]
		{
			Color.FromArgb(255, 20, 20, 20),
			Color.FromArgb(255, 50, 34, 105),
			Color.FromArgb(255, 1, 32, 87),
			Color.FromArgb(255, 8, 77, 158),
			Color.FromArgb(255, 1, 159, 224),
			Color.FromArgb(255, 0, 177, 17),
			Color.FromArgb(255, 120, 200, 25),
			Color.FromArgb(255, 250, 245, 10),
			Color.FromArgb(255, 250, 240, 0),
			Color.FromArgb(255, 236, 86, 1),
			Color.FromArgb(255, 227, 1, 103),
			Color.FromArgb(255, 177, 11, 35),
			Color.FromArgb(255, 112, 37, 42),
			Color.FromArgb(255, 146, 114, 63),
			Color.FromArgb(255, 160, 160, 160),
			Color.FromArgb(255, 235, 235, 235)
		};

		private bool m_HasName;

		private string m_LanguageName;

		private int m_shoecolor1;

		private int m_shoecolor2;

		private int m_shoedesign;

		private int m_powid;

		private bool m_IsAdidas;

		private bool m_IsAvailableinStore;

		private bool m_IsGender;

		private bool m_IsEmbargoed;

		private bool m_IsLegacy;

		private bool m_IsLicensed;

		private bool m_IsLocked;

		public static Color[] ShoesColorPalette => c_ShoesColor;

		public bool HasName
		{
			get
			{
				return m_HasName;
			}
			set
			{
				m_HasName = value;
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
				FifaEnvironment.Language.SetShoesName(base.Id, m_LanguageName);
				m_HasName = true;
			}
		}

		public int shoecolor1
		{
			get
			{
				return m_shoecolor1;
			}
			set
			{
				m_shoecolor1 = value;
			}
		}

		public int shoecolor2
		{
			get
			{
				return m_shoecolor2;
			}
			set
			{
				m_shoecolor2 = value;
			}
		}

		public int shoedesign
		{
			get
			{
				return m_shoedesign;
			}
			set
			{
				m_shoedesign = value;
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

		public bool IsAdidas
		{
			get
			{
				return m_IsAdidas;
			}
			set
			{
				m_IsAdidas = value;
			}
		}

		public bool IsAvailableinStore
		{
			get
			{
				return m_IsAvailableinStore;
			}
			set
			{
				m_IsAvailableinStore = value;
			}
		}

		public bool IsGender
		{
			get
			{
				return m_IsGender;
			}
			set
			{
				m_IsGender = value;
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

		public bool IsLegacy
		{
			get
			{
				return m_IsLegacy;
			}
			set
			{
				m_IsLegacy = value;
			}
		}

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

		public bool IsLocked
		{
			get
			{
				return m_IsLocked;
			}
			set
			{
				m_IsLocked = value;
			}
		}

		public static Color GetGenericColor(int colorId)
		{
			if (colorId >= 0 && colorId <= 15)
			{
				return c_ShoesColor[colorId];
			}
			return Color.FromArgb(0, 0, 0, 0);
		}

		public Shoes(Record r)
			: base(r.IntField[FI.playerboots_shoetype])
		{
			m_LanguageName = FifaEnvironment.Language.GetShoesName(base.Id);
			m_HasName = (m_LanguageName != null);
			if (!m_HasName)
			{
				string str = FifaUtil.PadBlanks(base.Id.ToString(), 3);
				m_LanguageName = "Shoes n. " + str;
			}
			m_shoecolor1 = r.GetAndCheckIntField(FI.playerboots_shoecolor1);
			m_shoecolor2 = r.GetAndCheckIntField(FI.playerboots_shoecolor2);
			m_shoedesign = r.GetAndCheckIntField(FI.playerboots_shoedesign);
			m_IsAdidas = ((r.IntField[FI.playerboots_isadidas] > 0) ? true : false);
			m_IsAvailableinStore = ((r.IntField[FI.playerboots_isavailableinstore] > 0) ? true : false);
			m_IsEmbargoed = ((r.IntField[FI.playerboots_isembargoed] > 0) ? true : false);
			if (FI.playerboots_gender >= 0)
			{
				m_IsGender = ((r.IntField[FI.playerboots_gender] != 0) ? true : false);
			}
			m_IsLegacy = ((r.IntField[FI.playerboots_islegacy] > 0) ? true : false);
			m_IsLicensed = ((r.IntField[FI.playerboots_islicensed] > 0) ? true : false);
			m_IsLocked = ((r.IntField[FI.playerboots_islocked] > 0) ? true : false);
			m_powid = r.GetAndCheckIntField(FI.playerboots_powid);
		}

		public Shoes(int shoesId)
			: base(shoesId)
		{
			m_LanguageName = FifaEnvironment.Language.GetShoesName(base.Id);
			m_HasName = (m_LanguageName != null);
			if (!m_HasName)
			{
				string str = FifaUtil.PadBlanks(base.Id.ToString(), 3);
				m_LanguageName = "Shoes n. " + str;
			}
			m_shoecolor1 = 30;
			m_shoecolor2 = 31;
			m_shoedesign = 0;
			m_IsAdidas = false;
			m_IsAvailableinStore = false;
			m_IsGender = false;
			m_IsEmbargoed = false;
			m_IsLegacy = false;
			m_IsLicensed = false;
			m_IsLocked = false;
			m_powid = -1;
		}

		public void SaveShoes(Record r)
		{
			r.IntField[FI.playerboots_shoetype] = base.Id;
			r.IntField[FI.playerboots_shoecolor1] = m_shoecolor1;
			r.IntField[FI.playerboots_shoecolor2] = m_shoecolor2;
			r.IntField[FI.playerboots_shoedesign] = m_shoedesign;
			r.IntField[FI.playerboots_isadidas] = (m_IsAdidas ? 1 : 0);
			r.IntField[FI.playerboots_isavailableinstore] = (m_IsAvailableinStore ? 1 : 0);
			r.IntField[FI.playerboots_gender] = (m_IsGender ? 2 : 0);
			r.IntField[FI.playerboots_isembargoed] = 0;
			r.IntField[FI.playerboots_islegacy] = 0;
			r.IntField[FI.playerboots_islicensed] = ((base.Id != 0) ? 1 : 0);
			r.IntField[FI.playerboots_islocked] = 0;
			r.IntField[FI.playerboots_powid] = m_powid;
		}

		public override IdObject Clone(int newId)
		{
			Shoes shoes = (Shoes)base.Clone(newId);
			if (shoes != null)
			{
				string str = FifaUtil.PadBlanks(base.Id.ToString(), 3);
				shoes.Name = "Shoes n. " + str;
				FifaEnvironment.CloneIntoZdata(ShoesTexturesFileName(base.Id, 0), ShoesTexturesFileName(newId, 0));
				FifaEnvironment.CloneIntoZdata(ShoesModelFileName(base.Id), ShoesModelFileName(newId));
			}
			return shoes;
		}

		public override string ToString()
		{
			return m_LanguageName;
		}

		public static string ShoesTexturesFileName(int shoesBrand, int shoesDesign)
		{
			return "data/sceneassets/shoe/shoe_" + shoesBrand.ToString() + "_" + shoesDesign.ToString() + "_textures.rx3";
		}

		public static string ShoesModelFileName(int shoesBrand)
		{
			return "data/sceneassets/shoe/shoe_" + shoesBrand.ToString() + ".rx3";
		}

		public static Bitmap GetShoesColorTexture(int shoesBrand, int shoesDesign)
		{
			return FifaEnvironment.GetBmpFromRx3(ShoesTexturesFileName(shoesBrand, shoesDesign));
		}

		public static Bitmap[] GetShoesTextures(int shoesBrand, int shoesDesign)
		{
			return FifaEnvironment.GetBmpsFromRx3(ShoesTexturesFileName(shoesBrand, shoesDesign));
		}

		public static bool ImportShoesTextures(int shoesBrand, int shoesColor, string rx3FileName)
		{
			return SetShoesTextures(shoesBrand, shoesColor, rx3FileName);
		}

		public static bool ExportShoesTextures(int shoesBrand, int shoesColor, string exportDir)
		{
			return FifaEnvironment.ExportFileFromZdata(ShoesTexturesFileName(shoesBrand, shoesColor), exportDir);
		}

		public static Rx3File GetShoesModel(int shoesBrand)
		{
			return FifaEnvironment.GetRx3FromZdata(ShoesModelFileName(shoesBrand));
		}

		public static string ShoeTemplateFilename()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/sceneassets/shoe/2014_shoe_#_%_textures.rx3";
			}
			return "data/sceneassets/shoe/shoe_#_%_textures.rx3";
		}

		public static bool SetShoesTextures(int shoesBrand, int shoesDesign, Bitmap[] bitmaps)
		{
			ShoesTexturesFileName(shoesBrand, shoesDesign);
			return FifaEnvironment.ImportBmpsIntoZdata(ids: new int[2]
			{
				shoesBrand,
				shoesDesign
			}, templateRx3Name: ShoeTemplateFilename(), bitmaps: bitmaps, compressionMode: ECompressionMode.Chunkzip);
		}

		public static bool SetShoesTextures(int shoesBrand, int shoesDesign, string rx3FileName)
		{
			string archivedName = ShoesTexturesFileName(shoesBrand, shoesDesign);
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public static bool SetShoesModel(int shoesBrand, string rx3FileName)
		{
			string archivedName = ShoesModelFileName(shoesBrand);
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public static bool ExportShoesModel(int shoesBrand, string exportDir)
		{
			return FifaEnvironment.ExportFileFromZdata(ShoesModelFileName(shoesBrand), exportDir);
		}

		public static bool DeleteShoes(int shoesBrand, int shoesColor)
		{
			FifaEnvironment.DeleteFromZdata(ShoesTexturesFileName(shoesBrand, shoesColor));
			FifaEnvironment.DeleteFromZdata(ShoesModelFileName(shoesBrand));
			return true;
		}

		public static bool DeleteShoesTextures(int shoesBrand, int shoesColor)
		{
			FifaEnvironment.DeleteFromZdata(ShoesTexturesFileName(shoesBrand, shoesColor));
			return true;
		}

		public static bool DeleteShoesModel(int shoesBrand)
		{
			FifaEnvironment.DeleteFromZdata(ShoesModelFileName(shoesBrand));
			return true;
		}
	}
}
