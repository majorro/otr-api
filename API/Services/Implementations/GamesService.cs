using API.DTOs;
using API.Services.Interfaces;
using AutoMapper;
using Database.Entities;
using Database.Repositories.Interfaces;

namespace API.Services.Implementations;

public class GamesService(IGamesRepository gamesRepository, IPlayersRepository playersRepository, IMapper mapper) : IGamesService
{
    public async Task<GameDTO?> GetAsync(int id, bool verified)
    {
        GameDTO? game = mapper.Map<GameDTO?>(await gamesRepository.GetAsync(id, verified));

        if (game is null)
        {
            return null;
        }

        game.Players = await GetPlayerCompactsAsync(game);
        return game;
    }

    public async Task<GameDTO?> UpdateAsync(int id, GameDTO game)
    {
        Game? existing = await gamesRepository.GetAsync(id);
        if (existing is null)
        {
            return null;
        }

        existing.Ruleset = game.Ruleset;
        existing.ScoringType = game.ScoringType;
        existing.TeamType = game.TeamType;
        existing.Mods = game.Mods;
        existing.VerificationStatus = game.VerificationStatus;
        existing.ProcessingStatus = game.ProcessingStatus;
        existing.RejectionReason = game.RejectionReason;
        existing.StartTime = game.StartTime;
        existing.EndTime = game.EndTime ?? existing.EndTime;

        await gamesRepository.UpdateAsync(existing);
        return mapper.Map<GameDTO>(existing);
    }

    public async Task<bool> ExistsAsync(int id) =>
        await gamesRepository.ExistsAsync(id);

    public async Task DeleteAsync(int id) =>
        await gamesRepository.DeleteAsync(id);

    private async Task<ICollection<PlayerCompactDTO>> GetPlayerCompactsAsync(GameDTO game)
    {
        IEnumerable<int> playerIds = game.Scores.Select(s => s.PlayerId);
        return mapper.Map<ICollection<PlayerCompactDTO>>(await playersRepository.GetAsync(playerIds));
    }
}
