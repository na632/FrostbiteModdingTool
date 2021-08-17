using FrostyEditor.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrostbiteSdk;

namespace SdkGenerator.Frostbite21
{
    public enum BasicTypesEnum
    {
        kTypeCode_Void,
        kTypeCode_DbObject,
        kTypeCode_ValueType,
        kTypeCode_Class,
        kTypeCode_Array,
        kTypeCode_FixedArray,
        kTypeCode_String,
        kTypeCode_CString,
        kTypeCode_Enum,
        kTypeCode_FileRef,
        kTypeCode_Boolean,
        kTypeCode_Int8,
        kTypeCode_Uint8,
        kTypeCode_Int16,
        kTypeCode_Uint16,
        kTypeCode_Int32,
        kTypeCode_Uint32,
        kTypeCode_Int64,
        kTypeCode_Uint64,
        kTypeCode_Float32,
        kTypeCode_Float64,
        kTypeCode_Guid,
        kTypeCode_SHA1,
        kTypeCode_ResourceRef,
        kTypeCode_BasicTypeCount
    };

    public struct TestList
    {
    };//Size=0x0010

    public struct MemberInfoFlags
    {

    };//Size=0x0002

    public class ModuleInfo
    {
        public string m_ModuleName; //0x0000 
        public ModuleInfo m_NextModule; //0x0008 
        public TestList m_TestList; //0x0010 
        //char _0x0018[16]; // 0x0018 GUID
        //unsigned short _id; // 0x0028
        //unsigned short isDataContainer; // 0x002A
        //char _0x0018[4]; //0x002C
    };//Size=0x0018

    public class MemberInfoData
    {
        public string m_Name; //0x0000
        public uint m_NameHash; //0x0008
        public MemberInfoFlags m_Flags; //0x000C

        public virtual void LoadFromMemory(MemoryReader reader)
        {
            m_Name = reader.ReadNullTerminatedString();
        }

    };//Size=0x000A

    public class MemberInfo
    {
        public MemberInfoData m_InfoData; //0x0000

        public virtual void LoadFromMemory(MemoryReader reader)
        {
            m_InfoData = new MemberInfoData();
            m_InfoData.LoadFromMemory(reader);
        }

    };//Size=0x0008

    public class TypeInfoData : MemberInfoData
    {
        ushort m_TotalSize; //0x000E
        public Guid guid; // 0x0010 GUID
        public ModuleInfo m_Module; //0x0020 
        public string _0x0028; // 0x0028 UNK
        public ushort m_Alignment; //0x0030
        public ushort m_FieldCount; //0x0032
        public char[] _0x001B; // 0x0034 padding
    };//Size=0x0030

    public class TypeInfo : MemberInfo
    {
        public TypeInfo m_PrevMaybe; //0x0008
        public TypeInfo m_Next; //0x0010
        public ushort m_RuntimeId; //0x0018
        public ushort m_Flags; //0x001A

        public override void LoadFromMemory(MemoryReader reader)
        {
            // always load base first
            base.LoadFromMemory(reader);



        }

    };//Size=0x0018

}
