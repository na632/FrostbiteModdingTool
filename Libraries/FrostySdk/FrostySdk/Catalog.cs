using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostySdk
{
	//public class CatalogInfo
	public class Catalog
    {
		public int? Index { get; set; }

		public Guid Id;

		public string Name;

		public bool AlwaysInstalled;

		public Dictionary<string, bool> superBundles = new Dictionary<string, bool>();

		public int? PersistentIndex { get { return Index; } set { Index = value; } }

        //public string CasDataFile
        //      {
        //          get
        //          {
        //		if(!string.IsNullOrEmpty(Name))
        //              {
        //			return CasDataFile;
        //              }
        //		return null;
        //          }
        //      }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }
            return base.ToString();
        }

		public Dictionary<string, bool> SuperBundles => superBundles;
		public List<string> SuperBundleKeys => superBundles.Keys.ToList();

		public Catalog()
        {

        }

		public Catalog(int index, Guid id, string name, bool alwaysInstalled)
		{
			Index = index;
			Id = id;
			Name = name ?? throw new ArgumentNullException("name");
			AlwaysInstalled = alwaysInstalled;
		}

		public bool HasSuperBundle(string name)
		{
			return superBundles.ContainsKey(name);
		}

		public void AddSuperBundle(string name, bool isSplit)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			superBundles.Add(name, isSplit);
		}

		public bool IsSplitSuperBundle(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return superBundles[name];
		}

	}
}
