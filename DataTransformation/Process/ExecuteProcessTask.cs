using System;
using System.Diagnostics;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Process
{
    public class ExecuteProcessTask : Task
    {
        public ExecuteProcessTask()
        {
            StandardOutput = string.Empty;
            StandardError = string.Empty;
            StartInfo = new ProcessStartInfo();
        }

        public ProcessStartInfo StartInfo { get; }
        public string StandardOutput { get; private set; }
        public string StandardError { get; private set; }

        public string AllOutput
        {
            get
            {
                return string.Join(Environment.NewLine, StandardOutput ?? string.Empty, StandardError ?? string.Empty);
            }
        }

        protected override void ExecuteTasks()
        {
            StartInfo.CreateNoWindow = StartInfo.WindowStyle != ProcessWindowStyle.Hidden;
            StartInfo.UseShellExecute = !StartInfo.RedirectStandardError;

            System.Diagnostics.Process process = new System.Diagnostics.Process {StartInfo = StartInfo};
            process.OutputDataReceived += OnOutputDataReceived;
            process.ErrorDataReceived += OnErrorDataReceived;

            process.Start();
            if (StartInfo.RedirectStandardOutput)
            {
                process.BeginOutputReadLine();
            }
            if (StartInfo.RedirectStandardError)
            {
                process.BeginErrorReadLine();
            }

            process.WaitForExit(ProcessTimeout);

            if (process.ExitCode != 0)
            {
                throw new Exception("Process did not exit with 0.");
            }
        }

        private int ProcessTimeout
        {
            get
            {
                return Timeout != 0
                    ? (int) TimeSpan.FromMinutes(Timeout).TotalMilliseconds
                    : int.MaxValue;
            }
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs eventDetails)
        {
            System.Diagnostics.Process parent = sender as System.Diagnostics.Process;
            if (parent != null && parent.StartInfo.RedirectStandardOutput)
            {
                Debug.Print(eventDetails.Data);
                StandardOutput += eventDetails.Data;
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs eventDetails)
        {
            System.Diagnostics.Process parent = sender as System.Diagnostics.Process;
            if (parent != null && parent.StartInfo.RedirectStandardError)
            {
                Debug.Print(eventDetails.Data);
                StandardError += eventDetails.Data;
            }
        }
    }
}