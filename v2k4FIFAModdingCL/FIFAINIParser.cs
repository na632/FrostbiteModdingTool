using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FIFAModdingUI.ini
{
   




public class IniReader
    {
        Dictionary<string, Dictionary<string, string>> ini = new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);

        public bool StripComments = false;

        public IniReader(string file) : base()
        {
            LoadFromFile(file, false, Assembly.GetCallingAssembly());
        }

        public IniReader(string file, bool isResource)
        {
            LoadFromFile(file, isResource, Assembly.GetCallingAssembly());
        }

        public IniReader(string file, bool isResource, Assembly assembly)
        {
            LoadFromFile(file, isResource, assembly);
        }

        private void LoadFromFile(string file, bool isResource, Assembly assembly)
        {
            if (isResource)
            {
                if (assembly == null)
                    assembly = Assembly.GetCallingAssembly();

                var manifestResourceNames = assembly.GetManifestResourceNames();
                var resourceName = manifestResourceNames.FirstOrDefault(str => str.EndsWith(file));
                if (resourceName == null)
                    throw new FileNotFoundException("Could not find file " + file);

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    File.WriteAllText("_temp", result);
                }
            }


            var txt = File.ReadAllText(!isResource ? file : "_temp");

            if (isResource) File.Delete("_temp");

            Dictionary<string, string> currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            ini[""] = currentSection;

            foreach (var line in txt.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                   .Where(t => !string.IsNullOrWhiteSpace(t))
                                   .Select(t => t.Trim().Replace(@"\t","")))
            {
                if (StripComments && (line.StartsWith(";") || line.IndexOf("/") < 2))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    var currentAvailableSections = GetSections().ToList();
                    if ((line == "[]" || line == "[ ]") && currentAvailableSections.Contains(""))
                    {
                        currentSection = ini[""];
                        continue;
                    }
                    else
                    {
                        currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                        ini[line.Substring(1, line.LastIndexOf("]") - 1)] = currentSection;
                    }
                    continue;
                }

                var idx = line.IndexOf("=");
                if (idx == -1)
                    currentSection[line] = "";
                else
                {
                    if (line.Contains("//"))
                    {
                        var splitCommentedLine = line.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
                        if (splitCommentedLine.Length > 0 && !string.IsNullOrEmpty(splitCommentedLine[0]))
                        {
                            currentSection[line.Substring(0, idx)] = splitCommentedLine[0].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                        }
                    }
                    else
                        currentSection[line.Substring(0, idx)] = line.Substring(idx + 1);
                }
            }

        }

        public string GetValue(string key)
        {
            return GetValue(key, "", "");
        }

        public string GetValue(string key, string section)
        {

            return GetValue(key, section, "");
        }

        public string GetValue(string key, string section, string @default)
        {
            if (!ini.ContainsKey(section))
                return @default;

            if (!ini[section].ContainsKey(key))
                return @default;

            return ini[section][key].Trim();
        }

        public string[] GetKeys(string section)
        {
            if (!ini.ContainsKey(section))
                return new string[0];

            return ini[section].Keys.ToArray();
        }

        public string[] GetSections()
        {
            //return ini.Keys.Where(t => t != "").ToArray();
            return ini.Keys.ToArray();
        }
    }
}
