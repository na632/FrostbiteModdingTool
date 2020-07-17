#pragma once
#include "engine.h"
#include "game_hooks.h"
#include "hook_manager.h"
#include "misc.h"
//#include "lua_api.h"
//#include "renderer/gui.h"


int __cdecl fn_stdio_common_vsprintf(unsigned __int64 _Options, char* _Buffer, size_t _BufferCount, char const* _Format, _locale_t _Locale, va_list _ArgList)
{
    int result = PLH::FnCast(hkStdioPrinfTramp, __stdio_common_vsprintf)(_Options, _Buffer, _BufferCount, _Format, _Locale, _ArgList);
    if (logger.logGameLogs) {
        logger.WriteHooked(LOG_INFO, std::string(_Buffer));
    }

    return result;
}

namespace hooks {
    namespace player_heads_manager_builder {
        inline unsigned int __fastcall detouredFunc(__int64 unk1, unsigned int playerid, __int64 unk2, __int64 unk3) {
            auto pHk = g_HookMgr.pHk_player_heads_manager_builder;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(unk1, playerid, unk2, unk3);
                }
            }

            if (!playerid) {
                return PLH::FnCast(tramp, addr_orgFunc)(unk1, playerid, unk2, unk3);
            }

            std::string fPath = g_engine.ModsRootLegacyPath + "\\data\\ui\\imgAssets\\heads\\p" + std::to_string(playerid) + ".dds";
            if (fs::exists(fPath)) {
                return 0xffffffff;
            }


