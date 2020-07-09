#pragma once
#include <map>
#include <string>

inline std::map<std::string, std::string> g_AOBs = {
        { "AOB_screenID", "4C 0F 45 3D ?? ?? ?? ?? 48 8B FE"},
        { "AOB_pGlobal", "48 8B 05 ?? ?? ?? ?? BA 91 D4 6D 0D 48 8D 0D ?? ?? ?? ?? FF 50 30"}, //ResolveOffset at +3
        { "AOB_pScriptFunctions", "48 8b 43 20 49 89 ee 48 8b 08 48 83 c1 20 48 3b 4b 10 4c 0f 42 f1 48 89 d9 4c 89 f2"}, //ResolveOffset at +61


        // LUA Wrappers for C functions from Global Engine array (Career mode only)
        // ResolveOffset at +3 - number of functions?
        // ResolveOffset at +10 - first item
        { "AOB_LUAEngineFuncReg", "48 63 15 ?? ?? ?? ?? 4C 8D 05 ?? ?? ?? ?? 48 8D 05 ?? ?? ?? ?? 48 8D 0C 52 FF C2 49 89 04 C8 48 8D 05 ?? ?? ?? ?? 41 C7 44 C8 08 FF FF FF FF"}, //ResolveOffset at +3


        // Functions
        { "AOB_fnGetGlobalClass", "48 89 5c 24 08 48 89 74 24 10 57 48 83 ec 20 44 0F B6 01"},
        { "AOB_fnProcessMouseInput", "48 89 5C 24 10 57 48 83 EC 60 ?? ?? ?? ?? ?? ?? ?? 48 33 C4 48 89 44 24 50 8D B9 18 FC FF FF"}, // 
        { "AOB_fnFIFA_WNDPROC", "40 55 53 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 E1 48 81 EC C8 00 00 00 48 C7 45 A7 FE FF FF FF"}, // 
        //{ "AOB_fnFIFA_WNDPROC", "40 55 53 56 57 41 54 41 55 41 56 41 57 48 8d ac 24 c8 fe ff ff 48 81 ec 38 02 00 00 48 c7 45 f0 fe ff ff ff"}, // 
        { "AOB_fnIGO_WNDPROC", "4C 89 4C 24 20 4C 89 44 24 18 48 89 54 24 10 48 89 4C 24 08 57 B8 A0 10 00 00"}, // 
        { "AOB_fnFRAPS64_Present", "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 20 41 8B F8 8B EA 48 8B D9"}, // 
        { "AOB_fnPlayerHeadsManagerBuilder", "8B FA BB FF FF FF FF 81 FA C0 45 04 00"}, // - 38
        { "AOB_fnGetTeamName", "40 57 48 83 EC 60 48 C7 44 24 30 FE FF FF FF 48 89 5C 24 70 8B"},
        { "AOB_fnGetPlayerName", "40 57 48 83 EC 40 48 c7 44 24 30 fe ff ff ff 48 89 5c 24 58 48 89 6c 24 60 48 89 74 24 68 8b ea 48 8b f9 39 91 cc 0c 00 00"},

        // DIRECTX_SELECT
        // ptrFIFASetup - ResolveOffset at +17
        // fnReadIni - ResolveOffset at +22
        // ptrLocaleIni - ResolveOffset at +39
        { "AOB_IniStuff", "48 89 45 DF 45 33 C0 48 8D 15 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 8B C0 48 8D 15 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8"},
        
        // From GameFrameWork::FileSystem::LoadFile('%s')
        { "AOB_fnVFSGetClass", "80 38 00 75 F0 E8 ?? ?? ?? ?? 45 33 C9 4C 8D 44 24 30 48 8D 54 24 60 48 8B C8"}, //ResolveOffset at +6
        { "AOB_fnGetFileInfo", "4C 8D 44 24 30 48 8D 54 24 60 48 8B C8 E8 ?? ?? ?? ?? 84 C0 75 07"}, //ResolveOffset at +14
        { "AOB_fnLoadFile", "48 8D 4C 24 30 4C 8B C3 33 D2 E8 ?? ?? ?? ?? 84 C0 75 13"}, //ResolveOffset at +11

        // LUA API
        { "AOB_fnLua_pcallk", "40 53 55 56 57 48 83 EC 38 33 F6"},
        { "AOB_fnLua_pushstring", "40 53 48 83 EC 30 4C 8B D2 4C"},
        { "AOB_fnLua_load", "48 89 5c 24 08 48 89 74 24 10 48 89 7c 24 18 55 41 54 41 55 41 56 41 57 48 8d 6c 24 d1 48 81 ec b0 00 00 00 0f b7 b9 c4 00 00 00"},
        { "AOB_fnLua_loadfileex", "40 55 53 41 54 41 56 41 57 48 8D AC 24 A0"},
        { "AOB_fnLua_tolstring", "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 20 49 8B D8 8B EA 48 8B F1"},
        { "AOB_fnLuaL_tolstring", "48 89 5C 24 08 48 89 6C 24 18 48 89 74 24 20 57 48 83 EC 20 8D"},
        { "AOB_fnLua_newstate", "40 55 56 48 83 EC 58 48 8B 05 ?? ?? ?? ?? 48 31 E0 48 89 44 24 48 48"},
        { "AOB_fnClose_state", "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 48 89 7C 24 20 41 56 48 83 EC 20 48 8B 51 38 48 8B E9 4C 8B 71 18"},
        


        // Database
        { "AOB_gDB", "4C 0F 44 35 ?? ?? ?? ?? 41 8B 4E 08"} //ResolveOffset at +4

};


