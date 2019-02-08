using System;
using System.Collections.Generic;

namespace Betlln.Data.File
{
    public interface IDataFileAdapter : IDisposable
    {
        string CurrentSectionName { get; set; }
        void SelectSection(Func<IEnumerable<string>, string> sectionSelector);
        IEnumerable<FileRow> PlainData { get; }
    }
}