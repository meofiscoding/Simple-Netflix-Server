using System;
using Movie.API.Models;

namespace Movie.API.Repository
{
    public interface IMovieRepository
    {
       public Task<List<List<MovieInformation>>> GetAllMoviesAsync(); 
    }
}
