using System;

namespace Betlln.Data.Integration.Core
{
    public interface IConnectionManager
    {
        IDisposable GetConnection();
    }
}