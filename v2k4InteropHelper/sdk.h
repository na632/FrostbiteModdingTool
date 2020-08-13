#pragma once
#pragma warning(disable: 4996)
#include <Windows.h>
#include <map>
#include <algorithm>
#include <sstream>
#include "logger.h"
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
    char pad_0000[4408]; //0x0000
    class ScriptFunctionsCalendarr* pScriptFunctionsCalendar; //0x1138
    char pad_1140[152]; //0x1140
    class ScriptFunctionsUnk1* pScriptFunctionsUnk1; //0x11D8
}; //Size: 0x11E0

class ScriptFunctionsCalendarr
{
public:
    class ScriptFunctionsCalendar* pScriptFunctionsCalendar0; //0x0000
}; //Size: 0x0008

class ScriptFunctionsCalendar
{
public:
    class CalendarCurrentDates0* pScriptFunctionsCalendar1; //0x0000
    char pad_0008[8]; //0x0008
    int32_t ret_current_day; //0x0010
    int32_t ret_current_month; //0x0014
    int32_t ret_current_year; //0x0018
}; //Size: 0x001C
class CalendarCurrentDates0
{
public:
    char pad_0000[824]; //0x0000
    class CalendarCurrentDates1* pCalendarCurrentDates1; //0x0338
    char pad_0340[8]; //0x0340
}; //Size: 0x0348
class CalendarCurrentDates1
{
public:
    class CalendarCurrentDatesFinal* pCalendarCurrentDatesFinal; //0x0000
}; //Size: 0x0008

class CalendarCurrentDatesFinal
{
public:
    char pad_0000[52]; //0x0000
    int32_t current_day; //0x0034
    int32_t current_month; //0x0038
    int32_t current_year; //0x003C
    char pad_0040[32]; //0x0040
}; //Size: 0x0060

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

class clsToNames
{
public:
    char pad_0000[680]; //0x0000
    class DBTableCls* pBGwe; //0x02A8
    char pad_02B0[1624]; //0x02B0
}; //Size: 0x0908

class DBTableCls
{
public:
    char pad_0008[32]; //0x0008
    class DB_TABLE* instDBTable; //0x0028
    class clsToNames* toNames; //0x0030
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
    std::string parent_table_name = "";
    std::string name = "";
    std::string shortname = "";
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
    char uid[14] = { "\0" };
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

    // pkey - row map
    std::map<std::string, FIFADBRow*> pkey_row_map;

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

    FIFADBRow* GetRowForPkey(std::string pkey_value) {
        if (pkey_row_map.count(pkey_value) == 1)
            return pkey_row_map.at(pkey_value);

        return NULL;
    }

