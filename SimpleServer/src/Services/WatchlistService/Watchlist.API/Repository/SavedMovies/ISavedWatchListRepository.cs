using System;

namespace Watchlist.API.Repository.WatchList
{
    public interface ISavedWatchListRepository
    {
        // Add movie to watchlist
        Task InsertToSavedWatchedListAsync(string userId, string movieId);
        // Remove movie from watchlist
        Task RemoveFromSavedWatchedListAsync(string userId, string movieId);
        // Get watchlist
        Task<List<string>> GetSavedWatchListAsync(string userId);
    }
}
