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

        public async Task TestCronJob()
        {
            List<MovieItem> item = new(){
                new MovieItem(){
                    MovieName = "Tết Ở Làng Địa Ngục",
                    Poster = "https://storage.googleapis.com/simple-netflix/Tet-O-Lang-%C4%90ia-Nguc/poster.jpg",
                    Status = "Tập 10 HD",
                    StreamingUrls = new Dictionary<string, string>{
                        {"1", "https://www.googleapis.com/storage/v1/b/simple-netflix/o/Tet-O-Lang-%C4%90ia-Nguc%2Fepisode-1%2Findex.m3u8"},
                        {"2", "https://www.googleapis.com/storage/v1/b/simple-netflix/o/Tet-O-Lang-%C4%90ia-Nguc%2Fepisode-2%2Findex.m3u8"},
                        {"3", "https://www.googleapis.com/storage/v1/b/simple-netflix/o/Tet-O-Lang-%C4%90ia-Nguc%2Fepisode-3%2Findex.m3u8"},
                        {"4", "https://www.googleapis.com/storage/v1/b/simple-netflix/o/Tet-O-Lang-%C4%90ia-Nguc%2Fepisode-4%2Findex.m3u8"},
                        {"5", "https://www.googleapis.com/storage/v1/b/simple-netflix/o/Tet-O-Lang-%C4%90ia-Nguc%2Fepisode-5%2Findex.m3u8"}
                    },
                    UrlDetail = "https://phimmoiyyy.net/phim-bo/tet-o-lang-dia-nguc-154213",
                    MovieCategory = Category.Movies,
                    Description = "Tết Ở Làng Địa Ngục (Netflix) Full HD Trọn bộTết Ở Làng Địa Ngục các hậu duệ của một băng cướp khét tiếng điều tra hàng loạt án mạng tàn bạo ở làng của họ. Liệu đây là nghiệp chướng hay “tác phẩm” của kẻ báo thù?",
                    Tags = new List<Tag>(){Tag.Hot, Tag.NewSeries},
                    IsAvailable = false,
                    AvailableEpisode = 5,
                    UpdatedAt = DateTime.Now
                }
            };
            await PublishMoviesAsync(item);
            Console.WriteLine("TestCronJob Done!");
        }

        public async Task CrawlMovieDataAsync()
        {
            //CRAWL MOVIE INFOs
            List<MovieItem> movies = CrawlHelper.CrawlMovieInfo(Consts.MOVIE_WEBSITE_URL);
            if (movies == null || movies.Count == 0)
            {
                Log.Error("No movie found!");
                return;
            }

            List<MovieItem> moviesWithNonNullStreamingUrls = await MovieHelper.GetMoviesWithStreamingUrls(Category.Movies, _database);
            // get movies with category is TVShows which all streamingUrls value is not null
            List<MovieItem> tvShowsWithFullNonNullStreamingUrls = await MovieHelper.GetMoviesWithStreamingUrls(Category.TVShows, _database);

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
            //     tvShowsWithFullNonNullStreamingUrls = await MovieHelper.GetMoviesWithStreamingUrls(Category.TVShows,_database);
            //     moviesWithNonNullStreamingUrls = await MovieHelper.GetMoviesWithStreamingUrls(Category.Movies, _database);
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
                moviesWithNonNullStreamingUrls = await MovieHelper.GetMoviesWithStreamingUrls(Category.Movies, _database);
                tvShowsWithFullNonNullStreamingUrls = await MovieHelper.GetMoviesWithStreamingUrls(Category.TVShows, _database);
                if (tvShowsWithFullNonNullStreamingUrls.Any(tvShow => tvShow.StreamingUrls.Count + moviesWithNonNullStreamingUrls.Count >= Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY))
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
                    Log.Information($"Movie {movie.MovieName} published to MassTransit-RabbitMQ successfully!");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"PublishMoviesAsync error: {ex.Message}");
            }
        }
    }
}
