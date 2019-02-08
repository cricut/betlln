using System;
using Betlln.Data.Integration.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Betlln.Data.Integration.Mongo
{
    public class MongoCollectionCommandTask : Task
    {
        public IConnectionManager Connection { get; set; }
        public string CollectionName { get; set; }
        public string Filter { get; set; }
        public MongoCollectionCommand Command { get; set; }

        protected override void ExecuteTasks()
        {
            switch (Command)
            {
                case MongoCollectionCommand.DeleteDocuments:
                    Delete();
                    break;
                case MongoCollectionCommand.DropCollection:
                    Drop();
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void Delete()
        {
            using (IMongoDB client = (IMongoDB) Connection.GetConnection())
            {
                IMongoCollection<BsonDocument> collection = client.Service.GetCollection<BsonDocument>(CollectionName);
                collection.DeleteMany(Filter);
            }
        }

        private void Drop()
        {
            using (IMongoDB client = (IMongoDB) Connection.GetConnection())
            {
                client.Service.DropCollection(CollectionName);
            }
        }
    }
}