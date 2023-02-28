using FMT.FileTools;
using System.Collections.Generic;

namespace FrostySdk.Resources
{
    public class MeshVariationDbBlock : ShaderBlockResource
    {
        private List<byte[]> unknowns = new List<byte[]>();

        public override void Read(NativeReader reader, List<ShaderBlockResource> shaderBlockEntries)
        {
            base.Read(reader, shaderBlockEntries);
            long position = reader.ReadLong();
            long num = reader.ReadLong();
            reader.Position = position;
            for (long num2 = 0L; num2 < num; num2++)
            {
                unknowns.Add(reader.ReadBytes(20));
            }
        }

        internal override void Save(NativeWriter writer, List<int> relocTable, out long startOffset)
        {
            long position = writer.BaseStream.Position;
            for (int i = 0; i < unknowns.Count; i++)
            {
                writer.Write(unknowns[i]);
            }
            writer.WritePadding(8);
            base.Save(writer, relocTable, out startOffset);
            writer.Write(position);
            writer.Write((long)unknowns.Count);
        }
    }
}
