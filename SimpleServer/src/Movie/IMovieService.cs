using System;
using MongoConnector.Models;

namespace SimpleServer.src.Movie
{
	public interface IMovieService
	{
		public Task<List<List<Movies>>> GetAllMoviesAsync();
	}
}

