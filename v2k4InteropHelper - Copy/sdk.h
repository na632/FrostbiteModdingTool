#pragma once
#pragma warning(disable: 4996)
#include <Windows.h>
#include <map>
#include <algorithm>
#include <string>
#include <algorithm> 
#include <vector>

#pragma region GameClasses
class CurrentSreen
{
public:
    char* name;
    __int32 strlen;
    __int8 unk1; // 
    __int8 unk2; // 
    __int8 unk3; // 
    __int8 unk4; // Always 0x80?
};

class Globals
{
public:
    class GlobalsChild* child; //0x0000
}; //Size: 0x0008

class GlobalsChild
{
public:
    void* fnUnk1; //0x0000
    void* fnUnk2; //0x0008
    void* fnUnk3; //0x0010
    void* fnUnk4; //0x0018
    void* fnUnk5; //0x0020
    void* fnUnk6; //0x0028
    void* fnGetPtr; //0x0030
    void* fnUnk7; //0x0038
    char pad_0040[248]; //0x0040
}; //Size: 0x0138

class LuaCMEngineFuncReg
{
public:
    char* fName; //0x0000
    int32_t nArgs; //0x0008
    int32_t Unk; //0x000C
    void* pCFunc; //0x0010
}; //Size: 0x0018


class ScriptFunctions
{
public:
    class ScriptFunctions0* pScriptFunctions0; //0x0000
    char pad_0008[8]; //0x0008
}; //Size: 0x0010

class ScriptFunctions0
{
public:
    char pad_0000[4568]; //0x0000
    class ScriptFunctionsUnk1* pScriptFunctionsUnk1; //0x11D8
}; //Size: 0x11E0

class ScriptFunctionsUnk1
{
public:
    class ScriptFunctionsUnk2* pScriptFunctionsUnk2; //0x0000
}; //Size: 0x0008

class ScriptFunctionsUnk2
{
public:
    class LuaScriptFunctions3* pLuaScriptFunctions; //0x0000
}; //Size: 0x0008

class LuaScriptFunctions3
{
public:
    void* pThis; //0x0000
}; //Size: 0x0008



class ScriptService
{
public:
    char pad_0000[8]; //0x0000
    void* Lua_State; //0x0008
    void* mLuaCallHistory; //0x0010
    char pad_0018[40]; //0x0018
    void* MaybeLuaFunctions; //0x0040
    int32_t CompiledLuaCount; //0x0048
    char pad_004C[4]; //0x004C
    void* CompiledLua1; //0x0050
    void* N00000BCC; //0x0058
    int32_t MaybeCompiledLua1Size; //0x0060
    char pad_0064[4]; //0x0064
    void* CompiledLua2; //0x0068
    void* N00000BCF; //0x0070
    int32_t MaybeCompiledLua2Size; //0x0078
    char pad_007C[12]; //0x007C
}; //Size: 0x0088


class g_Databases
{
public:
    class DBinDBContainer* pDBinDBContainer; //0x0000

}; //Size: 0x0008

class DBinDBContainer
{
public:
    char pad_0000[16]; //0x0000
    class DBinDB* pDBinDB; //0x0010
}; //Size: 0x0018

class DBPath
{
public:
    class DBDescInst* pThis; //0x0008
    char txtPath[32]; //0x0010
    char pad_0030[32]; //0x0030

    virtual void Function0();
    virtual void Function1();
    virtual void Function2();
}; //Size: 0x0050

class DBDescInst
{
public:
    class DBPath* N000023E5; //0x0000
    char pad_0008[4]; //0x0008
    char shortname[4]; //0x000C
    char pad_0010[8]; //0x0010
    char writeshortname[4]; //0x0018
    char pad_001C[108]; //0x001C
}; //Size: 0x0088

class DB_FIELD
{
public:
    int32_t fieldtype; //0x0000
    int32_t bitoffset; //0x0004
    char fieldshortname[4]; //0x0008
    int32_t depth; //0x000C
}; //Size: 0x0010

