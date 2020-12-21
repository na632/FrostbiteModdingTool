using FrostyEditor.IO;
using FrostyEditor.Windows;
using FrostySdk;
using Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using v2k4FIFASDKGenerator.BaseInfo;
using v2k4FIFASDKGenerator.Madden21;

namespace v2k4FIFASDKGenerator
{
	//[StructLayout(LayoutKind.Explicit)]
	//class BuildInfo
	//{
	//	[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
	//	static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
	//	//public:
	//	string getBranchName()
 //       {
	//		return "";
 //       }
	//	//  virtual const char* getBranchName();
	//	//virtual const char* getLicenseeId();
	//	//virtual const char* getEngine();
	//	//virtual __int64 getChangelistOne();
	//	//virtual __int64 getChangelistTwo();
	//	//virtual __int64 getFrostbiteChangelist();
	//	//virtual const char* getUsername();
	//	//virtual bool getBool();
	//	//virtual const char* getUsergroup();
	//	//virtual const char* getBuildTime();
	//	//virtual const char* getBuildDate();
	//	//virtual const char* getBuildDateTime();
	//	//virtual const char* getBuildDateTimee();
	//	//virtual const char* getBuildDateTimeee();
	//	//virtual __int64 getUnk();   // nullptr

	//	public static BuildInfo GetInstance()
	//	{
	//		Mem MemLib = new Mem();
	//		MemLib.OpenProcess("Madden21");
	//		var ptrMod = MemLib.modules.FirstOrDefault(x => x.Key == "Engine.BuildInfo.dll");

	//		var getBuildInfo = GetProcAddress(ptrMod.Value, "getBuildInfo");

	//		BuildInfo buildInfo = new BuildInfo();
	//		Marshal.PtrToStructure(ptrMod.Value, buildInfo);
	//		return buildInfo;
	//	}
	//};


	public class BuildSDK2
	{
		Mem MemLib = new Mem();

		bool ResultState = false;

