using System;
using System.Collections.Generic;

namespace Betlln.Data.File
{
    public class FileAdapterCache : IDataFileAdapter
    {
        private readonly IDataFileAdapter _sourceAdapter;
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

                Section section = _fileSections.Find(x => x.Name == sectionName);
                if (section == null)
                {
                    _sourceAdapter.CurrentSectionName = sectionName;
                    section = new Section(sectionName);
                    _fileSections.Add(section);
                }

                CurrentSection = section;
            }
        }

        public void SelectSection(Func<IEnumerable<string>, string> sectionSelector)
        {
            _sourceAdapter.SelectSection(sectionSelector);

            string currentSectionName = _sourceAdapter.CurrentSectionName;
            Section section = _fileSections.Find(x => x.Name == currentSectionName);

            if (section == null)
            {
                section = new Section(currentSectionName);
                _fileSections.Add(section);
            }

            CurrentSection = section;
        }

        public IEnumerable<FileRow> PlainData
        {
            get
            {
                if (CurrentSection == null)
                {
                    Section newSection = new Section(CurrentSectionName);
                    newSection.PlainData.AddRange(_sourceAdapter.PlainData);
                    _fileSections.Add(newSection);
                    CurrentSection = newSection;
                }

                return CurrentSection.PlainData;
            }
        }

        public void Dispose()
        {
            _sourceAdapter.Dispose();
        }

        private class Section
        {
            public Section(string name)
            {
                Name = name;
                PlainData = new List<FileRow>();
            }

            public string Name { get; }
            public List<FileRow> PlainData { get; }
        }
    }
}