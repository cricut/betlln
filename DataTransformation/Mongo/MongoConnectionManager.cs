using System;
using Betlln.Data.Integration.Core;
using MongoDB.Driver;

namespace Betlln.Data.Integration.Mongo
{
    // ReSharper disable once HollowTypeName
    public class MongoConnectionManager : IConnectionManager
    {
        public IDisposable GetConnection()
        {
            string url = $"mongodb://{UserName}:{Password}@{Host}/{Database}";
            MongoClient client = new MongoClient(url);
            IMongoDatabase database = client.GetDatabase(Database);
            return new Wrapper(database);
        }

        public Type GetDataAdapterType()
        {
            throw new NotSupportedException();
        }

        public string Host { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        private class Wrapper : IMongoDB
        {
            public Wrapper(IMongoDatabase database)
            {
                Service = database;
            }

            public IMongoDatabase Service { get; }

            public void Dispose()
            {
            }
        }
    }
}