using MongoDB.Driver;
using Watchlist.API.Entity;
using Watchlist.API.Infrastructure.Data;

namespace Watchlist.API.Repository.WatchList
{
    public class SavedWatchListRepository : ISavedWatchListRepository
    {
        private readonly IWatchListContext _context;

        public SavedWatchListRepository(IWatchListContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<string>> GetSavedWatchListAsync(string userId)
        {
            // Get all watch lists from database where userId == userId
            return (await _context.SavedMovies.FindAsync(x => x.UserId == userId))
                .FirstOrDefault()?.SavedMovieIds ?? new List<string>();
        }

        public async Task InsertToSavedWatchedListAsync(string userId, string movieId)
        {
            await _context.SavedMovies.UpdateOneAsync(
                x => x.UserId == userId,
                Builders<UserWatchList>.Update.AddToSet(x => x.SavedMovieIds, movieId),
                new UpdateOptions { IsUpsert = true });
        }

        public async Task RemoveFromSavedWatchedListAsync(string userId, string movieId)
        {
            await _context.SavedMovies.UpdateOneAsync(
                x => x.UserId == userId,
                Builders<UserWatchList>.Update.Pull(x => x.SavedMovieIds, movieId));
        }
    }
}
