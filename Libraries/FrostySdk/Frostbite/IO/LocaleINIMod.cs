namespace FrostySdk.Frostbite.IO
{
    public class LocaleINIMod
    {
        public byte[] OriginalData { get; set; }

        public bool OriginalDataWasEncrypted { get; set; } = FileSystem.Instance.LocaleIsEncrypted;

        public byte[] UserData { get; set; }

        public bool HasUserData { get { return UserData != null && UserData.Length > 0; } }

        public byte[] UserDataEncrypted
        {
            get
            {
                return FileSystem.Instance.WriteLocaleIni(UserData, false);
            }
        }


        public LocaleINIMod()
        {
            Load();
        }

        public LocaleINIMod(in byte[] data) : this()
        {
            UserData = data;
        }

        public byte[] Load()
        {
            if (FileSystem.Instance == null)
                return null;

            if (OriginalData == null || OriginalData.Length == 0)
                OriginalData = FileSystem.Instance.ReadLocaleIni();


            return UserData;
        }

        public byte[] Save(in byte[] inData)
        {
            return null; ;
        }
    }
}
