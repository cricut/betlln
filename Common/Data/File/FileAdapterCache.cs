using System;
using System.Collections.Generic;
using System.Linq;
using Betlln.Collections;

namespace Betlln.Data.File
{
    public class FileAdapterCache : IDataFileAdapter
    {
        private readonly IDataFileAdapter _sourceAdapter;
        private readonly Dictionary<string, CachedReader<FileRow>> _readers;

        public FileAdapterCache(IDataFileAdapter sourceAdapter)
        {
            if (sourceAdapter == null)
            {
                throw new ArgumentNullException(nameof(sourceAdapter));
            }

            _sourceAdapter = sourceAdapter;
            _readers = new Dictionary<string, CachedReader<FileRow>>();
        }

        ~FileAdapterCache()
        {
            Dispose();
        }

        public string CurrentSectionName
        {
            get { return _sourceAdapter.CurrentSectionName; }
            set { _sourceAdapter.CurrentSectionName = value; }
        }

        public IEnumerable<string> SectionNames
        {
            get { return _sourceAdapter.SectionNames; }
        }

        public void SelectSection(Func<IEnumerable<string>, string> sectionSelector)
        {
            _sourceAdapter.SelectSection(sectionSelector);
        }

        public IEnumerable<FileRow> PlainData
        {
            get
            {
                string sectionName = _sourceAdapter.CurrentSectionName;

                if (!_readers.ContainsKey(sectionName))
                {
                    _readers.Add(sectionName, new CachedReader<FileRow>(_sourceAdapter.PlainData));
                }

                return _readers[sectionName];
            }
        }

        public void Dispose()
        {
            if (_readers != null)
            {
                foreach (var readerInfo in _readers)
                {
                    readerInfo.Value.Dispose();
                }

                if (_readers.Values.All(x => x.FullyCached) && 
                    _readers.Count == _sourceAdapter?.SectionNames?.Count())
                {
                    _sourceAdapter?.Dispose();
                }
            }
            else  //only accessible by finalizer
            {
                _sourceAdapter?.Dispose();
            }
        }
    }
}