class DB_TABLE
{
public:
    char pad_0000[8]; //0x0000
    class DB_TABLE* nestedTable; //0x0008
    char pad_0010[32]; //0x0010
    void* firstRecord; //0x0030
    char pad_0038[8]; //0x0038
    char shortname[4]; //0x0040
    int32_t record_size; //0x0044
    int32_t bit_records_count; //0x0048
    int32_t unknown0; //0x004C
    int32_t compressed_str_len; //0x0050
    char pad_0054[36]; //0x0054
    int16_t total_records; //0x0078
    int16_t total_records2; //0x007A
    int16_t written_records; //0x007C
    int16_t canceled_records; //0x007E
    int16_t Unk16; //0x0080
    int8_t fieldcount; //0x0082
    int8_t unk; //0x0083
    class DB_FIELD arrFields[999]; //0x0084
}; //Size: 0x0964

class DBTableCls
{
public:
    char pad_0008[32]; //0x0008
    class DB_TABLE* instDBTable; //0x0028
    char pad_0030[56]; //0x0030

    virtual void Function0();
    virtual void Function1();
    virtual void Function2();
    virtual void Function3();
    virtual void Function4();
    virtual void Function5();
    virtual void Function6();
    virtual void Function7();
    virtual void Function8();
    virtual void Function9();
}; //Size: 0x0068

class DBTableClsWraper
{
public:
    class DBTableCls* pDBTableCLs; //0x0000
}; //Size: 0x0008

class DBTableArr
{
public:
    class DBTableClsWraper instDBTableCls[99]; //0x0000
}; //Size: 0x0178

class DBObj
{
public:
    char vTable[8]; //0x0000
    class DBDescInst* pDBDesc; //0x0008
    char pad_0010[4]; //0x0010
    int32_t NumOfTablesInDB; //0x0014
    char pad_0018[8]; //0x0018
    class DBTableArr* pDBTables; //0x0020
    char pad_0028[8]; //0x0028

    //virtual void Function0();
    //virtual void Function1();
    //virtual void Function2();
    //virtual void Function3();
    //virtual void Function4();
    //virtual void Function5();
    //virtual void Function6();
    //virtual void Function7();
    //virtual void Function8();
    //virtual void Function9();
}; //Size: 0x0030

class DBinDB
{
public:
    char pad_0000[4]; //0x0000
    char shortname_loc[4]; //0x0004
    char pad_0008[864]; //0x0008
    int32_t NumOfDBinDB; //0x0368
    char pad_036C[4]; //0x036C
    class DBObj DBinDBArr[99]; //0x0370
}; //Size: 0x0D10

class PlayerName
{
public:
    int32_t playerid; //0x0000
    char pad_0004[772]; //0x0004
    char Name[8]; //0x0308
    char pad_0310[216]; //0x0310
}; //Size: 0x03E8

class FileInfo
{
public:
    unsigned int unk1;
    unsigned int unk2;
    unsigned int unk3;
    unsigned int unk4;
    char _0x0028[0x18];
    __int64 fileSize;
};

// https://www.unknowncheats.me/forum/frostbite/162503-buildinfo.html
class BuildInfo
{
public:
    virtual const char* getBranchName();
    virtual const char* getLicenseeId();
    virtual const char* getEngine();
    virtual __int64            getChangelistOne();
    virtual __int64            getChangelistTwo();
    virtual __int64            getFrostbiteChangelist();
    virtual const char* getUsername();
    virtual bool getBool();
    virtual const char* getUsergroup();
    virtual const char* getBuildTime();
    virtual const char* getBuildDate();
    virtual const char* getBuildDateTime();
    virtual const char* getBuildDateTimee();
    virtual const char* getBuildDateTimeee();
    virtual __int64 getUnk();   // nullptr

    static BuildInfo* GetInstance()
    {
        HMODULE hBuildInfoDLL = GetModuleHandle("Engine.BuildInfo.dll");
        if (!hBuildInfoDLL) {
            return 0;
        }

        typedef BuildInfo* (__cdecl* getBuildInfo_t)(void);
        getBuildInfo_t getBuildInfo = (getBuildInfo_t)GetProcAddress(hBuildInfoDLL, "getBuildInfo");

        return getBuildInfo();
    }
};

