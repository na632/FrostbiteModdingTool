using Frostbite.FileManagers;
using FrostbiteSdk.Frostbite.FileManagers;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA22Plugin
{
    public class LFM_Fifa22 : ChunkFileManager2022, IChunkFileManager, ICustomAssetManager
    {
		public override void Initialize(ILogger logger)
		{
			base.Initialize(logger);
		}

	}
}
