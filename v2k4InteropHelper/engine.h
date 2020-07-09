#pragma once
//#include "pch.h"
#include "sdk.h"

#include <fstream>

static ScriptService* script_service_instance;

class Engine
{
public:
    std::string ModsPath = "";
    std::string ModsRootPath = "";
    std::string ModsRootLegacyPath = "";

    FIFADBManager dbMgr;
    FIFAPlayersManager playersMgr;

    Engine();
    ~Engine();

    // Frostbite build info
    void LogBuildInfo();

    void Setup();
    void SetupMods();

    bool ParseDBMetaXML(const char* fPath);

    char* GetCurrentSreen();

    bool isInCM();
    int GetUserTeamID();
    char* GetTeamName(unsigned int teamid);
    char* GetPlayerName(unsigned int playerid);
    std::vector<std::string> GetDBTablesNames();
    std::vector<FIFADBFieldDesc*> GetDBTableFields(std::string table_name);
    std::vector<FIFADBRow*> GetDBTableRows(std::string table_name);
    bool EditDBTableField(std::string table_name, std::string field_name, __int64 addr, unsigned __int32 offset, std::string new_value);

    bool DumpFile(const char* fPath, const char* fPathOut);

    void ReloadDB();
    void LoadDB();

    void SetupMainLua();
    bool LUASetupComplete;

    bool ValidateFieldChange(std::string new_value, FIFADBFieldDesc* fdesc);

    ScriptService* script_service;
    ScriptFunctions* script_functions;

private:
    bool ready = false;

    // FIFA Virtual File System
    uint64_t vfs = NULL;

    CurrentSreen* scr;
    uintptr_t pCurrentScreenID = NULL;

    
    
    g_Databases* gDB;
    uintptr_t p_gDB = NULL;

    // functions
    typedef char*(__fastcall* fnGetTeamName)(__int64 pThis, unsigned int teamid);
    uintptr_t addrfnGetTeamName = NULL;
};

extern Engine g_engine;
