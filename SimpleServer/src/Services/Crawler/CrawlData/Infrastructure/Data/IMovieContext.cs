using System;
using CrawlData.Model;
using MongoDB.Driver;

namespace CrawlData.Infrastructure.Data
{
    public interface IMovieContext
    {
        IMongoCollection<MovieItem> Movies { get; }
    }
}
