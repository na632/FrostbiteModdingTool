#pragma once
#include <windows.h> 
#include <stdio.h> 
#include <tchar.h>
#include <strsafe.h>

#define BUFSIZE 512

DWORD WINAPI ClientInstanceThread(LPVOID lpvParam);
VOID GetAnswerToRequest(LPTSTR, LPTSTR, LPDWORD);
DWORD WINAPI WaitForConnection(LPVOID lpvParam);

class PIPEInteraction
{
public:
	void CreateServerPipe();
	inline static LPCTSTR lpszPipename = TEXT("\\\\.\\pipe\\dllnamedpipe");
	inline static DWORD  dwThreadId = 0;
private:
	BOOL   fConnected = FALSE;
	HANDLE hPipe = INVALID_HANDLE_VALUE, hThread = NULL, hWaitForConnectionThread = NULL;


};
