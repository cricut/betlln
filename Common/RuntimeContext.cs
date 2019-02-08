using System;
using System.Linq;
using System.Reflection;
using Betlln.Logging;

namespace Betlln
{
    public static class RuntimeContext
    {
        private static Assembly _topAssembly;
        private static string _applicationName;
        private static Version _applicationVersion;
        private static Logger _defaultLogger;

        static RuntimeContext()
        {
            TopAssembly = Assembly.GetEntryAssembly();
        }

        public static Assembly TopAssembly
        {
            get
            {
                return _topAssembly;
            }
            set
            {
                _topAssembly = value;
                _applicationName = null;
                _applicationVersion = null;
            }
        }

        public static string ApplicationName
        {
            get
            {
                if (_applicationName == null && TopAssembly != null)
                {
                    AssemblyName assemblyName = TopAssembly.GetName();
                    object[] assemblyAttributes = TopAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
                    AssemblyProductAttribute productAttribute = assemblyAttributes.Cast<AssemblyProductAttribute>().FirstOrDefault();
                    _applicationName = productAttribute != null ? productAttribute.Product : assemblyName.Name;
                }
                return _applicationName;
            }
            set
            {
                _applicationName = value;
            }
        }

        public static Version ApplicationVersion
        {
            get
            {
                if (_applicationVersion == null && TopAssembly != null)
                {
                    AssemblyName assemblyName = TopAssembly.GetName();
                    _applicationVersion = assemblyName.Version;
                }
                return _applicationVersion;
            }
        }

        public static string ApplicationAndVersion
        {
            get { return $"{ApplicationName} {ApplicationVersion}".Trim(); }
        }

        public static Logger DefaultLogger
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_defaultLogger == null)
                {
                    _defaultLogger = new DebugLogger();
                }
                return _defaultLogger;
            }
            set
            {
                _defaultLogger = value;
            }
        }
    }
}