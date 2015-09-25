using MongoDB.Bson;
using MongoDB.Driver;
using NLog.Common;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NLog.MongoAsync
{
    [Target("MongoTarget")]
    public sealed class MongoTarget : Target
    {
        private object _lock = new object();
        private IMongoCollection<BsonDocument> _collection;

        private string _collectionName;
        private string _connectionString = "mongodb://localhost";
        private string _databaseName = "logs";
        private bool _useFormattedMessage = false;
        private bool _useCappedCollection = true;
        private long _cappedCollectionSize = 8589934592; // 1Go
        private long? _cappedCollectionMaxItems = null;
        private Exception _collectionError = null;

        public MongoTarget()
        {
            try
            {
                if (CollectionName == null)
                {
                    CollectionName = Assembly.GetEntryAssembly().GetName().Name;
                }
            }
            catch
            {
            }
        }

        #region Properties

        public string CollectionName
        {
            get { return _collectionName; }
            set { _collectionName = value; Reset(); }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; Reset(); }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; Reset(); }
        }

        public bool UseFormattedMessage
        {
            get { return _useFormattedMessage; }
            set { _useFormattedMessage = value; Reset(); }
        }

        public bool UseCappedCollection
        {
            get { return _useCappedCollection; }
            set { _useCappedCollection = value; Reset(); }
        }

        public long CappedCollectionSize
        {
            get { return _cappedCollectionSize; }
            set { _cappedCollectionSize = value; Reset(); }
        }

        public long? CappedCollectionMaxItems
        {
            get { return _cappedCollectionMaxItems; }
            set { _cappedCollectionMaxItems = value; Reset(); }
        }

        #endregion

        private IMongoCollection<BsonDocument> Collection
        {
            get
            {
                if (_collection != null)
                    return _collection;

                // If Mongo doesn't exist ...
                if (_collectionError != null)
                    return null;

                lock (_lock)
                {
                    if (_collection != null)
                        return _collection;

                    try
                    {
                        var client = new MongoClient(ConnectionString);
                        var db = client.GetDatabase(DatabaseName);

                        bool collectionExists = false;
                        using (var cursor = db.ListCollectionsAsync().Result)
                        {
                            var collections = cursor.ToListAsync();
                            foreach (var collection in collections.Result)
                                if (collection["name"] == CollectionName)
                                    collectionExists = true;
                        }

                        if (!collectionExists)
                        {
                            db.CreateCollectionAsync(CollectionName, new CreateCollectionOptions()
                            {
                                Capped = UseCappedCollection,
                                MaxSize = UseCappedCollection ? (long?)CappedCollectionSize : null,
                                MaxDocuments = UseCappedCollection && CappedCollectionMaxItems.HasValue ? CappedCollectionMaxItems : null
                            });
                        }

                        _collection = db.GetCollection<BsonDocument>(CollectionName);

                        if (!collectionExists)
                        {
                            var keys = Builders<BsonDocument>.IndexKeys.Ascending("logger").Ascending("lvl");
                            _collection.Indexes.CreateOneAsync(keys).Wait();
                        }

                        return _collection;
                    }
                    catch (Exception ex)
                    {
                        _collectionError = ex;
                        return null;
                    }
                }
            }
        }

        private void Reset()
        {
            _collection = null;
            _collectionError = null;
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            Reset();
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            var documents = new List<BsonDocument>();

            foreach (var log in logEvents)
            {
                var doc = ToBsonDocument(log.LogEvent);
                documents.Add(doc);
            }

            var coll = Collection;

            // If not config error
            if (coll != null)
                coll.InsertManyAsync(documents).Wait();
        }

        public BsonDocument ToBsonDocument(LogEventInfo log)
        {
            var doc = new BsonDocument();

            Add(doc, "date", log.TimeStamp);
            Add(doc, "logger", log.LoggerName);
            Add(doc, "lvl", log.Level);
            Add(doc, "trace", log.StackTrace);
            Add(doc, "frame", log.UserStackFrame);
            Add(doc, "frameNb", log.UserStackFrameNumber);
            Add(doc, "ex", log.Exception);
            Add(doc, "data", log.Properties);

            if (UseFormattedMessage)
                Add(doc, "msg", log.FormattedMessage);
            else
                Add(doc, "msg", log.Message, log.Parameters);

            return doc;
        }

        private void Add(BsonDocument doc, string name, string message, object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                Add(doc, name, new object[] { message });
                return;
            }

            var msg = new object[parameters.Length + 1];
            msg[0] = message;
            parameters.CopyTo(msg, 1);
            Add(doc, name, msg);
        }

        private void Add(BsonDocument doc, string name, IDictionary<object, object> values)
        {
            if (values == null || values.Count == 0)
                return;

            AddObj(doc, name, values);
        }

        private void Add(BsonDocument doc, string name, object[] values)
        {
            if (values == null || values.Length == 0)
                return;

            AddObj(doc, name, values);
        }

        private void Add(BsonDocument doc, string name, int value)
        {
            if (value != default(int))
                doc.Add(name, new BsonInt32(value));
        }

        private void Add(BsonDocument doc, string name, string value)
        {
            if (value != null)
                doc.Add(name, new BsonString(value));
        }

        private void Add(BsonDocument doc, string name, object value)
        {
            AddObj(doc, name, value);
        }

        private void AddObj(BsonDocument doc, string name, object value)
        {
            if (value == null)
                return;

            BsonValue bsonValue;
            if (!BsonTypeMapper.TryMapToBsonValue(value, out bsonValue))
                bsonValue = BsonValue.Create(value.ToString());

            doc.Add(name, bsonValue);
        }
    }
}
