using System;
using CrawlData.Infrastructure.Data;
using CrawlData.Model;
using MongoDB.Driver;

namespace Crawler.CrawlData.Repository
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IMovieContext _context;

        public MovieRepository(IMovieContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void AddRangesOfMovie(List<MovieItem> movieItems)
        {
            try
            {
                _context.Movies.InsertMany(movieItems);
            }
            catch (System.Exception)
            {
                throw new InvalidOperationException("Cannot add range of movies to database");
            }
        }
    }
}
