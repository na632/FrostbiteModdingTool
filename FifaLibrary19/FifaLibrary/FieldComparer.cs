using System.Collections;

namespace FifaLibrary
{
	public class FieldComparer : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			FieldDescriptor fieldDescriptor = (FieldDescriptor)x;
			FieldDescriptor fieldDescriptor2 = (FieldDescriptor)y;
			if (fieldDescriptor.BitOffset != fieldDescriptor2.BitOffset)
			{
				if (fieldDescriptor.BitOffset <= fieldDescriptor2.BitOffset)
				{
					return -1;
				}
				return 1;
			}
			return 0;
		}
	}
}
