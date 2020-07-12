#pragma once

#include "patscanner.h"
#include "game_hooks.h"
#include "hook_manager.h"
//#include "../external/tinyxml2.h"
#include "lua_api.h"
#include "engine.h"
#include "misc.h"

Engine::Engine()
{
    scr = nullptr;
    gDB = nullptr;
}

Engine::~Engine()
{
}

void Engine::LogBuildInfo() {
    BuildInfo* pBuildInfo = BuildInfo::GetInstance();
    if (!pBuildInfo) {
        logger.Write(LOG_WARN, "Engine.BuildInfo.dll not found");
        return;
    }

    char* pBranchName = (char*)pBuildInfo->getBranchName();
    char* pLicenseeId = (char*)pBuildInfo->getLicenseeId();
    char* pEngine = (char*)pBuildInfo->getEngine();

    __int64 ChangelistOne = pBuildInfo->getChangelistOne();
    __int64 ChangelistTwo = pBuildInfo->getChangelistTwo();
    __int64 FrostbiteChangelist = pBuildInfo->getFrostbiteChangelist();

    char* pUsername = (char*)pBuildInfo->getUsername();
    char* pUsergroup = (char*)pBuildInfo->getUsergroup();
    char* pBuildTime = (char*)pBuildInfo->getBuildTime();
    char* pBuildDate = (char*)pBuildInfo->getBuildDate();

    char* pBuildDateTime = (char*)pBuildInfo->getBuildDateTime();
    char* pBuildDateTimee = (char*)pBuildInfo->getBuildDateTimee();
    char* pBuildDateTimeee = (char*)pBuildInfo->getBuildDateTimeee();

    logger.Write(LOG_INFO, "----------------------[Frostbite BuildInfo]----------------------");
    logger.Write(LOG_INFO, "pBranchName:                    %s", pBranchName);
    logger.Write(LOG_INFO, "pLicenseeId:                    %s", pLicenseeId);
    logger.Write(LOG_INFO, "pEngine:                        %s", pEngine);
    logger.Write(LOG_INFO, "ChangelistOne:                  %llu", ChangelistOne);
    logger.Write(LOG_INFO, "ChangelistTwo:                  %llu", ChangelistTwo);
    logger.Write(LOG_INFO, "FrostbiteChangelist:            %llu", FrostbiteChangelist);
    logger.Write(LOG_INFO, "pUsername:                      %s", pUsername);
    logger.Write(LOG_INFO, "pUsergroup:                     %s", pUsergroup);
    logger.Write(LOG_INFO, "pBuildTime:                     %s", pBuildTime);
    logger.Write(LOG_INFO, "pBuildDate:                     %s", pBuildDate);
    logger.Write(LOG_INFO, "-----------------------------------------------------------------");
}

void Engine::SetupMods() {
    logger.Write(LOG_INFO, "Engine::SetupMods");

    ModsPath = "LiveEditorMods";
    createDir(ModsPath);

    ModsRootPath = ModsPath + "\\" + "root";
    createDir(ModsRootPath);

    ModsRootLegacyPath = ModsRootPath + "\\" + "Legacy";
    createDir(ModsRootLegacyPath);
}