            return PLH::FnCast(tramp, addr_orgFunc)(unk1, playerid, unk2, unk3);
        };
    }

    namespace read_ini {
        inline uint64_t __fastcall detouredFunc(__int64 pFile, char* section_and_key, unsigned int default_value) {
            auto pHk = g_HookMgr.pHk_read_ini;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(pFile, section_and_key, default_value);
                }
            }

            uint64_t result = PLH::FnCast(tramp, addr_orgFunc)(pFile, section_and_key, default_value);
            if (logger.logIniRead) {
                logger.Write(LOG_INFO, "IniRead(pFile=0x%08llX, section_and_key=%s, default_value=%d) = %llu",
                    pFile, section_and_key, default_value, result
                );
            }

            return result;
        };
    }

    namespace get_file_info {
        inline bool __fastcall detouredFunc(__int64 vf, char* path, FileInfo* outFileInfo, unsigned int flags) {
            auto pHk = g_HookMgr.pHk_get_file_info;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(vf, path, outFileInfo, flags);
                }
            }

            bool result = PLH::FnCast(tramp, addr_orgFunc)(vf, path, outFileInfo, flags);

            std::stringstream ss;
            ss << outFileInfo->unk1 << "-" << outFileInfo->unk2 << "-" << outFileInfo->unk3 << "-" << outFileInfo->unk4;
            std::string uid = ss.str();

            std::string fPath = ReplaceAll(g_engine.ModsRootLegacyPath + "\\" + std::string(path), "/", "\\");
            g_HookMgr.fInfo_map[uid] = fPath;

            bool is_modded = false;
            if (fs::exists(fPath)) {
                outFileInfo->fileSize = fs::file_size(fPath);
                result = true;
                is_modded = true;
            }

            if (logger.logGetFileInfo) {
                logger.Write(LOG_INFO, "GetFileInfo(path=%s, flags=%d) = %s (is_modded = %s)",
                    path, flags, BOOL_STR(result), BOOL_STR(is_modded)
                );
            }

            return result;
        };
    }

    namespace load_legacy_file {
        inline bool __fastcall detouredFunc(FileInfo* fInfo, int always_0, __int64 bufsize, char* buf) {
            auto pHk = g_HookMgr.pHk_load_legacy_file;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(fInfo, always_0, bufsize, buf);
                }
            }

            std::stringstream ss;
            ss << fInfo->unk1 << "-" << fInfo->unk2 << "-" << fInfo->unk3 << "-" << fInfo->unk4;
            std::string uid = ss.str();

            bool is_modded = false;
            if (g_HookMgr.fInfo_map.count(uid) > 0) {
                //logger.Write(LOG_INFO, "hookLoadLegacyFile - %s, UID - %s", g_HookMgr.fInfo_map.at(uid).c_str(), uid.c_str());

                std::string fPath = g_HookMgr.fInfo_map.at(uid);
                is_modded = fs::exists(fPath);

                //logger.Write(LOG_INFO, "hookLoadLegacyFile - Find %s", fPath.c_str());
                if (is_modded) {
                    FILE* f = fopen(fPath.c_str(), "rb");
                    fseek(f, 0, SEEK_END);
                    long fsize = ftell(f);
                    fseek(f, 0, SEEK_SET);
                    fread(buf, fsize, 1, f);
                    fclose(f);
                }
                if (logger.logLoadFile) {
                    logger.Write(LOG_INFO, "LoadFile(path=%s) (is_modded = %s)",
                        fPath.c_str(), BOOL_STR(is_modded)
                    );
                }

                if (is_modded)
                    return bool(1);
            }
            else {
                if (logger.logLoadFile) {
                    logger.Write(LOG_INFO, "LoadFile - unknown");
                }
            }




            //logger.Write(LOG_INFO, "hookLoadLegacyFile - FileInfo - 0x%08llX", &fInfo);
            //logger.Write(LOG_INFO, "hookLoadLegacyFile - FileInfo - 0x%08llX", fInfo);
            //logger.Write(LOG_INFO, "hookLoadLegacyFile - FileInfo - 0x%08llX", *fInfo);
            //logger.Write(LOG_INFO, "hookLoadLegacyFile - FileName - %s", g_HookMgr.fInfo_map[__int64(fInfo)]);

            //if (fInfo->fileSize == 32536) {
            //    FILE* f = fopen("p1025.DDS", "rb");
            //    fseek(f, 0, SEEK_END);
            //    long fsize = ftell(f);
            //    fseek(f, 0, SEEK_SET);
            //    fread(buf, fsize, 1, f);
            //    fclose(f);
            //    return bool(1);
            //}

            return PLH::FnCast(tramp, addr_orgFunc)(fInfo, always_0, bufsize, buf);
        };
    }

    namespace process_mouse_input {
        bool __fastcall detouredFunc(__int32 a1)
        {
            auto pHk = g_HookMgr.pHk_process_mouse_input;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(a1);
                }
            }

           /* if (!g_GUI.windows.show_main_window) {
                return PLH::FnCast(tramp, addr_orgFunc)(a1);
            }*/
            return false;
        }
    }

    namespace game_wnd_proc {
        inline void __fastcall detouredFunc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
            auto pHk = g_HookMgr.pHk_game_wnd_proc;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(hwnd, uMsg, wParam, lParam);
                }
            }

            //logger.Write(
            //    LOG_INFO, 
            //    "hookGameWndProc - uMsg - 0x%08llX, wParam - 0x%08llX, lParam - 0x%08llX", 
            //    uMsg, wParam, lParam
            //);
            //if (!g_dtr_IGOWngProc) {
            //    d3dhook_common::WndProc(hwnd, uMsg, wParam, lParam);
            //    //return;
            //}

            //d3dhook_common::WndProc(hwnd, uMsg, wParam, lParam);
            //// Don't process keyboard input ingame when menu is shown
            //if (g_GUI.windows.show_main_window) {
            //    switch (uMsg) {
            //    case WM_LBUTTONDOWN: case WM_LBUTTONDBLCLK:
            //    case WM_RBUTTONDOWN: case WM_RBUTTONDBLCLK:
            //    case WM_MBUTTONDOWN: case WM_MBUTTONDBLCLK:
            //    case WM_XBUTTONDOWN: case WM_XBUTTONDBLCLK:
            //    case WM_NCLBUTTONDOWN: case WM_NCLBUTTONDBLCLK:
            //    case WM_NCRBUTTONDOWN: case WM_NCRBUTTONDBLCLK:
            //    case WM_NCMBUTTONDOWN: case WM_NCMBUTTONDBLCLK:
            //    case WM_NCXBUTTONDOWN: case WM_NCXBUTTONDBLCLK:
            //    case WM_LBUTTONUP:
            //    case WM_RBUTTONUP:
            //    case WM_MBUTTONUP:
            //    case WM_XBUTTONUP:
            //    case WM_NCLBUTTONUP:
            //    case WM_NCRBUTTONUP:
            //    case WM_NCMBUTTONUP:
            //    case WM_NCXBUTTONUP:
            //        return PLH::FnCast(tramp, addr_orgFunc)(hwnd, uMsg, wParam, lParam);
            //    }
            //    return;
            //}
            return PLH::FnCast(tramp, addr_orgFunc)(hwnd, uMsg, wParam, lParam);
        }
    }

    namespace game_lua_newstate {
        inline lua_State* __fastcall detouredFunc(void* f, void* ud) {
            auto pHk = g_HookMgr.pHk_lua_newstate;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(f, ud);
                }
            }

            lua_State* L = PLH::FnCast(tramp, addr_orgFunc)(f, ud);
            //g_LRunner.add_game_lua_state(L);
            return L;
        };
    }

    namespace game_lua_closestate {
        void __fastcall detouredFunc(lua_State* L)
        {
            auto pHk = g_HookMgr.pHk_lua_closestate;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(L);
                }
            }
            //g_LRunner.remove_game_lua_state(L);
            return PLH::FnCast(tramp, addr_orgFunc)(L);
        }
    }

    namespace game_lua_pcall {
        int __fastcall detouredFunc(lua_State* L, int nargs, int nresults, int errfunc)
        {
            auto pHk = g_HookMgr.pHk_lua_pcall;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(L, nargs, nresults, errfunc);
                }
            }
            //g_LRunner.add_game_lua_state(L);

            return PLH::FnCast(tramp, addr_orgFunc)(L, nargs, nresults, errfunc);
        }
    }

    namespace game_lua_getglobal {
        int __fastcall detouredFunc(lua_State* L, const char* name)
        {
            auto pHk = g_HookMgr.pHk_lua_getglobal;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(L, name);
                }
            }
            //logger.Write(LOG_DEBUG, "[LUA] GetGlobal: %s", name);

            return PLH::FnCast(tramp, addr_orgFunc)(L, name);
        }
    }

    namespace game_lua_setglobal {
        void __fastcall detouredFunc(lua_State* L, const char* name)
        {
            auto pHk = g_HookMgr.pHk_lua_setglobal;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(L, name);
                }
            }
            //logger.Write(LOG_DEBUG, "[LUA] SetGlobal: %s", name);

            return PLH::FnCast(tramp, addr_orgFunc)(L, name);
        }
    }

    namespace game_lua_pushcclosure {
        void __fastcall detouredFunc(lua_State* L, lua_CFunction fn, int n)
        {
            auto pHk = g_HookMgr.pHk_lua_pushcclosure;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(L, fn, n);
                }
            }
            //logger.Write(LOG_DEBUG, "[LUA] PushC Func: 0x%08llX (0x%08llX)", fn, &fn);

            return PLH::FnCast(tramp, addr_orgFunc)(L, fn, n);
        }
    }

    namespace game_lua_tolstring {
        const char* __fastcall detouredFunc(lua_State* L, int idx, size_t* len)
        {
            auto pHk = g_HookMgr.pHk_lua_tolstring;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(L, idx, len);
                }
            }

            return PLH::FnCast(tramp, addr_orgFunc)(L, idx, len);
        }
    }

    namespace game_lual_tolstring {
        const char* __fastcall detouredFunc(lua_State* L, int idx, size_t* len)
        {
            auto pHk = g_HookMgr.pHk_lual_tolstring;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(L, idx, len);
                }
            }

            if (!logger.logluaL_tolstring) {
                return PLH::FnCast(tramp, addr_orgFunc)(L, idx, len);
            }

            const char* result = PLH::FnCast(tramp, addr_orgFunc)(L, idx, len);
            if (strlen(result) > 20480) {
                logger.Write(LOG_DEBUG, "[LUA API] luaL_tolstring (L: 0x%08llX): too long... %d", L, strlen(result));
            }
            else {
                logger.Write(LOG_DEBUG, "[LUA API] luaL_tolstring (L: 0x%08llX): %s", L, result);
            }


            return result;

            //return PLH::FnCast(tramp, addr_orgFunc)(L, idx, len);
        }
    }

    namespace game_lua_loadfileex {
        int __fastcall detouredFunc(lua_State* L, const char* filename, const char* mode)
        {
            auto pHk = g_HookMgr.pHk_lua_loadfileex;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(L, filename, mode);
                }
            }

            logger.Write(LOG_INFO, "[LUA API] Lua LoadFileX: %s", filename);

            return PLH::FnCast(tramp, addr_orgFunc)(L, filename, mode);
        }
    }

    namespace game_lua_pushstring {
        const char* __fastcall detouredFunc(lua_State* L, const char* s)
        {
            auto pHk = g_HookMgr.pHk_lua_pushstring;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(L, s);
                }
            }

            //if (strlen(s) <= 6000) {
            //    logger.Write(LOG_INFO, "[LUA API] Lua Push String: %s (L: 0x%08llX)", s, L);
            //}
            //else {
            //    logger.Write(LOG_INFO, "[LUA API] Lua Push StringLen: %d (L: 0x%08llX)", strlen(s), L);
            //}

            return PLH::FnCast(tramp, addr_orgFunc)(L, s);
        }
    }

    namespace game_lua_load {
        inline int __fastcall detouredFunc(lua_State* L, lua_Reader reader, void* data, const char* chunkname, const char* mode) {
            auto pHk = g_HookMgr.pHk_lua_load;
            if (pHk) {
                if (!pHk->isEnabled) {
                    return PLH::FnCast(tramp, addr_orgFunc)(L, reader, data, chunkname, mode);
                }
                //g_LRunner.add_game_lua_state(L);
                logger.Write(LOG_DEBUG, "[LUA API] Lua Load (L: 0x%08llX)", L);
            }
            return PLH::FnCast(tramp, addr_orgFunc)(L, reader, data, chunkname, mode);
        };
    }
}

