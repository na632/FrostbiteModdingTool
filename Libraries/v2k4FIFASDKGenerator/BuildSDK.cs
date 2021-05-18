using Frosty;
using FrostyEditor.IO;
using FrostyEditor.Windows;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SdkGenerator
{
    public class BuildSDK
    {

        private ILogger logger;

        public ILogger Logger
        {
            get { 
				if(logger== null)
                {
					logger = new NullLogger();
                }
				return logger
					
					; }
            set { logger = value; }
        }

		public BuildSDK(ILogger inLogger = null)
        {
			if(inLogger != null)
            {
				logger = inLogger;
            }
        }

		public Process GetProcess()
        {
			var allProcesses = Process.GetProcesses();
			var process = allProcesses.FirstOrDefault(x =>
					x.ProcessName.Contains("FIFA18")
					|| x.ProcessName.Contains("FIFA19")
					|| x.ProcessName.Contains("FIFA20")
					|| x.ProcessName.Contains("FIFA21")
					|| x.ProcessName.ToUpper().Contains("MADDEN21")
					|| x.ProcessName.Contains("bf4")
					);
			return process;
		}

		public bool WaitForMainMenu = false;

        Process SdkProcess = null;
		bool ResultState = false;
		public async Task<bool> Build()
        {
			int attemptToFindProcess = 0;
			do
			{
				SdkProcess = GetProcess();
			
				if (SdkProcess != null)
				{
					string text = SdkProcess.MainModule?.ModuleName;
				}
				attemptToFindProcess++;
				await Task.Delay(1000);
				if (attemptToFindProcess > 60)
				{
					break;
				}
			}
			while (SdkProcess == null);

			if (SdkProcess != null)
			{
				ProfilesLibrary.Initialize(SdkProcess.ProcessName);

				Debug.WriteLine($"Process Found {SdkProcess.ProcessName}");
				Trace.WriteLine($"Process Found {SdkProcess.ProcessName}");
				Console.WriteLine($"Process Found {SdkProcess.ProcessName}");

				Logger.Log($"Process Found {SdkProcess.ProcessName}");

				if(WaitForMainMenu)
					await Task.Delay(5000);
			}
			else
			{

				Debug.WriteLine("Process Not Found");
				Trace.WriteLine("Process Not Found");
				Console.WriteLine("Process Not Found");

				Logger.LogError($"Process Not Found");


				ResultState = false;
				return false;
			}

			List<SdkUpdateTask> list = new List<SdkUpdateTask>
			{
				//new SdkUpdateTask
				//{
				//	DisplayName = "Waiting for process to become active",
				//	Task = OnDetectRunningProcess
				//},
				new SdkUpdateTask
				{
					DisplayName = "Scanning for type info offset",
					Task = OnFindTypeInfoOffset
				},
				new SdkUpdateTask
				{
					DisplayName = "Dumping types from memory",
					Task = OnGatherTypesFromMemory
				},
				new SdkUpdateTask
				{
					DisplayName = "Cross referencing assets",
					Task = OnCrossReferenceAssets
				},
				new SdkUpdateTask
				{
					DisplayName = "Creating SDK",
					Task = OnCreateSdk
				}
			};

			SdkUpdateState state = new SdkUpdateState();
			foreach (SdkUpdateTask task in list)
			{
				task.State = SdkUpdateTaskState.Active;
				await Task.Run(delegate
				{
					task.Task(task, state);
				});
				if (task.State == SdkUpdateTaskState.CompletedFail)
				{
					break;
				}
			}

			Debug.WriteLine("Finished");
			Trace.WriteLine("Finished");
			Console.WriteLine("Finished");
			Logger.Log("Finished SDK Build");
			return ResultState;
		}

		private bool OnFindTypeInfoOffset(SdkUpdateTask task, object state)
		{
			SdkUpdateState sdkUpdateState = state as SdkUpdateState;
			if (SdkProcess != null)
			{
				long baseAddress = SdkProcess.MainModule.BaseAddress.ToInt64();
				MemoryReader memoryReader = new MemoryReader(SdkProcess, baseAddress);
				sdkUpdateState.Process = SdkProcess;
				if (memoryReader == null)
				{
					task.State = SdkUpdateTaskState.CompletedFail;
					return false;
				}
				//if (process.ProcessName.ToUpper() == "MADDEN21")
    //            {
				//	//sdkUpdateState.TypeInfoOffset = 0x14854BDF0;
				//	// "48 39 1D ?? ?? ?? ?? 75 18 48 8b 43 10" // Madden 21
				//	sdkUpdateState.TypeInfoOffset = 0x1483E1760;

				//}
                //else
                //if (process.ProcessName.ToUpper() == "FIFA21")
                //{
                //    //sdkUpdateState.TypeInfoOffset = 0x14854BDF0;
                //    //sdkUpdateState.TypeInfoOffset = 0x1498D7290;
                //    sdkUpdateState.TypeInfoOffset = 0x149B54680;
                //    // AOB 48 39 3d ?? ?? ?? ?? 75 18 48 8b 47 10 48 89 05 ?? ?? ?? ?? 48 85 c0 74 08

                //}
                else
				{


					// TODO: You need this for FIFA 20 and older

					List<string> patterns = new List<string>()
					{
    //            "488b05???????? 48894108 48890d???????? 48???? C3",
    //            "488b05???????? 48894108 48890d????????",
    //            "488b05???????? 488905???????? 488d05???????? 488905???????? E9",

                //"48 39 3d ?? ?? ?? ?? 75 18 48 8b 47 10 48 89 05 ?? ?? ?? ?? 48 85 c0 74 08", // FIFA 21
				"48 39 1D ?? ?? ?? ?? 75 18 48 8b 43 10", // Madden 21 & FIFA 21


                //"30 40 96 49 01 00 00 00 48 70 12 48 01 00 00 00 D0 6F 12 48 01"

				//"488B05???????? 48894108 ?? 488D05???????? 483905???????????? 488B05???????? 488905????????",
			//"488B05???????? 48894108 C3 488D05????C5?? 483905???????????? 488B05???????? 488905????????",
			//"488B05F6?????? 48894108 C3 488D05????C5?? 483905d3?????????? 488B05????C5?? 488905????????",
			//"488b05???????? 48894108 48890d???????? 48???? C3",
			//"488b05???????? 48894108 48890d????????",
			//"488b05???????? 488905???????? 488d05???????? 488905???????? E9"

					};

					List<long> listOfOffsets = null;

					foreach (string pattern in patterns)
					{
						memoryReader.Position = baseAddress;
						listOfOffsets = memoryReader.scan(pattern).ToList();
						if (listOfOffsets.Count != 0)
						{
							break;
						}
					}
					//if (longList == null || longList.Count == 0)
					//{
					//	offset = 0L;
					//	return false;
					//}
					listOfOffsets = listOfOffsets.OrderByDescending(x => x).ToList();
					memoryReader.Position = listOfOffsets[0] + 3;
					int num = memoryReader.ReadInt();
					memoryReader.Position = listOfOffsets[0] + 3 + num + 4;
					sdkUpdateState.TypeInfoOffset = memoryReader.ReadLong();

					//List<long> list = null;
					//foreach (string pattern in patterns)
					//{
					//	memoryReader.Position = baseAddress;
					//	list = memoryReader.scan(pattern).ToList();
					//	if (list.Count != 0)
					//	{
					//		break;
					//	}
					//}
					//if (list.Count == 0)
					//{
					//	task.State = SdkUpdateTaskState.CompletedFail;
					//	task.FailMessage = "Unable to find the first type info offset";
					//	Debug.WriteLine(task.FailMessage);
					//	Trace.WriteLine(task.FailMessage);
					//	Console.WriteLine(task.FailMessage);
					//	return false;
					//}
					//list.Sort();


					//               memoryReader.Position = list.First() + 3;
					//               int off = memoryReader.ReadInt();
					//               memoryReader.Position = list.First() + off + 7;


					//               long neOffset = memoryReader.ReadLong();

					//               Debug.WriteLine(neOffset.ToString("X2"));


					//sdkUpdateState.TypeInfoOffset = neOffset;
				}


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
            obj.Creator = new SdkGenerator.ClassesSdkCreator(obj);
            //obj.Creator = new SdkGenerator.ClassesSdkCreator(SdkProcess, obj.TypeInfoOffset);

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
            //bool flag = (state as SdkUpdateState).Creator.CreateSDK((int)AssetManager.Instance.fs.Head, ProfilesLibrary.SDKFilename, AssetManager.Instance.fs);
            task.State = (flag ? SdkUpdateTaskState.CompletedSuccessful : SdkUpdateTaskState.CompletedFail);

			Debug.WriteLine("OnCreateSdk:: " + flag);
			ResultState = flag;
			return flag;
		}
	}
}
