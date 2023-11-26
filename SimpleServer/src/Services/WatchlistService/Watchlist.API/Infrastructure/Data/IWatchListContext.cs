using MongoDB.Driver;
using Watchlist.API.Entity;

namespace Watchlist.API.Infrastructure.Data
{
    public interface IWatchListContext
    {
        IMongoCollection<UserWatchList> SavedMovies { get; }
        IMongoCollection<UserUnfinishedMovies> UnfinishedMovies { get; }
    }
}
