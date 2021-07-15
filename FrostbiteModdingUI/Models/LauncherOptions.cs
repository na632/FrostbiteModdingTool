using FIFAModdingUI;
using FMT;
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

        private bool? useModData = true;

        public bool? UseModData
        {
            get { return useModData; }
            set { useModData = value; }
        }


        private bool? useLegacyModSupport = true;

        public bool? UseLegacyModSupport
        {
            get { return useLegacyModSupport; }
            set { useLegacyModSupport = value; }
        }

        public bool? UseLiveEditor { get; internal set; }

        public static async Task<LauncherOptions> LoadAsync()
        {
            if (File.Exists(App.ApplicationDirectory + "\\LauncherOptions.json"))
            {
                var loText = await File.ReadAllTextAsync(App.ApplicationDirectory + "\\LauncherOptions.json");
                return JsonConvert.DeserializeObject<LauncherOptions>(loText);
            }
            return new LauncherOptions();
        }

        public void Save()
        {
            File.WriteAllText(App.ApplicationDirectory + "\\LauncherOptions.json", JsonConvert.SerializeObject(this));
        }
    }
}
