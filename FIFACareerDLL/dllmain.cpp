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


HWND hGameWindow;

RECT tSize;
MSG Message;

bool InCareerMode = false;


DWORD WINAPI HackThread(HMODULE hModule) {

	// create console
	AllocConsole();
	SetConsoleCtrlHandler(NULL, true);
	FILE* f;
	freopen_s(&f, "CONOUT$", "w", stdout);

	std::cout << "Running Career Mod DLL \n";

	HANDLE hProcess = 0;

	uintptr_t moduleBase = 0, localPtr = 0, transferBudgetAddr = 0;
	bool bHealth = false, bAmmo = false, bRecoil = false;

	const int newValue = 1337;

	//Get ProcId of the target process
	DWORD procId = GetProcId("FIFA20.exe");


	DWORD pID;
	//Get Handles
	hGameWindow = FindWindow(NULL, "FIFA 20");
	GetWindowThreadProcessId(hGameWindow, &pID);
	v2k4::FIFAProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pID);
	if (procId)
	{
		moduleBase = GetModuleBaseAddress(procId, "FIFA20.exe");
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
		try {
			Sleep(100);

			if (!InCareerMode)
			{
				int gameDate = FIFAManager::GetGameDate(moduleBase, v2k4::FIFAProcessHandle);

				if
					(gameDate >= 20190101
						&& gameDate <= 20990101)

				{
					InCareerMode = true;
					std::cout << "In Career Mode! \n";
					Sleep(1000);

				}
			}
			/*else if (InCareerMode
				&&
				(gameDate == 0
					)
				)
			{
				InCareerMode = false;
				std::cout << "Not In Career Mode! \n";
			}*/
		}
		catch (exception e) {
			std::cout << e.what() << "\n";

		}



		if (GetAsyncKeyState(VK_END) & 1) {
			std::cout << "Trying to exit \n";
			FreeConsole();
			CloseHandle(v2k4::FIFAProcessHandle);
			return 0;
		}

		if (GetAsyncKeyState(VK_NUMPAD1) & 1) {
			if (InCareerMode) {
				FIFAFinances::RequestAdditionalFunds(moduleBase, v2k4::FIFAProcessHandle);
			}
		}

		if (GetAsyncKeyState(VK_NUMPAD2) & 1) {
		}

		if (GetAsyncKeyState(VK_NUMPAD3) & 1) {
		}

		// get us out of here
		/*if (GetAsyncKeyState(VK_ESCAPE) & 1) {
			std::cout << "Trying to exit \n";
			FreeConsole();
			CloseHandle(v2k4::FIFAProcessHandle);
			return 0;
		}*/
	}

	// key input


	// continuous 


	//
	FreeLibraryAndExitThread(hModule, 0);
	ExitThread(0);
}


HANDLE Thread;


BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	FILE* fp;
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		Thread = CreateThread(nullptr, 0, (LPTHREAD_START_ROUTINE)HackThread, hModule, 0, nullptr);
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
		Thread = NULL;
	case DLL_PROCESS_DETACH:
		Thread = NULL;
		break;
	}
	return TRUE;
}


extern "C"
{
	__declspec(dllexport) bool CareerModeLoaded_OUT()
	{
		return InCareerMode;
	}
}
