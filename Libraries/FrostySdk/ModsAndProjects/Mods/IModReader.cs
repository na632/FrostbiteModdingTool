﻿using FMT.FileTools.Modding;

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
