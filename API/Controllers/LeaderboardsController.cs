using API.Authorization;
using API.DTOs;
using API.Services.Interfaces;
using API.Utilities.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class LeaderboardsController(IPlayerRatingsService playerRatingsService) : Controller
{
    /// <summary>
    /// Get a leaderboard of players which fit an optional request query
    /// </summary>
    /// <response code="200">Returns the leaderboard</response>
    [HttpGet]
    [Authorize(Roles = OtrClaims.Roles.User)]
    [ProducesResponseType<LeaderboardDTO>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LeaderboardDTO>> GetAsync(
        LeaderboardRequestQueryDTO requestQuery,
        LeaderboardFilterDTO filter,
        LeaderboardTierFilterDTO tierFilter
    )
    {
        requestQuery.Filter = filter;
        requestQuery.Filter.TierFilters = tierFilter;

        LeaderboardDTO leaderboard = await playerRatingsService.GetLeaderboardAsync(requestQuery);
        return Ok(leaderboard);
    }
}
