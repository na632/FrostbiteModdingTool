namespace Modding.Categories
{
    [ModCategory(ModMainCategory.Kits)]
    public enum ModKitsSubCategory : byte
    {
        Kit = 1,
        [System.ComponentModel.Description("Fantasy Kit")]
        FantasyKit = 2,
        [System.ComponentModel.Description("Retro Kit")]
        RetroKit = 3,
        None = 0,
        Custom = byte.MaxValue
    }
}