    void CreateHeaders() {
        logger.Write(LOG_DEBUG, "CreateHeaders for %s", name.c_str());
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
        logger.Write(LOG_DEBUG, "CreateRows for %s", name.c_str());
        __int64 addr = first_record;

        std::map<std::string, float> tmp_width;

        signed __int32 huf_tree_size = INT_MAX;
        signed __int32 huf_nNodes = 0;
        //__int64 compressed_strings_addr = first_record + (record_size * written_records) + 32;
        __int64 compressed_strings_addr = 0;

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
                    const char* buf = new char[len + 1];
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

                        if (compressed_strings_addr == 0) {
                            compressed_strings_addr = first_record + *(__int32*)(first_record - 0x10);
                        }

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
                    logger.Write(LOG_DEBUG, "Other Field Type: %s (%d)", f->name.c_str(), f->itype);
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
                if (r->row.count(pkey) == 1) {
                    std::string pkey_value = r->row[pkey]->value;

                    if (pkey_row_map.count(pkey_value) == 1) {
                        logger.Write(LOG_WARN, "Pkey duplicate found. Value: %s alredy in %s table", pkey_value.c_str(), name.c_str());
                    }
                    else {
                        pkey_row_map[pkey_value] = r;
                    }
                }

                rows.push_back(r);
            }
            else {
                invalid_rows.push_back(r);
            }
            count_created_rows += 1;
            // Next Record
            addr = addr + record_size;

#ifdef _DEBUG
            // For quick testing, get only 1000 rows from players table
            if (name == "players" && count_created_rows >= 1000) {
                logger.Write(LOG_DEBUG, "DEBUG QUICK - only 1000 rows for players table");
                break;
            }
#endif
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
        rows_created = true;
    }

    void Clear() {
        //name.clear();
        //shortname.clear();
        //fields.clear();

        pkey_row_map.clear();
        pkey.clear();
        field_name_shortname.clear();
        record_size = 0;

        for (std::map<std::string, FIFAEditedField*>::iterator itr = edited_floats.begin(); itr != edited_floats.end(); itr++)
        {
            delete (itr->second);
        }
        edited_floats.clear();

        for (std::map<std::string, FIFAEditedField*>::iterator itr = edited_ints.begin(); itr != edited_ints.end(); itr++)
        {
            delete (itr->second);
        }
        edited_ints.clear();

        for (std::map<std::string, FIFAEditedField*>::iterator itr = edited_strings.begin(); itr != edited_strings.end(); itr++)
        {
            delete (itr->second);
        }
        edited_strings.clear();
        longest_values.clear();

        cwidths.clear();
        col_order.clear();
        count_created_rows = 0;

        for (int i = 0; i < invalid_rows.size(); i++) {
            auto row = invalid_rows[i];
            for (std::map<std::string, FIFADBField*>::iterator itr = row->row.begin(); itr != row->row.end(); itr++)
            {
                delete (itr->second);
            }
            row->row.clear();
            delete row;
        }
        invalid_rows.clear();

        for (int i = 0; i < rows.size(); i++) {
            auto row = rows[i];
            for (std::map<std::string, FIFADBField*>::iterator itr = row->row.begin(); itr != row->row.end(); itr++)
            {
                delete (itr->second);
            }
            row->row.clear();
            delete row;
        }
        rows.clear();
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
                    logger.Write(LOG_WARN,
                        "total_records (%d) != total_records2 (%d)",
                        pGameTable->total_records,
                        pGameTable->total_records2
                    );
                }

                tbl->total_records = pGameTable->total_records;
                tbl->written_records = pGameTable->written_records;
                tbl->valid_records = pGameTable->written_records - pGameTable->canceled_records;

                tbl->first_record = reinterpret_cast<__int64>(pGameTable->firstRecord);
                tbl->n_of_fields = pGameTable->fieldcount;
                logger.Write(
                    LOG_DEBUG,
                    "Table: %s, FirstRecord addr - 0x%08llX, n_of_fields - %d",
                    tbl->name.c_str(), pGameTable->firstRecord, tbl->n_of_fields
                );
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
                        logger.Write(LOG_WARN, "DB_FIELD %d, %s not found", k, fshrtnm);
                    }
                }
            }
            else {
                logger.Write(LOG_DEBUG, "%s already in", tbl->name.c_str());
            }
        }
    }
};

struct FIFADate {
    __int32 day;
    __int32 month;
    __int32 year;
};

inline class FIFAUtils {
public:
    //https://stackoverflow.com/questions/58595965/c-int-to-any-date-without-external-library
    // birthdate from players table to date
    // FIFA Count days since year=1582, month=10, day=14
    FIFADate BirthdateToDate(int birthdate) {
        FIFADate result;

        int a, b, c, d, e, m;
        a = birthdate + 2331204;

        b = (4 * a + 3) / 146097;
        c = -b * 146097 / 4 + a;
        d = (4 * c + 3) / 1461;
        e = -1461 * d / 4 + c;
        m = (5 * e + 2) / 153;
        result.day = -(153 * m + 2) / 5 + e + 1;
        result.month = -m / 10 * 12 + m + 3;
        result.year = b * 100 + d - 4800 + m / 10;

        return result;
    }

    int DateToBirthdate(FIFADate date) {
        int a = (14 - date.month) / 12;
        int m = date.month + 12 * a - 3;
        int y = date.year + 4800 - a;
        return date.day + (153 * m + 2) / 5 + y * 365 + y / 4 - y / 100 + y / 400 - 2331205;

    }
} FIFAUtils;

