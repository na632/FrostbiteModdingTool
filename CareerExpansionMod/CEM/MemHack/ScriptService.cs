using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareerExpansionMod.CEM.MemHack
{
    unsafe public struct Globals
    {
        public GlobalsChild* child; //0x0000
    }

    unsafe public struct GlobalsChild
    {
        public void* fnUnk1; //0x0000
        public void* fnUnk2; //0x0008
        public void* fnUnk3; //0x0010
        public void* fnUnk4; //0x0018
        public void* fnUnk5; //0x0020
        public void* fnUnk6; //0x0028
        public void* fnGetPtr; //0x0030
        public void* fnUnk7; //0x0038
        public fixed char pad_0040[248]; //0x0040
    }; //Size: 0x0138

    unsafe public struct ScriptService
    {
        fixed char pad_0000[8]; //0x0000
        void* Lua_State; //0x0008
        void* mLuaCallHistory; //0x0010
        fixed char pad_0018[40]; //0x0018
        void* MaybeLuaFunctions; //0x0040
        int CompiledLuaCount; //0x0048
        fixed char pad_004C[4]; //0x004C
        void* CompiledLua1; //0x0050
        void* N00000BCC; //0x0058
        int MaybeCompiledLua1Size; //0x0060
        fixed char pad_0064[4]; //0x0064
        void* CompiledLua2; //0x0068
        void* N00000BCF; //0x0070
        int MaybeCompiledLua2Size; //0x0078
        fixed char pad_007C[12]; //0x007C
    }; //Size: 0x0088

    unsafe public class LuaCMEngineFuncReg
    {
        public string fName; //0x0000
        public int nArgs; //0x0008
        public int Unk; //0x000C
        public void* pCFunc; //0x0010
    }; //Size: 0x0018

    unsafe public struct ScriptFunctions
    {
        public ScriptFunctions0* pScriptFunctions0; //0x0000
        public fixed char pad_0008[8]; //0x0008
    }; //Size: 0x0010

    unsafe public struct ScriptFunctions0
    {
        public fixed char pad_0000[4568]; //0x0000
        public ScriptFunctionsUnk1* pScriptFunctionsUnk1; //0x11D8
}; //Size: 0x11E0

    unsafe public struct ScriptFunctionsUnk1
    {
    unsafe public ScriptFunctionsUnk2* pScriptFunctionsUnk2; //0x0000
}; //Size: 0x0008

    unsafe public struct ScriptFunctionsUnk2
    {
    unsafe public LuaScriptFunctions3* pLuaScriptFunctions; //0x0000
}; //Size: 0x0008

    unsafe public struct LuaScriptFunctions3
    {
    public void* pThis; //0x0000
    }; //Size: 0x0008

}


