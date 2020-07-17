#pragma once
#include <string>
#include "engine.h"
#include "lua_api.h"
#include <vector>

class v2k4helpers
{
public:
    std::string RunFIFAScript(std::string code) {
        if (g_engine.isInCM()) {
            logger.Write(LOG_DEBUG, "Engine::RunFIFAScript::" + code);
            g_LRunner.Reset();
            auto lstate = reinterpret_cast<lua_State*>(g_engine.script_service->Lua_State);
            g_LRunner.add_game_lua_state(lstate);
            return g_LRunner.RunCode(code, true);
        }
        return "ERROR";
    }

};

struct EngineHelper : Engine {
public:
    bool EditDBTableField(std::string table, std::string field, FIFADBRow* row, std::string newValue) {
        return g_engine.EditDBTableField("teamplayerlinks", "teamid", row->row.at("teamid")->addr, row->row.at("teamid")->offset, newValue);
    }

};

struct SDKHelper_FIFADBTable : FIFADBTable {
private:
    FIFADBRow* GetSingleRowByFieldAlreadyGotAllRows(std::string field_name, std::string value) {
        auto pkey = this->pkey;
        auto fsn = this->field_name_shortname.at("teamid");
        auto f = this->fields.at(fsn);
        //auto row = t->rows..at(0);
        auto row = std::find_if(this->rows.begin(), this->rows.end(), [&](FIFADBRow* o) { return o->row.at("playerid")->value == "190871"; });
        if (row != this->rows.end())
        {
            return *row;
        }
        return NULL;
    }
public:

    FIFADBRow* GetSingleRowByField(std::string field_name, std::string value) {
        if (rows_created) return GetSingleRowByFieldAlreadyGotAllRows(field_name, value);

        longest_values.clear();
        count_created_rows = 0;

        CreateHeaders();
        logger.Write(LOG_DEBUG, "GetSingleRowByField(%s,%s) for %s", field_name.c_str(), value.c_str(), name.c_str());
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
            }
            auto v = r->row.at(field_name)->value;
            if (v == value)
                return r;

            // Next Record
            addr = addr + record_size;
        }

        return NULL;
    }


};