//{ Game: FIFA20.exe
//Version :
//Date: 2020 - 01 - 29
//Author : Ja
//AOB_fnProcessMouseInput
//This script does blah blah blah
//}
//
//[ENABLE]
////code from here to '[DISABLE]' will be used to enable the cheat
//
//
//
//aobscanmodule(INJECT, FIFA20.exe, CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC 48 89 5C 24 10 57 48 83 EC 60 48 8B 05) // should be unique
//alloc(newmem, $1000, "FIFA20.exe" + C97C90)
//
//label(code)
//label(return)
//
//newmem:
//
//code:
//mov[rsp + 10], rbx
//jmp return
//
//INJECT + 10 :
//    jmp newmem
//    return :
//    registersymbol(INJECT)
//
//    [DISABLE]
////code from here till the end of the code will be used to disable the cheat
//INJECT + 10 :
//    db 48 89 5C 24 10
//
//    unregistersymbol(INJECT)
//    dealloc(newmem)
//
//{
//https://i.imgur.com/auD6tMw.png
//// ORIGINAL CODE - INJECTION POINT: "FIFA20.exe"+C97C90
//
//    "FIFA20.exe" + C97C86 : CC - int 3
//        "FIFA20.exe" + C97C87 : CC - int 3
//        "FIFA20.exe" + C97C88 : CC - int 3
//        "FIFA20.exe" + C97C89 : CC - int 3
//        "FIFA20.exe" + C97C8A : CC - int 3
//        "FIFA20.exe" + C97C8B : CC - int 3
//        "FIFA20.exe" + C97C8C : CC - int 3
//        "FIFA20.exe" + C97C8D : CC - int 3
//        "FIFA20.exe" + C97C8E : CC - int 3
//        "FIFA20.exe" + C97C8F : CC - int 3
//        // ---------- INJECTING HERE ----------
//        "FIFA20.exe" + C97C90 : 48 89 5C 24 10 - mov[rsp + 10], rbx
//        // ---------- DONE INJECTING  ----------
//        "FIFA20.exe" + C97C95 : 57 - push rdi
//        "FIFA20.exe" + C97C96 : 48 83 EC 60 - sub rsp, 60
//        "FIFA20.exe" + C97C9A : 48 8B 05 6F 63 B4 05 - mov rax, [FIFA20.exe + 67DE010]
//        "FIFA20.exe" + C97CA1 : 48 33 C4 - xor rax, rsp
//        "FIFA20.exe" + C97CA4 : 48 89 44 24 50 - mov[rsp + 50], rax
//        "FIFA20.exe" + C97CA9 : 8D B9 18 FC FF FF - lea edi, [rcx - 000003E8]
//        "FIFA20.exe" + C97CAF : 8B D9 - mov ebx, ecx
//        "FIFA20.exe" + C97CB1 : 83 FF 05 - cmp edi, 05
//        "FIFA20.exe" + C97CB4 : 73 7A - jae FIFA20.exe + C97D30
//        "FIFA20.exe" + C97CB6 : B9 17 00 00 00 - mov ecx, 00000017
//
//}