using System.Drawing;

namespace FifaLibrary
{
	public class NumberFont : IdObject
	{
		private static int s_MaxColors = 20;

		private int m_Style;

		private int m_Color;

		public NumberFont(int fontId)
			: base(fontId)
		{
			ComputeStyleAndColor(fontId);
		}

		private void ComputeStyleAndColor(int fontId)
		{
			m_Style = fontId / s_MaxColors;
			m_Color = fontId - m_Style * s_MaxColors;
		}

		public override string ToString()
		{
			FifaUtil.PadBlanks(base.Id.ToString(), 3);
			string str = "Font n. " + m_Style + " ";
			switch (m_Color)
			{
			case 0:
				return str + "Transparent";
			case 1:
				return str + "White";
			case 2:
				return str + "Black";
			case 3:
				return str + "Blue";
			case 4:
				return str + "Red";
			case 5:
				return str + "Yellow";
			case 6:
				return str + "Green";
			case 7:
				return str + "Orange";
			case 8:
				return str + "Violet";
			case 9:
				return str + "Brown";
			case 10:
				return str + "Pink";
			case 11:
				return str + "Dark Red";
			case 12:
				return str + "Cyano";
			case 13:
				return str + "Dark Blue";
			case 14:
				return str + "Gray";
			case 15:
				return str + "Pale Green";
			case 16:
				return str + "Dark Gold";
			case 17:
				return str + "Gold";
			case 18:
				return str + "Light Red";
			case 19:
				return str + "Dark Green";
			default:
				return str + m_Color.ToString();
			}
		}

		public static void Clone(int oldStyle, int oldColor, int newStyle, int newColor)
		{
			FifaEnvironment.CloneIntoZdata(NumberFontFileName(oldStyle, oldColor), NumberFontFileName(newStyle, newColor));
		}

		public static string NumberFontFileName(int styleId, int colorId)
		{
			return "data/sceneassets/kitnumbers/kitnumbers_" + styleId.ToString() + "_" + colorId.ToString() + ".rx3";
		}

		public string NumberFontFileName()
		{
			return NumberFontFileName(m_Style, m_Color);
		}

		public static string NumberFontTemplateName()
		{
			return "data/sceneassets/kitnumbers/kitnumbers_#_%.rx3";
		}

		public static Rx3Signatures NumberFontSignature(int id, int colorId)
		{
			string[] array = new string[10];
			for (int i = 0; i < 10; i++)
			{
				array[i] = "numbers_" + id.ToString() + "_" + colorId.ToString() + "_" + i.ToString();
			}
			return new Rx3Signatures(439280, 24, array);
		}

		public static Rx3File GetNumberFontRx3(int style, int color)
		{
			return FifaEnvironment.GetRx3FromZdata(NumberFontFileName(style, color));
		}

		public static Bitmap[] GetNumberFont(int style, int color)
		{
			return FifaEnvironment.GetBmpsFromRx3(NumberFontFileName(style, color));
		}

		public static bool SetNumberFont(int style, int color, Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(ids: new int[2]
			{
				style,
				color
			}, templateRx3Name: NumberFontTemplateName(), bitmaps: bitmaps, compressionMode: ECompressionMode.Chunkzip, signatures: NumberFontSignature(style, color));
		}

		public static bool SetNumberFont(int style, int color, string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, NumberFontFileName(style, color), delete: false, ECompressionMode.Chunkzip);
		}

		public static bool Delete(int style, int color)
		{
			return FifaEnvironment.DeleteFromZdata(NumberFontFileName(style, color));
		}

		public static bool Import(int style, int color, string rx3FileName)
		{
			string archivedName = NumberFontFileName(style, color);
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, archivedName, delete: false, ECompressionMode.Chunkzip, NumberFontSignature(style, color));
		}

		public static bool Export(int style, int color, string exportDir)
		{
			return FifaEnvironment.ExportFileFromZdata(NumberFontFileName(style, color), exportDir);
		}
	}
}
