using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Watchlist.API.Controllers
{
    [Authorize(Roles = "Member")]
    [Route("api/unfinishedWatchList")]
    public class UnfinishedWatchListController : Controller
    {

    }
}