void Engine::Setup() {
    logger.Write(LOG_INFO, "Engine::Setup");

    //SetupMods();
    installCrucialHooks();
    logger.Write(LOG_INFO, "Engine::installCrucialHooks::Complete");

    uintptr_t hkaddr = 0;
    pCurrentScreenID = g_ctx_proc.getAddr("AOB_screenID", true, 4);

    if (pCurrentScreenID == 0) {
        logger.Write(LOG_ERROR, "Engine::Setup::pCurrentScreenID==0");
        return;
    }

    ready = true;
    /*scr = reinterpret_cast<CurrentSreen*>(pCurrentScreenID);
    int scrid_len = 0;
    while (true)
    {
        scrid_len = scr->strlen;
        if (scrid_len > 0 && scrid_len <= 100) {
            logger.Write(LOG_INFO, "scr len: %d", scrid_len);
            break;
        }
        Sleep(250);
    }

    logger.Write(LOG_INFO, "Initialized at scr: %s", scr->name);*/

    // Frostbite Build Info
    //LogBuildInfo();

    // In Game Origin API
    //HookIGO();

    // Hook DirectX11 or DirectX12
    //CreateDXHook();

    //installGameHooks();

    // Get ptr for vfs
    uintptr_t vfsaddr = g_ctx_proc.getAddr("AOB_fnVFSGetClass", true, 6);
    if (vfsaddr) {
        typedef uint64_t(__fastcall* _vfsCtor)();

        vfs = ((_vfsCtor)(vfsaddr))();

        if (!ParseDBMetaXML("data/db/fifa_ng_db-meta.xml")) {
            logger.Write(LOG_ERROR, "ParseDBMetaXML failed");
            return;
        }
    }
    logger.Write(LOG_INFO, "Engine::AOB_fnVFSGetClass::Done");

    // Game DB Tables
    p_gDB = g_ctx_proc.getAddr("AOB_gDB", true, 4);
    if (p_gDB) {
        gDB = reinterpret_cast<g_Databases*>(p_gDB);
    }
    logger.Write(LOG_INFO, "Engine::AOB_gDB::Done");

    // LUA Script Functions?
    uintptr_t pScriptFunctions = g_ctx_proc.getAddr("AOB_pScriptFunctions", true, 61);
    logger.Write(LOG_DEBUG, "pScriptFunctions:  0x%08llX", pScriptFunctions);
    script_functions = reinterpret_cast<ScriptFunctions*>(pScriptFunctions);


    // LUA Script Service
    uintptr_t pGlobal = g_ctx_proc.getAddr("AOB_pGlobal", true, 3);
    logger.Write(LOG_DEBUG, "pGlobal:  0x%08llX", pGlobal);
    // 0x6757DC30

    typedef uint64_t(__fastcall* _getPtr)(__int64 pThis, __int32 uid);

    Globals* G = reinterpret_cast<Globals*>(pGlobal);

    uintptr_t fnAddr = (uintptr_t)(G->child->fnGetPtr);
    uintptr_t pScript = ((_getPtr)fnAddr)(pGlobal, 0x0D6DD491);
    logger.Write(LOG_DEBUG, "pScript:  0x%08llX", pScript);

    script_service = reinterpret_cast<ScriptService*>(pScript);
    
    uintptr_t addrfnGetPlayerName = g_ctx_proc.getAddr("AOB_fnGetPlayerName");
    logger.Write(LOG_DEBUG, "addrfnGetPlayerName:  0x%08llX", addrfnGetPlayerName);

    playersMgr.initAddrs(pScriptFunctions, addrfnGetPlayerName);

    SetupMainLua();
}

