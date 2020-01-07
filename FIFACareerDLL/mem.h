#pragma once
#include "pch.h"
#include <windows.h>
#include <vector>
//https://guidedhacking.com

namespace mem
{
	void PatchEx(BYTE* dst, BYTE* src, unsigned int size, HANDLE hProcess);
	void NopEx(BYTE* dst, unsigned int size, HANDLE hProcess);
}