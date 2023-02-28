namespace Modding.Categories
{
    [ModCategory(ModMainCategory.Tattoos)]
    public enum ModTattoosSubCategory : byte
    {
        Tattoo = 1,
        [System.ComponentModel.Description("Tattoo Pack")]
        TattooPack = 2,
        None = 0,
        Custom = byte.MaxValue
    }
}
