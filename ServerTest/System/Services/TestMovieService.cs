using System;
using AutoFixture;
using FakeItEasy;
using MongoConnector;
using MongoConnector.Models;
using MongoDB.Driver;
using NUnit.Framework;
using SimpleServer.src.Movie;

namespace SimpleServer.Test
{
    [TestFixture]
    public class TestMovieService
    {
        private MongoDbService _mongoService;
        private IMovieService _movieService;
        private IMongoCollection<Movies> _moviesCollection;

        [SetUp]
        public void Setup()
        {
            _mongoService = A.Fake<MongoDbService>();
            _moviesCollection = A.Fake<IMongoCollection<Movies>>();
            _movieService = new MovieService(_mongoService, _moviesCollection);
        }

        [TestCase(0, 0)]
        [TestCase(3, 1)]
		[TestCase(5, 1)]
		[TestCase(6, 2)]
		[TestCase(10, 2)]
		[TestCase(11, 3)]
		[TestCase(1000, 200)]
		// naming convention: MethodName_StateUnderTest_ExpectedBehavior
        public async Task GetAllMoviesAsync_WithDifferenceMoviesCount_ReturnsCorrectChunkSize(int count, int chunkSize)
        {
            // Arrange
            var movies = A.CollectionOfDummy<Movies>(count);

            var fakeCursor = A.Fake<IAsyncCursor<Movies>>();
            A.CallTo(() => fakeCursor.Current).Returns(movies);
            A.CallTo(() => fakeCursor.MoveNextAsync(default)).ReturnsNextFromSequence(true, false);

            try
            {
                A.CallTo(() => _moviesCollection
               .FindAsync(A<FilterDefinition<Movies>>._, A<FindOptions<Movies, Movies>>._, default))
               .Returns(fakeCursor);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            // Act: Perform the test.
            var result = await _movieService.GetAllMoviesAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(chunkSize));
        }
    }
}

