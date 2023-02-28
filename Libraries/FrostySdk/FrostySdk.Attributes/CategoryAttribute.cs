using System;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CategoryAttribute : Attribute
    {
        public string Name
        {
            get;
            set;
        }

        public CategoryAttribute(string name)
        {
            Name = name;
        }
    }
}