void CreateDXHook() {
    //logger.Write(LOG_INFO, "Hooking DirectX...");
    //HMODULE hDXGIDLL = 0;
    //do
    //{
    //    hDXGIDLL = GetModuleHandle("dxgi.dll");
    //    Sleep(100);
    //} while (!hDXGIDLL);
    //logger.Write(LOG_INFO, "dxgi.dll - found");

    //__int64* ptrFIFASetup = (__int64*)g_ctx_proc.getAddr("AOB_IniStuff", true, 17);
    //__int64* ptrLocaleIni = (__int64*)g_ctx_proc.getAddr("AOB_IniStuff", true, 39);

    ////logger.Write(LOG_INFO,
    ////    "ptrFIFASetup: 0x%08llX, ptrLocaleIni: 0x%08llX",
    ////    *ptrFIFASetup,
    ////    *ptrLocaleIni
    ////);

    //// First read DIRECTX_SELECT from: Documents\FIFA 20\fifasetup.ini
    //unsigned int DX_Select = (unsigned int)hooks::read_ini::detouredFunc(*ptrFIFASetup, "DIRECTX_SELECT", 1);

    //// Next read DIRECTX_SELECT from: <ORIGIN_GAMES_DIRECTORY>\FIFA 20\Data\locale.ini (I:\Gry\Origin\FIFA 20\Data)
    //DX_Select = (unsigned int)hooks::read_ini::detouredFunc(*ptrLocaleIni, "DIRECTX_SELECT", DX_Select);
    //if (DX_Select == 1) {
    //    d3d11hook::SetupDirectXHooks();
    //    if (!g_bPresent11Initialised) {
    //        d3d12hook::SetupDirectXHooks();
    //    }
    //}
    //else {
    //    d3d12hook::SetupDirectXHooks();
    //    if (!g_bPresent12Initialised) {
    //        d3d11hook::SetupDirectXHooks();
    //    }
    //}

    //if (!g_bPresent12Initialised && !g_bPresent11Initialised) {
    //    logger.Write(LOG_FATAL, "DirectX not hooked");
    //}
}

