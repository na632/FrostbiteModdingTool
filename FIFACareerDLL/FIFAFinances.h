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



class FIFAFinances
{
public:

	static int GetTransferBudget(uintptr_t moduleBase, HANDLE pHandle)
	{
		DWORD off1, off2, off3, off4;
		DWORD baseAddress;
		DWORD healthAddy;
		int currentBudget;
		// NOT needed for anything within the Base EXE (i.e. Transfer Budget)
		//char moduleName[] = "client.dll";

		////Get Client Base Addy
		//DWORD clientBase = dwGetModuleBaseAddress(t2, pID);
		ReadProcessMemory(pHandle, (LPCVOID)(moduleBase + 0x072BC110), &baseAddress, sizeof(baseAddress), NULL);
		//std::cout << "Base Addy is: " << std::hex << baseAddress << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(baseAddress + 0x18), &off1, sizeof(off1), NULL);
		//std::cout << "Offset 1: " << std::hex << off1 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off1 + 0x18), &off2, sizeof(off2), NULL);
		//std::cout << "Offset 2: " << std::hex << off2 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off2 + 0x2A8), &off3, sizeof(off3), NULL);
		//std::cout << "Offset 3: " << std::hex << off3 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off3 + 0x268), &off4, sizeof(off3), NULL);
		//std::cout << "Offset 4: " << std::hex << off4 << std::endl;
		healthAddy = off4 + 0x8;
		//std::cout << "Final Addy: " << std::hex << healthAddy << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(healthAddy), &currentBudget, 4, NULL);
		std::cout << "Budget: " << currentBudget << std::endl;
		return currentBudget;
	}
	static void SetTransferBudget(uintptr_t moduleBase, HANDLE pHandle, int newTransferBudget)
	{
		DWORD off1, off2, off3, off4;
		DWORD baseAddress;
		DWORD healthAddy;
		int currentBudget;
		// NOT needed for anything within the Base EXE (i.e. Transfer Budget)
		//char moduleName[] = "client.dll";

		////Get Client Base Addy
		//DWORD clientBase = dwGetModuleBaseAddress(t2, pID);
		ReadProcessMemory(pHandle, (LPCVOID)(moduleBase + 0x072BC110), &baseAddress, sizeof(baseAddress), NULL);
		//std::cout << "Base Addy is: " << std::hex << baseAddress << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(baseAddress + 0x18), &off1, sizeof(off1), NULL);
		//std::cout << "Offset 1: " << std::hex << off1 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off1 + 0x18), &off2, sizeof(off2), NULL);
		//std::cout << "Offset 2: " << std::hex << off2 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off2 + 0x2A8), &off3, sizeof(off3), NULL);
		//std::cout << "Offset 3: " << std::hex << off3 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off3 + 0x268), &off4, sizeof(off3), NULL);
		//std::cout << "Offset 4: " << std::hex << off4 << std::endl;
		healthAddy = off4 + 0x8;
		//std::cout << "Final Addy: " << std::hex << healthAddy << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(healthAddy), &currentBudget, 4, NULL);
		//std::cout << "Budget: " << currentBudget << std::endl;
		WriteProcessMemory(pHandle, (LPVOID)(healthAddy), &newTransferBudget, sizeof(newTransferBudget), 0);
		ReadProcessMemory(pHandle, (LPCVOID)(healthAddy), &currentBudget, 4, NULL);
		std::cout << "Transfer Budget: " << currentBudget << std::endl;
	}

	static int GetStartingBudget(uintptr_t moduleBase, HANDLE pHandle)
	{
		DWORD off1, off2, off3, off4;
		DWORD baseAddress;
		DWORD healthAddy;
		int currentBudget;
		// NOT needed for anything within the Base EXE (i.e. Transfer Budget)
		//char moduleName[] = "client.dll";

		////Get Client Base Addy
		//DWORD clientBase = dwGetModuleBaseAddress(t2, pID);
		ReadProcessMemory(pHandle, (LPCVOID)(moduleBase + 0x06E01930), &baseAddress, sizeof(baseAddress), NULL);
		//std::cout << "Base Addy is: " << std::hex << baseAddress << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(baseAddress + 0x10), &off1, sizeof(off1), NULL);
		//std::cout << "Offset 1: " << std::hex << off1 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off1 + 0x48), &off2, sizeof(off2), NULL);
		//std::cout << "Offset 2: " << std::hex << off2 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off2 + 0x30), &off3, sizeof(off3), NULL);
		//std::cout << "Offset 3: " << std::hex << off3 << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(off3 + 0x58), &off4, sizeof(off3), NULL);
		//std::cout << "Offset 4: " << std::hex << off4 << std::endl;
		healthAddy = off4 + 0x5E0;
		//std::cout << "Final Addy: " << std::hex << healthAddy << std::endl;
		ReadProcessMemory(pHandle, (LPCVOID)(healthAddy), &currentBudget, 4, NULL);
		std::cout << "Starting Budget: " << currentBudget << std::endl;
		return currentBudget;
	}

	static bool RequestAdditionalFunds(uintptr_t moduleBase, HANDLE pHandle)
	{
		std::cout << "Requesting additional funds? " << std::endl;
		int StartingBudget = GetStartingBudget(moduleBase, pHandle);
		int TransferBudget = GetTransferBudget(moduleBase, pHandle);
		double ratioBudget = round((double)StartingBudget / (double)TransferBudget * 100);
		int managerRating = FIFAManager::GetManagerRating(moduleBase, pHandle);
		std::cout << "Budget Ratio: " << ratioBudget << std::endl;
		std::cout << "Manager Rating: " << managerRating << std::endl;

		if (StartingBudget
			>
			TransferBudget
			)
		{
			std::cout << "Starting Budget is more than Transfer Budget: Answer is MAYBE!" << std::endl;
			int AdditionalBudget = StartingBudget - TransferBudget;
			std::cout << "Ahh go on then... you can have " << AdditionalBudget << " more!" << std::endl;
			SetTransferBudget(moduleBase, pHandle, StartingBudget + AdditionalBudget);

			return true;
		}
		else {
			std::cout << "Starting Budget is less than Transfer Budget: Answer is NO!" << std::endl;
			return false;
		}
	}
};

//// WHEN YOU GET BACK PAUL READ THIS
//// USE C# to create the overlay you numpty!!!!
extern "C"
{
	__declspec(dllexport) int GetTransferBudget_OUT()
	{
		int budget = 0;
		DWORD procId = GetProcId("FIFA20.exe");
		
		if (procId && v2k4::FIFAProcessHandle)
		{
			//Get Handle to Process
			//hProcess = OpenProcess(PROCESS_ALL_ACCESS, NULL, procId);

			//Getmodulebaseaddress
			auto moduleBase = GetModuleBaseAddress(procId, "FIFA20.exe");
			budget = FIFAFinances::GetTransferBudget(moduleBase, v2k4::FIFAProcessHandle);

		}
		return budget;
	}

	__declspec(dllexport) bool RequestAdditionalFunds_OUT()
	{
		bool approved = false;
		DWORD procId = GetProcId("FIFA20.exe");
		if (procId && v2k4::FIFAProcessHandle)
		{
			auto moduleBase = GetModuleBaseAddress(procId, "FIFA20.exe");
			approved = FIFAFinances::RequestAdditionalFunds(moduleBase, v2k4::FIFAProcessHandle);

		}
		return approved;
	}
}
