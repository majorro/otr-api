using API.DTOs;
using API.Enums;
using API.Services.Interfaces;
using API.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[EnableCors]
[Authorize(Roles = "user")]
[Route("api/[controller]")]
public class LeaderboardsController : Controller
{
	private readonly ILeaderboardService _leaderboardService;
	public LeaderboardsController(ILeaderboardService leaderboardService) { _leaderboardService = leaderboardService; }

	[HttpGet]
	[AllowAnonymous] // TODO: Frontend needs to have a dedicated client for these requests.
	public async Task<ActionResult<LeaderboardDTO>> GetAsync([FromQuery] LeaderboardRequestQueryDTO requestQuery)
	{
		/*
		 * Note:
		 *
		 * Due to model binding, the query is able to be called as such:
		 *
		 * ?bronze=true
		 * ?grandmaster=false&bronze=true
		 * ?mode=0&pagesize=25&minrating=500&maxwinrate=0.5
		 *
		 * This avoids annoying calls to ".Filter" in the query string (and .Filter.TierFilters for the tier filters)
		 */

		int? authorizedUserId = HttpContext.AuthorizedUserIdentity();

		if (!authorizedUserId.HasValue && requestQuery.ChartType == LeaderboardChartType.Country)
		{
			return BadRequest("Country leaderboards are only available to logged in users");
		}

		var leaderboard = await _leaderboardService.GetLeaderboardAsync(requestQuery, authorizedUserId);
		return Ok(leaderboard);
	}
}