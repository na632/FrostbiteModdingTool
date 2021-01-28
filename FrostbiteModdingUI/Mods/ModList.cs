using FrostySdk;
using FrostySdk.Frosty;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModding;

namespace FIFAModdingUI.Mods
{
    public class ModList
    {
        public class ModItem
        {
            public string Path { get; set; }
            public string Type
            {
                get
                {
                    var fi = new FileInfo(Path);
                    if (fi.Exists)
                    {
                        return fi.Extension;
                    }
                    return null;
                }
            }

            public string ModType
            {
                get
                {
                    var mt = string.Empty;
                    if(!string.IsNullOrEmpty(Type))
                    {
                        switch (Type)
                        {
                            case ".fbmod":
                                mt = "Frostbite";
                                break;
                            case ".lmod":
                                mt = "Legacy";
                                break;
                            case ".zip":
                                mt = "Bundle";
                                break;
                            case ".fifamod":
                                mt = "FIFA (FET)";
                                break;
                        }
                    }
                    return mt;
                }
            }

            public PackIconKind MaterialDesignKindIcon
            {
                get
                {
                    var pik = PackIconKind.Cog;
                    switch (ModType)
                    {
                        case "Frostbite":
                            pik = PackIconKind.Cog;
                            break;
                        case "Legacy":
                            pik = PackIconKind.Injection;
                            break;
                        case "Bundle":
                            pik = PackIconKind.FolderZip;
                            break;
                        case "FIFA (FET)":
                            pik = PackIconKind.WarningCircle;
                            break;
                    }
                    return pik ;
                }
            }

            public Color IconColor
            {
                get
                {
                    var pik = Color.FromArgb(255, 0, 0, 0);
                    switch (ModType)
                    {
                        case "Frostbite":
                            break;
                        case "Legacy":
                            break;
                        case "Bundle":
                            break;
                        case "FIFA (FET)":
                            pik = Color.Red;
                            break;
                    }
                    return pik;
                }
            }

            public bool HasIssues
            {
                get
                {
                    return ModType == "FIFA (FET)";
                }
            }

            public string Warning
            {
                get
                {
                    if (HasIssues)
                    {
                        return "This mod type may have problems or missing features on this launcher.";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            public IFrostbiteMod GetFrostbiteMod()
            {
                switch (ModType)
                {
                    case "Frostbite":
                        return new FrostbiteMod(Path);
                    case "FIFA (FET)":
                        return new FIFAMod(string.Empty, Path);
                }
                return null;
            }

            public ModItem(string p)
            {
                Path = p;
            }

            public IEnumerable<BaseModResource> ModResources
            {
                get
                {
                    return GetFrostbiteMod().Resources.Where(x => x.Type != ModResourceType.Embedded);
                }
            }

            public override bool Equals(object obj)
            {
                if(obj is ModItem)
                {
                    return ((ModItem)obj).Path.ToLower() == this.Path.ToLower();
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Path.GetHashCode();
            }

            public override string ToString()
            {
                FileInfo fileInfo = new FileInfo(Path);
                if (fileInfo.Exists)
                {
                    var fbm = GetFrostbiteMod();
                    if(fbm != null)
                    {
                        if (fbm.ModDetails != null)
                        {
                            return fbm.ModDetails.Title + " (" + fbm.ModDetails.Version + ")";
                        }
                    }
                    return fileInfo.Name;
                }
                return base.ToString();
            }
        }

        public List<ModItem> ModListItems = new List<ModItem>();

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
                ModListItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ModItem>>(System.IO.File.ReadAllText(ModListLocation));
                var oldCount = ModListItems.Count;
                foreach (var i in ModListItems.Select(x=>x.Path))
                {
                    if (!File.Exists(i))
                    {
                        ModListItemErrors.Add($"Mod file {i} no longer exists");
                    }
                }

                ModListItems = ModListItems.Where(x => File.Exists(x.Path)).ToList();
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
