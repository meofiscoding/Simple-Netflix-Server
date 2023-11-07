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
var tvShowsWithFullNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.TVShows);
while (moviesWithNonNullStreamingUrls.Count < 5 && (tvShowsWithFullNonNullStreamingUrls.Count != 1 || (tvShowsWithFullNonNullStreamingUrls[0].StreamingUrls.Count < 5 && tvShowsWithFullNonNullStreamingUrls[0].StreamingUrls.Count + moviesWithNonNullStreamingUrls.Count < 5)))
{
    foreach (var movie in movies)
    {
        // get movie from database that has the same name as the movie
        var movieFromDB = await database.GetMovieByNameAsync(movie.MovieName);
        var result = await CrawlHelper.CrawlMovieDetailAsync(movieFromDB ?? movie);
        // add the movie to the database
        await database.UpsertMovieAsync(result);
        Console.WriteLine($"Movie {result.MovieName} added to the database successfully!!");
    }
    // get all movies from the database
    movies = await database.GetAllMovie();
    moviesWithNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.Movies);
    tvShowsWithFullNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.TVShows);
}

// TODO: Only push 5 item of movie to GCS in each day
// Push movie asset to GCS
// await MovieHelper.PushMovieAssetToGCS(movies);
