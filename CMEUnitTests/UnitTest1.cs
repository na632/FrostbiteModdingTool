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

namespace CMEUnitTests
{
    

    [TestClass]
    public class UnitTest1
    {
        //[DllImport("paulv2k4HackHelpers.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern ulong ResolvePtr(UIntPtr address, int offsetpos);

        [DllImport("v2k4InteropHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShowAMessageBox(string message);

        [DllImport("v2k4InteropHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InjectTheDLL();

        [DllImport("v2k4InteropHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EjectTheDLL();

        [DllImport(@"v2k4InteropHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsInCM();



        [TestMethod]
        public void AttachInteropHelper()
        {
            var proc = CoreHack.GetProcess();
            if (proc.HasValue)
            {
                //if (File.Exists("v2k4InteropHelper.dll"))
                //    CoreHack.MemLib.InjectDLL("v2k4InteropHelper.dll");
                //Assembly.Load()
                var dllpath = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\v2k4InteropHelper.dll";
                //var bl = new Bleak.Injector("FIFA20", @"G:\Work\FIFA Modding\FIFAModdingUI\v2k4InteropHelper\x64\Debug\v2k4InteropHelper.dll", Bleak.InjectionMethod.CreateThread, Bleak.InjectionFlags.None);
                var bl = new Bleak.Injector("FIFA20", dllpath, Bleak.InjectionMethod.CreateThread, Bleak.InjectionFlags.None);
                bl.InjectDll();
                //bl.EjectDll();
                //Thread checkThread = new Thread(() => {
                while (true)
                {
                    Thread.Sleep(2000);
                    //if (IsInCM() == 1)
                    //{
                    //    Debug.WriteLine("Well fuck me sideways, that worked!!");
                    //}
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
            //ShowAMessageBox("tits!");

            var proc = CoreHack.GetProcess();
            if(proc.HasValue)
            {

                //var codeGameDB_addy = CoreHack.MemLib.AoBScan("44 8B 7A 34 44 8B 62 38", true, true).Result.FirstOrDefault();
                var codeGameDB_addy = CoreHack.MemLib.AoBScan(long.Parse(CoreHack.MemLib.getMinAddress().ToString()), 0x00007ffffffeffff, "4C 0F 44 35 ?? ?? ?? ?? 41 8B 4E 08", true, true, true).Result.FirstOrDefault();
                if (codeGameDB_addy > 0)
                {
                    
                    var newptr = CoreHack.ResolvePtr(codeGameDB_addy, 4).ToString("X8");
                    Debug.WriteLine(newptr);
                }
            }
        }

        [TestMethod]
        public unsafe void LoadFIFALUAAndRunScript()
        {
            var proc = CoreHack.GetProcess();
            if (proc.HasValue)
            {

                //var codeGameDB_addy = CoreHack.MemLib.AoBScan("44 8B 7A 34 44 8B 62 38", true, true).Result.FirstOrDefault();
                var addrGlobal = CoreHack.MemLib.AoBScan(CareerExpansionMod.CEM.MemHack.AOBMap.g_AOBs["AOB_pGlobal"], true, true, true).Result.FirstOrDefault();
                if (addrGlobal > 0)
                {
                    UIntPtr pScriptFunctions = CoreHack.ResolvePtr(CareerExpansionMod.CEM.MemHack.AOBMap.g_AOBs["AOB_pScriptFunctions"], 61).ToUIntPtr();
                    Debug.WriteLine(pScriptFunctions.ToUInt64().ToString("X8"));

                    //var ScriptFunctions = Cast.Reinterpret<ScriptFunctions, UIntPtr>(pScriptFunctions);


                    var globalPtr = CoreHack.ResolvePtr(addrGlobal, 3);
                    Debug.WriteLine(globalPtr.ToString("X8"));
                    var ptrglobalPtr = new UIntPtr(Convert.ToUInt64(globalPtr));
                    Globals G = new Globals();
                    G.child = new GlobalsChild();
                    G.child.fnUnk1 = ptrglobalPtr.ToPointer();
                    G.child.fnGetPtr = (ptrglobalPtr+30).ToPointer();
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


