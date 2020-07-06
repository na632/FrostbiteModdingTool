using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CareerExpansionMod.CEM
{
    public partial class CEMCoreSettings
    {


        public static string CEMSettingsSaveLocation
        {
            get
            {
                //return CEMCore.CEMMyDocumentsDbSaveDirectory + "Settings.json";
                return CEMCore.CEMMyDocumentsDbSaveDirectory_RAW + "\\Settings.json";
            }
        }

        [Display(Name="Save Folder")]
        public string OtherSaveFolder { get; set; }

        [Display(Name="Allow CEM to Modify Manager Rating")]
        public bool AllowModificationOfManagerRating { get; set; }

        public CEMCoreSettings()
        {
            //Load();
        }

        public static CEMCoreSettings Load()
        {
            //if (!Directory.Exists(CEMCore.CEMMyDocumentsDbSaveDirectory))
            //    Directory.CreateDirectory(CEMCore.CEMMyDocumentsDbSaveDirectory);

            if (!File.Exists(CEMSettingsSaveLocation))
            {
                File.WriteAllText(CEMSettingsSaveLocation, JsonConvert.SerializeObject(new CEMCoreSettings()));
            }

            var settings = JsonConvert.DeserializeObject<CEMCoreSettings>(File.ReadAllText(CEMSettingsSaveLocation));
            return settings;
            //return new CEMCoreSettings();
        }

        public void Save()
        {
            if (!Directory.Exists(CEMCore.CEMMyDocumentsDbSaveDirectory))
                Directory.CreateDirectory(CEMCore.CEMMyDocumentsDbSaveDirectory);

            if (File.Exists(CEMSettingsSaveLocation))
                File.Delete(CEMSettingsSaveLocation);

            File.WriteAllText(CEMSettingsSaveLocation, JsonConvert.SerializeObject(this));
        }
    }

}
