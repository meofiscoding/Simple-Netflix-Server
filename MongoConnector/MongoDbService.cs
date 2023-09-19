using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using MongoConnector.Models;

namespace MongoConnector
{
    public class MongoDbService
    {
        private IMongoClient MongoClient { get; }

        public IMongoDatabase MongoDatabase { get; }

        public MongoDbService()
        {
            var databaseURI = Environment.GetEnvironmentVariable("MongoDB_ConnectionURI");
            var databaseName = Environment.GetEnvironmentVariable("MongoDB_DatabaseName");

            if (databaseURI == null || databaseName == null)
            {
                throw new Exception("Cannot read MongoDB settings in appsettings");
            }

            var mongoDbSettings = MongoClientSettings.FromConnectionString(databaseURI);
            mongoDbSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
            mongoDbSettings.LinqProvider = MongoDB.Driver.Linq.LinqProvider.V3;

            MongoClient = new MongoClient(mongoDbSettings);
            MongoDatabase = MongoClient.GetDatabase(databaseName);
        }
        public MongoDbService(IMongoClient mongoClient)
        {
            this.MongoDatabase = mongoClient.GetDatabase("dbname");

        }

        public bool PingDatabase()
        {
            var result = MongoDatabase.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            return true;
        }

        // Seed data Diet
        public async Task SeedMovieData()
        {
            var moviesData = await this.Movies.Find(_ => true).ToListAsync();
            if (moviesData.Count == 0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Seeder", "movies.json");
                // var data = File.ReadAllText("../MongoConnector/Seeder/movies.json");
                var data = File.ReadAllText(path);
                var movies = JsonConvert.DeserializeObject<List<Movies>>(data);
                await this.Movies.InsertManyAsync(movies);
            }
        }

        public virtual IMongoCollection<Movies> Movies
        {
            get
            {
                return MongoDatabase.GetCollection<Movies>(MongoCollections.Movies);
            }
        }
    }
}
