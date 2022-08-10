using System;
using System.Collections.Generic;
using FrostbiteSdk;
using FrostySdk;

namespace FrostbiteSdk.SdkGenerator
{
    public interface ISdkGenInfo
    {
        public void Read(MemoryReader memoryReader);

    }

    public interface IClassInfo : ISdkGenInfo
    {
        public ITypeInfo typeInfo { get; set; }

        public ushort id { get; set; }

        public ushort isDataContainer { get; set; }

        public byte[] padding { get; set; }

        public long parentClass { get; set; }

		public long nextOffset { get; set; }
    }

    public interface ITypeInfo : ISdkGenInfo
    {
		public int nameHash { get; set; }
        public string name { get; set; }

        public ushort flags { get; set; }

        public uint size { get; set; }

        public Guid guid { get; set; }

        public ushort padding1 { get; set; }

        public string nameSpace { get; set; }

        public ushort alignment { get; set; }

        public uint fieldCount { get; set; }

        public uint padding3 { get; set; }

        public long parentClass { get; set; }

        public List<IFieldInfo> fields { get; set; }

        public int Type => (flags >> 4) & 0x1F;

        /// <summary>
        /// DEPRECATED. Dont worry about this 
        /// </summary>
        /// <param name="classObj"></param>
        public void Modify(DbObject classObj);
    }

    public interface IFieldInfo : ISdkGenInfo
    {
        public int nameHash { get; set; }

        public string name { get; set; }

        public ushort flags { get; set; }

        public uint offset { get; set; }

        public ushort padding1 { get; set; }

        public long typeOffset { get; set; }

        public int index { get; set; }

        /// <summary>
        /// DEPRECATED. Dont worry about this 
        /// </summary>
        /// <param name="classObj"></param>
        public void Modify(DbObject classObj);

    }
}
