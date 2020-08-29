using FrostyEditor.IO;
using FrostyEditor.Windows;
using FrostySdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace v2k4FIFASDKGenerator
{
    public class BuildSDK
    {
		Process process = null;
		bool ResultState = false;
		public async Task<bool> Build()
        {
			int attemptToFindProcess = 0;
			do
			{
				var allProcesses = Process.GetProcesses();
				process = allProcesses.FirstOrDefault(x =>
						x.ProcessName.Contains("FIFA18")
						|| x.ProcessName.Contains("FIFA19")
						|| x.ProcessName.Contains("FIFA20")
						|| x.ProcessName.Contains("FIFA21")
						|| x.ProcessName.ToUpper().Contains("MADDEN21")
						|| x.ProcessName.Contains("bf4")
						);
                //if (process.ProcessName.ToUpper() == "MADDEN21")
                //{
                //    var result = await new BuildSDK2().Build();
                //    return result;
                //}
                string text = process.MainModule?.ModuleName;

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
				ProfilesLibrary.Initialize(process.ProcessName);

				Debug.WriteLine($"Process Found {process.ProcessName}");
				Trace.WriteLine($"Process Found {process.ProcessName}");
				Console.WriteLine($"Process Found {process.ProcessName}");
			}
			else
			{

				Debug.WriteLine("Process Not Found");
				Trace.WriteLine("Process Not Found");
				Console.WriteLine("Process Not Found");

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
			return ResultState;
		}

		private bool OnFindTypeInfoOffset(SdkUpdateTask task, object state)
		{
			SdkUpdateState sdkUpdateState = state as SdkUpdateState;
			if (process != null)
			{
				long baseAddress = process.MainModule.BaseAddress.ToInt64();
				MemoryReader memoryReader = new MemoryReader(process, baseAddress);
				sdkUpdateState.Process = process;
				if (memoryReader == null)
				{
					task.State = SdkUpdateTaskState.CompletedFail;
					return false;
				}
				if (process.ProcessName.ToUpper() != "MADDEN21")
				{


					// TODO: You need this for FIFA 20 and older

					string[] obj = new string[4]
					{
				"488b05???????? 48894108 48890d???????? 48???? C3",
				"488b05???????? 48894108 48890d????????",
				"488b05???????? 488905???????? 488d05???????? 488905???????? E9",
				"48 39 35 ? ? ? ? 0f 85 57 01 00 00 48 8b 46 10 48 89 05 ? ? ? ? 48 85 c0",
					};
					IList<long> list = null;
					string[] array = obj;
					foreach (string pattern in array)
					{
						memoryReader.Position = baseAddress;
						list = memoryReader.scan(pattern);
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

					long neOffset = memoryReader.ReadLong();


					sdkUpdateState.TypeInfoOffset = neOffset;
				}
				else
                {
					sdkUpdateState.TypeInfoOffset = 0x14854BDF0;

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
