using FMT.FileTools.Modding;
using FrostySdk.Frosty.FET;
using Modding.Categories;
using System;

public class FIFAModDetails : FrostbiteModDetails
{
    private readonly string category;

    //private ImageSource icon;

    //public string Title
    //{
    //	get;
    //}

    //public string Author
    //{
    //	get;
    //}

    //public string Version
    //{
    //	get;
    //}

    //public string Description
    //{
    //	get;
    //}

    public string MainCategoryString
    {
        get
        {
            //if (MainCategory != ModMainCategory.Custom)
            //{
            //	return EnumHelper.EnumToString(MainCategory);
            //}
            return Category;
        }
    }

    public string SubCategoryString
    {
        get
        {
            if (Convert.ToByte(SubCategory) == 0)
            {
                return null;
            }
            if (Convert.ToByte(SubCategory) == byte.MaxValue)
            {
                if (MainCategory != ModMainCategory.Custom)
                {
                    if (!string.IsNullOrEmpty(Category))
                    {
                        return Category;
                    }
                    return null;
                }
                if (!string.IsNullOrEmpty(SecondCustomCategory))
                {
                    return SecondCustomCategory;
                }
                return null;
            }
            return EnumHelper.EnumToString(SubCategory);
        }
    }

    public ModMainCategory MainCategory
    {
        get;
        set;
    }

    public Enum SubCategory
    {
        get;
        set;
    }

    //public string Category
    //{
    //	get
    //	{
    //		if (!(category != string.Empty))
    //		{
    //			return "Misc";
    //		}
    //		return category;
    //	}
    //}

    public string SecondCustomCategory
    {
        get;
        set;
    }

    public string OutOfDateModWebsiteLink
    {
        get;
        set;

    } = string.Empty;


    public string DiscordServerLink
    {
        get;
        set;

    } = string.Empty;


    public string PatreonLink
    {
        get;
        set;

    } = string.Empty;


    public string TwitterLink
    {
        get;
        set;

    } = string.Empty;


    public string YouTubeLink
    {
        get;
        set;

    } = string.Empty;


    public string InstagramLink
    {
        get;
        set;

    } = string.Empty;


    public string FacebookLink
    {
        get;
        set;

    } = string.Empty;


    public string CustomLink
    {
        get;
        set;

    } = string.Empty;


    //public ImageSource Icon => icon;

    //public List<ImageSource> Screenshots
    //{
    //	get;
    //} = new List<ImageSource>();


  

    public FIFAModDetails(string inTitle, string inAuthor, ModMainCategory mainCategory, Enum subCategory, string customCategory, string secondCustomCategory, string inVersion, string inDescription)
        : base(inTitle, inAuthor, mainCategory.ToString(), inVersion, inDescription)
    {
        Title = inTitle;
        Author = inAuthor;
        Version = inVersion;
        Description = inDescription;
        MainCategory = mainCategory;
        SubCategory = subCategory;
        category = customCategory;
        SecondCustomCategory = secondCustomCategory;
    }

    //public void SetIcon(byte[] buffer)
    //{
    //	//icon = LoadImage(buffer);
    //}

    //public void AddScreenshot(byte[] buffer)
    //{
    //	//ImageSource item = LoadImage(buffer);
    //	//Screenshots.Add(item);
    //}

    public override int GetHashCode()
    {
        return HashCode.Combine(Title, Author, Version);
    }

    //private static BitmapImage LoadImage(byte[] buffer)
    //{
    //	if (buffer == null || buffer.Length == 0)
    //	{
    //		return null;
    //	}
    //	BitmapImage bitmapImage = new BitmapImage();
    //	MemoryStream streamSource = new MemoryStream(buffer);
    //	bitmapImage.BeginInit();
    //	bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
    //	bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
    //	bitmapImage.UriSource = null;
    //	bitmapImage.StreamSource = streamSource;
    //	bitmapImage.EndInit();
    //	bitmapImage.Freeze();
    //	return bitmapImage;
    //}
}
