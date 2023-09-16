using System;
using System.Net;
using SimpleServer.src.Movie;
using Microsoft.AspNetCore.Mvc.Testing;

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
        }
    }
}
