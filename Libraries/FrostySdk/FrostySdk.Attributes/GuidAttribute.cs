using System;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class GuidAttribute : Attribute
    {
        public Guid Guid
        {
            get;
            set;
        }

        public GuidAttribute(string inGuid)
        {
            Guid = Guid.Parse(inGuid);
        }
    }
}
