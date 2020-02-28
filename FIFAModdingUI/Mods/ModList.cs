using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFAModdingUI.Mods
{
    public class ModList
    {
        public List<string> ModListItems = new List<string>();

        public ModList()
        {
            if (System.IO.File.Exists("Mods\\ModList.json"))
            {
                ModListItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(System.IO.File.ReadAllText("Mods\\ModList.json"));
            }
            else
            {
                Cleanup_2020_1_Mods_List();
            }
        }

        private void Cleanup_2020_1_Mods_List()
        {
            ModListItems = System.IO.Directory.GetFiles("Mods\\", "*.fbmod").ToList();
            var newModListItems = new List<string>();
            foreach (var i in ModListItems)
            {
                var newI = System.Text.RegularExpressions.Regex.Replace(i, "[0-9][.]", "");
                newModListItems.Add(newI);
                if (newI != i)
                {
                    var fi = new System.IO.FileInfo(i);
                    if (!System.IO.File.Exists(newI))
                    {
                        fi.CopyTo(newI);
                        fi.Delete();
                    }
                }
            }
            ModListItems = newModListItems;
            System.IO.File.WriteAllText("Mods\\ModList.json", Newtonsoft.Json.JsonConvert.SerializeObject(ModListItems));
        }
    }
}
