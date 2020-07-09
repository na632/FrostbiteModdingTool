#pragma once

#include "hook_manager.h"

HookMgr::HookMgr()
{
}

HookMgr::~HookMgr()
{
    delete pHk_player_heads_manager_builder;
    delete pHk_read_ini;
    delete pHk_get_file_info;
    delete pHk_load_legacy_file;
    delete pHk_process_mouse_input;
    delete pHk_game_wnd_proc;
    delete pHk_lua_newstate;
    delete pHk_lua_closestate;
    delete pHk_lua_pcall;
    delete pHk_lua_getglobal;
    delete pHk_lua_setglobal;
    delete pHk_lua_pushcclosure;
    delete pHk_lua_tolstring;
    delete pHk_lual_tolstring;
    delete pHk_lua_loadfileex;
    delete pHk_lua_pushstring;
    delete pHk_lua_load;
}

void HookMgr::AddHook(std::string name, std::unique_ptr<PLH::x64Detour> pDetour) {
    if (all_hooked.count(name) == 1) {
        logger.Write(LOG_ERROR, "AddHook Error - %s already hooked", name.c_str());
    }

    bool hooked = pDetour->hook();
    if (!hooked) {
        logger.Write(LOG_ERROR, "AddHook Error - PLH failed %s", name.c_str());
        return;
    }

    all_hooked[name] = std::move(pDetour);
}

void HookMgr::RemoveHook(std::string name) {
    logger.Write(LOG_DEBUG, "RemoveHook: %s", name.c_str());
    if (all_hooked.count(name) == 1) {
        all_hooked[name]->unHook();
        all_hooked.erase(name);
    }
    else {
        logger.Write(LOG_ERROR, "RemoveHook: key not found");
    }
}

void HookMgr::UnHookAll() {
    std::vector<std::string> keys;
    for (auto const& [key, val] : all_hooked) {
        keys.push_back(key);
    }

    for (auto const& key : keys) {
        RemoveHook(key);
    }

    all_hooked.clear();
};

HookMgr g_HookMgr;