// # Fieldtypes
// # DBOFIELDTYPE_STRING = 0,
// # DBOFIELDTYPE_INTEGER = 3,
// # DBOFIELDTYPE_REAL = 4,
// # DBOFIELDTYPE_STRING_COMPRESSED_SHORT = 13,
// # DBOFIELDTYPE_STRING_COMPRESSED_LONG = 14

enum DBFIELDTYPE {
    DBOFIELDTYPE_STRING = 0,
    DBOFIELDTYPE_INTEGER = 3,
    DBOFIELDTYPE_REAL = 4,
    DBOFIELDTYPE_STRING_COMPRESSED_SHORT = 13,
    DBOFIELDTYPE_STRING_COMPRESSED_LONG = 14
};
#pragma endregion GameClasses


#pragma region MyClasses
class HuffmannTree {
public:
    signed __int32 nodes = 0;

    std::map<signed __int32, std::map<unsigned __int32, unsigned char>> a;
    std::map<signed __int32, std::map<unsigned __int32, unsigned char>> b;
    void Create(signed __int32 nNodes, __int64 addr) {
        nodes = nNodes;

        unsigned __int32 n = 0;
        for (int i = 0; i < nodes; i++) {
            n = i * 4;
            a[i][0] = *(unsigned char*)(addr + n);
            b[i][0] = *(unsigned char*)(addr + n + 1);
            a[i][1] = *(unsigned char*)(addr + n + 2);
            b[i][1] = *(unsigned char*)(addr + n + 3);
        }
    }

    std::string ReadString(__int64 addr, unsigned __int32 strlen) {
        std::vector<unsigned char> chars = UncompressChars(addr, strlen);

        std::string s(chars.begin(), chars.end());

        //logger.Write(LOG_DEBUG, "ReadString: %s, len: %d", s.c_str(), strlen);
        //for (auto ch : chars) {
        //    logger.Write(LOG_DEBUG, "0x%02llX", ch);
        //}

        return s;
    }
private:
    std::vector<unsigned char> UncompressChars(__int64 addr, unsigned __int32 strlen) {
        std::vector<unsigned char> chars;
        unsigned char byte = 0;

        unsigned __int32 n = 0;
        unsigned __int32 n2 = 0;

        unsigned __int32 bytes_to_read = (strlen + 1);
        for (unsigned int i = 0; i < bytes_to_read; i++) {
            byte = *(unsigned char*)(addr + i);

            for (int j = 7; j >= 0; j--) {
                unsigned __int32 n3 = byte >> j & 1ULL;
                unsigned __int32 n4 = (unsigned __int32)a[n2][n3];

                if (n4 == 0) {
                    chars.push_back(b[n2][n3]);
                    if (chars.size() >= strlen) {
                        chars.push_back('\0');
                        return chars;
                    }
                    n2 = 0;
                }
                else {
                    n2 = n4;
                }
            }
        }
        chars.push_back('\0');
        return chars;
    }
};

class ScreenID
{
public:
    CurrentSreen* pCurrentScreen;
};

struct FIFADBFieldDesc {
    std::string name = "";
    std::string shortname ="";
    std::string type = "";
    __int32 itype = -1;
    __int32 depth = 0;
    __int32 rangehigh = 0;
    __int32 rangelow = 0;
    __int32 bitoffset = 0;

    unsigned __int32 offset = 0;
    unsigned __int32 startbit = 0;
    bool is_key = false;
};


struct FIFADBField {
    std::string name = "";
    char uid[32] = { "\0" };
    __int64 addr;
    unsigned __int32 offset;
    std::string value;
};

struct FIFADBRow {
    // Fieldname - row
    std::map<std::string, FIFADBField*> row;
    bool is_valid;
};

struct FIFAEditedField {
    std::string name;
    signed __int32 itype;
    __int64 addr;
    signed __int32 new_val;         // For logging only
    signed __int32 new_val_write;   // This we want to write directly into memory
    std::string s_new_val;
    unsigned __int32 str_max_len;
};

struct FIFADBTable {
    std::string name;
    std::string shortname;
    std::string pkey;

    __int32 record_size = 0;
    __int32 total_records;
    __int32 valid_records;
    __int32 written_records;
    __int32 canceled_records;
    __int64 first_record;
    __int32 n_of_fields;

