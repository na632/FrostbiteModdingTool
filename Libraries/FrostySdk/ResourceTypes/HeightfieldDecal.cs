using System;
using System.IO;

namespace FrostySdk.Resources
{
    public class HeightfieldDecal
    {
        private Stream data;

        public Stream Data => data;

        public int Size => (int)Math.Sqrt(data.Length);

        public HeightfieldDecal(Stream stream)
        {
            data = stream;
        }
    }
}
