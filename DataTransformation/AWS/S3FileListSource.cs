using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Amazon.S3.Model;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.AWS
{
    public class S3FileListSource : DataSource, IS3FolderSource
    {
        internal S3FileListSource()
        {
        }

        public string BucketName { get; set; }
        public string Directory { get; set; }
        public string FilePattern { get; set; }

        protected override IDataRecordIterator CreateReader()
        {
            return new DirectoryIterator(this);
        }

        private class DirectoryIterator : IDataRecordIterator, IS3FolderSource
        {
            private IS3Client _client;
            private string _continuationToken;
            private IEnumerator<S3Object> _fileEnumerator;
            private List<ColumnInfo> _columns;

            public DirectoryIterator(IS3FolderSource settings)
            {
                Connection = settings.Connection;
                BucketName = settings.BucketName;
                Directory = settings.Directory;
                FilePattern = settings.FilePattern;

                if (FilePattern.StartsWith("*"))
                {
                    FilePattern = FilePattern.Substring(1);
                }

                SetLayout();
            }

            public IConnectionManager Connection { get; set; }
            public string BucketName { get; set; }
            public string Directory { get; set; }
            public string FilePattern { get; set; }

            private void SetLayout()
            {
                _columns = new List<ColumnInfo>();
                PropertyInfo[] properties = typeof(S3Object).GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    Type propertyType = property.PropertyType;
                    if (!propertyType.IsClass || propertyType == typeof(string))
                    {
                        _columns.Add(new ColumnInfo(property.Name, property.PropertyType));
                    }
                }
            }

            public IEnumerator<DataRecord> GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public bool MoveNext()
            {
                if (_client == null)
                {
                    _client = (IS3Client) Connection.GetConnection();
                }

                if (_fileEnumerator == null)
                {
                    GetNextFileBatch();
                }

                if (_fileEnumerator.MoveNext())
                {
                    S3Object s3Object = _fileEnumerator.Current;
                    Debug.Assert(s3Object != null);

                    if (s3Object.Key.EndsWith(FilePattern, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Current = ReadFile(s3Object);
                        return true;
                    }
                    else
                    {
                        return MoveNext();
                    }
                }
                else
                {
                    _fileEnumerator.Dispose();
                    _fileEnumerator = null;
                    if (!string.IsNullOrWhiteSpace(_continuationToken))
                    {
                        return MoveNext();
                    }
                    else
                    {
                        Dispose();
                        return false;
                    }
                }
            }

            private void GetNextFileBatch()
            {
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = BucketName,
                    Prefix = Directory
                };
                request.ContinuationToken = _continuationToken;

                ListObjectsV2Response response = _client.Service.ListObjectsV2(request);

                _continuationToken = response.IsTruncated ? response.NextContinuationToken : null;
                _fileEnumerator = response.S3Objects.GetEnumerator();
            }

            private DataRecord ReadFile(S3Object s3Object)
            {
                DataRecord record = new DataRecord();

                foreach (ColumnInfo columnInfo in _columns)
                {
                    string columnName = columnInfo.Name;

                    PropertyInfo propertyInfo = typeof(S3Object).GetProperty(columnName);
                    Debug.Assert(propertyInfo != null);

                    record[columnName] = propertyInfo.GetValue(s3Object);
                }

                return record;
            }

            public void Reset()
            {
                Dispose();
            }

            public DataRecord Current { get; private set; }
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _client?.Dispose();
                _fileEnumerator?.Dispose();
                _continuationToken = null;
                Current = null;
            }
        }
    }
}