    // shortname - field desc
    std::map<std::string, FIFADBFieldDesc*> fields;

    // name - shortname
    std::map<std::string, std::string> field_name_shortname;

    bool rows_created = false;
    __int32 count_created_rows = 0;
    std::vector<FIFADBRow*> rows;
    std::vector<FIFADBRow*> invalid_rows;

    // (Address_bitstart_depth) - Edited Field
    std::map<std::string, FIFAEditedField*> edited_ints;
    std::map<std::string, FIFAEditedField*> edited_floats;
    std::map<std::string, FIFAEditedField*> edited_strings;

    // Renderer stuff

    // field_name - field_value
    std::map<std::string, std::string> longest_values;

    std::vector<float> cwidths;
    std::vector<std::string> col_order;

    signed __int32 getInt(__int64 addr, unsigned __int32 offset, unsigned __int32 startbit, unsigned __int32 depth, signed __int32 rangelow) {
        unsigned __int32 v = *(unsigned __int32*)(addr + offset);
        signed __int32 a = v >> startbit;
        unsigned __int32 b = (1ULL << depth) - 1ULL;
        unsigned __int32 c = a & b;

        signed __int32 result = c + rangelow;
        return result;
    }

    float getFloat(__int64 addr, unsigned __int32 offset, unsigned __int32 startbit, unsigned __int32 depth, signed __int32 rangelow) {
        float f;
        signed __int32 i = getInt(addr, offset, startbit, depth, rangelow);
        memcpy(&f, &i, sizeof f);
        return f;
    }

    void AddEditedField(std::string newval, FIFADBField* field, FIFADBFieldDesc* fdesc) {
        FIFAEditedField* pEdited = new FIFAEditedField;
        pEdited->itype = DBOFIELDTYPE_STRING;

        __int64 addr = field->addr + field->offset;

        pEdited->name = fdesc->name;
        pEdited->addr = addr;
        pEdited->s_new_val = newval;
        pEdited->str_max_len = fdesc->depth / 8;

        unsigned __int32 depth = fdesc->depth;
        unsigned __int32 startbit = fdesc->startbit;
        std::string key = std::to_string(addr) + "_" + std::to_string(startbit) + "_" + std::to_string(depth);

        if (edited_strings.count(key) == 1) {
            auto del_pEdited = edited_strings.at(key);
            delete del_pEdited;
            edited_strings.erase(key);
        }

        edited_strings.insert(std::pair<std::string, FIFAEditedField*>(key, pEdited));
    }


    signed __int32 GetWriteValue(signed __int32 newval, FIFADBField* field, FIFADBFieldDesc* fdesc) {
        __int64 addr = field->addr + field->offset;
        signed __int32 value = *(signed __int32*)(addr);

        unsigned __int32 depth = fdesc->depth;
        unsigned __int32 startbit = fdesc->startbit;
        for (unsigned __int32 i = 0; i < depth; ++i) {
            __int32 currentbit = startbit + i;
            __int32 is_set = ((newval) >> (i)) & 1ULL;

            if (is_set == 1ULL) {
                value = value | 1ULL << currentbit;
            }
            else {
                value = value & ~(1ULL << currentbit);
            }
        }

        return value;
    }


    void AddEditedField(float newval, FIFADBField* field, FIFADBFieldDesc* fdesc) {
        FIFAEditedField* pEdited = new FIFAEditedField;
        pEdited->itype = DBOFIELDTYPE_REAL;

        __int64 addr = field->addr + field->offset;
        signed __int32 value = *(signed __int32*)(addr);

        signed __int32 inewvalue;
        memcpy(&inewvalue, &newval, sizeof inewvalue);

        pEdited->name = fdesc->name;
        pEdited->addr = addr;
        pEdited->new_val = inewvalue;
        pEdited->new_val_write = GetWriteValue(inewvalue, field, fdesc);

        unsigned __int32 depth = fdesc->depth;
        unsigned __int32 startbit = fdesc->startbit;
        std::string key = std::to_string(addr) + "_" + std::to_string(startbit) + "_" + std::to_string(depth);

        if (edited_floats.count(key) == 1) {
            auto del_pEdited = edited_floats.at(key);
            delete del_pEdited;
            edited_floats.erase(key);
        }

        edited_floats.insert(std::pair<std::string, FIFAEditedField*>(key, pEdited));
    }

