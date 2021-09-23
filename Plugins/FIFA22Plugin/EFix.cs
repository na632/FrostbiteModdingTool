using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFA22Plugin
{
    public class EFIX
    {
        public uint block_header { get; set; }
        public uint block_size { get; set; }

        public Guid file_Guid { get; set; }
        public uint class_type_count { get; set; }

        //struct CLASS_TYPE
        //{
        //    Guid Guid;
        //}
        //class_types[class_type_count];
        public List<Guid> class_types = new List<Guid>();   

        public Guid ClassTypeGuid { get; set; }

        public uint type_info_Guid_count { get; set; }

        //public struct TYPE_INFO_Guid
        //{
        //    uint type_info_Guid_last4bytes { get; set; }
        //}
        public List<uint> type_info_Guids = new List<uint>(); //  type_info_Guids[type_info_Guid_count];

        public uint data_offset_count { get; set; }
        public uint unk3_count = 0;

        //uint data_offsets[data_offset_count];
        public List<uint> data_offsets = new List<uint>();

        //List<uint> unk3s = new List<uint>(unk3_count - data_offset_count);
        public List<uint> unk3s = new List<uint>();

        public uint unk4_count { get; set; }
        //uint unk4s[unk4_count];
        public List<uint> unk4s = new List<uint>();

        public uint unk5_count { get; set; }
        //uint unk5s[unk5_count];
        public List<uint> unk5s = new List<uint>();

        public uint import_reference_count { get; set; }

        public struct IMPORT_REFERENCE
        {
            public Guid file_Guid { get; set; }
            public Guid class_Guid { get; set; }
        }
        //import_reference[import_reference_count];
        public List<IMPORT_REFERENCE> import_reference = new List<IMPORT_REFERENCE>();// import_reference[import_reference_count];

        public uint unk6_count;
        //uint unk6s[unk6_count];
        public List<uint> unk6s = new List<uint>();

        public uint unk7_count { get; set; }
        public List<uint> unk7s = new List<uint>();

        public uint data_size { get; set; }
        public uint total_ebx_data_size;
        public uint total_ebx_data_size_2;
    }
}
