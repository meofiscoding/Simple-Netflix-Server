// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using EventBus.Message.Common.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movie.API.Models;
using Movie.API.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SimpleServer.src.Movie;

[Authorize(Roles = "Member")]
[Route("api/")]
public class MovieController : Controller
{
    private readonly IMovieRepository _movieService;
    private readonly ILogger<MovieController> _logger;

    public MovieController(IMovieRepository movieService, ILogger<MovieController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    // Get all category
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
    public ActionResult<Dictionary<int, string>> GetAllCategories()
    {
        return Ok(_movieService.GetAllCategories());
    }

    // Get category by id
    [HttpGet("category/{id}")]
    [ProducesResponseType(typeof(IEnumerable<MovieInformation>), (int)HttpStatusCode.OK)]
    public ActionResult<string> GetCategoryById(int id)
    {
        string category = ((Category)id).ToString();
        return Ok(new { category });
    }

    // Seach movie
    [HttpGet("movies/search")]
    [ProducesResponseType(typeof(MovieSearchResult), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<MovieSearchResult>> SearchMovies([FromQuery] MovieSearchQueryModel queryParams)
    {
        return Ok(await _movieService.SearchMoviesAsync(queryParams));
    }

    // Get All Tags
    [HttpGet("tags")]
    [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
    public ActionResult<List<string>> GetAllTags()
    {
        return Ok(_movieService.GetAllTags());
    }

    // GET: api/values
    [HttpGet("tag/{tag}")]
    [ProducesResponseType(typeof(IEnumerable<MovieInformation>), (int)HttpStatusCode.OK)]
    public ActionResult<List<List<MovieInformation>>> GetAllMovies(string tag)
    {
        return Ok(_movieService.GetAllMoviesByTag(tag));
    }

    // GET api/values/5
    [HttpGet("movies/{id}")]
    [ProducesResponseType(typeof(MoviePlayerModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<MoviePlayerModel>> GetMovieById(string id)
    {
        return Ok(await _movieService.GetMovieByIdAsync(id));
    }
}
