using FMT.FileTools;
using FMT.FileTools.Modding;
using FrostbiteSdk.FrostbiteSdk.Managers;
using FrostySdk;
using FrostySdk.Managers;
using FrostySdk.ModsAndProjects.Mods;
using System;
using System.IO;

namespace FrostbiteSdk.Frosty.Abstract
{

    public abstract class BaseModReader : NativeReader, IModReader
    {
        public BaseModReader(Stream stream)
            : base(stream)
        {
        }

        public bool IsValid { get; set; }
        public string GameName { get; set; }

        public EGame Game
        {
            get
            {
                Enum.TryParse<EGame>(GameName.Replace(" ", ""), out EGame result);

                return result;
            }
        }

        public int GameVersion { get; set; }
        public uint Version { get; set; }

        public class EmbeddedResource : BaseModResource
        {
            public override ModResourceType Type => ModResourceType.Embedded;
        }

        public class EbxResource : BaseModResource
        {
            public override ModResourceType Type => ModResourceType.Ebx;
        }

        public class ResResource : BaseModResource
        {
            private uint resType;

            private ulong resRid;

            private byte[] resMeta;

            public override ModResourceType Type => ModResourceType.Res;

            public override void Read(NativeReader reader, uint modVersion = 6u)
            {
                base.Read(reader);
                resType = reader.ReadUInt();
                resRid = reader.ReadULong();
                resMeta = reader.ReadBytes(reader.ReadInt32LittleEndian());
            }

            public override void FillAssetEntry(IAssetEntry entry)
            {
                base.FillAssetEntry(entry);
                ResAssetEntry obj = (ResAssetEntry)entry;
                obj.ResType = resType;
                obj.ResRid = resRid;
                obj.ResMeta = resMeta;
            }
        }

        public class ChunkResource : BaseModResource
        {
            private uint rangeStart;

            private uint rangeEnd;

            private uint logicalOffset;

            private uint logicalSize;

            private int h32;

            private int firstMip;

            public override ModResourceType Type => ModResourceType.Chunk;

            public override bool IsDDS
            {
                get
                {
                    return firstMip >= 0;
                }
            }

            public override void Read(NativeReader reader, uint modVersion = 6u)
            {
                base.Read(reader);
                rangeStart = reader.ReadUInt32LittleEndian();
                rangeEnd = reader.ReadUInt32LittleEndian();
                logicalOffset = reader.ReadUInt32LittleEndian();
                logicalSize = reader.ReadUInt32LittleEndian();
                h32 = reader.ReadInt32LittleEndian();
                firstMip = reader.ReadInt32LittleEndian();
            }

            public override void FillAssetEntry(IAssetEntry entry)
            {
                base.FillAssetEntry(entry);
                ChunkAssetEntry chunkAssetEntry = (ChunkAssetEntry)entry;
                chunkAssetEntry.Id = new Guid(name);
                chunkAssetEntry.RangeStart = rangeStart;
                chunkAssetEntry.RangeEnd = rangeEnd;
                chunkAssetEntry.LogicalOffset = logicalOffset;
                chunkAssetEntry.LogicalSize = logicalSize;
                chunkAssetEntry.H32 = h32;
                chunkAssetEntry.FirstMip = firstMip;
                chunkAssetEntry.IsTocChunk = base.IsTocChunk;
                if (chunkAssetEntry.FirstMip == -1 && chunkAssetEntry.RangeStart != 0)
                {
                    chunkAssetEntry.FirstMip = 0;
                }
                if (!string.IsNullOrEmpty(UserData))
                {
                    if (chunkAssetEntry.ModifiedEntry == null)
                        chunkAssetEntry.ModifiedEntry = new ModifiedAssetEntry();

                    chunkAssetEntry.ModifiedEntry.UserData = UserData;
                }
            }
        }

        public class LegacyFileResource : BaseModResource
        {
            public override ModResourceType Type => ModResourceType.Legacy;

            public override void Read(NativeReader reader, uint modVersion = 6u)
            {
                base.Read(reader);
                name = reader.ReadLengthPrefixedString();
            }

            public override void FillAssetEntry(IAssetEntry entry)
            {
                base.FillAssetEntry(entry);
                LegacyFileEntry legAssetEntry = (LegacyFileEntry)entry;
                legAssetEntry.Name = name;
            }
        }

        public class EmbeddedFileResource : BaseModResource
        {
            private bool isAppended;

            private string exportedLocation;

            public override ModResourceType Type => ModResourceType.EmbeddedFile;

            public override void Read(NativeReader reader, uint modVersion = 6u)
            {
                base.Read(reader);
                name = reader.ReadLengthPrefixedString();
                if (modVersion == 23u)
                {

                }
            }

            public override void FillAssetEntry(IAssetEntry entry)
            {
                base.FillAssetEntry(entry);
                EmbeddedFileEntry assetEntry = (EmbeddedFileEntry)entry;
                assetEntry.Name = name;
                assetEntry.ExportedRelativePath = exportedLocation;
                assetEntry.IsAppended = isAppended;
            }
        }
    }

}
