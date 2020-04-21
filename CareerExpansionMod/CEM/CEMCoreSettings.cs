using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                return CEMCore.CEMMyDocumentsDirectory + CEMCore.SaveFolder + ".json";
            }
        }

        public string OtherSaveFolder { get; set; }

        public void Load()
        {
            if(!File.Exists(CEMSettingsSaveLocation))
            {
                File.WriteAllText(CEMSettingsSaveLocation, JsonConvert.SerializeObject(this));
            }

            var settings = JsonConvert.DeserializeObject<CEMCoreSettings>(File.ReadAllText(CEMSettingsSaveLocation));
            OtherSaveFolder = settings.OtherSaveFolder;
        }

        public void Save()
        {
            if (File.Exists(CEMSettingsSaveLocation))
                File.Delete(CEMSettingsSaveLocation);

            File.WriteAllText(CEMSettingsSaveLocation, JsonConvert.SerializeObject(this));
        }
    }

}