bool Engine::ParseDBMetaXML(const char* fPath) {
    logger.Write(LOG_DEBUG, "ParseDBMetaXML: %s", fPath);
    if (!vfs) {
        logger.Write(LOG_ERROR, "VFS not initialized");
        return false;
    }

    FileInfo fileInfo;
    
    bool fInf = hooks::get_file_info::addr_orgFunc(vfs, (char*)fPath, &fileInfo, 0);
    if (!fInf) {
        logger.Write(LOG_ERROR, "GetFileInfo failed. File not exists?");
        return false;
    }

    __int64 fs = fileInfo.fileSize;
    char* buf = new char[fs];
    
    bool lf = hooks::load_legacy_file::addr_orgFunc(&fileInfo, 0, fs, buf);

    if (!lf) {
        logger.Write(LOG_ERROR, "LoadLegacyFile failed");
        return false;
    }

    // Clear dbMgr
    logger.Write(LOG_DEBUG, "Clear dbMgr");
    for (const auto& tbl : dbMgr.tables) {
        for (const auto& fld : tbl.second->fields) {
            delete fld.second;
        }
        tbl.second->fields.clear();
        delete tbl.second;
    }
    dbMgr.tables.clear();

    // Parse XML
    //logger.Write(LOG_DEBUG, "Start Parse XML");
    //tinyxml2::XMLDocument xmlDoc;
    //xmlDoc.Parse(buf, fs);
    //try {
    //    tinyxml2::XMLElement* pRoot = xmlDoc.FirstChildElement("database");
    //    //logger.Write(LOG_DEBUG, "DB_NAME - %s", pRoot->Attribute("name"));

    //    for (tinyxml2::XMLElement* pTable = pRoot->FirstChildElement("table"); pTable != NULL; pTable = pTable->NextSiblingElement()) {
    //        FIFADBTable* tbl = new FIFADBTable;
    //        tbl->name = std::string(pTable->Attribute("name"));
    //        tbl->shortname = std::string(pTable->Attribute("shortname"));
    //        //logger.Write(LOG_DEBUG, "TABLE_NAME - %s (%s)", tbl->name.c_str(), tbl->shortname.c_str());

    //        for (tinyxml2::XMLElement* pField = pTable->FirstChildElement("fields")->FirstChildElement("field"); pField != NULL; pField = pField->NextSiblingElement()) {
    //            FIFADBFieldDesc* fld = new FIFADBFieldDesc;
    //            fld->name = std::string(pField->Attribute("name"));
    //            fld->shortname = std::string(pField->Attribute("shortname"));
    //            fld->type = std::string(pField->Attribute("type"));
    //            fld->depth = pField->IntAttribute("depth");
    //            fld->rangehigh = pField->IntAttribute("rangehigh");
    //            fld->rangelow = pField->IntAttribute("rangelow");

    //            if (pField->Attribute("key")) {
    //                if (strcmp(pField->Attribute("key"), "True") == 0) {
    //                    fld->is_key = true;
    //                }
    //            }
    //            tbl->fields.insert(std::pair<std::string, FIFADBFieldDesc*>(fld->shortname, fld));
    //        }

    //        dbMgr.name_shortname.insert(std::pair<std::string, std::string>(tbl->name, tbl->shortname));
    //        dbMgr.tables.insert(std::pair<std::string, FIFADBTable*>(tbl->shortname, tbl));
    //        dbMgr.tables_ordered.insert(std::pair<std::string, std::string>(tbl->name, tbl->shortname));
    //    }
    //}
    //catch (...) {
    //    logger.Write(LOG_ERROR, "ParseDBMetaXML err");
    //    return false;
    //}
    return true;

}

char* Engine::GetCurrentSreen() {
    if (scr) {
        return scr->name;
    }
    return NULL;
}

// Constraints: None.
// Params(0): Function doesn't take any params
// Return(1)(bool): True if inside career mode, otherwise return false
// Return on error: 
bool Engine::isInCM() {
    logger.Write(LOG_DEBUG, "IsInCM()");

   
    // Do a check for the pointers. If they don't exist, re-run the Setup to create them
    if (!script_service) {
        Setup();
        // If it still doesn't exist, then just block the call
        if (!script_service)
            return false;
    }

    if (!script_service->Lua_State)
        return false;

    auto lstate = reinterpret_cast<lua_State*>(script_service->Lua_State);
    return lstate != NULL;
    //// LUA state always 0 outside of CM?
    //__int64 L = reinterpret_cast<__int64>(script_service->Lua_State);

    //return !(L == 0);
}

// Constraints: Only in Career Mode.
// Params(0): Function doesn't take any params
// Return(1)(int): value from field "clubteamid" from first record in career_users table
// Return on error: 0 and write error in log file.

int Engine::GetUserTeamID() {
    if (!isInCM()) {
        logger.Write(LOG_WARN, "GetUserTeamID - not in career mode?");
        return 0;
    }

    LoadDB();

    if (dbMgr.name_shortname.count("career_users") != 1) {
        logger.Write(LOG_ERROR, "GetUserTeamID - no shortname for career_users_table");
        return 0;
    }

    auto shortname = dbMgr.name_shortname.at("career_users");
    if (dbMgr.tables.count(shortname) != 1) {
        logger.Write(LOG_ERROR, "GetUserTeamID - no career_users_table");
        return 0;
    }
    auto career_users_table = dbMgr.tables.at(shortname);
    career_users_table->CreateRows();

    if (career_users_table->rows.size() <= 0) {
        logger.Write(LOG_ERROR, "GetUserTeamID - empty career_users_table?");
        return 0;
    }

    auto pRow = career_users_table->rows[0];
    auto field = pRow->row["clubteamid"];
    
    int result = std::stoi(field->value);
    return result;
}

