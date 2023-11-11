using System;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Movie.API.Infrastructure.Data;
using Movie.API.Models;
using Movie.API.Repository;

namespace Movie.UnitTests.Application
{
    [TestFixture]
    public class MovieRepositoryTest
    {
        private IMovieRepository _movieRepository;
        private IMovieContext _movieContext;
        private IMongoCollection<MovieInformation> _moviesCollection;
        [SetUp]
        public void Setup()
        {
            _movieContext = A.Fake<IMovieContext>();
            _movieRepository = new MovieRepository(_movieContext);
            _moviesCollection = A.Fake<IMongoCollection<MovieInformation>>();
        }

        [TestCase(0, 0)]
        [TestCase(3, 1)]
        [TestCase(5, 1)]
        [TestCase(6, 2)]
        // naming convention: MethodName_StateUnderTest_ExpectedBehavior
        public async Task GetAllMoviesAsync_DifferenceListMoviesCount_ReturnCorrectChunkSize(int inputLength, int expectedOutput)
        {
            try
            {
                // Arrange
                var movieList = A.CollectionOfDummy<MovieInformation>(inputLength);
                A.CallTo(() => _movieContext.Movies).Returns(_moviesCollection);
                FakeFindAsync(movieList);

                // Act
                var result = await _movieRepository.GetAllMoviesAsync();

                // Assert
                Assert.That(result, Has.Count.EqualTo(expectedOutput));
            }
            catch (System.Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        [Test]
        public async Task GetAllMoviesAsync_ListOfmovieWithKnownOrder_ReturnChunksOfMoviesWithPreservingOder()
        {
            try
            {
                // Arrange
                List<MovieInformation> movieList = new();
                movieList.AddRange(Enumerable
                                    .Range(0, 5)
                                    .Select(i => new MovieInformation
                                    {
                                        Id = i.ToString(),
                                    })
                                    .ToList()
                                );
                A.CallTo(() => _movieContext.Movies).Returns(_moviesCollection);
                FakeFindAsync(movieList);

                // Act
                var result = await _movieRepository.GetAllMoviesAsync();

                // Assert
                // Validate chunk with each element have id in order
                Assert.Multiple(() =>
                {
                    Assert.That(result, Has.Count.EqualTo(1));
                    Assert.That(result[0], Has.Count.EqualTo(5));
                    Assert.That(result[0][0].GetId(), Is.EqualTo("0"));
                    Assert.That(result[0][1].GetId(), Is.EqualTo("1"));
                    Assert.That(result[0][2].GetId(), Is.EqualTo("2"));
                    Assert.That(result[0][3].GetId(), Is.EqualTo("3"));
                    Assert.That(result[0][4].GetId(), Is.EqualTo("4"));
                });
            }
            catch (System.Exception ex)
            {
                Console.Write(ex.Message);
            }

        }

        [Test]
        public async Task GetAllMoviesAsync_ValidMovieInformation_ReturnValidMovieInformation()
        {
            // Arrange
            List<MovieInformation> movieList = new(){
                new MovieInformation(){
                    Id = "1",
                    BackdropPath = "backdropPath",
                    Genres = new List<string>(){ "genre1", "genre2"},
                    OriginalTitle = "originalTitle",
                    Overview = "overview",
                    PosterPath = "posterPath",
                    ReleaseDate = "releaseDate",
                    Title = "title"
                }
            };
            A.CallTo(() => _movieContext.Movies).Returns(_moviesCollection);
            FakeFindAsync(movieList);

            // Act
            var result = await _movieRepository.GetAllMoviesAsync();

            // Assert
            // Validate result should return a list of lists, where each inner list contains valid movie information.
            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result[0], Has.Count.EqualTo(1));
                Assert.That(result[0][0].GetId(), Is.EqualTo("1"));
                Assert.That(result[0][0].BackdropPath, Is.EqualTo("backdropPath"));
                Assert.That(result[0][0].Genres, Is.EqualTo(new List<string>(){ "genre1", "genre2"}));
                Assert.That(result[0][0].OriginalTitle, Is.EqualTo("originalTitle"));
                Assert.That(result[0][0].Overview, Is.EqualTo("overview"));
                Assert.That(result[0][0].PosterPath, Is.EqualTo("posterPath"));
                Assert.That(result[0][0].ReleaseDate, Is.EqualTo("releaseDate"));
                Assert.That(result[0][0].Title, Is.EqualTo("title"));
            });
        }

        [Test]
        public async Task GetAllMoviesAsync_NullMovieList_ReturnsEmptyList(){
            // Arrange
            List<MovieInformation>? movieList = null;
            A.CallTo(() => _movieContext.Movies).Returns(_moviesCollection);
            FakeFindAsync(movieList!);

            // Act
            var result = await _movieRepository.GetAllMoviesAsync();

            // Assert
            Assert.That(result, Is.Empty);
        }

        private void FakeFindAsync(IList<MovieInformation> movieList)
        {
            var fakeCursor = A.Fake<IAsyncCursor<MovieInformation>>();
            A.CallTo(() => fakeCursor.Current).Returns(movieList);
            A.CallTo(() => fakeCursor.MoveNextAsync(default)).ReturnsNextFromSequence(true, false);
            A.CallTo(() =>
                _moviesCollection
                .FindAsync(A<FilterDefinition<MovieInformation>>._, A<FindOptions<MovieInformation, MovieInformation>>._, default))
                .Returns(fakeCursor);
        }

    }
}
