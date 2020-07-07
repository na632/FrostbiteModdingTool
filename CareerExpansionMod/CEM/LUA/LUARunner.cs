using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLua;

namespace CareerExpansionMod.CEM.LUA
{
    public class LUARunner
    {
        public Lua LuaState;
        public static LUARunner LUARunnerInstance;

        public LUARunner()
        {
            LuaState = new Lua();

            if(LUARunnerInstance == null)
                LUARunnerInstance = this;


            //KeraLua.LuaHookFunction()
        }

        int lua_GetUserTeamID()
        {
            //int user_teamid = g_engine.GetUserTeamID();
            //lua_pushinteger(L, user_teamid);
            return 1;
        }

        int lua_GetTeamName()
        {
            //unsigned int teamid = lua_tointeger(L, 1);
            //char* teamname = g_engine.GetTeamName(teamid);
            //lua_pushstring(L, teamname);
            return 1;
        }
    }
}
