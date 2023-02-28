using System;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class HashAttribute : Attribute
    {
        public uint Hash { get; set; }
        //      public ulong ActualHash
        //{
        //	get;
        //	set;
        //}

        //public long ActualHash
        //{
        //	get;
        //	set;
        //}

        public HashAttribute(int inHash)
        {
            Hash = (uint)inHash;
        }

        public HashAttribute(uint inHash)
        {
            Hash = inHash;
        }
    }
}
