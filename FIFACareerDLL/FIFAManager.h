#pragma once
#include "pch.h"
#include<Windows.h>
#include <cstdio>
#include <iostream>
#include "proc.h"

#include <vector>
#include <TlHelp32.h>
#include <tchar.h>

#include <stdlib.h>
#include <string>
//
//#include "atlbase.h"
//#include "atlstr.h"
#include "comutil.h"
class FIFAManager
{
public:
	static int GetManagerRating(uintptr_t moduleBase, HANDLE pHandle)
	{
		DWORD off1, off2, off3, off4;
		DWORD baseAddress;
		DWORD healthAddy;
		int currentRating;
		// NOT needed for anything within the Base EXE (i.e. Transfer Budget)
		//char moduleName[] = "client.dll";

		////Get Client Base Addy
		//DWORD clientBase = dwGetModuleBaseAddress(t2, pID);
		ReadProcessMemory(pHandle, (LPCVOID)(moduleBase + 0x06E045B0), &baseAddress, sizeof(baseAddress), NULL);
		//std::cout << "Base Addy is: " << std::hex << baseAddress << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(baseAddress + 0x538), &off1, sizeof(off1), NULL);
		//std::cout << "Offset 1: " << std::hex << off1 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off1 + 0x0), &off2, sizeof(off2), NULL);
		//std::cout << "Offset 2: " << std::hex << off2 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off2 + 0x20), &off3, sizeof(off3), NULL);
		//std::cout << "Offset 3: " << std::hex << off3 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off3 + 0x240), &off4, sizeof(off3), NULL);
		//std::cout << "Offset 4: " << std::hex << off4 << std::endl;
		healthAddy = off4 + 0x574;
		//std::cout << "Final Addy: " << std::hex << healthAddy << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(healthAddy), &currentRating, 4, NULL);
		std::cout << "Manager Rating: " << currentRating << std::endl;
		return currentRating;
	}
};

