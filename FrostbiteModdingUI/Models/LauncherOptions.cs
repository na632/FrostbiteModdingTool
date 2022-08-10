using FIFAModdingUI;
using FMT;
using FrostySdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FrostbiteModdingUI.Models
{
    public class LauncherOptions
    {
        public bool? InstallEmbeddedFiles { get; set; }
        public bool? CleanLegacyMods { get; set; }

        private bool? useModData;

        public bool? UseModData
        {
            get { return useModData; }
            set { useModData = value; }
        }


        private bool? useLegacyModSupport;

        public bool? UseLegacyModSupport
        {
            get { return useLegacyModSupport; }
            set { useLegacyModSupport = value; }
        }

        public bool? UseLiveEditor { get; internal set; }

        public static string OldSaveFileLocation
        {
            get
            {
                return AppContext.BaseDirectory + "LauncherOptions.json";

            }
        }
        public static string SaveFileLocation 
        { 
            get 
            {
                var dir = AppContext.BaseDirectory + "/Mods/Profiles/" + ProfilesLibrary.ProfileName + "/";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return dir + "/LauncherOptions.json";

            } 
        }

        public static LauncherOptions Load()
        {
            if (File.Exists(OldSaveFileLocation) && !File.Exists(SaveFileLocation))
            {
                File.Move(OldSaveFileLocation, SaveFileLocation);
            }

            if (File.Exists(SaveFileLocation))
            {
                var loText = File.ReadAllText(SaveFileLocation);
                return JsonConvert.DeserializeObject<LauncherOptions>(loText);
            }
            return new LauncherOptions();
        }

        public static async Task<LauncherOptions> LoadAsync()
        {
            return await Task.Run(() => { return Load(); });
        }

        public void Save()
        {
            File.WriteAllText(SaveFileLocation, JsonConvert.SerializeObject(this));
        }
    }
}
