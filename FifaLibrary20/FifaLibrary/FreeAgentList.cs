namespace FifaLibrary
{
	public class FreeAgentList : IdArrayList
	{
		public FreeAgentList()
			: base(typeof(FreeAgent))
		{
			Clear();
			Add(new FreeAgent());
		}
	}
}
