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
		bool ResultState = false;
        public async Task<bool> Build()
        {
			List<SdkUpdateTask> list = new List<SdkUpdateTask>
			{
				new SdkUpdateTask
				{
					DisplayName = "Waiting for process to become active",
					Task = OnDetectRunningProcess
				},
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


		private bool OnDetectRunningProcess(SdkUpdateTask task, object state)
		{
			Process process = null;
			SdkUpdateState sdkUpdateState = state as SdkUpdateState;
			int attemptToFindProcess = 0;
			do
			{
				var allProcesses = Process.GetProcesses();
				Process[] processes = allProcesses.Where(x =>
						x.ProcessName.Contains("FIFA18")
						|| x.ProcessName.Contains("FIFA19")
						|| x.ProcessName.Contains("FIFA20")
						|| x.ProcessName.Contains("FIFA21")
						|| x.ProcessName.Contains("bf4")
						).ToArray();
				foreach (Process process2 in processes)
				{

					string text = process2.MainModule?.ModuleName;
					process = process2;

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
				long num = sdkUpdateState.Process.MainModule.BaseAddress.ToInt64();
				MemoryReader memoryReader = new MemoryReader(sdkUpdateState.Process, num);
				if (memoryReader == null)
				{
					task.State = SdkUpdateTaskState.CompletedFail;
					return false;
				}
				string[] obj = new string[3]
				{
				"488b05???????? 48894108 48890d???????? 48???? C3",
				"488b05???????? 48894108 48890d????????",
				"488b05???????? 488905???????? 488d05???????? 488905???????? E9"
				};
				IList<long> list = null;
				string[] array = obj;
				foreach (string pattern in array)
				{
					memoryReader.Position = num;
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
				sdkUpdateState.TypeInfoOffset = memoryReader.ReadLong();
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
