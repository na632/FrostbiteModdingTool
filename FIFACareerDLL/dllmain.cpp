// dllmain.cpp : Defines the entry point for the DLL application.
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
#include "dllmain.h"


#include "LiveEditorToCPP.h"
#include "FIFAFinances.h"
#include "FIFAManager.h"
#include <dwmapi.h>

using namespace std;


int Width = GetSystemMetrics(SM_CXSCREEN);
int Height = GetSystemMetrics(SM_CYSCREEN);

const MARGINS Margin = { 0, 0, Width, Height };

char lWindowName[256] = " ";
HWND hWnd;

char tWindowName[256] = "FIFA 20";
HWND tWnd;

HWND hGameWindow;

RECT tSize;
MSG Message;


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
	hGameWindow = FindWindow(NULL, L"FIFA 20");
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
		CloseHandle(pHandle);

		getchar();
		return 0;
	}

	DWORD dwExit = 0;
	while (true && procId)
	{
		if (GetAsyncKeyState(VK_END) & 1) {
			FreeConsole();
			CloseHandle(pHandle);
			return 0;
		}

		if (GetAsyncKeyState(VK_NUMPAD1) & 1) {

			FIFAFinances::RequestAdditionalFunds(moduleBase, pHandle);
		}

		if (GetAsyncKeyState(VK_NUMPAD2) & 1) {
		}

		if (GetAsyncKeyState(VK_NUMPAD3) & 1) {
		}

		// get us out of here
		if (GetAsyncKeyState(VK_ESCAPE) & 1) {
			FreeConsole();
			CloseHandle(pHandle);
			return 0;
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
		CreateThread(nullptr, 0, (LPTHREAD_START_ROUTINE)HackThread, hModule, 0, nullptr);
    case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}
