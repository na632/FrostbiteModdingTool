using System;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SdkVersionAttribute : Attribute
    {
        public int Version
        {
            get;
            set;
        }

        public SdkVersionAttribute(int version)
        {
            Version = version;
        }
    }
}
