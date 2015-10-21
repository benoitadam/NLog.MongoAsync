using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NLog.MongoAsync
{
    /// <summary>
    /// Use for manage the log MongoDb collection
    /// </summary>
    public interface ILogRepository
    {
        /// <summary>
        /// Reset the repository for change the configuration
        /// </summary>
        void Reset();

        /// <summary>
        /// Insert documents into MongoDb
        /// </summary>
        /// <param name="documents"></param>
        Task InsertAsync(IEnumerable<BsonDocument> documents);

        /// <summary>
        /// Insert one document into MongoDb
        /// </summary>
        /// <param name="documents"></param>
        Task InsertAsync(BsonDocument document);
    }
}