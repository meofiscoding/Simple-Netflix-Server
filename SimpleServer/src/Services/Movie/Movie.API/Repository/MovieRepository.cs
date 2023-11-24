using System;
using Movie.API.Infrastructure.Data;
using Movie.API.Models;
using MongoDB.Driver;
using EventBus.Message.Common.Enum;
using MongoDB.Bson;
using Diacritics.Extensions;

namespace Movie.API.Repository
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IMovieContext _context;
        const int chunkSize = 7;

        public MovieRepository(IMovieContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<List<MovieInformation>>> GetAllMoviesByCategoryAsync(Category category)
        {
            var cursor = await _context.Movies.FindAsync(movie => movie.MovieCategory == category);
            var movies = await cursor.ToListAsync();

            var result = new List<List<MovieInformation>>();

            for (int i = 0; i < movies.Count; i += chunkSize)
            {
                List<MovieInformation> chunk = movies
                    .Skip(i)
                    .Take(chunkSize)
                    .ToList();

                result.Add(chunk);
            }

            return result;
        }

        public Task UpsertMovieAsync(MovieInformation movie)
        {
            // filter by name
            var filter = Builders<MovieInformation>.Filter.Eq(movie.MovieName, movie.MovieName);
            return _context.Movies.ReplaceOneAsync(filter, movie, new ReplaceOptions { IsUpsert = true });
        }

        public Dictionary<int, string> GetAllCategories()
        {
            return Enum.GetValues(typeof(Category)).Cast<Category>().ToDictionary(t => (int)t, t => t.ToString());
        }

        private static TEnum ParseStringToEnum<TEnum>(string value) where TEnum : struct
        {
            if (Enum.TryParse(value, out TEnum result))
            {
                return result;
            }
            // Handle parsing error or return a default value
            throw new ArgumentException($"Failed to parse '{value}' to enum type {typeof(TEnum)}");
        }

        public List<List<MovieInformation>> GetAllMoviesByTag(string tag)
        {
            var tagEnum = ParseStringToEnum<EventBus.Message.Common.Enum.Tag>(tag);
            var cursor = _context.Movies.Find(movie => movie.Tags.Contains(tagEnum));
            var movies = cursor.ToList();

            var result = new List<List<MovieInformation>>();

            for (int i = 0; i < movies.Count; i += chunkSize)
            {
                List<MovieInformation> chunk = movies
                    .Skip(i)
                    .Take(chunkSize)
                    .ToList();

                result.Add(chunk);
            }

            return result;
        }

        public async Task<MoviePlayerModel> GetMovieByIdAsync(string id)
        {
            var movie = (await _context.Movies.FindAsync(movie => movie.Id.Equals(id))).FirstOrDefault();
            return new MoviePlayerModel
            {
                StreamingUrls = movie.StreamingUrls.Values.ToList(),
                IsSeries = movie.MovieCategory == Category.TVShows,
                Poster = movie.Poster
            };
        }

        public List<MovieInformation> GetAllMoviesByCategory(int category)
        {
            var cursor = _context.Movies.Find(movie => movie.MovieCategory == (Category)category);
            return cursor.ToList();
        }

        public List<string> GetAllTags()
        {
            return Enum.GetValues(typeof(EventBus.Message.Common.Enum.Tag)).Cast<EventBus.Message.Common.Enum.Tag>().Select(v => v.ToString()).ToList();
        }

        public async Task<MovieSearchResult> SearchMoviesAsync(MovieSearchQueryModel queryModel)
        {
            var filterBuilder = Builders<MovieInformation>.Filter;
            var filter = filterBuilder.Empty;

            if (queryModel.Category != null)
            {
                filter &= filterBuilder.Eq(nameof(MovieInformation.MovieCategory), queryModel.Category);
            }

            // TODO: implement full-text search
            if (!string.IsNullOrEmpty(queryModel.Query))
            {
                var allMovies = (await _context.Movies.FindAsync(_ => true)).ToList();
                // remove diacritics
                var query = queryModel.Query.RemoveDiacritics();
                var filteredMovies = allMovies
                    .Where(
                        movie => movie.MovieName
                            .RemoveDiacritics()
                            // remove whitespaces
                            .Replace(" ", "")
                            .Contains(queryModel.Query, StringComparison.OrdinalIgnoreCase)
                    )
                    .Select(movie => movie.Id)
                    .ToList();
                filter &= filterBuilder.In(nameof(MovieInformation.Id), filteredMovies);
            }

            if (filter == filterBuilder.Empty)
            {
                return new MovieSearchResult
                {
                    Data = new List<DataResult>(),
                    TotalResults = 0
                };
            }

            var movies = (await _context.Movies.FindAsync(filter)).ToList();
            var dataResults = movies.ConvertAll(movie => new DataResult
            {
                Id = movie.Id,
                Title = movie.MovieName,
                Poster = movie.Poster,
                Description = movie.Description ?? ""
            });

            // pagination result, Constants.Limit per page
            dataResults = dataResults.Skip(queryModel.Page * Constants.LIMIT).Take(Constants.LIMIT).ToList();

            return new MovieSearchResult
            {
                Data = dataResults,
                TotalResults = movies.Count
            };
        }
    }
}