    void AddEditedField(signed __int32 newval, FIFADBField* field, FIFADBFieldDesc* fdesc) {
        FIFAEditedField* pEdited = new FIFAEditedField;
        pEdited->itype = DBOFIELDTYPE_INTEGER;

        __int64 addr = field->addr + field->offset;
        signed __int32 value = *(signed __int32*)(addr);

        pEdited->name = fdesc->name;
        pEdited->addr = addr;
        pEdited->new_val = newval;
        pEdited->new_val_write = GetWriteValue(newval - fdesc->rangelow, field, fdesc);

        unsigned __int32 startbit = fdesc->startbit;
        unsigned __int32 depth = fdesc->depth;
        std::string key = std::to_string(addr) + "_" + std::to_string(startbit) + "_" + std::to_string(depth);

        if (edited_ints.count(key) == 1) {
            auto del_pEdited = edited_ints.at(key);
            delete del_pEdited;
            edited_ints.erase(key);
        }

        edited_ints.insert(std::pair<std::string, FIFAEditedField*>(key, pEdited));
    }

    void CreateHeaders() {
        std::vector<std::string> tmp_cols_to_sort;
        for (std::map<std::string, FIFADBFieldDesc*>::iterator it = fields.begin(); it != fields.end(); it++) {
            auto f = it->second;

            // Set the primary key for the table
            if ((f->is_key)) {
                pkey = f->name;
            }
            else {
                tmp_cols_to_sort.push_back(f->name);
            }
            longest_values[f->name] = f->name;

        }

        // primary key always first col
        col_order.push_back(pkey);

        sort(tmp_cols_to_sort.begin(), tmp_cols_to_sort.end());
        for (auto colname : tmp_cols_to_sort) {
            col_order.push_back(colname);
        }
    }

