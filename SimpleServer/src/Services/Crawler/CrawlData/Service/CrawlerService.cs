using EventBus.Message.Common.Enum;
using CrawlData.Helper;
using CrawlData.Model;
using Serilog;
using MassTransit;
using AutoMapper;
using EventBus.Message.Events;

namespace CrawlData.Service
{
    public class CrawlerService : ICrawlerService
    {
        private readonly MongoHelper _database;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        public CrawlerService(MongoHelper database, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _database = database;
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public void TestCronJob()
        {
            Console.WriteLine("TestCronJob");
        }

        public async Task CrawlMovieData()
        {
            //CRAWL MOVIE INFOs
            List<MovieItem> movies = CrawlHelper.CrawlMovieInfo(Consts.MOVIE_WEBSITE_URL);
            if (movies == null || movies.Count == 0)
            {
                Log.Error("No movie found!");
                return;
            }

            var moviesWithNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.Movies);
            // get movies with category is TVShows which all streamingUrls value is not null
            var tvShowsWithFullNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.TVShows);

            // CRAWL MOVIE DETAILs
            // loop until tvShowsWithFullNonNullStreamingUrls have an element that its number of streamingUrls + moviesWithNonNullStreamingUrls.Count = numberOfMoviesToPushEachDay
            // while (!tvShowsWithFullNonNullStreamingUrls.Any(tvShow => tvShow.StreamingUrls.Count + moviesWithNonNullStreamingUrls.Count >= Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY))
            // {
            //     foreach (var movie in movies)
            //     {
            //         // get movie from _database that has the same name as the movie
            //         var movieFromDB = await _database.GetMovieByNameAsync(movie.MovieName);
            //         var result = await CrawlHelper.CrawlMovieDetailAsync(movieFromDB ?? movie);
            //         // add the movie to the _database
            //         await _database.UpsertMovieAsync(result);
            //         Console.WriteLine($"Movie {result.MovieName} added to the _database successfully!!");
            //     }
            //     // get all movies from the _database
            //     movies = await _database.GetAllMovie();
            //     tvShowsWithFullNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.TVShows);
            // }

            // CRAWL MOVIE DETAILs Logic Updated
            foreach (var movie in movies)
            {
                // get movie from _database that has the same name as the movie
                var movieFromDB = await _database.GetMovieByNameAsync(movie.MovieName);
                var result = await CrawlHelper.CrawlMovieDetailAsync(movieFromDB ?? movie);
                // add the movie to the _database
                await _database.UpsertMovieAsync(result);
                Console.WriteLine($"Movie {result.MovieName} added to the _database successfully!!");
                moviesWithNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.Movies);
                tvShowsWithFullNonNullStreamingUrls = MovieHelper.GetMoviesWithStreamingUrls(movies, Category.TVShows);
                if(tvShowsWithFullNonNullStreamingUrls.Any(tvShow => tvShow.StreamingUrls.Count + moviesWithNonNullStreamingUrls.Count >= Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY))
                {
                    break;
                }
            }

            // PUSH MOVIE ASSET TO GCS
            // Only push 5 item of movie to GCS in each day
            // Movie can be both in Movies and TVShows category
            if (moviesWithNonNullStreamingUrls.Count == 0 && tvShowsWithFullNonNullStreamingUrls.Count == 0)
            {
                Log.Error("No movie to push to GCS today!");
            }
            else
            {
                var moviesToPushToGCS = MovieHelper.GetMoviesToPushToGCS(moviesWithNonNullStreamingUrls, tvShowsWithFullNonNullStreamingUrls);
                var result = await MovieHelper.PushMovieAssetToGCS(moviesToPushToGCS, _database);
                // Send event to MassTransit-RabbitMQ
                await PublishMoviesAsync(result);
            }
            Console.WriteLine("Done crawling movie data!");
        }

        private async Task PublishMoviesAsync(List<MovieItem> moviesToPushToGCS)
        {
            try
            {
                // map each MovieItem from list moviesToPushToGCS to TransferMovieListEvent 
                // and publish each TransferMovieListEvent to MassTransit-RabbitMQ
                foreach (var movie in moviesToPushToGCS)
                {
                    var moviePublishedEvent = _mapper.Map<TransferMovieListEvent>(movie);
                    await _publishEndpoint.Publish(moviePublishedEvent);
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"PublishMoviesAsync error: {ex.Message}");
            }
        }
    }
}
