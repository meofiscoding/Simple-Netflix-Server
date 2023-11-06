using CrawlData.Enum;
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

MongoHelper database = new(new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());

List<MovieItem> movies = CrawlHelper.CrawlMovieInfo("https://phimmoiyyy.net/");
if (movies == null || movies.Count == 0)
{
    Log.Error("No movie found!");
    return;
}

var moviesWithNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.Movies);
// get movies with category is TVShows which all streamingUrls value is not null
var tvShowsWithFullNonNullStreamingUrls =MovieHelper.GetMoviesWithStreamingUrls(movies, Category.TVShows);
// crawl detail of each movie until total movie with streamingUrls >= 5
while (moviesWithNonNullStreamingUrls.Count + tvShowsWithFullNonNullStreamingUrls.Count < 5)
{
    foreach (var movie in movies)
    {
        // get movie from database that has same name with movie
        var movieFromDB = await database.GetMovieByNameAsync(movie.MovieName);
        var result = await CrawlHelper.CrawlMovieDetailAsync(movieFromDB ?? movie);
        // add movie to database
        await database.UpsertMovieAsync(result);
        Console.WriteLine($"Movie {result.MovieName} added to database successfully!!");
    }
    // get all movues from database
    movies = await database.GetAllMovie();
    moviesWithNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.Movies);
    tvShowsWithFullNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.TVShows);
}

// TODO: Only push 5 item of movie to GCS in each day
// If movie category is Movies, then that movie will equal to 1 item
// If movie category is TVShows, then each episode of that movie will equal to 1 item
// Push movie asset to GCS
// await MovieHelper.PushMovieAssetToGCS(movies);
// Write movie list to file
// await FileHelper.ExtractDataToJsonAsync(movies);
