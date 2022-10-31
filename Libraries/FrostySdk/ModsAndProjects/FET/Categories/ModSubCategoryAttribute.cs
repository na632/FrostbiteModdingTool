using System;

namespace Modding.Categories
{
	public class ModSubCategoryAttribute : Attribute
	{
		public Type SubCategory
		{
			get;
			set;
		}

		public ModSubCategoryAttribute(Type subCategory)
		{
			SubCategory = subCategory;
		}
	}
}
