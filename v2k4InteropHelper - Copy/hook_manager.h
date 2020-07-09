#pragma once
#include <map>
#include <string>
#include <vector>
#include <memory>

#include "AOBs.h"
#include "logger.h"
#include "globals.h"
// Hook Manager
class HookMgr {
public:
    class hkBase {
    public:
        std::unique_ptr<PLH::x64Detour> detour = nullptr;

        bool isEnabled = true;
        int count = 0;

        ~hkBase() {};

        void Toggle() {
            isEnabled = !isEnabled;
        }
    };

    class clsHk_player_heads_manager_builder : public hkBase {};
    class clsHk_read_ini : public hkBase {};
    class clsHk_get_file_info : public hkBase {};
    class clsHk_load_legacy_file : public hkBase {};
    class clsHk_process_mouse_input : public hkBase {};
    class clsHk_game_wnd_proc : public hkBase {};
    class clsHk_lua_newstate : public hkBase {};
    class clsHk_lua_closestate : public hkBase {};
    class clsHk_lua_pcall : public hkBase {};
    class clsHk_lua_getglobal : public hkBase {};
    class clsHk_lua_setglobal : public hkBase {};
    class clsHk_lua_pushcclosure : public hkBase {};
    class clsHk_lua_tolstring : public hkBase {};
    class clsHk_lual_tolstring : public hkBase {};
    class clsHk_lua_loadfileex : public hkBase {};
    class clsHk_lua_pushstring : public hkBase {};
    class clsHk_lua_load : public hkBase {};

    clsHk_player_heads_manager_builder* pHk_player_heads_manager_builder = new clsHk_player_heads_manager_builder;
    clsHk_read_ini* pHk_read_ini = new clsHk_read_ini;
    clsHk_get_file_info* pHk_get_file_info = new clsHk_get_file_info;
    clsHk_load_legacy_file* pHk_load_legacy_file = new clsHk_load_legacy_file;
    clsHk_process_mouse_input* pHk_process_mouse_input = new clsHk_process_mouse_input;
    clsHk_game_wnd_proc* pHk_game_wnd_proc = new clsHk_game_wnd_proc;
    clsHk_lua_newstate* pHk_lua_newstate = new clsHk_lua_newstate;
    clsHk_lua_closestate* pHk_lua_closestate = new clsHk_lua_closestate;
    clsHk_lua_pcall* pHk_lua_pcall = new clsHk_lua_pcall;
    clsHk_lua_getglobal* pHk_lua_getglobal = new clsHk_lua_getglobal;
    clsHk_lua_setglobal* pHk_lua_setglobal = new clsHk_lua_setglobal;
    clsHk_lua_pushcclosure* pHk_lua_pushcclosure = new clsHk_lua_pushcclosure;
    clsHk_lua_tolstring* pHk_lua_tolstring = new clsHk_lua_tolstring;
    clsHk_lual_tolstring* pHk_lual_tolstring = new clsHk_lual_tolstring;
    clsHk_lua_loadfileex* pHk_lua_loadfileex = new clsHk_lua_loadfileex;
    clsHk_lua_pushstring* pHk_lua_pushstring = new clsHk_lua_pushstring;
    clsHk_lua_load* pHk_lua_load = new clsHk_lua_load;

    HookMgr();
    ~HookMgr();

    void AddHook(std::string name, std::unique_ptr<PLH::x64Detour> pDetour);
    void RemoveHook(std::string name);

    void UnHookAll();

    std::map<std::string, std::string> fInfo_map = {};

private:
    // hook name - detour
    std::map<std::string, std::unique_ptr<PLH::x64Detour>> all_hooked;
};

extern HookMgr g_HookMgr;