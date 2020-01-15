using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Memory;

namespace FIFAModdingTestNetFramework
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


                var transferbudget = MemLib.readInt("FIFA20.exe+072BC110,0x18,0x18,0x2A8,0x268,0x8");
                if(transferbudget > 0)
                {
                    MemLib.writeMemory("FIFA20.exe+072BC110,0x18,0x18,0x2A8,0x268,0x8", "int", "999999999");
                }

                var ransferbudget2 = MemLib.readUInt("FIFA20.exe+072BC110,0x8,0x268,0x2A8,0x18,0x18");
                if (transferbudget > 0)
                {

                }

                var startingbudget = MemLib.readInt("FIFA20.exe+06E00C48");
                if (startingbudget > 0)
                {

                }
            }
        }
    }
}
