namespace Watchlist.API.Entity
{
    public class UserUnfinishedMovies
    {
        public string UserId { get; set; } = string.Empty;

        public List<UnfinishedMovies> UnfinishedMoviesList { get; set; } = new();
    }
}
