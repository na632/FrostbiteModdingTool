#pragma once
#include "engine.h"
#include "game_hooks.h"
#include "lua_api.h"
//#include "renderer/gui.h"
#include "misc.h"

typedef int (LUARunner::* mem_func)(lua_State* L);
template <mem_func func>
int dispatch(lua_State* L) {
    LUARunner* ptr = *static_cast<LUARunner**>(lua_getextraspace(L));
    return ((*ptr).*func)(L);
}
LUARunner::LUARunner(){
}
LUARunner::~LUARunner() {
}

lua_State* LUARunner::L() {
    return LMain;
}

int LUARunner::lua_Test(lua_State* L) {
    float a = (float)lua_tonumber(L, 1);
    float b = (float)lua_tonumber(L, 2);
    float c = a * b;
    lua_pushnumber(L, c);
    return 1;
}

void LUARunner::Reset() {
    if (LMain)
        lua_close(LMain);
    LMain = luaL_newstate();
    luaL_openlibs(LMain);

    lua_register(LMain, "Log", &dispatch<&LUARunner::lua_Log>);
    lua_register(LMain, "MessageBox", &dispatch<&LUARunner::lua_MessageBox>);
    lua_register(LMain, "IsInCM", &dispatch<&LUARunner::lua_IsInCM>);
    lua_register(LMain, "GetUserTeamID", &dispatch<&LUARunner::lua_GetUserTeamID>);
    lua_register(LMain, "GetTeamName", &dispatch<&LUARunner::lua_GetTeamName>);
    lua_register(LMain, "GetPlayerName", &dispatch<&LUARunner::lua_GetPlayerName>);
    lua_register(LMain, "ReloadDB", &dispatch<&LUARunner::lua_ReloadDB>);
    lua_register(LMain, "GetDBTablesNames", &dispatch<&LUARunner::lua_GetDBTablesNames>);
    lua_register(LMain, "GetDBTableFields", &dispatch<&LUARunner::lua_GetDBTableFields>);
    lua_register(LMain, "GetDBTableRows", &dispatch<&LUARunner::lua_GetDBTableRows>);
    lua_register(LMain, "EditDBTableField", &dispatch<&LUARunner::lua_EditDBTableField>);

   /* const std::string live_editor_lib = g_ctx_dll.GetFolder() + "\\" + "lua\\libs\\live_editor.lua";
    if (fs::exists(live_editor_lib)) {
        luaL_dofile(LMain, live_editor_lib.c_str());
    }
    else {
        logger.Write(LOG_ERROR, "Main lua lib not found in: %s", live_editor_lib.c_str());
    }*/
    
}

int LUARunner::lua_MessageBox(lua_State* L) {
    /*g_GUI.msgbox.show = true;

    std::string title = lua_tostring(L, 1);
    if (title.empty()) {
        title = "MessageBox";
    }

    std::string content = lua_tostring(L, 2);

    g_GUI.msgbox.title = title;
    g_GUI.msgbox.text = content;*/
    return 1;
}

int LUARunner::lua_Log(lua_State* L) {
    std::string s = lua_tostring(L, 1);
    logger.Write(LOG_INFO, "[LUA] %s", s.c_str());
    return 1;
}

void LUARunner::add_game_lua_state(lua_State* L) {
    std::vector<lua_State*>::iterator position = std::find(game_lua_states.begin(), game_lua_states.end(), L);
    if (position != game_lua_states.end())
        return;

    logger.Write(LOG_DEBUG, "[LUA API] Add State L = 0x%08llX", L);
    game_lua_states.push_back(L);
}
void LUARunner::remove_game_lua_state(lua_State* L) {
    logger.Write(LOG_DEBUG, "[LUA API] Remove State L = 0x%08llX", L);
    std::vector<lua_State*>::iterator position = std::find(game_lua_states.begin(), game_lua_states.end(), L);
    if (position != game_lua_states.end())
        game_lua_states.erase(position);
}

