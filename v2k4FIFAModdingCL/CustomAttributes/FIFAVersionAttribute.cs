using System;
using System.Collections.Generic;
using System.Text;

namespace v2k4FIFAModdingCL.CustomAttributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class FIFAVersionAttribute : Attribute
    {
        public FIFAVersionAttribute(string version)
        {
            Version = version;
        }

        public string Version
        {
            get;
            set;
        }
    }
}
