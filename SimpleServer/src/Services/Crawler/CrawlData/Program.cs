using System.Collections.Concurrent;
using CrawlData;
using CrawlData.Helper;
using CrawlData.Model;
using CrawlData.Utils;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

List<MovieItem> movies = CrawlHelper.CrawlMovieInfo("https://phimmoiyyy.net/");
if (movies == null || movies.Count == 0)
{
    Console.WriteLine("No movie found");
    return;
}

var semaphore = new SemaphoreSlim(4);

// for each movie, crawl its streaming url
var tasks = movies.Select(async movie =>
{
    await semaphore.WaitAsync();
    try
    {
        movie = await CrawlHelper.CrawlMovieDetailAsync(movie);

        if (movie.StreamingUrls == null || movie.Poster == null)
        {
            Log.Error($"Error when crawling movie detail for movie {movie.MovieName}");
            return;
        }

        if (movie.StreamingUrls.Count == 1)
        {
            await HLSHandler.UploadHLSStream(movie.StreamingUrls[0], movie.MovieName);
        }
        else
        {
            movie.StreamingUrls.ForEach(async url => await HLSHandler.UploadHLSStream(url, movie.MovieName, $"ep {movie.StreamingUrls.IndexOf(url) + 1}"));
        }

        // upload movie poster tp GCS
        GCSHelper.UploadFile(Consts.bucketName, movie.Poster, $"{movie.MovieName}/poster.jpg");
    }
    catch (Exception ex)
    {
        Log.Error(ex, $"Error when crawling streaming url for movie {movie.MovieName}");
    }
    finally
    {
        semaphore.Release();
    }
});

await Task.WhenAll(tasks);
// Write movie list to file
await FileHelper.ExtractDataToJsonAsync(movies);
