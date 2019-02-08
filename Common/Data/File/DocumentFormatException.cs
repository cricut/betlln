using System;

namespace Betlln.Data.File
{
    public class DocumentFormatException : Exception
    {
        public DocumentFormatException(string message)
            : base(message)
        {
        }

        public DocumentFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