inline struct PlayerConsts {
    std::vector<std::string> int_fields = {
        "overallrating",
        "potential",
        "modifier",
        "skillmoves",
        "weakfootabilitytypecode",
        "birthdate"
    };

    std::vector<std::string> position_fields = {
        "preferredposition1",
        "preferredposition2",
        "preferredposition3",
        "preferredposition4"
    };

    std::vector<std::string> positions = {
        "GK", "SW", "RWB", "RB", "RCB", "CB", "LCB", "LB", "LWB", "RDM", "CDM", "LDM",
        "RM", "RCM", "CM", "LCM", "LM", "RAM", "CAM", "LAM", "RF", "CF", "LF", "RW", "RS",
        "ST", "LS", "LW", "NONE"
    };

    __int32 attribute_min = 1;
    __int32 attribute_max = 99;

    enum ATTRIB_LAYOUT_STYLE {
        DEFAULT,
    };

    // Same indexes as in the game
    std::vector<std::string> all_attribs{
        "acceleration",
        "sprintspeed",
        "agility",
        "balance",
        "jumping",
        "stamina",
        "strength",
        "reactions",
        "aggression",
        "composure",
        "interceptions",
        "positioning",
        "vision",
        "ballcontrol",
        "crossing",
        "dribbling",
        "finishing",
        "freekickaccuracy",
        "headingaccuracy",
        "longpassing",
        "shortpassing",
        "marking",
        "shotpower",
        "longshots",
        "standingtackle",
        "slidingtackle",
        "volleys",
        "curve",
        "penalties",
        "gkdiving",
        "gkhandling",
        "gkkicking",
        "gkreflexes",
        "gkpositioning"
    };
    std::map<int, std::map<std::string, float>> ovr_formula;

    // Attribute name - Perc of ovr

    // position id - position ids in the group
    std::vector<std::vector<int>> main_positions = {
        { 0 },
        { 2, 8 },
        { 3, 7 },
        { 4, 5, 6 },
        { 9, 10, 11 },
        { 12, 16},
        { 13, 14, 15},
        { 17, 18, 19},
        { 20, 21, 22},
        { 23, 27},
        { 24, 25, 26}
    };

    // GK (0)
    std::map<std::string, float> gk_formula = {
        {"reactions", 0.11f},
        {"gkdiving", 0.21f},
        {"gkhandling", 0.21f},
        {"gkkicking", 0.05f},
        {"gkreflexes", 0.21f},
        {"gkpositioning", 0.21f}
    };

    // RWB(2) or LWB(8)
    std::map<std::string, float> wing_back_formula = {
        {"acceleration", 0.04f},
        {"sprintspeed", 0.06f},
        {"stamina", 0.10f},
        {"reactions", 0.08f},
        {"interceptions", 0.12f},
        {"ballcontrol", 0.08f},
        {"crossing", 0.12f},
        {"dribbling", 0.04f},
        {"shortpassing", 0.10f},
        {"marking", 0.07f},
        {"standingtackle", 0.08f},
        {"slidingtackle", 0.11f}
    };

    // RB(3) or LB(7)
    std::map<std::string, float> full_back_formula = {
        {"acceleration", 0.05f},
        { "sprintspeed", 0.07f },
        { "stamina", 0.08f },
        { "reactions", 0.08f },
        { "interceptions", 0.12f },
        { "ballcontrol", 0.07f },
        { "crossing", 0.09f },
        { "headingaccuracy", 0.04f },
        { "shortpassing", 0.07f },
        { "marking", 0.08f },
        { "standingtackle", 0.11f },
        { "slidingtackle", 0.14f }
    };

    // RCB (4) or CB (5) or LCB (6)
    std::map<std::string, float> centre_back_formula = {
        {"sprintspeed", 0.02f},
        {"jumping", 0.03f},
        {"strength", 0.10f},
        {"reactions", 0.05f},
        {"aggression", 0.07f},
        {"interceptions", 0.13f},
        {"ballcontrol", 0.04f},
        {"headingaccuracy", 0.10f},
        {"shortpassing", 0.05f},
        {"marking", 0.14f},
        {"standingtackle", 0.17f},
        {"slidingtackle", 0.10f}
    };

    // RDM (9) or CDM(10) or LDM(11)
    std::map<std::string, float> def_mid_formula = {
        {"stamina", 0.06f},
        { "strength", 0.04f },
        { "reactions", 0.07f },
        { "aggression", 0.05f },
        { "interceptions", 0.14f },
        { "vision", 0.04f },
        { "ballcontrol", 0.10f },
        { "longpassing", 0.10f },
        { "shortpassing", 0.14f },
        { "marking", 0.09f },
        { "standingtackle", 0.12f },
        { "slidingtackle", 0.05f }
    };

    // RM (12) or LM (16)
    std::map<std::string, float> wing_mid_formula = {
        {"acceleration", 0.07f},
        {"sprintspeed", 0.06f},
        {"stamina", 0.05f},
        {"reactions", 0.07f},
        {"positioning", 0.08f},
        {"vision", 0.07f},
        {"ballcontrol", 0.13f},
        {"crossing", 0.10f},
        {"dribbling", 0.15f},
        {"finishing", 0.06f},
        {"longpassing", 0.05f},
        {"shortpassing", 0.11f}
    };

    // RCM (13) or CM (14) or LCM (15)
    std::map<std::string, float> centre_mid_formula = {
        {"stamina", 0.06f},
        {"reactions", 0.08f},
        {"interceptions", 0.05f},
        {"positioning", 0.06f},
        {"vision", 0.13f},
        {"ballcontrol", 0.14f},
        {"dribbling", 0.07f},
        {"finishing", 0.02f},
        {"longpassing", 0.13f},
        {"shortpassing", 0.17f},
        {"longshots", 0.04f},
        {"standingtackle", 0.05f}
    };

    // RAM (17) or CAM (18) or LAM (19)
    std::map<std::string, float> attacking_mid_formula = {
        {"acceleration", 0.04f},
        {"sprintspeed", 0.03f},
        {"agility", 0.03f},
        {"reactions", 0.07f},
        {"positioning", 0.09f},
        {"vision", 0.14f},
        {"ballcontrol", 0.15f},
        {"dribbling", 0.13f},
        {"finishing", 0.07f},
        {"longpassing", 0.04f},
        {"shortpassing", 0.16f},
        {"longshots", 0.05f}
    };

    // RF (20) or CF (21) or LF (22)
    std::map<std::string, float> forwards_formula = {
        {"acceleration", 0.05f},
        {"sprintspeed", 0.05f},
        {"reactions", 0.09f},
        {"positioning", 0.13f},
        {"vision", 0.08f},
        {"ballcontrol", 0.15f},
        {"dribbling", 0.14f},
        {"finishing", 0.11f},
        {"headingaccuracy", 0.02f},
        {"shortpassing", 0.09f},
        {"shotpower", 0.05f},
        {"longshots", 0.04f}
    };

    // RW (23) or LW (27)
    std::map<std::string, float> wingers_formula = {
        {"acceleration", 0.07f},
        {"sprintspeed", 0.06f},
        {"agility", 0.03f},
        {"reactions", 0.07f},
        {"positioning", 0.09f},
        {"vision", 0.06f},
        {"ballcontrol", 0.14f},
        {"crossing", 0.09f},
        {"dribbling", 0.16f},
        {"finishing", 0.10f},
        {"shortpassing", 0.09f},
        {"longshots", 0.04f}
    };

    // RS (24) or ST (25) or LS (26)
    std::map<std::string, float> strikers_formula = {
        {"acceleration", 0.04f},
        {"sprintspeed", 0.05f},
        {"strength", 0.05f},
        {"reactions", 0.08f},
        {"positioning", 0.13f},
        {"ballcontrol", 0.10f},
        {"dribbling", 0.07f},
        {"finishing", 0.18f},
        {"headingaccuracy", 0.10f},
        {"shortpassing", 0.05f},
        {"shotpower", 0.10f},
        {"longshots", 0.03f},
        {"volleys", 0.02f}
    };

    int GetAttribID(std::string attr) {
        std::vector<std::string>::iterator it = std::find(all_attribs.begin(), all_attribs.end(), attr);

        if (it != all_attribs.end())
            return (int)std::distance(all_attribs.begin(), it);
        else
            return -1;
    }

    std::vector<std::string> GetAttribGroupNames(ATTRIB_LAYOUT_STYLE style) {
        std::vector<std::string> grp_names;

        switch (style)
        {
        case PlayerConsts::DEFAULT:
            grp_names = {
                "attack",
                "defending",
                "skill",
                "power",
                "movement",
                "mentality",
                "goalkeeper"
            };
            break;
        default:
            break;
        }

        return grp_names;
    }

    std::vector<std::vector<std::string>> GetGroupAttributes(ATTRIB_LAYOUT_STYLE style) {
        std::vector<std::vector<std::string>> grp_attrs;

        switch (style)
        {
        case PlayerConsts::DEFAULT:
            grp_attrs = {
                { "crossing", "finishing", "headingaccuracy", "shortpassing", "volleys" },
                { "marking", "standingtackle", "slidingtackle"},
                { "dribbling", "curve", "freekickaccuracy", "longpassing", "ballcontrol"},
                { "shotpower", "jumping", "stamina", "strength", "longshots"},
                { "acceleration", "sprintspeed", "agility", "reactions", "balance"},
                { "aggression", "composure", "interceptions", "positioning", "vision", "penalties"},
                { "gkdiving", "gkhandling", "gkkicking", "gkpositioning", "gkreflexes"}
            };
            break;
        default:
            break;
        }

        return grp_attrs;
    }

    PlayerConsts() {
        // Fill ovr_formula

        std::vector< std::map<std::string, float>> grouped = {
            gk_formula,
            wing_back_formula,
            full_back_formula,
            centre_back_formula,
            def_mid_formula,
            wing_mid_formula,
            centre_mid_formula,
            attacking_mid_formula,
            forwards_formula,
            wingers_formula,
            strikers_formula
        };

        int idx = 0;
        for (auto pos_ids : main_positions) {
            for (auto pos_id : pos_ids) {
                ovr_formula[pos_id] = grouped[idx];
            }
            idx += 1;
        }
    };

} PlayerConsts;

