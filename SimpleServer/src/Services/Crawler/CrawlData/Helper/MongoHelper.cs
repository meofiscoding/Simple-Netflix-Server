using System;
using CrawlData.Infrastructor;
using CrawlData.Model;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace CrawlData.Helper
{
    public class MongoHelper
    {
        private readonly IMongoCrawlerDBContext _context;
        protected IMongoCollection<MovieItem> _movieCollection;

        public MongoHelper(IMongoCrawlerDBContext context)
        {
            _context = context;
            _movieCollection = _context.GetCollection<MovieItem>();
        }

        // Upsert movie to database
        public async Task UpsertMovieAsync(MovieItem movie)
        {
            movie.Id ??= ObjectId.GenerateNewId().ToString();
            var filter = Builders<MovieItem>.Filter.Eq(x => x.Id, movie.Id);
            await _movieCollection.ReplaceOneAsync(filter, movie, new ReplaceOptions { IsUpsert = true });
        }

        // Get movie by name
        public async Task<MovieItem> GetMovieByNameAsync(string movieName)
        {
            var filter = Builders<MovieItem>.Filter.Eq(x => x.MovieName, movieName);
            return (await _movieCollection.FindAsync(filter)).FirstOrDefault();
        }

        public async Task<List<MovieItem>> GetAllMovie()
        {
            return (await _movieCollection.FindAsync(_ => true)).ToList();
        }
    }
}
