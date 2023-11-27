using System;
using MongoDB.Driver;
using Watchlist.API.Entity;

namespace Watchlist.API.Infrastructure.Data
{
    public class WatchListContext : IWatchListContext
    {
        public WatchListContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));
            SavedMovies = database.GetCollection<UserWatchList>("userSavedList");
            UnfinishedMovies = database.GetCollection<UserUnfinishedMovies>("userUnfinishedMovies");
        }

        public WatchListContext()
        {
        }


        public IMongoCollection<UserWatchList> SavedMovies { get; }

        public IMongoCollection<UserUnfinishedMovies> UnfinishedMovies { get; }
    }
}
