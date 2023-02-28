namespace FMT.FileTools.Modding
{
    public class FrostbiteModDetails
    {
        //private string title;

        //private string author;

        //private string version;

        //private string description;

        private string category;

        public string Title { get; set; }

        public string Author { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public int ScreenshotsCount { get; set; }

        public string Category
        {
            get
            {
                if (!(category == ""))
                {
                    return category;
                }
                return "Misc";
            }
        }

        public int EmbeddedFileCount { get; set; }

        public FrostbiteModDetails(string inTitle, string inAuthor, string inCategory, string inVersion, string inDescription)
        {
            Title = inTitle;
            Author = inAuthor;
            Version = inVersion;
            Description = inDescription;
            category = inCategory;
        }

        public FrostbiteModDetails(string inTitle, string inAuthor, string inCategory, string inVersion, string inDescription, int embeddedFileCount)
        {
            Title = inTitle;
            Author = inAuthor;
            Version = inVersion;
            Description = inDescription;
            category = inCategory;
            EmbeddedFileCount = embeddedFileCount;
        }

        public void SetIcon(byte[] buffer)
        {
        }

        public void AddScreenshot(byte[] buffer)
        {
        }


    }
}
