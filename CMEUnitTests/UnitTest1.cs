using CareerExpansionMod.CEM.FIFA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using v2k4FIFAModding.Career.CME.FIFA;
using CareerExpansionMod.CEM.Finances;
using v2k4FIFAModdingCL.MemHack.Core;
using CareerExpansionMod.CEM.Data;
using CareerExpansionMod.CEM;
using System.Threading;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection.Metadata.Ecma335;
using CareerExpansionMod.CEM.MemHack;
using Newtonsoft.Json;
using System.Buffers;
using System.Buffers.Text;
using System.IO;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO.Pipes;
using Reinterpret.Net;

namespace CMEUnitTests
{
    

    [TestClass]
    public class UnitTest1
    {
        //[DllImport("paulv2k4HackHelpers.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern ulong ResolvePtr(UIntPtr address, int offsetpos);

        //[DllImport("v2k4InteropHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void ShowAMessageBox(string message);

        //[DllImport("v2k4InteropHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void InjectTheDLL();

        //[DllImport("v2k4InteropHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void EjectTheDLL();

        [DllImport(@"v2k4InteropHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern dynamic GetScriptService();

        [DllImport(@"v2k4InteropHelper.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetLUAState();



        [TestMethod]
        public void AttachInteropHelper()
        {
            int? proc = CoreHack.GetProcess();
            while (!proc.HasValue)
            {
                Debug.WriteLine($"Waiting for FIFA to appear");
                proc = CoreHack.GetProcess();
                Thread.Sleep(4000);
            }
            if (proc.HasValue)
            {
                Thread.Sleep(1000);

                //if (File.Exists("v2k4InteropHelper.dll"))
                //    CoreHack.MemLib.InjectDLL("v2k4InteropHelper.dll");
                //Assembly.Load()
                var dllpath = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\v2k4InteropHelper.dll";
                Debug.WriteLine($"About to inject: {dllpath}");
                var bl = new Bleak.Injector("FIFA20", dllpath, Bleak.InjectionMethod.CreateThread, Bleak.InjectionFlags.None);
                bl.InjectDll();
                Debug.WriteLine($"Injected: {dllpath}");
                //bl.EjectDll();
                //Thread checkThread = new Thread(() => {
                while (proc.HasValue)
                {
                    proc = CoreHack.GetProcess();
                    Thread.Sleep(2000);
                }


                //});
                //checkThread.Start();
                //checkThread.Join();

                //byte[] payload = Encoding.Unicode.GetBytes("");
                //// Synchronously waits for dllmain() to finish, then unloads the DLL
                //Injector.Inject((uint)proc.Value, "v2k4InteropHelper.dll", "v2k4InteropHelper.dll", "my shared memory name", payload);


            }
            //InjectTheDLL();
        }

        [TestMethod]
        public void LoadFIFAGameDBPointers()
        {
            var proc = CoreHack.GetProcess();
            if(proc.HasValue)
            {
                /*
                 * local codeGameDB = tonumber(get_validated_address('AOB_codeGameDB'), 16)
    local base_ptr = readPointer(byteTableToDword(readBytes(codeGameDB+4, 4, true)) + codeGameDB + 8)
    if DEBUG_MODE then
        do_log(string.format("codeGameDB base_ptr %X", base_ptr))
    end

    local DB_One_Tables_ptr = readMultilevelPointer(base_ptr, {0x10, 0x390})
    local DB_Two_Tables_ptr = readMultilevelPointer(base_ptr, {0x10, 0x3C0})
    local DB_Three_Tables_ptr = readMultilevelPointer(base_ptr, {0x10, 0x3F0})
                 */
                var codeGameDB = CoreHack.MemLib.AoBScan(long.Parse(CoreHack.MemLib.getMinAddress().ToString()), 0x00007ffffffeffff, "4C 0F 44 35 ?? ?? ?? ?? 41 8B 4E 08", true, true, true).Result.FirstOrDefault();
                if (codeGameDB > 0)
                {
                    var base_ptr = CoreHack.ResolvePtr(codeGameDB, 4).ToString("X8");
                    Debug.WriteLine(base_ptr);

                    //public string GAME_SAVE_FILE = "FIFA20.exe+06dea808,0x20,0x20,0x54C";
                    //var r = "FIFA20.exe+" + base_ptr + ",0x10,0x390";
                    var r = base_ptr + ",0x10,0x390";
                    //var DB_One_Tables_ptr = CoreHack.ResolvePtr(CoreHack.MemLib.readLong(r), 4).ToString("X8");
                    var DB_One_Tables_ptr = CoreHack.MemLib.readLong(r).ToString("X8");
                    Debug.WriteLine(DB_One_Tables_ptr);
                    var DB_Two_Tables_ptr = CoreHack.MemLib.readLong(base_ptr + ",0x10,0x3C0").ToString("X8");
                    Debug.WriteLine(DB_Two_Tables_ptr);
                    var DB_Three_Tables_ptr = CoreHack.MemLib.readLong(base_ptr + ",0x10,0x3F0").ToString("X8");
                    Debug.WriteLine(DB_Three_Tables_ptr);
                    /*
                    local players_firstrecord = readMultilevelPointer(DB_One_Tables_ptr, { 0xB0, 0x28, 0x30})
                    -- [firstPlayerDataPtr + b0] + 28]+30]0
                    if DEBUG_MODE then
                        do_log(string.format("players_firstrecord %X", players_firstrecord))
                    end

                    writeQword("firstPlayerDataPtr", players_firstrecord)
                    writeQword("playerDataPtr", players_firstrecord)
                    */
                    var players_firstrecord = CoreHack.MemLib.readLong(DB_One_Tables_ptr + ",0xB0,0x28,0x30").ToString("X8");
                    Debug.WriteLine(players_firstrecord);

                }
            }
        }

        [TestMethod]
        public unsafe void LoadFIFALUAAndRunScript()
        {
            var proc = CoreHack.GetProcess();
            if (proc.HasValue)
            {
                var i = GetLUAState();
                //var codeGameDB_addy = CoreHack.MemLib.AoBScan("44 8B 7A 34 44 8B 62 38", true, true).Result.FirstOrDefault();

                //// SCRIPT FUNCTIONS
                //    UIntPtr pScriptFunctions = CoreHack.ResolvePtr(CareerExpansionMod.CEM.MemHack.AOBMap.g_AOBs["AOB_pScriptFunctions"], 61).ToUIntPtr();
                //    Debug.WriteLine(pScriptFunctions.ToUInt64().ToString("X8"));
                //    ScriptFunctions* scriptFunctions = (ScriptFunctions*)pScriptFunctions;

                //// SCRIPT SERVICE
                //var addrGlobal = CoreHack.MemLib.AoBScan(CareerExpansionMod.CEM.MemHack.AOBMap.g_AOBs["AOB_pGlobal"], true, true, true).Result.FirstOrDefault();

                //var globalPtr = CoreHack.ResolvePtr(addrGlobal, 3);
                //Debug.WriteLine(globalPtr.ToString("X8"));
                //var ptrglobalPtr = new UIntPtr(Convert.ToUInt64(globalPtr));

                //Globals* G = (Globals*)(globalPtr);
                //GlobalsChild* GC = (GlobalsChild*)G->child;
                //UIntPtr fnAddr = (UIntPtr)(GC->fnGetPtr);
               //var glbs = ASCIIEncoding.ASCII.GetBytes(globalPtr.ToString()).Reinterpret<Globals>();
                //Globals G = new Globals();
                //G->child->fnUnk1 = ptrglobalPtr.ToPointer();
                //G->child->fnGetPtr = (ptrglobalPtr+30).ToPointer();

                /*
                uintptr_t pScriptFunctions = g_ctx_proc.getAddr("AOB_pScriptFunctions", true, 61);
                logger.Write(LOG_DEBUG, "pScriptFunctions:  0x%08llX", pScriptFunctions);
                script_functions = reinterpret_cast<ScriptFunctions*>(pScriptFunctions);


                // LUA Script Service
                uintptr_t pGlobal = g_ctx_proc.getAddr("AOB_pGlobal", true, 3);
                logger.Write(LOG_DEBUG, "pGlobal:  0x%08llX", pGlobal);
                // 0x6757DC30

                typedef uint64_t(__fastcall* _getPtr)(__int64 pThis, __int32 uid);

                Globals* G = reinterpret_cast<Globals*>(pGlobal);

                uintptr_t fnAddr = (uintptr_t)(G->child->fnGetPtr);
                uintptr_t pScript = ((_getPtr)fnAddr)(pGlobal, 0x0D6DD491);
                logger.Write(LOG_DEBUG, "pScript:  0x%08llX", pScript);
                */



                //var i2 = CoreHack.MemLib.readBytes((globalPtr+8).ToString("X8"), 8);
                //var i3 = CoreHack.MemLib.readBytes((globalPtr+10).ToString("X8"), 8);
                //Debug.WriteLine(JsonConvert.SerializeObject(G));
                //Globals G = (Globals)ptrglobalPtr;


                //var addrScriptService = CoreHack.MemLib.AoBScan(CareerExpansionMod.CEM.MemHack.AOBMap.g_AOBs["AOB_pScriptFunctions"], true, true, true).Result.FirstOrDefault();
                //if (addrScriptService > 0)
                //{
                //    var scriptServicePtr = CoreHack.ResolvePtr(addrScriptService, 3).ToString("X8");
                //    Debug.WriteLine(scriptServicePtr);
                //}



                //UIntPtr addr_max = CoreHack.ResolvePtr(AOBMap.g_AOBs["AOB_LUAEngineFuncReg"], 3).ToUIntPtr();
                //UIntPtr addr_item = CoreHack.ResolvePtr(AOBMap.g_AOBs["AOB_LUAEngineFuncReg"], 10).ToUIntPtr();
                //int n_of_funcs = CoreHack.MemLib.readInt(addr_max.ToUInt64().ToString("X8"));

                //int sizeOfStruct = 0x18;
                //for (var i = 0; i < n_of_funcs; i++)
                //{
                //    var offset = i * sizeOfStruct;
                //    UIntPtr addr = addr_item + offset;
                //    LuaCMEngineFuncReg currentF = new LuaCMEngineFuncReg();
                //    var tst = CoreHack.ResolveMultipointer(addr, new List<int>() { 0x0, 0x8, 0xc, 0x10 });
                //    currentF.fName = CoreHack.MemLib.readString(addr.ToUInt64().ToString("X8"));

                //    //logger.Write(LOG_DEBUG,
                //    //    "[LUA] Engine Reg CFuncs: Fname: %s (0x%08llX), nArgs: %d, Unk: %d",
                //    //    currentF->fName, currentF->pCFunc, currentF->nArgs, currentF->Unk
                //    //);
                //    //lua_register(LMain, currentF->fName, (lua_CFunction)currentF->pCFunc);
                //}

            }
        }


        [TestMethod]
        public void LoadFIFALeaguesFromCSV()
        {
            var fifaleagues = FIFALeague.GetFIFALeagues();
        }

        [TestMethod]
        public void SaveSwanseaSponsorsToFile()
        {
            CareerDB1.FIFAUser = new FIFAUsers();
            CareerDB1.FIFAUser.clubteamid = 1960;
            CoreHack coreHack = new CoreHack();
            //coreHack.SaveName = "Test";

            DbDataCache.CreateAllSponsorFiles();

            DbDataCache.SponsorsToTeams.AddRange(
                new List<SponsorsToTeam>() {
                new SponsorsToTeam()
                {
                    TeamId = 1960,
                     IsUserTeam = false,
                      SponsorName = "yobet",
                       SponsorType = eSponsorType.Main,
                        ContractLengthInYears = 12,
                         GameDateStarted = DateTime.Now
                }
                , new SponsorsToTeam()
                {
                    TeamId = 1960,
                    IsUserTeam = false,
                    SponsorName = "lowcostvans",
                    SponsorType = eSponsorType.Travel,
                        ContractLengthInYears = 12,
                         GameDateStarted = DateTime.Now
                }
                , new SponsorsToTeam()
                {
                    TeamId = 1960,
                    IsUserTeam = false,
                    SponsorName = "carling",
                    SponsorType = eSponsorType.Alcohol,
                        ContractLengthInYears = 12,
                         GameDateStarted = DateTime.Now
                }
                , new SponsorsToTeam()
                {
                    TeamId = 1960,
                    IsUserTeam = false,
                    SponsorName = "swanseauniversity",
                    SponsorType = eSponsorType.Training,
                        ContractLengthInYears = 12,
                         GameDateStarted = DateTime.Now
                }
                }
                );

            SponsorsToTeam.SaveAll();
        }
    
    
        [TestMethod]
        public void TestChangeOfSave()
        {
            CEMCore core = new CEMCore();
            for(var i = 0; i < 3; i++)
            {
                var getGameDate = core.CoreHack.GameDate;
                Thread.Sleep(1000);

            }

        }
    }
}


