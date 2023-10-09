using System;
using MongoDB.Driver;
using Movie.API.Models;

namespace Movie.API.Infrastructure.Data
{
    public class MovieContext : IMovieContext
    {
        public MovieContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
            Movies = database.GetCollection<MovieInformation>(configuration.GetValue<string>("DatabaseSettings:CollectionName"));
        }

        public IMongoCollection<MovieInformation> Movies {get;}
    }
}
