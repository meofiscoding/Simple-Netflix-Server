using CrawlData.Helper;
using CrawlData.Model;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    // Specify which file to write logs to and when need to create a new log file
    .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

List<MovieItem> movies = CrawlHelper.CrawlMovieInfo("https://phimmoiyyy.net/");
if (movies == null || movies.Count == 0)
{
    Log.Error("No movie found!");
    return;
}

// Push movie asset to GCS
await MovieHelper.PushMovieAssetToGCS(movies);
// Write movie list to file
await FileHelper.ExtractDataToJsonAsync(movies);