class FIFAPlayer {
public:
    FIFADBTable* pPlayersTable = nullptr;
    FIFADBRow* row = nullptr;
    std::string filterable = "";

    std::string label = "";

    std::string playerid = "";
    std::string full_name = "";

    //std::map<std::string, 
    bool quick_loaded = false;
    bool positions_loaded = false;
    bool attribs_loaded = false;
    bool need_ovr_recalc = false;

    // Attributes
    int ovr = 0;
    int pot = 0;
    int modifier = 0;

    bool need_avg_recalc = true;
    std::map<int, int> avg_per_group;

    struct INT_FIELD {
        bool is_edited = false;
        __int32 org_val;   // Value before editing
        __int32 new_val;   // Value after editing
        __int32 min;
        __int32 max;
    };

    struct BIRTHDATE {
        FIFADate bdate;
        INT_FIELD* age;
        INT_FIELD* day;
        INT_FIELD* month;
        INT_FIELD* year;
    } birthdate;


    // field name - pINT_FIELD struct
    std::map<std::string, INT_FIELD*> int_field_val_map;

    struct CHANGES_TRACKER {
        // key - changed description
        // ex.
        // [penalties] = "Penalties, 50->90"
        std::map<std::string, std::string> map_of_changes;

        // long string of all changes
        std::string full_str;
        bool is_full_str_built = false;
    } changes_tracker;

