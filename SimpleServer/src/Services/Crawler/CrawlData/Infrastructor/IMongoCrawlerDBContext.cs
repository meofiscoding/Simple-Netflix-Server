using System;
using CrawlData.Model;
using MongoDB.Driver;

namespace CrawlData.Infrastructor
{
    public interface IMongoCrawlerDBContext
    {
        IMongoCollection<MovieItem> GetCollection<MovieItem>();
    }
}