std::string LUARunner::RunCode(std::string lua_code, lua_State* L) {
    logger.Write(LOG_INFO, "Executing lua script in provided LUA State");
    auto ret = luaL_dostring(L, lua_code.c_str());
    logger.Write(LOG_INFO, "Done");
    if (ret != LUA_OK) {
        logger.Write(LOG_ERROR, "[LUA API] Error: %s", lua_tostring(L, -1));
        lua_pop(L, 1); // pop error message
        exec_status = "Failed in all Lua states";
    }
    else {
        exec_status = "Script executed";
    }
    return exec_status;
}

std::string LUARunner::RunCode(std::string lua_code, bool exec_all_states) {

    if (exec_all_states) {
        // Execute in in-game lua states
        logger.Write(LOG_INFO, "Executing lua script in all lua states");
        // Temp lua file
        std::string tmplua_file = "_ExecutedLuaScript.lua";
        int exec_count = 0;
        // For some reson lua load buffer always return 0 even if script is invalid
        std::ofstream out(tmplua_file, std::ofstream::out | std::ofstream::trunc);
        out << lua_code << "\n";
        out.close();

        logger.Write(LOG_INFO, "[LUA API] Execute: %s on %i states", lua_code.c_str(), game_lua_states.capacity());
        std::vector<lua_State*> states(game_lua_states);

        for (auto i = states.begin(); i != states.end(); i++) {
            auto L = *i;

            __int32 x = *reinterpret_cast<__int32*>(L);

            // Check if state is valid, may not be perfect
            try {
                if (x != 0) {
                    logger.Write(LOG_ERROR, "[LUA API] Invalid L = 0x%08llX", L);
                    continue;
                }
            }
            catch (...) {
                logger.Write(LOG_ERROR, "[LUA API] Exception L = 0x%08llX", L);
                continue;
            }

            
            int status = hooks::game_lua_loadfileex::addr_orgFunc(L, tmplua_file.c_str(), NULL);
            if (!status) {
                status = hooks::game_lua_pcall::addr_orgFunc(L, 0, -1, 0);
                if (status == 0) {
                    exec_count += 1;
                    logger.Write(LOG_DEBUG, "[LUA API] Script Executed L = 0x%08llX", L);
                }
                else {
                    logger.Write(LOG_ERROR, "[LUA API] Error: %s", hooks::game_lua_tolstring::addr_orgFunc(L, -1, NULL));
                }
            }
        }

        if (exec_count > 0) {
            std::ostringstream ssExecStatus;
            ssExecStatus << "Script executed in " << exec_count << " of " << states.size() << " Lua states";
            exec_status = ssExecStatus.str();
        }
        else {
            exec_status = "Failed in all Lua states";
        }
    }
    else {
        logger.Write(LOG_INFO, "Executing lua script in main live editor lua state");
        auto ret = luaL_dostring(LMain, lua_code.c_str());
        if (ret != LUA_OK) {
            logger.Write(LOG_ERROR, "[LUA API] Error: %s", lua_tostring(LMain, -1));
            lua_pop(LMain, 1); // pop error message
            exec_status = "Failed in all Lua states";
        }
        else {
            exec_status = "Script executed";
        }
    }

    logger.Write(LOG_DEBUG, "%s", exec_status.c_str());

    thread_started = false;
    return exec_status;
}

int LUARunner::lua_GetUserTeamID(lua_State* L)
{    
    int user_teamid = g_engine.GetUserTeamID();
    lua_pushinteger(L, user_teamid);
    return 1;
}

int LUARunner::lua_GetTeamName(lua_State* L)
{
    unsigned int teamid = lua_tointeger(L, 1);
    char* teamname = g_engine.GetTeamName(teamid);
    lua_pushstring(L, teamname);
    return 1;
}

int LUARunner::lua_GetPlayerName(lua_State* L)
{
    unsigned int playerid = lua_tointeger(L, 1);
    char* playername = g_engine.GetPlayerName(playerid);
    lua_pushstring(L, playername);
    return 1;
}

