using FrostySdk.IO._2022.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostySdk.IO.Readers
{
    public class EbxReaderRiff : EbxReader22B
    {

        public EbxReaderRiff(Stream ebxDataStream, bool inPatched)
            : base(ebxDataStream, inPatched)
        {
        }
    }
}