char* Engine::GetTeamName(unsigned int teamid) {
    if (!addrfnGetTeamName) {
        addrfnGetTeamName = g_ctx_proc.getAddr("AOB_fnGetTeamName");
        logger.Write(LOG_DEBUG, "addrfnGetTeamName:  0x%08llX", addrfnGetTeamName);
        if (!addrfnGetTeamName) {
            logger.Write(LOG_ERROR, "GetTeamName - addrfnGetTeamName: 0x%08llX", addrfnGetTeamName);
            return NULL;
        }
    };
    __int64 pThis = (__int64)script_functions->pScriptFunctions0->pScriptFunctionsUnk1->pScriptFunctionsUnk2->pLuaScriptFunctions;

    char* teamname = ((fnGetTeamName)(addrfnGetTeamName))(pThis, teamid);

    return teamname;
}

char* Engine::GetPlayerName(unsigned int playerid) {
    //if (!isInCM()) {
    //    logger.Write(LOG_ERROR, "GetPlayerName - Not in career mode");
    //    return "";
    //}

    return playersMgr.GetPlayerName(playerid);
}


std::vector<std::string> Engine::GetDBTablesNames() {
    LoadDB();
    std::vector<std::string> result;
    for (std::map<std::string, std::string>::iterator tbl = dbMgr.tables_ordered.begin(); tbl != dbMgr.tables_ordered.end(); tbl++)
    {
        if (dbMgr.tables.count(tbl->second) == 1) {
            auto db_tbl = dbMgr.tables.at(tbl->second);
            if (db_tbl->record_size > 0) {
                result.push_back(db_tbl->name);
            }
        }
    }

    return result;
}

std::vector<FIFADBFieldDesc*> Engine::GetDBTableFields(std::string table_name) {
    LoadDB();

    std::vector<FIFADBFieldDesc*> result;
    if (dbMgr.tables_ordered.count(table_name) != 1) {
        logger.Write(LOG_ERROR, "GetDBTableFields table %s not found", table_name.c_str());
        return result;
    }

    std::string shortname = dbMgr.tables_ordered.at(table_name);
    auto table = dbMgr.tables.at(shortname);

    for (std::map<std::string, FIFADBFieldDesc*>::iterator fld = table->fields.begin(); fld != table->fields.end(); fld++)
    {
        result.push_back(fld->second);
    }

    return result;
}

std::vector<FIFADBRow*> Engine::GetDBTableRows(std::string table_name) {
    LoadDB();

    std::vector<FIFADBRow*> result;
    if (dbMgr.tables_ordered.count(table_name) != 1) {
        logger.Write(LOG_ERROR, "GetDBTableRows table %s not found", table_name.c_str());
        return result;
    }

    std::string shortname = dbMgr.tables_ordered.at(table_name);
    auto table = dbMgr.tables.at(shortname);
    table->CreateRows();

    return table->rows;
}

