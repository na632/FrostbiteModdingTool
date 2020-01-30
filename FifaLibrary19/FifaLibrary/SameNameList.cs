namespace FifaLibrary
{
	public class SameNameList : IdArrayList
	{
		public SameNameList()
			: base(typeof(SameName))
		{
			Clear();
			Add(new SameName());
		}
	}
}
