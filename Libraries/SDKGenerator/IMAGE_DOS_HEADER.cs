using FMT.FileTools;

namespace FrostyEditor
{
    internal class IMAGE_DOS_HEADER
    {
        public ushort e_magic;

        public ushort e_cblp;

        public ushort e_cp;

        public ushort e_crlc;

        public ushort e_cparhdr;

        public ushort e_minalloc;

        public ushort e_maxalloc;

        public ushort e_ss;

        public ushort e_sp;

        public ushort e_csum;

        public ushort e_ip;

        public ushort e_cs;

        public ushort e_lfarlc;

        public ushort e_ovno;

        public ushort[] e_res = new ushort[4];

        public ushort e_oemid;

        public ushort e_oeminfo;

        public ushort[] e_res2 = new ushort[10];

        public int e_lfanew;

        public void Read(NativeReader reader)
        {
            e_magic = reader.ReadUShort();
            e_cblp = reader.ReadUShort();
            e_cp = reader.ReadUShort();
            e_crlc = reader.ReadUShort();
            e_cparhdr = reader.ReadUShort();
            e_minalloc = reader.ReadUShort();
            e_maxalloc = reader.ReadUShort();
            e_ss = reader.ReadUShort();
            e_sp = reader.ReadUShort();
            e_csum = reader.ReadUShort();
            e_ip = reader.ReadUShort();
            e_cs = reader.ReadUShort();
            e_lfarlc = reader.ReadUShort();
            e_ovno = reader.ReadUShort();
            for (int i = 0; i < 4; i++)
            {
                e_res[i] = reader.ReadUShort();
            }
            e_oemid = reader.ReadUShort();
            e_oeminfo = reader.ReadUShort();
            for (int j = 0; j < 10; j++)
            {
                e_res2[j] = reader.ReadUShort();
            }
            e_lfanew = reader.ReadInt();
        }
    }
}
