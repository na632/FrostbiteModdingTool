using System;

namespace Modding.Categories
{
	public class ModCategoryAttribute : Attribute
	{
		public ModMainCategory MainCategory
		{
			get;
			set;
		}

		public ModCategoryAttribute(ModMainCategory mainCategory)
		{
			MainCategory = mainCategory;
		}
	}
}
