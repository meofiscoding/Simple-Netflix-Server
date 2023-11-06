using System;
using System.Globalization;
using System.Text;
using CrawlData.Enum;
using CrawlData.Model;
using CrawlData.Utils;
using Serilog;

namespace CrawlData.Helper
{
    public static class MovieHelper
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

        public static async Task PushMovieAssetToGCS(List<MovieItem> movies)
        {
            foreach (var movie in movies)
            {
                try
                {
                    var result = await CrawlHelper.CrawlMovieDetailAsync(movie);

                    if (result.StreamingUrls == null || result.Poster == null)
                    {
                        Log.Error($"Error when crawling movie detail for movie {result.MovieName}");
                        return;
                    }

                    // convert movie name from Biệt Đội Đánh Thuê 4 to Biet-doi-danh-thue-4
                    movie.MovieName = ConvertMovieNameToUrlFriendly(result.MovieName);

                    if (result.MovieCategory == Category.Movies)
                    {
                        await HLSHandler.UploadHLSStream(result.StreamingUrls[0], result.MovieName);
                    }
                    else
                    {
                        // upload movie streaming url in dictionary to GCS
                        foreach (var (key, value) in result.StreamingUrls)
                        {
                            await HLSHandler.UploadHLSStream(value, result.MovieName, $"episode-{key}");
                        }
                    }

                    // upload movie poster tp GCS
                    GCSHelper.UploadFile(Consts.bucketName, result.Poster, $"{result.MovieName}/poster.jpg");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error when crawling streaming url for movie {movie.MovieName}");
                }
            }
        }

        private static string ConvertMovieNameToUrlFriendly(string movieName)
        {
            if (string.IsNullOrWhiteSpace(movieName))
            {
                return string.Empty;
            }

            // Remove diacritics (accents)
            string str = movieName.Normalize(NormalizationForm.FormD);
            StringBuilder result = new();

            foreach (char c in str)
            {
                // Remove diacritics (accents)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    // Keep characters that are not letters or digits
                    // For example: spaces, parentheses, hyphens, etc. will become hyphens 
                    // Ex: Biệt Đội Đánh Thuê 4 -> Biet-Doi-Danh-Thuê-4
                    // Ex: The Falcon@meofiscoding -> The-Falcon-meofiscoding
                    if (!char.IsLetterOrDigit(c))
                    {
                        result.Append('-');
                    }
                    else
                    {
                        result.Append(c);
                    }
                }
            }

            return result.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
