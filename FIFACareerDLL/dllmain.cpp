// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include<Windows.h>
#include <cstdio>
#include <iostream>
#include "mem.h"
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
#include "dllmain.h"

using namespace std;


int GetTransferBudget(uintptr_t moduleBase, HANDLE pHandle)
{
	DWORD pID;
	DWORD off1, off2, off3, off4;
	DWORD baseAddress;
	DWORD healthAddy;
	int currentBudget;
	// NOT needed for anything within the Base EXE (i.e. Transfer Budget)
	//char moduleName[] = "client.dll";

	////Get Client Base Addy
	//DWORD clientBase = dwGetModuleBaseAddress(t2, pID);
	ReadProcessMemory(pHandle, (LPCVOID)(moduleBase + 0x072BC110), &baseAddress, sizeof(baseAddress), NULL);
	std::cout << "Base Addy is: " << std::hex << baseAddress << std::endl;
	ReadProcessMemory(pHandle, (LPCVOID)(baseAddress + 0x18), &off1, sizeof(off1), NULL);
	std::cout << "Offset 1: " << std::hex << off1 << std::endl;
	ReadProcessMemory(pHandle, (LPCVOID)(off1 + 0x18), &off2, sizeof(off2), NULL);
	std::cout << "Offset 2: " << std::hex << off2 << std::endl;
	ReadProcessMemory(pHandle, (LPCVOID)(off2 + 0x2A8), &off3, sizeof(off3), NULL);
	std::cout << "Offset 3: " << std::hex << off3 << std::endl;
	ReadProcessMemory(pHandle, (LPCVOID)(off3 + 0x268), &off4, sizeof(off3), NULL);
	std::cout << "Offset 4: " << std::hex << off4 << std::endl;
	healthAddy = off4 + 0x8;
	std::cout << "Final Addy: " << std::hex << healthAddy << std::endl;
	ReadProcessMemory(pHandle, (LPCVOID)(healthAddy), &currentBudget, 4, NULL);
	std::cout << "Budget: " << currentBudget << std::endl;
	cout << "Budget: " << currentBudget << std::endl;
	return currentBudget;
}
void SetTransferBudget(uintptr_t moduleBase, HANDLE pHandle, int newTransferBudget)
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
	std::cout << "Base Addy is: " << std::hex << baseAddress << std::endl;
	ReadProcessMemory(pHandle, (LPCVOID)(baseAddress + 0x18), &off1, sizeof(off1), NULL);
	std::cout << "Offset 1: " << std::hex << off1 << std::endl;
	ReadProcessMemory(pHandle, (LPCVOID)(off1 + 0x18), &off2, sizeof(off2), NULL);
	std::cout << "Offset 2: " << std::hex << off2 << std::endl;
	ReadProcessMemory(pHandle, (LPCVOID)(off2 + 0x2A8), &off3, sizeof(off3), NULL);
	std::cout << "Offset 3: " << std::hex << off3 << std::endl;
	ReadProcessMemory(pHandle, (LPCVOID)(off3 + 0x268), &off4, sizeof(off3), NULL);
	std::cout << "Offset 4: " << std::hex << off4 << std::endl;
	healthAddy = off4 + 0x8;
	std::cout << "Final Addy: " << std::hex << healthAddy << std::endl;
	ReadProcessMemory(pHandle, (LPCVOID)(healthAddy), &currentBudget, 4, NULL);
	std::cout << "Budget: " << currentBudget << std::endl;
	WriteProcessMemory(pHandle, (LPVOID)(healthAddy), &newTransferBudget, sizeof(newTransferBudget), 0);
	ReadProcessMemory(pHandle, (LPCVOID)(healthAddy), &currentBudget, 4, NULL);
	cout << "Budget: " << currentBudget << std::endl;
}

DWORD dwGetModuleBaseAddress(TCHAR* lpszModuleName, DWORD pID) {
	DWORD dwModuleBaseAddress = 0;
	HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, pID);
	MODULEENTRY32 ModuleEntry32 = { 0 };
	ModuleEntry32.dwSize = sizeof(MODULEENTRY32);

	if (Module32First(hSnapshot, &ModuleEntry32))
	{
		do {
			if (_tcscmp(ModuleEntry32.szModule, lpszModuleName) == 0)
			{
				dwModuleBaseAddress = (DWORD)ModuleEntry32.modBaseAddr;
				break;
			}
		} while (Module32Next(hSnapshot, &ModuleEntry32));


	}
	CloseHandle(hSnapshot);
	return dwModuleBaseAddress;
}


DWORD WINAPI HackThread(HMODULE hModule) {

	// create console
	AllocConsole();
	FILE* f;
	freopen_s(&f, "CONOUT$", "w", stdout);

	std::cout << "Running Career Mod DLL \n";

	HANDLE hProcess = 0;

	uintptr_t moduleBase = 0, localPtr = 0, transferBudgetAddr = 0;
	bool bHealth = false, bAmmo = false, bRecoil = false;

	const int newValue = 1337;

	//Get ProcId of the target process
	DWORD procId = GetProcId(L"FIFA20.exe");

	HANDLE pHandle;

	DWORD pID;
	//Get Handles
	HWND hGameWindow = FindWindow(NULL, L"FIFA 20");
	GetWindowThreadProcessId(hGameWindow, &pID);
	pHandle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pID);

	if (procId)
	{
		//Get Handle to Process
		//hProcess = OpenProcess(PROCESS_ALL_ACCESS, NULL, procId);

		//Getmodulebaseaddress
		moduleBase = GetModuleBaseAddress(procId, L"FIFA20.exe");
		//std::cout << moduleBase + "\n";

		//Resolve address
		//localPtr = moduleBase + 0x072BC110;
		//std::cout << localPtr + "\n";


		//Resolve base address of the pointer chain
		/*transferBudgetAddr = FindDMAAddy(hProcess, localPtr, { 0x18,  0x18,  0x2A8, 0x268, 0x8 });
		std::cout << transferBudgetAddr + "\n";*/

		std::cout << "Found FIFA 20! \n";

	}
	else
	{
		std::cout << "Process not found, press enter to exit\n";
		getchar();
		return 0;
	}

	DWORD dwExit = 0;
	while (true && procId)
	{
		if (GetAsyncKeyState(VK_END) & 1) {

			std::cout << "VK_END \n";
			break;

		}

		if (GetAsyncKeyState(VK_NUMPAD1) & 1) {

			std::cout << "Set Transfer Budget to 99999999 \n";
			int currentBudget = GetTransferBudget(moduleBase, pHandle);
			SetTransferBudget(moduleBase, pHandle, currentBudget + 9999999);
		}

		if (GetAsyncKeyState(VK_NUMPAD2) & 1) {
		}

		if (GetAsyncKeyState(VK_NUMPAD3) & 1) {
		}

		// get us out of here
		if (GetAsyncKeyState(VK_ESCAPE) & 1) {
			break;
		}
	}

	// key input


	// continuous 


	//

	return 0;
}





BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
	FILE* fp;
	switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
		//MessageBox(NULL, L"Hello World!", L"Dll says:", MB_OK);


		//fp = fopen("test.txt", "w+");
		//fprintf(fp, "Attahed\n");
		//fputs("Attahed\n", fp);
		//fclose(fp);

		CreateThread(nullptr, 0, (LPTHREAD_START_ROUTINE)HackThread, hModule, 0, nullptr);


    case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

