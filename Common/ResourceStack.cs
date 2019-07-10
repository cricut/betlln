using System;
using System.Collections.Generic;
using System.Linq;

namespace Betlln
{
    public class ResourceStack : IDisposable
    {
        private readonly Stack<IDisposable> _resources;

        public ResourceStack()
        {
            _resources = new Stack<IDisposable>();
        }

        public void Push(IDisposable resource)
        {
            _resources.Push(resource);
        }

        public IDisposable Tip
        {
            get { return _resources.Peek(); }
        }

        public int Count
        {
            get { return _resources.Count; }
        }

        public void Dispose()
        {
            List<Exception> exceptions = new List<Exception>();

            while (_resources.Count > 0)
            {
                IDisposable resource = _resources.Pop();

                try
                {
                    resource.Dispose();
                }
                catch (Exception disposeError)
                {
                    exceptions.Add(disposeError);
                }
            }

            if (exceptions.Any())
            {
                AggregateException aggregateException = new AggregateException(exceptions);
                throw new ObjectDisposedException("One or more resources could not be disposed properly.", aggregateException);
            }
        }
    }
}