    void CreateRows() {
        if (rows_created) return;

        longest_values.clear();
        count_created_rows = 0;

        CreateHeaders();
        //logger.Write(LOG_DEBUG, "CreateRows for %s", name.c_str());
        __int64 addr = first_record;
        
        std::map<std::string, float> tmp_width;

        signed __int32 huf_tree_size = INT_MAX;
        signed __int32 huf_nNodes = 0;
        //__int64 compressed_strings_addr = first_record + (record_size * written_records) + 32;
        __int64 compressed_strings_addr = first_record + *(__int32*)(first_record - 0x10);

        std::vector<FIFADBField*> huf_compressed;
        for (__int32 row = 0; row < written_records; row++) {
            FIFADBRow* r = new FIFADBRow;

            // Last byte in row
            unsigned __int8 last_byte = *(unsigned __int8*)(addr + record_size - 1ULL);
            r->is_valid = !((last_byte & (char)128) > 0);

            for (std::map<std::string, FIFADBFieldDesc*>::iterator it = fields.begin(); it != fields.end(); it++) {
                auto f = it->second;

                FIFADBField* newf = new FIFADBField;
                newf->addr = addr;
                if (f->itype == DBOFIELDTYPE_STRING) {
                    const __int64 len = (f->depth / 8);
                    const char* buf = new char[len+1];
                    memcpy((void*)buf, (void*)(addr + f->offset), len);
                    newf->value = std::string(buf);

                    delete[] buf;
                }
                else if (f->itype == DBOFIELDTYPE_REAL) {
                    float result = getFloat(addr, f->offset, f->startbit, f->depth, f->rangelow);
                    newf->value = std::to_string(result);
                }
                else if (f->itype == DBOFIELDTYPE_INTEGER) {
                    signed __int32 result = getInt(addr, f->offset, f->startbit, f->depth, f->rangelow);
                    newf->value = std::to_string(result);
                }
                else if (f->itype == DBOFIELDTYPE_STRING_COMPRESSED_SHORT) {
                    //if (huf_tree_size == 0) {
                    //    __int64 addrlast = first_record + (record_size * (written_records-1));
                    //    huf_tree_size = *(signed __int32*)(addrlast + f->offset);
                    //    huf_nNodes = huf_tree_size / 4;
                    //    //logger.Write(LOG_DEBUG, "addrlast: 0x%08llX", addrlast);
                    //    //logger.Write(LOG_DEBUG, "huf_tree_size: %d", huf_tree_size);
                    //}
                    signed __int32 compressedstr_offset = *(signed __int32*)(addr + f->offset);
                    if (compressedstr_offset == -1) {
                        newf->value = "";
                    }
                    else {
                        if (compressedstr_offset < huf_tree_size) {
                            huf_tree_size = compressedstr_offset;
                        };

                        signed char strlen = *(signed char*)(compressed_strings_addr + compressedstr_offset);
                        newf->value = std::to_string((__int32)strlen);
                    }
                    newf->name = f->name;
                    huf_compressed.push_back(newf);
                }
                else {
                    //if (f->name.compare("assetid") == 0) {
                    //    int xxxxxx = 0;
                    //}
                    //logger.Write(LOG_DEBUG, "Other Field Type: %s (%d)", f->name.c_str(), f->itype);
                    //signed __int32 result = getInt(addr, f->offset, f->startbit, f->depth, f->rangelow);
                    newf->value = "TODO";
                }
                newf->offset = f->offset;
                sprintf(newf->uid, "%d-%d", row, (__int32)std::distance(fields.begin(), it));

                __int32 cur_longest_val = (__int32)longest_values.at(f->name).length();
                if (cur_longest_val < newf->value.length()) {
                    longest_values[f->name] = newf->value;
                }

                r->row[f->name] = newf;
                
                //logger.Write(LOG_DEBUG, "ROW: %d, %s = %s(%d)", row, f->name.c_str(), r->row[f->name].c_str(), v);
            }
            if (r->is_valid) {
                rows.push_back(r);
            }
            else {
                invalid_rows.push_back(r);
            }
            count_created_rows += 1;
            // Next Record
            addr = addr + record_size;
        }

        if (huf_compressed.size() > 0) {
            huf_nNodes = huf_tree_size / 4;
            HuffmannTree* pHuffmannTree = new HuffmannTree;

            pHuffmannTree->Create(huf_nNodes, compressed_strings_addr);
            for (auto f : huf_compressed) {
                
                if (f->value.empty()) continue;
                signed __int32 compressedstr_offset = *(signed __int32*)(f->addr + f->offset);
                auto strlen = *(unsigned char*)(compressed_strings_addr + compressedstr_offset);
                f->value = pHuffmannTree->ReadString(compressed_strings_addr + compressedstr_offset + 1, strlen);

                __int32 cur_longest_val = (__int32)longest_values.at(f->name).length();
                if (cur_longest_val < f->value.length()) {
                    longest_values[f->name] = f->value;
                }
            }

            delete pHuffmannTree;
        }
        //logger.Write(LOG_DEBUG, "addr: 0x%08llX", addr);
        rows_created = true;
    }

    void Clear() {
        //name.clear();
        //shortname.clear();
        //fields.clear();

        pkey.clear();
        field_name_shortname.clear();
        record_size = 0;

        edited_floats.clear();
        edited_ints.clear();
        edited_strings.clear();
        longest_values.clear();
        rows.clear();
        invalid_rows.clear();
        cwidths.clear();
        col_order.clear();
        count_created_rows = 0;
        rows_created = false;
    }

};

class FIFADBManager {
public:
    bool initialized = false;

    // table name -> table shortname
    std::map<std::string, std::string> name_shortname;
    // shortname - table
    std::map<std::string, FIFADBTable*> tables;
    
    // name - shortname
    std::map<std::string, std::string> tables_ordered;  // Ordered by table name

