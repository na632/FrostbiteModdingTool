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
#include "FIFAManager.h"
class LiveEditorToCPP
{
public:
	/*uintptr_t readMultilevelPointer(uintptr_t moduleBase, HANDLE pHandle, uintptr_t base_addr, std::vector<uintptr_t> offsets) {
		for (auto i = offsets.begin(); i != offsets.end(); ++i)
		{
			if (base_addr == 0 or !base_addr) {
				for (auto j = offsets.begin(); j != offsets.end(); ++j)
				{
				}
				ReadProcessMemory(pHandle, (LPCVOID)(base_addr + i), &base_addr, sizeof(base_addr), NULL);
			}
		}
		return base_addr;
	}*/
};

