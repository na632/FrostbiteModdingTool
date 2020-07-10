#pragma once


#include <string>
#include <vector>
#include <fstream>
#include "lua_libs.h"
#include "logger.h"

//typedef void* lua_State;
//typedef const char* (*lua_Reader) (lua_State* L, void* ud, size_t* sz);

//typedef int(*_lua_pcall)(lua_State* L, int nargs, int nresults, int errfunc);
//typedef int(*lua_load)(lua_State* L, lua_Reader reader, void* data, const char* chunkname, const char* mode);

//typedef struct LoadS {
//    const char* s;
//    size_t size;
//} LoadS;
//
//static const char* getS(lua_State* L, void* ud, size_t* size) {
//    LoadS* ls = (LoadS*)ud;
//    (void)L;  /* not used */
//    if (ls->size == 0) return NULL;
//    *size = ls->size;
//    ls->size = 0;
//    return ls->s;
//}



// https://github.com/OneLoneCoder/videos/blob/master/OneLoneCoder_EmbeddingLua_Part1.cpp
class LUARunner {
public:
    std::string exec_status = "";
    bool thread_started = false;

    LUARunner();
    ~LUARunner();
    
    lua_State* L();

    void Reset();
    std::string RunCode(std::string lua_code, lua_State* L);
    std::string RunCode(std::string lua_code, bool exec_all_states);

    void add_game_lua_state(lua_State* L);
    void remove_game_lua_state(lua_State* L);

    int lua_Test(lua_State* L);

    // Callables from LUA
    int lua_MessageBox(lua_State* L);
    int lua_Log(lua_State* L);
    int lua_IsInCM(lua_State* L);
    int lua_GetUserTeamID(lua_State* L);
    int lua_GetTeamName(lua_State* L);
    int lua_GetPlayerName(lua_State* L);
    int lua_ReloadDB(lua_State* L);
    int lua_GetDBTablesNames(lua_State* L);
    int lua_GetDBTableFields(lua_State* L);
    int lua_GetDBTableRows(lua_State* L);
    int lua_EditDBTableField(lua_State* L);
    

private:
    lua_State* LMain = nullptr;

    std::vector<lua_State*> game_lua_states;
};

extern LUARunner g_LRunner;