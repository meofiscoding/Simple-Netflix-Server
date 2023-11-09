using System;
using CrawlData.Model;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

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
            movie.Id ??= ObjectId.GenerateNewId().ToString();
            var filter = Builders<MovieItem>.Filter.Eq(x => x.Id, movie.Id);
            await Movies.ReplaceOneAsync(filter, movie, new ReplaceOptions { IsUpsert = true });
        }

        // Get movie by name
        public async Task<MovieItem> GetMovieByNameAsync(string movieName)
        {
            var filter = Builders<MovieItem>.Filter.Eq(x => x.MovieName, movieName);
            return (await Movies.FindAsync(filter)).FirstOrDefault();
        }

        public async Task<List<MovieItem>> GetAllMovie()
        {
            return (await Movies.FindAsync(_ => true)).ToList();
        }

        public static void UpdateMovie(MovieItem movie)
        {
            var filter = Builders<MovieItem>.Filter.Eq(x => x.Id, movie.Id);
            var update = Builders<MovieItem>.Update
                .Set(x => x.AvailableEpisode, movie.AvailableEpisode)
                .Set(x => x.IsAvailable, movie.IsAvailable)
                .Set(x => x.StreamingUrls, movie.StreamingUrls)
                .Set(x => x.UpdatedAt, DateTime.Now);
        }
    }
}
