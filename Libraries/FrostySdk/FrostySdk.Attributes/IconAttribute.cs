using System;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class IconAttribute : Attribute
    {
        public string Icon
        {
            get;
            set;
        }

        public IconAttribute(string inIcon)
        {
            Icon = inIcon;
        }
    }
}
