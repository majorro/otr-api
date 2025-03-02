using API.DTOs;
using API.Enums;
using API.Repositories.Interfaces;
using AutoMapper;
using Database;
using Database.Enums;
using Database.Enums.Verification;
using Database.Repositories.Implementations;
using Database.Repositories.Interfaces;
using Database.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories.Implementations;

public class ApiTournamentsRepository(OtrContext context, IBeatmapsRepository beatmapsRepository) :
    TournamentsRepository(context, beatmapsRepository), IApiTournamentsRepository
{
    private readonly OtrContext _context = context;

    public async Task<IEnumerable<TournamentSearchResultDTO>> SearchAsync(string name) =>
        await _context.Tournaments
            .AsNoTracking()
            .WhereSearchQuery(name)
            .Select(t => new TournamentSearchResultDTO()
            {
                Id = t.Id,
                Ruleset = t.Ruleset,
                LobbySize = t.LobbySize,
                Name = t.Name
            })
            .Take(30)
            .ToListAsync();

    public async Task<PlayerTournamentLobbySizeCountDTO> GetLobbySizeStatsAsync(
    int playerId,
    Ruleset ruleset,
    DateTime dateMin,
    DateTime dateMax
)
    {
        var participatedTournaments =
            await QueryForParticipation(playerId, ruleset, dateMin, dateMax)
            .Select(t => new { TournamentId = t.Id, TeamSize = t.LobbySize })
            .Distinct() // Ensures each tournament is counted once
            .ToListAsync();

        return new PlayerTournamentLobbySizeCountDTO
        {
            Count1v1 = participatedTournaments.Count(x => x.TeamSize == 1),
            Count2v2 = participatedTournaments.Count(x => x.TeamSize == 2),
            Count3v3 = participatedTournaments.Count(x => x.TeamSize == 3),
            Count4v4 = participatedTournaments.Count(x => x.TeamSize == 4),
            CountOther = participatedTournaments.Count(x => x.TeamSize > 4)
        };
    }
}
