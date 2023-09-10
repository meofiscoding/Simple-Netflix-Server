// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MongoConnector.Models;

namespace SimpleServer.src.Movie;

public interface IMovieService
{
    public Task<List<List<Movies>>> GetAllMoviesAsync();
}

