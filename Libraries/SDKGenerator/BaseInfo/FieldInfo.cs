using FrostbiteSdk;
using FrostbiteSdk.SdkGenerator;
using FrostySdk;

namespace SdkGenerator.BaseInfo
{
    public class FieldInfo : ISdkGenInfo
    {
        public string name;

        public ushort flags;

        public uint offset;

        public ushort padding1;

        public long typeOffset;

        public int index;

        public virtual void Read(MemoryReader reader)
        {
            name = reader.ReadNullTerminatedString();
            flags = reader.ReadUShort();
            offset = reader.ReadUInt();
            padding1 = reader.ReadUShort();
            typeOffset = reader.ReadLong();
        }

        public virtual void Modify(DbObject fieldObj)
        {
        }
    }
}
