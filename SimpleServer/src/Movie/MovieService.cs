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
        public async Task<List<List<Movies>>> GetAllMoviesAsync()
        {
            var movies = (await _mongoService.Movies.FindAsync(_ => true)).ToList().Take(30).ToList();
            var result = new List<List<Movies>>();
            int chunkSize = 5;

            for (int i = 0; i < movies.Count; i += chunkSize)
            {
                List<Movies> chunk = movies
                    .Skip(i)
                    .Take(chunkSize)
                    .ToList();

                result.Add(chunk);
            }

            return result;
        }
    }
}

