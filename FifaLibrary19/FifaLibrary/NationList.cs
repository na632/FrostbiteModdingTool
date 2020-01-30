using System;
using System.Collections;

namespace FifaLibrary
{
	public class NationList : IdArrayList
	{
		public NationList()
			: base(typeof(Nation))
		{
		}

		public void LinkCountry(CountryList countryList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Nation)enumerator.Current).LinkCountry(countryList);
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}
	}
}
