#pragma once
extern "C"
{
#include "lua.h"
#include "lauxlib.h"
#include "lualib.h"
}
#ifdef _DEBUG
#pragma comment(lib, "Lua534_d.lib")
#else
#pragma comment(lib, "Lua534.lib")
#endif
