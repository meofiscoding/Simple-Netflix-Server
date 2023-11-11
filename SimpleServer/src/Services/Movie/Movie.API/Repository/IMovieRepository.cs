using System;
using Movie.API.Models;

namespace Movie.API.Repository
{
    public interface IMovieRepository
    {
        Task AddMovieAsync(MovieInformation movie);
        public Task<List<List<MovieInformation>>> GetAllMoviesAsync(); 
    }
}
