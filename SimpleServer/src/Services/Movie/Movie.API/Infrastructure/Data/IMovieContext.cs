using System;
using MongoDB.Driver;
using Movie.API.Models;

namespace Movie.API.Infrastructure.Data
{
    public interface IMovieContext
    {
        IMongoCollection<MovieInformation> Movies { get; }
    }
}
