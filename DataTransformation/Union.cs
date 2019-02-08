using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration
{
    public class Union : DataFeed
    {
        internal Union()
        {
            Sources = new List<DataFeed>();
        }

        public List<DataFeed> Sources { get; }

        protected override IDataRecordIterator CreateReader()
        {
            return new MultiReader(Sources);
        }

        private class MultiReader : IDataRecordIterator
        {
            public MultiReader(List<DataFeed> sources)
            {
                Sources = new Queue<DataFeed>(sources);
            }

            private IDataRecordIterator CurrentIterator { get; set; }
            private Queue<DataFeed> Sources { get; }

            public IEnumerator<DataRecord> GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public bool MoveNext()
            {
                if (CurrentIterator == null)
                {
                    if(Sources.Any())
                    {
                        CurrentIterator = Sources.Dequeue().GetReader();
                    }
                    else
                    {
                        return false;
                    }
                }

                if (CurrentIterator.MoveNext())
                {
                    Current = CurrentIterator.Current;
                    return true;
                }
                else
                {
                    CurrentIterator.Dispose();
                    CurrentIterator = null;
                    return MoveNext();
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public DataRecord Current { get; private set; }
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                CurrentIterator?.Dispose();
                CurrentIterator = null;
                Current = null;
                Sources.Clear();
            }
        }
    }
}