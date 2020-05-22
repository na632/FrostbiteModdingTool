using FrostyEditor.IO;
using FrostyEditor.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FIFAModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for BuildSDKAndCache.xaml
    /// </summary>
    public partial class BuildSDKAndCache : Window
    {
        public BuildSDKAndCache()
        {
            InitializeComponent();


        }

        private async void btnRunBuild_Click(object sender, RoutedEventArgs e)
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
			tasksListBox.ItemsSource = list;
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
					//failedTask = task;
					break;
				}
			}
			//successMessage.Visibility = ((failedTask != null) ? Visibility.Collapsed : Visibility.Visible);
			//failMessage.Text = ((failedTask != null) ? failedTask.FailMessage : "");
			//finishButton.IsEnabled = true;
		}

		private bool OnDetectRunningProcess(SdkUpdateTask task, object state)
		{
			Process process = null;
			SdkUpdateState sdkUpdateState = state as SdkUpdateState;
			do
			{
				Process[] processes = Process.GetProcesses().Where(x => x.ProcessName.Contains("FIFA20")).ToArray();
				foreach (Process process2 in processes)
				{
					
					string text = process2.MainModule?.ModuleName;
					process = process2;
					
				}
			}
			while (process == null);
			if (process.TotalProcessorTime < TimeSpan.FromSeconds(5.0))
			{
				Thread.Sleep(TimeSpan.FromSeconds(5.0) - process.TotalProcessorTime);
			}
			sdkUpdateState.Process = process;
			task.StatusMessage = process.ProcessName;
			task.State = SdkUpdateTaskState.CompletedSuccessful;
			return true;
		}

		private bool OnFindTypeInfoOffset(SdkUpdateTask task, object state)
		{
			SdkUpdateState sdkUpdateState = state as SdkUpdateState;
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
				return false;
			}
			memoryReader.Position = list[0] + 3;
			int num2 = memoryReader.ReadInt();
			memoryReader.Position = list[0] + 3 + num2 + 4;
			sdkUpdateState.TypeInfoOffset = memoryReader.ReadLong();
			task.State = SdkUpdateTaskState.CompletedSuccessful;
			task.StatusMessage = string.Format("0x{0}", sdkUpdateState.TypeInfoOffset.ToString("X8"));
			return true;
		}

		private bool OnGatherTypesFromMemory(SdkUpdateTask task, object state)
		{
			SdkUpdateState obj = state as SdkUpdateState;
			obj.Creator = new v2k4FIFASDKGenerator.ClassesSdkCreator(obj);
			bool flag = obj.Creator.GatherTypeInfos(task);
			task.State = (flag ? SdkUpdateTaskState.CompletedSuccessful : SdkUpdateTaskState.CompletedFail);
			return flag;
		}

		private bool OnCrossReferenceAssets(SdkUpdateTask task, object state)
		{
			bool flag = (state as SdkUpdateState).Creator.CrossReferenceAssets(task);
			task.State = (flag ? SdkUpdateTaskState.CompletedSuccessful : SdkUpdateTaskState.CompletedFail);
			return flag;
		}

		private bool OnCreateSdk(SdkUpdateTask task, object state)
		{
			bool flag = (state as SdkUpdateState).Creator.CreateSDK();
			task.State = (flag ? SdkUpdateTaskState.CompletedSuccessful : SdkUpdateTaskState.CompletedFail);
			return flag;
		}
	}
}
