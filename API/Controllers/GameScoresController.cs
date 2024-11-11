using API.Authorization;
using API.DTOs;
using API.Services.Interfaces;
using API.Utilities.Extensions;
using Asp.Versioning;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public partial class GameScoresController(IGameScoresService gameScoresService, IAdminNoteService adminNoteService) : Controller
{
    /// <summary>
    /// Amend score data
    /// </summary>
    /// <param name="id">Score id</param>
    /// <param name="patch">JsonPatch data</param>
    /// <response code="404">A score matching the given id does not exist</response>
    /// <response code="400">The JsonPatch data is malformed</response>
    /// <response code="200">Returns the updated score</response>
    [HttpPatch("{id:int}")]
    [Authorize(Roles = OtrClaims.Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<GameScoreDTO>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] JsonPatchDocument<GameScoreDTO> patch)
    {
        // Ensure target game score exists
        GameScoreDTO? score = await gameScoresService.GetAsync(id);
        if (score is null)
        {
            return NotFound();
        }

        // Ensure request is only attempting to perform a replace operation.
        if (patch.Operations.Count == 0 || !patch.IsReplaceOnly())
        {
            return BadRequest();
        }

        // Patch and validate
        patch.ApplyTo(score, ModelState);
        if (!TryValidateModel(score))
        {
            return BadRequest(ModelState.ErrorMessage());
        }

        // Apply patched values to entity
        GameScoreDTO? updatedScore = await gameScoresService.UpdateAsync(id, score);
        return Ok(updatedScore!);
    }

    /// <summary>
    /// Delete a score
    /// </summary>
    /// <param name="id">Score id</param>
    /// <response code="404">A score matching the given id does not exist</response>
    /// <response code="204">The score was deleted successfully</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = OtrClaims.Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        GameScoreDTO? result = await gameScoresService.GetAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        await gameScoresService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// List all admin notes for a score
    /// </summary>
    /// <param name="id">Score id</param>
    /// <response code="404">A score matching the given id does not exist</response>
    /// <response code="200">Returns all admin notes from a score</response>
    [HttpGet("{id:int}/notes")]
    [Authorize(Roles = $"{OtrClaims.Roles.User}, {OtrClaims.Roles.Client}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IEnumerable<AdminNoteDTO>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAdminNotesAsync(int id)
    {
        if (!await gameScoresService.ExistsAsync(id))
        {
            return NotFound();
        }

        return Ok(await adminNoteService.ListAsync<GameScoreAdminNote>(id));
    }
}
