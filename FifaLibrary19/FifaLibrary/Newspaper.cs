using System.Drawing;

namespace FifaLibrary
{
	public class Newspaper
	{
		public enum ENewspaperCountry
		{
			Equipe,
			Kicker,
			SkySport,
			CorriereDelloSport,
			Carrusel,
			LasNoticias,
			FutbolMundial,
			EAInsider,
			RMC,
			Deportes,
			Marca,
			LaStampa,
			talkSport,
			Carrusel2,
			Soccer
		}

		public static string NewspaperTemplateBigFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/artassets/newspapers/2014_np_#.big";
			}
			return "data/ui/artassets/newspapers/np_#.big";
		}

		public static string NewspaperTemplateDdsName()
		{
			return "4";
		}

		public static string NewspaperBigFileName(int id)
		{
			return "data/ui/artassets/newspapers/np_" + id.ToString() + ".big";
		}

		public static Bitmap GetNewspaper(int id)
		{
			return FifaEnvironment.GetArtasset(NewspaperBigFileName(id));
		}

		public static bool SetNewspaper(int id, Bitmap bitmap)
		{
			return FifaEnvironment.SetArtasset(NewspaperTemplateBigFileName(), NewspaperTemplateDdsName(), id, bitmap);
		}
	}
}
