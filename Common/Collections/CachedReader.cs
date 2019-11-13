using System.Collections;
using System.Collections.Generic;

namespace Betlln.Collections
{
    /// <summary>
    /// Reads rows from the source and keeps a copy in memory for subsequent re-reads
    /// </summary>
    /// <remarks>
    /// When a caller (such as "foreach") disposes an enumerator, it is expected that an identical enumerator can be created later
    /// This implementation keeps the same enumerator, and so treats "dispose" as more like a Reset()
    /// </remarks>
    internal class CachedReader<T> : IEnumerable<T>, IEnumerator<T>
    {
        private IEnumerator<T> _enumerator;
        private IEnumerable<T> _source;

        public CachedReader(IEnumerable<T> source)
        {
            _source = source;
            Content = new List<T>();
        }

        private bool FullyCached { get; set; }
        private List<T> Content { get; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            return FullyCached ? MoveNextCache() : MoveNextSource();
        }

        private bool MoveNextSource()
        {
            if (_enumerator == null)
            {
                _enumerator = _source.GetEnumerator();
            }

            if (_enumerator.MoveNext())
            {
                Content.Add(_enumerator.Current);
                Current = _enumerator.Current;
                return true;
            }
            
            FullyCached = true;

            //once the source has finished being read, we want to release references so it can be garbage collected
            _source = null;   

            return false;
        }

        private bool MoveNextCache()
        {
            if (_enumerator == null)
            {
                _enumerator = Content.GetEnumerator();
            }

            if (_enumerator.MoveNext())
            {
                Current = _enumerator.Current;
                return true;
            }

            Current = default(T);
            return false;
        }

        public void Reset()
        {
            _enumerator?.Reset();
            MoveBeforeFirst();
        }

        object IEnumerator.Current => Current;
        public T Current { get; private set; }

        public void Dispose()
        {
            MoveBeforeFirst();

            _enumerator?.Dispose();
            _enumerator = null;
        }

        private void MoveBeforeFirst()
        {
            Current = default(T);
            if (!FullyCached)
            {
                Content.Clear();
            }
        }
    }
}