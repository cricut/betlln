using System;
using MongoDB.Driver;

namespace Betlln.Data.Integration.Mongo
{
    public interface IMongoDB : IDisposable
    {
        IMongoDatabase Service { get; }
    }
}