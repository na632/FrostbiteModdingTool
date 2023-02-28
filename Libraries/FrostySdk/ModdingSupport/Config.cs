using FMT.FileTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrostyModManager
{
    public class Config
    {
        private class ConfigSection
        {
            private Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            public void Add(string key, object value)
            {
                if (values.ContainsKey(key))
                {
                    values[key] = value;
                }
                else
                {
                    values.Add(key, value);
                }
            }

            public T Get<T>(string key, T defaultValue = default(T))
            {
                if (!values.ContainsKey(key))
                {
                    return defaultValue;
                }
                return (T)Convert.ChangeType(values[key], typeof(T));
            }

            public void Remove(string key)
            {
                if (values.ContainsKey(key))
                {
                    values.Remove(key);
                }
            }

            public IEnumerable<string> EnumerateKeys()
            {
                List<string> list = values.Keys.ToList();
                foreach (string item in list)
                {
                    yield return item;
                }
            }

            public void Write(NativeWriter writer)
            {
                foreach (string key in values.Keys)
                {
                    writer.WriteLine($"{key}={values[key].ToString()}");
                }
            }
        }

        private static Config Current;

        private Dictionary<string, ConfigSection> sections = new Dictionary<string, ConfigSection>();

        public bool LoadEntries(string configFilename)
        {
            if (!File.Exists(configFilename))
            {
                return false;
            }
            using (NativeReader nativeReader = new NativeReader(new FileStream(configFilename, FileMode.Open, FileAccess.Read)))
            {
                ConfigSection configSection = null;
                while (nativeReader.Position < nativeReader.Length)
                {
                    string text = nativeReader.ReadLine().Trim();
                    if (!text.StartsWith("#"))
                    {
                        if (text.StartsWith("["))
                        {
                            text = text.Trim('[', ']');
                            configSection = new ConfigSection();
                            sections.Add(text, configSection);
                        }
                        else
                        {
                            string[] array = text.Split('=');
                            configSection.Add(array[0], array[1]);
                        }
                    }
                }
            }
            return true;
        }

        public static bool LoadDefault(string configFilename)
        {
            Current = new Config();
            if (!File.Exists(configFilename))
            {
                return false;
            }
            using (NativeReader nativeReader = new NativeReader(new FileStream(configFilename, FileMode.Open, FileAccess.Read)))
            {
                ConfigSection configSection = null;
                while (nativeReader.Position < nativeReader.Length)
                {
                    string text = nativeReader.ReadLine().Trim();
                    if (!text.StartsWith("#"))
                    {
                        if (text.StartsWith("["))
                        {
                            text = text.Trim('[', ']');
                            configSection = new ConfigSection();
                            Current.sections.Add(text, configSection);
                        }
                        else
                        {
                            string[] array = text.Split('=');
                            if (array.Length >= 2)
                            {
                                configSection.Add(array[0], array[1]);
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static void Load(Config config)
        {
            Current = config;
        }

        public static void Save(string filename)
        {
            using (NativeWriter nativeWriter = new NativeWriter(new FileStream(filename, FileMode.Create)))
            {
                foreach (string key in Current.sections.Keys)
                {
                    nativeWriter.WriteLine($"[{key}]");
                    Current.sections[key].Write(nativeWriter);
                }
            }
        }

        public static void Delete(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        public static void Add(string section, string key, object value)
        {
            if (!Current.sections.ContainsKey(section))
            {
                Current.sections.Add(section, new ConfigSection());
            }
            Current.sections[section].Add(key, value);
        }

        public void SaveEntries(string filename)
        {
            using (NativeWriter nativeWriter = new NativeWriter(new FileStream(filename, FileMode.Create)))
            {
                foreach (string key in sections.Keys)
                {
                    nativeWriter.WriteLine($"[{key}]");
                    sections[key].Write(nativeWriter);
                }
            }
        }

        public void AddEntry(string section, string key, object value)
        {
            if (!sections.ContainsKey(section))
            {
                sections.Add(section, new ConfigSection());
            }
            sections[section].Add(key, value);
        }

        public static T Get<T>(string section, string key, T defaultValue)
        {
            return Current.GetEntry(section, key, defaultValue);
        }

        public static void Remove(string section, string key)
        {
            if (Current.sections.ContainsKey(section))
            {
                Current.sections[section].Remove(key);
            }
        }

        public T GetEntry<T>(string section, string key, T defaultValue)
        {
            if (!sections.ContainsKey(section))
            {
                return defaultValue;
            }
            return sections[section].Get(key, defaultValue);
        }

        public static IEnumerable<string> EnumerateKeys(string section)
        {
            if (!Current.sections.ContainsKey(section))
            {
                return null;
            }
            return Current.sections[section].EnumerateKeys();
        }

        public static bool ContainsSection(string section)
        {
            return Current.sections.ContainsKey(section);
        }
    }
}
