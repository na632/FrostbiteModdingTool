using System;

namespace FrostySdk.Resources
{
    public class ShaderBlockHashException : Exception
    {
        public ShaderBlockHashException(int length)
            : base("A hashing exception has occurred. Length = " + length)
        {
        }
    }
}
