using FrostySdk.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostbiteModdingTests
{
    internal interface IFMTTest : ILogger
    {
        public string GamePath { get; }

        public string GameName { get; }

        public string GameEXE { get; }

        public string GamePathEXE { get; }

        public void BuildCache();
    }
}