    // BEST AT
    bool best_at_loaded = false;
    // ovr -  positions
    std::map<int, std::string> best_at_map;

    void SetAge(FIFADate current_date) {
        auto bdate = birthdate.bdate;
        int age = current_date.year - bdate.year;

        if (bdate.month > current_date.month) {
            age -= 1;
        };

        INT_FIELD* pAge = new INT_FIELD;
        INT_FIELD* pDay = new INT_FIELD;
        INT_FIELD* pMonth = new INT_FIELD;
        INT_FIELD* pYear = new INT_FIELD;

        pAge->is_edited = false;
        pAge->max = 299;
        pAge->min = 1;
        pAge->org_val = age;
        pAge->new_val = age;
        birthdate.age = pAge;

        pDay->is_edited = false;
        pDay->max = 31;
        pDay->min = 1;
        pDay->org_val = bdate.day;
        pDay->new_val = bdate.day;
        birthdate.day = pDay;

        pMonth->is_edited = false;
        pMonth->max = 12;
        pMonth->min = 1;
        pMonth->org_val = bdate.month;
        pMonth->new_val = bdate.month;
        birthdate.month = pMonth;

        pYear->is_edited = false;
        pYear->max = 2100;
        pYear->min = 1600;
        pYear->org_val = bdate.year;
        pYear->new_val = bdate.year;
        birthdate.year = pYear;
    }

    // Preffered positions
    void LoadPositions() {
        if (positions_loaded) return;
        auto r = row->row;
        for (auto fname : PlayerConsts.position_fields) {
            INT_FIELD* pPosition = new INT_FIELD;
            pPosition->org_val = pPosition->new_val = std::stoi(r[fname]->value);
            int_field_val_map[fname] = pPosition;
        }

        positions_loaded = true;
    }

