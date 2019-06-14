using System.IO;

namespace Betlln.Data.Integration.Core
{
    public class NamedStream
    {
        public string Name { get; set; }
        public Stream Content { get; set; }
    }
}