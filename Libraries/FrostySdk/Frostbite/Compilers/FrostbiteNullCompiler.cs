using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using ModdingSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.Frostbite.Compilers
{
    public class FrostbiteNullCompiler : IAssetCompiler
    {
        public bool Compile(FileSystem fs, ILogger logger, ModExecutor modExecuter)
        {
            logger.Log($"NULL Compiler. Doing nothing.");

            return true;
        }
    }
}
