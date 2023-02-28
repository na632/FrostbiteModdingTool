using System;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DisplayNameAttribute : Attribute
    {
        public string Name
        {
            get;
            set;
        }

        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}
