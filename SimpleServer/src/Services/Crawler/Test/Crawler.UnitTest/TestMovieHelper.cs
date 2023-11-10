using CrawlData;
using EventBus.Message.Common.Enum;
using CrawlData.Helper;
using CrawlData.Model;
using FakeItEasy;

namespace Crawler.UnitTest
{
    [TestFixture]
    public class TestMovieHelper
    {
        [Test]
        [TestCase(5)]
        [TestCase(8)]
        public void TestGetMovieToPushToGCS_moviesWithNonNullStreamingUrlsGreaterThanOrEuqalTo5_ReturnCorrectNumberOfMovies(int numberOfMoviesWithNonNullStreamingUrls)
        {
            // Arrange
            // fake a list of movieOfNonNullStreamingUrls with all element had Category is Movies
            var moviesWithNonNullStreamingUrls = A.CollectionOfDummy<MovieItem>(numberOfMoviesWithNonNullStreamingUrls).ToList();
            // set MovieCategory of all item in moviesWithNonNullStreamingUrls is Movie
            moviesWithNonNullStreamingUrls.ForEach(x => x.MovieCategory = Category.Movies);

            var tvShowsWithFullNonNullStreamingUrls = A.CollectionOfDummy<MovieItem>(0).ToList();
            tvShowsWithFullNonNullStreamingUrls.ForEach(x => x.MovieCategory = Category.TVShows);
            // Act
            var result = MovieHelper.GetMoviesToPushToGCS(moviesWithNonNullStreamingUrls, tvShowsWithFullNonNullStreamingUrls);

            // Assert
            Assert.That(result, Has.Count.EqualTo(Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY));
            // Assert all element in result had category is Movies
            Assert.That(result, Has.All.Matches<MovieItem>(x => x.MovieCategory == Category.Movies));
        }

        // Basic Test Case - Movies Available, but Less than 5
        [Test]
        [TestCase(3)]
        [TestCase(4)]
        public void TestGetMovieToPushToGCS_moviesWithNonNullStreamingUrlsLessThan5_ReturnCorrectNumberOfMovies(int numberOfMoviesWithNonNullStreamingUrls)
        {
            // Arrange
            // fake a list of movieOfNonNullStreamingUrls with all element had Category is Movies
            var moviesWithNonNullStreamingUrls = A.CollectionOfDummy<MovieItem>(numberOfMoviesWithNonNullStreamingUrls).ToList();
            // set MovieCategory of all item in moviesWithNonNullStreamingUrls is Movie
            moviesWithNonNullStreamingUrls.ForEach(x => x.MovieCategory = Category.Movies);

            var tvShowsWithFullNonNullStreamingUrls = A.CollectionOfDummy<MovieItem>(0).ToList();
            tvShowsWithFullNonNullStreamingUrls.ForEach(x => x.MovieCategory = Category.TVShows);
            // Act
            var result = MovieHelper.GetMoviesToPushToGCS(moviesWithNonNullStreamingUrls, tvShowsWithFullNonNullStreamingUrls);

            // Assert
            Assert.That(result, Has.Count.EqualTo(numberOfMoviesWithNonNullStreamingUrls));
            // Assert all element in result had category is Movies
            Assert.That(result, Has.All.Matches<MovieItem>(x => x.MovieCategory == Category.Movies));
        }

        // Edge Case - Movies Not Available, TV Shows Available with No TV Shows Eligible
        [Test]
        public void TestGetMovieToPushToGCS_MovieNotAvailableTvShowAvailableWithNoTvShowEligible_ReturnEmptyList()
        {
            // Arrange
            // Setup Movies
            var moviesWithNonNullStreamingUrls = A.CollectionOfDummy<MovieItem>(0).ToList();
            moviesWithNonNullStreamingUrls.ForEach(x => x.MovieCategory = Category.Movies);

            Random random = new Random();
            // Arrange TvShows
            var tvShowsWithFullNonNullStreamingUrls = Enumerable
                .Range(1, 3)
                .Select(x => new MovieItem
                {
                    MovieCategory = Category.TVShows,
                    StreamingUrls = Enumerable
                        .Range(1, random.Next(Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY - 1))
                        .Select(x => new KeyValuePair<string, string>(x.ToString(), x.ToString()))
                        .ToDictionary(x => x.Key, x => x.Value),
                    AvailableEpisode = 0
                })
                .ToList();

            // Act
            var result = MovieHelper.GetMoviesToPushToGCS(moviesWithNonNullStreamingUrls, tvShowsWithFullNonNullStreamingUrls);

            // Assert 
            // All element in result had category is TVShows and StreamingUrls.Count is less than Consts.Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY
            Assert.That(result, Has.All.Matches<MovieItem>(x => x.MovieCategory == Category.TVShows && x.StreamingUrls.Count < Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY));
        }

        [Test]
        // [TestCase(1, 3, 5, 1, 1)]
        // [TestCase(5, 3, 5, 5, 0)]
        // [TestCase(3, 3, 5, 3, 0)]
        // [TestCase(0, 3, 5, 0, 1)]
        // [TestCase(3, 3, 0, 0, 0)]
        public void TestGetMovieToPushToGCS_MovieInRangeAndTvShowEpisodeEligible_ReturnOneTvShowAndOneMovie()
        {
            // Arrange
            // Setup Movies
            var moviesWithNonNullStreamingUrls = new List<MovieItem>(){
                new MovieItem(){
                    MovieCategory = Category.Movies,
                    StreamingUrls = new(){
                        {"1", "url 1"},
                    },
                    AvailableEpisode = 0
                }
            };

            // Arrange TvShows
            var tvShowsWithFullNonNullStreamingUrls = Enumerable
                .Range(1, 2)
                .Select(x => new MovieItem
                {
                    Id = x.ToString(),
                    MovieCategory = Category.TVShows,
                    StreamingUrls = Enumerable
                        .Range(1, Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY)
                        .Select(x => new KeyValuePair<string, string>(x.ToString(), $"url {x}"))
                        .ToDictionary(x => x.Key, x => x.Value),
                    AvailableEpisode = 0
                })
                .ToList();

            // Act
            var result = MovieHelper.GetMoviesToPushToGCS(moviesWithNonNullStreamingUrls, tvShowsWithFullNonNullStreamingUrls);

            // Assert 
            Assert.That(result, Has.Count.EqualTo(2));
            // result must contain one tv show with id = 1
            Assert.That(result, Has.One.Matches<MovieItem>(x => x.Id == "1" && x.MovieCategory == Category.TVShows));
            // result must contain one movie
            Assert.That(result, Has.One.Matches<MovieItem>(x => x.MovieCategory == Category.Movies));
        }
    
    }
}

