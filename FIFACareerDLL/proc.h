#pragma once
#include <vector>
#include <windows.h>
#include <TlHelp32.h>
//https://guidedhacking.com

DWORD GetProcId(const char* procName);

uintptr_t GetModuleBaseAddress(DWORD procId, const char* modName);

uintptr_t FindDMAAddy(HANDLE hProc, uintptr_t ptr, std::vector<unsigned int> offsets);