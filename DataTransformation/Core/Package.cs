using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Betlln.Data.Integration.Core
{
    public abstract class Package : Task
    {
        protected Package()
        {
            ParametersBinder = PropertyBinder.AttachTo(this, "PackageParameters");
            Streams = new ResourceStack();
        }

        internal PropertyBinder ParametersBinder { get; }
        private ResourceStack Streams { get; }

        protected override void PreExecute()
        {
            if (ParametersBinder != null)
            {
                Dts.Notify.All($"Starting package {GetType().Name} Parameters:{Environment.NewLine}{ParametersBinder}");
            }
            else
            {
                Dts.Notify.All($"Starting package {GetType().Name}");
            }
        }

        protected T AddSubComponent<T>()
            where T : IDisposable
        {
            T item = (T) Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance, null, null, CultureInfo.CurrentCulture);
            Streams.Push(item);
            return item;
        }

        // ReSharper disable InconsistentNaming
        protected IEnumerable<string> FolderContents(string Directory, string FilePattern)
        // ReSharper restore InconsistentNaming
        {
            return System.IO.Directory.GetFiles(Directory, FilePattern, SearchOption.TopDirectoryOnly);
        }

        protected static string StringAggregate(IEnumerable<string> list, string separator)
        {
            return string.Join(separator, list);
        }

        public override void Dispose()
        {
            Streams.Dispose();
        }
    }
}