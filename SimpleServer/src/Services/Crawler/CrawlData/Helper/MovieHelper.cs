using System;
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
            var semaphore = new SemaphoreSlim(4);

            // for each movie, crawl its streaming url
            var tasks = movies.Select(async movie =>
            {
                await semaphore.WaitAsync();
                try
                {
                    movie = await CrawlHelper.CrawlMovieDetailAsync(movie);

                    if (movie.StreamingUrls == null || movie.Poster == null)
                    {
                        Log.Error($"Error when crawling movie detail for movie {movie.MovieName}");
                        return;
                    }

                    if (movie.StreamingUrls.Count == 1)
                    {
                        await HLSHandler.UploadHLSStream(movie.StreamingUrls[0], movie.MovieName);
                    }
                    else
                    {
                        movie.StreamingUrls.ForEach(async url => await HLSHandler.UploadHLSStream(url, movie.MovieName, $"ep {movie.StreamingUrls.IndexOf(url) + 1}"));
                    }

                    // upload movie poster tp GCS
                    GCSHelper.UploadFile(Consts.bucketName, movie.Poster, $"{movie.MovieName}/poster.jpg");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error when crawling streaming url for movie {movie.MovieName}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
