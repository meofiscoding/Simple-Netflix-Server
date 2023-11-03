using CrawlData.Helper;
using CrawlData.Model;
using System.Collections.Concurrent;

List<MovieItem> movies = CrawlHelper.CrawlMovieInfo("https://phimmoiyyy.net/");
if (movies == null || movies.Count == 0)
{
    Console.WriteLine("No movie found");
    return;
}
int batchSize = 5; // Adjust the batch size as needed

var crawledData = new ConcurrentBag<MovieItem>();

Parallel.ForEach(
    movies,
    // limiting the parallelization level to 5 pages at a time 
    new ParallelOptions { MaxDegreeOfParallelism = batchSize },
    movie =>
    {
        var movieDetail = CrawlHelper.CrawlMovieDetailAsync(movie);
    }
);

