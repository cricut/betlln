using System;
using System.Linq;
using System.Reflection;

namespace Betlln.Data.Integration.Core
{
    public static class ProjectInfo
    {
        static ProjectInfo()
        {
            Parameters = GetBinder("ProjectParameters");
            ConnectionManagers = GetBinder("ConnectionManagers");
        }

        public static string Name => RuntimeContext.ApplicationName;
        internal static StaticBinder Parameters { get; }
        internal static StaticBinder ConnectionManagers { get; }
        private static Type[] ProjectTypes => Assembly.GetEntryAssembly().GetTypes();

        internal static Type GetPackageType(string packageName)
        {
            return GetTypeByName(packageName);
        }

        private static StaticBinder GetBinder(string typeName)
        {
            Type type = GetTypeByName(typeName);
            return type != null ? new StaticBinder(type) : null;
        }

        private static Type GetTypeByName(string typeName)
        {
            return ProjectTypes.FirstOrDefault(x => x.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}