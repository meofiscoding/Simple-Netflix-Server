using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Watchlist.API.Repository.WatchList;

namespace Watchlist.API.Controllers
{
    [Authorize(Roles = "Member")]
    [Route("api/watchList")]
    public class SavedWatchListController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWatchListRepository _watchListService;
        private readonly ILogger<SavedWatchListController> _logger;

        public SavedWatchListController(IWatchListRepository watchListService, ILogger<SavedWatchListController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _watchListService = watchListService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult<List<string>>> GetWatchListAsync()
        {
            var userId = GetUserId();
            return Ok(await _watchListService.GetWatchListAsync(userId));
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? throw new ArgumentNullException("User not found");
        }

        [HttpPost]
        public async Task<ActionResult> AddMovieToWatchListAsync([FromBody] string movieId)
        {
            var userId = GetUserId();
            await _watchListService.InsertToWatchedListAsync(userId, movieId);
            return Ok();
        }

        [HttpDelete("{movieId}")]
        public async Task<ActionResult> RemoveMovieFromWatchListAsync(string movieId)
        {
            var userId = GetUserId();
            await _watchListService.RemoveFromWatchedListAsync(userId, movieId);
            return Ok();
        }
    }
}
