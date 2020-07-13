// dllmain.cpp : Defines the entry point for the DLL application.
//#include "pch.h"
#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>

#include <string>
#include <cstring>
#include "logger.h"
#include "hook_manager.h"
#include <iostream>
#include <map>
#include <filesystem>
#include <TlHelp32.h>
#include "engine.h"
#include "PIPEInteraction.h"
#include "StringBuilder.h"
#include "v2k4helpers.h"

#pragma warning(disable: 4996)

namespace fs = std::filesystem;
PIPEInteraction* PIPEInteractionInstance;

void SetupLogger() {
    const std::string logPath = g_ctx_dll.GetFolder() + "\\Logs";
    if (!fs::is_directory(logPath) || !fs::exists(logPath)) {
        fs::create_directory(logPath);
    }

    SYSTEMTIME currTimeLog;
    GetLocalTime(&currTimeLog);
    std::ostringstream ssLogFile;
    ssLogFile << "log_" <<
        std::setw(2) << std::setfill('0') << currTimeLog.wDay << "-" <<
        std::setw(2) << std::setfill('0') << currTimeLog.wMonth << "-" <<
        std::setw(4) << std::setfill('0') << currTimeLog.wYear << ".txt";
    const std::string logFile = logPath + "\\" + ssLogFile.str();
    logger.SetFile(logFile);

    // Setup log file for intercepted in game logging
    std::ostringstream ssHookedLogFile;
    ssHookedLogFile << "game_log_" <<
        std::setw(2) << std::setfill('0') << currTimeLog.wDay << "-" <<
        std::setw(2) << std::setfill('0') << currTimeLog.wMonth << "-" <<
        std::setw(4) << std::setfill('0') << currTimeLog.wYear << ".txt";
    const std::string GamelogFile = logPath + "\\" + ssHookedLogFile.str();
    logger.SetHookedFile(GamelogFile);
}

void EjectDLL(HMODULE hModule)
{
    logger.Write(LOG_INFO, "Ejecting...");
    //g_GUI.windows.show_main_window = false;
    Sleep(100);

    //SAFE_RELEASE(pSwapChain);

   /* d3d12hook::DeleteDirectXHooks();
    d3d11hook::DeleteDirectXHooks();*/

    Sleep(1000);
    //(WNDPROC)SetWindowLongPtr(
    //    g_main_window, GWLP_WNDPROC, (LONG_PTR)d3dhook_common::OriginalWndProcHandler
    //);
    //Sleep(1000);

    //g_HookMgr.UnHookAll();

    Sleep(1000);
    logger.Write(LOG_INFO, "Ejected...");

    FreeLibraryAndExitThread(hModule, EXIT_SUCCESS);
};


