using Database.Entities.Processor;

namespace Database.Repositories.Interfaces;

public interface IMatchRatingStatsRepository : IRepository<RatingAdjustment>
{
    /// <summary>
    ///  Returns one entry per match, with each entry being all matches that occur on the same day.
    ///  Typically, each list will contain one item. However, if a player plays multiple matches in a day,
    ///  there will be multiple items in the list.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="mode"></param>
    /// <param name="dateMin"></param>
    /// <param name="dateMax"></param>
    /// <returns></returns>
    Task<IEnumerable<IEnumerable<RatingAdjustment>>> GetForPlayerAsync(
        int playerId,
        int mode,
        DateTime? dateMin = null,
        DateTime? dateMax = null
    );

    Task TruncateAsync();

    Task<int> HighestGlobalRankAsync(
        int playerId,
        int mode,
        DateTime? dateMin = null,
        DateTime? dateMax = null
    );

    Task<int> HighestCountryRankAsync(
        int playerId,
        int mode,
        DateTime? dateMin = null,
        DateTime? dateMax = null
    );

    Task<IEnumerable<RatingAdjustment>> TeammateRatingStatsAsync(
        int playerId,
        int teammateId,
        int mode,
        DateTime dateMin,
        DateTime dateMax
    );

    Task<IEnumerable<RatingAdjustment>> OpponentRatingStatsAsync(
        int playerId,
        int opponentId,
        int mode,
        DateTime dateMin,
        DateTime dateMax
    );
}
