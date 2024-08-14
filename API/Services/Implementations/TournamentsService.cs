using API.DTOs;
using API.Services.Interfaces;
using AutoMapper;
using Database.Entities;
using Database.Enums;
using Database.Enums.Verification;
using Database.Repositories.Interfaces;

namespace API.Services.Implementations;

public class TournamentsService(ITournamentsRepository tournamentsRepository, IMatchesRepository matchesRepository, IMapper mapper) : ITournamentsService
{
    // TODO: Refactor to use enum for param "verificationSource"
    public async Task<TournamentCreatedResultDTO> CreateAsync(TournamentWebSubmissionDTO wrapper, bool verify)
    {
        // Only create matches that dont already exist
        IEnumerable<long> enumerableMatchIds = wrapper.Ids.ToList();
        IEnumerable<long> existingMatchIds = (await matchesRepository.GetAsync(enumerableMatchIds))
            .Select(m => m.OsuId)
            .ToList();

        Tournament tournament = await tournamentsRepository.CreateAsync(new Tournament
        {
            Name = wrapper.TournamentName,
            Abbreviation = wrapper.Abbreviation,
            ForumUrl = wrapper.ForumPost,
            RankRangeLowerBound = wrapper.RankRangeLowerBound,
            Ruleset = (Ruleset)wrapper.Mode,
            TeamSize = wrapper.TeamSize,
            SubmittedByUserId = wrapper.SubmitterId,
            Matches = enumerableMatchIds
                .Except(existingMatchIds)
                .Select(matchId => new Match
                {
                    OsuId = matchId,
                    VerificationStatus = VerificationStatus.None,
                    VerifiedByUserId = verify ? wrapper.SubmitterId : null,
                    SubmittedByUserId = wrapper.SubmitterId
                }).ToList()
        });
        return mapper.Map<TournamentCreatedResultDTO>(tournament);
    }

    public async Task<bool> ExistsAsync(int id) => await tournamentsRepository.ExistsAsync(id);

    public async Task<bool> ExistsAsync(string name, int mode) => await tournamentsRepository.ExistsAsync(name, mode);

    public async Task<IEnumerable<TournamentDTO>> ListAsync()
    {
        IEnumerable<Tournament> items = await tournamentsRepository.GetAllAsync();
        items = items.OrderBy(x => x.Name);

        return mapper.Map<IEnumerable<TournamentDTO>>(items);
    }

    public async Task<TournamentDTO?> GetAsync(int id, bool eagerLoad = true) =>
        mapper.Map<TournamentDTO>(await tournamentsRepository.GetAsync(id, eagerLoad));

    public async Task<int> CountPlayedAsync(
        int playerId,
        int mode,
        DateTime? dateMin = null,
        DateTime? dateMax = null
    ) => await tournamentsRepository.CountPlayedAsync(playerId, mode, dateMin ?? DateTime.MinValue, dateMax ?? DateTime.MaxValue);

    public async Task<TournamentDTO?> UpdateAsync(int id, TournamentDTO wrapper)
    {
        Tournament? existing = await tournamentsRepository.GetAsync(id);
        if (existing is null)
        {
            return null;
        }

        existing.Name = wrapper.Name;
        existing.Abbreviation = wrapper.Abbreviation;
        existing.ForumUrl = wrapper.ForumUrl;
        existing.Ruleset = (Ruleset)wrapper.Ruleset;
        existing.RankRangeLowerBound = wrapper.RankRangeLowerBound;
        existing.TeamSize = wrapper.TeamSize;

        await tournamentsRepository.UpdateAsync(existing);
        return mapper.Map<TournamentDTO>(existing);
    }
}
