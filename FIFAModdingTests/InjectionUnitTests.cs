using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Memory;
using v2k4FIFAModdingCL.MemHack.Core;
using v2k4FIFAModdingCL.MemHack.Career;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FIFAModdingTests
{
    [TestClass]
    public class InjectionUnitTests
    {
        public Mem MemLib = new Mem();
        [TestMethod]
        public void TestInject()
        {
            int gameProcId = MemLib.getProcIDFromName("FIFA20"); //use task manager to find game name. For CoD MW3 it is iw5sp. Do not add .exe extension

            if (gameProcId != 0)
            {
                MemLib.OpenProcess(gameProcId);
                if(MemLib.getMinAddress() > 0)
                {
                    var u = MemLib.AoBScan("?? ?? ?? ?? C0 F6 15 01", true, true).Result.FirstOrDefault().ToString("X8");
                    var i0 = MemLib.readInt(u);
                    if (i0 > 0)
                    {

                    }
                }

                ////SigScanSharp sigScan = new SigScanSharp(MemLib.theProc.Handle);
                ////sigScan.SelectModule(MemLib.theProc.MainModule);
                ////var offset = sigScan.FindPattern("? ? ? ? C0 F6 15 01", out long time);
                ////if (offset > 0)
                ////{

                ////}


                //var u = MemLib.AoBScan("FIFA20.exe",                     "?? ?? ?? ?? C0 F6 15 01", true, true).Result.FirstOrDefault();
                //var i0 = MemLib.readInt("FIFA20.exe+" + u.ToString());
                //if (i0 > 0)
                //{

                //}
                //MemLib.writeMemory(u.ToString(), "int", "9999");


                //var transferbudget = MemLib.readInt("FIFA20.exe+072BC110,0x18,0x18,0x2A8,0x268,0x8");
                //if (transferbudget > 0)
                //{
                //    MemLib.writeMemory("FIFA20.exe+072BC110,0x18,0x18,0x2A8,0x268,0x8", "int", "999999999");
                //}

            }

            //Finances.SetTransferBudget(999999999);
            //Manager.SetManagerRating(99);


        }


    }

}

/*
 * Name: SigScanSharp
 * Author: Striekcarl/GENESIS @ Unknowncheats
 * Date: 14/05/2017
 * Purpose: Find memory patterns, both individually or simultaneously, as fast as possible
 * 
 * Example:
 *  Init:
 *      Process TargetProcess = Process.GetProcessesByName("TslGame")[0];
 *      SigScanSharp Sigscan = new SigScanSharp(TargetProcess.Handle);
 *      Sigscan.SelectModule(procBattlegrounds.MainModule);
 * 
 *  Find Patterns (Simultaneously):
 *      Sigscan.AddPattern("Pattern1", "48 8D 0D ? ? ? ? E8 ? ? ? ? E8 ? ? ? ? 48 8B D6");
 *      Sigscan.AddPattern("Pattern2", "E8 0A EC ? ? FF");
 *      
 *      long lTime;
 *      var result = Sigscan.FindPatterns(out lTime);
 *      var offset = result["Pattern1"];
 *      
 *  Find Patterns (Individual):
 *      long lTime;
 *      var offset = Sigscan.FindPattern("48 8D 0D ? ? ? ? E8 ? ? ? ? E8 ? ? ? ? 48 8B D6", out lTime);
 * 
 */
public class SigScanSharp
{
    private IntPtr g_hProcess { get; set; }
    private byte[] g_arrModuleBuffer { get; set; }
    private ulong g_lpModuleBase { get; set; }

    private Dictionary<string, string> g_dictStringPatterns { get; }

    public SigScanSharp(IntPtr hProc)
    {
        g_hProcess = hProc;
        g_dictStringPatterns = new Dictionary<string, string>();
    }

    public bool SelectModule(ProcessModule targetModule)
    {
        g_lpModuleBase = (ulong)targetModule.BaseAddress;
        g_arrModuleBuffer = new byte[targetModule.ModuleMemorySize];

        g_dictStringPatterns.Clear();

        return Win32.ReadProcessMemory(g_hProcess, g_lpModuleBase, g_arrModuleBuffer, targetModule.ModuleMemorySize);
    }

    public void AddPattern(string szPatternName, string szPattern)
    {
        g_dictStringPatterns.Add(szPatternName, szPattern);
    }

    private bool PatternCheck(int nOffset, byte[] arrPattern)
    {
        for (int i = 0; i < arrPattern.Length; i++)
        {
            if (arrPattern[i] == 0x0)
                continue;

            if (arrPattern[i] != this.g_arrModuleBuffer[nOffset + i])
                return false;
        }

        return true;
    }

    public ulong FindPattern(string szPattern, out long lTime)
    {
        if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
            throw new Exception("Selected module is null");

        Stopwatch stopwatch = Stopwatch.StartNew();

        byte[] arrPattern = ParsePatternString(szPattern);

        for (int nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
        {
            if (this.g_arrModuleBuffer[nModuleIndex] != arrPattern[0])
                continue;

            if (PatternCheck(nModuleIndex, arrPattern))
            {
                lTime = stopwatch.ElapsedMilliseconds;
                return g_lpModuleBase + (ulong)nModuleIndex;
            }
        }

        lTime = stopwatch.ElapsedMilliseconds;
        return 0;
    }
    public Dictionary<string, ulong> FindPatterns(out long lTime)
    {
        if (g_arrModuleBuffer == null || g_lpModuleBase == 0)
            throw new Exception("Selected module is null");

        Stopwatch stopwatch = Stopwatch.StartNew();

        byte[][] arrBytePatterns = new byte[g_dictStringPatterns.Count][];
        ulong[] arrResult = new ulong[g_dictStringPatterns.Count];

        // PARSE PATTERNS
        for (int nIndex = 0; nIndex < g_dictStringPatterns.Count; nIndex++)
            arrBytePatterns[nIndex] = ParsePatternString(g_dictStringPatterns.ElementAt(nIndex).Value);

        // SCAN FOR PATTERNS
        for (int nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++)
        {
            for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
            {
                if (arrResult[nPatternIndex] != 0)
                    continue;

                if (this.PatternCheck(nModuleIndex, arrBytePatterns[nPatternIndex]))
                    arrResult[nPatternIndex] = g_lpModuleBase + (ulong)nModuleIndex;
            }
        }

        Dictionary<string, ulong> dictResultFormatted = new Dictionary<string, ulong>();

        // FORMAT PATTERNS
        for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
            dictResultFormatted[g_dictStringPatterns.ElementAt(nPatternIndex).Key] = arrResult[nPatternIndex];

        lTime = stopwatch.ElapsedMilliseconds;
        return dictResultFormatted;
    }

    private byte[] ParsePatternString(string szPattern)
    {
        List<byte> patternbytes = new List<byte>();

        foreach (var szByte in szPattern.Split(' '))
            patternbytes.Add(szByte == "?" ? (byte)0x0 : Convert.ToByte(szByte, 16));

        return patternbytes.ToArray();
    }

    private static class Win32
    {
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, int dwSize, int lpNumberOfBytesRead = 0);
    }
}
