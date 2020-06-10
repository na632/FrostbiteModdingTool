using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFAModdingUI.Mods
{
    public class ModList
    {
        public List<string> ModListItems = new List<string>();
        public List<string> ModListItemErrors = new List<string>();

        public ModList()
        {
            Load();
            //else
            //{
            //    CreateAModListJsonFile();
            //}
        }

        private void CreateAModListJsonFile()
        {
            ModListItems = System.IO.Directory.GetFiles("Mods\\", "*.fbmod").ToList();
            var newModListItems = new List<string>();
            foreach (var i in ModListItems)
            {
                var newI = i.Replace("fbmod", "");
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

        public void Load()
        {
            if (System.IO.File.Exists("Mods\\ModList.json"))
            {
                ModListItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(System.IO.File.ReadAllText("Mods\\ModList.json"));
                foreach (var i in ModListItems)
                {
                    if (!File.Exists(i))
                    {
                        ModListItemErrors.Add($"Mod file {i} no longer exists");
                    }
                }
            }
        }

        public void Save()
        {
            System.IO.File.WriteAllText("Mods\\ModList.json", Newtonsoft.Json.JsonConvert.SerializeObject(ModListItems));
        }
    }
}
