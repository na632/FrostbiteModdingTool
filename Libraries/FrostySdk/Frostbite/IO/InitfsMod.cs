using System.Collections.Generic;

namespace FrostySdk.Frostbite.IO
{
    public class InitFSMod
    {
        public string Description { get; set; } = string.Empty;

        public Dictionary<string, byte[]> Data { get; } = new Dictionary<string, byte[]>();

        public void ModifyFile(string key, byte[] data)
        {
            this.Data[key] = data;
        }

        public void Clear(string key)
        {
            if (this.Data.Remove(key))
            {
            }
        }

    }
}
