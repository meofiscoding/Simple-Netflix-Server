using System;

namespace Watchlist.API.Entity
{
    public class UserWatchList
    {
        public string UserId { get; set; }
        public List<string> SavedMovies { get; set; }
    }
}
