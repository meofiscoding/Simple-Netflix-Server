using System;
using System.Net;
using SimpleServer.src.Movie;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using MongoConnector.Models;

namespace SimpleServer.Test.System.Controller
{
    [TestFixture]
    public class MovieControllerIntegrationTests
    {
        private HttpClient _client;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var factory = new WebApplicationFactory<Program>();
            _client = factory.CreateClient();
        }

        [Test]
        // naming convention: MethodName_StateUnderTest_ExpectedBehavior
        public async Task GetAllMoviesAsync_WhenCalled_ReturnsOkResult()
        {
            // Arrange

            // Act
            var httpResponse = await _client.GetAsync("/api/Movie");

            // Assert
            httpResponse.EnsureSuccessStatusCode(); // Status Code 200-299

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var movies = JsonConvert.DeserializeObject<List<List<Movies>>>(stringResponse);
            if(movies.Count > 0){
                // if movie count is divisible by 5, then all chunk will have 5 elements
                if(movies.Count % 5 == 0){
                    movies.ForEach(movie => Assert.That(movie.Count, Is.EqualTo(5)));
                }else{
                    // if movie count is not divisible by 5, then last chunk will have less than 5 elements
                    Assert.That(movies[movies.Count - 1].Count, Is.LessThan(5));
                }
            }
        }
    }
}
