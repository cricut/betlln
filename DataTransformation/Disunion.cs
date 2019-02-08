using System;
using System.Collections;
using System.Collections.Generic;
using Betlln.Data.Integration.Collections;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration
{
    public class Disunion : IDisposable
    {
        internal Disunion()
        {
        }

        public ListOf<string> ListToCheck { get; set; }
        public ListOf<string> IncomingItems { get; set; }

        private DataFeed _unexpectedItems;
        public DataFeed UnexpectedItems
        {
            get
            {
                if (_unexpectedItems == null)
                {
                    _unexpectedItems = new ListFeed(ListToCheck, IncomingItems);
                }
                return _unexpectedItems;
            }
        }

        public void Dispose()
        {
            _unexpectedItems?.Dispose();
            _unexpectedItems = null;
        }

        private class ListFeed : DataFeed, IDataRecordIterator
        {
            private IEnumerator<string> _baseIterator;

            public ListFeed(List<string> listToCheck, List<string> incomingItems)
            {
                List<string> list = new List<string>();

                foreach (string item in incomingItems)
                {
                    if (!listToCheck.Contains(item))
                    {
                        list.Add(item);
                    }
                }

                _baseIterator = list.GetEnumerator();
            }

            protected override IDataRecordIterator CreateReader()
            {
                return this;
            }

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
                if (_baseIterator.MoveNext())
                {
                    DataRecord record = new DataRecord();
                    record["_default"] = _baseIterator.Current;
                    Current = record;

                    return true;
                }
                else
                {
                    Current = null;
                    return false;
                }
            }

            public void Reset()
            {
                _baseIterator.Reset();
            }

            public DataRecord Current { get; private set; }
            object IEnumerator.Current => Current;

            public override void Dispose()
            {
                _baseIterator?.Dispose();
                _baseIterator = null;
            }
        }
    }
}