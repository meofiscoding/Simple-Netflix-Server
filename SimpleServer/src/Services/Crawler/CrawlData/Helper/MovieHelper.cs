using System;
using System.Globalization;
using System.Text;
using EventBus.Message.Common.Enum;
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

        public static async Task<List<MovieItem>> PushMovieAssetToGCS(List<MovieItem> movies, MongoHelper _database)
        {
            List<MovieItem> result = new();
            foreach (var movie in movies)
            {
                try
                {
                    // convert movie name from Biệt Đội Đánh Thuê 4 to Biet-doi-danh-thue-4
                    movie.MovieName = ConvertMovieNameToUrlFriendly(movie.MovieName);

                    if (movie.MovieCategory == Category.Movies)
                    {
                        movie.StreamingUrls["0"] = await HLSHandler.UploadHLSStream(movie.StreamingUrls["0"], movie.MovieName);
                        // update availableEpisode and IsAvailable property
                        movie.AvailableEpisode = 1;
                        movie.IsAvailable = true;
                    }
                    else
                    {
                        // get all streaming url that has key > AvailableEpisode
                        var streamingUrls = movie.StreamingUrls.Where(x => int.Parse(x.Key) > movie.AvailableEpisode).ToDictionary(x => x.Key, x => x.Value);
                        // if stramingUrls.Count > 5 => get first 5 streaming url
                        streamingUrls = streamingUrls.Count > Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY ? streamingUrls.Take(Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY).ToDictionary(x => x.Key, x => x.Value) : streamingUrls;
                        // upload movie streaming url in dictionary to GCS
                        foreach (var (key, value) in streamingUrls)
                        {
                            movie.StreamingUrls[key] = await HLSHandler.UploadHLSStream(value, movie.MovieName, $"episode-{key}");
                        }
                        // update availableEpisode and IsAvailable property
                        movie.AvailableEpisode += streamingUrls.Count;
                        movie.IsAvailable = movie.AvailableEpisode == movie.StreamingUrls.Count;
                    }

                    // Check if poster is exist on CGS
                    if (!GCSHelper.IsFileExist($"{movie.MovieName}/poster.jpg") && !string.IsNullOrEmpty(movie.Poster))
                    {
                        // upload movie poster tp GCS
                        movie.Poster = GCSHelper.UploadFile(movie.Poster, $"{movie.MovieName}/poster.jpg");
                    }

                    _database.UpdateMovie(movie);
                    result.Add(movie);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error when crawling streaming url for movie {movie.MovieName}");
                }
            }
            return result;
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

        public static async Task<List<MovieItem>> GetMoviesWithStreamingUrls(Category category, MongoHelper _database)
        {
            var movies = await _database.GetAllMovie();
            return movies.Where(x => x.MovieCategory == category && x.StreamingUrls.Count > 0 && x.StreamingUrls.All(y => !string.IsNullOrEmpty(y.Value)) && !x.IsAvailable).ToList();
        }

        public static List<MovieItem> GetMoviesToPushToGCS(List<MovieItem> moviesWithNonNullStreamingUrls, List<MovieItem> tvShowsWithFullNonNullStreamingUrls)
        {
            var numberOfMvoviesWithNonNullStreamingUrls = moviesWithNonNullStreamingUrls.Count;
            var moviesToPushToGCS = new List<MovieItem>();
            if (numberOfMvoviesWithNonNullStreamingUrls >= Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY)
            {
                // Get first 5 url of movie from Movies category to push to GCS
                moviesToPushToGCS = moviesWithNonNullStreamingUrls.Take(Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY).ToList();
            }
            else if (numberOfMvoviesWithNonNullStreamingUrls > 0 && numberOfMvoviesWithNonNullStreamingUrls < Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY)
            {
                moviesToPushToGCS = moviesWithNonNullStreamingUrls;
                // get tvshow from tvShowsWithFullNonNullStreamingUrls that has less episode than Consts.Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY
                MovieItem tvShowToPushToGCS = tvShowsWithFullNonNullStreamingUrls.Find(tvShow => tvShow.StreamingUrls.Count - tvShow.AvailableEpisode <= Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY);
                if (tvShowToPushToGCS != null)
                {
                    moviesToPushToGCS.Add(tvShowToPushToGCS);
                }
            }
            else
            {
                if (tvShowsWithFullNonNullStreamingUrls.Count > 0)
                {
                    // get tvshow from tvShowsWithFullNonNullStreamingUrls that its streamingUrls.Count - AvailableEpiside <= Consts.Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY
                    List<MovieItem> tvShowsToPushToGCS = tvShowsWithFullNonNullStreamingUrls.FindAll(tvShow => tvShow.StreamingUrls.Count - tvShow.AvailableEpisode <= Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY);
                    // get tvshow from tvShowsWithFullNonNullStreamingUrls that has min number of unavailable episode
                    if (tvShowsToPushToGCS.Count == 0)
                    {
                        MovieItem tvShowToPushToGCS = tvShowsWithFullNonNullStreamingUrls.Find(tvShow => tvShow.StreamingUrls.Count - tvShow.AvailableEpisode == tvShowsWithFullNonNullStreamingUrls.Min(x => x.StreamingUrls.Count - x.AvailableEpisode));
                        return new List<MovieItem>() { tvShowToPushToGCS };
                    }
                    // get number of unavailable episode of each tvShow in tvShowToPushToGCS
                    Dictionary<MovieItem, int> unavailableEpisodeOfTvShow = tvShowsToPushToGCS.ToDictionary(tvShow => tvShow, tvShow => tvShow.StreamingUrls.Count - tvShow.AvailableEpisode);
                    // Handlecase if number of episode of tvShowToPushToGCS is greater than Consts.Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY
                    // find combination of element in unavailableEpisodeOfTvShow that sum of unavailable episode is equal to Consts.Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY
                    var combinationOfTvShowToPushToGCS = FindCombinationOfTvShowToPushToGCS(unavailableEpisodeOfTvShow);
                    if (combinationOfTvShowToPushToGCS != null)
                    {
                        moviesToPushToGCS = combinationOfTvShowToPushToGCS;
                    }
                }
            }

            return moviesToPushToGCS;
        }

        public static List<MovieItem>? FindCombinationOfTvShowToPushToGCS(Dictionary<MovieItem, int> unavailableEpisodeOfTvShow)
        {
            // If there is an element that already has number of unavailable episode equal to Consts.Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY
            if (unavailableEpisodeOfTvShow.Any(x => x.Value == Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY))
            {
                // return first element that has number of unavailable episode equal to Consts.Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY
                return new List<MovieItem>() { unavailableEpisodeOfTvShow.First(x => x.Value == Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY).Key };
            }
            else if (unavailableEpisodeOfTvShow.Count == 1)
            {
                return new List<MovieItem>() { unavailableEpisodeOfTvShow.First().Key };
            }
            else
            {
                // find combination of element in unavailableEpisodeOfTvShow that sum of unavailable episode is equal to Consts.Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY
                var combinationOfTvShowToPushToGCS = new List<MovieItem>();
                var sumOfUnavailableEpisode = 0;
                // sort unavailableEpisodeOfTvShow by value
                var sortedUnavailableEpisodeOfTvShow = unavailableEpisodeOfTvShow.OrderBy(x => x.Value).ToList();
                for (int i = 0; i < sortedUnavailableEpisodeOfTvShow.Count; i++)
                {
                    // If sum is greater than Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY + Consts.ACCEPTABLE_ERROR_RANGE
                    // Case i == 0 => return sortedUnavailableEpisodeOfTvShow[i].Key
                    // Case i > 0 => return combinationOfTvShowToPushToGCS that sum of unavailable episode is in the acceptable error range
                    sumOfUnavailableEpisode += sortedUnavailableEpisodeOfTvShow[i].Value;
                    combinationOfTvShowToPushToGCS.Add(sortedUnavailableEpisodeOfTvShow[i].Key);

                    if (sumOfUnavailableEpisode > Consts.NUMBER_OF_MOVIE_TO_PUSH_EACH_DAY + Consts.ACCEPTABLE_ERROR)
                    {
                        if (i == 0)
                        {
                            return new List<MovieItem>() { sortedUnavailableEpisodeOfTvShow[i].Key };
                        }
                        else
                        {
                            combinationOfTvShowToPushToGCS.Remove(sortedUnavailableEpisodeOfTvShow[i].Key);
                            return combinationOfTvShowToPushToGCS;
                        }
                    }
                }
                return null;
            }
        }
    }
}
