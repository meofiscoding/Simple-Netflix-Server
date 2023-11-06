using System;
using CrawlData.Model;

namespace Crawler.CrawlData.Repository
{
    public interface IMovieRepository
    {
       // Add movie to database
        public void AddRangesOfMovie(List<MovieItem> movieItems);
    }
}