		public async Task<bool> Build()
        {
			var listOfAOBs = new List<string>()
			{
				//"48 8b 05 ?? ?? ?? ?? 48 89 41 08 48 89 0d ?? ?? ?? ?? c3",
				"48 39 35 ? ? ? ? 0f 85 57 01 00 00 48 8b 46 10 48 89 05 ? ? ? ? 48 85 c0"
			};
			foreach (string pattern in listOfAOBs)
			{
				MemLib.OpenProcess("Madden21");
				long location = MemLib.AoBScan(pattern, true, true).Result.FirstOrDefault();
				if (location != 0)
				{
					// C++ RESOLVE OFFSET
					/*
					 __int32 offset	 = *reinterpret_cast<__int32*>( address + 3 );
					__int64 extended = 0;

					extended |= 0xFFFFFFFF;
					extended <<= 32;
					extended |= offset;

					return address + extended + 7 + 0x100000000;
					 */
					// Get first address = IMPORTANT = First Address for madden is 14854BDF0

					// ReadLong // Convert to X2

					// ReadLong // Convert to X2

					// Name is here // ReadNullTerminatedString

					Debug.WriteLine("Original Location/Address :: " + location.ToString("X2"));
					//var readAddr = MemLib.readInt(location.ToString("X2"));
					//Debug.WriteLine("readAddr Address :: " + readAddr.ToString("X2"));
					var offset = location + 3;
					long extended = 0;
					extended |= 0xFFFFFFFF;
					extended <<= 32;
					extended |= offset;

					location = location + extended + 7 + 0x100000000;
					Debug.WriteLine("Offset Location/Address :: " + location.ToString("X2"));
					// 

					string typeStr = "v2k4FIFASDKGenerator.Madden21.ClassInfo";
					MemoryReader memoryReader = new MemoryReader(MemLib.theProc, 0x14854BDF0);

					long typeInfoOffset = memoryReader.Position;
					var classInfos = new List<BaseInfo.ClassInfo>();
					var numberOfTypesFound = 0;
					offset = memoryReader.Position;
					while (offset != 0L)
					{
						numberOfTypesFound++;
						memoryReader.Position = offset + (50 * (numberOfTypesFound - 1));
                        var t = Type.GetType(typeStr);
						BaseInfo.ClassInfo classInfo = (BaseInfo.ClassInfo)Activator.CreateInstance(t);
						classInfo.Read(memoryReader);
						Debug.WriteLine(classInfo.typeInfo.name);
						classInfos.Add(classInfo);
                        //offsetClassInfoMapping.Add(typeInfoOffset, classInfo);
                        if (offset != 0L)
                        {
                            typeInfoOffset = offset;
                        }
                    }


					/*
					 * 
					 * DWORD_PTR dwOffset = *(DWORD*)(dwMatch + 3);

			BYTE* first = (BYTE*)&dwOffset;
			if (first[3] == 0xFF)
				dwOffset = dwOffset + 0xFFFFFFFF00000000;

			DWORD_PTR dwOffset2 = (dwMatch + 7);

			instance = (ClassInfo**)(dwOffset + dwOffset2);
					 * 
					 */

					/*
					 * TypeInfo* typeInfo; //0x0000 
					ClassInfo* next; //0x0008 
					unsigned short id; //0x0010 
					unsigned short isDataContainer; //0x0012 
					char pad_0x0014[0x4]; //0x0014
					ClassInfo* parent; //0x0018 
					char pad_0x0020[0x8]; //0x0020
					unsigned short id3; //0x0028 
					char pad_0x002C[0x94]; //0x002C
					*/

				}
			}
return false;
}


//public async Task<bool> Build()
//{

//	List<SdkUpdateTask> list = new List<SdkUpdateTask>
//	{
//		new SdkUpdateTask
//		{
//			DisplayName = "Waiting for process to become active",
//			Task = OnDetectRunningProcess
//		},
//		new SdkUpdateTask
//		{
//			DisplayName = "Scanning for type info offset",
//			Task = OnFindTypeInfoOffset
//		},
//		new SdkUpdateTask
//		{
//			DisplayName = "Dumping types from memory",
//			Task = OnGatherTypesFromMemory
//		},
//		new SdkUpdateTask
//		{
//			DisplayName = "Cross referencing assets",
//			Task = OnCrossReferenceAssets
//		},
//		new SdkUpdateTask
//		{
//			DisplayName = "Creating SDK",
//			Task = OnCreateSdk
//		}
//	};

//	SdkUpdateState state = new SdkUpdateState();
//	foreach (SdkUpdateTask task in list)
//	{
//		task.State = SdkUpdateTaskState.Active;
//		await Task.Run(delegate
//		{
//			task.Task(task, state);
//		});
//		if (task.State == SdkUpdateTaskState.CompletedFail)
//		{
//			break;
//		}
//	}

//	Debug.WriteLine("Finished");
//	Trace.WriteLine("Finished");
//	Console.WriteLine("Finished");
//	return ResultState;
//}


private bool OnDetectRunningProcess(SdkUpdateTask task, object state)
{
Process process = null;
SdkUpdateState sdkUpdateState = state as SdkUpdateState;
int attemptToFindProcess = 0;
do
{
var allProcesses = Process.GetProcesses();
process = allProcesses.FirstOrDefault(x =>
	x.ProcessName.Contains("FIFA21")
	|| x.ProcessName.ToUpper().Contains("MADDEN21")
	);
if (process != null)
{
string text = process.MainModule?.ModuleName;
MemLib.OpenProcess(process.ProcessName);
}
attemptToFindProcess++;
Thread.Sleep(100);
if (attemptToFindProcess > 5)
{
break;
}
}
while (process == null);

if (process != null)
{
//if (process.TotalProcessorTime < TimeSpan.FromSeconds(5.0))
//{
//	Thread.Sleep(TimeSpan.FromSeconds(5.0) - process.TotalProcessorTime);
//}
sdkUpdateState.Process = process;
task.StatusMessage = process.ProcessName;
task.State = SdkUpdateTaskState.CompletedSuccessful;

ProfilesLibrary.Initialize(process.ProcessName);

Debug.WriteLine($"FIFA Process Found {process.ProcessName}");
Trace.WriteLine($"FIFA Process Found {process.ProcessName}");
Console.WriteLine($"FIFA Process Found {process.ProcessName}");

ResultState = true;
return true;

}

Debug.WriteLine("FIFA Process Not Found");
Trace.WriteLine("FIFA Process Not Found");
Console.WriteLine("FIFA Process Not Found");

task.State = SdkUpdateTaskState.CompletedFail;
ResultState = false;
return false;
}

private bool OnFindTypeInfoOffset(SdkUpdateTask task, object state)
{
SdkUpdateState sdkUpdateState = state as SdkUpdateState;
if (sdkUpdateState.Process != null)
{
long baseAddress = sdkUpdateState.Process.MainModule.BaseAddress.ToInt64();
MemoryReader memoryReader = new MemoryReader(sdkUpdateState.Process, baseAddress);
if (memoryReader == null)
{
task.State = SdkUpdateTaskState.CompletedFail;
return false;
}
// TODO: You need this for FIFA 20 and older

string[] obj = new string[1]
{
//"488b05???????? 48894108 48890d???????? 48???? C3",
//"488b05???????? 48894108 48890d",
//"488b05???????? 488905???????? 488d05???????? 488905???????? E9",
"48 8b 05 ?? ?? ?? ?? 48 89 41 08 48 89 0d ?? ?? ?? ?? c3",
};
IList<long> list = null;
string[] array = obj;
foreach (string pattern in array)
{
memoryReader.Position = baseAddress;
list = MemLib.AoBScan(pattern, true, true).Result.ToList();
//list = memoryReader.scan(pattern);
if (list.Count != 0)
{
	break;
}
}
if (list.Count == 0)
{
task.State = SdkUpdateTaskState.CompletedFail;
task.FailMessage = "Unable to find the first type info offset";
Debug.WriteLine(task.FailMessage);
Trace.WriteLine(task.FailMessage);
Console.WriteLine(task.FailMessage);
return false;
}

memoryReader.Position = list[0] + 3;
int num2 = memoryReader.ReadInt();
memoryReader.Position = list[0] + 3 + num2 + 4;
Debug.WriteLine(memoryReader.Position.ToString("X2"));

var neOffset = memoryReader.ReadLong();
sdkUpdateState.TypeInfoOffset = neOffset;

Debug.WriteLine(sdkUpdateState.TypeInfoOffset.ToString("X"));

//sdkUpdateState.TypeInfoOffset = Convert.ToInt64("145371E58");
task.State = SdkUpdateTaskState.CompletedSuccessful;

task.StatusMessage = string.Format("0x{0}", sdkUpdateState.TypeInfoOffset.ToString("X8"));
Debug.WriteLine(task.StatusMessage);
Trace.WriteLine(task.StatusMessage);
Console.WriteLine(task.StatusMessage);
ResultState = true;
return true;
}

task.State = SdkUpdateTaskState.CompletedFail;
ResultState = false;
return false;
}

private bool OnGatherTypesFromMemory(SdkUpdateTask task, object state)
{
Debug.WriteLine("OnGatherTypesFromMemory");

SdkUpdateState obj = state as SdkUpdateState;
obj.Creator = new v2k4FIFASDKGenerator.ClassesSdkCreator(obj);

bool flag = obj.Creator.GatherTypeInfos(task);
task.State = (flag ? SdkUpdateTaskState.CompletedSuccessful : SdkUpdateTaskState.CompletedFail);

Debug.WriteLine("OnGatherTypesFromMemory:: " + flag);

ResultState = flag;
return flag;
}



private bool OnCrossReferenceAssets(SdkUpdateTask task, object state)
{

Debug.WriteLine("OnCrossReferenceAssets");

bool flag = (state as SdkUpdateState).Creator.CrossReferenceAssets(task);
task.State = (flag ? SdkUpdateTaskState.CompletedSuccessful : SdkUpdateTaskState.CompletedFail);

Debug.WriteLine("OnCrossReferenceAssets:: " + flag);

ResultState = flag;
return flag;
}

private bool OnCreateSdk(SdkUpdateTask task, object state)
{
Debug.WriteLine("OnCreateSdk");

bool flag = (state as SdkUpdateState).Creator.CreateSDK();
task.State = (flag ? SdkUpdateTaskState.CompletedSuccessful : SdkUpdateTaskState.CompletedFail);

Debug.WriteLine("OnCreateSdk:: " + flag);
ResultState = flag;
return flag;
}
}
}
