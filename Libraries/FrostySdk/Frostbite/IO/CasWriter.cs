using FMT.FileTools;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.Frostbite.IO
{
    public class CasWriter : NativeWriter
    {
        public CasWriter(Stream inStream, bool leaveOpen = false, bool wide = false)
            : base(inStream, leaveOpen, wide)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is CasWriter writer &&
                   EqualityComparer<Stream>.Default.Equals(OutStream, writer.OutStream) &&
                   EqualityComparer<Stream>.Default.Equals(BaseStream, writer.BaseStream);
        }

        public override int GetHashCode()
        {
            int hashCode = 138834367;
            hashCode = hashCode * -1521134295 + EqualityComparer<Stream>.Default.GetHashCode(OutStream);
            hashCode = hashCode * -1521134295 + EqualityComparer<Stream>.Default.GetHashCode(BaseStream);
            return hashCode;
        }
    }
}
