using System;
using CrawlData.Model;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CrawlData.Helper
{
    public class MongoHelper
    {
        private readonly IMongoDatabase db;

        public MongoHelper(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            db = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
            Movies = db.GetCollection<MovieItem>(configuration.GetValue<string>("DatabaseSettings:CollectionName"));
        }

        public IMongoCollection<MovieItem> Movies { get; set; }

        // Upsert movie to database
        public async Task UpsertMovieAsync(MovieItem movie)
        {
            var filter = Builders<MovieItem>.Filter.Eq(x => x.MovieName, movie.MovieName);
            await Movies.ReplaceOneAsync(filter, movie, new ReplaceOptions { IsUpsert = true });
        }

    }
}
