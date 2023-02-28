namespace Modding.Categories
{
    [ModCategory(ModMainCategory.OtherGraphics)]
    public enum ModOtherGraphicsSubCategory : byte
    {
        Banners = 1,
        Gloves = 2,
        Trophies = 3,
        [System.ComponentModel.Description("Manager Outfits")]
        ManagerOutfits = 4,
        Minifaces = 5,
        Backgrounds = 6,
        Themes = 7,
        Scoreboards = 8,
        Overlays = 9,
        [System.ComponentModel.Description("TV Logos")]
        TVLogos = 10,
        Adboards = 11,
        Balls = 12,
        Turf = 13,
        None = 0,
        Custom = byte.MaxValue
    }
}