void HookIGO() {
    /*HMODULE hIGODLL = GetModuleHandle("igo64.dll");

    if (!hIGODLL) {
        logger.Write(LOG_ERROR, "igo64.dll - not found");
        return;
    }

    core::Context IGODLL;
    IGODLL.Update(hIGODLL);

    uintptr_t addr = 0;
    addr = IGODLL.getAddr("AOB_fnIGO_WNDPROC");
    if (addr) {
        d3dhook_common::hkIGOWndProc = (d3dhook_common::_hkIGOWndProc)(addr);
        g_dtr_IGOWngProc = std::make_unique<PLH::x64Detour>((char*)d3dhook_common::hkIGOWndProc, (char*)d3dhook_common::fnIGOWndProc, &hookIGOWngProcTramp, dis);
        g_HookMgr.AddHook("IGO_WndProc", std::move(g_dtr_IGOWngProc));
        logger.Write(LOG_INFO, "igo hwndproc hooked");
    }*/
}

void installCrucialHooks() {
    logger.Write(LOG_DEBUG, "installCrucialHooks");

    uintptr_t addr = 0;

    addr = g_ctx_proc.getAddr("AOB_fnGetFileInfo", true, 14);
    if (addr) {
        hooks::get_file_info::addr_orgFunc = (hooks::get_file_info::orgFunc)addr;
        g_HookMgr.pHk_get_file_info->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::get_file_info::addr_orgFunc,
            (char*)hooks::get_file_info::detouredFunc,
            &hooks::get_file_info::tramp,
            dis
            );
        g_HookMgr.AddHook("GetFileInfo", std::move(g_HookMgr.pHk_get_file_info->detour));
    }
    logger.Write(LOG_INFO, "GetFileInfo::Done");

    addr = g_ctx_proc.getAddr("AOB_fnLoadFile", true, 11);
    if (addr) {
        hooks::load_legacy_file::addr_orgFunc = (hooks::load_legacy_file::orgFunc)addr;
        g_HookMgr.pHk_load_legacy_file->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::load_legacy_file::addr_orgFunc,
            (char*)hooks::load_legacy_file::detouredFunc,
            &hooks::load_legacy_file::tramp,
            dis
            );
        g_HookMgr.AddHook("LoadFile", std::move(g_HookMgr.pHk_load_legacy_file->detour));
    }
    logger.Write(LOG_INFO, "LoadFile::Done");


    addr = g_ctx_proc.getAddr("AOB_IniStuff", true, 22);
    if (addr) {
        hooks::read_ini::addr_orgFunc = (hooks::read_ini::orgFunc)addr;
        g_HookMgr.pHk_read_ini->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::read_ini::addr_orgFunc,
            (char*)hooks::read_ini::detouredFunc,
            &hooks::read_ini::tramp,
            dis
            );
        g_HookMgr.AddHook("IniStuff", std::move(g_HookMgr.pHk_read_ini->detour));
    }
    logger.Write(LOG_INFO, "IniStuff::Done");

    // LUA
    // 140D01050 - lua_pcallk
    // 140D01480 - lua_load
    // 14B375440 - luaL_loadfilex
    // 14726CA48 - rna cache

    addr = g_ctx_proc.getAddr("AOB_fnLua_newstate");
    if (addr) {
        hooks::game_lua_newstate::addr_orgFunc = (hooks::game_lua_newstate::orgFunc)addr;
        g_HookMgr.pHk_lua_newstate->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lua_newstate::addr_orgFunc,
            (char*)hooks::game_lua_newstate::detouredFunc,
            &hooks::game_lua_newstate::tramp,
            dis
            );
        g_HookMgr.AddHook("game_lua_newstate", std::move(g_HookMgr.pHk_lua_newstate->detour));
    }
    logger.Write(LOG_INFO, "game_lua_newstate::Done");

    addr = g_ctx_proc.getAddr("AOB_fnClose_state");
    if (addr) {
        hooks::game_lua_closestate::addr_orgFunc = (hooks::game_lua_closestate::orgFunc)addr;
        g_HookMgr.pHk_lua_closestate->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lua_closestate::addr_orgFunc,
            (char*)hooks::game_lua_closestate::detouredFunc,
            &hooks::game_lua_closestate::tramp,
            dis
            );
        g_HookMgr.AddHook("game_lua_closestate", std::move(g_HookMgr.pHk_lua_closestate->detour));
    }
    logger.Write(LOG_INFO, "game_lua_closestate::Done");


    addr = g_ctx_proc.getAddr("AOB_fnLua_pcallk");
    if (addr) {
        hooks::game_lua_pcall::addr_orgFunc = (hooks::game_lua_pcall::orgFunc)addr;
        g_HookMgr.pHk_lua_pcall->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lua_pcall::addr_orgFunc,
            (char*)hooks::game_lua_pcall::detouredFunc,
            &hooks::game_lua_pcall::tramp,
            dis
            );
        g_HookMgr.AddHook("game_lua_pcall", std::move(g_HookMgr.pHk_lua_pcall->detour));
    }
    logger.Write(LOG_INFO, "game_lua_pcall::Done");

    // 48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 20 48 8B 41 18 48 8B F1 4C 8B 40 40 41 83 78 10 01 76 0A 49 8B 58 18 48 83 C3 10 EB 3E
    //addr = 0x140D009E0;
    addr = 0;
    if (addr) {
        hooks::game_lua_getglobal::addr_orgFunc = (hooks::game_lua_getglobal::orgFunc)addr;
        g_HookMgr.pHk_lua_getglobal->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lua_getglobal::addr_orgFunc,
            (char*)hooks::game_lua_getglobal::detouredFunc,
            &hooks::game_lua_getglobal::tramp,
            dis
            );
        g_HookMgr.AddHook("game_lua_getglobal", std::move(g_HookMgr.pHk_lua_getglobal->detour));
    }
    logger.Write(LOG_INFO, "game_lua_getglobal::Done");

    //addr = 0x140D00C60;
    addr = 0;
    if (addr) {
        hooks::game_lua_setglobal::addr_orgFunc = (hooks::game_lua_setglobal::orgFunc)addr;
        g_HookMgr.pHk_lua_setglobal->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lua_setglobal::addr_orgFunc,
            (char*)hooks::game_lua_setglobal::detouredFunc,
            &hooks::game_lua_setglobal::tramp,
            dis
            );
        g_HookMgr.AddHook("game_lua_setglobal", std::move(g_HookMgr.pHk_lua_setglobal->detour));
    }
    logger.Write(LOG_INFO, "game_lua_setglobal::Done");

    //48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 20 49 63 F0 48 8B EA 48 8B F9 45 85 C0 75 23 48 8B 41 10 48 89 10 C7 40 08 16 00 00 00 48 83 41 10 10
    //addr = 0x140D00890;
    addr = 0;
    if (addr) {
        hooks::game_lua_pushcclosure::addr_orgFunc = (hooks::game_lua_pushcclosure::orgFunc)addr;
        g_HookMgr.pHk_lua_pushcclosure->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lua_pushcclosure::addr_orgFunc,
            (char*)hooks::game_lua_pushcclosure::detouredFunc,
            &hooks::game_lua_pushcclosure::tramp,
            dis
            );
        g_HookMgr.AddHook("game_lua_pushcclosure", std::move(g_HookMgr.pHk_lua_pushcclosure->detour));
    }
    logger.Write(LOG_INFO, "game_lua_pushcclosure::Done");


    addr = g_ctx_proc.getAddr("AOB_fnLuaL_tolstring");
    if (addr) {
        hooks::game_lual_tolstring::addr_orgFunc = (hooks::game_lual_tolstring::orgFunc)addr;
        g_HookMgr.pHk_lual_tolstring->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lual_tolstring::addr_orgFunc,
            (char*)hooks::game_lual_tolstring::detouredFunc,
            &hooks::game_lual_tolstring::tramp,
            dis
            );
        g_HookMgr.AddHook("game_luaL_tolstring", std::move(g_HookMgr.pHk_lual_tolstring->detour));
    }
    logger.Write(LOG_INFO, "game_luaL_tolstring::Done");

    addr = g_ctx_proc.getAddr("AOB_fnLua_tolstring");
    if (addr) {
        hooks::game_lua_tolstring::addr_orgFunc = (hooks::game_lua_tolstring::orgFunc)addr;
        g_HookMgr.pHk_lua_tolstring->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lua_tolstring::addr_orgFunc,
            (char*)hooks::game_lua_tolstring::detouredFunc,
            &hooks::game_lua_tolstring::tramp,
            dis
            );
        g_HookMgr.AddHook("game_lua_tolstring", std::move(g_HookMgr.pHk_lua_tolstring->detour));
    }
    logger.Write(LOG_INFO, "game_lua_tolstring::Done");

    addr = g_ctx_proc.getAddr("AOB_fnLua_load");
    if (addr) {
        hooks::game_lua_load::addr_orgFunc = (hooks::game_lua_load::orgFunc)addr;
        g_HookMgr.pHk_lua_load->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lua_load::addr_orgFunc,
            (char*)hooks::game_lua_load::detouredFunc,
            &hooks::game_lua_load::tramp,
            dis
            );
        g_HookMgr.AddHook("game_lua_load", std::move(g_HookMgr.pHk_lua_load->detour));
    }
    logger.Write(LOG_INFO, "game_lua_load::Done");

    addr = g_ctx_proc.getAddr("AOB_fnLua_loadfileex");;
    if (addr) {

        hooks::game_lua_loadfileex::addr_orgFunc = (hooks::game_lua_loadfileex::orgFunc)addr;
        g_HookMgr.pHk_lua_loadfileex->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lua_loadfileex::addr_orgFunc,
            (char*)hooks::game_lua_loadfileex::detouredFunc,
            &hooks::game_lua_loadfileex::tramp,
            dis
            );
        g_HookMgr.AddHook("game_lua_loadfileex", std::move(g_HookMgr.pHk_lua_loadfileex->detour));
    }
    logger.Write(LOG_INFO, "game_lua_loadfileex::Done");

    addr = g_ctx_proc.getAddr("AOB_fnLua_pushstring");;
    if (addr) {
        hooks::game_lua_pushstring::addr_orgFunc = (hooks::game_lua_pushstring::orgFunc)addr;
        g_HookMgr.pHk_lua_pushstring->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_lua_pushstring::addr_orgFunc,
            (char*)hooks::game_lua_pushstring::detouredFunc,
            &hooks::game_lua_pushstring::tramp,
            dis
            );
        g_HookMgr.AddHook("game_lua_pushstring", std::move(g_HookMgr.pHk_lua_pushstring->detour));
    }
    logger.Write(LOG_INFO, "game_lua_pushstring::Done");

}

