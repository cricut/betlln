using System;
using System.Collections.Generic;
using System.Linq;
using Betlln.Data.Integration.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Betlln.Data.Integration.Mongo
{
    public class MongoCursor
    {
        private string _filter;
        private IEnumerable<string> _pipeline;

        public MongoCursor()
        {
            BatchSize = 100;
        }

        public IConnectionManager Connection { get; set; }
        public string CollectionName { get; set; }

        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                if (AggregationPipeline != null)
                {
                    throw new InvalidOperationException();
                }
                _filter = value;
            }
        }

        public IEnumerable<string> AggregationPipeline
        {
            get
            {
                return _pipeline;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(Filter))
                {
                    throw new InvalidOperationException();
                }
                _pipeline = value;
            }
        }

        public int BatchSize { get; set; }
        public EventHandler<BsonDocument> DocumentReceived;
        public EventHandler BatchFinished;

        public void Read()
        {
            if (DocumentReceived == null)
            {
                throw new ArgumentNullException();
            }

            using (IMongoDB client = (IMongoDB) Connection.GetConnection())
            {
                IMongoCollection<BsonDocument> collection = client.Service.GetCollection<BsonDocument>(CollectionName);
                if (AggregationPipeline == null || !AggregationPipeline.Any())
                {
                    ReadList(collection);
                }
                else
                {
                    ReadAggregation(collection);
                }
            }
        }

        private void ReadList(IMongoCollection<BsonDocument> collection)
        {
            FindOptions<BsonDocument, BsonDocument> options = new FindOptions<BsonDocument, BsonDocument>();
            options.BatchSize = BatchSize;
            using (IAsyncCursor<BsonDocument> cursor = collection.FindSync(FilterDefinition, options))
            {
                ReadCursor(cursor);
            }
        }

        private void ReadAggregation(IMongoCollection<BsonDocument> collection)
        {
            PipelineDefinition<BsonDocument, BsonDocument> pipeline = PipelineDefinition<BsonDocument, BsonDocument>.Create(AggregationPipeline);
            AggregateOptions options = new AggregateOptions();
            options.BatchSize = BatchSize;
            using (IAsyncCursor<BsonDocument> cursor = collection.Aggregate(pipeline, options))
            {
                ReadCursor(cursor);
            }
        }

        private void ReadCursor(IAsyncCursor<BsonDocument> cursor)
        {
            while (cursor.MoveNext())
            {
                IEnumerable<BsonDocument> currentBatch = cursor.Current;
                foreach (BsonDocument document in currentBatch)
                {
                    DocumentReceived(this, document);
                }

                BatchFinished?.Invoke(this, EventArgs.Empty);
            }
        }

        public IEnumerable<T> ReadDistinct<T>(string fieldName)
        {
            if (AggregationPipeline != null)
            {
                throw new InvalidOperationException("Aggregate and Distinct cannot be combined.");
            }

            if (DocumentReceived != null)
            {
                throw new InvalidOperationException("Documents are not captured with Distinct.");
            }

            using (IMongoDB client = (IMongoDB) Connection.GetConnection())
            {
                IMongoCollection<BsonDocument> collection = client.Service.GetCollection<BsonDocument>(CollectionName);
                using (IAsyncCursor<T> cursor = collection.Distinct<T>(fieldName, FilterDefinition))
                {
                    while (cursor.MoveNext())
                    {
                        foreach (T item in cursor.Current)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }

        private FilterDefinition<BsonDocument> FilterDefinition
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Filter))
                {
                    return new BsonDocument();
                }
                return Filter;
            }
        }
    }
}