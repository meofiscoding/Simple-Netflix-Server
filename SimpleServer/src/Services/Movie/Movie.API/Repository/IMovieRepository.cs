using System;
using EventBus.Message.Common.Enum;
using Movie.API.Models;

namespace Movie.API.Repository
{
    public interface IMovieRepository
    {
        Task UpsertMovieAsync(MovieInformation movie);
        Dictionary<int,string> GetAllCategories();
        public List<MovieInformation> GetAllMoviesByCategory(int category);
        List<List<MovieInformation>> GetAllMoviesByTag(string tag);
        Task<MoviePlayerModel> GetMovieByIdAsync(string id);
        List<string> GetAllTags();
        Task<MovieSearchResult> SearchMoviesAsync(MovieSearchQueryModel queryModel);
    }
}
