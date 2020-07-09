#pragma once
#include <Windows.h>
#include <psapi.h>
#include <string>
#include <vector>

#pragma comment(lib, "Psapi.lib")
#define INRANGE( x, a, b )   ( x >= a && x <= b )
#define GETBITS( x )      ( INRANGE( x, '0', '9' ) ? ( x - '0' ) : ( ( x & ( ~0x20 ) ) - 'A' + 0xa ) )
#define GETBYTE( x )      ( GETBITS( x[0] ) << 4 | GETBITS( x[1] ) )

namespace PatScanner
{
    inline __int64 ResolvePtr(uintptr_t address, int offsetpos)
    {
        __int32 offset = *reinterpret_cast<__int32*>(address + offsetpos);

        __int64 resolved = address + offset + offsetpos + 4;
        return resolved;
    }

    inline uintptr_t Find(uintptr_t base, uintptr_t size, const char* pattern)
    {
        const unsigned char* pat = reinterpret_cast<const unsigned char*>(pattern);

        uintptr_t firstMatch = 0;
        uintptr_t range = base + size;
        uintptr_t find_result = NULL;

        for (uintptr_t pCurrent = base; pCurrent < range; ++pCurrent)
        {
            if (*const_cast<PBYTE>(pat) == static_cast<BYTE>('\?') || *reinterpret_cast<BYTE*>(pCurrent) == GETBYTE(pat))
            {
            if (!firstMatch)
                firstMatch = pCurrent;

            if (!pat[2]) {
                find_result = firstMatch;
                break;
            }
               

            pat += (*(PWORD)pat == (WORD)'\?\?' || *(PBYTE)pat != (BYTE)'\?') ? 3 : 2;

            if (!*pat) {
                find_result = firstMatch;
                break;
            }

            }
            else if (firstMatch) {

            pCurrent = firstMatch;
            pat = reinterpret_cast<const unsigned char*>(pattern);
            firstMatch = 0;

            }
        }
        return find_result;
    }

    inline void LogNewAddrModule(uintptr_t newaddr) {
        HMODULE hMods[1024];
        DWORD cbNeeded;
        HANDLE hproc = GetCurrentProcess();
        if (!EnumProcessModulesEx(hproc, hMods, sizeof(hMods), &cbNeeded, LIST_MODULES_ALL)) {
            DWORD e = GetLastError();
            return;
        }
        unsigned int i;
        for (i = 0; i < (cbNeeded / sizeof(HMODULE)); i++) {
            MODULEINFO modinfo;
            uintptr_t module_base = NULL;
            uintptr_t module_size = NULL;
            uintptr_t module_end = NULL;

            std::string mname = "";
            std::string mfullpath = "";
            char fullModPath[MAX_PATH];

            if (GetModuleInformation(hproc, hMods[i], &modinfo, sizeof(MODULEINFO))) {
                module_base = reinterpret_cast<uintptr_t>(modinfo.lpBaseOfDll);
                module_size = static_cast<uintptr_t>(modinfo.SizeOfImage);
                module_end = module_base + module_size;
            }
            else {
                DWORD e = GetLastError();
                //logger.Write(LOG_ERROR, "LogNewAddrModule - GetModuleInformation Err: %d", e);
                continue;
            }

            if (GetModuleFileNameA(hMods[i], fullModPath, MAX_PATH))
            {
                mfullpath = fullModPath;
                size_t lastIndex = mfullpath.find_last_of("\\") + 1;
                mname = mfullpath.substr(lastIndex, mfullpath.length() - lastIndex);
            }

            if (newaddr >= module_base && newaddr <= module_end) {
                /*logger.Write(
                    LOG_INFO,
                    "newaddr: 0x%08llX (%s+0x%08llX) is at module %s (%s)",
                    newaddr, mname.c_str(), (newaddr-module_base), mname.c_str(), mfullpath.c_str()
                );*/
            }
            else {
               /* logger.Write(LOG_DEBUG, "%s  0x%08llX - 0x%08llX (%s)",
                    mname.c_str(), module_base, module_end, mfullpath.c_str()
                );*/
            }
        }
    }

    inline DWORD_PTR FollowHooks(std::string name, uintptr_t addr, int num, bool rec=false) {
        std::vector<unsigned char> bArr = {};

        unsigned char b = 0x0;

        char arr_of_bytes[70];
        for (__int64 i = 0; i <= num; i++) {
            b = *(BYTE*)(addr + i);
            bArr.push_back(b);
            sprintf_s(arr_of_bytes + (3 * i), 70 - (3 * i), "%02X ", b);
        }
        if (!rec) {
            //logger.Write(LOG_INFO, "Original bytes for %s: %s", name.c_str(), arr_of_bytes);
        }
        else {
            //logger.Write(LOG_INFO, "Bytes in: %s", arr_of_bytes);
        }


        // jmp -> jmp qword
        // RivaTuner (RTSS)
        if (bArr[0] == 0xE9) {
            uintptr_t newaddr = ResolvePtr(addr, 1);
            //logger.Write(LOG_INFO, "JMP 0xE9 detected, ORG: - 0x%08llX New: - 0x%08llX", addr, newaddr);
            LogNewAddrModule(newaddr);
            return FollowHooks(name, newaddr, num, true);
        }

        std::vector<unsigned char> jmpRIP = {
            { 0xFF },
            { 0x25 }
        };
        if (jmpRIP[0] == bArr[0] && jmpRIP[1] == bArr[1]) {
            __int64 offset = *reinterpret_cast<__int64*>(addr + 6);
            //logger.Write(LOG_INFO, "JMP jmpRIP detected, ORG: - 0x%08llX New: - 0x%08llX", addr, offset);
            LogNewAddrModule((uintptr_t)offset);
            addr = offset;
            return FollowHooks(name, addr, num, true);
        };

        // jmp rax
        // For example Fraps
        std::vector<unsigned char> jmpRAX = {
            { 0xFF },
            { 0xE0 }
        };
        if (jmpRAX[0] == bArr[7] && jmpRAX[1] == bArr[8]) {
            __int32 offset = *reinterpret_cast<__int32*>(addr + 3);
            //logger.Write(LOG_INFO, "JMP RAX detected, ORG: - 0x%08llX New: - 0x%08llX", addr, (uintptr_t)offset);
            LogNewAddrModule((uintptr_t)offset);
            addr = offset;
            return FollowHooks(name, addr, num, true);
        };
        return addr;
    }
}