    // 
    void AddTable(DB_TABLE* pGameTable) {
        char shrtnm[5];
        memcpy(shrtnm, pGameTable->shortname, 4);
        shrtnm[4] = '\0';

        std::string s_shrtnm = std::string(shrtnm);
        if (tables.count(s_shrtnm) == 1) {
            auto tbl = tables.at(s_shrtnm);
            if (tbl->record_size == 0) {
                tbl->record_size = pGameTable->record_size;

                if (pGameTable->total_records != pGameTable->total_records2) {
                    /*logger.Write(LOG_WARN,
                        "total_records (%d) != total_records2 (%d)",
                        pGameTable->total_records,
                        pGameTable->total_records2
                    );*/
                }

                tbl->total_records = pGameTable->total_records;
                tbl->written_records = pGameTable->written_records;
                tbl->valid_records = pGameTable->written_records - pGameTable->canceled_records;

                //logger.Write(LOG_DEBUG, "FirstRecord addr - 0x%08llX", pGameTable->firstRecord);
                tbl->first_record = reinterpret_cast<__int64>(pGameTable->firstRecord);
                tbl->n_of_fields = pGameTable->fieldcount;
                //logger.Write(LOG_DEBUG, "n_of_fields - %d", tbl->n_of_fields);
                for (int k = 0; k < tbl->n_of_fields; k++) {
                    auto game_fld = pGameTable->arrFields[k];
                    char fshrtnm[5];
                    memcpy(fshrtnm, game_fld.fieldshortname, 4);
                    fshrtnm[4] = '\0';
                    std::string s_fshrtnm = std::string(fshrtnm);
                    if (tbl->fields.count(s_fshrtnm) == 1) {
                        auto fld = tbl->fields.at(s_fshrtnm);
                        fld->itype = game_fld.fieldtype;
                        fld->bitoffset = game_fld.bitoffset;
                        fld->startbit = game_fld.bitoffset % 8;
                        fld->offset = game_fld.bitoffset / 8;

                        tbl->field_name_shortname[fld->name] = fld->shortname;

                    }
                    else {
                        //logger.Write(LOG_WARN, "DB_FIELD %d, %s not found", k, fshrtnm);
                    }
                }
            }
            else {
                //logger.Write(LOG_DEBUG, "Already in");
            }
        }
    }
};

class FIFAPlayer {
public:
    FIFADBRow* row = NULL;
    std::string playerid = "";
    std::string full_name = "";
};

class FIFAPlayersManager {
public:
    FIFADBTable* pTable;
    bool initialized = false;
    bool thread_started = false;

    __int32 total_players = 0;
    __int32 loaded_players = 0;
    // playerid -> player
    std::map<std::string, FIFAPlayer*> players_map;

    std::vector<__int32> playerids;

    void initAddrs(uintptr_t pScriptFunctions, uintptr_t GetPlayerNameAddr) {
        script_functions = reinterpret_cast<ScriptFunctions*>(pScriptFunctions);
        fnGetPlayerNameAddr = GetPlayerNameAddr;
    }

    void SetTable(FIFADBTable* table) {
        pTable = table;
    }

    void Reload() {
        Clear();
        Load();
    }

    void Load() {
        if (initialized) return;
        pTable->CreateRows();

        auto pRows = pTable->rows;

        total_players = (__int32)pRows.size();

        __int32 counter = 0;
        for (auto pRow : pRows) {
            auto row = pRow->row;

            FIFAPlayer* newplayer = new FIFAPlayer;

            std::string pid = row["playerid"]->value;
            __int32 ipid = std::stoi(pid);


            newplayer->row = pRow;
            newplayer->playerid = pid;
            newplayer->full_name = GetPlayerName(ipid);

            players_map[pid] = newplayer;
            playerids.push_back(ipid);

            counter += 1;
            loaded_players = counter;
        }

        initialized = true;
    }

    void Clear() {
        for (std::map<std::string, FIFAPlayer*>::iterator p = players_map.begin(); p != players_map.end(); p++) {
            delete p->second;
        };

        players_map.clear();
        playerids.clear();
    }

    char* GetPlayerName(unsigned int playerid) {
        __int64 pThis = (__int64)script_functions->pScriptFunctions0->pScriptFunctionsUnk1->pScriptFunctionsUnk2->pLuaScriptFunctions;

        PlayerName* playername = ((fnGetPlayerName)(fnGetPlayerNameAddr))(pThis, playerid);

        return playername->Name;
    }

private:
    ScriptFunctions* script_functions = NULL;
    typedef PlayerName* (__fastcall* fnGetPlayerName)(__int64 pThis, unsigned int playerid);
    uintptr_t fnGetPlayerNameAddr = NULL;
};
#pragma endregion MyClasses
