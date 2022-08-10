using System.Collections;

namespace FifaLibrary
{
	public class IdObjectComparer : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			return ((IdObject)x).Id - ((IdObject)y).Id;
		}
	}
}