bool Engine::EditDBTableField(std::string table_name, std::string field_name, __int64 addr, unsigned __int32 offset, std::string new_value) {
    LoadDB();

    if (dbMgr.tables_ordered.count(table_name) != 1) {
        logger.Write(LOG_ERROR, "EditDBTableField table %s not found", table_name.c_str());
        return false;
    }

    std::string shortname = dbMgr.tables_ordered.at(table_name);
    auto table = dbMgr.tables.at(shortname);

    if (table->field_name_shortname.count(field_name) != 1) {
        logger.Write(LOG_ERROR, "EditDBTableField field_name %s not found", field_name.c_str());
        return false;
    }

    std::string field_shortname = table->field_name_shortname.at(field_name);
    auto fdesc = table->fields.at(field_shortname);

    if (!ValidateFieldChange(new_value, fdesc)) {
        logger.Write(LOG_ERROR, "Field %s (table: %s) change not valid.", field_name, table_name);
        return false;
    }

    if (fdesc->itype == DBOFIELDTYPE_STRING) {
        signed __int32 maxlen = (fdesc->depth / 8) - 1;
        ZeroMemory((void*)(addr+offset), maxlen);

        const char* buf = new_value.c_str();
        memcpy((void*)(addr + offset), (void*)buf, new_value.length());
    }
    else if (fdesc->itype == DBOFIELDTYPE_REAL) {
        signed __int32 inewvalue;
        float fnew_value = std::stof(new_value);
        memcpy(&inewvalue, &fnew_value, sizeof inewvalue);

        FIFADBField* tmp_field = new FIFADBField;
        tmp_field->addr = addr;
        tmp_field->offset = offset;

        *(signed __int32*)(addr+offset) = table->GetWriteValue(inewvalue, tmp_field, fdesc);

        delete tmp_field;
    }
    else if (fdesc->itype == DBOFIELDTYPE_INTEGER) {
        signed __int32 inewvalue = std::stoi(new_value);

        FIFADBField* tmp_field = new FIFADBField;
        tmp_field->addr = addr;
        tmp_field->offset = offset;

        *(signed __int32*)(addr+offset) = table->GetWriteValue(inewvalue - fdesc->rangelow, tmp_field, fdesc);

        delete tmp_field;
    }

    return true;
}

bool Engine::ValidateFieldChange(std::string new_value, FIFADBFieldDesc* fdesc) {
    if (fdesc->itype == DBOFIELDTYPE_STRING) {
        signed __int32 maxlen = (fdesc->depth / 8) - 1;
        if (new_value.length() > maxlen) {
            logger.Write(LOG_ERROR,
                "Newval: %s is too long. String length is %d, max is %d",
                new_value.c_str(),
                new_value.length(),
                maxlen
            );
            return false;
        }
    }
    else if (fdesc->itype == DBOFIELDTYPE_REAL) {
        // Probably for float is always 0, but better check
        if (fdesc->rangelow > 0) {
            logger.Write(LOG_ERROR, "DBOFIELDTYPE_REAL Field %s is not supported", fdesc->name.c_str());
            return false;
        }
    }
    else if (fdesc->itype == DBOFIELDTYPE_INTEGER) {
        signed __int32 newval = fdesc->rangelow;
        if (!new_value.empty()) {
            newval = std::stoi(new_value);
        }

        if (newval < fdesc->rangelow) {
            logger.Write(LOG_ERROR, "Newval: %d is too small. Min is: %d", newval, fdesc->rangelow);
            return false;
        }
        else if (newval > fdesc->rangehigh) {
            logger.Write(LOG_ERROR, "Newval: %d is too big. Max is: %d", newval, fdesc->rangehigh);
            return false;
        }
    }

    return true;
}

bool Engine::DumpFile(const char* fPath, const char* fPathOut) {
    if (vfs == NULL) {
        uintptr_t addr = g_ctx_proc.getAddr("AOB_fnVFSGetClass", true, 6);
        typedef uint64_t(__fastcall* _vfsCtor)();
        vfs = ((_vfsCtor)(addr))();
    }
    //uintptr_t addr = g_ctx_proc.getAddr("AOB_fnVFSGetClass", true, 6);
    //typedef uint64_t(__fastcall* _vfsCtor)();

    //uint64_t vfs = ((_vfsCtor)(addr))();
    FileInfo fileInfo;
    
    bool fInf = hooks::get_file_info::addr_orgFunc(vfs, (char*)fPath, &fileInfo, 0);

    if (!fInf) return false;

    __int64 fs = fileInfo.fileSize;
    char* buf = new char[fs];
    
    bool lf = hooks::load_legacy_file::addr_orgFunc(&fileInfo, 0, fs, buf);

    if (lf) {
        std::ofstream outfile(fPathOut, std::ios::out | std::ios::binary);
        if (outfile.is_open())
        {
            outfile.write(buf, fs);
            outfile.close();
        }

        delete[] buf;
        return true;
    }
    else {
        delete[] buf;
        return false;
    }
}

