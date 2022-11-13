using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FMT.FileTools
{
    public static class EmbeddedResourceHelper
    {
        public static Stream GetEmbeddedResourceByName(string name)
        {
            Stream stream = null;
            string resourceName = null;
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var allResources = a.GetManifestResourceNames();
                resourceName = allResources.SingleOrDefault(x => x.EndsWith(name));
                if (!string.IsNullOrEmpty(resourceName))
                {
                    stream = a.GetManifestResourceStream(resourceName);
                    break;
                }
            }

            if (resourceName == null || stream == null)
                throw new FileNotFoundException(name + " cannot be found");

            return stream;
        }
    }
}
