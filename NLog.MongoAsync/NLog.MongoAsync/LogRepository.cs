using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NLog.MongoAsync
{
    /// <summary>
    /// Use for manage the log MongoDb collection
    /// </summary>
    public class LogRepository : ILogRepository
    {
        protected Exception _configError = null;
        protected IMongoCollection<BsonDocument> _collection;
        protected readonly ILogRepositoryConfig _config;

        /// <summary>
        /// Get or create the log collection
        /// </summary>
        public virtual IMongoCollection<BsonDocument> Collection
        {
            get
            {
                // If the collection is initialised
                if (_collection != null)
                    return _collection;

                // If the configuration is not valid (ex: Mongo doesn't exist)
                if (_configError != null)
                    return null;

                // May be more than one create collection on same time
                lock (this)
                {
                    // If the collection is created during the lock
                    if (_collection != null)
                        return _collection;

                    try
                    {
                        // Create the collection
                        _collection = CreateCollection();
                        return _collection;
                    }
                    catch (Exception ex)
                    {
                        // On error return null and save error
                        // We need reset to unlock the error
                        _configError = ex;
                        return null;
                    }
                }
            }
        }
        
        public LogRepository(ILogRepositoryConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Reset the repository for change the configuration
        /// </summary>
        public virtual void Reset()
        {
            _collection = null;
            _configError = null;
        }

        /// <summary>
        /// Insert documents into MongoDb
        /// </summary>
        /// <param name="documents"></param>
        public virtual async Task InsertAsync(IEnumerable<BsonDocument> documents)
        {
            var coll = Collection;

            // If not config error
            if (coll != null)
                await coll.InsertManyAsync(documents);
        }
        
        /// <summary>
        /// Insert one document into MongoDb
        /// </summary>
        /// <param name="documents"></param>
        public virtual async Task InsertAsync(BsonDocument document)
        {
            var coll = Collection;

            // If not config error
            if (coll != null)
                await coll.InsertOneAsync(document);
        }

        /// <summary>
        /// Create the log collection
        /// </summary>
        /// <returns></returns>
        protected virtual IMongoCollection<BsonDocument> CreateCollection()
        {
            var client = new MongoClient(_config.ConnectionString);
            var db = client.GetDatabase(_config.DatabaseName);
            var collectionExists = CheckIfTheCollectionExist(db, _config.CollectionName);

            // Create capped collection
            if (!collectionExists)
            {
                db.CreateCollectionAsync(_config.CollectionName, new CreateCollectionOptions()
                {
                    Capped = _config.UseCappedCollection,
                    MaxSize = _config.UseCappedCollection ? _config.CappedCollectionSize : null,
                    MaxDocuments = _config.UseCappedCollection ? _config.CappedCollectionMaxItems : null
                }).Wait();
            }

            _collection = db.GetCollection<BsonDocument>(_config.CollectionName);

            // Create index
            if (!collectionExists)
            {
                CreateIndexKeys(_collection);
            }

            return _collection;
        }

        /// <summary>
        /// Create index on logger and lvl
        /// </summary>
        /// <param name="collection"></param>
        protected virtual void CreateIndexKeys(IMongoCollection<BsonDocument> collection)
        {
            var keys = Builders<BsonDocument>.IndexKeys.Ascending("logger").Ascending("lvl");
            _collection.Indexes.CreateOneAsync(keys).Wait();
        }

        /// <summary>
        /// Check if the collection exist
        /// </summary>
        /// <param name="db"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        protected virtual bool CheckIfTheCollectionExist(IMongoDatabase db, string collectionName)
        {
            using (var cursor = db.ListCollectionsAsync().Result)
            {
                var collections = cursor.ToListAsync();
                foreach (var collection in collections.Result)
                    if (collection["name"] == collectionName)
                        return true;
            }
            return false;
        }
    }
}
