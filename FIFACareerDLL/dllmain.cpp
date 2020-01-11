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
	/*FILE* f;
	freopen_s(&f, "CONOUT$", "w", stdout);*/

	std::cout << "Running Career Mod DLL \n";

	/*HANDLE hProcess = 0;

	uintptr_t moduleBase = 0, localPtr = 0, transferBudgetAddr = 0;
	bool bHealth = false, bAmmo = false, bRecoil = false;

	const int newValue = 1337;*/

	//Get ProcId of the target process
	DWORD procId = GetProcId("FIFA20.exe");


	DWORD pID;
	//Get Handles
	hGameWindow = FindWindow(NULL, "FIFA 20");
	GetWindowThreadProcessId(hGameWindow, &pID);
	v2k4::FIFAProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pID);
	if (procId)
	{
		//auto moduleBase = GetModuleBaseAddress(procId, "FIFA20.exe");
		std::cout << "Found FIFA 20! \n";
	}
	else
	{
		std::cout << "Process not found, press enter to exit\n";
		CloseHandle(v2k4::FIFAProcessHandle);

		getchar();
		return 0;
	}

	DWORD dwExit = 0;
	while (true && procId)
	{
		if (GetAsyncKeyState(VK_END) & 1) {
			FreeConsole();
			CloseHandle(v2k4::FIFAProcessHandle);
			return 0;
		}

		if (GetAsyncKeyState(VK_NUMPAD1) & 1) {

			auto moduleBase = GetModuleBaseAddress(procId, "FIFA20.exe");
			FIFAFinances::RequestAdditionalFunds(moduleBase, v2k4::FIFAProcessHandle);
		}

		if (GetAsyncKeyState(VK_NUMPAD2) & 1) {
		}

		if (GetAsyncKeyState(VK_NUMPAD3) & 1) {
		}

		// get us out of here
		if (GetAsyncKeyState(VK_ESCAPE) & 1) {
			FreeConsole();
			CloseHandle(v2k4::FIFAProcessHandle);
			return 0;
		}
	}

	// key input


	// continuous 


	//

	return 0;
}



HANDLE ThreadHandle;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
	FILE* fp;
	switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
		ThreadHandle = CreateThread(nullptr, 0, (LPTHREAD_START_ROUTINE)HackThread, hModule, 0, nullptr);
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:

		CloseHandle(ThreadHandle);
        break;
    }
    return TRUE;
}

//// WHEN YOU GET BACK PAUL READ THIS
//// USE C# to create the overlay you numpty!!!!
extern "C"
{
	__declspec(dllexport) void CloseHook_OUT()
	{
		CloseHandle(ThreadHandle);
	}

}
