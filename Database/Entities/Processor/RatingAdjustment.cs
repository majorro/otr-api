using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Database.Enums;

namespace Database.Entities.Processor;

/// <summary>
/// Describes an individual change to a <see cref="Processor.PlayerRating"/>
/// </summary>
/// <remarks>
/// Generated by the <a href="https://docs.otr.stagec.xyz/o-tr-processor.html">o!TR Processor</a>
/// <br/><br/>
/// A full collection of <see cref="RatingAdjustment"/>s functionally outline the change in rating over time for a
/// <see cref="Entities.Player"/> in a <see cref="Ruleset"/>.
/// Where <see cref="Processor.PlayerRating"/> describes final and current rating information,
/// a collection of <see cref="RatingAdjustment"/>s describe the changes in rating that bring it to that final state.
/// For example, a <see cref="RatingAdjustment"/> is created for each <see cref="Entities.Match"/> played
/// or elapsed period of decay. This distinction is denoted by the <see cref="AdjustmentType"/>
/// </remarks>
[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.CircularDependency")]
public class RatingAdjustment : EntityBase
{
    /// <summary>
    /// The <see cref="RatingAdjustmentType"/>
    /// </summary>
    public RatingAdjustmentType AdjustmentType { get; init; }

    /// <summary>
    ///     The <see cref="Ruleset" />
    /// </summary>
    public Ruleset Ruleset { get; set; }

    /// <summary>
    /// Timestamp for when the adjustment was applied
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Rating before the adjustment was considered
    /// </summary>
    public double RatingBefore { get; init; }

    /// <summary>
    /// Rating after the adjustment was considered
    /// </summary>
    public double RatingAfter { get; init; }

    /// <summary>
    /// Volatility before the adjustment was considered
    /// </summary>
    public double VolatilityBefore { get; init; }

    /// <summary>
    /// Volatility after the adjustment was considered
    /// </summary>
    public double VolatilityAfter { get; init; }

    /// <summary>
    /// Id of the <see cref="Processor.PlayerRating"/> that the adjustment affects
    /// </summary>
    public int PlayerRatingId { get; init; }

    /// <summary>
    /// The <see cref="Processor.PlayerRating"/> that the adjustment affects
    /// </summary>
    public PlayerRating PlayerRating { get; init; } = null!;

    /// <summary>
    /// Id of the <see cref="Entities.Player"/> the adjustment was generated for
    /// </summary>
    public int PlayerId { get; init; }

    /// <summary>
    /// The <see cref="Entities.Player"/> the adjustment was generated for
    /// </summary>
    public Player Player { get; init; } = null!;

    /// <summary>
    /// Id of the <see cref="Entities.Match"/> the rating stat was generated for
    /// </summary>
    /// <remarks>
    /// Optional. Only populated if <see cref="AdjustmentType"/> is <see cref="RatingAdjustmentType.Match"/>
    /// </remarks>
    public int? MatchId { get; init; }

    /// <summary>
    /// The <see cref="Entities.Match"/> the rating stat was generated for
    /// </summary>
    /// <remarks>
    /// Optional. Only populated if <see cref="AdjustmentType"/> is <see cref="RatingAdjustmentType.Match"/>
    /// </remarks>
    public Match? Match { get; init; }

    /// <summary>
    /// Total change in rating
    /// </summary>
    [NotMapped]
    public double RatingDelta => RatingBefore - RatingAfter;

    /// <summary>
    /// Total change in volatility
    /// </summary>
    [NotMapped]
    public double VolatilityDelta => VolatilityBefore - VolatilityAfter;
}