    // Load Attributes from DB row
    void LoadAttribs() {
        if (attribs_loaded) return;
        auto r = row->row;
        for (auto attrib : PlayerConsts.all_attribs) {
            if (r.count(attrib) != 1) {
                logger.Write(LOG_ERROR, "Player LoadAttribs, attrib %s not found", attrib.c_str());
                continue;
            }
            INT_FIELD* pAttrib = new INT_FIELD;
            pAttrib->org_val = pAttrib->new_val = std::stoi(r.at(attrib)->value);
            int_field_val_map[attrib] = pAttrib;
        }
        attribs_loaded = true;
    }

    int CalcOvr(int posid) {
        LoadAttribs();
        if (PlayerConsts.ovr_formula.count(posid) != 1)
            return 0;
        logger.Write(LOG_DEBUG, "CalcOvr for %d", posid);

        auto formula = PlayerConsts.ovr_formula.at(posid);
        float sum = 0;
        for (auto const& [attr_name, perc] : formula) {
            auto attr_val = (float)int_field_val_map.at(attr_name)->new_val;
            sum = sum + (attr_val * perc);
        };

        int result = (int)round(sum);
        return result;
    }

    void LoadBestAt() {
        if (best_at_loaded) return;
        best_at_map.clear();

        LoadAttribs();

        auto modifier = int_field_val_map.at("modifier")->new_val;
        std::string smod = "";

        if (modifier > 0) {
            smod = "+" + std::to_string(modifier);
        }
        else if (modifier < 0) {
            smod = std::to_string(modifier);
        };

        for (auto pos_ids : PlayerConsts.main_positions) {
            int ovr = CalcOvr(pos_ids[0]);

            std::string pos_names = "";

            for (auto pos_id : pos_ids) {
                pos_names = pos_names + PlayerConsts.positions.at(pos_id) + "/";
            };
            // Remove last '/'
            if (!pos_names.empty())
                pos_names.pop_back();

            std::string best_at_record = std::to_string(ovr) + smod + "    ";
            if (best_at_map.count(ovr) == 1) {
                best_at_record = best_at_map[ovr] + "/" + pos_names;
                best_at_map[ovr] = best_at_record;
            }
            else {
                best_at_record = best_at_record + pos_names;
                best_at_map[ovr] = best_at_record;
            }
        }

        best_at_loaded = true;
    }

    void SetGroupsAvgs(int group_idx, std::vector<std::string> attributes_for_group) {
        if (!need_avg_recalc) return;
        LoadAttribs();
        unsigned __int32 sum = 0;
        unsigned __int32 num_of_attribs = 0;
        for (auto attr : attributes_for_group) {
            auto pAttr = int_field_val_map.at(attr);
            sum += pAttr->new_val;
            num_of_attribs += 1;
        }
        avg_per_group[group_idx] = (sum / num_of_attribs);
    }

    void RegisterChange(std::string key, std::string changed) {
        changes_tracker.is_full_str_built = false;
        if (changes_tracker.map_of_changes.count(key) == 1) {
            changes_tracker.map_of_changes.at(key) = changed;
        }
        else {
            changes_tracker.map_of_changes.insert({ key, changed });
        }
    }

    void RemoveChange(std::string key) {
        if (changes_tracker.map_of_changes.count(key) == 1) {
            changes_tracker.map_of_changes.erase(key);
            changes_tracker.is_full_str_built = false;
        }
    }

    void FieldChange(std::string key, INT_FIELD* fld, std::string what) {
        std::vector<std::string> vec;
        FieldChange(key, fld, what, vec);
    }

    void FieldChange(std::string key, INT_FIELD* fld, std::string what, std::vector<std::string>& id_name) {
        if (fld->org_val == fld->new_val) {
            fld->is_edited = false;
            RemoveChange(key);
        }
        else {
            fld->is_edited = true;
            std::ostringstream change_description_stream;

            if (id_name.size() > 0) {
                int v_org = fld->org_val;
                int v_new = fld->new_val;

                if (v_org < 0)
                    v_org = (int)id_name.size() - 1;

                if (v_new < 0)
                    v_new = (int)id_name.size() - 1;

                change_description_stream
                    << what
                    << ", "
                    << id_name.at(v_org)
                    << "->"
                    << id_name.at(v_new);
            }
            else {
                change_description_stream
                    << what
                    << ", "
                    << fld->org_val
                    << "->"
                    << fld->new_val;
            }

            std::string change_description = change_description_stream.str();
            RegisterChange(key, change_description);
        }
    }

