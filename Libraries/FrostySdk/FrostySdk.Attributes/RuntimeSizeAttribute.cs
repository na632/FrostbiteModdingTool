using System;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
    public class RuntimeSizeAttribute : Attribute
    {
        public int Size
        {
            get;
            set;
        }

        public RuntimeSizeAttribute(int inSize)
        {
            Size = inSize;
        }
    }
}
