using FMT.FileTools.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrostySdk.ProfileManager;

namespace FrostySdk.ModsAndProjects.Mods
{
    internal interface IModReader
    {
        public bool IsValid { get; set; }

        public string GameName { get; set; }

        public int GameVersion { get; set; }

        public uint Version { get; set; }

        public EGame Game { get; }
    }
}
