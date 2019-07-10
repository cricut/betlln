using System;
using System.IO;

namespace Betlln.Data.Integration.Core
{
    public class NamedStream : IDisposable
    {
        public NamedStream(string name, Stream content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Name = name;
            Content = content;
        }

        public string Name { get; }
        public Stream Content { get; }

        public void Dispose()
        {
            Content.Dispose();
        }
    }
}