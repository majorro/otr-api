using Database.Entities;
using Database.Entities.Interfaces;
using Database.Entities.Processor;
using Database.Enums;
using Database.Enums.Queries;
using Database.Enums.Verification;
using Microsoft.EntityFrameworkCore;

namespace Database.Utilities.Extensions;

public static class QueryExtensions
{
    /// <summary>
    /// Gets the desired "page" of a query
    /// </summary>
    /// <param name="limit">Page size</param>
    /// <param name="page">Desired page (zero-indexed)</param>
    public static IQueryable<T> Page<T>(this IQueryable<T> query, int limit, int page) =>
        query.AsQueryable().Skip(limit * page).Take(limit);

    #region Ratings

    /// <summary>
    /// Filters a <see cref="PlayerRating"/> query for those generated for a given <see cref="Ruleset"/>
    /// </summary>
    /// <param name="ruleset">Ruleset</param>
    public static IQueryable<PlayerRating> WhereRuleset(this IQueryable<PlayerRating> query, Ruleset ruleset) =>
        query.AsQueryable().Where(x => x.Ruleset == ruleset);

    /// <summary>
    /// Orders a <see cref="PlayerRating"/> query by <see cref="PlayerRating.Rating"/> in descending order
    /// </summary>
    public static IQueryable<PlayerRating> OrderByRatingDescending(this IQueryable<PlayerRating> query) =>
        query.AsQueryable().OrderByDescending(x => x.Rating);

    #endregion

    #region Players

    /// <summary>
    /// Filters a <see cref="Player"/> query by the given osu! id
    /// </summary>
    /// <param name="osuId">osu! id</param>
    public static IQueryable<Player> WhereOsuId(this IQueryable<Player> query, long osuId) =>
        query.AsQueryable().Where(x => x.OsuId == osuId);