int LUARunner::lua_GetDBTablesNames(lua_State* L)
{
    std::vector<std::string> table_names = g_engine.GetDBTablesNames();

    int sz_table_names = (int)table_names.size();
    lua_createtable(L, 0, sz_table_names);

    for (int i = 0; i < sz_table_names; i++) {
        lua_pushstring(L, table_names[i].c_str());
        lua_rawseti(L, -2, i+1);
    }
    return 1;
}

int LUARunner::lua_GetDBTableFields(lua_State* L)
{
    std::string table_name = lua_tostring(L, 1);
    std::vector<FIFADBFieldDesc*> fields = g_engine.GetDBTableFields(table_name);
    int sz_fields = (int)fields.size();
    lua_newtable(L);

    for (int i = 0; i < sz_fields; i++) {
        auto field = fields[i];

        lua_pushinteger(L, i + 1);
        lua_newtable(L);

        lua_pushstring(L, field->name.c_str());
        lua_setfield(L, -2, "name");

        lua_pushstring(L, field->shortname.c_str());
        lua_setfield(L, -2, "shortname");

        lua_pushboolean(L, field->is_key);
        lua_setfield(L, -2, "is_pkey");

        lua_pushinteger(L, field->rangelow);
        lua_setfield(L, -2, "min");
        
        lua_pushinteger(L, field->rangehigh);
        lua_setfield(L, -2, "max");

        lua_settable(L, -3);
    }
    return 1;
}

int LUARunner::lua_GetDBTableRows(lua_State* L) {
    std::string table_name = lua_tostring(L, 1);
    std::vector<FIFADBRow*> rows = g_engine.GetDBTableRows(table_name);

    int sz_rows = (int)rows.size();
    lua_newtable(L);
    for (int i = 0; i < sz_rows; i++) {
        auto row = rows[i];

        lua_pushinteger(L, i + 1);
        lua_newtable(L);

        int counter = 1;
        for (auto const& [key, val] : row->row) {
            lua_newtable(L);

            lua_pushstring(L, val->value.c_str());
            lua_setfield(L, -2, "value");

            // convert to string because we use 32bit lua (addr is 64bit)
            lua_pushstring(L, std::to_string(val->addr).c_str());
            lua_setfield(L, -2, "addr");

            lua_pushinteger(L, val->offset);
            lua_setfield(L, -2, "offset");

            lua_pushstring(L, table_name.c_str());
            lua_setfield(L, -2, "table_name");

            lua_pushstring(L, key.c_str());
            lua_setfield(L, -2, "name");

            lua_setfield(L, -2, key.c_str());
        }
        lua_settable(L, -3);
    }

    return 1;
}

int LUARunner::lua_EditDBTableField(lua_State* L) {
    lua_settop(L, 1);
    luaL_checktype(L, 1, LUA_TTABLE);
    lua_getfield(L, 1, "table_name");
    std::string table_name = lua_tostring(L, -1);

    lua_getfield(L, 1, "name");
    std::string field_name = lua_tostring(L, -1);

    lua_getfield(L, 1, "addr");
    std::string str_addr = lua_tostring(L, -1);
    __int64 addr = std::stoll(str_addr);

    lua_getfield(L, 1, "offset");
    unsigned __int32 offset = lua_tointeger(L, -1);

    lua_getfield(L, 1, "value");
    std::string new_value = lua_tostring(L, -1);


    bool success = g_engine.EditDBTableField(table_name, field_name, addr, offset, new_value);

    lua_pushboolean(L, success);
    return 1;
}

int LUARunner::lua_ReloadDB(lua_State* L) {
    g_engine.ReloadDB();
    return 1;
}

int LUARunner::lua_IsInCM(lua_State* L)
{
    bool isInCM = g_engine.isInCM();
    lua_pushboolean(L, isInCM);
    return 1;
}

LUARunner g_LRunner;