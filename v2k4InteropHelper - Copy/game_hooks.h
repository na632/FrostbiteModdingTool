#pragma once
#include "sdk.h"
#include "lua_libs.h"
#include "globals.h"


#define SAFE_UNHOOK(p)      { if (p) { (p)->unHook();    (p)=nullptr;  }	}
#define BOOL_STR(b) ((b)?"true":"false")

namespace hooks {
    namespace player_heads_manager_builder {
        inline uint64_t tramp = NULL;
        typedef unsigned int(__fastcall* orgFunc)(__int64 unk1, unsigned int playerid, __int64 unk2, __int64 unk3);
        inline orgFunc addr_orgFunc;

        inline unsigned int __fastcall detouredFunc(__int64 unk1, unsigned int playerid, __int64 unk2, __int64 unk3);
    }

    namespace read_ini {
        inline uint64_t tramp = NULL;
        typedef uint64_t(__fastcall* orgFunc)(__int64 pFile, char* section_and_key, unsigned int default_value);
        inline orgFunc addr_orgFunc;

        inline uint64_t __fastcall detouredFunc(__int64 pFile, char* section_and_key, unsigned int default_value);
    }

    namespace get_file_info {
        inline uint64_t tramp = NULL;
        typedef bool(__fastcall* orgFunc)(__int64 vf, char* path, FileInfo* outFileInfo, unsigned int flags);
        inline orgFunc addr_orgFunc;

        inline bool __fastcall detouredFunc(__int64 vf, char* path, FileInfo* outFileInfo, unsigned int flags);
    }

    namespace load_legacy_file {
        inline uint64_t tramp = NULL;
        typedef bool(__fastcall* orgFunc)(FileInfo* fInfo, int always_0, __int64 bufsize, char* buf);
        inline orgFunc addr_orgFunc;

        inline bool __fastcall detouredFunc(FileInfo* fInfo, int always_0, __int64 bufsize, char* buf);
    }

    namespace process_mouse_input {
        inline uint64_t tramp = NULL;
        typedef bool(__fastcall* orgFunc)(__int32 a1);
        inline orgFunc addr_orgFunc;
        bool __fastcall detouredFunc(__int32 a1);
    }

    namespace game_wnd_proc {
        inline uint64_t tramp = NULL;
        typedef void(__fastcall* orgFunc)(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
        inline orgFunc addr_orgFunc;

        inline void __fastcall detouredFunc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
    }

    namespace game_lua_newstate {
        inline uint64_t tramp = NULL;
        typedef lua_State*(__fastcall* orgFunc)(void* f, void* ud);
        inline orgFunc addr_orgFunc;

        inline lua_State* __fastcall detouredFunc(void* f, void* ud);
    }

    namespace game_lua_closestate {
        inline uint64_t tramp = NULL;
        typedef void(__fastcall* orgFunc)(lua_State* L);
        inline orgFunc addr_orgFunc;
        void __fastcall detouredFunc(lua_State* L);
    }

    namespace game_lua_pcall {
        inline uint64_t tramp = NULL;
        typedef int(__fastcall* orgFunc)(lua_State* L, int nargs, int nresults, int errfunc);
        inline orgFunc addr_orgFunc;
        int __fastcall detouredFunc(lua_State* L, int nargs, int nresults, int errfunc);
    }

    namespace game_lua_getglobal {
        inline uint64_t tramp = NULL;
        typedef int(__fastcall* orgFunc)(lua_State* L, const char* name);
        inline orgFunc addr_orgFunc;
        int __fastcall detouredFunc(lua_State* L, const char* name);
    }

    namespace game_lua_setglobal {
        inline uint64_t tramp = NULL;
        typedef void(__fastcall* orgFunc)(lua_State* L, const char* name);
        inline orgFunc addr_orgFunc;
        void __fastcall detouredFunc(lua_State* L, const char* name);
    }

    namespace game_lua_pushcclosure {
        inline uint64_t tramp = NULL;
        typedef void(__fastcall* orgFunc)(lua_State* L, lua_CFunction fn, int n);
        inline orgFunc addr_orgFunc;
        void __fastcall detouredFunc(lua_State* L, lua_CFunction fn, int n);
    }

    namespace game_lua_tolstring {
        inline uint64_t tramp = NULL;
        typedef const char* (__fastcall* orgFunc)(lua_State* L, int idx, size_t* len);
        inline orgFunc addr_orgFunc;
        const char* __fastcall detouredFunc(lua_State* L, int idx, size_t* len);
    }

    namespace game_lual_tolstring {
        inline uint64_t tramp = NULL;
        typedef const char* (__fastcall* orgFunc)(lua_State* L, int idx, size_t* len);
        inline orgFunc addr_orgFunc;
        const char* __fastcall detouredFunc(lua_State* L, int idx, size_t* len);
    }

    namespace game_lua_loadfileex {
        inline uint64_t tramp = NULL;
        typedef int(__fastcall* orgFunc)(lua_State* L, const char* filename, const char* mode);
        inline orgFunc addr_orgFunc;
        int __fastcall detouredFunc(lua_State* L, const char* filename, const char* mode);
    }

    namespace game_lua_pushstring {
        inline uint64_t tramp = NULL;
        typedef const char* (__fastcall* orgFunc)(lua_State* L, const char* s);
        inline orgFunc addr_orgFunc;
        const char* __fastcall detouredFunc(lua_State* L, const char* s);
    }

    namespace game_lua_load {
        inline uint64_t tramp = NULL;
        typedef int(__fastcall* orgFunc)(lua_State* L, lua_Reader reader, void* data, const char* chunkname, const char* mode);
        inline orgFunc addr_orgFunc;
        inline int __fastcall detouredFunc(lua_State* L, lua_Reader reader, void* data, const char* chunkname, const char* mode);
    }
}

void CreateDXHook();
void HookIGO();
void installCrucialHooks();
void installGameHooks();


