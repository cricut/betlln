using System;
using System.Collections.Generic;
using Betlln.Collections;

namespace Betlln.Data.File
{
    public class FileAdapterCache : IDataFileAdapter
    {
        private IDataFileAdapter _sourceAdapter;
        private readonly List<Section> _fileSections;

        public FileAdapterCache(IDataFileAdapter sourceAdapter)
        {
            if (sourceAdapter == null)
            {
                throw new ArgumentNullException(nameof(sourceAdapter));
            }

            _sourceAdapter = sourceAdapter;
            _fileSections = new List<Section>();
        }

        ~FileAdapterCache()
        {
            _sourceAdapter.Dispose();
            _sourceAdapter = null;
        }

        private Section CurrentSection { get; set; }

        public string CurrentSectionName
        {
            get
            {
                return _sourceAdapter.CurrentSectionName;
            }
            set
            {
                string sectionName = value;

                if (CurrentSection == null || CurrentSection.Name != sectionName)
                {
                    Section section = _fileSections.Find(x => x.Name == sectionName);
                    if (section == null)
                    {
                        _sourceAdapter.CurrentSectionName = sectionName;
                        section = new Section(_sourceAdapter);
                        _fileSections.Add(section);
                    }
                    else
                    {
                        throw new ArgumentException($"The file does not contain section {sectionName}.");
                    }

                    CurrentSection = section;
                }
            }
        }

        public void SelectSection(Func<IEnumerable<string>, string> sectionSelector)
        {
            _sourceAdapter.SelectSection(sectionSelector);
            CurrentSectionName = _sourceAdapter.CurrentSectionName;
        }

        public IEnumerable<FileRow> PlainData
        {
            get
            {
                CurrentSectionName = _sourceAdapter.CurrentSectionName;
                return CurrentSection?.PlainData;
            }
        }

        public void Reset()
        {
            foreach (Section section in _fileSections)
            {
                section.Dispose();
            }
        }

        public void Dispose()
        {
            Reset();
        }

        private class Section : IDisposable
        {
            public Section(IDataFileAdapter sourceAdapter)
            {
                Name = sourceAdapter.CurrentSectionName;
                PlainData = new CachedReader<FileRow>(sourceAdapter.PlainData);
            }

            public string Name { get; }
            public CachedReader<FileRow> PlainData { get; }

            public void Dispose()
            {
                PlainData.Dispose();
            }
        }
    }
}