void Engine::ReloadDB() {
    dbMgr.initialized = false;

    for (std::map<std::string, FIFADBTable*>::iterator it_tbl = dbMgr.tables.begin(); it_tbl != dbMgr.tables.end(); it_tbl++) {
        auto tbl = it_tbl->second;

        tbl->Clear();
    }
    LoadDB();
}

void Engine::LoadDB() {
    if (dbMgr.initialized) return;

    try {
        int DBCount = gDB->pDBinDBContainer->pDBinDB->NumOfDBinDB;
        logger.Write(LOG_DEBUG, "DBCount - %d", DBCount);

        std::vector<DB_TABLE*> arr_nested;
        for (int i = 0; i < DBCount; i++) {
            DBObj* o = reinterpret_cast<DBObj*>(&gDB->pDBinDBContainer->pDBinDB->DBinDBArr[i]);
            //logger.Write(LOG_INFO, "DBObj %d, addr - 0x%08llX", i, gDB->pDBinDBContainer->pDBinDB->DBinDBArr[i]);
            logger.Write(LOG_DEBUG, "DBObj %d, addr - 0x%08llX, Tables: %d", i, o, o->NumOfTablesInDB);

            for (int j = 0; j < o->NumOfTablesInDB; j++) {
                DBTableClsWraper* w = reinterpret_cast<DBTableClsWraper*>(&o->pDBTables->instDBTableCls[j]);
                auto game_dbtbl = w->pDBTableCLs->instDBTable;
                arr_nested.push_back(game_dbtbl->nestedTable);
                logger.Write(LOG_DEBUG, "DBTableClsWraper %d, addr - 0x%08llX", j, w);
                dbMgr.AddTable(game_dbtbl);
            }
        }

        for (auto pTable : arr_nested) {
            dbMgr.AddTable(pTable);
        }

    }
    catch (...) {
        logger.Write(LOG_ERROR, "LoadDB err");
    }
    dbMgr.initialized = true;
}

//void Engine::unique_lua_states() {
//    sort(arr_lua_states.begin(), arr_lua_states.end());
//    std::vector<lua_State*>::iterator it;
//    it = std::unique(arr_lua_states.begin(), arr_lua_states.end());
//
//    arr_lua_states.resize(std::distance(arr_lua_states.begin(), it));
//}


void Engine::SetupMainLua() {
    g_LRunner.Reset();
    LUASetupComplete = true;
}

std::string Engine::RunFIFAScript(std::string code)
{
    if (isInCM()) {
        logger.Write(LOG_DEBUG, "Engine::RunFIFAScript::" + code);
        g_LRunner.Reset();
        auto lstate = reinterpret_cast<lua_State*>(script_service->Lua_State);
        g_LRunner.add_game_lua_state(lstate);
        return g_LRunner.RunCode(code, true);
    }
    return "ERROR";
}

//void Engine::LoadLegacyTexture(IMGUI_HELPER::ImTexture* tex, std::string path, std::string default_path) {
//    FileInfo fileInfo;
//
//    bool fInf = hooks::get_file_info::addr_orgFunc(vfs, (char*)path.c_str(), &fileInfo, 0);
//
//    if (!fInf)
//        fInf = hooks::get_file_info::addr_orgFunc(vfs, (char*)default_path.c_str(), &fileInfo, 0);
//
//    if (!fInf) return;
//
//    __int64 fs = fileInfo.fileSize;
//    char* buf = new char[fs];
//
//    bool lf = hooks::load_legacy_file::addr_orgFunc(&fileInfo, 0, fs, buf);
//
//    if (lf) {
//        if (d3d11hook::CreateTexture(tex, (unsigned char*)buf, (size_t)fs)) {
//            tex->loaded = true;
//        }
//    }
//    delete[] buf;
//}

Engine g_engine;