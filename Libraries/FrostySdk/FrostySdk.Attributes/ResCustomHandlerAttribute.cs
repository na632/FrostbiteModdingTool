using FrostySdk.Managers;
using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class ResCustomHandlerAttribute : Attribute
	{
		public ResourceType ResType
		{
			get;
			set;
		}

		public Type CustomHandler
		{
			get;
			set;
		}

		public ResCustomHandlerAttribute(ResourceType resType, Type customHandler)
		{
			ResType = resType;
			CustomHandler = customHandler;
		}
	}
}
