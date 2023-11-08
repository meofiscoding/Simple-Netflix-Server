using System;
using CrawlData.Model;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CrawlData.Infrastructor
{
    public class MongoCrawlerDBContext : IMongoCrawlerDBContext
    {
        private IMongoDatabase Db { get; set; }
        private IMongoClient MongoClient { get; set; }
        public string CollectionName { get; set; }

        public MongoCrawlerDBContext(IOptions<DatabaseSettings> configuration)
        {
            MongoClient = new MongoClient(configuration.Value.ConnectionString);
            Db = MongoClient.GetDatabase(configuration.Value.DatabaseName);
            CollectionName = configuration.Value.CollectionName;
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            return Db.GetCollection<T>(CollectionName);
        }
    }
}
