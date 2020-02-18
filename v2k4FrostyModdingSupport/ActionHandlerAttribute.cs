using System;

namespace Frosty.ModSupport.Handlers
{
	public class ActionHandlerAttribute : Attribute
	{
		public uint Hash;

		public ActionHandlerAttribute(uint inHash)
		{
			Hash = inHash;
		}
	}
}
