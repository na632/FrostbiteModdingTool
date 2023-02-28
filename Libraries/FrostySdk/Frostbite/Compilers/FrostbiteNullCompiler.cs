using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.Interfaces;
using ModdingSupport;

namespace FrostySdk.Frostbite.Compilers
{
    public class FrostbiteNullCompiler : IAssetCompiler
    {
        public bool Cleanup(FileSystem fs, ILogger logger, ModExecutor modExecuter)
        {
            return false;
        }

        public bool Compile(FileSystem fs, ILogger logger, ModExecutor modExecuter)
        {
            logger.Log($"NULL Compiler. Doing nothing.");

            return true;
        }
    }
}
