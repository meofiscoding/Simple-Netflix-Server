using CrawlData.Helper;
using CrawlData.Model;
using Microsoft.Extensions.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    // Specify which file to write logs to and when need to create a new log file
    .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

MongoHelper database = new(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

List<MovieItem> movies = CrawlHelper.CrawlMovieInfo("https://phimmoiyyy.net/");
if (movies == null || movies.Count == 0)
{
    Log.Error("No movie found!");
    return;
}

// Add all movie to database
await database.Movies.InsertManyAsync(movies);

// TODO: Only push 5 item of movie to GCS in each day
// If movie category is Movies, then that movie will equal to 1 item
// If movie category is TVShows, then each episode of that movie will equal to 1 item
// TODO: Implement a method to get number of item in database

// Push movie asset to GCS
await MovieHelper.PushMovieAssetToGCS(movies);
// Write movie list to file
await FileHelper.ExtractDataToJsonAsync(movies);
