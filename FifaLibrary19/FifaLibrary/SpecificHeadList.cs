namespace FifaLibrary
{
	public class SpecificHeadList : IdArrayList
	{
		public SpecificHeadList()
			: base(typeof(SpecificHead))
		{
			Clear();
			Add(new SpecificHead());
		}
	}
}
