using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostbiteSdk.Managers
{
    public class ModifiedLegacyAssetEntry : AssetEntry, IModifiedAssetEntry
    {
        public byte[] Data { get; set; }

        public long? NewOffset
        {
            get; set;
        }
        public int H32 { get; set; }

        public long? CompressedOffset { get; set; }

        public long? CompressedOffsetEnd { get; set; }

        public byte[] ResMeta { get; set; }

        public uint LogicalOffset { get; set; }

        public uint LogicalSize { get; set; }

        public uint RangeStart { get; set; }

        public uint RangeEnd { get; set; }

        private int firstMip { get; set; } = -1;

        public int FirstMip
        {
            get { return firstMip; }
            set { firstMip = value; }
        }


        public bool AddToChunkBundle { get; set; } = true;

        public bool AddToTOCChunks { get; set; } = false;

        public bool IsTransientModified { get; set; }

        public List<Guid> DependentAssets { get; set; } = new List<Guid>();

        public string UserData { get; set; } = "";

        /// <summary>
        /// Only related to *.fifamod
        /// </summary>
        public virtual bool IsLegacyFile
        {
            get
            {
                return LegacyFullName != null;
            }
        }

        /// <summary>
        /// Only relavant to FIFAMod
        /// </summary>
        public virtual string LegacyFullName
        {
            get
            {
                if (!string.IsNullOrEmpty(UserData))
                {
                    if (UserData.Contains(";"))
                    {
                        return UserData.Split(";")[1];
                    }
                }
                return null;
            }
        }

        public Guid? ChunkId { get; set; }
    }
}
