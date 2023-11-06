using System;
using CrawlData.Model;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace CrawlData.Infrastructure.Data
{
    public class MovieContext : IMovieContext
    {
        public MovieContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
            Movies = database.GetCollection<MovieItem>(configuration.GetValue<string>("DatabaseSettings:CollectionName"));
        }

        public IMongoCollection<MovieItem> Movies { get; }
    }
}
