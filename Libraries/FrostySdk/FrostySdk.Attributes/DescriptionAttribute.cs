using System;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DescriptionAttribute : Attribute
    {
        public string Description
        {
            get;
            set;
        }

        public DescriptionAttribute(string desc)
        {
            Description = desc;
        }
    }
}
