// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Movie.API.Models;
using Movie.API.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SimpleServer.src.Movie;

[Route("api/[controller]")]
public class MovieController : Controller
{
    private readonly IMovieRepository _movieService;
    private readonly ILogger<MovieController> _logger;

    public MovieController(IMovieRepository movieService, ILogger<MovieController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    // GET: api/values
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MovieInformation>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<List<MovieInformation>>>> GetAllMoviesAsync()
    {
        return Ok(await _movieService.GetAllMoviesAsync());
    }
}
