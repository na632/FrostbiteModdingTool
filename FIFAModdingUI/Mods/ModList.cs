using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModding;

namespace FIFAModdingUI.Mods
{
    public class ModList
    {
        public List<string> ModListItems = new List<string>();
        public List<string> ModListItemErrors = new List<string>();
        public ModListProfile ModListProfile { get; set; }

        public ModList(ModListProfile profile = null)
        {
            Load();
            //else
            //{
            //    CreateAModListJsonFile();
            //}
        }

        public string ModListLocation { get
            {
                string v = "Mods\\ModList.json";
                if (ModListProfile != null)
                {
                    Directory.CreateDirectory("Mods\\Profiles\\");
                    v = "Mods\\Profiles\\" + ModListProfile.ProfileName + "\\ModList.json";
                }

                return v;
            } }

        //private void CreateAModListJsonFile()
        //{
        //    ModListItems = System.IO.Directory.GetFiles("Mods\\", "*.fbmod").ToList();
        //    var newModListItems = new List<string>();
        //    foreach (var i in ModListItems)
        //    {
        //        var newI = i.Replace("fbmod", "");
        //        newModListItems.Add(newI);
        //        if (newI != i)
        //        {
        //            var fi = new System.IO.FileInfo(i);
        //            if (!System.IO.File.Exists(newI))
        //            {
        //                fi.CopyTo(newI);
        //                fi.Delete();
        //            }
        //        }
        //    }
        //    ModListItems = newModListItems;
        //    System.IO.File.WriteAllText("Mods\\ModList.json", Newtonsoft.Json.JsonConvert.SerializeObject(ModListItems));
        //}

        public void Load()
        {
            if (System.IO.File.Exists(ModListLocation))
            {
                ModListItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(System.IO.File.ReadAllText(ModListLocation));
                var oldCount = ModListItems.Count;
                foreach (var i in ModListItems)
                {
                    if (!File.Exists(i))
                    {
                        ModListItemErrors.Add($"Mod file {i} no longer exists");
                    }
                }

                ModListItems = ModListItems.Where(x => File.Exists(x)).ToList();
                if (oldCount != ModListItems.Count)
                    Save();
            }
        }

        public void Save()
        {
            System.IO.File.WriteAllText(ModListLocation, Newtonsoft.Json.JsonConvert.SerializeObject(ModListItems));
        }
    }

    public class ModListProfile
    {
        public string ProfileName { get; set; }

        public string GameExecutableLocation { get; set; }

        public bool InstallLocaleFile { get; set; }

        public ModListProfile(string name)
        {
            if (name == "Default" || name == null)
                ProfileName = null;
            else
                ProfileName = name;
        }

        private string ModListProfileLocation { get
            {
                return "Mods\\Profiles\\" + ProfileName + "\\Profile.json";
            } }

        public void Load()
        {
            if (System.IO.File.Exists(ModListProfileLocation))
            {
                var r = Newtonsoft.Json.JsonConvert.DeserializeObject<ModListProfile> (System.IO.File.ReadAllText(ModListProfileLocation));
               if(r!=null)
                {
                    this.ProfileName = r.ProfileName;
                    this.InstallLocaleFile = r.InstallLocaleFile;
                    this.GameExecutableLocation = r.GameExecutableLocation;
                }
            }
        }

        public void Save()
        {
            System.IO.File.WriteAllText(ModListProfileLocation, Newtonsoft.Json.JsonConvert.SerializeObject(this));
        }

        public static IEnumerable<ModListProfile> LoadAll()
        {
            Directory.CreateDirectory("Mods\\Profiles\\");
            foreach(var d in Directory.GetDirectories("Mods\\Profiles\\"))
            {
                if(File.Exists(d + "\\ModList.json") && File.Exists(d + "\\Profile.json"))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(d);
                    var mlp = new ModListProfile(directoryInfo.Name);
                    mlp.Load();
                    yield return mlp;
                }
            }
        }
    }
}
