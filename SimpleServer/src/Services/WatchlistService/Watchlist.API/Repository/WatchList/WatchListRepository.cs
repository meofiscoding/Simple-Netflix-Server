using MongoDB.Driver;
using Watchlist.API.Entity;
using Watchlist.API.Infrastructure.Data;

namespace Watchlist.API.Repository.WatchList
{
    public class WatchListRepository : IWatchListRepository
    {
        private readonly IWatchListContext _context;

        public WatchListRepository(IWatchListContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<string>> GetWatchListAsync(string userId)
        {
            // Get all watch lists from database where userId == userId
            return (await _context.WatchLists.FindAsync(x => x.UserId == userId))
                .FirstOrDefault()?.SavedMovieIds ?? new List<string>();
        }

        public async Task InsertToWatchedListAsync(string userId, string movieId)
        {
            await _context.WatchLists.UpdateOneAsync(
                x => x.UserId == userId,
                Builders<UserWatchList>.Update.AddToSet(x => x.SavedMovieIds, movieId),
                new UpdateOptions { IsUpsert = true });
        }

        public async Task RemoveFromWatchedListAsync(string userId, string movieId)
        {
            await _context.WatchLists.UpdateOneAsync(
                x => x.UserId == userId,
                Builders<UserWatchList>.Update.Pull(x => x.SavedMovieIds, movieId));
        }
    }
}