void installGameHooks() {
    logger.Write(LOG_INFO, "installGameHooks");

    uintptr_t addr = 0;

    addr = g_ctx_proc.getAddr("AOB_fnProcessMouseInput");
    if (addr) {
        hooks::process_mouse_input::addr_orgFunc = (hooks::process_mouse_input::orgFunc)addr;
        g_HookMgr.pHk_process_mouse_input->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::process_mouse_input::addr_orgFunc,
            (char*)hooks::process_mouse_input::detouredFunc,
            &hooks::process_mouse_input::tramp,
            dis
            );
        g_HookMgr.AddHook("process_mouse_input", std::move(g_HookMgr.pHk_process_mouse_input->detour));
    }

    addr = g_ctx_proc.getAddr("AOB_fnFIFA_WNDPROC");
    if (addr) {
        hooks::game_wnd_proc::addr_orgFunc = (hooks::game_wnd_proc::orgFunc)addr;
        g_HookMgr.pHk_game_wnd_proc->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::game_wnd_proc::addr_orgFunc,
            (char*)hooks::game_wnd_proc::detouredFunc,
            &hooks::game_wnd_proc::tramp,
            dis
            );
        g_HookMgr.AddHook("game_wnd_proc", std::move(g_HookMgr.pHk_game_wnd_proc->detour));
    }

    addr = g_ctx_proc.getAddr("AOB_fnPlayerHeadsManagerBuilder");
    if (addr) {
        // Set addr to begin of the function
        addr = addr - 38;

        hooks::player_heads_manager_builder::addr_orgFunc = (hooks::player_heads_manager_builder::orgFunc)addr;
        g_HookMgr.pHk_player_heads_manager_builder->detour = std::make_unique<PLH::x64Detour>(
            (char*)hooks::player_heads_manager_builder::addr_orgFunc,
            (char*)hooks::player_heads_manager_builder::detouredFunc,
            &hooks::player_heads_manager_builder::tramp,
            dis
            );
        g_HookMgr.AddHook("player_heads_manager_builder", std::move(g_HookMgr.pHk_player_heads_manager_builder->detour));
    }

    // In-game logging
    // Hook ucrtbase.dll -> __stdio_common_vsprintf()
    HMODULE hUcrtbaseDLL = GetModuleHandle("ucrtbase.dll");
    if (hUcrtbaseDLL) {
        g_dtr_stdio_vprintf = std::make_unique<PLH::x64Detour>((char*)GetProcAddress(hUcrtbaseDLL, "__stdio_common_vsprintf"), (char*)fn_stdio_common_vsprintf, &hkStdioPrinfTramp, dis);
        g_HookMgr.AddHook("stdio_vprintf", std::move(g_dtr_stdio_vprintf));
    }
}