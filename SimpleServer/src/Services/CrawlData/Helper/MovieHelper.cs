using System;
using CrawlData.Enum;
using CrawlData.Model;

namespace CrawlData.Helper
{
    public class MovieHelper
    {
        public static void AssignMovieTag(MovieItem movie, string parent)
        {
            if (parent.Contains("featured-titles") && movie.Tags.All(x => x != Tag.Hot))
            {
                movie.Tags.Add(Tag.Hot);
            }
            else if (parent.Contains("genre_phim-chieu-rap") && movie.Tags.All(x => x != Tag.Cinema))
            {
                movie.Tags.Add(Tag.Cinema);
            }
            else if (parent.Contains("dt-tvshows") && movie.Tags.All(x => x != Tag.NewSeries))
            {
                movie.Tags.Add(Tag.NewSeries);
            }
            else if (parent.Contains("dt-movies") && movie.Tags.All(x => x != Tag.NewFeaturedMovies))
            {
                movie.Tags.Add(Tag.NewFeaturedMovies);
            }
        }
    }
}
