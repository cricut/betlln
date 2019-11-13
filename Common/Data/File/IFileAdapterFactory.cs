using System;

namespace Betlln.Data.File
{
    public interface IFileAdapterFactory
    {
        /// <summary>
        /// Returns a file adapter for the specified path.
        /// </summary>
        /// <param name="filePath">The path to the data file.</param>
        /// <returns>The data file adapter</returns>
        IDataFileAdapter GetFileAdapter(string filePath);

        /// <summary>
        /// Returns a file adapter for the specified path.
        /// </summary>
        /// <param name="filePath">The path to the data file.</param>
        /// <param name="useCached"><b>This value is ignored.</b> All data is cached.</param>
        /// <returns>The data file adapter</returns>
        [Obsolete("Use GetFileAdapter(string filePath) instead.")]
        IDataFileAdapter GetFileAdapter(string filePath, bool useCached);
    }
}