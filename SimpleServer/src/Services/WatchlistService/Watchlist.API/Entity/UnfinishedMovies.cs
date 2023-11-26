
namespace Watchlist.API.Entity
{
    public class UnfinishedMovies
    {
        public string MovieId { get; set; } = string.Empty;

        // total duration (in minutes)
        public int ProgressDuarion { get; set; }

        // Last watched
        public DateTime LastWatched { get; set; }
    }
}
