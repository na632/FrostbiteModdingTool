using System;
using System.Collections.Generic;

namespace FrostySdk
{
	public class CatalogInfo
	{
		public Guid Id;

		public string Name;

		public bool AlwaysInstalled;

		public Dictionary<string, bool> SuperBundles = new Dictionary<string, bool>();
	}
}
