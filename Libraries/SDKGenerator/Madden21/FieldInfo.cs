using FrostbiteSdk;
using FrostbiteSdk.SdkGenerator;
using FrostySdk;
using System;

namespace SdkGenerator.Madden21
{
    public class FieldInfo : IFieldInfo
    {

        public static Random RandomEmpty = new Random();

        public bool ReadSuccessfully = false;

        public string name { get; set; }

        public ushort flags { get; set; }

        public uint offset { get; set; }

        public ushort padding1 { get; set; }

        public long typeOffset { get; set; }

        public int index { get; set; }


        private Madden21.TypeInfo parentTypeInfo { get; }
        public uint nameHash { get; set; }

        public FieldInfo(Madden21.TypeInfo parentType)
        {
            parentTypeInfo = parentType;

        }

        public void Read(MemoryReader reader)
        {
            var position = reader.Position;
            name = reader.ReadNullTerminatedString();
            if (string.IsNullOrEmpty(name))
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = parentTypeInfo.name + "_UnkField_" + RandomEmpty.Next().ToString();
                }
            }
            //else
            //{
            ReadSuccessfully = true;
            //}

            //var index = 1;
            //for(index = 1; string.IsNullOrEmpty(name) && index < 7; index++)
            //         {
            //	reader.Position = parentTypeInfo.array[index];
            //	name = reader.ReadNullTerminatedString();
            //}
            nameHash = reader.ReadUInt();
            //if (nH == -237252713)
            //{

            //}
            //if (nameHash == 4057714583)
            //{

            //}


            flags = reader.ReadUShort();
            offset = reader.ReadUShort();
            typeOffset = reader.ReadLong();
        }

        //public override void Read(MemoryReader reader)
        //{
        //	name = reader.ReadNullTerminatedString();
        //	nameHash = reader.ReadUInt();
        //	flags = reader.ReadUShort();
        //	offset = reader.ReadUShort();
        //	typeOffset = reader.ReadLong();
        //}

        public void Modify(DbObject fieldObj)
        {
            fieldObj.SetValue("nameHash", nameHash);
        }

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
