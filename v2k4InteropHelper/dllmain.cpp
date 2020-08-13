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
#include "StringBuilder.h"
#include "v2k4helpers.h"
#include "AppInteraction.h"

#pragma warning(disable: 4996)

namespace fs = std::filesystem;
HMODULE hMods[1024];
bool RUNMAINENTRY = true;

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

HANDLE GetProcessByName(std::string name)
{
    DWORD pid = 0;

    // Create toolhelp snapshot.
    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    PROCESSENTRY32 process;
    ZeroMemory(&process, sizeof(process));
    process.dwSize = sizeof(process);

    // Walkthrough all processes.
    if (Process32First(snapshot, &process))
    {
        do
        {
            // Compare process.szExeFile based on format of name, i.e., trim file path
            // trim .exe if necessary, etc.
            if (std::string(process.szExeFile) == name)
            {
                pid = process.th32ProcessID;
                break;
            }
        } while (Process32Next(snapshot, &process));
    }

    CloseHandle(snapshot);

    if (pid != 0)
    {
        return OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
    }

    // Not found


    return NULL;
}

void Init(HMODULE lpModule) {
   

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
    auto ucrtbasedll = GetModuleHandle("ucrtbase.dll");
    if(ucrtbasedll != NULL)
        g_ctx_ucrtbase_dll.Update(ucrtbasedll);

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

    logger.SetMinLevel((LogLevel)0);

    // Live Editor Engine
    g_engine.Setup();

}


DWORD WINAPI mainFunc(LPVOID lpModule)
{
    Sleep(5000);
    if (RUNMAINENTRY) {
        //Init(reinterpret_cast<HMODULE>(lpModule));
        Init(GetModuleHandle(NULL));

        logger.Write(LOG_DEBUG, "Start main loop");

        while (RUNMAINENTRY) {
            Sleep(1);

            if (GetAsyncKeyState(VK_F4)) {
                Sleep(1000);
                fs::create_directory("sandbox");

            }

            if (GetAsyncKeyState(VK_END)) {
                Sleep(1000);
                RUNMAINENTRY = false;
            }
        }
        logger.Write(LOG_DEBUG, "End main loop");

        EjectDLL(reinterpret_cast<HMODULE>(lpModule));
    }
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
        //if(RUNMAINENTRY)
            
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