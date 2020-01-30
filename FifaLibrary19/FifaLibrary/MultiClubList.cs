namespace FifaLibrary
{
	public class MultiClubList : IdArrayList
	{
		public MultiClubList()
			: base(typeof(MultiClub))
		{
			Clear();
			Add(new MultiClub());
		}
	}
}
