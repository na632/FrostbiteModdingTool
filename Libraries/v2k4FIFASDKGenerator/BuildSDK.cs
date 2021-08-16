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

		public string ProcessName = ProfilesLibrary.ProfileName;
		public string OverrideProfileName = null;

		public Process GetProcess()
        {
			var allProcesses = Process.GetProcesses();
            //var process = allProcesses.FirstOrDefault(x =>
            //        x.ProcessName.Contains("FIFA18", StringComparison.OrdinalIgnoreCase)
            //        || x.ProcessName.Contains("FIFA19", StringComparison.OrdinalIgnoreCase)
            //        || x.ProcessName.Contains("FIFA20", StringComparison.OrdinalIgnoreCase)
            //        || x.ProcessName.Contains("FIFA21", StringComparison.OrdinalIgnoreCase)
            //        || x.ProcessName.ToUpper().Contains("MADDEN21", StringComparison.OrdinalIgnoreCase)
            //        || x.ProcessName.Contains("bf4", StringComparison.OrdinalIgnoreCase)
            //        || x.ProcessName.Contains("bfv", StringComparison.OrdinalIgnoreCase)
            //        || x.ProcessName.Contains("bf5", StringComparison.OrdinalIgnoreCase)
            //        || x.ProcessName.Contains("bf1", StringComparison.OrdinalIgnoreCase)
            //        );
			var process = allProcesses.FirstOrDefault(x =>
				   (x.ProcessName.Contains("FIFA", StringComparison.OrdinalIgnoreCase)
				   || x.ProcessName.Contains("MADDEN", StringComparison.OrdinalIgnoreCase)
				   || x.ProcessName.Contains("bf", StringComparison.OrdinalIgnoreCase))
				   && !x.ProcessName.Contains("config", StringComparison.OrdinalIgnoreCase)
				   );

			if(process == null)
            {
				var psi = new System.Diagnostics.ProcessStartInfo() { FileName = @"F:\Origin Games\Battlefield 2042 Technical Playtest\bf.exe"
							, UseShellExecute = true
							, Verb = "runas"
				};
				process = System.Diagnostics.Process.Start(psi);
				Thread.Sleep(3000);
			}
			//var process = allProcesses.FirstOrDefault(x => x.ProcessName.Contains(ProcessName, StringComparison.OrdinalIgnoreCase));
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
					//string text = SdkProcess.MainModule?.ModuleName;
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
				ProfilesLibrary.Initialize(OverrideProfileName == null ? SdkProcess.ProcessName : OverrideProfileName);

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
				long baseAddress = 0;
				try
                {
					baseAddress = SdkProcess.MainModule.BaseAddress.ToInt64();
				}
				catch (Exception)
                {

                }
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
                "488b05???????? 48894108 48890d???????? 48???? C3",
                //"488b05???????? 48894108 48890d????????",
                "488b05???????? 488905???????? 488d05???????? 488905???????? E9",

                //"48 39 3d ?? ?? ?? ?? 75 18 48 8b 47 10 48 89 05 ?? ?? ?? ?? 48 85 c0 74 08", // FIFA 21

						// Works for Madden and FIFA 21
				"48 39 1D ?? ?? ?? ?? 75 18 48 8b 43 10", // Madden 21 & FIFA 21


                //"30 40 96 49 01 00 00 00 48 70 12 48 01 00 00 00 D0 6F 12 48 01"

				"488B05???????? 48894108 ?? 488D05???????? 483905???????????? 488B05???????? 488905????????",
            "488B05???????? 48894108 C3 488D05????C5?? 483905???????????? 488B05???????? 488905????????",
            "488B05F6?????? 48894108 C3 488D05????C5?? 483905d3?????????? 488B05????C5?? 488905????????",
            "488b05???????? 48894108 48890d???????? 48???? C3",
            "488b05???????? 48894108 48890d????????",
            "488b05???????? 488905???????? 488d05???????? 488905???????? E9"

                    };

                    if (!string.IsNullOrEmpty(ProfilesLibrary.LoadedProfile.SDKAOBScan))
                    {
						patterns.Insert(0, ProfilesLibrary.LoadedProfile.SDKAOBScan);
						Debug.WriteLine("Attempting to use Profile Pattern :: " + ProfilesLibrary.LoadedProfile.SDKAOBScan);
					}


					if (!string.IsNullOrEmpty(ProfilesLibrary.LoadedProfile.SDKFirstTypeInfo))
					{
						sdkUpdateState.TypeInfoOffset = long.Parse(ProfilesLibrary.LoadedProfile.SDKFirstTypeInfo);

						Debug.WriteLine("Attempting to use Profile TypeInfoOffset :: " + ProfilesLibrary.LoadedProfile.SDKFirstTypeInfo);
					}
					else
					{
						List<long> listOfOffsets = null;

						var selectedPattern = string.Empty;
						foreach (string pattern in patterns)
						{
							memoryReader.Position = baseAddress;
							listOfOffsets = memoryReader.scan(pattern).ToList();
							if (listOfOffsets.Count != 0)
							{
								selectedPattern = pattern;
								break;
							}
						}
						if (listOfOffsets.Count == 0)
							throw new Exception("Unable to find TypeInfo Offset");

						Debug.WriteLine("Used Pattern :: " + selectedPattern);

						//listOfOffsets = listOfOffsets.OrderBy(x => x).ToList();
						memoryReader.Position = listOfOffsets[0] + 3;
						int num = memoryReader.ReadInt();
						memoryReader.Position = listOfOffsets[0] + 3 + num + 4;
						sdkUpdateState.TypeInfoOffset = memoryReader.ReadLong();
					}
				}


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
