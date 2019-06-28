using System;
using System.IO;

namespace Betlln.Data.Integration.Core
{
    public abstract class Transformation : IDisposable
    {
        public virtual string OutputName { get; set; }

        private NamedStream _output;
        public NamedStream Output
        {
            get
            {
                if (_output == null)
                {
                    _output = new NamedStream(OutputName, new MemoryStream());
                    WriteToStream(_output.Content);
                    _output.Content.Position = 0;
                }

                return _output;
            }
        }

        protected abstract void WriteToStream(Stream outputStream);

        public void Dispose()
        {
            _output?.Dispose();
            _output = null;
        }
    }
}