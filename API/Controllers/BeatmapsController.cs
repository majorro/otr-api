using API.Entities;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BeatmapsController : Controller
{
	private readonly ILogger<BeatmapsController> _logger;
	private readonly IBeatmapService _beatmapService;
	public BeatmapsController(ILogger<BeatmapsController> logger, IBeatmapService beatmapService)
	{
		_logger = logger;
		_beatmapService = beatmapService;
	}
	
	[HttpGet("all")]
	public async Task<ActionResult<IEnumerable<Beatmap>>> GetAllAsync()
	{
		return Ok(await _beatmapService.GetAllAsync());
	}

	[HttpGet("{beatmapId:long}")]
	public async Task<ActionResult<Beatmap>> GetByOsuBeatmapIdAsync(long beatmapId)
	{
		var beatmap = await _beatmapService.GetByBeatmapIdAsync(beatmapId);
		if (beatmap == null)
		{
			return NotFound("No matching beatmapId in the database.");
		}

		return Ok(beatmap);
	}
}