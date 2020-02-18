using System;

namespace Frosty.ModSupport
{
	public class FrostyModLoadException : Exception
	{
		public FrostyModLoadException(string message)
			: base(message)
		{
		}
	}
}
