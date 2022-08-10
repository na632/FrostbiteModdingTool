using System.ComponentModel;

namespace Modding.Categories
{
	public enum ModMainCategory : byte
	{
		[ModSubCategory(typeof(ModFacesSubCategory))]
		Faces = 1,
		[ModSubCategory(typeof(ModKitsSubCategory))]
		Kits = 2,
		[ModSubCategory(typeof(ModBootsSubCategory))]
		Boots = 3,
		[ModSubCategory(typeof(ModTattoosSubCategory))]
		Tattoos = 4,
		[ModSubCategory(typeof(ModOtherGraphicsSubCategory))]
		[System.ComponentModel.Description("Other Graphics")]
		OtherGraphics = 5,
		[ModSubCategory(typeof(ModLegacySubCategory))]
		Legacy = 6,
		[ModSubCategory(typeof(ModOtherSubCategory))]
		Other = 7,
		[ModSubCategory(typeof(ModCustomSubCategory))]
		Custom = byte.MaxValue
	}
}