    std::string GetChanges() {
        if (changes_tracker.is_full_str_built) {
            return changes_tracker.full_str;
        }

        std::string result = "";
        for (auto const& [key, val] : changes_tracker.map_of_changes) {
            std::ostringstream strstream;
            strstream << val << "\n";
            std::string str_to_app = strstream.str();
            result.append(str_to_app);
        }
        changes_tracker.full_str = result;
        changes_tracker.is_full_str_built = true;
        return changes_tracker.full_str;
    }

    bool HasUnappliedChanges() {
        return changes_tracker.map_of_changes.size() > 0;
    }

    void QuickLoad() {
        if (quick_loaded) return;
        LoadPositions();

        // Modifier, ovr, potential
        auto r = row->row;
        for (auto fname : PlayerConsts.int_fields) {
            auto desc = GetFieldDesc(fname);
            INT_FIELD* pFld = new INT_FIELD;
            pFld->org_val = pFld->new_val = std::stoi(r[fname]->value);
            pFld->min = desc->rangelow;
            pFld->max = desc->rangehigh;

            int_field_val_map[fname] = pFld;
        }

        quick_loaded = true;
    }

    void FullLoad() {
        QuickLoad();
        LoadAttribs();
        LoadPositions();
        LoadBestAt();
    }


    void Clear() {

        avg_per_group.clear();

        for (auto const& [key, val] : int_field_val_map) {
            delete val;
        }
        int_field_val_map.clear();

        changes_tracker.map_of_changes.clear();
        changes_tracker.full_str = "";
        changes_tracker.is_full_str_built = false;

        quick_loaded = false;
        positions_loaded = false;
        attribs_loaded = false;
        need_ovr_recalc = false;
        need_avg_recalc = true;

        birthdate.age->new_val = birthdate.age->org_val;
        birthdate.age->is_edited = false;

        birthdate.day->new_val = birthdate.day->org_val;
        birthdate.day->is_edited = false;

        birthdate.month->new_val = birthdate.month->org_val;
        birthdate.month->is_edited = false;

        birthdate.year->new_val = birthdate.year->org_val;
        birthdate.year->is_edited = false;
    }

private:
    FIFADBFieldDesc* GetFieldDesc(std::string fname) {
        auto shortname = pPlayersTable->field_name_shortname.at(fname);
        return pPlayersTable->fields.at(shortname);
    }

};

class FIFAPlayersManager {
public:
    FIFADBTable* pTable;
    FIFADBTable* pNamesTable;
    FIFADBTable* pDCNamesTable;
    bool initialized = false;
    bool thread_started = false;
    bool is_in_cm = false;

    FIFADate current_date;

    __int32 total_players = 0;
    __int32 loaded_players = 0;
    // playerid -> player
    std::map<std::string, FIFAPlayer*> players_map;

    std::vector<__int32> playerids;

    void initAddrs(uintptr_t pScriptService, uintptr_t pScriptFunctions, uintptr_t GetPlayerNameAddr) {
        script_service = reinterpret_cast<ScriptService*>(pScriptService);
        script_functions = reinterpret_cast<ScriptFunctions*>(pScriptFunctions);
        fnGetPlayerNameAddr = GetPlayerNameAddr;
    }

    void SetTable(FIFADBTable* table) {
        pTable = table;
    }

    void SetNamesTable(FIFADBTable* names_table, FIFADBTable* dcnames_table) {
        pNamesTable = names_table;
        pDCNamesTable = dcnames_table;
    }

    void Reload() {
        Clear();
        Load();
    }

    bool isInCM() {
        __int64 L = reinterpret_cast<__int64>(script_service->Lua_State);

        return !(L == 0);
    }

