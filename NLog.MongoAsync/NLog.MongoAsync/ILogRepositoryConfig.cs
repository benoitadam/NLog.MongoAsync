namespace NLog.MongoAsync
{
    public interface ILogRepositoryConfig
    {
        string CollectionName { get; }

        string ConnectionString { get; }

        string DatabaseName { get; }
        
        bool UseCappedCollection { get; }

        long? CappedCollectionSize { get; }

        long? CappedCollectionMaxItems { get; }
    }
}
