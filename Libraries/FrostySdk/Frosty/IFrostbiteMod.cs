using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrostySdk;

namespace FrostbiteSdk
{
    public interface IFrostbiteMod : IDisposable
    {
        public string Filename { get; set; }
        public IEnumerable<BaseModResource> Resources 
        { 
            get; 
            set;
        }

        public FrostbiteModDetails ModDetails { get; set; }

        public byte[] ModBytes { get; set; }

        public byte[] GetResourceData(BaseModResource resource);

        public byte[] GetResourceData(BaseModResource resource, Stream stream);


    }
}
