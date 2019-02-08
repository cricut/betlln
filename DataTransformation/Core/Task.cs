using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Betlln.Logging;

namespace Betlln.Data.Integration.Core
{
    public abstract class Task : IDisposable
    {
        protected Task()
        {
            Timeout = SystemVariables.TaskTimeout;
        }

        public uint Timeout { get; set; }

        public void Run()
        {
            Dts.Notify.Log($"Started {GetType().Name}", LogEventType.Debug);
            PreExecute();
            ExecuteTasks();
            Dispose();
        }

        private static readonly CancellationTokenSource Canceler = new CancellationTokenSource();
        private static readonly List<System.Threading.Tasks.Task> AllTasks = new List<System.Threading.Tasks.Task>();

        public void RunParallel()
        {
            if (SystemVariables.Multithreaded)
            {
                AllTasks.Add(System.Threading.Tasks.Task.Factory.StartNew(RunBackground, Canceler.Token));
            }
            else
            {
                Run();
            }
        }

        private void RunBackground()
        {
            try
            {
                Run();
            }
            catch (Exception taskError)
            {
                Canceler.Cancel();
                Dts.Notify.All(taskError.ToString(), LogEventType.Error);
                throw;
            }
        }

        internal static void WaitAll()
        {
            TimeSpan maximumWaitTime;
            if (SystemVariables.ParallelTimeout != 0)
            {
                maximumWaitTime = TimeSpan.FromMinutes(SystemVariables.ParallelTimeout);
                if (AllTasks.Any())
                {
                    Dts.Notify.Log($"Will wait for no more than {maximumWaitTime.TotalMinutes} minutes");
                }
            }
            else
            {
                maximumWaitTime = TimeSpan.MaxValue;
            }

            bool allThreadsCompleted = System.Threading.Tasks.Task.WaitAll(AllTasks.ToArray(), maximumWaitTime);
            if (!allThreadsCompleted)
            {
                throw new TimeoutException();
            }

            AllTasks.Clear();
        }

        protected virtual void PreExecute()
        {
        }

        protected abstract void ExecuteTasks();

        public static void CancelAll()
        {
            Canceler.Cancel();
        }

        public virtual void Dispose()
        {
        }
    }
}