using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Betlln.Logging;
using Betlln.Data.Integration.Core;
using Microsoft.Win32;

namespace Betlln.Data.Integration
{
    public static class Dts
    {
        private static Package _package;
        private static StaticBinder _systemVariables;

        internal static readonly Notify Notify = new Notify();

        public static void Start()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            SystemEvents.SessionEnding += OnWindowsLogout;

            try
            {
                _systemVariables = new StaticBinder(typeof(SystemVariables));
                InitializeProjectParameters();
                LoadFromCommandLine();

                ExecutePackage();
                WaitForParallelTasksToComplete();

                Notify.Console("Success!");
                Environment.ExitCode = 0;
            }
            catch (Exception error)
            {
                Notify.All(error.ToString(), LogEventType.Error);
                Environment.ExitCode = -1;
            }

            Notify.Flush();
            ConsoleScreen.Default.ConfirmExit();
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs eventDetails)
        {
            Debug.Print("Cancel key-press detected.");
            eventDetails.Cancel = true;
            CancelRun();
        }

        private static void OnWindowsLogout(object sender, SessionEndingEventArgs eventDetails)
        {
            Notify.Log("Windows is exiting, attempting to cancel operations.");
            eventDetails.Cancel = true;
            CancelRun();
        }

        private static void CancelRun()
        {
            Notify.Console("Stopping...");
            Task.CancelAll();
        }

        private static void InitializeProjectParameters()
        {
            if (ProjectInfo.Parameters != null)
            {
                ProjectInfo.Parameters.BindDefaultValues();
                ProjectInfo.Parameters.BindFromConfig();
            }
        }

        private static void LoadFromCommandLine()
        {
            int i = 0;
            List<string> arguments = Environment.GetCommandLineArgs().ToList();
            arguments.RemoveAt(0);
            while (i < arguments.Count)
            {
                string valueName = arguments[i].Trim();

                // ReSharper disable once ComplexConditionExpression
                string value = i == arguments.Count - 1 ? string.Empty : arguments[i + 1];

                if (valueName.Equals("--package", StringComparison.InvariantCultureIgnoreCase))
                {
                    SetupPackage(value);
                }
                else if (valueName.Equals("--logger", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] settingParts = value.Split("::".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string connectionName = settingParts.FirstOrDefault();
                    string subTarget = settingParts.Length == 2 ? settingParts[1] : null;

                    Notify.SetLogTarget(connectionName, subTarget);
                }
                else
                {
                    LoadParameterFromCommandLine(valueName, value);
                }

                i += 2;
            }
        }

        private static void SetupPackage(string packageName)
        {
            Type packageType = GetPackageType(packageName);
            _package = (Package) Activator.CreateInstance(packageType);
        }

        private static Type GetPackageType(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                throw new InvalidOperationException("The package name is required.");
            }

            Type packageType = ProjectInfo.GetPackageType(packageName);

            if (packageType == null)
            {
                throw new InvalidOperationException($"Package '{packageName}' cannot be found.");
            }

            return packageType;
        }

        private static void LoadParameterFromCommandLine(string parameterKind, string parameterSpecification)
        {
            int delimiterStart = parameterSpecification.IndexOf('=');
            string parameterName = parameterSpecification.Substring(0, delimiterStart);
            string parameterValue = parameterSpecification.Substring(delimiterStart + 1);

            if (parameterKind.Equals("--system", StringComparison.InvariantCultureIgnoreCase))
            {
                _systemVariables.BindProperty(parameterName, parameterValue);
            }
            else if (parameterKind.Equals("--projectParam", StringComparison.InvariantCultureIgnoreCase))
            {
                ProjectInfo.Parameters.BindProperty(parameterName, parameterValue);
            }
            else if (parameterKind.Equals("--packageParam", StringComparison.InvariantCultureIgnoreCase))
            {
                AssertPackageParametersSetup();
                _package.ParametersBinder.BindProperty(parameterName, parameterValue);
            }
        }

        private static void AssertPackageParametersSetup()
        {
            if (_package == null)
            {
                throw new InvalidOperationException("The package name must be specified first.");
            }
            if (_package.ParametersBinder == null)
            {
                throw new InvalidOperationException($"The package {_package.GetType().Name} does not take any parameters.");
            }
        }

        private static void ExecutePackage()
        {
            if (_package == null)
            {
                throw new InvalidOperationException("No package was found to run.");
            }

            Notify.All($"Starting project {ProjectInfo.Name}{Environment.NewLine}{_systemVariables}{Environment.NewLine}{ProjectInfo.Parameters}");

            _package.Run();
        }

        public static void WaitForParallelTasksToComplete()
        {
            Task.WaitAll();
        }

        /// <summary>
        /// This class and its methods are only retained for backwards compatibility
        /// </summary>
        public static class Events
        {
            // ReSharper disable UnusedMember.Global
            // ReSharper disable TooManyArguments
            // ReSharper disable FlagArgument

            [Obsolete("Use Dts.Events.RaiseInformation instead", true)]
            public static void FireInformation(int informationCode, string subComponent, string description, string helpFile, int helpContext, ref bool fireAgain)
            {
                if (!fireAgain)
                {
                    Debug.Print("Fire Again suppression is not honored.");
                }

                Notify.Log(BuildLegacyMessage(informationCode, subComponent, description, helpFile, helpContext));
            }

            [Obsolete("Use Dts.Events.RaiseError instead", true)]
            public static void FireError(int informationCode, string subComponent, string description, string helpFile, int helpContext)
            {
                Notify.Log(BuildLegacyMessage(informationCode, subComponent, description, helpFile, helpContext), LogEventType.Error);
            }

            // ReSharper restore FlagArgument
            // ReSharper restore TooManyArguments
            // ReSharper restore UnusedMember.Global

            private static string BuildLegacyMessage(int informationCode, string subComponent, string description, string helpFile, int helpContext)
            {
                return $"{description}{Environment.NewLine}Component = {subComponent}, Help File = {helpFile} Information Code = {informationCode}, Help Code = {helpContext}";
            }

            public static void RaiseInformation(string message)
            {
                Notify.All(message);
            }

            public static void RaiseError(string message)
            {
                throw new Exception(message);
            }
        }
    }
}