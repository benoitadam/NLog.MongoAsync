using System;
using System.Linq;
using Xunit;
using Moq;
using FluentAssertions;
using System.Reflection;

namespace NLog.MongoAsync.Tests
{
    public class MongoTargetTest
    {
        const string DATABASE_NAME = "Test";
        const string CONNECTION_STRING = "mongodb://some.server/nlog";
        const bool USE_CAPPED_COLLECTION = false;
        const int CAPPED_COLLECTION_MAX_ITEMS = 10000;
        const int CAPPED_COLLECTION_SIZE = 1000000;
        const string COLLECTION_NAME = "loggerName";

        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        [Fact(DisplayName = "Test Settings")]
        public void TestSettings()
        {
            MongoTarget target = new MongoTarget();

            target.CappedCollectionMaxItems
                .Should().Be(null, "because is the default value");
            target.CappedCollectionSize
                .Should().Be(8589934592, "because is the default value");
            target.ConnectionString
                .Should().Be("mongodb://localhost", "because is the default value");
            target.DatabaseName
                .Should().Be("logs", "because is the default value");
            target.UseCappedCollection
                .Should().Be(true, "because is the default value");
            
            target = new MongoTarget
            {
                DatabaseName = DATABASE_NAME,
                ConnectionString = CONNECTION_STRING,
                UseCappedCollection = USE_CAPPED_COLLECTION,
                CappedCollectionMaxItems = CAPPED_COLLECTION_MAX_ITEMS,
                CappedCollectionSize = CAPPED_COLLECTION_SIZE,
                CollectionName = COLLECTION_NAME,
            };

            target.DatabaseName
                .Should().Be(DATABASE_NAME);
            target.ConnectionString
                .Should().Be(CONNECTION_STRING);
            target.UseCappedCollection
                .Should().Be(USE_CAPPED_COLLECTION);
            target.CappedCollectionMaxItems
                .Should().Be(CAPPED_COLLECTION_MAX_ITEMS);
            target.CappedCollectionSize
                .Should().Be(CAPPED_COLLECTION_SIZE);
            target.CollectionName
                .Should().Be(COLLECTION_NAME);
        }
        
        [Fact(DisplayName = "Insert One")]
        public void InsertOne()
        {
            _log.Info("one message");
        }

        [Fact(DisplayName = "Insert One Thousand")]
        public void InsertOneThousand()
        {
            for (int i = 0; i < 1000; i++)
                _log.Info("message number {0}", i);
        }

        [Fact(DisplayName = "Insert One Million")]
        public void InsertOneMillion()
        {
            for (int i = 0; i < 1000000; i++)
                _log.Info("message number {0}", i);
        }

        [Fact(DisplayName = "Wait Insert")]
        public void InsertOneMillionAndWait()
        {
            var target = new MongoTarget();
            var repo = target.GetLogRepository();

            // TODO
        }
    }
}
