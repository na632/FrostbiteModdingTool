namespace Modding.Categories
{
	[ModCategory(ModMainCategory.Legacy)]
	public enum ModLegacySubCategory : byte
	{
		Tournament = 1,
		Algorithm = 2,
		Database = 3,
		None = 0,
		Custom = byte.MaxValue
	}
}
