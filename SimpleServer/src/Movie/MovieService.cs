// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MongoConnector;
using MongoConnector.Models;
using MongoDB.Driver;

namespace SimpleServer.src.Movie;

public class MovieService : IMovieService
{
    private readonly MongoDbService _mongoService;
    private readonly IMongoCollection<Movies> _movieCollection;

    public MovieService(MongoDbService mongoService, IMongoCollection<Movies> moviesCollection)
    {
        _mongoService = mongoService;
        _movieCollection = moviesCollection;
    }

    // get all movies from the database
    public async Task<List<List<Movies>>> GetAllMoviesAsync()
    {
        var cursor = await _movieCollection.FindAsync(_ => true);
        var movies = await cursor.ToListAsync();

        var result = new List<List<Movies>>();
        int chunkSize = 5;

        for (int i = 0; i < movies.Count; i += chunkSize)
        {
            List<Movies> chunk = movies
                .Skip(i)
                .Take(chunkSize)
                .ToList();

            result.Add(chunk);
        }

        return result;
    }
}