DWORD WINAPI mainFunc(LPVOID lpModule)
{
    Sleep(200);

    // Attached to
    g_ctx_proc.Update(GetModuleHandle(NULL));

    std::string const module_name = g_ctx_proc.m_ModuleContext.m_Name;
    const char* module_name_c = const_cast<LPSTR>(module_name.c_str());
    unsigned long long mod_base_addr = g_ctx_proc.m_ModuleContext.m_Base;
    unsigned long long mod_size = g_ctx_proc.m_ModuleContext.m_Size;
    unsigned long long mod_end = mod_base_addr + mod_size;

    // Our DLL
    g_ctx_dll.Update(reinterpret_cast<HMODULE>(lpModule));

    //
    g_ctx_ucrtbase_dll.Update(GetModuleHandle("ucrtbase.dll"));

    // Setup all kind of logs
    SetupLogger();
    logger.Write(LOG_INFO, "Injected");

    logger.Write(LOG_INFO,
        "Proc: <%s> BaseAddr: 0x%08llX, EndAddr: 0x%08llX (Size: 0x%08llX)",
        module_name_c,
        mod_base_addr,
        mod_end,
        mod_size
    );

#ifdef _DEBUG
    logger.SetMinLevel((LogLevel)0);
#else
    int minlvl = g_config.ReadInt("/Logger/log_level", 1);
    logger.SetMinLevel((LogLevel)minlvl);
#endif

    // Live Editor Engine
    g_engine.Setup();
    //MessageBox(NULL, "This bitch has done something!", "This bitch has done something!", MB_OK);

    //PIPE Connection Setup
    PIPEInteractionInstance = new PIPEInteraction();
    PIPEInteractionInstance->CreateServerPipe();

    logger.Write(LOG_DEBUG, "Start main loop");
    for (;;) {
        Sleep(1000);

        if (GetAsyncKeyState(VK_DELETE)) {
            Sleep(1000);
            logger.Write(LOG_DEBUG, "Attempting to run script");
            if (g_engine.isInCM()) 
            {
                try {
                    g_engine.LoadDB();
                    g_engine.ReloadDB();
                    // This works but it is slow
                    /*std::vector<FIFADBRow*> rows = g_engine.GetDBTableRows("teamplayerlinks");
                    auto filtered = std::find(rows.begin(), rows.end(), [&](FIFADBRow * o) {
                        return o->row.at("playerid")->value == "190871";
                        });
                    if (filtered != rows.end())
                    {
                        auto item = (*filtered)->row;
                        logger.Write(LOG_DEBUG, "found:: " + item.at("playerid")->value);
                    }*/
                    std::string shortname = g_engine.dbMgr.tables_ordered.at("teamplayerlinks");
                    auto t = reinterpret_cast<SDKHelper_FIFADBTable*>(g_engine.dbMgr.tables.at(shortname));
                    auto row = t->GetSingleRowByField("playerid", "190871");
                    if (row) {
                        auto f = new FIFADBField();
                        //t->AddEditedField(10, f, row->row.begin() )
                    }
                        //t->CreateHeaders();
                    //t->CreateRows();
                    //auto pkey = t->pkey;
                    //auto fsn = t->field_name_shortname.at("teamid");
                    //auto f = t->fields.at(fsn);
                    ////auto row = t->rows..at(0);
                    //auto row = std::find_if(t->rows.begin(), t->rows.end(), [&](FIFADBRow* o) { return o->row.at("playerid")->value == "190871"; });
                    //if (row != t->rows.end())
                    //{
                    //   //(*row).
                    //}
                    //auto row = t->GetRowForPkey("playerid");
                }
                catch(const std::exception&) { /* */ 

                }

                //t->AddEditedField(10, f, f);

                //StringBuilder sb;// = new StringBuilder();
                //for(auto i = 0; i < 200000; i++) {
                //    sb.append("ForceCPUPlayerOntoTransferList(" + std::to_string(i) + ");");
                //    sb.append("ForceUserPlayerOntoTransferList(" + std::to_string(i) + ");");
                //}

                //for (auto j = 0; j < 20; j++) {
                //    sb.append("UpdateTeamBudget(" + std::to_string(j) + ", 1);");
                //}
                /*g_engine.RunFIFAScript("ForceCPUPlayerOntoTransferList(190870);");
                g_engine.RunFIFAScript("ForceCPUPlayerOntoTransferList(190871);");
                g_engine.RunFIFAScript("ForceCPUPlayerOntoTransferList(190872);");
                g_engine.RunFIFAScript("ForceCPUPlayerOntoTransferList(190879);");
                g_engine.RunFIFAScript("ForceCPUPlayerOntoTransferList(193079);");
                g_engine.RunFIFAScript("ForceCPUPlayerOntoTransferList(193080);");
                g_engine.RunFIFAScript("ForceCPUPlayerOntoTransferList(193081);");*/

                //g_engine.RunFIFAScript("SackManager(0);");
                //g_engine.RunFIFAScript("SackManager(1);");
                //g_engine.RunFIFAScript("SackManager(2);");
                //// Sack Manager (ID of Team)
                //g_engine.RunFIFAScript("SackManager(10);"); 
                //g_engine.RunFIFAScript("SackManager(11);");

                //g_engine.RunFIFAScript(sb.str());
                /*g_engine.RunFIFAScript("ForceCPUPlayerOntoTransferList(190871);");
                g_engine.RunFIFAScript("ForceUserPlayerOntoTransferList(193080);");
                g_engine.RunFIFAScript("ForceUserPlayerOntoTransferList(193080);");
                g_engine.RunFIFAScript("UpdateTeamBudget(11,11);");
                g_engine.RunFIFAScript("SackManager(1);");
                g_engine.RunFIFAScript("SackManager(2);");
                g_engine.RunFIFAScript("SackManager(11);");
                g_engine.RunFIFAScript("SackManager(0);");
                g_engine.RunFIFAScript("GenerateTransferActivityForTeam(11);");*/
            }
        }

        if (GetAsyncKeyState(VK_END)) {
            Sleep(1000);
            logger.Write(LOG_DEBUG, "Stopping");
            break;
        }
        //logger.Write(LOG_DEBUG, "Tick");
    }
    logger.Write(LOG_DEBUG, "End main loop");

    //MessageBox(NULL, "DLL Ejected...", "DLL Ejected...", MB_ICONINFORMATION);
    EjectDLL(reinterpret_cast<HMODULE>(lpModule));
    return 0;
}





BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
        // This will be the injection into FIFA if Needed
    case DLL_PROCESS_ATTACH:
        //DisableThreadLibraryCalls(hModule);
        CreateThread(NULL, 0, mainFunc, hModule, NULL, NULL);
        break;
    case DLL_THREAD_ATTACH:
        //CreateThread(NULL, 0, mainFunc, hModule, NULL, NULL);
        break;
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C"
{
    __declspec(dllexport) void ShowAMessageBox(char* message) {
        MessageBox(NULL, message, message, MB_OK);
    }
}
//
//HMODULE GetRemoteModuleHandleA(DWORD dwProcessId, const char* szModule)
//{
//    HANDLE tlh = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, dwProcessId);
//
//    MODULEENTRY32 modEntry;
//
//    modEntry.dwSize = sizeof(MODULEENTRY32);
//
//    Module32First(tlh, &modEntry);
//    do
//    {
//        if (_stricmp(szModule, modEntry.szModule) == 0)
  //            CloseHandle(tlh);
//
//            return modEntry.hModule;
//        }
//    } while (Module32Next(tlh, &modEntry));
//
//    CloseHandle(tlh);
//
//    return NULL;
//}
//
//DWORD GetProcessIdFromProcessName(const char* szProcessName) {
//    HANDLE tlh = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);
//
//    PROCESSENTRY32 procEntry;
//
//    procEntry.dwSize = sizeof(PROCESSENTRY32);
//
//    Process32First(tlh, &procEntry);
//    do
//    {
//        printf("proc: %s\n", procEntry.szExeFile);
//
//        if (_stricmp(szProcessName, procEntry.szExeFile) == 0) {
//            CloseHandle(tlh);
//
//            return procEntry.th32ProcessID;
//        }
//    } while (Process32Next(tlh, &procEntry));
//
//    CloseHandle(tlh);
//
//    return GetCurrentProcessId();
//}
//
//// here will be all the calls that c# can make
//extern "C"
//{
//    __declspec(dllexport) void ShowAMessageBox(char* message) {
//        MessageBox(NULL, message, message, MB_OK);
//    }
//
//    __declspec(dllexport) void InjectTheDLL() {
//
//        WORD dwProcessId = GetCurrentProcessId();
//        char szProcessName[256] = "FIFA20.exe";
//        char szModuleName[MAX_PATH] = "v2k4InteropHelper.dll";
//        SetupLogger();
//
//        if (strlen(szModuleName) == 0) {
//            MessageBox(NULL, "Module name is required...\n", "", MB_OK);
//            return;
//        }
//
//        if (strlen(szProcessName) == 0 && dwProcessId == GetCurrentProcessId()) {
//            MessageBox(NULL, "Invalid parameters!", "", MB_OK);
//            return;
//        }
//
//        if (strlen(szProcessName) > 0) {
//            if (dwProcessId == GetCurrentProcessId()) { // Only change the processid if it's not already set
//                dwProcessId = GetProcessIdFromProcessName(szProcessName);
//
//                if (dwProcessId == GetCurrentProcessId()) {
//                    MessageBox(NULL, "Failed to obtain process \"%s\"...\n", "", MB_OK);
//                    return;
//                }
//            }
//        }
//
//        HMODULE hKernel = LoadLibraryA("kernel32.dll");
//        DWORD64 dwLoadLibraryA = (DWORD64)GetProcAddress(hKernel, "LoadLibraryA") - (DWORD64)hKernel;
//
//        MessageBox(NULL, "kernel32.dll", "", MB_OK);
//        MessageBox(NULL, "LoadLibraryA", "", MB_OK);
//        MessageBox(NULL, "Module Name", "", MB_OK);
//
//        char szCurrentModulePath[MAX_PATH] = { 0 };
//
//        GetModuleFileNameA(GetModuleHandle(NULL), szCurrentModulePath, MAX_PATH);
//
//        for (size_t i = strlen(szCurrentModulePath); i > 0; i--) {
//            if (szCurrentModulePath[i] == '\\') {
//                szCurrentModulePath[i + 1] = 0;
//                break;
//            }
//        }
//
//        strcat_s(szCurrentModulePath, szModuleName);
//
//        MessageBox(NULL, "Full Path: %s\n", "", MB_OK);
//
//        DWORD dwFileAttributes = GetFileAttributesA(szModuleName);
//
//        if (dwFileAttributes == INVALID_FILE_ATTRIBUTES && GetLastError() == ERROR_FILE_NOT_FOUND) {
//            MessageBox(NULL, "File not found...\n", "", MB_OK);
//            return;
//        }
//
//        printf("Injecting: %s\n", szModuleName);
//        MessageBox(NULL, "Injecting: %s\n", "", MB_OK);
//
//        HMODULE hRemoteKernel = GetRemoteModuleHandleA(dwProcessId, "kernel32.dll");
//
//        if (hRemoteKernel == NULL) {
//            printf("Failed to locate kernel32 in remote process...\n");
//            return;
//        }
//
//        printf("kernel32 (remote): 0x%016llX\n", hRemoteKernel);
//
//        HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, dwProcessId);
//
//        if (hProcess == INVALID_HANDLE_VALUE) {
//            printf("Failed to locate remote process...\n");
//            return;
//        }
//
//        LPVOID lpModuleName = VirtualAllocEx(hProcess, NULL, strlen(szModuleName) + 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
//
//        if (lpModuleName == NULL) {
//            printf("Failed to allocate module name in remote process...\n");
//            return;
//        }
//
//        if (WriteProcessMemory(hProcess, lpModuleName, szCurrentModulePath, strlen(szCurrentModulePath), NULL) == FALSE) {
//            printf("Failed to write module name in remote process...\n");
//            return;
//        }
//
//        DWORD64 dwRemoteLoadLibraryAddress = ((DWORD64)hRemoteKernel + dwLoadLibraryA);
//
//        printf("LoadLibraryA (remote): %016llX\n", dwRemoteLoadLibraryAddress);
//
//        HANDLE hThread = CreateRemoteThread(hProcess, 0, 0, (LPTHREAD_START_ROUTINE)dwRemoteLoadLibraryAddress, lpModuleName, 0, 0);
//
//        printf("Injecting... ");
//
//        WaitForSingleObject(hThread, INFINITE);
//
//        printf("Injected!\n");
//
//        return;
//
//
//
//    }
//
//}
