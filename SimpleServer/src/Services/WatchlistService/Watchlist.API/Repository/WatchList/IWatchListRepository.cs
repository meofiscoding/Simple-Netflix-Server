using System;

namespace Watchlist.API.Repository.WatchList
{
    public interface IWatchListRepository
    {
        // Add movie to watchlist
        Task InsertToWatchedListAsync(string userId, string movieId);
        // Remove movie from watchlist
        Task RemoveFromWatchedListAsync(string userId, string movieId);
        // Get watchlist
        Task<List<string>> GetWatchListAsync(string userId);
    }
}
