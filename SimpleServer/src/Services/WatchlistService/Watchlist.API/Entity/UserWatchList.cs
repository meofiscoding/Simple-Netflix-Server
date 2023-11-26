namespace Watchlist.API.Entity
{
    public class UserWatchList
    {
        public string UserId { get; set; } = string.Empty,

        public List<string> SavedMovieIds { get; set; } = new()
    }
}
