using NLog.Targets;
using System.Reflection;
using NLog.Common;

namespace NLog.MongoAsync
{
    /// <summary>
    /// NLog message target for MongoDB.
    /// </summary>
	[Target("Mongo")]
	public sealed class MongoTarget : Target, ILogRepositoryConfig
    {
        private string _collectionName;
        private string _connectionString = "mongodb://localhost";
        private string _databaseName = "logs";
        private bool _useFormattedMessage = true;
        private bool _useCappedCollection = true;
        private long? _cappedCollectionSize = 8589934592; // 1Go
        private long? _cappedCollectionMaxItems = null;
        private ILogRepository _logRepository = null;

        public MongoTarget()
		{
            try
            {
                // Use Assembly Name by default
                var entryAssembly = Assembly.GetEntryAssembly();
                CollectionName = entryAssembly != null ? entryAssembly.GetName().Name : "Other";
            }
            catch
            {
            }
        }

        #region ILogRepositoryConfig

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public string CollectionName
        {
            get { return _collectionName; }
            set { _collectionName = value; Reset(); }
        }

        /// <summary>
        /// Gets or sets the connection string name string.
        /// </summary>
        /// <value>
        /// The connection name string.
        /// </value>
        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; Reset(); }
        }

        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public string DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; Reset(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use a capped collection.
        /// </summary>
        /// <value>
        /// <c>true</c> to use a capped collection; otherwise, <c>false</c>.
        /// </value>
        public bool UseCappedCollection
        {
            get { return _useCappedCollection; }
            set { _useCappedCollection = value; Reset(); }
        }

        /// <summary>
        /// Gets or sets the size in bytes of the capped collection.
        /// </summary>
        /// <value>
        /// The size of the capped collection.
        /// </value>
        public long? CappedCollectionSize
        {
            get { return _cappedCollectionSize; }
            set { _cappedCollectionSize = value; Reset(); }
        }

        /// <summary>
        /// Gets or sets the capped collection max items.
        /// </summary>
        /// <value>
        /// The capped collection max items.
        /// </value>
        public long? CappedCollectionMaxItems
        {
            get { return _cappedCollectionMaxItems; }
            set { _cappedCollectionMaxItems = value; Reset(); }
        }

        #endregion
        
        /// <summary>
        /// Gets or sets a value indicating whether to use the default message formating.
        /// </summary>
        /// <value>
        /// <c>true</c> to use the default message formating; otherwise, <c>false</c>.
        /// </value>
        public bool UseFormattedMessage
        {
            get { return _useFormattedMessage; }
            set { _useFormattedMessage = value; Reset(); }
        }
        
        private void Reset()
        {
            if (_logRepository == null)
                _logRepository = new LogRepository(this);

            // Free mongodb collection
            _logRepository.Reset();
        }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        /// <exception cref="NLog.NLogConfigurationException">Can not resolve MongoDB ConnectionString. Please make sure the ConnectionString property is set.</exception>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            Reset();
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            var documents = logEvents.ToBsonDocuments(UseFormattedMessage);
            _logRepository.InsertAsync(documents).Wait();
        }

        /// <summary>
        /// Writes logging event to the log target.
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            var document = logEvent.ToBsonDocument(UseFormattedMessage);
            _logRepository.InsertAsync(document).Wait();
        }

        /// <summary>
        /// Get the current repository
        /// </summary>
        /// <returns></returns>
        public ILogRepository GetLogRepository()
        {
            return _logRepository;
        }
    }
}