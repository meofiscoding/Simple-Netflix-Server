using System;
using Movie.API.Infrastructure.Data;
using Movie.API.Models;
using MongoDB.Driver;

namespace Movie.API.Repository
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IMovieContext _context;
        const int chunkSize = 5;

        public MovieRepository(IMovieContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<List<MovieInformation>>> GetAllMoviesAsync()
        {
            var cursor = await _context.Movies.FindAsync(_ => true);
            if (cursor.Current == null)
            {
                return new List<List<MovieInformation>>();
            }
            var movies = await cursor.ToListAsync();

            var result = new List<List<MovieInformation>>();

            for (int i = 0; i < movies.Count; i += chunkSize)
            {
                List<MovieInformation> chunk = movies
                    .Skip(i)
                    .Take(chunkSize)
                    .ToList();

                result.Add(chunk);
            }

            return result;
        }
    }
}