    /// <summary>
    /// Filters a <see cref="Player"/> query by the given the given username
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="partialMatch">
    /// If the filter should partially match the username. If false, strict matching is used
    /// </param>
    public static IQueryable<Player> WhereUsername(this IQueryable<Player> query, string username, bool partialMatch)
    {
        //_ is a wildcard character in psql so it needs to have an escape character added in front of it.
        username = username.Replace("_", @"\_");
        var pattern = partialMatch
            ? $"%{username}%"
            : username;

        return query.AsQueryable().Where(p => EF.Functions.ILike(p.Username, pattern, @"\"));
    }

    #endregion

    #region Tournaments

    /// <summary>
    /// Filters a <see cref="Tournament"/> query for those with a <see cref="VerificationStatus"/>
    /// of <see cref="VerificationStatus.Verified"/>
    /// </summary>
    public static IQueryable<Tournament> WhereVerified(this IQueryable<Tournament> query) =>
        query.AsQueryable().Where(x => x.VerificationStatus == VerificationStatus.Verified);

    /// <summary>
    /// Filters a <see cref="Tournament"/> query for those with a <see cref="TournamentProcessingStatus"/>
    /// of <see cref="TournamentProcessingStatus.Done"/>
    /// </summary>
    public static IQueryable<Tournament> WhereProcessingCompleted(this IQueryable<Tournament> query) =>
        query.AsQueryable().Where(x => x.ProcessingStatus == TournamentProcessingStatus.Done);

    /// <summary>
    /// Orders the query based on the specified sort type and direction.
    /// </summary>
    /// <param name="query">The query to be ordered.</param>
    /// <param name="sortType">Defines which key to order the results by</param>
    /// <param name="descending">A boolean indicating whether the ordering should be in descending order. Defaults to false (ascending).</param>
    /// <returns>The ordered query</returns>
    public static IQueryable<Tournament> OrderBy(this IQueryable<Tournament> query, TournamentQuerySortType sortType, bool descending = false) =>
        sortType switch
        {
            TournamentQuerySortType.Id => descending ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id),
            TournamentQuerySortType.Name => descending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
            TournamentQuerySortType.StartTime => descending ? query.OrderByDescending(t => t.StartTime) : query.OrderBy(t => t.StartTime),
            TournamentQuerySortType.Created => descending ? query.OrderByDescending(t => t.Created) : query.OrderBy(t => t.Created),
            _ => query
        };

    #endregion

    #region Matches

    /// <summary>
    /// Filters a <see cref="Match"/> query for those with a <see cref="VerificationStatus"/>
    /// of <see cref="VerificationStatus.Verified"/>
    /// </summary>
    public static IQueryable<Match> WhereVerified(this IQueryable<Match> query) =>
        query.AsQueryable().Where(x => x.VerificationStatus == VerificationStatus.Verified);

    /// <summary>
    /// Filters a <see cref="Match"/> query for those with a <see cref="MatchProcessingStatus"/>
    /// of <see cref="MatchProcessingStatus.Done"/>
    /// </summary>
    public static IQueryable<Match> WhereProcessingCompleted(this IQueryable<Match> query) =>
        query.AsQueryable().Where(x => x.ProcessingStatus == MatchProcessingStatus.Done);

    /// <summary>
    /// Includes navigation properties for a <see cref="Match"/>
    /// <br/>Includes: <see cref="Match.WinRecord"/>, <see cref="Match.PlayerMatchStats"/>,
    /// <see cref="Match.PlayerRatingAdjustments"/>, <see cref="Match.Games"/>
    /// (<see cref="Game.Scores"/>, <see cref="Game.Beatmap"/>, <see cref="Game.WinRecord"/>)
    /// </summary>
    public static IQueryable<Match> IncludeChildren(this IQueryable<Match> query) =>
        query
            .AsQueryable()
            .Include(m => m.WinRecord)
            .Include(m => m.PlayerMatchStats)
            .Include(m => m.PlayerRatingAdjustments)
            .Include(m => m.Games)
            .ThenInclude(g => g.Scores)
            .ThenInclude(s => s.Player)
            .Include(m => m.Games)
            .ThenInclude(s => s.AdminNotes)
            .Include(m => m.Games)
            .ThenInclude(g => g.Beatmap)
            .Include(m => m.Games)
            .ThenInclude(g => g.WinRecord)
            .Include(m => m.Games)
            .ThenInclude(g => g.AdminNotes);

    /// <summary>
    /// Includes the <see cref="Tournament"/> navigation on this <see cref="Match"/>
    /// </summary>
    /// <param name="query">The <see cref="Match"/> query</param>
    /// <returns>The query with the <see cref="Tournament"/> included</returns>
    public static IQueryable<Match> IncludeTournament(this IQueryable<Match> query) =>
        query
            .AsQueryable()
            .Include(m => m.Tournament);

    /// <summary>
    /// Filters a <see cref="Match"/> query for those with a <see cref="Match.StartTime"/> that is greater than
    /// the given date
    /// </summary>
    /// <param name="date">Date comparison</param>
    public static IQueryable<Match> AfterDate(this IQueryable<Match> query, DateTime date) =>
        query.AsQueryable().Where(x => x.StartTime > date);

    /// <summary>
    /// Filters a <see cref="Match"/> query for those with a <see cref="Match.StartTime"/> that is less than
    /// the given date
    /// </summary>
    /// <param name="date">Date comparison</param>
    public static IQueryable<Match> BeforeDate(this IQueryable<Match> query, DateTime date) =>
        query.AsQueryable().Where(x => x.StartTime < date);

    /// <summary>
    /// Filters a <see cref="Match"/> query for those played between a given date range
    /// </summary>
    /// <param name="dateMin">Date range lower bound</param>
    /// <param name="dateMax">Date range upper bound</param>
    public static IQueryable<Match> WhereDateRange(
        this IQueryable<Match> query,
        DateTime dateMin,
        DateTime dateMax
    ) => query.AsQueryable().AfterDate(dateMin).BeforeDate(dateMax);

    /// <summary>
    /// Filters a <see cref="Match"/> query for those played in the given <see cref="Ruleset"/>
    /// </summary>
    /// <param name="ruleset">Ruleset</param>
    public static IQueryable<Match> WhereRuleset(this IQueryable<Match> query, Ruleset ruleset) =>
        query.AsQueryable().Where(x => x.Tournament.Ruleset == ruleset);

    /// <summary>
    /// Filters a <see cref="Match"/> query for those with a partial match of the given name
    /// </summary>
    /// <param name="name">Match name</param>
    public static IQueryable<Match> WhereName(this IQueryable<Match> query, string name)
    {
        name = name.Replace("_", @"\_");
        return query
            .AsQueryable()
            .Where(x => EF.Functions.ILike(x.Name, $"%{name}%", @"\"));
    }

    /// <summary>
    /// Orders the query based on the specified sort type and direction.
    /// </summary>
    /// <param name="query">The query to be ordered.</param>
    /// <param name="sortType">Defines which key to order the results by</param>
    /// <param name="descending">A boolean indicating whether the ordering should be in descending order. Defaults to false (ascending).</param>
    /// <returns>The ordered query</returns>
    public static IQueryable<Match> OrderBy(this IQueryable<Match> query, MatchQuerySortType sortType, bool descending = false) =>
        sortType switch
        {
            MatchQuerySortType.Id => descending ? query.OrderByDescending(m => m.Id) : query.OrderBy(m => m.Id),
            MatchQuerySortType.OsuId => descending ? query.OrderByDescending(m => m.OsuId) : query.OrderBy(m => m.OsuId),
            MatchQuerySortType.StartTime => descending ? query.OrderByDescending(m => m.StartTime) : query.OrderBy(m => m.StartTime),
            MatchQuerySortType.EndTime => descending ? query.OrderByDescending(m => m.EndTime) : query.OrderBy(m => m.EndTime),
            MatchQuerySortType.Created => descending ? query.OrderByDescending(m => m.Created) : query.OrderBy(m => m.Created),
            _ => query
        };


    /// <summary>
    /// Filters a <see cref="Match"/> query for those where a <see cref="Player"/> with the given osu! id participated
    /// </summary>
    /// <remarks>
    /// Does not filter for <see cref="VerificationStatus"/> or <see cref="MatchProcessingStatus"/>. Should only be
    /// used after filtering for validity
    /// </remarks>
    /// <param name="osuPlayerId">osu! id of the target <see cref="Player"/></param>
    public static IQueryable<Match> WherePlayerParticipated(this IQueryable<Match> query, long osuPlayerId) =>
        query.AsQueryable().Where(x => x.Games.Any(y => y.Scores.Any(z => z.Player.OsuId == osuPlayerId)));

    #endregion

    #region Games

    /// <summary>
    /// Filters a <see cref="Game"/> query for those with a <see cref="VerificationStatus"/>
    /// of <see cref="VerificationStatus.Verified"/>
    /// </summary>
    public static IQueryable<Game> WhereVerified(this IQueryable<Game> query) =>
        query.AsQueryable().Where(x => x.VerificationStatus == VerificationStatus.Verified);

    /// <summary>
    /// Filters a <see cref="Game"/> query for those with a <see cref="GameProcessingStatus"/>
    /// of <see cref="GameProcessingStatus.Done"/>
    /// </summary>
    public static IQueryable<Game> WhereProcessingCompleted(this IQueryable<Game> query) =>
        query.AsQueryable().Where(x => x.ProcessingStatus == GameProcessingStatus.Done);

    /// <summary>
    /// Filters a <see cref="Game"/> query for those with a <see cref="Game.StartTime"/> that is greater than
    /// the given date
    /// </summary>
    /// <param name="date">Date comparison</param>
    public static IQueryable<Game> AfterDate(this IQueryable<Game> query, DateTime date) =>
        query.AsQueryable().Where(x => x.StartTime > date);

    /// <summary>
    /// Filters a <see cref="Game"/> query for those with a <see cref="Match.StartTime"/> that is less than
    /// the given date
    /// </summary>
    /// <param name="date">Date comparison</param>
    public static IQueryable<Game> BeforeDate(this IQueryable<Game> query, DateTime date) =>
        query.AsQueryable().Where(x => x.StartTime < date);

    /// <summary>
    /// Filters a <see cref="Game"/> query for those played between a given date range
    /// </summary>
    /// <param name="dateMin">Date range lower bound</param>
    /// <param name="dateMax">Date range upper bound</param>
    public static IQueryable<Game> WhereDateRange(
        this IQueryable<Game> query,
        DateTime dateMin,
        DateTime dateMax
    ) => query.AsQueryable().AfterDate(dateMin).BeforeDate(dateMax);

    #endregion

    #region Scores

    /// <summary>
    /// Filters a <see cref="GameScore"/> query for those with a <see cref="VerificationStatus"/>
    /// of <see cref="VerificationStatus.Verified"/>
    /// </summary>
    public static IQueryable<GameScore> WhereVerified(this IQueryable<GameScore> query) =>
        query.AsQueryable().Where(x => x.VerificationStatus == VerificationStatus.Verified);

    /// <summary>
    /// Filters a <see cref="GameScore"/> query for those with a <see cref="ScoreProcessingStatus"/>
    /// of <see cref="ScoreProcessingStatus.Done"/>
    /// </summary>
    public static IQueryable<GameScore> WhereProcessingCompleted(this IQueryable<GameScore> query) =>
        query.AsQueryable().Where(x => x.ProcessingStatus == ScoreProcessingStatus.Done);

    /// <summary>
    /// Filters a <see cref="GameScore"/> query for those set after a given date
    /// </summary>
    /// <param name="date">Date comparison</param>
    public static IQueryable<GameScore> AfterDate(this IQueryable<GameScore> query, DateTime date) =>
        query.AsQueryable().Where(x => x.Game.EndTime > date);

    /// <summary>
    /// Filters a <see cref="GameScore"/> query for those set before a given date
    /// </summary>
    /// <param name="date">Date comparison</param>
    public static IQueryable<GameScore> BeforeDate(this IQueryable<GameScore> query, DateTime date) =>
        query.AsQueryable().Where(x => x.Game.EndTime < date);

    /// <summary>
    /// Filters a <see cref="GameScore"/> query for those set between a given date range
    /// </summary>
    /// <param name="dateMin">Date range lower bound</param>
    /// <param name="dateMax">Date range upper bound</param>
    public static IQueryable<GameScore> WhereDateRange(
        this IQueryable<GameScore> query,
        DateTime dateMin,
        DateTime dateMax
    ) => query.AsQueryable().AfterDate(dateMin).BeforeDate(dateMax);

    /// <summary>
    /// Filters a <see cref="GameScore"/> query for those set in a given <see cref="Ruleset"/>
    /// </summary>
    /// <param name="ruleset">Ruleset</param>
    public static IQueryable<GameScore> WhereRuleset(this IQueryable<GameScore> query, Ruleset ruleset) =>
        query.AsQueryable().Where(x => x.Ruleset == ruleset);

    /// <summary>
    /// Filters a <see cref="GameScore"/> query for those set with the given <see cref="Mods"/> enabled
    /// </summary>
    /// <param name="enabledMods">Mods</param>
    public static IQueryable<GameScore> WhereMods(
        this IQueryable<GameScore> query,
        Mods enabledMods
    ) =>
        query
            .AsQueryable()
            .Where(x =>
                x.Game.Mods == enabledMods
                || x.Game.Mods == (enabledMods | Mods.NoFail)
                || x.Mods == enabledMods
                || x.Mods == (enabledMods | Mods.NoFail)
            );

    /// <summary>
    /// Filters a <see cref="GameScore"/> query for those where the <see cref="Player"/> who set the score is
    /// not on the same team as the <see cref="Player"/> with the given osu! id
    /// </summary>
    /// <param name="osuId">osu! id</param>
    public static IQueryable<GameScore> WhereOpponentOf(this IQueryable<GameScore> query, long osuId) =>
        query
            .AsQueryable()
            .Where(x =>
                x.Game.Scores.Any(y => y.Player.OsuId == osuId)
                && x.Player.OsuId != osuId
                && x.Team != x.Game.Scores.First(y => y.Player.OsuId == osuId).Team
            );

    /// <summary>
    /// Filters a <see cref="GameScore"/> query for those where the <see cref="Player"/> who set the score is
    /// on the same team as the <see cref="Player"/> with the given osu! id
    /// </summary>
    /// <param name="osuId">osu! id</param>
    public static IQueryable<GameScore> WhereTeammateOf(this IQueryable<GameScore> query, long osuId) =>
        query
            .AsQueryable()
            .Where(x =>
                x.Game.Scores.Any(y => y.Player.OsuId == osuId)
                && x.Player.OsuId != osuId
                && x.Team == x.Game.Scores.First(y => y.Player.OsuId == osuId).Team
            );

    public static IQueryable<GameScore> WherePlayerId(this IQueryable<GameScore> query, int playerId) =>
        query.AsQueryable().Where(x => x.PlayerId == playerId);

    #endregion

    #region Admin Notes

    public static IQueryable<TEntity> IncludeAdminNotes<TEntity, TAdminNote>(this IQueryable<TEntity> query)
        where TEntity : class, IAdminNotableEntity<TAdminNote>
        where TAdminNote : IAdminNoteEntity
        => query.Include(e => e.AdminNotes);

    #endregion
}
