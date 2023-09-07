using System;
using MongoConnector;
using MongoConnector.Models;
using MongoDB.Driver;

namespace SimpleServer.src.Movie
{
    public class MovieService : IMovieService
    {
        private readonly MongoDbService _mongoService;
        public MovieService(MongoDbService mongoService)
        {
            _mongoService = mongoService;
        }

        // get all movies from the database
        public async Task<List<Movies>> GetAllMoviesAsync()
        {
            return (await _mongoService.Movies.FindAsync(movie => true)).ToList();
        }
    }
}

