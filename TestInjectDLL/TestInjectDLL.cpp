// TestInjectDLL.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>

#include <string>
#include <cstring>
#include <iostream>
#include <map>
#include <filesystem>
#include <TlHelp32.h>

#pragma warning(disable: 4996)
namespace fs = std::filesystem;

HMODULE GetRemoteModuleHandleA(DWORD dwProcessId, const char* szModule)
{
    HANDLE tlh = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, dwProcessId);

    MODULEENTRY32 modEntry;

    modEntry.dwSize = sizeof(MODULEENTRY32);

    Module32First(tlh, &modEntry);
    do
    {
        if (_stricmp(szModule, modEntry.szModule) == 0)
        {
            CloseHandle(tlh);

            return modEntry.hModule;
        }
    } while (Module32Next(tlh, &modEntry));

    CloseHandle(tlh);

    return NULL;
}

DWORD GetProcessIdFromProcessName(const char* szProcessName) {
    HANDLE tlh = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

    PROCESSENTRY32 procEntry;

    procEntry.dwSize = sizeof(PROCESSENTRY32);

    Process32First(tlh, &procEntry);
    do
    {
        printf("proc: %s\n", procEntry.szExeFile);

        if (_stricmp(szProcessName, procEntry.szExeFile) == 0) {
            CloseHandle(tlh);

            return procEntry.th32ProcessID;
        }
    } while (Process32Next(tlh, &procEntry));

    CloseHandle(tlh);

    return GetCurrentProcessId();
}
int main()
{
        WORD dwProcessId = GetCurrentProcessId();
    char szProcessName[256] = "FIFA20.exe";
    char szModuleName[MAX_PATH] = "G:\\Work\\FIFA Modding\\FIFAModdingUI\\v2k4InteropHelper\\x64\\Debug\\v2k4InteropHelper.dll";

    if (strlen(szModuleName) == 0) {
        MessageBox(NULL, "Module name is required...\n", "", MB_OK);
        return 0;
    }

    if (strlen(szProcessName) == 0 && dwProcessId == GetCurrentProcessId()) {
        MessageBox(NULL, "Invalid parameters!", "", MB_OK);
        return 0;
    }

    if (strlen(szProcessName) > 0) {
        if (dwProcessId == GetCurrentProcessId()) { // Only change the processid if it's not already set
            dwProcessId = GetProcessIdFromProcessName(szProcessName);

            if (dwProcessId == GetCurrentProcessId()) {
                MessageBox(NULL, "Failed to obtain process \"%s\"...\n", "", MB_OK);
                return 0;
            }
        }
    }

    HMODULE hKernel = LoadLibraryA("kernel32.dll");
    DWORD64 dwLoadLibraryA = (DWORD64)GetProcAddress(hKernel, "LoadLibraryA") - (DWORD64)hKernel;

    MessageBox(NULL, "kernel32.dll", "", MB_OK);
    MessageBox(NULL, "LoadLibraryA", "", MB_OK);
    MessageBox(NULL, "Module Name", "", MB_OK);

    char szCurrentModulePath[MAX_PATH] = { 0 };

    GetModuleFileNameA(GetModuleHandle(NULL), szCurrentModulePath, MAX_PATH);

    for (size_t i = strlen(szCurrentModulePath); i > 0; i--) {
        if (szCurrentModulePath[i] == '\\') {
            szCurrentModulePath[i + 1] = 0;
            break;
        }
    }

    strcat_s(szCurrentModulePath, szModuleName);

    MessageBox(NULL, "Full Path: %s\n", "", MB_OK);

    DWORD dwFileAttributes = GetFileAttributesA(szModuleName);

    if (dwFileAttributes == INVALID_FILE_ATTRIBUTES && GetLastError() == ERROR_FILE_NOT_FOUND) {
        MessageBox(NULL, "File not found...\n", "", MB_OK);
        return 0;
    }

    printf("Injecting: %s\n", szModuleName);
    MessageBox(NULL, "Injecting: %s\n", "", MB_OK);

    HMODULE hRemoteKernel = GetRemoteModuleHandleA(dwProcessId, "kernel32.dll");

    if (hRemoteKernel == NULL) {
        printf("Failed to locate kernel32 in remote process...\n");
        return 0;
    }

    printf("kernel32 (remote): 0x%016llX\n", hRemoteKernel);

    HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, dwProcessId);

    if (hProcess == INVALID_HANDLE_VALUE) {
        printf("Failed to locate remote process...\n");
        return 0;
    }

    LPVOID lpModuleName = VirtualAllocEx(hProcess, NULL, strlen(szModuleName) + 1, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

    if (lpModuleName == NULL) {
        printf("Failed to allocate module name in remote process...\n");
        return 0;
    }

    if (WriteProcessMemory(hProcess, lpModuleName, szCurrentModulePath, strlen(szCurrentModulePath), NULL) == FALSE) {
        printf("Failed to write module name in remote process...\n");
        return 0;
    }

    DWORD64 dwRemoteLoadLibraryAddress = ((DWORD64)hRemoteKernel + dwLoadLibraryA);

    printf("LoadLibraryA (remote): %016llX\n", dwRemoteLoadLibraryAddress);

    HANDLE hThread = CreateRemoteThread(hProcess, 0, 0, (LPTHREAD_START_ROUTINE)dwRemoteLoadLibraryAddress, lpModuleName, 0, 0);

    printf("Injecting... ");

    WaitForSingleObject(hThread, INFINITE);

    printf("Injected!\n");

    return 0;
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
