using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Madden22Plugin
{
    public class Madden22AssetCompiler : IAssetCompiler
    {
        public const string ModDirectory = "FMTModData";
        public const string DataDirectory = "Data";
        public const string PatchDirectory = "Patch";

        public bool Compile(FileSystem fs, ILogger logger, object frostyModExecuter)
        {
            throw new NotImplementedException();
        }
    }
}
