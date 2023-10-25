using System;
using MongoDB.Driver;
using Movie.API.Models;
using Newtonsoft.Json;

namespace Movie.API.Infrastructure.Data
{
    public class MovieContext : IMovieContext
    {
        public MovieContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
            Movies = database.GetCollection<MovieInformation>(configuration.GetValue<string>("DatabaseSettings:CollectionName"));
            // seed data
            SeedMovies();
        }

        public MovieContext()
        {
        }

        private async void SeedMovies()
        {
            var moviesData = await this.Movies.Find(_ => true).ToListAsync();
            if (moviesData.Count == 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Data", "movies.json");
                // var data = File.ReadAllText("../MongoConnector/Seeder/movies.json");
                var data = File.ReadAllText(path);
                var movies = JsonConvert.DeserializeObject<List<MovieInformation>>(data);
                await this.Movies.InsertManyAsync(movies);
            }
        }

        public IMongoCollection<MovieInformation> Movies { get; }
    }
}
