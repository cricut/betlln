using System.Data;
using Betlln.Data.Integration.Collections;
using Betlln.Data.Integration.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Betlln.Data.Integration.Mongo
{
    public class MongoDataSource : DataSource, IColumnMapper
    {
        private const string SourcePathPropertyName = "SourceJsonPath";
        private readonly MongoCursor _cursor;

        private readonly DataTable _results;

        internal MongoDataSource()
        {
            _results = new DataTable();
            _cursor = new MongoCursor();
        }

        public override IConnectionManager Connection
        {
            get { return _cursor.Connection; }
            set { _cursor.Connection = value; }
        }

        public string CollectionName
        {
            get { return _cursor.CollectionName; }
            set { _cursor.CollectionName = value; }
        }

        public void MapColumns<T>(string sourceJsonPath, string outputAlias)
        {
            DataColumn column = _results.Columns.Add(outputAlias, typeof(T));
            column.ExtendedProperties[SourcePathPropertyName] = sourceJsonPath;
        }

        public override DataTable GetResults()
        {
            if (_cursor.DocumentReceived == null)
            {
                _cursor.DocumentReceived += OnDocumentReceived;
            }

            _cursor.Read();

            return _results;
        }

        private void OnDocumentReceived(object sender, BsonDocument document)
        {
            DataRow dataRow = _results.NewRow();

            foreach (DataColumn column in _results.Columns)
            {
                string jsonPath = column.ExtendedProperties[SourcePathPropertyName].ToString();
                string columnName = column.ColumnName;

                object value = document.GetValue(jsonPath, column.DataType);
                dataRow[columnName] = value;
            }

            _results.Rows.Add(dataRow);
        }

        public override long GetRowCount()
        {
            FilterDefinition<BsonDocument> filterDefinition = new BsonDocument();
            if (!string.IsNullOrWhiteSpace(_cursor.Filter))
            {
                filterDefinition = _cursor.Filter;
            }

            using (IMongoDB client = (IMongoDB) Connection.GetConnection())
            {
                IMongoCollection<BsonDocument> collection = client.Service.GetCollection<BsonDocument>(CollectionName);
                return collection.Count(filterDefinition);
            }
        }

        protected override IDataRecordIterator CreateReader()
        {
            if (_results.Rows.Count == 0)
            {
                GetResults();
            }

            return new DataTableRecordIterator(_results);
        }
    }
}