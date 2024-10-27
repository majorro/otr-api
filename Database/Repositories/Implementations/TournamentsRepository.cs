using System.Diagnostics.CodeAnalysis;
using Database.Entities;
using Database.Entities.Processor;
using Database.Enums;
using Database.Enums.Verification;
using Database.Repositories.Interfaces;
using Database.Utilities;
using Database.Utilities.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories.Implementations;

[SuppressMessage("Performance",
    "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
[SuppressMessage("ReSharper", "SpecifyStringComparison")]
public class TournamentsRepository(OtrContext context) : RepositoryBase<Tournament>(context), ITournamentsRepository
{
    private readonly OtrContext _context = context;

    public async Task<Tournament?> GetAsync(int id, bool eagerLoad = false) =>
        eagerLoad ? await TournamentsBaseQuery().FirstOrDefaultAsync(x => x.Id == id) : await base.GetAsync(id);

    public async Task<Tournament?> GetVerifiedAsync(int id) =>
        await _context.Tournaments
            .AsSplitQuery()
            .Include(t => t.Matches.Where(m => m.VerificationStatus == VerificationStatus.Verified && m.ProcessingStatus == MatchProcessingStatus.Done))
            .ThenInclude(m => m.Games.Where(g => g.VerificationStatus == VerificationStatus.Verified && g.ProcessingStatus == GameProcessingStatus.Done))
            .ThenInclude(g => g.Beatmap)
            .Include(t => t.Matches.Where(m => m.VerificationStatus == VerificationStatus.Verified && m.ProcessingStatus == MatchProcessingStatus.Done))
            .ThenInclude(m => m.Games.Where(g => g.VerificationStatus == VerificationStatus.Verified && g.ProcessingStatus == GameProcessingStatus.Done))
            .ThenInclude(g => g.Scores.Where(gs => gs.VerificationStatus == VerificationStatus.Verified && gs.ProcessingStatus == ScoreProcessingStatus.Done))
            .ThenInclude(gs => gs.Player)
            .Include(t => t.AdminNotes)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Tournament>> GetNeedingProcessingAsync(int limit) =>
        await _context.Tournaments
            .AsSingleQuery()
            .Include(t => t.PooledBeatmaps)
            .Include(t => t.PlayerTournamentStats)
            .Include(t => t.Matches)
            .ThenInclude(m => m.WinRecord)
            .Include(t => t.Matches)
            .ThenInclude(m => m.PlayerMatchStats)
            .Include(t => t.Matches)
            .ThenInclude(m => m.PlayerRatingAdjustments)
            .Include(t => t.Matches)
            .ThenInclude(m => m.Games)
            .ThenInclude(g => g.Beatmap)
            .Include(t => t.Matches)
            .ThenInclude(m => m.Games)
            .ThenInclude(g => g.Scores)
            .ThenInclude(s => s.Player)
            .Include(t => t.Matches)
            .ThenInclude(m => m.Games)
            .ThenInclude(g => g.WinRecord)
            .Where(t => t.ProcessingStatus != TournamentProcessingStatus.Done && t.ProcessingStatus != TournamentProcessingStatus.NeedsApproval)
            .OrderBy(t => t.LastProcessingDate)
            .Take(limit)
            .ToListAsync();

    public async Task<bool> ExistsAsync(string name, Ruleset ruleset) =>
        await _context.Tournaments.AnyAsync(x => x.Name.ToLower() == name.ToLower() && x.Ruleset == ruleset);

    public async Task<int> CountPlayedAsync(
        int playerId,
        Ruleset ruleset,
        DateTime dateMin,
        DateTime dateMax
    ) => await QueryForParticipation(playerId, ruleset, dateMin, dateMax).Select(x => x.Id).Distinct().CountAsync();

    public async Task<ICollection<Tournament>> GetAsync(int page, int pageSize, TournamentQuerySortType querySortType,
        bool descending = false, bool verified = true, Ruleset? ruleset = null)
    {
        IQueryable<Tournament> query = _context.Tournaments
            .AsNoTracking()
            .OrderBy(querySortType, descending);

        if (verified)
        {
            query = query.WhereVerified().WhereProcessingCompleted();
        }

        if (ruleset.HasValue)
        {
            query = query.Where(x => x.Ruleset == ruleset.Value);
        }

        return await query
                .Page(pageSize, page)
                .ToListAsync();
    }

    public async Task AcceptVerificationStatuses(int id)
    {
        Tournament? tournament = await TournamentsBaseQuery()
            .Where(t => t.ProcessingStatus == TournamentProcessingStatus.NeedsVerification)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tournament is null)
        {
            return;
        }

        tournament.VerificationStatus = EnumUtils.ConfirmPreStatus(tournament.VerificationStatus);
        foreach (Match match in tournament.Matches)
        {
            match.VerificationStatus = EnumUtils.ConfirmPreStatus(match.VerificationStatus);

            foreach (Game game in match.Games)
            {
                game.VerificationStatus = EnumUtils.ConfirmPreStatus(game.VerificationStatus);

                foreach (GameScore score in game.Scores)
                {
                    score.VerificationStatus = EnumUtils.ConfirmPreStatus(score.VerificationStatus);
                }
            }
        }

        await UpdateAsync(tournament);
    }

    public async Task RerunAutomationChecks(int id, bool force = false)
    {
        Tournament? tournament = await TournamentsBaseQuery()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tournament is null)
        {
            return;
        }

        var update = force || (tournament.VerificationStatus != VerificationStatus.Rejected &&
                               tournament.VerificationStatus != VerificationStatus.Verified);

        if (update)
        {
            tournament.VerificationStatus = VerificationStatus.None;
            tournament.RejectionReason = TournamentRejectionReason.None;
            tournament.ProcessingStatus = TournamentProcessingStatus.NeedsAutomationChecks;
        }

        ResetMatchChecks(tournament, force);
        await UpdateAsync(tournament);
    }

    private static void ResetMatchChecks(Tournament tournament, bool force)
    {
        foreach (Match match in tournament.Matches)
        {
            var matchUpdate = force || (match.VerificationStatus != VerificationStatus.Rejected &&
                                        match.VerificationStatus != VerificationStatus.Verified);

            if (!matchUpdate)
            {
                continue;
            }

            match.VerificationStatus = VerificationStatus.None;
            match.WarningFlags = MatchWarningFlags.None;
            match.RejectionReason = MatchRejectionReason.None;
            match.ProcessingStatus = MatchProcessingStatus.Done;

            ResetGameChecks(match, force);
        }
    }

    private static void ResetGameChecks(Match match, bool force)
    {
        foreach (Game game in match.Games)
        {
            var gameUpdate = force || (game.VerificationStatus != VerificationStatus.Rejected &&
                                        game.VerificationStatus != VerificationStatus.Verified);

            if (!gameUpdate)
            {
                continue;
            }

            game.VerificationStatus = VerificationStatus.None;
            game.WarningFlags = GameWarningFlags.None;
            game.RejectionReason = GameRejectionReason.None;
            game.ProcessingStatus = GameProcessingStatus.Done;

            ResetScoreChecks(game, force);
        }
    }

    private static void ResetScoreChecks(Game game, bool force)
    {
        foreach (GameScore score in game.Scores)
        {
            var scoreUpdate = force || (score.VerificationStatus != VerificationStatus.Rejected &&
                                        score.VerificationStatus != VerificationStatus.Verified);

            if (!scoreUpdate)
            {
                continue;
            }

            score.VerificationStatus = VerificationStatus.None;
            score.RejectionReason = ScoreRejectionReason.None;
            score.ProcessingStatus = ScoreProcessingStatus.Done;
        }
    }

    /// <summary>
    /// Returns a queryable containing tournaments for <see cref="ruleset"/>
    /// with *any* match applicable to all of the following criteria:
    /// - Is verified
    /// - Started between <paramref name="dateMin"/> and <paramref name="dateMax"/>
    /// - Contains a <see cref="RatingAdjustment"/> for given <paramref name="playerId"/> (Denotes participation)
    /// </summary>
    /// <param name="playerId">Id (primary key) of target player</param>
    /// <param name="ruleset">Ruleset</param>
    /// <param name="dateMin">Date lower bound</param>
    /// <param name="dateMax">Date upper bound</param>
    /// <remarks>Since filter uses Any, invalid matches can still exist in the resulting query</remarks>
    /// <returns></returns>
    protected IQueryable<Tournament> QueryForParticipation(
        int playerId,
        Ruleset ruleset,
        DateTime? dateMin,
        DateTime? dateMax
    )
    {
        dateMin ??= DateTime.MinValue;
        dateMax ??= DateTime.MaxValue;

        return _context.Tournaments
            .Include(t => t.Matches)
            .ThenInclude(m => m.PlayerRatingAdjustments)
            .Where(t =>
                t.Ruleset == ruleset
                // Contains *any* match that is:
                && t.Matches.Any(m =>
                    // Within time range
                    m.StartTime >= dateMin
                    && m.StartTime <= dateMax
                    // Verified
                    && m.VerificationStatus == VerificationStatus.Verified
                    // Participated in by player
                    && m.PlayerRatingAdjustments.Any(stat => stat.PlayerId == playerId)
                ));
    }

    private IQueryable<Tournament> TournamentsBaseQuery()
    {
        return _context.Tournaments
            .Include(e => e.Matches)
            .ThenInclude(m => m.Games)
            .ThenInclude(g => g.Scores)
            .Include(e => e.Matches)
            .ThenInclude(m => m.Games)
            .ThenInclude(g => g.Beatmap)
            .Include(e => e.SubmittedByUser)
            .Include(t => t.AdminNotes)
            .AsSplitQuery();
    }
}
