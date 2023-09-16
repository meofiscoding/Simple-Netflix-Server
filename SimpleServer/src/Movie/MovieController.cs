// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using MongoConnector.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SimpleServer.src.Movie;

[Route("api/[controller]")]
public class MovieController : Controller
{
    private readonly IMovieService _movieService;
    public MovieController(IMovieService movieService)
    {
        _movieService = movieService;
    }
    // GET: api/values
    [HttpGet]
    public async Task<ActionResult<List<List<Movies>>>> GetAllMoviesAsync()
    {
        return Ok(await _movieService.GetAllMoviesAsync());
    }
}

