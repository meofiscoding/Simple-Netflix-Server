using System;
using CrawlData.Infrastructure.Data;
using MongoDB.Driver;

namespace Crawler.CrawlData.Repository
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IMovieContext _context;
        const int chunkSize = 5;

        public MovieRepository(IMovieContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

       
    }
}
