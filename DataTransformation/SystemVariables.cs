using System.IO;

namespace Betlln.Data.Integration
{
    public static class SystemVariables
    {
        static SystemVariables()
        {
            Multithreaded = true;
        }

        public static string WorkingFolder
        {
            get { return Path.GetDirectoryName(RuntimeContext.TopAssembly.Location); }
        }

        public static uint TaskTimeout { get; set; }
        public static bool Multithreaded { get; set; }
        public static uint ParallelTimeout { get; set; }

        public static string ApplicationName
        {
            get { return RuntimeContext.ApplicationName; }
        }

        public static string ApplicationVersion
        {
            get { return RuntimeContext.ApplicationVersion.ToString(); }
        }
    }
}