    void Load() {
        if (initialized) return;

        pNamesTable->CreateRows();
        pDCNamesTable->CreateRows();
        pTable->CreateRows();

        auto pRows = pTable->rows;

        total_players = (__int32)pRows.size();

        __int32 counter = 0;
        for (auto pRow : pRows) {
            auto row = pRow->row;

            FIFAPlayer* newplayer = new FIFAPlayer;

            std::string pid = row["playerid"]->value;
            __int32 ipid = std::stoi(pid);

            newplayer->pPlayersTable = pTable;
            newplayer->row = pRow;
            newplayer->playerid = pid;
            newplayer->full_name = GetPlayerName(ipid);

            std::string lbl = newplayer->full_name;
            lbl.erase(std::find(lbl.begin(), lbl.end(), '\0'), lbl.end());
            lbl.append(" (PlayerID: " + newplayer->playerid + ")");
            newplayer->label = lbl;

            newplayer->filterable = lbl;

            newplayer->birthdate.bdate = FIFAUtils.BirthdateToDate(std::stoi(row["birthdate"]->value));
            newplayer->SetAge(current_date);
            newplayer->QuickLoad();

            players_map[pid] = newplayer;
            playerids.push_back(ipid);

            counter += 1;
            loaded_players = counter;
        }

        initialized = true;
    }


    void PrepClear() {
        total_players = 0;
        loaded_players = 0;
        pTable = nullptr;
        pNamesTable = nullptr;
        pDCNamesTable = nullptr;
        thread_started = false;
        initialized = false;
    }

    void Clear() {
        for (std::map<std::string, FIFAPlayer*>::iterator p = players_map.begin(); p != players_map.end(); p++) {
            p->second->Clear();
            delete p->second;
        };

        players_map.clear();
        playerids.clear();

        pTable = nullptr;
        pNamesTable = nullptr;
        pDCNamesTable = nullptr;

        thread_started = false;
        initialized = false;
    }

    // FULL NAME
    std::string GetPlayerName(unsigned int playerid) {
        if (isInCM()) {
            __int64 pThis = (__int64)script_functions->pScriptFunctions0->pScriptFunctionsUnk1->pScriptFunctionsUnk2->pLuaScriptFunctions;

            PlayerName* playername = ((fnGetPlayerName)(fnGetPlayerNameAddr))(pThis, playerid);
            std::string s(playername->Name);
            return s;
        }
        else {
            pNamesTable->CreateRows();
            pDCNamesTable->CreateRows();
            pTable->CreateRows();

            auto player = pTable->GetRowForPkey(std::to_string(playerid));
            if (!player) {
                logger.Write(LOG_ERROR, "GetPlayerName pkey - %d not found in players table", playerid);
                return "";
            }

            auto first_dcnameid = std::stoi(pDCNamesTable->rows[0]->row["nameid"]->value);

            auto commonnameid = player->row["commonnameid"]->value;
            if (commonnameid != "0") {
                if (std::stoi(commonnameid) >= first_dcnameid) {
                    auto dcname = pDCNamesTable->GetRowForPkey(commonnameid);
                    if (!dcname) {
                        return "";
                    }
                    return dcname->row["name"]->value;
                }
                else {
                    auto pname = pNamesTable->GetRowForPkey(commonnameid);
                    if (!pname) {
                        return "";
                    }
                    return pname->row["name"]->value;
                }
            }
            else {
                std::string firstname = "";
                std::string lastname = "";

                auto firstnameid = player->row["firstnameid"]->value;
                FIFADBRow* firstname_row;
                if (std::stoi(firstnameid) >= first_dcnameid) {
                    firstname_row = pDCNamesTable->GetRowForPkey(firstnameid);
                }
                else {
                    firstname_row = pNamesTable->GetRowForPkey(firstnameid);
                }
                if (firstname_row)
                    firstname = firstname_row->row["name"]->value;

                auto lastnameid = player->row["lastnameid"]->value;
                FIFADBRow* lastname_row;
                if (std::stoi(lastnameid) >= first_dcnameid) {
                    lastname_row = pDCNamesTable->GetRowForPkey(lastnameid);
                }
                else {
                    lastname_row = pNamesTable->GetRowForPkey(lastnameid);
                }

                if (lastname_row)
                    lastname = lastname_row->row["name"]->value;

                std::string playername = "";

                if (!firstname.empty()) {
                    firstname.erase(firstname.size() - 1);
                    playername.append(firstname);
                    playername.append(" ");
                }
                playername.append(lastname);

                return playername;
            }

        }

    }



private:
    ScriptService* script_service = NULL;
    ScriptFunctions* script_functions = NULL;
    typedef PlayerName* (__fastcall* fnGetPlayerName)(__int64 pThis, unsigned int playerid);
    uintptr_t fnGetPlayerNameAddr = NULL;
};



#pragma endregion MyClasses
