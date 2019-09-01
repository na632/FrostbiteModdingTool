using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Linq;

namespace v2k4FIFAModdingCL.CustomAttributes
{
    public class FIFAVersionHelper
    {
        public struct PropertyAndAttribute
        {
            public PropertyInfo PropertyInfo { get; set; }
            public FIFAVersionAttribute VersionAttribute { get; set; }
        }
        public static IEnumerable<PropertyAndAttribute> GetCustomAttributes(Object i)
        {
            var props = from p in i.GetType().GetProperties()
                        let attr = p.GetCustomAttributes(typeof(FIFAVersionAttribute), true)
                        where attr.Length == 1
                        select new PropertyAndAttribute { PropertyInfo = p, VersionAttribute = attr.First() as FIFAVersionAttribute };

            return props;

        }
    }
}
